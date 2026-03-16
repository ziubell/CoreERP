using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CoreERP.Infrastructure.Email;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendNotificaEmailAsync(string toEmail, string titolo, string? messaggio, string? link, string? mittenteNome);
}

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var subject = "Reimposta la tua password - CoreERP";
        var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <h2 style=""color: #333;"">Reimposta la tua password</h2>
    <p>Hai richiesto di reimpostare la password del tuo account CoreERP.</p>
    <p>Clicca sul pulsante qui sotto per procedere:</p>
    <p style=""text-align: center; margin: 30px 0;"">
        <a href=""{resetLink}""
           style=""background-color: #16B1FF; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;"">
            Reimposta Password
        </a>
    </p>
    <p style=""color: #666; font-size: 14px;"">Se non hai richiesto tu il reset della password, puoi ignorare questa email.</p>
    <p style=""color: #666; font-size: 14px;"">Il link scadrà tra 24 ore.</p>
    <hr style=""border: none; border-top: 1px solid #eee; margin: 20px 0;"">
    <p style=""color: #999; font-size: 12px;"">CoreERP - Spadhausen</p>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendNotificaEmailAsync(string toEmail, string titolo, string? messaggio, string? link, string? mittenteNome)
    {
        var subject = $"CoreERP - {titolo}";
        var senderInfo = mittenteNome is not null
            ? $"<p style=\"color: #555; font-size: 14px;\">Da: <strong>{mittenteNome}</strong></p>"
            : "";
        var linkButton = link is not null
            ? $@"<p style=""text-align: center; margin: 30px 0;"">
                <a href=""{link}""
                   style=""background-color: #16B1FF; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;"">
                    Apri in CoreERP
                </a>
            </p>"
            : "";

        var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <h2 style=""color: #333;"">{titolo}</h2>
    {senderInfo}
    {(messaggio is not null ? $"<p>{messaggio}</p>" : "")}
    {linkButton}
    <hr style=""border: none; border-top: 1px solid #eee; margin: 20px 0;"">
    <p style=""color: #999; font-size: 12px;"">CoreERP - Spadhausen</p>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _configuration["Smtp:FromName"] ?? "CoreERP",
            _configuration["Smtp:FromEmail"] ?? "noreply@spadhausen.com"));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                _configuration["Smtp:Host"],
                int.Parse(_configuration["Smtp:Port"] ?? "587"),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _configuration["Smtp:Username"],
                _configuration["Smtp:Password"]);

            await client.SendAsync(message);
            _logger.LogInformation("Email inviata a {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore invio email a {Email}", toEmail);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
