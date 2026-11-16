using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduGame.Infrastructure.Services;

internal static class CertificatePdf
{
    static CertificatePdf()
    {
        Settings.License = LicenseType.Community;
    }

    public static byte[] Create(string studentName, string courseTitle, string signer, DateTime date)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(0); 

                page.DefaultTextStyle(TextStyle.Default.FontFamily("Arial").FontColor("#F2F4F7"));

                page.Content()
                    .Background("#0D1A33") 
                    .Padding(18)
                    .Background("#172947") 
                    .Border(2, "#D1AD52") 
                    .Padding(16)
                    .Border(1, "#D1AD52") 
                    .Padding(40)
                    .Column(column =>
                    {
                        column.Item().PaddingBottom(30).Text("EDUGAME")
                            .FontColor("#D1AD52")
                            .FontSize(20)
                            .Bold()
                            .LetterSpacing(0.2f)
                            .AlignCenter();

                        column.Item().PaddingBottom(25).Text("СЕРТИФИКАТ О ПРОХОЖДЕНИИ")
                            .FontColor("#EBEEF5")
                            .FontSize(28)
                            .Bold()
                            .AlignCenter();

                        column.Item().PaddingBottom(15).Text("Настоящий документ подтверждает, что")
                            .FontColor("#CCDBF0")
                            .FontSize(14)
                            .AlignCenter();

                        column.Item().Text(studentName)
                            .FontColor("#F2D159")
                            .FontSize(32)
                            .Bold()
                            .AlignCenter();

                        column.Item()
                            .AlignCenter() 
                            .PaddingTop(5)
                            .PaddingBottom(20)
                            .Width(450)
                            .Height(1)
                            .Background("#D1AD52");

                        column.Item().PaddingBottom(15).Text("успешно завершил(а) обучение на курсе")
                            .FontColor("#CCDBF0")
                            .FontSize(14)
                            .AlignCenter();

                        column.Item().Text(courseTitle)
                            .FontColor("#EBEEF5")
                            .FontSize(22)
                            .Bold()
                            .AlignCenter();

                        column.Item().AlignBottom().Row(row =>
                        {
                            row.RelativeItem().Column(dateCol =>
                            {
                                dateCol.Item().Text(date.ToString("dd MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")))
                                    .FontColor("#CCDBF0")
                                    .FontSize(12)
                                    .AlignCenter();
                                
                                dateCol.Item()
                                    .AlignCenter()
                                    .PaddingTop(4)
                                    .Width(120)
                                    .Height(1)
                                    .Background("#D1AD52");
                                    
                                dateCol.Item()
                                    .PaddingTop(4)
                                    .Text("ДАТА ВЫДАЧИ")
                                    .FontColor("#CCDBF0")
                                    .FontSize(8)
                                    .AlignCenter();
                            });

                            row.RelativeItem().Column(signerCol =>
                            {
                                signerCol.Item().Text(signer)
                                    .FontColor("#D1AD52")
                                    .FontSize(14)
                                    .Italic()
                                    .AlignCenter();
                                    
                                signerCol.Item()
                                    .AlignCenter()
                                    .PaddingTop(4)
                                    .Width(120)
                                    .Height(1)
                                    .Background("#D1AD52");
                                    
                                signerCol.Item()
                                    .PaddingTop(4)
                                    .Text("ИНСТРУКТОР КУРСА")
                                    .FontColor("#CCDBF0")
                                    .FontSize(8)
                                    .AlignCenter();
                            });
                        });
                    });
            });
        })
        .GeneratePdf();
    }
}