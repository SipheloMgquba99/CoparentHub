using CoparentHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace CoparentHub.Infrastructure
{
    public class BrevoEmailSender(HttpClient http, IConfiguration config) : IEmailSender
    {
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(config["Brevo:ApiKey"]) &&
            !string.IsNullOrWhiteSpace(config["Brevo:SenderEmail"]);

        public async Task SendFamilyInviteAsync(
            string toEmail,
            string familyName,
            string inviterFullName,
            string code,
            DateTime expiresAt,
            CancellationToken ct = default)
        {
            var senderEmail = config["Brevo:SenderEmail"]!;
            var senderName = config["Brevo:SenderName"];
            if (string.IsNullOrWhiteSpace(senderName)) senderName = "coparenthub";

            var appUrl = config["Cors:AllowedOrigins:0"];
            if (string.IsNullOrWhiteSpace(appUrl)) appUrl = "https://app.coparenthub.com";

            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail } },
                subject = $"{inviterFullName} invited you to join {familyName} on coparenthub",
                htmlContent = BuildHtml(familyName, inviterFullName, code, expiresAt, appUrl),
            };

            var response = await http.PostAsJsonAsync("smtp/email", payload, ct);
            response.EnsureSuccessStatusCode();
        }
        private static string BuildHtml(
            string familyName, string inviterFullName, string code, DateTime expiresAt, string appUrl) => $"""
            <!DOCTYPE html>
            <html>
            <body style="margin:0;padding:0;background-color:#f4f1ea;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f1ea;padding:32px 0;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="480" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.06);">
                      <tr>
                        <td style="background-color:#1f2937;padding:28px 32px;text-align:center;">
                          <span style="font-family:Georgia,'Times New Roman',serif;font-size:22px;color:#f4c95d;font-weight:bold;letter-spacing:0.5px;">coparenthub</span>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:32px;font-family:Arial,Helvetica,sans-serif;">
                          <p style="margin:0 0 16px;font-size:16px;color:#1f2937;">
                            <strong>{inviterFullName}</strong> invited you to join <strong>{familyName}</strong> as a co-parent on coparenthub.
                          </p>
                          <p style="margin:0 0 24px;font-size:14px;color:#4b5563;line-height:1.5;">
                            coparenthub helps separated and co-parenting families share schedules, events, and updates about their kids in one place.
                          </p>
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:24px;">
                            <tr>
                              <td align="center" style="background-color:#f4f1ea;border-radius:8px;padding:18px;">
                                <div style="font-size:12px;color:#6b7280;text-transform:uppercase;letter-spacing:1px;margin-bottom:6px;">Your invite code</div>
                                <div style="font-size:28px;letter-spacing:4px;font-weight:bold;color:#1f2937;font-family:'Courier New',monospace;">{code}</div>
                              </td>
                            </tr>
                          </table>
                          <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 auto 24px;">
                            <tr>
                              <td align="center" style="border-radius:8px;background-color:#f4c95d;">
                                <a href="{appUrl}" style="display:inline-block;padding:12px 28px;font-size:15px;font-weight:bold;color:#1f2937;text-decoration:none;">Open coparenthub</a>
                              </td>
                            </tr>
                          </table>
                          <p style="margin:0;font-size:13px;color:#9ca3af;">
                            This code expires {expiresAt:MMMM d, yyyy 'at' h:mm tt} UTC. If you weren't expecting this invite, you can safely ignore this email.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    /// <summary>
    /// Registered when no Brevo API key is configured (e.g. local dev). Never sends —
    /// callers must check <see cref="IsConfigured"/> and surface a clear failure instead of
    /// calling <see cref="SendFamilyInviteAsync"/>, so a call reaching here is unexpected.
    /// </summary>
    public class NullEmailSender(ILogger<NullEmailSender> logger) : IEmailSender
    {
        public bool IsConfigured => false;

        public Task SendFamilyInviteAsync(
            string toEmail, string familyName, string inviterFullName, string code, DateTime expiresAt,
            CancellationToken ct = default)
        {
            logger.LogWarning(
                "SendFamilyInviteAsync called with no email provider configured (to={ToEmail}, family={FamilyName})",
                toEmail, familyName);
            return Task.CompletedTask;
        }
    }
}
