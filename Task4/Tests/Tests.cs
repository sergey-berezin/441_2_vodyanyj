using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using System.Text;
using System.Net.Http.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;

namespace Task4
{
    public class TextControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        public TextControllerTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

     
        [Fact]
        public async Task EmptyTextTest()
        {
            var client = factory.CreateClient();
            var json = "{\"Text\":\"\",\"Questions\":[\"q1\"]}";
            var postResponse = await client.PostAsJsonAsync("api/questions", json);
            postResponse.EnsureSuccessStatusCode();
            var answersJson = await postResponse.Content.ReadAsStringAsync();
            var answers = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(answersJson);
            Assert.Empty(answers["Answers"]);
        }

        [Fact]
        public async Task EmptyQuestionsTest()
        {
            var client = factory.CreateClient();
            var json = "{\"Text\":\"Example\",\"Questions\":[]}";
            var postResponse = await client.PostAsJsonAsync("api/questions", json);
            postResponse.EnsureSuccessStatusCode();
            var answersJson = await postResponse.Content.ReadAsStringAsync();
            var answers = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(answersJson);
            Assert.Empty(answers["Answers"]);
        }

        [Fact]
        public async Task SimpleTest()
        {
            var client = factory.CreateClient();
            var json = "{\"Text\":\"This is text\",\"Questions\":[\"What is this\"]}";
            var postResponse = await client.PostAsJsonAsync("api/questions", json);
            postResponse.EnsureSuccessStatusCode();
            string answersJson = await postResponse.Content.ReadAsStringAsync();
            var answers = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(answersJson);
            Assert.Equal("text", answers["Answers"][0]);
        }

        [Fact]
        public async Task MultipleAnswersTest()
        {
            var client = factory.CreateClient();
            var json = "{\"Text\":\"This is a test\",\"Questions\":[\"What is this?\", \"Is it a test?\"]}";
            var postResponse = await client.PostAsJsonAsync("api/questions", json);
            postResponse.EnsureSuccessStatusCode();
            var answersJson = await postResponse.Content.ReadAsStringAsync();
            var answers = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(answersJson);
            Assert.Equal(2, answers["Answers"].Count);
        }

        [Fact]
        public async Task CaseInsensitiveQuestionTest()
        {
            var client = factory.CreateClient();
            var json = "{\"Text\":\"Capital city of France is Paris\",\"Questions\":[\"capital of france\"]}";
            var postResponse = await client.PostAsJsonAsync("api/questions", json);
            postResponse.EnsureSuccessStatusCode();
            var answersJson = await postResponse.Content.ReadAsStringAsync();
            var answers = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(answersJson);
            Assert.Equal("Paris", answers["Answers"][0]);
        }

        [Fact]
        public async Task SpecialCharactersInTextTest()
        {
            var client = factory.CreateClient();
            var json = "{\"Text\":\"@#$%^&*()\",\"Questions\":[\"What are these characters?\"]}";
            var postResponse = await client.PostAsJsonAsync("api/questions", json);
            postResponse.EnsureSuccessStatusCode();
            var answersJson = await postResponse.Content.ReadAsStringAsync();
            var answers = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(answersJson);
            Assert.Equal("special characters", answers["Answers"][0]);
        }
    }
}
}
