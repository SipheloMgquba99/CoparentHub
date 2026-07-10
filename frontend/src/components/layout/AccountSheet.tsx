import { type FC } from "react";
import type { User } from "../../types";
import { Ico, Icons } from "../icons";
import { ini } from "../../lib/utils";

interface AccountSheetProps {
  user: User;
  onClose: () => void;
  onLogout: () => void;
}

export const AccountSheet: FC<AccountSheetProps> = ({ user, onClose, onLogout }) => {
  return (
    <div className="ov" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="sh">
        <div className="shdrag" />
        <div className="shhead">
          <div className="shtitle">Account</div>
          <button type="button" className="shclose" onClick={onClose} aria-label="Close">
            <Ico d={Icons.x} size={15} />
          </button>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 22 }}>
          <div className="av">{ini(user.fullName)}</div>
          <div>
            <div style={{ fontSize: 15, fontWeight: 600, color: "var(--text)" }}>{user.fullName}</div>
            <div style={{ fontSize: 13, color: "var(--muted)" }}>{user.email}</div>
          </div>
        </div>

        <button type="button" className="btn btn-o" onClick={onLogout} style={{ gap: 6 }}>
          <Ico d={Icons.out} size={15} />Sign Out
        </button>
      </div>
    </div>
  );
};
