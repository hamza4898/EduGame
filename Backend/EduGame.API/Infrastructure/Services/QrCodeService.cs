using QRCoder;
using EduGame.Application.Common.Interfaces;

namespace EduGame.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    public string CreateTotpUrl(string issuer, string username, string secret)
    {
        var encodedUsername = Uri.EscapeDataString(username);
        return $"otpauth://totp/{issuer}:{encodedUsername}?secret={secret}&issuer={issuer}";
    }

    public string GenerateBase64Png(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        var qrCodeBytes = qrCode.GetGraphic(20);
        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
    }
}