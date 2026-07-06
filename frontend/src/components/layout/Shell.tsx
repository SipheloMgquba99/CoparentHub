import { type FC, type ReactNode } from "react";
import { Ico, Icons } from "../icons";
import { ini } from "../../lib/utils";
import { useTheme } from "../../context/useTheme";
import { NotificationBell } from "./NotificationBell";
import { FamilySwitcher } from "./FamilySwitcher";
import type { AppNotification, Family } from "../../types";

interface ShellProps {
  user: string;
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
  { id: "home",  label: "Home",     icon: Icons.home  },
  { id: "sched", label: "Schedule", icon: Icons.cal   },
  { id: "fam",   label: "Family",   icon: Icons.users },
] as const;

const THEME_ICON = { light: "☀️", dark: "🌙", navy: "🌊" };

export const Shell: FC<ShellProps> = ({ user, tab, setTab, onLogout, notifications, onMarkNotificationRead, families, activeFamilyId, onSelectFamily, children }) => {
  const { theme, cycleTheme } = useTheme();

  return (
    <div className="shell">
      <div className="topbar">
        <div className="tlogo">Coparent<em>Hub</em></div>
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
            title="Sign out"
            aria-label="Sign out"
            onClick={onLogout}
            onKeyDown={e => { if (e.key === "Enter" || e.key === " ") onLogout(); }}
          >
            {ini(user)}
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
    </div>
  );
};