using Ardalis.Result;

namespace EduGame.Application.Common.Interfaces;

public interface IQuizValidationService
{
    Result ValidateQuizAnswer(string quizContent, int? selectedAnswer);
}