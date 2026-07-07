import type {
  Family,
  FamilyInvite,
  FamilyInviteStatus,
  PendingInvite,
  AddChildRequest,
  CreateFamilyRequest,
} from "../types";
import { request } from "./http";

export function getMyFamilies(): Promise<Family[]> {
  return request<Family[]>("GET", "/family");
}

export function getFamily(familyId: string): Promise<Family> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<Family>("GET", `/family/${familyId}`);
}

export function createFamily(req: CreateFamilyRequest): Promise<string> {
  if (!req?.name) {
    return Promise.reject(new Error("Family name is required"));
  }

  return request<string>("POST", "/family", req);
}

export function deleteFamily(familyId: string): Promise<string> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<string>("DELETE", `/family/${familyId}`);
}

export function joinFamilyByCode(code: string): Promise<string> {
  if (!code) {
    return Promise.reject(new Error("Invite code is required"));
  }

  return request<string>("POST", "/family/join", { code: code.trim().toUpperCase() });
}

export function createFamilyInvite(familyId: string): Promise<FamilyInvite> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<FamilyInvite>("POST", `/family/${familyId}/invites`);
}

export function getActiveFamilyInvite(familyId: string): Promise<FamilyInvite | null> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<FamilyInvite | null>("GET", `/family/${familyId}/invites/active`);
}

export function sendFamilyInviteEmail(familyId: string, email: string): Promise<FamilyInvite> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  if (!email) {
    return Promise.reject(new Error("email is required"));
  }

  return request<FamilyInvite>("POST", `/family/${familyId}/invites/email`, { email: email.trim() });
}

export function getFamilyInviteStatus(familyId: string): Promise<FamilyInviteStatus | null> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<FamilyInviteStatus | null>("GET", `/family/${familyId}/invites/status`);
}

export function getPendingInvite(): Promise<PendingInvite | null> {
  return request<PendingInvite | null>("GET", "/family/pending-invite");
}

export function addChild(
  familyId: string,
  req: AddChildRequest
): Promise<string> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  if (!req?.name) {
    return Promise.reject(new Error("Child name is required"));
  }

  return request<string>(
    "POST",
    `/family/${familyId}/children`,
    req
  );
}

export function removeChild(
  familyId: string,
  childId: string
): Promise<string> {
  if (!familyId || !childId) {
    return Promise.reject(
      new Error("familyId and childId are required")
    );
  }

  return request<string>(
    "DELETE",
    `/family/${familyId}/children/${childId}`
  );
}
