import { useState, useEffect, type FC } from "react";
import type { User, Family, CustodySchedule, CustodyRange } from "../../types";
import * as api from "../../api";
import { CustodySetupSheet } from "../../components/custody/CustodySetupSheet";
import { Ico, Icons } from "../../components/icons";
import { PageSpinner } from "../../components/ui";
import { toLocalDateString } from "../../lib/utils";

interface CustodyPageProps {
  user: User;
  family: Family | null;
  refresh: number;
  onEventsChanged: () => void;
}

const getMonday = (): string => {
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  d.setDate(d.getDate() - d.getDay());
  return toLocalDateString(d);
};

const addDays = (dateStr: string, n: number): string => {
  const d = new Date(dateStr + "T00:00:00");
  d.setDate(d.getDate() + n);
  return toLocalDateString(d);
};

const fmt = (s?: string) => {
  if (!s) return "...";
  const d = new Date(s + "T00:00:00");
  if (isNaN(d.getTime())) return "...";
  return d.toLocaleDateString("en-US", { month: "short", day: "numeric" });
};

const CustodyPage: FC<CustodyPageProps> = ({ user, family, refresh, onEventsChanged }) => {
  const [ws, setWs] = useState<string>(getMonday);
  const [schedule, setSchedule] = useState<CustodySchedule | null>(null);
  const [range, setRange] = useState<CustodyRange | null>(null);
  const [busy, setBusy] = useState(true);
  const [showSetup, setShowSetup] = useState(false);
  const [tick, setTick] = useState(0);

  useEffect(() => {
    if (!family) { setBusy(false); setSchedule(null); return; }
    setBusy(true);
    api.getActiveCustodySchedule(family.id)
      .then(setSchedule)
      .catch(() => setSchedule(null))
      .finally(() => setBusy(false));
  }, [family?.id, refresh, tick]);

  useEffect(() => {
    if (!family || !schedule) { setRange(null); return; }
    const weekEnd = addDays(ws, 6);
    api.getCustodyForRange(family.id, ws, weekEnd)
      .then(setRange)
      .catch(() => setRange(null));
  }, [family?.id, schedule?.id, ws]);

  const shift = (n: number) => setWs(addDays(ws, n * 7));
  const jumpToToday = () => setWs(getMonday());

  const today = toLocalDateString(new Date());
  const isCurrentWeek = ws === getMonday();

  const parentColor = (parentUserId: string): string =>
    schedule && parentUserId === schedule.parentAUserId ? "var(--accent)" : "var(--gold)";

  return (
    <>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 16 }}>
        <div className="page-title" style={{ marginBottom: 0 }}>Custody Schedule</div>
        {family && schedule && (
          <button type="button" className="btn btn-o btn-sm" onClick={() => setShowSetup(true)}>
            Change
          </button>
        )}
      </div>

      {!family ? (
        <div className="empty" style={{ paddingTop: 40 }}>Join or create a family to set up a custody schedule.</div>
      ) : busy ? (
        <PageSpinner />
      ) : !schedule ? (
        <div style={{ textAlign: "center", padding: "44px 16px" }}>
          <div style={{ fontSize: 44, marginBottom: 14 }}>🔄</div>
          <div style={{ fontFamily: "'Fraunces',serif", fontSize: 20, color: "var(--text)", marginBottom: 8 }}>
            No custody schedule yet
          </div>
          <div style={{ fontSize: 14, color: "var(--muted)", marginBottom: 24, lineHeight: 1.6 }}>
            Set up a recurring pattern so everyone knows<br />whose day it is.
          </div>
          <button className="btn btn-p" style={{ width: "auto", padding: "11px 24px" }} onClick={() => setShowSetup(true)}>
            Set Up Schedule
          </button>
        </div>
      ) : (
        <>
          <div className="wnav">
            <button type="button" className="wbtn" onClick={() => shift(-1)} aria-label="Previous week"><Ico d={Icons.chL} size={16} /></button>
            <div className="wlbl">{range ? `${fmt(range.from)} – ${fmt(range.to)}` : "..."}</div>
            <button type="button" className="wbtn" onClick={() => shift(1)} aria-label="Next week"><Ico d={Icons.chR} size={16} /></button>
          </div>

          {!isCurrentWeek && (
            <button
              type="button"
              className="btn btn-o btn-sm"
              onClick={jumpToToday}
              style={{ width: "auto", margin: "-8px auto 16px", display: "block", padding: "6px 16px" }}
            >
              Jump to this week
            </button>
          )}

          {!range ? (
            <PageSpinner />
          ) : (
            range.days.map(day => (
              <div key={day.date} className={`drow ${day.date === today ? "dtoday" : ""}`}>
                <div className="dlbl">
                  {day.dayName.slice(0, 3)} {new Date(day.date + "T00:00:00").getDate()}
                  {day.date === today && (
                    <span style={{ fontSize: 10, color: "var(--gold)", fontWeight: 700 }}>Today</span>
                  )}
                </div>
                <div style={{ padding: "7px 0 7px 18px", fontSize: 13.5, display: "flex", alignItems: "center", gap: 8 }}>
                  <span style={{ width: 8, height: 8, borderRadius: "50%", background: parentColor(day.parentUserId), flexShrink: 0 }} />
                  {day.parentName}
                  {day.parentUserId === user.id && (
                    <span style={{ fontSize: 11, color: "var(--muted)" }}>(You)</span>
                  )}
                </div>
              </div>
            ))
          )}
        </>
      )}

      {showSetup && family && (
        <CustodySetupSheet
          family={family}
          currentUserId={user.id}
          onSubmit={async data => {
            await api.createCustodySchedule(family.id, data);
            setShowSetup(false);
            setTick(t => t + 1);
            onEventsChanged();
          }}
          onClose={() => setShowSetup(false)}
        />
      )}
    </>
  );
};

export default CustodyPage;
