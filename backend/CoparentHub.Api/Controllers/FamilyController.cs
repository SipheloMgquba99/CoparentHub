using CoparentHub.Application.Features.Family;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CoparentHub.Api.Controllers
{
    [Route("api/family")]
    [Authorize]
    public class FamilyController(ISender sender) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetMine(CancellationToken ct)
            => ToResponse(await sender.Send(new GetMyFamiliesQuery(CurrentUserId), ct));

        [HttpGet("{familyId:guid}")]
        public async Task<IActionResult> Get(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new GetFamilyQuery(familyId, CurrentUserId), ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFamilyCommand cmd, CancellationToken ct)
            => ToResponse(await sender.Send(cmd with { UserId = CurrentUserId }, ct));

        [HttpPost("join")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Join([FromBody] JoinFamilyRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(new JoinFamilyByCodeCommand(req.Code, CurrentUserId), ct));

        [HttpDelete("{familyId:guid}")]
        public async Task<IActionResult> Delete(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new DeleteFamilyCommand(familyId, CurrentUserId), ct));

        [HttpGet("pending-invite")]
        public async Task<IActionResult> GetPendingInvite(CancellationToken ct)
            => ToResponse(await sender.Send(new GetPendingInviteQuery(CurrentUserEmail), ct));

        [HttpPost("{familyId:guid}/invites")]
        public async Task<IActionResult> CreateInvite(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new CreateFamilyInviteCommand(familyId, CurrentUserId), ct));

        [HttpGet("{familyId:guid}/invites/active")]
        public async Task<IActionResult> GetActiveInvite(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new GetActiveFamilyInviteQuery(familyId, CurrentUserId), ct));

        [HttpPost("{familyId:guid}/invites/email")]
        public async Task<IActionResult> SendInviteEmail(Guid familyId, [FromBody] SendFamilyInviteEmailRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(new SendFamilyInviteEmailCommand(familyId, CurrentUserId, req.Email), ct));

        [HttpGet("{familyId:guid}/invites/status")]
        public async Task<IActionResult> GetInviteStatus(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new GetFamilyInviteStatusQuery(familyId, CurrentUserId), ct));

        [HttpPost("{familyId:guid}/children")]
        public async Task<IActionResult> AddChild(Guid familyId, [FromBody] AddChildRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(
                new AddChildCommand(familyId, CurrentUserId, req.Name, req.DateOfBirth), ct));

        [HttpDelete("{familyId:guid}/children/{childId:guid}")]
        public async Task<IActionResult> RemoveChild(Guid familyId, Guid childId, CancellationToken ct)
            => ToResponse(await sender.Send(
                new RemoveChildCommand(familyId, childId, CurrentUserId), ct));

        [HttpPut("{familyId:guid}/children/{childId:guid}/info")]
        public async Task<IActionResult> UpdateChildInfo(Guid familyId, Guid childId, [FromBody] UpdateChildInfoRequest req, CancellationToken ct)
            => ToResponse(await sender.Send(
                new UpdateChildInfoCommand(
                    familyId, childId, CurrentUserId,
                    req.Allergies, req.Medications, req.MedicalNotes,
                    req.DoctorName, req.DoctorPhone,
                    req.SchoolName, req.SchoolContact,
                    req.ClothingSize, req.ShoeSize,
                    req.EmergencyContactName, req.EmergencyContactPhone), ct));
    }

    public record AddChildRequest(string Name, DateOnly? DateOfBirth);
    public record JoinFamilyRequest(string Code);
    public record SendFamilyInviteEmailRequest(string Email);

    public record UpdateChildInfoRequest(
        string? Allergies,
        string? Medications,
        string? MedicalNotes,
        string? DoctorName,
        string? DoctorPhone,
        string? SchoolName,
        string? SchoolContact,
        string? ClothingSize,
        string? ShoeSize,
        string? EmergencyContactName,
        string? EmergencyContactPhone);
}
