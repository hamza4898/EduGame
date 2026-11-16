namespace EduGame.Application.Developer.CreateDeveloperUpdate;

public record CreateDeveloperUpdateRequest(
    string Title, 
    string Category, 
    string Content);