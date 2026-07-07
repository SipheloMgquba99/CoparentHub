import type { FC } from "react";
import { usePushSetup } from "../hooks/usePushSetup";
import { Ico, Icons } from "./icons";
import { Spinner } from "./ui";

interface PushEnablePromptProps {
  userId: string | null;
}

export const PushEnablePrompt: FC<PushEnablePromptProps> = ({ userId }) => {
  const { state, busy, error, enable, dismiss } = usePushSetup(userId);

  if (state !== "ready-to-enable" && state !== "ios-not-installed") return null;

  return (
    <div className="ov" onClick={e => e.target === e.currentTarget && !busy && dismiss()}>
      <div className="sh">
        <div className="shdrag" />
        <div className="shhead">
          <div className="shtitle">
            {state === "ios-not-installed" ? "Enable Notifications" : "Stay in the Loop"}
          </div>
          <button type="button" className="shclose" onClick={dismiss} disabled={busy} aria-label="Close">
            <Ico d={Icons.x} size={15} />
          </button>
        </div>

        {state === "ios-not-installed" ? (
          <>
            <p style={{ fontSize: 14, color: "var(--text)", lineHeight: 1.6, marginBottom: 12 }}>
              To get notified about upcoming events on your iPhone, add coparenthub to your Home Screen first.
            </p>
            <p style={{ fontSize: 13, color: "var(--text-2)", lineHeight: 1.7, marginBottom: 20 }}>
              Tap the <strong>Share</strong> button in Safari, then choose <strong>"Add to Home Screen."</strong> Open the app from there afterward to turn on notifications.
            </p>
            <button className="btn btn-p" onClick={dismiss}>Got It</button>
          </>
        ) : (
          <>
            <p style={{ fontSize: 14, color: "var(--text)", lineHeight: 1.6, marginBottom: 20 }}>
              Get notified about upcoming events and updates — even when coparenthub isn't open.
            </p>
            {error && <div className="err">{error}</div>}
            <div style={{ display: "flex", gap: 10 }}>
              <button type="button" className="btn btn-o" onClick={dismiss} disabled={busy} style={{ flex: 1 }}>
                Not Now
              </button>
              <button type="button" className="btn btn-p" onClick={enable} disabled={busy} style={{ flex: 1 }}>
                {busy ? <Spinner /> : "Enable"}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default PushEnablePrompt;
