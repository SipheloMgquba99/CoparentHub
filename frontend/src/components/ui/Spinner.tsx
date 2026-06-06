import type{ FC } from "react";

export const Spinner: FC<{ dark?: boolean }> = ({ dark }) => (
  <span className={`spin ${dark ? "spind" : ""}`} />
);

export const PageSpinner: FC = () => (
  <div style={{ display: "flex", justifyContent: "center", padding: 40 }}>
    <Spinner dark />
  </div>
);
