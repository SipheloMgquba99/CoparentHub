import { useState, useEffect, type FC } from "react";
import type { User, Family, Expense, ExpenseBalance } from "../../types";
import * as api from "../../api";
import { ExpenseSheet } from "../../components/expenses";
import { Ico, Icons } from "../../components/icons";
import { PageSpinner, Spinner } from "../../components/ui";
import { fmtZAR } from "../../lib/utils";

interface ExpensesPageProps {
  user: User;
  family: Family | null;
  refresh: number;
  onEventsChanged: () => void;
}

const fmtDate = (s: string) =>
  new Date(s + "T00:00:00").toLocaleDateString("en-ZA", { month: "short", day: "numeric", year: "numeric" });

const ExpensesPage: FC<ExpensesPageProps> = ({ user, family, refresh, onEventsChanged }) => {
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [balance, setBalance] = useState<ExpenseBalance | null>(null);
  const [busy, setBusy] = useState(true);
  const [add, setAdd] = useState(false);
  const [showSettleConfirm, setShowSettleConfirm] = useState(false);
  const [settling, setSettling] = useState(false);
  const [settleErr, setSettleErr] = useState("");
  const [tick, setTick] = useState(0);

  useEffect(() => {
    if (!family) { setBusy(false); return; }
    setBusy(true);
    Promise.all([api.getExpenses(family.id), api.getExpenseBalance(family.id)])
      .then(([e, b]) => { setExpenses(e); setBalance(b); })
      .catch(() => { setExpenses([]); setBalance(null); })
      .finally(() => setBusy(false));
  }, [family?.id, refresh, tick]);

  const memberName = (userId: string) =>
    family?.members.find(m => m.userId === userId)?.fullName ?? "Someone";

  const handleRemove = async (expenseId: string) => {
    if (!family) return;
    if (!confirm("Remove this expense?")) return;
    try {
      await api.removeExpense(family.id, expenseId);
      setTick(t => t + 1);
      onEventsChanged();
    } catch (ex: unknown) {
      alert(ex instanceof Error ? ex.message : "Failed to remove expense.");
    }
  };

  const confirmSettleAll = async () => {
    if (!family) return;
    setSettling(true); setSettleErr("");
    try {
      await api.settleAllExpenses(family.id);
      setShowSettleConfirm(false);
      setTick(t => t + 1);
      onEventsChanged();
    } catch (ex: unknown) {
      setSettleErr(ex instanceof Error ? ex.message : "Failed to settle expenses.");
    }
    setSettling(false);
  };

  const familyFull = (family?.members.length ?? 0) >= 2;

  return (
    <>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 16 }}>
        <div className="page-title" style={{ marginBottom: 0 }}>Expenses</div>
        {family && (
          <button type="button" className="btn btn-p btn-sm" onClick={() => setAdd(true)} style={{ gap: 4 }}>
            <Ico d={Icons.plus} size={13} />Add
          </button>
        )}
      </div>

      {!family ? (
        <div className="empty" style={{ paddingTop: 40 }}>Join or create a family to track expenses.</div>
      ) : busy ? (
        <PageSpinner />
      ) : (
        <>
          <div className="card">
            {!familyFull ? (
              <div className="empty">Invite a co-parent to start splitting expenses.</div>
            ) : !balance || balance.amount === 0 ? (
              <div style={{ textAlign: "center", padding: "8px 0" }}>
                <div style={{ fontFamily: "'Fraunces',serif", fontSize: 20, color: "var(--text)" }}>
                  All settled up 🎉
                </div>
              </div>
            ) : (
              <>
                <div style={{ textAlign: "center", padding: "4px 0 14px" }}>
                  <div className="sl" style={{ marginBottom: 6 }}>Balance</div>
                  <div style={{ fontFamily: "'Fraunces',serif", fontSize: 22, color: "var(--text)", lineHeight: 1.4 }}>
                    <strong>{memberName(balance.owedByUserId!)}</strong> owes <strong>{memberName(balance.owedToUserId!)}</strong>
                    <br />
                    <span style={{ color: "var(--accent)" }}>{fmtZAR(balance.amount)}</span>
                  </div>
                </div>
                {settleErr && <div className="err">{settleErr}</div>}
                <button className="btn btn-o btn-sm" style={{ width: "100%" }} onClick={() => { setSettleErr(""); setShowSettleConfirm(true); }}>
                  Mark All Settled
                </button>
              </>
            )}
          </div>

          <div className="card">
            <div className="ct" style={{ marginBottom: 12 }}>History ({expenses.length})</div>
            {expenses.length === 0 && <div className="empty">No expenses logged yet</div>}
            {expenses.map(ex => (
              <div key={ex.id} className="ci">
                <div>
                  <div className="cn">{ex.description}</div>
                  <div className="cd">
                    {ex.category} · {fmtDate(ex.date)} · Paid by {ex.paidByName}
                    {ex.childName && ` · ${ex.childName}`}
                  </div>
                </div>
                <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                  {ex.isSettled && <span className="cage">Settled</span>}
                  <span style={{ fontWeight: 600, color: "var(--text)" }}>{fmtZAR(ex.amount)}</span>
                  {!ex.isSettled && ex.paidByUserId === user.id && (
                    <button className="btn btn-gh btn-sm" style={{ color: "var(--danger)", padding: 6 }} onClick={() => handleRemove(ex.id)} aria-label="Remove expense" title="Remove expense">
                      <Ico d={Icons.trash} size={14} />
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </>
      )}

      {add && family && (
        <ExpenseSheet
          children={family.children}
          onSubmit={async data => {
            await api.createExpense(family.id, {
              childId: data.childId || null,
              amount: data.amount,
              description: data.description,
              category: data.category,
              date: data.date,
              splitPercentForPayer: data.splitPercentForPayer,
            });
            setAdd(false);
            setTick(t => t + 1);
            onEventsChanged();
          }}
          onClose={() => setAdd(false)}
        />
      )}

      {showSettleConfirm && (
        <div className="ov" onClick={e => e.target === e.currentTarget && !settling && setShowSettleConfirm(false)}>
          <div className="sh">
            <div className="shdrag" />
            <div className="shhead">
              <div className="shtitle">Mark All Settled</div>
              <button type="button" className="shclose" onClick={() => setShowSettleConfirm(false)} disabled={settling} aria-label="Close">
                <Ico d={Icons.x} size={15} />
              </button>
            </div>
            <p style={{ fontSize: 14, color: "var(--text)", lineHeight: 1.6, marginBottom: 20 }}>
              This clears the balance and marks every unsettled expense as paid. This cannot be undone.
            </p>
            {settleErr && <div className="err">{settleErr}</div>}
            <div style={{ display: "flex", gap: 10 }}>
              <button type="button" className="btn btn-o" onClick={() => setShowSettleConfirm(false)} disabled={settling} style={{ flex: 1 }}>
                Cancel
              </button>
              <button type="button" className="btn btn-p" onClick={confirmSettleAll} disabled={settling} style={{ flex: 1 }}>
                {settling ? <Spinner /> : "Confirm"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default ExpensesPage;
