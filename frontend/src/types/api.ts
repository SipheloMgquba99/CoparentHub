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
}

export interface AddChildRequest {
  name: string;
  dateOfBirth?: string | null; // "YYYY-MM-DD"
}

export interface CreateFamilyRequest {
  name: string;
}
