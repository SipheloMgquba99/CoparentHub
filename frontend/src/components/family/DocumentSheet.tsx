import { useState, type FC, type FormEvent } from "react";
import type { Child, DocumentCategory } from "../../types";
import { Spinner } from "../ui";
import { Ico, Icons } from "../icons";
import { DOCUMENT_CATEGORIES } from "../../lib/utils";

export interface DocumentSheetData {
  file: File;
  category: DocumentCategory;
  childId: string;
  description: string;
}

interface DocumentSheetProps {
  children: Child[];
  onSubmit: (data: DocumentSheetData) => Promise<void>;
  onClose: () => void;
}

const MAX_FILE_BYTES = 10_000_000;

export const DocumentSheet: FC<DocumentSheetProps> = ({ children, onSubmit, onClose }) => {
  const [file, setFile] = useState<File | null>(null);
  const [category, setCategory] = useState<DocumentCategory>("Other");
  const [childId, setChildId] = useState("");
  const [description, setDescription] = useState("");
  const [err, setErr] = useState("");
  const [busy, setBusy] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setErr("");

    if (!file) { setErr("Please choose a file."); return; }
    if (file.size > MAX_FILE_BYTES) { setErr("Files must be 10MB or smaller."); return; }

    setBusy(true);
    try {
      await onSubmit({ file, category, childId, description });
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
          <div className="shtitle">Upload Document</div>
          <button type="button" className="shclose" onClick={onClose} aria-label="Close">
            <Ico d={Icons.x} size={15} />
          </button>
        </div>
        {err && <div className="err">{err}</div>}
        <form onSubmit={submit}>
          <div className="f">
            <label>File</label>
            <input
              type="file"
              accept=".pdf,.jpg,.jpeg,.png,.doc,.docx,application/pdf,image/jpeg,image/png,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
              onChange={e => setFile(e.target.files?.[0] ?? null)}
              required
            />
          </div>

          <div className="f" style={{ marginBottom: 14 }}>
            <label>Category</label>
            <div className="chips" style={{ marginTop: 5 }}>
              {DOCUMENT_CATEGORIES.map(c => (
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
            <label>Description <span style={{ fontWeight: 400, textTransform: "none", letterSpacing: 0, color: "var(--muted)", fontSize: 11 }}>(optional)</span></label>
            <input value={description} onChange={e => setDescription(e.target.value)} placeholder="Notes about this document" maxLength={500} />
          </div>

          <button className="btn btn-p" type="submit" disabled={busy} style={{ marginTop: 12 }}>
            {busy ? <Spinner /> : "Upload"}
          </button>
        </form>
      </div>
    </div>
  );
};
