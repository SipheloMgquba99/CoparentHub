import { type FC, type ReactNode } from "react";
import { Ico, Icons } from "../icons";
import { ini } from "../../lib/utils";
import { useTheme } from "../../context/ThemeContext";

interface ShellProps {
  user: string;
  tab: string;
  setTab: (t: string) => void;
  onLogout: () => void;
  children: ReactNode;
}

const NAV_ITEMS = [
  { id: "home",  label: "Home",     icon: Icons.home  },
  { id: "sched", label: "Schedule", icon: Icons.cal   },
  { id: "fam",   label: "Family",   icon: Icons.users },
] as const;

const THEME_ICON = { light: "☀️", dark: "🌙", navy: "🌊" };

export const Shell: FC<ShellProps> = ({ user, tab, setTab, onLogout, children }) => {
  const { theme, cycleTheme } = useTheme();

  return (
    <div className="shell">
      <div className="topbar">
        <div className="tlogo">Coparent<em>Hub</em></div>
        <div className="topbar-right">
          <button className="theme-btn" onClick={cycleTheme}>{THEME_ICON[theme]}</button>
          <div className="tav" title="Sign out" onClick={onLogout}>{ini(user)}</div>
        </div>
      </div>
      <div className="page">{children}</div>
      <nav className="nav">
        {NAV_ITEMS.map(n => (
          <button key={n.id} className={`ni ${tab === n.id ? "on" : ""}`} onClick={() => setTab(n.id)}>
            <Ico d={n.icon} size={20} />
            <span>{n.label}</span>
          </button>
        ))}
      </nav>
    </div>
  );
};