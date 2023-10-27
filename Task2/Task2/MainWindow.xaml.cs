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

namespace Task2
{
    public partial class MainWindow : Window
    {

        ViewData viewData = new ViewData();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewData;
            Chat.ItemsSource = viewData.ChatView;
            viewData.GetBertAsync();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            viewData.сancellationTokenSource.Cancel();
        }

        private async void ButtonSendClick(object sender, RoutedEventArgs e)
        {
            ButtonSend.IsEnabled = false;
            try
            {
                string question = TextBoxQuestion.Text;
                TextBoxQuestion.Clear();
                viewData.ChatView.Add(question);
                Chat.Items.Refresh();
                if (question.StartsWith("/load"))
                {
                    var openFileDialog = new OpenFileDialog()
                    {
                        Title = "File",
                        Filter = "Text Document (*.txt) | *.txt",
                        FileName = ""
                    };
                    if (openFileDialog.ShowDialog() == true)
                    {
                        viewData.text = File.ReadAllText(openFileDialog.FileName);
                        viewData.ChatView.Add(viewData.text);
                    }
                }
                else if (!viewData.isDownloaded)
                {
                    MessageBox.Show("Model is not downloaded yet");
                }
                else if (viewData.text == "")
                {
                    MessageBox.Show("Enter /load to get text file");
                }
                else
                {
                    var answer = await viewData.bert.GetAnswerAsync(viewData.text, question, viewData.сancellationToken);
                    if (answer != null)
                    {
                        viewData.ChatView.Add("Answer: " + answer);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Chat.Items.Refresh();
            ButtonSend.IsEnabled = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
