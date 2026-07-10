import { useState, type FC, type FormEvent } from "react";
import type { Child, ExpenseCategory } from "../../types";
import { Spinner } from "../ui";
import { Ico, Icons } from "../icons";
import { EXPENSE_CATEGORIES, toLocalDateString } from "../../lib/utils";

export interface ExpenseSheetData {
  childId: string;
  amount: number;
  description: string;
  category: ExpenseCategory;
  date: string;
  splitPercentForPayer: number;
}

interface ExpenseSheetProps {
  children: Child[];
  onSubmit: (data: ExpenseSheetData) => Promise<void>;
  onClose: () => void;
}

export const ExpenseSheet: FC<ExpenseSheetProps> = ({ children, onSubmit, onClose }) => {
  const [childId, setChildId] = useState("");
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [category, setCategory] = useState<ExpenseCategory>("Other");
  const [date, setDate] = useState(toLocalDateString(new Date()));
  const [splitPercent, setSplitPercent] = useState("50");
  const [err, setErr] = useState("");
  const [busy, setBusy] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setErr("");

    const amountNum = parseFloat(amount);
    if (!amountNum || amountNum <= 0) { setErr("Please enter a valid amount."); return; }

    const splitNum = parseFloat(splitPercent);
    if (isNaN(splitNum) || splitNum < 0 || splitNum > 100) { setErr("Split must be between 0 and 100."); return; }

    setBusy(true);
    try {
      await onSubmit({
        childId,
        amount: amountNum,
        description,
        category,
        date,
        splitPercentForPayer: splitNum,
      });
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Something went wrong.");
    }
    setBusy(false);
  };

  return (
    <div className="ov" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="sh">
        <div className="shdrag" />
        <div className="shhead">
          <div className="shtitle">Add Expense</div>
          <button type="button" className="shclose" onClick={onClose} aria-label="Close">
            <Ico d={Icons.x} size={15} />
          </button>
        </div>
        {err && <div className="err">{err}</div>}
        <form onSubmit={submit}>
          <div className="f">
            <label>Description</label>
            <input value={description} onChange={e => setDescription(e.target.value)} placeholder="Winter jacket" maxLength={300} required autoFocus />
          </div>

          <div className="frow">
            <div className="f">
              <label>Amount (ZAR)</label>
              <input type="number" step="0.01" min="0.01" value={amount} onChange={e => setAmount(e.target.value)} placeholder="0.00" required />
            </div>
            <div className="f">
              <label>Date</label>
              <input type="date" value={date} onChange={e => setDate(e.target.value)} max={toLocalDateString(new Date())} required />
            </div>
          </div>

          <div className="f" style={{ marginBottom: 14 }}>
            <label>Category</label>
            <div className="chips" style={{ marginTop: 5 }}>
              {EXPENSE_CATEGORIES.map(c => (
                <button type="button" key={c} className={`chip ${category === c ? "on" : ""}`} onClick={() => setCategory(c)}>
                  {c}
                </button>
              ))}
            </div>
          </div>

          <div className="f">
            <label>For Child <span style={{ fontWeight: 400, textTransform: "none", letterSpacing: 0, color: "var(--muted)", fontSize: 11 }}>(optional)</span></label>
            <select value={childId} onChange={e => setChildId(e.target.value)}>
              <option value="">Not child-specific</option>
              {children.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>

          <div className="f">
            <label>Your Share (%)</label>
            <input type="number" min="0" max="100" step="1" value={splitPercent} onChange={e => setSplitPercent(e.target.value)} required />
          </div>

          <button className="btn btn-p" type="submit" disabled={busy} style={{ marginTop: 12 }}>
            {busy ? <Spinner /> : "Add Expense"}
          </button>
        </form>
      </div>
    </div>
  );
};
