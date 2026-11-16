namespace EduGame.Application.Courses.SaveCoursePrice;

public sealed record SaveCoursePriceRequestDto(
    bool IsPaid, 
    int PriceCoins);