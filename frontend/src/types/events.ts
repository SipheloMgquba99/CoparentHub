export type EventType = "School" | "Medical" | "Activity" | "Other";

export type AttendanceStatus = "Accepted" | "Tentative" | "Declined";

export interface Attendance {
  userId: string;
  status: AttendanceStatus;
  reason: string | null;
}

export interface ScheduledEvent {
  id: string;
  familyId: string;
  childId: string;
  childName: string;
  title: string;
  type: EventType;
  startsAt: string; 
  endsAt: string | null;
  notes: string | null;
  isCancelled: boolean;
  createdByUserId: string;
  attendances: Attendance[];
}

export interface WeekDay {
  date: string; 
  dayName: string;
  events: ScheduledEvent[];
}

export interface WeeklySchedule {
  weekStart: string;
  weekEnd: string;
  days: WeekDay[];
}