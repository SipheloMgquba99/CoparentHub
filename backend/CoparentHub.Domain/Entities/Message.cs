using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public class Message : BaseEntity
    {
        public Guid FamilyId { get; private set; }
        public Guid SenderUserId { get; private set; }
        public string Body { get; private set; } = default!;
        public bool IsReadByRecipient { get; private set; }

        private Message() { }

        public static Message Create(Guid familyId, Guid senderUserId, string body) => new()
        {
            FamilyId = familyId,
            SenderUserId = senderUserId,
            Body = body.Trim(),
            IsReadByRecipient = false,
        };
    }
}
