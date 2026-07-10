using CoparentHub.Application.Features.DTOs;
using CoparentHub.Application.Interfaces;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;
using System.Text.Json;

namespace CoparentHub.Application.Features.Messages
{
    public class SendMessageHandler(IUnitOfWork uow, IPushSender pushSender)
        : IRequestHandler<SendMessageCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(SendMessageCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<Guid>.Fail("Access denied.");

            if (family.Members.Count != 2)
                return Result<Guid>.Fail("Invite a co-parent before sending messages.");

            var message = Message.Create(cmd.FamilyId, cmd.UserId, cmd.Body);
            uow.Messages.Add(message);

            var recipient = family.Members.First(m => m.UserId != cmd.UserId);
            var sender = await uow.Users.GetByIdAsync(cmd.UserId, ct);
            var senderName = sender?.FullName ?? "Someone";

            uow.Notifications.Add(Notification.Create(
                userId: recipient.UserId,
                familyId: cmd.FamilyId,
                type: NotificationType.NewMessage,
                message: $"{senderName} sent you a message.",
                eventId: null));

            await uow.SaveAsync(ct);

            if (pushSender.IsConfigured)
            {
                var payload = JsonSerializer.Serialize(new { title = "New message", body = $"{senderName} sent you a message.", url = "/" });
                await CoparentHub.Application.Features.Push.PushFanout.SendToUsersAsync(
                    uow, pushSender, [recipient.UserId], payload, ct);
            }

            return Result<Guid>.Ok(message.Id);
        }
    }

    public class MarkThreadReadHandler(IUnitOfWork uow)
        : IRequestHandler<MarkThreadReadCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(MarkThreadReadCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<bool>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<bool>.Fail("Access denied.");

            await uow.Messages.MarkThreadReadAsync(cmd.FamilyId, cmd.UserId, ct);

            return Result<bool>.Ok(true);
        }
    }

    public class GetMessagesHandler(IUnitOfWork uow)
        : IRequestHandler<GetMessagesQuery, Result<List<MessageDto>>>
    {
        public async Task<Result<List<MessageDto>>> Handle(GetMessagesQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<List<MessageDto>>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<List<MessageDto>>.Fail("Access denied.");

            var messages = await uow.Messages.GetByFamilyAsync(q.FamilyId, ct);

            var senderNames = new Dictionary<Guid, string>();
            foreach (var senderId in messages.Select(m => m.SenderUserId).Distinct())
            {
                var user = await uow.Users.GetByIdAsync(senderId, ct);
                senderNames[senderId] = user?.FullName ?? "Unknown";
            }

            var dtos = messages
                .Select(m => new MessageDto(
                    m.Id,
                    m.FamilyId,
                    m.SenderUserId,
                    senderNames.GetValueOrDefault(m.SenderUserId, "Unknown"),
                    m.Body,
                    m.IsReadByRecipient,
                    m.CreatedAt))
                .ToList();

            return Result<List<MessageDto>>.Ok(dtos);
        }
    }
}
