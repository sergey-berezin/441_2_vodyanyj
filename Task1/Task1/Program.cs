using Bert_neural_network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Task1
{
    class Program
    {
        static Mutex consoleMutex = new();
        static async Task Main(string[] args)
        {

            Console.WriteLine("Приложение запущено");
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("Укажите ровно один аргумент командной строки - файл");
                    return;
                }
                else
                {
                    CancellationTokenSource сancellationTokenSource = new();
                    CancellationToken сancellationToken = сancellationTokenSource.Token;

                    string filePath = @args[0];
                    string currentDirectory = Directory.GetCurrentDirectory();
                    Console.WriteLine("Директория, из которой запущено приложение: " + currentDirectory);
                    string text = getText(filePath);
                    Console.WriteLine("Текст, который был считан моделью: ");
                    Console.Write(text);
                    Console.WriteLine();
                    string modelUrl = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";

                    Bert bert = Bert.getBert(modelUrl);
                    consoleMutex.WaitOne();
                    while ((!сancellationToken.IsCancellationRequested))
                    {
                        string? question = Console.ReadLine();
                        consoleMutex.ReleaseMutex();
                        if (question == "cancel" || question == "") 
                        { 
                            сancellationTokenSource.Cancel();
                        }
                        var answer = getAnsFromBertAsync(bert, text, question, сancellationToken);
                        consoleMutex.WaitOne();
                    }
                }
            }
            catch (Exception ex)
            {
                consoleMutex.WaitOne();
                Console.WriteLine(ex.Message);
                consoleMutex.ReleaseMutex();
            }
        }
        static string getText(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                return "Error during reading";
            }
        }

        static async Task getAnsFromBertAsync(Bert bert, string text, string question, CancellationToken сancellationToken)
        {
            try
            {
                var answer = await Task.Run(() => bert.GetAnswerAsync(text, question, сancellationToken));
               // consoleMutex.WaitOne();
                Console.WriteLine(question + " : " + answer);
               // consoleMutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                consoleMutex.WaitOne();
                Console.WriteLine(question + " : " + ex.Message);
                consoleMutex.ReleaseMutex();
            }
        }
    }
}