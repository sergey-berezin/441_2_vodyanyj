using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Bert_neural_network;
using System.Threading;
using Newtonsoft.Json;

namespace Task2
{
    public partial class MainWindow : Window
    {
        public static string modelUrl = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        public static string modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        public static CancellationTokenSource сancellationTokenSource = new();
        public static CancellationToken сancellationToken = сancellationTokenSource.Token;
        public static Bert bert = new Bert(modelPath, сancellationToken);
        private string historyFilePath = "dialog_history.json";
        public MainWindow()
        {
            InitializeComponent();
            bert.DownloadBertAsync();
        }

        private void LoadTextButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string text = File.ReadAllText(openFileDialog.FileName);
                    LoadedTextControl.Text = text;
                    DialogHistoryListBox.Items.Clear();
                    InitializeDialog();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void InitializeDialog()
        {
            DialogHistory? history = LoadHistory();
            if (history != null)
            {
                foreach (var context in history.Contexts)
                {
                    if (context.Text == LoadedTextControl.Text)
                    {
                        foreach (var entry in context.Entries)
                        {
                            Message question = new Message { IsQuestion = true, Text = entry.Question };
                            Message answer = new Message { IsQuestion = false, Text = entry.Answer };

                            AddMessage(question);
                            AddMessage(answer);
                        }
                        return;
                    }
                    
                }

            }

        }

        private async void GetAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            string question_text = QuestionTextBox.Text;
            if (question_text == "")
            {
                MessageBox.Show("Вы не ввели вопрос");
                return;
            }
            if (LoadedTextControl.Text == "")
            {
                MessageBox.Show("Сначала загрузите файл");
                return;
            }
            
            DialogHistory? history = LoadHistory();

            // Check if text has already been processed
            DialogContext? context = history.Contexts.FirstOrDefault(ctx => ctx.Text == LoadedTextControl.Text);

            if (context == null)
            {
                context = new DialogContext { Text = LoadedTextControl.Text };
                history.Contexts.Add(context);
            }

            DialogEntry? savedEntry = context.Entries.FirstOrDefault(entry => entry.Question == question_text);

            if (savedEntry != null)
            {
                string? answer_text = savedEntry.Answer;
                Message question = new Message { IsQuestion = true, Text = question_text };
                Message answer = new Message { IsQuestion = false, Text = answer_text };

                AddMessage(question);
                AddMessage(answer);
            }

            else
            {
                GetAnsButton.IsEnabled = false;
                string answer_text = "";
                QuestionTextBox.Clear();
                try
                {
                    if (!Bert.isDownloaded)
                    {
                        MessageBox.Show("Модель еще не загружена, пожалуйста, подождите");
                    }
                    else
                    {
                        answer_text = await bert.GetAnswerAsync(LoadedTextControl.Text, question_text, сancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                Message question = new Message { IsQuestion = true, Text = question_text };
                Message answer = new Message { IsQuestion = false, Text = answer_text };

                AddMessage(question);
                AddMessage(answer);

                context.Entries.Add(new DialogEntry { Question = question_text, Answer = answer_text });
               
                SaveHistory(history);
            }
            GetAnsButton.IsEnabled = true;
        }

        private void AddMessage(Message entry)
        {
            DialogHistoryListBox.Items.Add(entry);
            DialogHistoryListBox.ScrollIntoView(entry);
        }

        public class Message
        {
            public bool IsQuestion { get; set; }
            public string? Text { get; set; }
        }

        private void CancelAnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            сancellationTokenSource.Cancel();
        }

        [Serializable]
        public class DialogHistory
        {
            public List<DialogContext> Contexts { get; set; } = new List<DialogContext>();
        }

        [Serializable]
        public class DialogContext
        {
            public string Text { get; set; }
            public List<DialogEntry> Entries { get; set; } = new List<DialogEntry>();
        }

        [Serializable]
        public class DialogEntry
        {
            public string? Question { get; set; }
            public string? Answer { get; set; }
        }

        private void SaveHistory(DialogHistory history)
        {
            string json = JsonConvert.SerializeObject(history);
            File.WriteAllText(historyFilePath, json);
        }

        private DialogHistory LoadHistory()
        {
            if (File.Exists(historyFilePath))
            {
                string json = File.ReadAllText(historyFilePath);
                return JsonConvert.DeserializeObject<DialogHistory>(json);
            }
            return new DialogHistory();
        }

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(historyFilePath);
            DialogHistoryListBox.Items.Clear();
        }

    }
}
