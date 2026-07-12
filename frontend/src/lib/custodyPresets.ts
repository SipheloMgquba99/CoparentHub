export type CustodyPreset = "weekOnWeekOff" | "twoTwoThree" | "alternatingWeekends" | "custom";

export interface CustodyPattern {
  cycleLengthDays: number;
  dayPattern: string;
}

export const CUSTODY_PRESETS: { id: CustodyPreset; label: string }[] = [
  { id: "weekOnWeekOff", label: "Week On / Week Off" },
  { id: "twoTwoThree", label: "2-2-3" },
  { id: "alternatingWeekends", label: "Weekdays + Alternating Weekends" },
  { id: "custom", label: "Custom" },
];

export function weekOnWeekOff(): CustodyPattern {
  return { cycleLengthDays: 14, dayPattern: "A".repeat(7) + "B".repeat(7) };
}

export function twoTwoThree(): CustodyPattern {
  return { cycleLengthDays: 14, dayPattern: "AABBAAABBAABBB" };
}

export function alternatingWeekends(): CustodyPattern {
  return { cycleLengthDays: 14, dayPattern: "AAAA" + "BBB" + "AAAA" + "AAA" };
}

export function patternForPreset(preset: CustodyPreset): CustodyPattern | null {
  switch (preset) {
    case "weekOnWeekOff": return weekOnWeekOff();
    case "twoTwoThree": return twoTwoThree();
    case "alternatingWeekends": return alternatingWeekends();
    case "custom": return null;
  }
}
