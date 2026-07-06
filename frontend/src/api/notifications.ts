import type { AppNotification } from "../types";
import { request } from "./http";

export function getNotifications(): Promise<AppNotification[]> {
  return request<AppNotification[]>("GET", "/notifications");
}
export function markNotificationRead(notificationId: string): Promise<string> {
  return request<string>("POST", `/notifications/${notificationId}/read`);
}
