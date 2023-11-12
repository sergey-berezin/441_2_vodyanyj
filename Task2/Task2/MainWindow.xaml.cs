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

namespace Task2
{
    public partial class MainWindow : Window
    {
        public static string modelUrl = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        public static string modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        public static CancellationTokenSource сancellationTokenSource = new();
        public static CancellationToken сancellationToken = сancellationTokenSource.Token;
        public static Bert bert = new Bert(modelPath, сancellationToken);

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

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
