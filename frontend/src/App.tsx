import { useState, useEffect, type FC } from "react";
import type { Family } from "./types";
import { CSS } from "./styles/global";
import { Shell } from "./components/layout";
import { ThemeProvider, useTheme } from "./context/ThemeContext";
import { AuthProvider, useAuth } from "./context/AuthContext";

import HomePage from "./pages/home/HomePage";
import SchedPage from "./pages/schedule/SchedulePage";
import FamPage from "./pages/family/FamilyPage";
import AuthPage from "./pages/auth/AuthPage";
import * as api from "./api";

const Inner: FC = () => {
  const { theme } = useTheme();
  const { logout, user } = useAuth();

  const [tab, setTab] = useState<string>("home");
  const [refresh, setRefresh] = useState<number>(0);
  const [family, setFamily] = useState<Family | null>(null);
  const [loadingFamily, setLoadingFamily] = useState<boolean>(false);

  const onEventsChanged = () => setRefresh((n) => n + 1);
  const onFamChange = () => setRefresh((n) => n + 1);

  useEffect(() => {
    if (!user?.familyId) {
      setFamily(null);
      setLoadingFamily(false);
      return;
    }

    let isMounted = true; 
    setLoadingFamily(true);

    api.getFamily(user.familyId)
      .then((f) => {
        if (isMounted) setFamily(f);
      })
      .catch(() => {
        if (isMounted) setFamily(null);
      })
      .finally(() => {
        if (isMounted) setLoadingFamily(false);
      });

    return () => {
      isMounted = false;
    };
  }, [user?.familyId, refresh]);

  if (!user) return null;

  return (
    <div className={`app theme-${theme}`}>
      <Shell
        user={user.fullName}
        tab={tab}
        setTab={setTab}
        onLogout={logout}
      >
        {loadingFamily ? (
          <div style={{ padding: 20 }}>Loading family...</div>
        ) : tab === "home" ? (
          <HomePage
            user={user}
            family={family}
            setTab={setTab}
            refresh={refresh}
            onEventsChanged={onEventsChanged}
          />
        ) : tab === "sched" ? (
          <SchedPage
            user={user}
            family={family}
            refresh={refresh}
            onEventsChanged={onEventsChanged}
          />
        ) : tab === "fam" ? (
          <FamPage
            user={user}
            family={family}
            onFamChange={onFamChange}
          />
        ) : null}
      </Shell>
    </div>
  );
};

const AppContent: FC = () => {
  const { user, loading } = useAuth();

  if (loading) {
    return <div style={{ padding: 20 }}>Loading...</div>;
  }

  if (!user) {
    return <AuthPage />;
  }

  return <Inner />;
};
export default function App() {
  return (
    <>
      <style>{CSS}</style>
      <ThemeProvider>
        <AuthProvider>
          <AppContent />
        </AuthProvider>
      </ThemeProvider>
    </>
  );
}