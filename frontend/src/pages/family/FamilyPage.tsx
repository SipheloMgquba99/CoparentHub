import { useState, type FC, type FormEvent } from "react";
import type { User, Family } from "../../types";
import * as api from "../../api";
import { Ico, Icons } from "../../components/icons";
import { Spinner } from "../../components/ui";
import { calcAge, ini, toLocalDateString } from "../../lib/utils";

interface FamilyPageProps {
  user: User;
  families: Family[];
  activeFamilyId: string | null;
  onSelectFamily: (familyId: string) => void;
  onFamChange: (familyId?: string) => void;
}

const FamilyPage: FC<FamilyPageProps> = ({ user, families, activeFamilyId, onSelectFamily, onFamChange }) => {
  const [mode, setMode] = useState<string | null>(null);
  const [input, setInput] = useState("");
  const [cname, setCname] = useState("");
  const [dob, setDob] = useState("");
  const [err, setErr] = useState("");
  const [busy, setBusy] = useState(false);
  const [copied, setCopied] = useState(false);

  const family = families.find(f => f.id === activeFamilyId) ?? null;

  const handleFamilySubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErr(""); setBusy(true);

    try {
      const familyId = mode === "create"
        ? await api.createFamily({ name: input })
        : await api.joinFamily(input.trim());

      onFamChange(familyId);
      setMode(null); setInput("");
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Error");
    }

    setBusy(false);
  };

  const handleAddChild = async (e: FormEvent) => {
    e.preventDefault(); setErr(""); setBusy(true);
    try {
      await api.addChild(family!.id, { name: cname, dateOfBirth: dob || null });
      onFamChange();
      setMode(null); setCname(""); setDob("");
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Error");
    }
    setBusy(false);
  };

  // ── Remove Child ───────────────────────────────────────────────────────
  const handleRemoveChild = async (childId: string) => {
    if (!confirm("Remove this child from the family?")) return;
    try {
      await api.removeChild(family!.id, childId);
      onFamChange();
    } catch (ex: unknown) {
      alert(ex instanceof Error ? ex.message : "Failed to remove child.");
    }
  };

  // ── Copy Family ID ─────────────────────────────────────────────────────
  const copyId = () => {
    navigator.clipboard?.writeText(family!.id);
    setCopied(true); setTimeout(() => setCopied(false), 2000);
  };

  const familySheet = mode && (mode === "create" || mode === "join") && (
    <div className="ov" onClick={e => e.target === e.currentTarget && setMode(null)}>
      <div className="sh">
        <div className="shdrag" />
        <div className="shhead">
          <div className="shtitle">{mode === "create" ? "Create Family" : "Join Family"}</div>
          <button type="button" className="shclose" onClick={() => setMode(null)} aria-label="Close">
            <Ico d={Icons.x} size={15} />
          </button>
        </div>
        {err && <div className="err">{err}</div>}
        <form onSubmit={handleFamilySubmit}>
          <div className="f">
            <label>{mode === "create" ? "Family Name" : "Family ID"}</label>
            <input
              value={input}
              onChange={e => setInput(e.target.value)}
              placeholder={mode === "create" ? "The Smith Family" : "Paste the Family ID here…"}
              maxLength={mode === "create" ? 100 : 200}
              required autoFocus
            />
          </div>
          <button className="btn btn-p" type="submit" disabled={busy}>
            {busy ? <Spinner /> : mode === "create" ? "Create" : "Join"}
          </button>
        </form>
      </div>
    </div>
  );

  // ── No family yet ──────────────────────────────────────────────────────
  if (families.length === 0) return (
    <div style={{ textAlign: "center", padding: "44px 16px" }}>
      <div style={{ fontSize: 52, marginBottom: 14 }}>🏡</div>
      <div style={{ fontFamily: "'Fraunces',serif", fontSize: 24, color: "var(--text)", marginBottom: 8 }}>
        No family yet
      </div>
      <div style={{ fontSize: 14, color: "var(--muted)", marginBottom: 28, lineHeight: 1.6 }}>
        Create a new group or join an existing<br />one by pasting a Family ID.
      </div>
      <div style={{ display: "flex", gap: 10, justifyContent: "center" }}>
        <button className="btn btn-p" style={{ width: "auto", padding: "11px 24px" }} onClick={() => { setErr(""); setMode("create"); }}>
          Create Family
        </button>
        <button className="btn btn-o" style={{ width: "auto", padding: "11px 24px" }} onClick={() => { setErr(""); setMode("join"); }}>
          Join Family
        </button>
      </div>

      {familySheet}
    </div>
  );

  // ── One or more families ────────────────────────────────────────────────
  return (<>
    <div className="card">
      <div className="ch">
        <div className="ct">My Families ({families.length})</div>
        <div style={{ display: "flex", gap: 6 }}>
          <button className="btn btn-o btn-sm" onClick={() => { setErr(""); setMode("join"); }}>
            Join
          </button>
          <button className="btn btn-p btn-sm" onClick={() => { setErr(""); setMode("create"); }} style={{ gap: 4 }}>
            <Ico d={Icons.plus} size={13} />Create
          </button>
        </div>
      </div>

      {families.map(f => (
        <div
          key={f.id}
          className={`myfam-row ${f.id === activeFamilyId ? "active" : ""}`}
          role="button"
          tabIndex={0}
          onClick={() => onSelectFamily(f.id)}
          onKeyDown={e => { if (e.key === "Enter" || e.key === " ") onSelectFamily(f.id); }}
        >
          <div>
            <div className="myfam-name">{f.name}</div>
            <div className="myfam-meta">
              {f.members.length} co-parent{f.members.length === 1 ? "" : "s"} · {f.children.length} child{f.children.length === 1 ? "" : "ren"}
            </div>
          </div>
          {f.id === activeFamilyId
            ? <span className="myfam-badge">Active</span>
            : <span className="myfam-switch">Switch</span>
          }
        </div>
      ))}
    </div>

    {family && (<>
      <div style={{ fontFamily: "'Fraunces',serif", fontSize: 24, color: "var(--text)", marginBottom: 2 }}>
        {family.name}
      </div>
      <div style={{ fontSize: 13, color: "var(--muted)", marginBottom: 18 }}>Family Group</div>

      <div className="fidbox">
        <div>
          <div className="fidlbl">Family ID — share to invite</div>
          <div className="fidval">{family.id.slice(0, 24)}…</div>
        </div>
        <button
          className="btn btn-sm"
          onClick={copyId}
          aria-label="Copy family ID to clipboard"
          style={{ background: "rgba(255,255,255,.12)", color: "#fff", border: "1px solid rgba(255,255,255,.2)", gap: 5, fontWeight: 600 }}
        >
          {copied
            ? <><Ico d={Icons.ok} size={13} />Copied!</>
            : <><Ico d={Icons.copy} size={13} />Copy</>
          }
        </button>
      </div>

      <div className="card">
        <div className="ct" style={{ marginBottom: 12 }}>Co-parents ({family.members.length})</div>
        {family.members.map(m => (
          <div key={m.userId} className="mi">
            <div className="av">{ini(m.fullName)}</div>
            <div>
              <div className="mn">
                {m.fullName}
                {m.userId === user.id && (
                  <span style={{ fontSize: 11, color: "var(--gold)", fontWeight: 700, marginLeft: 5 }}>· You</span>
                )}
              </div>
              <div className="me">{m.email}</div>
            </div>
          </div>
        ))}
      </div>

      <div className="card">
        <div className="ch">
          <div className="ct">Children ({family.children.length})</div>
          <button className="btn btn-p btn-sm" onClick={() => { setErr(""); setMode("child"); }} style={{ gap: 4 }}>
            <Ico d={Icons.plus} size={13} />Add
          </button>
        </div>

        {family.children.length === 0 && <div className="empty">No children added yet</div>}

        {family.children.map(c => (
          <div key={c.id} className="ci">
            <div style={{ display: "flex", alignItems: "center", gap: 11 }}>
              <div className="av" style={{ background: "var(--echild-bg)", color: "var(--echild-text)" }}>
                <Ico d={Icons.kid} size={15} stroke="var(--echild-text)" />
              </div>
              <div>
                <div className="cn">{c.name}</div>
                <div className="cd">
                  {c.dateOfBirth
                    ? new Date(c.dateOfBirth + "T00:00:00").toLocaleDateString("en-US", { month: "long", day: "numeric", year: "numeric" })
                    : "No date of birth set"}
                </div>
              </div>
            </div>
            <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
              {calcAge(c.dateOfBirth) !== null && <span className="cage">{calcAge(c.dateOfBirth)}y</span>}
              <button className="btn btn-gh btn-sm" style={{ color: "var(--danger)", padding: 6 }} onClick={() => handleRemoveChild(c.id)} aria-label={`Remove ${c.name}`} title={`Remove ${c.name}`}>
                <Ico d={Icons.trash} size={14} />
              </button>
            </div>
          </div>
        ))}
      </div>

      {mode === "child" && (
        <div className="ov" onClick={e => e.target === e.currentTarget && setMode(null)}>
          <div className="sh">
            <div className="shdrag" />
            <div className="shhead">
              <div className="shtitle">Add Child</div>
              <button type="button" className="shclose" onClick={() => setMode(null)} aria-label="Close">
                <Ico d={Icons.x} size={15} />
              </button>
            </div>
            {err && <div className="err">{err}</div>}
            <form onSubmit={handleAddChild}>
              <div className="f">
                <label>Name</label>
                <input value={cname} onChange={e => setCname(e.target.value)} placeholder="Alex" maxLength={100} required autoFocus />
              </div>
              <div className="f">
                <label>Date of Birth</label>
                <input type="date" value={dob} onChange={e => setDob(e.target.value)} max={toLocalDateString(new Date())} />
              </div>
              <button className="btn btn-p" type="submit" disabled={busy}>
                {busy ? <Spinner /> : "Add Child"}
              </button>
            </form>
          </div>
        </div>
      )}
    </>)}

    {familySheet}

    {copied && <div className="toast">✓ Family ID copied to clipboard!</div>}
  </>);
};

export default FamilyPage;
