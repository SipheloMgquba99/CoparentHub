import { useState, useEffect, useCallback, useRef, type FC } from "react";
import type { Family, AppNotification, PendingInvite } from "./types";
import { CSS } from "./styles/global";
import { Shell } from "./components/layout";
import { ThemeProvider } from "./context/ThemeContext";
import { useTheme } from "./context/useTheme";
import { AuthProvider } from "./context/AuthContext";
import { useAuth } from "./context/useAuth";
import { Ico, Icons } from "./components/icons";

import HomePage from "./pages/home/HomePage";
import SchedPage from "./pages/schedule/SchedulePage";
import CustodyPage from "./pages/custody/CustodyPage";
import ExpensesPage from "./pages/expenses/ExpensesPage";
import MessagesPage from "./pages/messages/MessagesPage";
import FamPage from "./pages/family/FamilyPage";
import AuthPage from "./pages/auth/AuthPage";
import ResetPasswordPage from "./pages/auth/ResetPasswordPage";
import * as api from "./api";
import { PageSpinner, Spinner } from "./components/ui";
import { PushEnablePrompt } from "./components/PushEnablePrompt";

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
  const [pendingInvite, setPendingInvite] = useState<PendingInvite | null>(null);
  const [dismissedPendingInvite, setDismissedPendingInvite] = useState(false);
  const [joiningPending, setJoiningPending] = useState(false);
  const [pendingJoinErr, setPendingJoinErr] = useState("");
  const [joinedToast, setJoinedToast] = useState<string | null>(null);

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

  useEffect(() => {
    if (!user) { setPendingInvite(null); return; }
    let cancelled = false;
    api.getPendingInvite()
      .then(invite => { if (!cancelled) setPendingInvite(invite); })
      .catch(() => { if (!cancelled) setPendingInvite(null); });
    return () => { cancelled = true; };
  }, [user?.id]);

  const handleJoinPending = async () => {
    if (!pendingInvite) return;
    setJoiningPending(true); setPendingJoinErr("");
    try {
      const familyName = pendingInvite.familyName;
      const familyId = await api.joinFamilyByCode(pendingInvite.code);

      // Wait for the fresh family list here (rather than just bumping `refresh` and hoping
      // the background effect catches up) so the modal only closes once the newly-joined
      // family is actually visible — otherwise, under a slow/cold-started API, the user sees
      // the modal vanish with nothing else changing and no sense of whether it worked.
      const list = await api.getMyFamilies();
      setFamilies(list);
      setActiveFamilyId(familyId);
      setTab("fam");
      setPendingInvite(null);
      setJoinedToast(`You've joined ${familyName}!`);
      setTimeout(() => setJoinedToast(null), 4000);
    } catch (ex: unknown) {
      setPendingJoinErr(ex instanceof Error ? ex.message : "Failed to join family.");
    }
    setJoiningPending(false);
  };

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
    <>
      <Shell
        user={user}
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
        ) : tab === "custody" ? (
          <CustodyPage
            user={user}
            family={activeFamily}
            refresh={refresh}
            onEventsChanged={onEventsChanged}
          />
        ) : tab === "expenses" ? (
          <ExpensesPage
            user={user}
            family={activeFamily}
            refresh={refresh}
            onEventsChanged={onEventsChanged}
          />
        ) : tab === "messages" ? (
          <MessagesPage
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

      {pendingInvite && !dismissedPendingInvite && (
        <div className="ov" onClick={e => e.target === e.currentTarget && !joiningPending && setDismissedPendingInvite(true)}>
          <div className="sh">
            <div className="shdrag" />
            <div className="shhead">
              <div className="shtitle">You're Invited!</div>
              <button
                type="button"
                className="shclose"
                onClick={() => setDismissedPendingInvite(true)}
                disabled={joiningPending}
                aria-label="Close"
              >
                <Ico d={Icons.x} size={15} />
              </button>
            </div>
            <p style={{ fontSize: 14, color: "var(--text)", lineHeight: 1.6, marginBottom: 20 }}>
              You've been invited to join <strong>{pendingInvite.familyName}</strong> as a co-parent on coparenthub.
            </p>
            {pendingJoinErr && <div className="err">{pendingJoinErr}</div>}
            <div style={{ display: "flex", gap: 10 }}>
              <button
                type="button"
                className="btn btn-o"
                onClick={() => setDismissedPendingInvite(true)}
                disabled={joiningPending}
                style={{ flex: 1 }}
              >
                Maybe Later
              </button>
              <button
                type="button"
                className="btn btn-p"
                onClick={handleJoinPending}
                disabled={joiningPending}
                style={{ flex: 1 }}
              >
                {joiningPending ? <Spinner /> : "Join Now"}
              </button>
            </div>
          </div>
        </div>
      )}

      {joinedToast && <div className="toast">✓ {joinedToast}</div>}

      {(!pendingInvite || dismissedPendingInvite) && <PushEnablePrompt userId={user.id} />}
    </>
  );
};

const AppContent: FC = () => {
  const { user, loading } = useAuth();
  const [resetToken, setResetToken] = useState<string | null>(
    () => new URLSearchParams(window.location.search).get("reset")
  );

  if (resetToken) {
    return (
      <ResetPasswordPage
        token={resetToken}
        onDone={() => {
          const url = new URL(window.location.href);
          url.searchParams.delete("reset");
          window.history.replaceState(null, "", url.toString());
          setResetToken(null);
        }}
      />
    );
  }

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
