using CoparentHub.Application.Features.DTOs;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;

namespace CoparentHub.Application.Features.Custody
{
    public class CreateCustodyScheduleHandler(IUnitOfWork uow)
        : IRequestHandler<CreateCustodyScheduleCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateCustodyScheduleCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<Guid>.Fail("Access denied.");

            if (!family.IsMember(cmd.ParentAUserId) || !family.IsMember(cmd.ParentBUserId))
                return Result<Guid>.Fail("Both parents must be members of this family.");

            var schedule = CustodySchedule.Create(
                cmd.FamilyId, cmd.StartDate, cmd.CycleLengthDays, cmd.DayPattern,
                cmd.ParentAUserId, cmd.ParentBUserId);

            await uow.ExecuteInTransactionAsync(async innerCt =>
            {
                await uow.CustodySchedules.DeactivateAllForFamilyAsync(cmd.FamilyId, innerCt);
                uow.CustodySchedules.Add(schedule);
                await uow.SaveAsync(innerCt);
            }, ct);

            var creator = await uow.Users.GetByIdAsync(cmd.UserId, ct);
            var creatorName = creator?.FullName ?? "Someone";

            foreach (var member in family.Members.Where(m => m.UserId != cmd.UserId))
            {
                uow.Notifications.Add(Notification.Create(
                    userId: member.UserId,
                    familyId: cmd.FamilyId,
                    type: NotificationType.CustodyScheduleUpdated,
                    message: $"{creatorName} set up a new custody schedule.",
                    eventId: null));
            }

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(schedule.Id);
        }
    }

    public class GetActiveCustodyScheduleHandler(IUnitOfWork uow)
        : IRequestHandler<GetActiveCustodyScheduleQuery, Result<CustodyScheduleDto>>
    {
        public async Task<Result<CustodyScheduleDto>> Handle(GetActiveCustodyScheduleQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<CustodyScheduleDto>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<CustodyScheduleDto>.Fail("Access denied.");

            var schedule = await uow.CustodySchedules.GetActiveByFamilyIdAsync(q.FamilyId, ct);

            if (schedule is null)
                return Result<CustodyScheduleDto>.Fail("No custody schedule set up yet.");

            var parentA = await uow.Users.GetByIdAsync(schedule.ParentAUserId, ct);
            var parentB = await uow.Users.GetByIdAsync(schedule.ParentBUserId, ct);

            return Result<CustodyScheduleDto>.Ok(new CustodyScheduleDto(
                schedule.Id, schedule.StartDate, schedule.CycleLengthDays, schedule.DayPattern,
                schedule.ParentAUserId, parentA?.FullName ?? "Unknown",
                schedule.ParentBUserId, parentB?.FullName ?? "Unknown"));
        }
    }

    public class GetCustodyForRangeHandler(IUnitOfWork uow)
        : IRequestHandler<GetCustodyForRangeQuery, Result<CustodyRangeDto>>
    {
        public async Task<Result<CustodyRangeDto>> Handle(GetCustodyForRangeQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<CustodyRangeDto>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<CustodyRangeDto>.Fail("Access denied.");

            var schedule = await uow.CustodySchedules.GetActiveByFamilyIdAsync(q.FamilyId, ct);

            if (schedule is null)
                return Result<CustodyRangeDto>.Fail("No custody schedule set up yet.");

            var parentA = await uow.Users.GetByIdAsync(schedule.ParentAUserId, ct);
            var parentB = await uow.Users.GetByIdAsync(schedule.ParentBUserId, ct);
            var parentAName = parentA?.FullName ?? "Unknown";
            var parentBName = parentB?.FullName ?? "Unknown";

            var days = new List<CustodyDayDto>();
            for (var date = q.From; date <= q.To; date = date.AddDays(1))
            {
                var dayIndex = (((date.DayNumber - schedule.StartDate.DayNumber) % schedule.CycleLengthDays)
                    + schedule.CycleLengthDays) % schedule.CycleLengthDays;

                var isParentA = schedule.DayPattern[dayIndex] == 'A';

                days.Add(new CustodyDayDto(
                    date,
                    date.DayOfWeek.ToString(),
                    isParentA ? schedule.ParentAUserId : schedule.ParentBUserId,
                    isParentA ? parentAName : parentBName));
            }

            return Result<CustodyRangeDto>.Ok(new CustodyRangeDto(q.From, q.To, days));
        }
    }
}
