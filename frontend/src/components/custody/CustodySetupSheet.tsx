import { useState, type FC, type FormEvent } from "react";
import type { Family, CreateCustodyScheduleRequest } from "../../types";
import { Spinner } from "../ui";
import { Ico, Icons } from "../icons";
import { toLocalDateString } from "../../lib/utils";
import { CUSTODY_PRESETS, patternForPreset, type CustodyPreset } from "../../lib/custodyPresets";

interface CustodySetupSheetProps {
  family: Family;
  currentUserId: string;
  onSubmit: (data: CreateCustodyScheduleRequest) => Promise<void>;
  onClose: () => void;
}

export const CustodySetupSheet: FC<CustodySetupSheetProps> = ({ family, currentUserId, onSubmit, onClose }) => {
  const otherMember = family.members.find(m => m.userId !== currentUserId);

  const [preset, setPreset] = useState<CustodyPreset>("weekOnWeekOff");
  const [startDate, setStartDate] = useState(toLocalDateString(new Date()));
  const [cycleLengthDays, setCycleLengthDays] = useState(14);
  const [dayPattern, setDayPattern] = useState("A".repeat(7) + "B".repeat(7));
  const [parentAUserId, setParentAUserId] = useState(currentUserId);
  const [parentBUserId, setParentBUserId] = useState(otherMember?.userId ?? "");
  const [err, setErr] = useState("");
  const [busy, setBusy] = useState(false);

  const handlePresetChange = (next: CustodyPreset) => {
    setPreset(next);
    const pattern = patternForPreset(next);
    if (pattern) {
      setCycleLengthDays(pattern.cycleLengthDays);
      setDayPattern(pattern.dayPattern);
    }
  };

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setErr("");

    if (!parentAUserId || !parentBUserId) { setErr("Please select both parents."); return; }
    if (parentAUserId === parentBUserId) { setErr("Parents must be different people."); return; }

    const pattern = dayPattern.trim().toUpperCase();
    if (pattern.length !== cycleLengthDays) { setErr("Pattern length must match the cycle length."); return; }
    if (!/^[AB]+$/.test(pattern)) { setErr("Pattern can only contain 'A' or 'B'."); return; }

    setBusy(true);
    try {
      await onSubmit({
        startDate,
        cycleLengthDays,
        dayPattern: pattern,
        parentAUserId,
        parentBUserId,
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
          <div className="shtitle">Set Up Custody Schedule</div>
          <button type="button" className="shclose" onClick={onClose} aria-label="Close">
            <Ico d={Icons.x} size={15} />
          </button>
        </div>
        {err && <div className="err">{err}</div>}
        <form onSubmit={submit}>
          <div className="f" style={{ marginBottom: 14 }}>
            <label>Pattern</label>
            <div className="chips" style={{ marginTop: 5 }}>
              {CUSTODY_PRESETS.map(p => (
                <button type="button" key={p.id} className={`chip ${preset === p.id ? "on" : ""}`} onClick={() => handlePresetChange(p.id)}>
                  {p.label}
                </button>
              ))}
            </div>
          </div>

          {preset === "custom" && (
            <div className="frow">
              <div className="f">
                <label>Cycle Length (days)</label>
                <input type="number" min="1" max="90" value={cycleLengthDays} onChange={e => setCycleLengthDays(Number(e.target.value))} required />
              </div>
              <div className="f">
                <label>Pattern (A/B)</label>
                <input value={dayPattern} onChange={e => setDayPattern(e.target.value.toUpperCase())} placeholder="AABBAABB" maxLength={90} required />
              </div>
            </div>
          )}

          <div className="f">
            <label>Start Date</label>
            <input type="date" value={startDate} onChange={e => setStartDate(e.target.value)} required />
          </div>

          <div className="frow">
            <div className="f">
              <label>Parent A</label>
              <select value={parentAUserId} onChange={e => setParentAUserId(e.target.value)} required>
                {family.members.map(m => <option key={m.userId} value={m.userId}>{m.fullName}</option>)}
              </select>
            </div>
            <div className="f">
              <label>Parent B</label>
              <select value={parentBUserId} onChange={e => setParentBUserId(e.target.value)} required>
                <option value="" disabled>-- Select --</option>
                {family.members.map(m => <option key={m.userId} value={m.userId}>{m.fullName}</option>)}
              </select>
            </div>
          </div>

          <button className="btn btn-p" type="submit" disabled={busy} style={{ marginTop: 12 }}>
            {busy ? <Spinner /> : "Save Schedule"}
          </button>
        </form>
      </div>
    </div>
  );
};
