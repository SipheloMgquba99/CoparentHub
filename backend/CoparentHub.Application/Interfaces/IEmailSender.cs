namespace CoparentHub.Application.Interfaces
{
    public interface IEmailSender
    {
        /// <summary>
        /// True when a real provider is configured. When false, callers should surface a
        /// clear failure to the user rather than attempt to send (see <see cref="NullEmailSender"/>
        /// in CoparentHub.Infrastructure).
        /// </summary>
        bool IsConfigured { get; }

        Task SendFamilyInviteAsync(
            string toEmail,
            string familyName,
            string inviterFullName,
            string code,
            DateTime expiresAt,
            CancellationToken ct = default);
    }
}
