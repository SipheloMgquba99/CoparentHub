import { useState, useEffect, useCallback, useRef, type FC } from "react";
import type { Family, AppNotification } from "./types";
import { CSS } from "./styles/global";
import { Shell } from "./components/layout";
import { ThemeProvider } from "./context/ThemeContext";
import { useTheme } from "./context/useTheme";
import { AuthProvider } from "./context/AuthContext";
import { useAuth } from "./context/useAuth";

import HomePage from "./pages/home/HomePage";
import SchedPage from "./pages/schedule/SchedulePage";
import FamPage from "./pages/family/FamilyPage";
import AuthPage from "./pages/auth/AuthPage";
import * as api from "./api";
import { PageSpinner } from "./components/ui";

const NOTIFICATIONS_POLL_MS = 15_000;

const activeFamilyStorageKey = (userId: string) => `cp_active_family:${userId}`;

const Inner: FC = () => {
  const { logout, user } = useAuth();

  const [tab, setTab] = useState<string>("home");
  const [refresh, setRefresh] = useState<number>(0);
  const [families, setFamilies] = useState<Family[]>([]);
  const [activeFamilyId, setActiveFamilyIdState] = useState<string | null>(null);
  const [loadingFamilies, setLoadingFamilies] = useState<boolean>(false);
  const [notifications, setNotifications] = useState<AppNotification[]>([]);
  const isFirstFamiliesLoadRef = useRef(true);

  const onEventsChanged = () => setRefresh((n) => n + 1);

  const setActiveFamilyId = useCallback((familyId: string | null) => {
    setActiveFamilyIdState(familyId);
    if (!user) return;
    if (familyId) localStorage.setItem(activeFamilyStorageKey(user.id), familyId);
    else localStorage.removeItem(activeFamilyStorageKey(user.id));
  }, [user]);

  const onFamChange = (familyId?: string) => {
    setRefresh((n) => n + 1);
    if (familyId) setActiveFamilyId(familyId);
  };

  useEffect(() => {
    if (!user) {
      setFamilies([]);
      setActiveFamilyIdState(null);
      setLoadingFamilies(false);
      isFirstFamiliesLoadRef.current = true;
      return;
    }

    let isMounted = true;
    if (isFirstFamiliesLoadRef.current) {
      setLoadingFamilies(true);
    }

    api.getMyFamilies()
      .then((list) => {
        if (!isMounted) return;
        setFamilies(list);
        setActiveFamilyIdState((current) => {
          const stored = localStorage.getItem(activeFamilyStorageKey(user.id));
          const resolved =
            current && list.some(f => f.id === current) ? current :
            stored && list.some(f => f.id === stored) ? stored :
            (list[0]?.id ?? null);

          if (resolved) localStorage.setItem(activeFamilyStorageKey(user.id), resolved);
          else localStorage.removeItem(activeFamilyStorageKey(user.id));

          return resolved;
        });
      })
      .catch(() => {
        if (isMounted) setFamilies([]);
      })
      .finally(() => {
        if (isMounted) {
          setLoadingFamilies(false);
          isFirstFamiliesLoadRef.current = false;
        }
      });

    return () => {
      isMounted = false;
    };
  }, [user, refresh]);

  const activeFamily = families.find(f => f.id === activeFamilyId) ?? null;

  const fetchNotifications = useCallback(() => {
    api.getNotifications()
      .then((next) => {
        setNotifications(prev => {
          const prevIds = new Set(prev.map(n => n.id));
          const hasNew = next.some(n => !prevIds.has(n.id));
          if (hasNew) setRefresh(r => r + 1);
          return next;
        });
      })
      .catch(() => { });
  }, []);

  useEffect(() => {
    fetchNotifications();
    const id = setInterval(fetchNotifications, NOTIFICATIONS_POLL_MS);
    return () => clearInterval(id);
  }, [fetchNotifications, refresh]);

  const onMarkNotificationRead = (id: string) => {
    setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
    api.markNotificationRead(id).catch(() => fetchNotifications());
  };

  if (!user) return null;

  return (
    <Shell
      user={user.fullName}
      tab={tab}
      setTab={setTab}
      onLogout={logout}
      notifications={notifications}
      onMarkNotificationRead={onMarkNotificationRead}
      families={families}
      activeFamilyId={activeFamilyId}
      onSelectFamily={setActiveFamilyId}
    >
      {loadingFamilies ? (
        <PageSpinner label="Waking up the server — this can take up to a minute if it's been idle." />
      ) : tab === "home" ? (
        <HomePage
          user={user}
          family={activeFamily}
          setTab={setTab}
          refresh={refresh}
          onEventsChanged={onEventsChanged}
        />
      ) : tab === "sched" ? (
        <SchedPage
          user={user}
          family={activeFamily}
          refresh={refresh}
          onEventsChanged={onEventsChanged}
        />
      ) : tab === "fam" ? (
        <FamPage
          user={user}
          families={families}
          activeFamilyId={activeFamilyId}
          onSelectFamily={setActiveFamilyId}
          onFamChange={onFamChange}
        />
      ) : null}
    </Shell>
  );
};

const AppContent: FC = () => {
  const { user, loading } = useAuth();

  if (loading) {
    return <PageSpinner />;
  }

  if (!user) {
    return <AuthPage />;
  }

  return <Inner />;
};
const ThemedApp: FC = () => {
  const { theme } = useTheme();
  return (
    <div className={`app theme-${theme}`}>
      <AppContent />
    </div>
  );
};

export default function App() {
  return (
    <>
      <style>{CSS}</style>
      <ThemeProvider>
        <AuthProvider>
          <ThemedApp />
        </AuthProvider>
      </ThemeProvider>
    </>
  );
}
