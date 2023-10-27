using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bert_neural_network;
using Microsoft.ML.OnnxRuntime;

namespace Task2
{
    public class ViewData
    {
        public Bert bert;
        
        string modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        string modelUrl = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        public bool isDownloaded = false;

        public CancellationToken сancellationToken;
        public CancellationTokenSource сancellationTokenSource;
        public string text = "";
        
        public List<string> ChatView { get; set; }

        public ViewData()
        {
            CancellationTokenSource сancellationTokenSource = new();
            сancellationToken = сancellationTokenSource.Token;
            Bert bert = new Bert();
            ChatView = new List<string>();
        }

        public Task GetBertAsync()
        {
            return Task.Factory.StartNew(() => {

                try
                {
                    Bert.DownloadBert(modelPath, modelUrl);
                    isDownloaded = true;
                    return;
                }
                catch { }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
