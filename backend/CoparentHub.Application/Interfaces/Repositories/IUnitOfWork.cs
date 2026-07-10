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
        IExpenseRepository Expenses { get; }
        IMessageRepository Messages { get; }
        Task SaveAsync(CancellationToken ct = default);

        // Wraps multiple Save/Execute calls in one DB transaction.
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default);
    }
}
