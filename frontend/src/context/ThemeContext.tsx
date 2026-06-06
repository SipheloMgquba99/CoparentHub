import { createContext, useContext, useState, useCallback, type FC, type ReactNode } from "react";

export type Theme = "light" | "dark" | "navy";

interface ThemeContextValue {
  theme: Theme;
  setTheme: (t: Theme) => void;
  cycleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextValue | null>(null);

const CYCLE: Theme[] = ["light", "dark", "navy"];

export const useTheme = (): ThemeContextValue => {
  const ctx = useContext(ThemeContext);
  if (!ctx) throw new Error("useTheme must be used inside <ThemeProvider>");
  return ctx;
};

export const ThemeProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const stored = (localStorage.getItem("cp_theme") as Theme) ?? "light";
  const [theme, setThemeState] = useState<Theme>(stored);

  const setTheme = useCallback((t: Theme) => {
    setThemeState(t);
    localStorage.setItem("cp_theme", t);
  }, []);

  const cycleTheme = useCallback(() => {
    setTheme(CYCLE[(CYCLE.indexOf(theme) + 1) % CYCLE.length]);
  }, [theme, setTheme]);

  return (
    <ThemeContext.Provider value={{ theme, setTheme, cycleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};
