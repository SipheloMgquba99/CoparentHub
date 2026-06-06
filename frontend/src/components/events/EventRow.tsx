import type { FC } from "react";
import type { ScheduledEvent, AttendanceStatus } from "../../types";
import { Ico, Icons } from '../icons';
import { fmtD, fmtT, RSVP_STATUSES, RSVP_CODE } from '../../lib/utils';

interface EventRowProps {
  ev: ScheduledEvent;
  userId: string;
  onEdit?:   (ev: ScheduledEvent) => void;
  onCancel?: (id: string) => void;
  onRsvp?:   (id: string, status: AttendanceStatus) => void;
  compact?: boolean;
}

export const EventRow: FC<EventRowProps> = ({ ev, userId, onEdit, onCancel, onRsvp, compact }) => {
  const tc  = (ev.type ?? "Other").toLowerCase();
  const my  = ev.attendances.find(a => a.userId === userId);
  const oth = ev.attendances.filter(a => a.userId !== userId);

  // In compact (home) mode: only show RSVP buttons when status is Tentative or no response yet.
  // In full (schedule) mode: always show RSVP buttons so user can change their mind.
  const needsResponse = !my || my.status === "Tentative";
  const showRsvp = onRsvp && !ev.isCancelled && (compact ? needsResponse : true);

  return (
    <div className={`ei ${ev.isCancelled ? "ecanc" : ""}`}>
      <div className={`dot ${tc}`} style={{ marginTop: 5 }} />
      <div className="einfo">
        <div className="etitle">
          {ev.title}
          {ev.isCancelled && <span className="bcx">Cancelled</span>}
          {/* Show a small pill on home when already accepted */}
          {compact && my?.status === "Accepted" && !ev.isCancelled && (
            <span style={{
              display: "inline-block",
              marginLeft: 6,
              fontSize: 10,
              fontWeight: 700,
              background: "#e8f5e9",
              color: "#15803d",
              padding: "1px 7px",
              borderRadius: 20,
              verticalAlign: "middle",
            }}>✓ Going</span>
          )}
          {compact && my?.status === "Declined" && !ev.isCancelled && (
            <span style={{
              display: "inline-block",
              marginLeft: 6,
              fontSize: 10,
              fontWeight: 700,
              background: "var(--danger-bg)",
              color: "var(--danger)",
              padding: "1px 7px",
              borderRadius: 20,
              verticalAlign: "middle",
            }}>✗ Declined</span>
          )}
        </div>

        <div className="emeta">
          {fmtD(ev.startsAt)} · {fmtT(ev.startsAt)}{ev.endsAt ? `–${fmtT(ev.endsAt)}` : ""}
        </div>

        <span className="echild"><Ico d={Icons.kid} size={10} />{ev.childName}</span>

        {ev.notes && !compact && <div className="enotes">"{ev.notes}"</div>}

        {oth.length > 0 && !compact && (
          <div className="atts">
            {oth.map(a => (
              <span key={a.userId} className={`ac ac${RSVP_CODE[a.status]}`}>
                {a.status === "Accepted" ? "✓" : a.status === "Declined" ? "✗" : "~"}&nbsp;Co-parent
              </span>
            ))}
          </div>
        )}

        {/* Tentative notice on home screen */}
        {compact && my?.status === "Tentative" && !ev.isCancelled && (
          <div style={{ fontSize: 11, color: "#92400e", background: "#fff7e3", padding: "3px 8px", borderRadius: 6, marginTop: 5, display: "inline-block", fontWeight: 600 }}>
            ⏳ Tentative — confirm below
          </div>
        )}

        {showRsvp && (
          <div className="rsvps">
            {RSVP_STATUSES.map(s => (
              <button
                type="button"
                key={s}
                className={`rb ${my?.status === s ? `r${RSVP_CODE[s]}` : ""}`}
                onClick={() => onRsvp!(ev.id, s)}
              >
                {s}
              </button>
            ))}
          </div>
        )}
      </div>

      {!ev.isCancelled && (onEdit || onCancel) && ev.createdByUserId === userId && (
        <div style={{ display: "flex", flexDirection: "column", gap: 2 }}>
          {onEdit && (
            <button type="button" className="btn btn-gh btn-sm" style={{ padding: 6 }} onClick={() => onEdit(ev)} title="Edit">
              <Ico d={Icons.edit} size={14} />
            </button>
          )}
          {onCancel && (
            <button type="button" className="btn btn-gh btn-sm" style={{ padding: 6, color: "var(--danger)" }} onClick={() => onCancel(ev.id)} title="Cancel">
              <Ico d={Icons.x} size={14} />
            </button>
          )}
        </div>
      )}
    </div>
  );
};