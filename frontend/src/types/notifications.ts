export type NotificationType = "EventRsvp" | "EventCreated" | "EventCancelled";

export interface AppNotification {
  id: string;
  type: NotificationType;
  message: string;
  eventId: string | null;
  isRead: boolean;
  createdAt: string;
}
