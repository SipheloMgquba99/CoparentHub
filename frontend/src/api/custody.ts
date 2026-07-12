import type { CustodySchedule, CustodyRange, CreateCustodyScheduleRequest } from "../types";
import { request } from "./http";

export function getActiveCustodySchedule(familyId: string): Promise<CustodySchedule> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<CustodySchedule>("GET", `/families/${familyId}/custody/active`);
}

export function getCustodyForRange(familyId: string, from: string, to: string): Promise<CustodyRange> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<CustodyRange>("GET", `/families/${familyId}/custody?from=${from}&to=${to}`);
}

export function createCustodySchedule(familyId: string, req: CreateCustodyScheduleRequest): Promise<string> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<string>("POST", `/families/${familyId}/custody`, req);
}
