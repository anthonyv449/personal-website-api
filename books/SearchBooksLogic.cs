using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace personal_website_api.Books
{
    public static class SearchBooksLogic
    {
        private const string HardcoverEndpoint = "https://api.hardcover.app/v1/graphql";

        public static async Task<List<BookResult>> Execute(HttpClient http, string? title, string? author, string token)
        {
            var whereClause = BuildWhereClause(title, author);
            var queryStr = $"query {{ books({whereClause} limit: 20) {{ title image {{ url }} rating contributions {{ author {{ name }} }} }} }}";

            var payload = JsonSerializer.Serialize(new { query = queryStr });
            var request = new HttpRequestMessage(HttpMethod.Post, HardcoverEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<GraphQLResponse<BooksData>>(json, options);

            return result?.Data?.Books?
                .Select(b => new BookResult
                {
                    Title = b.Title ?? string.Empty,
                    Authors = b.Contributions?
                        .Where(c => c.Author?.Name != null)
                        .Select(c => c.Author!.Name!)
                        .ToList() ?? [],
                    ImageUrl = b.Image?.Url,
                    Rating = b.Rating
                })
                .ToList() ?? [];
        }

        private static string BuildWhereClause(string? title, string? author)
        {
            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(title))
                conditions.Add("{title: {_ilike: \"%" + Escape(title) + "%\"}}");

            if (!string.IsNullOrWhiteSpace(author))
                conditions.Add("{contributions: {author: {name: {_ilike: \"%" + Escape(author) + "%\"}}}}");

            if (conditions.Count == 0)
                return string.Empty;

            return $"where: {{_and: [{string.Join(", ", conditions)}]}}";
        }

        // Strip characters that could break the inline GraphQL string
        private static string Escape(string input) =>
            input.Replace("\\", "").Replace("\"", "").Replace("\n", "").Replace("\r", "");
    }
}
