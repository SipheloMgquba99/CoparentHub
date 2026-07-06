import type {
  ScheduledEvent,
  WeeklySchedule,
  CreateEventRequest,
  UpdateEventRequest,
  RsvpRequest,
} from  '../types';
import { request } from './http';

export function getEvents(
  familyId: string,
  params?: { from?: string; to?: string; childId?: string },
): Promise<ScheduledEvent[]> {
  const qs = new URLSearchParams();
  if (params?.from)    qs.set("from",    params.from);
  if (params?.to)      qs.set("to",      params.to);
  if (params?.childId) qs.set("childId", params.childId);
  const query = qs.toString() ? `?${qs}` : "";
  return request<ScheduledEvent[]>("GET", `/families/${familyId}/events${query}`);
}

export function getWeekly(familyId: string, weekStart: string): Promise<WeeklySchedule> {
  return request<WeeklySchedule>("GET", `/families/${familyId}/events/weekly?weekStart=${weekStart}`);
}

export function createEvent(familyId: string, req: CreateEventRequest): Promise<string> {
  return request<string>("POST", `/families/${familyId}/events`, req);
}

export function updateEvent(familyId: string, eventId: string, req: UpdateEventRequest): Promise<string> {
  return request<string>("PUT", `/families/${familyId}/events/${eventId}`, req);
}

export function cancelEvent(familyId: string, eventId: string): Promise<string> {
  return request<string>("DELETE", `/families/${familyId}/events/${eventId}`);
}

export function rsvp(familyId: string, eventId: string, req: RsvpRequest): Promise<string> {
  return request<string>("POST", `/families/${familyId}/events/${eventId}/rsvp`, req);
}
