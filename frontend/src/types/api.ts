import type { EventType, AttendanceStatus } from "./events";

export interface CreateEventRequest {
  childId: string;
  title: string;
  type: EventType;
  startsAt: string;
  endsAt?: string | null;
  notes?: string | null;
}

export interface UpdateEventRequest {
  title: string;
  type: EventType;
  startsAt: string;
  endsAt?: string | null;
  notes?: string | null;
}

export interface RsvpRequest {
  status: AttendanceStatus;
  reason?: string | null;
}

