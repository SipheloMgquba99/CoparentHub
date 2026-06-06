import { useState, type FC, type FormEvent } from "react";
import { useAuth } from "../../context/AuthContext";
import { Ico, Icons } from "../../components/icons";
import { Spinner } from "../../components/ui";

const AuthPage: FC = () => {
  const { login, register } = useAuth();

  const [tab,    setTab]    = useState<"login" | "register">("login");
  const [email,  setEmail]  = useState("");
  const [pw,     setPw]     = useState("");
  const [cpw,    setCpw]    = useState("");
  const [fn,     setFn]     = useState("");
  const [ln,     setLn]     = useState("");
  const [showPw, setShowPw] = useState(false);
  const [err,    setErr]    = useState("");
  const [busy,   setBusy]   = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErr(""); setBusy(true);
    try {
      if (tab === "login") {
        await login({ email, password: pw });
      } else {
        if (pw !== cpw) throw new Error("Passwords do not match.");
        await register({ firstName: fn, lastName: ln, email, password: pw, confirmPassword: cpw });
      }
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Something went wrong.");
    }
    setBusy(false);
  };

  const switchTab = (t: "login" | "register") => { setTab(t); setErr(""); };

  return (
    <div className="ascreen">
      <div className="ahero">
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

        {err && <div className="err">{err}</div>}

        <form onSubmit={handleSubmit}>
          {tab === "register" && (
            <div className="frow">
              <div className="f">
                <label>First Name</label>
                <input value={fn} onChange={e => setFn(e.target.value)} placeholder="Jane" required />
              </div>
              <div className="f">
                <label>Last Name</label>
                <input value={ln} onChange={e => setLn(e.target.value)} placeholder="Smith" required />
              </div>
            </div>
          )}

          <div className="f">
            <label>Email</label>
            <input type="email" value={email} onChange={e => setEmail(e.target.value)} placeholder="you@example.com" required />
          </div>

          <div className="f">
            <label>Password</label>
            <div className="fw">
              <input
                type={showPw ? "text" : "password"}
                value={pw}
                onChange={e => setPw(e.target.value)}
                placeholder="••••••••"
                required
                style={{ paddingRight: 42 }}
              />
              <button type="button" className="ficon" onClick={() => setShowPw(s => !s)}>
                <Ico d={showPw ? Icons.eyeX : Icons.eye} size={16} />
              </button>
            </div>
          </div>

          {tab === "register" && (
            <div className="f">
              <label>Confirm Password</label>
              <input type="password" value={cpw} onChange={e => setCpw(e.target.value)} placeholder="••••••••" required />
            </div>
          )}

          <button className="btn btn-p" type="submit" disabled={busy} style={{ marginTop: 10 }}>
            {busy ? <Spinner /> : tab === "login" ? "Sign In" : "Create Account"}
          </button>
        </form>
      </div>
    </div>
  );
};

export default AuthPage;
