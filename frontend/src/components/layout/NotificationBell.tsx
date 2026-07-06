import { useState, useRef, useEffect, type FC } from "react";
import type { AppNotification } from "../../types";
import { Ico, Icons } from "../icons";
import { timeAgo } from "../../lib/utils";

interface NotificationBellProps {
  notifications: AppNotification[];
  onMarkRead: (id: string) => void;
}

export const NotificationBell: FC<NotificationBellProps> = ({ notifications, onMarkRead }) => {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const unreadCount = notifications.filter(n => !n.isRead).length;

  useEffect(() => {
    if (!open) return;
    const onClickOutside = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, [open]);

  return (
    <div ref={ref} className="nbell-wrap">
      <button
        type="button"
        className="theme-btn nbell-btn"
        onClick={() => setOpen(o => !o)}
        aria-label={unreadCount > 0 ? `Notifications (${unreadCount} unread)` : "Notifications"}
        title="Notifications"
      >
        <Ico d={Icons.bell} size={17} />
        {unreadCount > 0 && (
          <span className="nbell-badge" aria-hidden="true">
            {unreadCount > 9 ? "9+" : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div role="menu" className="nbell-menu">
          <div className="nbell-hd">Notifications</div>

          {notifications.length === 0 ? (
            <div className="nbell-empty">No notifications yet</div>
          ) : (
            notifications.map(n => (
              <button
                key={n.id}
                type="button"
                role="menuitem"
                onClick={() => !n.isRead && onMarkRead(n.id)}
                className={`nbell-item ${n.isRead ? "" : "unread"}`}
              >
                <div className="nbell-row">
                  {!n.isRead && <span className="nbell-dot" />}
                  <div className="nbell-col">
                    <div className="nbell-msg">{n.message}</div>
                    <div className="nbell-time">{timeAgo(n.createdAt)}</div>
                  </div>
                </div>
              </button>
            ))
          )}
        </div>
      )}
    </div>
  );
};
