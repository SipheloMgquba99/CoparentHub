import { useState, type FC, type ReactNode } from "react";
import { Ico, Icons } from "../icons";
import { ini } from "../../lib/utils";
import { useTheme } from "../../context/useTheme";
import { NotificationBell } from "./NotificationBell";
import { FamilySwitcher } from "./FamilySwitcher";
import { AccountSheet } from "./AccountSheet";
import type { AppNotification, Family, User } from "../../types";

interface ShellProps {
  user: User;
  tab: string;
  setTab: (t: string) => void;
  onLogout: () => void;
  notifications: AppNotification[];
  onMarkNotificationRead: (id: string) => void;
  families: Family[];
  activeFamilyId: string | null;
  onSelectFamily: (familyId: string) => void;
  children: ReactNode;
}

const NAV_ITEMS = [
  { id: "home",     label: "Home",     icon: Icons.home   },
  { id: "sched",    label: "Schedule", icon: Icons.cal    },
  { id: "expenses", label: "Expenses", icon: Icons.wallet },
  { id: "messages", label: "Messages", icon: Icons.chat   },
  { id: "fam",      label: "Family",   icon: Icons.users  },
] as const;

const THEME_ICON = { light: "☀️", dark: "🌙", navy: "🌊" };

export const Shell: FC<ShellProps> = ({ user, tab, setTab, onLogout, notifications, onMarkNotificationRead, families, activeFamilyId, onSelectFamily, children }) => {
  const { theme, cycleTheme } = useTheme();
  const [showAccount, setShowAccount] = useState(false);

  return (
    <div className="shell">
      <div className="topbar">
        <button
          type="button"
          className="tlogo"
          onClick={() => setTab("home")}
          aria-label="Go to Home"
          style={{ background: "none", border: "none", cursor: "pointer", padding: 0, font: "inherit" }}
        >
          Coparent<em>Hub</em>
        </button>
        <div className="topbar-right">
          <FamilySwitcher
            families={families}
            activeFamilyId={activeFamilyId}
            onSelectFamily={onSelectFamily}
            onAddFamily={() => setTab("fam")}
          />
          <NotificationBell notifications={notifications} onMarkRead={onMarkNotificationRead} />
          <button
            className="theme-btn"
            onClick={cycleTheme}
            aria-label={`Switch theme (current: ${theme})`}
            title={`Switch theme (current: ${theme})`}
          >
            {THEME_ICON[theme]}
          </button>
          <div
            className="tav"
            role="button"
            tabIndex={0}
            title="Account"
            aria-label="Account"
            onClick={() => setShowAccount(true)}
            onKeyDown={e => { if (e.key === "Enter" || e.key === " ") setShowAccount(true); }}
          >
            {ini(user.fullName)}
          </div>
        </div>
      </div>
      <div className="page">{children}</div>
      <nav className="nav" aria-label="Main navigation">
        {NAV_ITEMS.map(n => (
          <button
            key={n.id}
            className={`ni ${tab === n.id ? "on" : ""}`}
            onClick={() => setTab(n.id)}
            aria-current={tab === n.id ? "page" : undefined}
          >
            <Ico d={n.icon} size={20} />
            <span>{n.label}</span>
          </button>
        ))}
      </nav>

      {showAccount && (
        <AccountSheet
          user={user}
          onClose={() => setShowAccount(false)}
          onLogout={onLogout}
        />
      )}
    </div>
  );
};