import { request } from "./http";

export interface PushSubscriptionKeys {
  p256dh: string;
  auth: string;
}

export interface SubscribePushPayload {
  endpoint: string;
  keys: PushSubscriptionKeys;
}

export interface SendAnnouncementPayload {
  title: string;
  body: string;
  url?: string;
}

export function getVapidPublicKey(): Promise<string> {
  return request<string>("GET", "/push/vapid-public-key");
}

export function subscribePush(subscription: SubscribePushPayload): Promise<string> {
  if (!subscription?.endpoint) {
    return Promise.reject(new Error("endpoint is required"));
  }

  return request<string>("POST", "/push/subscribe", subscription);
}

export function unsubscribePush(endpoint: string): Promise<boolean> {
  if (!endpoint) {
    return Promise.reject(new Error("endpoint is required"));
  }

  return request<boolean>("POST", "/push/unsubscribe", { endpoint });
}

export function sendAnnouncement(payload: SendAnnouncementPayload): Promise<number> {
  if (!payload?.title || !payload?.body) {
    return Promise.reject(new Error("title and body are required"));
  }

  return request<number>("POST", "/push/announcements", payload);
}
