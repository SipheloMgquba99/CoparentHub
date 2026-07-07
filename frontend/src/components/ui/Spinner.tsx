import type{ FC } from "react";

export const Spinner: FC<{ dark?: boolean }> = ({ dark }) => (
  <span className={`spin ${dark ? "spind" : ""}`} />
);

export const PageSpinner: FC = () => (
  <div className="page-spinner">
    <Spinner dark />
  </div>
);
