using CoparentHub.Application.Features.DTOs;
using CoparentHub.Application.Interfaces.Repositories;
using CoparentHub.Domain.Common;
using CoparentHub.Domain.Entities;
using MediatR;

namespace CoparentHub.Application.Features.Events
{
    public class CreateEventHandler(IUnitOfWork uow)
    : IRequestHandler<CreateEventCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateEventCommand cmd, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(cmd.FamilyId, ct);

            if (family is null)
                return Result<Guid>.Fail("Family not found.");

            if (!family.IsMember(cmd.UserId))
                return Result<Guid>.Fail("Access denied.");

            if (!family.HasChild(cmd.ChildId))
                return Result<Guid>.Fail("Child not found in this family.");

            var ev = ScheduledEvent.Create(
                cmd.FamilyId,
                cmd.ChildId,
                cmd.UserId,
                cmd.Title,
                cmd.Type,
                cmd.StartsAt,
                cmd.EndsAt,
                cmd.Notes,
                family.Members.Select(m => m.UserId)
            );

            uow.Events.Add(ev);

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(ev.Id);
        }
    }

    public class UpdateEventHandler(IUnitOfWork uow)
        : IRequestHandler<UpdateEventCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(UpdateEventCommand cmd, CancellationToken ct)
        {
            var ev = await uow.Events.GetByIdAsync(cmd.EventId, ct);

            if (ev is null)
                return Result<Guid>.Fail("Event not found.");

            if (ev.CreatedByUserId != cmd.UserId)
                return Result<Guid>.Fail("Only the creator can edit this event.");

            var result = ev.Update(cmd.Title, cmd.Type, cmd.StartsAt, cmd.EndsAt, cmd.Notes);

            if (!result.IsSuccess)
                return Result<Guid>.Fail(result.Error!);

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(ev.Id);
        }
    }

    public class CancelEventHandler(IUnitOfWork uow)
        : IRequestHandler<CancelEventCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CancelEventCommand cmd, CancellationToken ct)
        {
            var ev = await uow.Events.GetByIdAsync(cmd.EventId, ct);

            if (ev is null)
                return Result<Guid>.Fail("Event not found.");

            if (ev.CreatedByUserId != cmd.UserId)
                return Result<Guid>.Fail("Only the creator can cancel this event.");

            var result = ev.Cancel();

            if (!result.IsSuccess)
                return Result<Guid>.Fail(result.Error!);

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(ev.Id);
        }
    }

    public class RsvpHandler(IUnitOfWork uow)
        : IRequestHandler<RsvpCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RsvpCommand cmd, CancellationToken ct)
        {
            var ev = await uow.Events.GetByIdAsync(cmd.EventId, ct);

            if (ev is null)
                return Result<Guid>.Fail("Event not found.");

            var result = ev.Rsvp(cmd.UserId, cmd.Status);

            if (!result.IsSuccess)
                return Result<Guid>.Fail(result.Error!);

            await uow.SaveAsync(ct);

            return Result<Guid>.Ok(ev.Id);
        }
    }

    public class GetEventsHandler(IUnitOfWork uow)
     : IRequestHandler<GetEventsQuery, Result<List<EventDto>>>
    {
        public async Task<Result<List<EventDto>>> Handle(GetEventsQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<List<EventDto>>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<List<EventDto>>.Fail("Access denied.");

            DateOnly? from = q.From.HasValue ? DateOnly.FromDateTime(q.From.Value) : null;
            DateOnly? to = q.To.HasValue ? DateOnly.FromDateTime(q.To.Value) : null;

            var events = await uow.Events.GetByFamilyAsync(q.FamilyId, from, to, q.ChildId, ct);

            return Result<List<EventDto>>.Ok(
                events.Select(e => EventMapper.ToDto(e, family)).ToList()
            );
        }
    }

    public class GetWeeklyHandler(IUnitOfWork uow)
        : IRequestHandler<GetWeeklyQuery, Result<WeekDto>>
    {
        public async Task<Result<WeekDto>> Handle(GetWeeklyQuery q, CancellationToken ct)
        {
            var family = await uow.Families.GetByIdAsync(q.FamilyId, ct);

            if (family is null)
                return Result<WeekDto>.Fail("Family not found.");

            if (!family.IsMember(q.UserId))
                return Result<WeekDto>.Fail("Access denied.");

            var events = await uow.Events.GetWeekAsync(q.FamilyId, q.WeekStart, ct);

            var days = Enumerable.Range(0, 7)
                .Select(i => q.WeekStart.AddDays(i))
                .Select(date => new DayDto(
                    date,
                    date.DayOfWeek.ToString(),
                    events.Where(e => DateOnly.FromDateTime(e.StartsAt) == date)
                          .Select(e => EventMapper.ToDto(e, family))
                          .ToList()
                ))
                .ToList();

            return Result<WeekDto>.Ok(
                new WeekDto(q.WeekStart, q.WeekStart.AddDays(6), days)
            );
        }
    }

    public static class EventMapper
    {
        public static EventDto ToDto(ScheduledEvent e, CoparentHub.Domain.Entities.Family family) =>
            new(
                e.Id,
                e.FamilyId,
                e.ChildId,
                family.Children.FirstOrDefault(c => c.Id == e.ChildId)?.Name ?? "Unknown",
                e.Title,
                e.Type.ToString(),
                e.StartsAt,
                e.EndsAt,
                e.Notes,
                e.IsCancelled,
                e.Attendances
                    .Select(a => new AttendanceDto(a.UserId, a.Status.ToString(), a.RespondedAt))
                    .ToList()
            );
    }
}
