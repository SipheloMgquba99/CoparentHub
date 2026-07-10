import type { Message } from "../types";
import { request } from "./http";

export function getMessages(familyId: string): Promise<Message[]> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<Message[]>("GET", `/families/${familyId}/messages`);
}

export function sendMessage(familyId: string, body: string): Promise<string> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<string>("POST", `/families/${familyId}/messages`, { body });
}

export function markThreadRead(familyId: string): Promise<boolean> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<boolean>("POST", `/families/${familyId}/messages/read`);
}
