import type { FC } from "react";

export interface IcoProps {
  d: string | readonly string[];
  size?: number;
  stroke?: string;
  fill?: string;
  sw?: number;
}

export const Ico: FC<IcoProps> = ({ d, size = 20, stroke = "currentColor", fill = "none", sw = 1.8 }) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill={fill} stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round">
    {(Array.isArray(d) ? d : [d]).map((p, i) => <path key={i} d={p} />)}
  </svg>
);
