export interface Member {
  userId: string;
  fullName: string;
  email: string;
}

export interface Child {
  id: string;
  name: string;
  dateOfBirth: string | null; 
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

export interface AddChildRequest {
  name: string;
  dateOfBirth: string | null;
}