import type {
  Family,
  AddChildRequest,
  CreateFamilyRequest,
  Child,
} from "../types";
import { request } from "./http";

export function getFamily(familyId: string): Promise<Family> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<Family>("GET", `/family/${familyId}`);
}

export function createFamily(req: CreateFamilyRequest): Promise<Family> {
  if (!req?.name) {
    return Promise.reject(new Error("Family name is required"));
  }

  return request<Family>("POST", "/family", req);
}

export function joinFamily(familyId: string): Promise<Family> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<Family>("POST", `/family/${familyId}/join`);
}

export function addChild(
  familyId: string,
  req: AddChildRequest
): Promise<Child> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  if (!req?.name) {
    return Promise.reject(new Error("Child name is required"));
  }

  return request<Child>(
    "POST",
    `/family/${familyId}/children`,
    req
  );
}

export function removeChild(
  familyId: string,
  childId: string
): Promise<void> {
  if (!familyId || !childId) {
    return Promise.reject(
      new Error("familyId and childId are required")
    );
  }

  return request<void>(
    "DELETE",
    `/family/${familyId}/children/${childId}`
  );
}