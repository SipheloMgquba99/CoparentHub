import { useState, type FC, type FormEvent } from "react";
import { Ico, Icons } from "../../components/icons";
import { Spinner } from "../../components/ui";
import * as api from "../../api";

interface ResetPasswordPageProps {
  token: string;
  onDone: () => void;
}

const ResetPasswordPage: FC<ResetPasswordPageProps> = ({ token, onDone }) => {
  const [pw, setPw] = useState("");
  const [cpw, setCpw] = useState("");
  const [showPw, setShowPw] = useState(false);
  const [showCpw, setShowCpw] = useState(false);
  const [err, setErr] = useState("");
  const [busy, setBusy] = useState(false);
  const [done, setDone] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErr(""); setBusy(true);
    try {
      if (pw !== cpw) throw new Error("Passwords do not match.");
      await api.resetPassword(token, pw, cpw);
      setDone(true);
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Something went wrong.");
    }
    setBusy(false);
  };

  return (
    <div className="ascreen">
      <div className="ahero">
        <div className="alogo">Coparent<em>Hub</em></div>
        <div className="atag">Coordinating family, together.</div>
      </div>

      <div className="abody">
        <div className="atitle">Reset your password</div>
        <div className="asub">{done ? "All set!" : "Choose a new password below."}</div>

        {done ? (
          <>
            <p style={{ fontSize: 14, color: "var(--text)", lineHeight: 1.6, margin: "16px 0 20px" }}>
              Your password has been updated. You can now sign in with your new password.
            </p>
            <button className="btn btn-p" onClick={onDone}>Continue to Sign In</button>
          </>
        ) : (
          <form onSubmit={handleSubmit} noValidate style={{ marginTop: 16 }}>
            {err && <div className="err" role="alert">{err}</div>}

            <div className="f">
              <label htmlFor="new-pw">New Password</label>
              <div className="fw">
                <input
                  id="new-pw"
                  type={showPw ? "text" : "password"}
                  autoComplete="new-password"
                  minLength={8}
                  maxLength={128}
                  value={pw}
                  onChange={e => setPw(e.target.value)}
                  placeholder="••••••••"
                  required
                  autoFocus
                  style={{ paddingRight: 42 }}
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
              <div style={{ fontSize: 11.5, color: "var(--muted)", marginTop: 6 }}>At least 8 characters.</div>
            </div>

            <div className="f">
              <label htmlFor="new-cpw">Confirm New Password</label>
              <div className="fw">
                <input
                  id="new-cpw"
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

            <button className="btn btn-p" type="submit" disabled={busy} style={{ marginTop: 10 }}>
              {busy ? <Spinner /> : "Reset Password"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};

export default ResetPasswordPage;
