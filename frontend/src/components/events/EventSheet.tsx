import { useState, type FC, type FormEvent } from "react";
import type { ScheduledEvent, EventType, Child } from "../../types";
import { Spinner } from '../ui';
import { Ico, Icons } from '../icons';
import { EVENT_TYPES, toLocalDatetime } from '../../lib/utils';

export interface EventSheetData {
  childId: string;
  title: string;
  type: EventType;
  startsAt: string;
  endsAt: string;
  notes: string;
}

interface EventSheetProps {
  children: Child[];
  onSubmit: (data: EventSheetData) => Promise<void>;
  onClose: () => void;
  ev?: ScheduledEvent;
  title?: string;
}

export const EventSheet: FC<EventSheetProps> = ({ children, onSubmit, onClose, ev, title = "New Event" }) => {
  const editing = !!ev;
  const [cid,  setCid]  = useState(ev?.childId ?? children[0]?.id ?? "");
  const [titl, setTitl] = useState(ev?.title ?? "");
  const [type, setType] = useState<EventType>(ev?.type ?? "School");
  const [sa,   setSa]   = useState(ev?.startsAt ? toLocalDatetime(ev.startsAt) : "");
  const [ea,   setEa]   = useState(ev?.endsAt   ? toLocalDatetime(ev.endsAt)   : "");
  const [note, setNote] = useState(ev?.notes ?? "");
  const [err,  setErr]  = useState("");
  const [busy, setBusy] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setErr("");
    setBusy(true);
    try {
      await onSubmit({
        childId:  cid,
        title:    titl,
        type,
        startsAt: new Date(sa).toISOString(),
        endsAt:   ea ? new Date(ea).toISOString() : "",
        notes:    note,
      });
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Something went wrong.");
    }
    setBusy(false);
  };

  return (
    <div className="ov" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="sh">
        <div className="shdrag" />
        <div className="shhead">
          <div className="shtitle">{editing ? "Edit Event" : title}</div>
          <button type="button" className="shclose" onClick={onClose} aria-label="Close">
            <Ico d={Icons.x} size={15} />
          </button>
        </div>
        {err && <div className="err">{err}</div>}
        <form onSubmit={submit}>

          <div className="f">
            <label>For Child</label>
            <select value={cid} onChange={e => setCid(e.target.value)}>
              {children.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>

          <div className="f">
            <label>Title</label>
            <input value={titl} onChange={e => setTitl(e.target.value)} placeholder="Doctor appointment…" maxLength={200} required />
          </div>

          <div className="f" style={{ marginBottom: 14 }}>
            <label>Type</label>
            <div className="chips" style={{ marginTop: 5 }}>
              {EVENT_TYPES.map(t => (
                <button type="button" key={t} className={`chip ${type === t ? "on" : ""}`} onClick={() => setType(t)}>
                  {t}
                </button>
              ))}
            </div>
          </div>
          <div className="f">
            <label>Starts</label>
            <input type="datetime-local" value={sa} onChange={e => setSa(e.target.value)} required />
          </div>

          <div className="f">
            <label>Ends <span style={{ fontWeight: 400, textTransform: "none", letterSpacing: 0, color: "var(--muted)", fontSize: 11 }}>(optional)</span></label>
            <input type="datetime-local" value={ea} onChange={e => setEa(e.target.value)} />
          </div>

          <div className="f">
            <label>Notes</label>
            <input value={note} onChange={e => setNote(e.target.value)} placeholder="Any details…" maxLength={1000} />
          </div>

          <button className="btn btn-p" type="submit" disabled={busy} style={{ marginTop: 12 }}>
            {busy ? <Spinner /> : editing ? "Save Changes" : "Create Event"}
          </button>

        </form>
      </div>
    </div>
  );
};