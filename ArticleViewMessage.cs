namespace personal_website_api;

public record ArticleViewMessage(int ArticleId, string ViewerId, DateTime ViewedAt);
