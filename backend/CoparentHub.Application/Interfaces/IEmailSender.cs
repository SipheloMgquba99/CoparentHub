namespace CoparentHub.Application.Interfaces
{
    public interface IEmailSender
    {
        bool IsConfigured { get; }

        Task SendFamilyInviteAsync(
            string toEmail,
            string familyName,
            string inviterFullName,
            string code,
            DateTime expiresAt,
            CancellationToken ct = default);

        Task SendPasswordResetAsync(
            string toEmail,
            string fullName,
            string token,
            DateTime expiresAt,
            CancellationToken ct = default);
    }
}
