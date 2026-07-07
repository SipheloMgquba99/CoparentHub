using System;
using System.Collections.Generic;
using System.Text;

namespace CoparentHub.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IFamilyRepository Families { get; }
        IEventRepository Events { get; }
        INotificationRepository Notifications { get; }
        IFamilyInviteRepository Invites { get; }
        IPasswordResetTokenRepository PasswordResetTokens { get; }
        IPushSubscriptionRepository PushSubscriptions { get; }
        Task SaveAsync(CancellationToken ct = default);
    }
}
