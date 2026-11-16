using EduGame.Application.Common.Interfaces;
using System.Text.Json;
using Ardalis.Result;

namespace EduGame.Infrastructure.Services;

public class QuizValidationService : IQuizValidationService
{
    public Result ValidateQuizAnswer(string quizContent, int? selectedAnswer)
    {
        if (selectedAnswer is null)  return Result.Invalid(new ValidationError("Выберите ответ на квиз."));

        try
        {
            using var quizData = JsonDocument.Parse(quizContent);
            var correctAnswer = quizData.RootElement.GetProperty("answer").GetInt32();
            
            if (selectedAnswer != correctAnswer)
                return Result.Invalid(new ValidationError("Ответ на квиз неверный."));
                
            return Result.Success();
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            return Result.Invalid(new ValidationError("Не удалось проверить квиз этого урока."));
        }
    }
}