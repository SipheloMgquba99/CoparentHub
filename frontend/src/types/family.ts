export interface Member {
  userId: string;
  fullName: string;
  email: string;
}

export interface Child {
  id: string;
  name: string;
  dateOfBirth: string | null;
  allergies: string | null;
  medications: string | null;
  medicalNotes: string | null;
  doctorName: string | null;
  doctorPhone: string | null;
  schoolName: string | null;
  schoolContact: string | null;
  clothingSize: string | null;
  shoeSize: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
}

export interface Family {
  id: string;
  name: string;
  members: Member[];
  children: Child[];
}

export interface CreateFamilyRequest {
  name: string;
 userId?: string;
}

export interface FamilyInvite {
  code: string;
  expiresAt: string;
}

export interface FamilyInviteStatus {
  expiresAt: string;
  isExpired: boolean;
}

export interface PendingInvite {
  familyName: string;
  code: string;
}

export interface AddChildRequest {
  name: string;
  dateOfBirth: string | null;
}

export interface UpdateChildInfoRequest {
  allergies: string | null;
  medications: string | null;
  medicalNotes: string | null;
  doctorName: string | null;
  doctorPhone: string | null;
  schoolName: string | null;
  schoolContact: string | null;
  clothingSize: string | null;
  shoeSize: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
}