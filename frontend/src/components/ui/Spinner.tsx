import type{ FC } from "react";

export const Spinner: FC<{ dark?: boolean }> = ({ dark }) => (
  <span className={`spin ${dark ? "spind" : ""}`} />
);

export const PageSpinner: FC<{ label?: string }> = ({ label }) => (
  <div className="page-spinner">
    <Spinner dark />
    {label && <div className="page-spinner-label">{label}</div>}
  </div>
);
