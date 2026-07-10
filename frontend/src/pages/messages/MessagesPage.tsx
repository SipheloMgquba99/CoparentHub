import { useState, useEffect, useRef, type FC, type FormEvent } from "react";
import type { User, Family, Message } from "../../types";
import * as api from "../../api";
import { Spinner, PageSpinner } from "../../components/ui";
import { timeAgo } from "../../lib/utils";

interface MessagesPageProps {
  user: User;
  family: Family | null;
  refresh: number;
  onEventsChanged: () => void;
}

const MESSAGES_POLL_MS = 15_000;

const MessagesPage: FC<MessagesPageProps> = ({ user, family, refresh, onEventsChanged }) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [busy, setBusy] = useState(true);
  const [body, setBody] = useState("");
  const [sending, setSending] = useState(false);
  const [err, setErr] = useState("");
  const [tick, setTick] = useState(0);
  const silentRef = useRef(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  const familyFull = (family?.members.length ?? 0) >= 2;

  useEffect(() => {
    if (!family || !familyFull) { setBusy(false); return; }
    if (!silentRef.current) setBusy(true);
    api.getMessages(family.id)
      .then(setMessages)
      .catch(() => setMessages([]))
      .finally(() => {
        setBusy(false);
        silentRef.current = false;
      });
  }, [family?.id, familyFull, refresh, tick]);

  useEffect(() => {
    if (!family || !familyFull) return;
    const id = setInterval(() => {
      silentRef.current = true;
      setTick(t => t + 1);
    }, MESSAGES_POLL_MS);
    return () => clearInterval(id);
  }, [family?.id, familyFull]);

  useEffect(() => {
    if (!family || !familyFull) return;
    const markRead = () => api.markThreadRead(family.id).then(onEventsChanged).catch(() => {});
    markRead();
    window.addEventListener("focus", markRead);
    return () => window.removeEventListener("focus", markRead);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [family?.id, familyFull]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages.length]);

  const handleSend = async (e: FormEvent) => {
    e.preventDefault();
    if (!family || !body.trim()) return;
    setErr(""); setSending(true);
    try {
      await api.sendMessage(family.id, body.trim());
      setBody("");
      setTick(t => t + 1);
      onEventsChanged();
    } catch (ex: unknown) {
      setErr(ex instanceof Error ? ex.message : "Failed to send message.");
    }
    setSending(false);
  };

  return (
    <>
      <div className="page-title">Messages</div>

      {!family ? (
        <div className="empty" style={{ paddingTop: 40 }}>Join or create a family to send messages.</div>
      ) : !familyFull ? (
        <div className="empty" style={{ paddingTop: 40 }}>Invite a co-parent before sending messages.</div>
      ) : busy ? (
        <PageSpinner />
      ) : (
        <>
          <div style={{ fontSize: 12, color: "var(--muted)", textAlign: "center", marginBottom: 16, lineHeight: 1.5 }}>
            Messages here are permanent and can't be edited or deleted.
          </div>

          <div className="card">
            {messages.length === 0 && <div className="empty">No messages yet — say hello!</div>}
            {messages.map(m => {
              const mine = m.senderUserId === user.id;
              return (
                <div key={m.id} style={{ display: "flex", justifyContent: mine ? "flex-end" : "flex-start", marginBottom: 12 }}>
                  <div style={{ maxWidth: "78%" }}>
                    <div
                      style={{
                        background: mine ? "var(--btn-p-bg)" : "var(--surface-2)",
                        color: mine ? "var(--btn-p-text)" : "var(--text)",
                        borderRadius: 14,
                        borderBottomRightRadius: mine ? 4 : 14,
                        borderBottomLeftRadius: mine ? 14 : 4,
                        padding: "10px 14px",
                        fontSize: 14.5,
                        lineHeight: 1.5,
                        wordBreak: "break-word",
                      }}
                    >
                      {m.body}
                    </div>
                    <div style={{ fontSize: 11, color: "var(--muted)", marginTop: 4, textAlign: mine ? "right" : "left" }}>
                      {mine ? "You" : m.senderName} · {timeAgo(m.createdAt)}
                    </div>
                  </div>
                </div>
              );
            })}
            <div ref={bottomRef} />
          </div>

          {err && <div className="err">{err}</div>}
          <form onSubmit={handleSend} style={{ display: "flex", gap: 8 }}>
            <input
              value={body}
              onChange={e => setBody(e.target.value)}
              placeholder="Type a message…"
              maxLength={2000}
              disabled={sending}
              style={{
                flex: 1, padding: "13px 16px", border: "1.5px solid var(--border)", borderRadius: "var(--rm)",
                fontSize: 15, fontFamily: "'Outfit', sans-serif", background: "var(--input-bg)", color: "var(--text)",
              }}
            />
            <button className="btn btn-p" type="submit" disabled={sending || !body.trim()} style={{ width: "auto", padding: "0 20px" }}>
              {sending ? <Spinner /> : "Send"}
            </button>
          </form>
        </>
      )}
    </>
  );
};

export default MessagesPage;
