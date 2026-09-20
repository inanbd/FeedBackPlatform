using FeedbackPlatform.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FeedbackPlatform.Infrastructure.Email;

public sealed class EmailNotificationBackgroundService(
    EmailNotificationQueue queue,
    IOptionsMonitor<EmailOptions> options,
    ILogger<EmailNotificationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
        {
            var settings = options.CurrentValue;
            if (!settings.Enabled)
            {
                continue;
            }

            try
            {
                await SendAsync(settings, message, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send feedback notification email to {OwnerEmail}", message.OwnerEmail);
            }
        }
    }

    private static async Task SendAsync(EmailOptions settings, FeedbackNotificationMessage message, CancellationToken ct)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.OwnerEmail));
        mime.Subject = $"New feedback for {message.AppName}: {message.FeedbackTitle}";

        var stars = new string('★', Math.Clamp(message.StarRating, 0, 5)).PadRight(5, '☆');
        mime.Body = new TextPart("plain")
        {
            Text =
                $"""
                 You received new feedback for "{message.AppName}".

                 Title:   {message.FeedbackTitle}
                 Rating:  {stars} ({message.StarRating}/5)
                 Version: {message.AppVersion}
                 When:    {message.CreatedAtUtc:u}

                 Comment:
                 {message.FeedbackComment}
                 """
        };

        using var client = new SmtpClient();
        var socketOptions = settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, socketOptions, ct);

        if (!string.IsNullOrEmpty(settings.Username))
        {
            await client.AuthenticateAsync(settings.Username, settings.Password, ct);
        }

        await client.SendAsync(mime, ct);
        await client.DisconnectAsync(true, ct);
    }
}
