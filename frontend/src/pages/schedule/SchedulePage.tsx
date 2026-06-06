import { useState, useEffect, type FC } from "react";
import type { User, Family, ScheduledEvent, WeeklySchedule, AttendanceStatus } from "../../types";
import * as api from "../../api";
import { EventSheet, EventRow } from "../../components/events";
import { Ico, Icons } from "../../components/icons";
import { PageSpinner } from "../../components/ui";

interface SchedPageProps {
  user: User;
  family: Family | null;
  refresh: number;
  onEventsChanged: () => void;
}

const getMonday = (): string => {
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  d.setDate(d.getDate() - d.getDay());
  return d.toISOString().slice(0, 10);
};

// Safe date formatter
const fmt = (s?: string) => {
  if (!s) return "...";
  const d = new Date(s + "T00:00:00");
  if (isNaN(d.getTime())) return "...";
  return d.toLocaleDateString("en-US", { month: "short", day: "numeric" });
};

const SchedPage: FC<SchedPageProps> = ({ user, family, refresh, onEventsChanged }) => {
  const [ws, setWs] = useState<string>(getMonday);
  const [week, setWeek] = useState<WeeklySchedule | null>(null);
  const [busy, setBusy] = useState<boolean>(true);
  const [edit, setEdit] = useState<ScheduledEvent | null>(null);
  const [add, setAdd] = useState<boolean>(false);
  const [tick, setTick] = useState<number>(0);

  useEffect(() => {
    if (!family) { setBusy(false); return; }
    setBusy(true);
    api.getWeekly(family.id, ws)
      .then(data => setWeek(data))
      .catch(() => setWeek(null))
      .finally(() => setBusy(false));
  }, [ws, tick, refresh, family?.id]);

  const shift = (n: number) => {
    const d = new Date(ws + "T00:00:00");
    d.setDate(d.getDate() + n * 7);
    setWs(d.toISOString().slice(0, 10));
  };

  const handleCancel = async (eventId: string) => {
    if (!family) return;
    await api.cancelEvent(family.id, eventId);
    setTick(t => t + 1);
    onEventsChanged();
  };

  const handleRsvp = async (eventId: string, status: AttendanceStatus) => {
    if (!family) return;
    await api.rsvp(family.id, eventId, { status });
    setTick(t => t + 1);
  };

  const done = () => {
    setEdit(null);
    setAdd(false);
    setTick(t => t + 1);
    onEventsChanged();
  };

  const today = new Date().toISOString().slice(0, 10);

  return (
    <>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 16 }}>
        <div className="page-title" style={{ marginBottom: 0 }}>Schedule</div>
        {family && (
          <button type="button" className="btn btn-p btn-sm" onClick={() => setAdd(true)} style={{ gap: 4 }}>
            <Ico d={Icons.plus} size={13} />Add
          </button>
        )}
      </div>

      <div className="wnav">
        <button type="button" className="wbtn" onClick={() => shift(-1)}><Ico d={Icons.chL} size={16} /></button>
        <div className="wlbl">{week ? `${fmt(week.from)} – ${fmt(week.to)}` : "..."}</div>
        <button type="button" className="wbtn" onClick={() => shift(1)}><Ico d={Icons.chR} size={16} /></button>
      </div>

      {!family
        ? <div className="empty" style={{ paddingTop: 40 }}>Join or create a family to see events.</div>
        : busy
          ? <PageSpinner />
          : week?.days.map(day => (
            <div key={day.date} className={`drow ${day.date === today ? "dtoday" : ""}`}>
              <div className="dlbl">
                {day.dayName.slice(0, 3)} {new Date(day.date + "T00:00:00").getDate()}
                {day.date === today && (
                  <span style={{ fontSize: 10, color: "var(--gold)", fontWeight: 700 }}>Today</span>
                )}
              </div>
              {day.events.length === 0
                ? <div className="dfree">Free</div>
                : day.events.map(e => (
                  <EventRow
                    key={e.id}
                    ev={e}
                    userId={user.id}
                    onEdit={setEdit}
                    onCancel={handleCancel}
                    onRsvp={handleRsvp}
                  />
                ))
              }
            </div>
          ))
      }

      {add && family && (
        <EventSheet
          children={family.children}
          onSubmit={async data => {
            await api.createEvent(family.id, {
              childId: data.childId,
              title: data.title,
              type: data.type,
              startsAt: data.startsAt,
              endsAt: data.endsAt || undefined,
              notes: data.notes || undefined,
            });
            done();
          }}
          onClose={() => setAdd(false)}
        />
      )}

      {edit && family && (
        <EventSheet
          ev={edit}
          children={family.children}
          onSubmit={async data => {
            await api.updateEvent(family.id, edit.id, {
              title: data.title,
              type: data.type,
              startsAt: data.startsAt,
              endsAt: data.endsAt || undefined,
              notes: data.notes || undefined,
            });
            done();
          }}
          onClose={() => setEdit(null)}
        />
      )}
    </>
  );
};

export default SchedPage;