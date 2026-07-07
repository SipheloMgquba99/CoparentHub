import { useState, useEffect, useCallback } from "react";
import * as api from "../api";

const dismissedKey = (userId: string) => `cp_push_prompt_dismissed:${userId}`;

const isIOS = (): boolean => /iphone|ipad|ipod/i.test(navigator.userAgent);

const isStandalone = (): boolean =>
  window.matchMedia?.("(display-mode: standalone)").matches === true ||
  (navigator as unknown as { standalone?: boolean }).standalone === true;

function urlBase64ToUint8Array(base64String: string): Uint8Array {
  const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding).replace(/-/g, "+").replace(/_/g, "/");
  const rawData = atob(base64);
  const bytes = new Uint8Array(rawData.length);
  for (let i = 0; i < rawData.length; i++) bytes[i] = rawData.charCodeAt(i);
  return bytes;
}

// Only iOS Safari requires "Add to Home Screen" before push works at all — Android/desktop
// Chrome and Firefox support Web Push in a regular browser tab, no install needed.
export type PushPromptState =
  | "unsupported"
  | "unavailable"
  | "ios-not-installed"
  | "denied"
  | "ready-to-enable"
  | "enabled"
  | "dismissed";

interface UsePushSetupResult {
  state: PushPromptState;
  busy: boolean;
  error: string;
  enable: () => Promise<void>;
  dismiss: () => void;
}

export function usePushSetup(userId: string | null): UsePushSetupResult {
  const [state, setState] = useState<PushPromptState>("unsupported");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [vapidKey, setVapidKey] = useState<string | null>(null);

  useEffect(() => {
    if (!userId) return;
    let cancelled = false;

    (async () => {
      const supported =
        "serviceWorker" in navigator && "PushManager" in window && "Notification" in window;

      if (!supported) { if (!cancelled) setState("unsupported"); return; }
      if (localStorage.getItem(dismissedKey(userId)) === "1") { if (!cancelled) setState("dismissed"); return; }
      if (Notification.permission === "denied") { if (!cancelled) setState("denied"); return; }

      let key: string;
      try {
        key = await api.getVapidPublicKey();
      } catch {
        if (!cancelled) setState("unavailable");
        return;
      }
      if (cancelled) return;
      setVapidKey(key);

      if (isIOS() && !isStandalone()) { setState("ios-not-installed"); return; }

      if (Notification.permission === "granted") {
        const registration = await navigator.serviceWorker.ready;
        const existing = await registration.pushManager.getSubscription();
        if (!cancelled) setState(existing ? "enabled" : "ready-to-enable");
        return;
      }

      setState("ready-to-enable");
    })();

    return () => { cancelled = true; };
  }, [userId]);

  const enable = useCallback(async () => {
    if (!userId || !vapidKey) return;
    setBusy(true); setError("");
    try {
      const permission = await Notification.requestPermission();
      if (permission !== "granted") {
        setState(permission === "denied" ? "denied" : "ready-to-enable");
        return;
      }

      const registration = await navigator.serviceWorker.ready;
      let subscription = await registration.pushManager.getSubscription();
      if (!subscription) {
        subscription = await registration.pushManager.subscribe({
          userVisibleOnly: true,
          applicationServerKey: urlBase64ToUint8Array(vapidKey) as unknown as BufferSource,
        });
      }

      const json = subscription.toJSON();
      await api.subscribePush({
        endpoint: json.endpoint!,
        keys: { p256dh: json.keys!.p256dh!, auth: json.keys!.auth! },
      });

      setState("enabled");
    } catch (ex: unknown) {
      setError(ex instanceof Error ? ex.message : "Failed to enable notifications.");
    }
    setBusy(false);
  }, [userId, vapidKey]);

  const dismiss = useCallback(() => {
    if (userId) localStorage.setItem(dismissedKey(userId), "1");
    setState("dismissed");
  }, [userId]);

  return { state, busy, error, enable, dismiss };
}
