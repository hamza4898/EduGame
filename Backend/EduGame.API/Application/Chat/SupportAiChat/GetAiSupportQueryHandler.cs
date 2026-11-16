using MediatR;
using Ardalis.Result;
using EduGame.Application.Common.Contracts;
using EduGame.Application.Common.Interfaces;
using System.Text.Json.Serialization;
using EduGame.Domain.Entities;

namespace EduGame.Application.Chat.SupportAiChat;

public class GetAiSupportQueryHandler(
    IChatRepository communityRepository,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<GetAiSupportQueryHandler> logger)
    : IRequestHandler<GetAiSupportQuery, Result<AiSupportMessageResponseDto>>
{
    private const int ContextMessageLimit = 12;

    public async Task<Result<AiSupportMessageResponseDto>> Handle(GetAiSupportQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return Result.Unauthorized("Не удалось определить пользователя.");

        if (string.IsNullOrWhiteSpace(request.UserPrompt)) return Result.Invalid(new ValidationError("Введите сообщение."));

        var knowledgePath = Path.Combine(AppContext.BaseDirectory, "ai-knowledge.txt");

        if (!File.Exists(knowledgePath)) throw new FileNotFoundException("The AI support knowledge file was not found.", knowledgePath);

        var systemInstruction = await File.ReadAllTextAsync(knowledgePath, cancellationToken);

        var previousMessages = await communityRepository.GetAiSupportHistoryAsync(request.UserId, ContextMessageLimit, cancellationToken);

        var messages = new List<OllamaChatRequestMessage>(previousMessages.Count + 2)
        {
            new("system", systemInstruction)
        };
        
        var ollamaFriendlyMessages = previousMessages.Select(msg => new OllamaChatRequestMessage(msg.Role, msg.Content));

        messages.AddRange(ollamaFriendlyMessages);
        messages.Add(new OllamaChatRequestMessage("user", request.UserPrompt));

        var model = configuration["Ollama:Model"] ?? "qwen2.5-coder:1.5b";

        using var response = await httpClientFactory.CreateClient("Ollama")
            .PostAsJsonAsync("api/chat", new OllamaChatRequest(model, messages), cancellationToken);

        response.EnsureSuccessStatusCode();

        var chatResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);

        var assistantContent = chatResponse?.Message?.Content;

        if (string.IsNullOrWhiteSpace(assistantContent)) throw new InvalidOperationException("Ollama returned an empty AI support response.");

        var userMessage = new AiSupportMessage
        {
            UserId = request.UserId,
            Role = "user",
            Content = request.UserPrompt
        };

        var assistantMessage = new AiSupportMessage
        {
            UserId = request.UserId,
            Role = "assistant",
            Content = assistantContent
        };

        await communityRepository.SaveAiSupportExchangeAsync(userMessage, assistantMessage, cancellationToken);
        await communityRepository.Save();

        logger.LogInformation("Stored an AI support exchange for user {UserId}", request.UserId);

        return Result.Success(new AiSupportMessageResponseDto(
            assistantMessage.Id,
            assistantMessage.Role,
            assistantMessage.Content,
            assistantMessage.CreatedAt));
    }

    private sealed record OllamaChatRequest(string Model, IReadOnlyList<OllamaChatRequestMessage> Messages)
    {
        [JsonPropertyName("stream")]
        public bool Stream => false;

        [JsonPropertyName("options")]
        public OllamaChatOptions Options => new(Temperature: 0.2, NumPredict: 400);
    }

    private sealed record OllamaChatRequestMessage(string Role, string Content);

    private sealed record OllamaChatOptions(
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("num_predict")] int NumPredict);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatResponseMessage? Message);

    private sealed record OllamaChatResponseMessage(
        [property: JsonPropertyName("content")] string? Content);
}