import { useState, useEffect, useRef, type FC } from "react";
import type { User, Family, ScheduledEvent } from "../../types";
import * as api from "../../api";
import { EventRow, EventSheet } from "../../components/events";
import { Ico, Icons } from "../../components/icons";
import { PageSpinner } from "../../components/ui";
import { calcAge, toLocalDateString } from "../../lib/utils";

const HOME_POLL_MS = 20_000;

const getGreeting = (): string => {
  const hour = new Date().getHours();
  if (hour < 12) return "Good morning";
  if (hour < 18) return "Good afternoon";
  return "Good evening";
};

interface HomePageProps {
  user: User;
  family: Family | null;
  setTab: (t: string) => void;
  refresh: number;
  onEventsChanged: () => void;
}

const HomePage: FC<HomePageProps> = ({ user, family, setTab, refresh, onEventsChanged }) => {
  const [evs, setEvs] = useState<ScheduledEvent[]>([]);
  const [busy, setBusy] = useState<boolean>(true);
  const [sheet, setSheet] = useState<boolean>(false);
  const [tick, setTick] = useState<number>(0);
  const silentRef = useRef(false);

  const today = toLocalDateString(new Date());

  useEffect(() => {
    if (!family) {
      setEvs([]);
      setBusy(false);
      return;
    }

    if (!silentRef.current) setBusy(true);

    const from = new Date().toISOString();
    const toDate = new Date();
    toDate.setDate(toDate.getDate() + 7);
    const to = toDate.toISOString();

    api.getEvents(family.id, { from, to })
      .then(data =>
        setEvs(
          data.sort(
            (a, b) =>
              new Date(a.startsAt).getTime() -
              new Date(b.startsAt).getTime()
          )
        )
      )
      .catch(() => setEvs([]))
      .finally(() => {
        setBusy(false);
        silentRef.current = false;
      });
  }, [family?.id, refresh, tick]);

  useEffect(() => {
    if (!family) return;
    const id = setInterval(() => {
      silentRef.current = true;
      setTick(t => t + 1);
    }, HOME_POLL_MS);
    return () => clearInterval(id);
  }, [family?.id]);

  const fn = user.fullName.split(" ")[0] ?? "there";
  const day = new Date().toLocaleDateString("en-US", { weekday: "long" });
  const date = new Date().toLocaleDateString("en-US", { month: "long", day: "numeric" });
  const todayCount = evs.filter(e => toLocalDateString(new Date(e.startsAt)) === today).length;

  return (
    <>
      <div className="greet">
        <div className="gname">
          {getGreeting()},<br />
          <em>{fn}</em> 👋
        </div>
        <div className="gdate">{day}, {date}</div>
      </div>

      <div className="stats">
        <div className="sc">
          <div className="sv">{todayCount}</div>
          <div className="sl">Events today</div>
        </div>
        <div className="sc">
          <div className="sv">{family?.children.length ?? 0}</div>
          <div className="sl">Children</div>
        </div>
      </div>

      <div className="card">
        <div className="ch">
          <div className="ct">Upcoming Events</div>
          {family && (
            <button
              className="btn btn-p btn-sm"
              onClick={() => setSheet(true)}
              style={{ gap: 4 }}
            >
              <Ico d={Icons.plus} size={13} />Add
            </button>
          )}
        </div>

        {busy ? (
          <PageSpinner />
        ) : evs.length === 0 ? (
          <div className="empty">
            {family
              ? "No events in the next 7 days"
              : "Join or create a family to get started"}
          </div>
        ) : (
          evs.map(e => (
            <EventRow key={e.id} ev={e} userId={user.id} compact />
          ))
        )}

        {evs.length > 0 && (
          <button
            onClick={() => setTab("sched")}
            style={{
              display: "flex",
              justifyContent: "flex-end",
              width: "100%",
              marginTop: 8,
              background: "none",
              border: "none",
              cursor: "pointer",
              color: "var(--accent)",
              fontSize: 13,
              fontFamily: "Outfit,sans-serif",
              fontWeight: 600,
            }}
          >
            View full schedule →
          </button>
        )}
      </div>

      {family && (
        <>
          <div className="card">
            <div className="ch">
              <div className="ct">Children</div>
              <button
                className="btn btn-gh btn-sm"
                onClick={() => setTab("fam")}
                style={{
                  color: "var(--accent)",
                  fontSize: 12,
                  fontWeight: 600,
                }}
              >
                Manage →
              </button>
            </div>

            {family.children.length === 0 && (
              <div className="empty">No children added yet</div>
            )}

            {family.children.map(c => (
              <div key={c.id} className="mi">
                <div
                  className="av"
                  style={{
                    background: "var(--echild-bg)",
                    color: "var(--echild-text)",
                  }}
                >
                  <Ico d={Icons.kid} size={16} stroke="var(--echild-text)" />
                </div>
                <div style={{ flex: 1 }}>
                  <div className="mn">{c.name}</div>
                  <div className="me">
                    {c.dateOfBirth
                      ? new Date(c.dateOfBirth + "T00:00:00").toLocaleDateString(
                          "en-US",
                          {
                            month: "short",
                            day: "numeric",
                            year: "numeric",
                          }
                        )
                      : "No DOB set"}
                  </div>
                </div>
                {calcAge(c.dateOfBirth) !== null && (
                  <span className="cage">
                    {calcAge(c.dateOfBirth)}y
                  </span>
                )}
              </div>
            ))}
          </div>

          <div className="card">
            <div className="ct" style={{ marginBottom: 12 }}>
              Co-parents
            </div>

            {family.members.map(m => (
              <div key={m.userId} className="mi">
                <div className="av">
                  {m.fullName
                    .split(" ")
                    .map(w => w[0])
                    .slice(0, 2)
                    .join("")
                    .toUpperCase()}
                </div>
                <div>
                  <div className="mn">
                    {m.fullName}
                    {m.userId === user.id && (
                      <span
                        style={{
                          fontSize: 11,
                          color: "var(--gold)",
                          fontWeight: 700,
                          marginLeft: 5,
                        }}
                      >
                        · You
                      </span>
                    )}
                  </div>
                  <div className="me">{m.email}</div>
                </div>
              </div>
            ))}
          </div>
        </>
      )}

      {sheet && family && (
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
            setSheet(false);
            onEventsChanged();
          }}
          onClose={() => setSheet(false)}
        />
      )}
    </>
  );
};

export default HomePage;