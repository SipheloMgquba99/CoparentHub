using CoparentHub.Domain.Common;

namespace CoparentHub.Domain.Entities
{
    public class FamilyMember : BaseEntity
    {
        public Guid FamilyId { get; private set; }
        public Guid UserId { get; private set; }

        private FamilyMember() { }

        internal static FamilyMember Create(Guid familyId, Guid userId) =>
            new() { FamilyId = familyId, UserId = userId };
    }

    public class Child : BaseEntity
    {
        public Guid FamilyId { get; private set; }
        public string Name { get; private set; } = default!;
        public DateOnly? DateOfBirth { get; private set; }

        public string? Allergies { get; private set; }
        public string? Medications { get; private set; }
        public string? MedicalNotes { get; private set; }
        public string? DoctorName { get; private set; }
        public string? DoctorPhone { get; private set; }
        public string? SchoolName { get; private set; }
        public string? SchoolContact { get; private set; }
        public string? ClothingSize { get; private set; }
        public string? ShoeSize { get; private set; }
        public string? EmergencyContactName { get; private set; }
        public string? EmergencyContactPhone { get; private set; }

        private Child() { }

        internal static Child Create(Guid familyId, string name, DateOnly? dob) =>
            new() { FamilyId = familyId, Name = name.Trim(), DateOfBirth = dob };

        public Result<Child> UpdateInfo(
            string? allergies, string? medications, string? medicalNotes,
            string? doctorName, string? doctorPhone,
            string? schoolName, string? schoolContact,
            string? clothingSize, string? shoeSize,
            string? emergencyContactName, string? emergencyContactPhone)
        {
            Allergies = allergies?.Trim();
            Medications = medications?.Trim();
            MedicalNotes = medicalNotes?.Trim();
            DoctorName = doctorName?.Trim();
            DoctorPhone = doctorPhone?.Trim();
            SchoolName = schoolName?.Trim();
            SchoolContact = schoolContact?.Trim();
            ClothingSize = clothingSize?.Trim();
            ShoeSize = shoeSize?.Trim();
            EmergencyContactName = emergencyContactName?.Trim();
            EmergencyContactPhone = emergencyContactPhone?.Trim();

            return Result<Child>.Ok(this);
        }
    }

    public class Family : BaseEntity
    {
        public string Name { get; private set; } = default!;

        private readonly List<FamilyMember> _members = [];
        private readonly List<Child> _children = [];

        public IReadOnlyList<FamilyMember> Members => _members.AsReadOnly();
        public IReadOnlyList<Child> Children => _children.AsReadOnly();

        private Family() { }

        public static Family Create(string name, Guid creatorId)
        {
            var family = new Family { Name = name.Trim() };
            family._members.Add(FamilyMember.Create(family.Id, creatorId));
            return family;
        }

        public Result<FamilyMember> AddMember(Guid userId)
        {
            if (_members.Count >= 2)
                return Result<FamilyMember>.Fail("Family already has 2 co-parents.");
            if (_members.Any(m => m.UserId == userId))
                return Result<FamilyMember>.Fail("User is already a member.");

            var member = FamilyMember.Create(Id, userId);
            _members.Add(member);
            return Result<FamilyMember>.Ok(member);
        }

        public Result<Child> AddChild(string name, DateOnly? dob = null)
        {
            if (_children.Count >= 5)
                return Result<Child>.Fail("A family can have at most 5 children.");

            if (_children.Any(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
                return Result<Child>.Fail($"A child named '{name}' already exists.");

            var child = Child.Create(Id, name, dob);
            _children.Add(child);

            return Result<Child>.Ok(child);
        }

        public Result<Child> RemoveChild(Guid childId)
        {
            var child = _children.FirstOrDefault(c => c.Id == childId);
            if (child is null)
                return Result<Child>.Fail("Child not found.");

            _children.Remove(child);
            return Result<Child>.Ok(child);
        }

        public bool IsMember(Guid userId) => _members.Any(m => m.UserId == userId);
        public bool HasChild(Guid childId) => _children.Any(c => c.Id == childId);
    }
}