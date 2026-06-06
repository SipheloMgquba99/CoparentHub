import type { AttendanceStatus, EventType } from "../types";

export const ini = (n: string): string =>
  (n || "").split(" ").map(w => w[0]).slice(0, 2).join("").toUpperCase();

export const fmtD = (s: string): string =>
  new Date(s).toLocaleDateString("en-US", { weekday: "short", month: "short", day: "numeric" });

export const fmtT = (s: string): string =>
  new Date(s).toLocaleTimeString("en-US", { hour: "numeric", minute: "2-digit" });

export const calcAge = (dob: string | null): number | null => {
  if (!dob) return null;
  const b = new Date(dob), n = new Date();
  let a = n.getFullYear() - b.getFullYear();
  if (n.getMonth() < b.getMonth() || (n.getMonth() === b.getMonth() && n.getDate() < b.getDate())) a--;
  return a;
};

export const toLocalDatetime = (iso: string): string => {
  const d = new Date(iso);
  const p = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
};

export const EVENT_TYPES: EventType[] = ["School", "Medical", "Activity", "Other"];

export const RSVP_STATUSES: AttendanceStatus[] = ["Accepted", "Tentative", "Declined"];

export const RSVP_CODE: Record<AttendanceStatus, string> = {
  Accepted: "A",
  Tentative: "T",
  Declined: "D",
};
