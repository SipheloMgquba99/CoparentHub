export interface Message {
  id: string;
  familyId: string;
  senderUserId: string;
  senderName: string;
  body: string;
  isReadByRecipient: boolean;
  createdAt: string;
}
