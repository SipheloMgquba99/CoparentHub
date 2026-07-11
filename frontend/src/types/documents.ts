export type DocumentCategory = "Legal" | "Medical" | "School" | "Financial" | "Other";

export interface FamilyDocument {
  id: string;
  familyId: string;
  childId: string | null;
  childName: string | null;
  uploadedByUserId: string;
  uploadedByName: string;
  fileName: string;
  contentType: string;
  category: DocumentCategory;
  sizeBytes: number;
  description: string | null;
  createdAt: string;
}
