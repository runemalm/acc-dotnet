using System.Net.Http.Json;
using ACC.Identity.Application.Ports.Communication;
using Microsoft.Extensions.Configuration;

namespace ACC.Identity.Infrastructure.Communication;

public sealed class ResendIdentityEmailSender : IIdentityEmailSender
{
    private const string From = "noreply@efbokforing.se";
    private const string Subject = "Verify your email";
    private const string SendEmailUrl = "https://api.resend.com/emails";
    private readonly HttpClient httpClient;
    private readonly string apiKey;
    private readonly string frontendUrl;
    private readonly string verifyEmailPath;

    public ResendIdentityEmailSender(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        this.httpClient = httpClient;
        apiKey = configuration["Email:Resend:ApiKey"] ?? string.Empty;
        frontendUrl = configuration["Frontend:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend:BaseUrl is missing.");
        verifyEmailPath = configuration["Identity:Email:VerifyEmailPath"]
            ?? "/verify-email";
    }

    public void SendVerificationEmail(
        string email,
        string verificationToken,
        DateTimeOffset expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(verificationToken);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Resend API key is not configured.");
        }

        var encodedToken = Uri.EscapeDataString(verificationToken);
        var verificationUrl =
            $"{frontendUrl.TrimEnd('/')}/{verifyEmailPath.Trim('/')}" +
            $"?token={encodedToken}";
        var payload = new
        {
            from = From,
            to = new[] { email },
            subject = Subject,
            html = $"""
                    <p>Welcome!</p>
                    <p>Click the link below to verify your email:</p>
                    <a href="{verificationUrl}">Verify Email</a>
                    """
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            SendEmailUrl);

        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = JsonContent.Create(payload);

        using var response = httpClient.Send(request);
        response.EnsureSuccessStatusCode();
    }
}
