using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bert_neural_network;
using System.Text.Json;

namespace Task4.Controllers
{
    public class QuestionRequest
    {
        public string Text { get; set; }
        public List<string> Questions { get; set; }
    }

    public class QuestionResponse
    {
        public List<string> Answers { get; set; }
    }


    [ApiController]
    [Route("api/questions")]
    public class QuestionController : Controller
    {
        private readonly Bert bert_;
        private CancellationTokenSource token_source_;

        public QuestionController()
        {
            string url = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
            string path = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
            token_source_ = new CancellationTokenSource();
            bert_ = new Bert(path, token_source_.Token);
            bert_.DownloadBert(path, url);
        }

        [HttpGet]
        public IActionResult Index()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "../index.html");
            return Content(System.IO.File.ReadAllText(filePath), "text/html");
        }


        [HttpPost]
        public async Task<IActionResult> AnswerQuestions([FromBody] QuestionRequest request)
        {

            Console.WriteLine(request.Text);
            try
            {
                var answers = new List<string>();
                foreach (var question in request.Questions)
                {
                    var answer = await bert_.GetAnswerAsync(request.Text, question, token_source_.Token);
                    answers.Add(answer);    
                }

                return Ok(JsonSerializer.Serialize(new { Answers = answers }));
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }
    }
}
