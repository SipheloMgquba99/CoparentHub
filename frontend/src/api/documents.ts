import type { FamilyDocument, DocumentCategory } from "../types";
import { request, apiUrl, getToken } from "./http";

export function getDocuments(familyId: string): Promise<FamilyDocument[]> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<FamilyDocument[]>("GET", `/families/${familyId}/documents`);
}

export function uploadDocument(
  familyId: string,
  file: File,
  category: DocumentCategory,
  childId: string | null,
  description: string | null
): Promise<string> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  const form = new FormData();
  form.set("file", file);
  form.set("category", category);
  if (childId) form.set("childId", childId);
  if (description) form.set("description", description);

  return request<string>("POST", `/families/${familyId}/documents`, form);
}

export function removeDocument(familyId: string, documentId: string): Promise<string> {
  if (!familyId || !documentId) {
    return Promise.reject(new Error("familyId and documentId are required"));
  }

  return request<string>("DELETE", `/families/${familyId}/documents/${documentId}`);
}

export async function downloadDocument(familyId: string, documentId: string, fallbackName: string): Promise<void> {
  const token = getToken();
  const res = await fetch(apiUrl(`/families/${familyId}/documents/${documentId}/content`), {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });

  if (!res.ok) {
    throw new Error("Failed to download document.");
  }

  const disposition = res.headers.get("Content-Disposition") ?? "";
  const match = /filename="?([^"]+)"?/.exec(disposition);
  const fileName = match?.[1] ?? fallbackName;

  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}
