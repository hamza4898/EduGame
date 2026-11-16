namespace EduGame.Application.Common.Interfaces;

public interface IQrCodeService
{
    string GenerateBase64Png(string content);

    string CreateTotpUrl(string issuer, string username, string secret);
}