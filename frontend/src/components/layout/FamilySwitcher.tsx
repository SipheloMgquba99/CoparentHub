import { useState, useRef, useEffect, type FC } from "react";
import type { Family } from "../../types";
import { Ico, Icons } from "../icons";

interface FamilySwitcherProps {
  families: Family[];
  activeFamilyId: string | null;
  onSelectFamily: (familyId: string) => void;
  onAddFamily: () => void;
}

export const FamilySwitcher: FC<FamilySwitcherProps> = ({ families, activeFamilyId, onSelectFamily, onAddFamily }) => {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const active = families.find(f => f.id === activeFamilyId);

  useEffect(() => {
    if (!open) return;
    const onClickOutside = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, [open]);

  if (families.length === 0) return null;

  return (
    <div ref={ref} className="fswitch-wrap">
      <button
        type="button"
        className="fswitch-btn"
        onClick={() => setOpen(o => !o)}
        aria-label={`Switch family (current: ${active?.name ?? "none"})`}
        title="Switch family"
      >
        <span className="fswitch-name">{active?.name ?? "Select family"}</span>
        <Ico d={Icons.chR} size={12} />
      </button>

      {open && (
        <div role="menu" className="fswitch-menu">
          {families.map(f => (
            <button
              key={f.id}
              type="button"
              role="menuitem"
              onClick={() => { onSelectFamily(f.id); setOpen(false); }}
              className={`fswitch-item ${f.id === activeFamilyId ? "active" : ""}`}
            >
              <span>{f.name}</span>
              {f.id === activeFamilyId && <Ico d={Icons.ok} size={13} />}
            </button>
          ))}
          <button
            type="button"
            role="menuitem"
            onClick={() => { onAddFamily(); setOpen(false); }}
            className="fswitch-item fswitch-add"
          >
            <Ico d={Icons.plus} size={13} />
            <span>Add another family</span>
          </button>
        </div>
      )}
    </div>
  );
};
