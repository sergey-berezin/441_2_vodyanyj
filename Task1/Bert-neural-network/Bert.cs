using BERTTokenizers;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Net;


namespace Bert_neural_network
{
    public class Bert
    {
        Mutex sessionMutex = new();
        public InferenceSession session;
        public Bert(InferenceSession inferenceSession)
        {
            this.session = inferenceSession;
        }

        public static Bert GetBert(string modelUrl)
        {
            string modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
            if (!File.Exists(modelPath))
            {
                DownloadBert(modelPath, modelUrl);
            }
            return new Bert(new InferenceSession(modelPath));
        }
        
        public static void DownloadBert(string modelPath, string modelUrl, int maxAttempts=3)
        {
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(modelUrl, modelPath);
                    }
                    return;
                }
                catch (WebException)
                {
                    attempts++;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка при скачивании модели: {ex.Message}");
                }
            }
            throw new Exception($"Не удалось скачать модель после {maxAttempts} попыток. Попробуйте позже");
        }

        public Task<String> GetAnswerAsync(string text, string question, CancellationToken cancellationToken)
        {

            return Task.Factory.StartNew(() =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var sentence = $"{{\"question\": {question}, \"context\": \"@CTX\"}}".Replace("@CTX", text);
                    var tokenizer = new BertUncasedLargeTokenizer();
                    var tokens = tokenizer.Tokenize(sentence);
                    var encoded = tokenizer.Encode(tokens.Count(), sentence);
                    var bertInput = new BertInput()
                    {
                        InputIds = encoded.Select(t => t.InputIds).ToArray(),
                        AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
                        TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
                    };

                    var input_ids = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
                    var attention_mask = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
                    var token_type_ids = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);

                    var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", input_ids),
                                                            NamedOnnxValue.CreateFromTensor("input_mask", attention_mask),
                                                            NamedOnnxValue.CreateFromTensor("segment_ids", token_type_ids) };

                    cancellationToken.ThrowIfCancellationRequested();

                    sessionMutex.WaitOne();
                    var output = session.Run(input);
                    sessionMutex.ReleaseMutex();

                    cancellationToken.ThrowIfCancellationRequested();

                    List<float> startLogits = (output.ToList().First().Value as IEnumerable<float>).ToList();
                    List<float> endLogits = (output.ToList().Last().Value as IEnumerable<float>).ToList();

                    var startIndex = startLogits.ToList().IndexOf(startLogits.Max());
                    var endIndex = endLogits.ToList().IndexOf(endLogits.Max());

                    var predictedTokens = tokens
                                .Skip(startIndex)
                                .Take(endIndex + 1 - startIndex)
                                .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
                                .ToList();
                    return String.Join(" ", predictedTokens);
                }
                catch (OperationCanceledException)
                {
                    sessionMutex.ReleaseMutex();
                    throw;
                }
                catch (Exception ex)
                {
                    sessionMutex.ReleaseMutex();
                    throw;
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension)
        {
            Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });

            for (var i = 0; i < inputArray.Length; i++)
            {
                input[0, i] = inputArray[i];
            }
            return input;
        }

    }

    public class BertInput
    {
        public long[] InputIds { get; set; }
        public long[] AttentionMask { get; set; }
        public long[] TypeIds { get; set; }
    }
}

