import type { AttendanceStatus, EventType, ExpenseCategory, DocumentCategory } from "../types";

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
  return a < 0 ? null : a;
};

export const toLocalDateString = (d: Date): string => {
  const p = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}`;
};

export const toLocalDatetime = (iso: string): string => {
  const d = new Date(iso);
  const p = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
};

export const EVENT_TYPES: EventType[] = ["School", "Medical", "Activity", "Other"];

export const timeAgo = (iso: string): string => {
  const diffMs = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diffMs / 60000);
  if (mins < 1) return "just now";
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(iso).toLocaleDateString("en-US", { month: "short", day: "numeric" });
};

export const RSVP_STATUSES: AttendanceStatus[] = ["Accepted", "Tentative", "Declined"];

export const RSVP_CODE: Record<AttendanceStatus, string> = {
  Accepted: "A",
  Tentative: "T",
  Declined: "D",
};

export const fmtZAR = (amount: number): string =>
  new Intl.NumberFormat("en-ZA", { style: "currency", currency: "ZAR" }).format(amount);

export const EXPENSE_CATEGORIES: ExpenseCategory[] = ["Medical", "School", "Clothing", "Activity", "Childcare", "Other"];

export const DOCUMENT_CATEGORIES: DocumentCategory[] = ["Legal", "Medical", "School", "Financial", "Other"];

export const fmtBytes = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
};
