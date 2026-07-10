import type { Expense, ExpenseBalance, CreateExpenseRequest } from "../types";
import { request } from "./http";

export function getExpenses(familyId: string): Promise<Expense[]> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<Expense[]>("GET", `/families/${familyId}/expenses`);
}

export function getExpenseBalance(familyId: string): Promise<ExpenseBalance> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<ExpenseBalance>("GET", `/families/${familyId}/expenses/balance`);
}

export function createExpense(familyId: string, req: CreateExpenseRequest): Promise<string> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<string>("POST", `/families/${familyId}/expenses`, req);
}

export function removeExpense(familyId: string, expenseId: string): Promise<string> {
  if (!familyId || !expenseId) {
    return Promise.reject(new Error("familyId and expenseId are required"));
  }

  return request<string>("DELETE", `/families/${familyId}/expenses/${expenseId}`);
}

export function settleAllExpenses(familyId: string): Promise<string> {
  if (!familyId) {
    return Promise.reject(new Error("familyId is required"));
  }

  return request<string>("POST", `/families/${familyId}/expenses/settle-all`);
}
