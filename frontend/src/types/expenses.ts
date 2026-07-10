export type ExpenseCategory = "Medical" | "School" | "Clothing" | "Activity" | "Childcare" | "Other";

export interface Expense {
  id: string;
  familyId: string;
  childId: string | null;
  childName: string | null;
  paidByUserId: string;
  paidByName: string;
  amount: number;
  description: string;
  category: ExpenseCategory;
  date: string;
  splitPercentForPayer: number;
  isSettled: boolean;
  createdAt: string;
}

export interface ExpenseBalance {
  familyId: string;
  owedByUserId: string | null;
  owedToUserId: string | null;
  amount: number;
}

export interface CreateExpenseRequest {
  childId: string | null;
  amount: number;
  description: string;
  category: ExpenseCategory;
  date: string;
  splitPercentForPayer: number;
}
