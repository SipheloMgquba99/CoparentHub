import { useState, type FC, type FormEvent } from "react";
import { useAuth } from "../../context/useAuth";
import { useTheme } from "../../context/useTheme";
import { Ico, Icons } from "../../components/icons";
import { Spinner } from "../../components/ui";
import * as api from "../../api";

const THEME_ICON = { light: "☀️", dark: "🌙", navy: "🌊" };

const AuthPage: FC = () => {
  const { login, register } = useAuth();
  const { theme, cycleTheme } = useTheme();

  const [tab,    setTab]    = useState<"login" | "register">("login");
  const [email,  setEmail]  = useState("");
  const [pw,     setPw]     = useState("");
  const [cpw,    setCpw]    = useState("");
  const [fn,     setFn]     = useState("");
  const [ln,     setLn]     = useState("");
  const [showPw, setShowPw] = useState(false);
  const [showCpw, setShowCpw] = useState(false);
  const [agreed, setAgreed] = useState(false);
  const [err,    setErr]    = useState("");
  const [busy,   setBusy]   = useState(false);

  const [showForgot, setShowForgot] = useState(false);
  const [forgotEmail, setForgotEmail] = useState("");
  const [sendingForgot, setSendingForgot] = useState(false);
  const [forgotSent, setForgotSent] = useState(false);
  const [forgotErr, setForgotErr] = useState("");

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErr(""); setBusy(true);
    try {
      if (tab === "login") {
        await login({ email, password: pw });
      } else {
        if (pw !== cpw) throw new Error("Passwords do not match.");
        if (!agreed) throw new Error("Please accept the Privacy Policy and Terms to continue.");
        await register({ firstName: fn, lastName: ln, email, password: pw, confirmPassword: cpw });
      }
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Something went wrong.");
    }
    setBusy(false);
  };

  const switchTab = (t: "login" | "register") => { setTab(t); setErr(""); };

  const openForgot = () => {
    setForgotEmail(email);
    setForgotErr("");
    setForgotSent(false);
    setShowForgot(true);
  };

  const handleForgotSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSendingForgot(true); setForgotErr("");
    try {
      await api.forgotPassword(forgotEmail);
      setForgotSent(true);
    } catch (ex: unknown) {
      setForgotErr(ex instanceof Error ? ex.message : "Something went wrong.");
    }
    setSendingForgot(false);
  };

  return (
    <div className="ascreen">
      <div className="ahero">
        <button
          type="button"
          className="theme-btn"
          onClick={cycleTheme}
          aria-label={`Switch theme (current: ${theme})`}
          title={`Switch theme (current: ${theme})`}
          style={{ position: "absolute", top: 20, right: 20, zIndex: 2 }}
        >
          {THEME_ICON[theme]}
        </button>
        <div className="alogo">Coparent<em>Hub</em></div>
        <div className="atag">Coordinating family, together.</div>
      </div>

      <div className="abody">
        <div className="atabs">
          <button className={`atab ${tab === "login" ? "on" : ""}`} onClick={() => switchTab("login")}>Sign In</button>
          <button className={`atab ${tab === "register" ? "on" : ""}`} onClick={() => switchTab("register")}>Create Account</button>
        </div>

        <div className="atitle">{tab === "login" ? "Welcome back" : "Get started"}</div>
        <div className="asub">{tab === "login" ? "Sign in to your account" : "Create your free account"}</div>

        {err && <div className="err" role="alert">{err}</div>}

        <form onSubmit={handleSubmit} noValidate>
          {tab === "register" && (
            <div className="frow">
              <div className="f">
                <label htmlFor="fn">First Name</label>
                <input id="fn" name="given-name" autoComplete="given-name" maxLength={100} value={fn} onChange={e => setFn(e.target.value)} placeholder="Jane" required />
              </div>
              <div className="f">
                <label htmlFor="ln">Last Name</label>
                <input id="ln" name="family-name" autoComplete="family-name" maxLength={100} value={ln} onChange={e => setLn(e.target.value)} placeholder="Smith" required />
              </div>
            </div>
          )}

          <div className="f">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              maxLength={256}
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
            />
          </div>

          <div className="f">
            <label htmlFor="password">Password</label>
            <div className="fw">
              <input
                id="password"
                name={tab === "login" ? "current-password" : "new-password"}
                type={showPw ? "text" : "password"}
                autoComplete={tab === "login" ? "current-password" : "new-password"}
                minLength={8}
                maxLength={128}
                value={pw}
                onChange={e => setPw(e.target.value)}
                placeholder="••••••••"
                required
                style={{ paddingRight: 42 }}
                aria-describedby={tab === "register" ? "pw-hint" : undefined}
              />
              <button
                type="button"
                className="ficon"
                onClick={() => setShowPw(s => !s)}
                aria-label={showPw ? "Hide password" : "Show password"}
                title={showPw ? "Hide password" : "Show password"}
              >
                <Ico d={showPw ? Icons.eyeX : Icons.eye} size={16} />
              </button>
            </div>
            {tab === "register" && (
              <div id="pw-hint" style={{ fontSize: 11.5, color: "var(--muted)", marginTop: 6 }}>
                At least 8 characters.
              </div>
            )}
            {tab === "login" && (
              <button
                type="button"
                onClick={openForgot}
                style={{
                  background: "none", border: "none", cursor: "pointer", padding: 0,
                  marginTop: 8, fontSize: 12.5, color: "var(--accent)", fontWeight: 600,
                }}
              >
                Forgot password?
              </button>
            )}
          </div>

          {tab === "register" && (
            <div className="f">
              <label htmlFor="cpw">Confirm Password</label>
              <div className="fw">
                <input
                  id="cpw"
                  name="new-password"
                  type={showCpw ? "text" : "password"}
                  autoComplete="new-password"
                  minLength={8}
                  maxLength={128}
                  value={cpw}
                  onChange={e => setCpw(e.target.value)}
                  placeholder="••••••••"
                  required
                  style={{ paddingRight: 42 }}
                />
                <button
                  type="button"
                  className="ficon"
                  onClick={() => setShowCpw(s => !s)}
                  aria-label={showCpw ? "Hide password" : "Show password"}
                  title={showCpw ? "Hide password" : "Show password"}
                >
                  <Ico d={showCpw ? Icons.eyeX : Icons.eye} size={16} />
                </button>
              </div>
            </div>
          )}

          {tab === "register" && (
            <label style={{ display: "flex", alignItems: "flex-start", gap: 8, fontSize: 12.5, color: "var(--text-2)", marginBottom: 16, cursor: "pointer", lineHeight: 1.5 }}>
              <input
                type="checkbox"
                checked={agreed}
                onChange={e => setAgreed(e.target.checked)}
                style={{ marginTop: 2, width: 16, height: 16, flexShrink: 0 }}
                required
              />
              <span>
                I agree to the Privacy Policy and Terms of Service, and understand this app
                is used to coordinate care for my family, including my children's schedule information.
              </span>
            </label>
          )}

          <button className="btn btn-p" type="submit" disabled={busy} style={{ marginTop: 10 }}>
            {busy ? <Spinner /> : tab === "login" ? "Sign In" : "Create Account"}
          </button>
        </form>
      </div>

      {showForgot && (
        <div className="ov" onClick={e => e.target === e.currentTarget && !sendingForgot && setShowForgot(false)}>
          <div className="sh">
            <div className="shdrag" />
            <div className="shhead">
              <div className="shtitle">Reset Password</div>
              <button type="button" className="shclose" onClick={() => setShowForgot(false)} disabled={sendingForgot} aria-label="Close">
                <Ico d={Icons.x} size={15} />
              </button>
            </div>

            {forgotSent ? (
              <p style={{ fontSize: 14, color: "var(--text)", lineHeight: 1.6 }}>
                If an account exists for <strong>{forgotEmail}</strong>, we've sent a link to reset your password.
              </p>
            ) : (
              <form onSubmit={handleForgotSubmit}>
                <p style={{ fontSize: 13, color: "var(--text-2)", lineHeight: 1.6, marginBottom: 16 }}>
                  Enter your account email and we'll send you a link to reset your password.
                </p>
                <div className="f">
                  <label htmlFor="forgot-email">Email</label>
                  <input
                    id="forgot-email"
                    type="email"
                    autoComplete="email"
                    maxLength={256}
                    value={forgotEmail}
                    onChange={e => setForgotEmail(e.target.value)}
                    placeholder="you@example.com"
                    required
                    autoFocus
                  />
                </div>
                {forgotErr && <div className="err">{forgotErr}</div>}
                <button className="btn btn-p" type="submit" disabled={sendingForgot}>
                  {sendingForgot ? <Spinner /> : "Send Reset Link"}
                </button>
              </form>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default AuthPage;
