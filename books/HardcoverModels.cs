using System.Collections.Generic;

namespace personal_website_api.Books
{
    // --- Internal GraphQL deserialization models ---

    internal class GraphQLResponse<T>
    {
        public T? Data { get; set; }
    }

    internal class BooksData
    {
        public List<HardcoverBook>? Books { get; set; }
    }

    internal class HardcoverBook
    {
        public string? Title { get; set; }
        public HardcoverImage? Image { get; set; }
        public double? Rating { get; set; }
        public List<HardcoverContribution>? Contributions { get; set; }
    }

    internal class HardcoverImage
    {
        public string? Url { get; set; }
    }

    internal class HardcoverContribution
    {
        public HardcoverAuthor? Author { get; set; }
    }

    internal class HardcoverAuthor
    {
        public string? Name { get; set; }
    }

    // --- Public response model ---

    public class BookResult
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Authors { get; set; } = [];
        public string? ImageUrl { get; set; }
        public double? Rating { get; set; }
    }
}
