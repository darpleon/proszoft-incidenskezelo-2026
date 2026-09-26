import type { ReactNode } from "react";

type DetailFieldProps = {
  label: string;
  children: ReactNode;
};

export function DetailField({ label, children }: DetailFieldProps) {
  return (
    <div className="relative mr-[22px] pr-[22px] after:absolute after:inset-y-[3px] after:right-0 after:w-px after:bg-line last:mr-0 last:pr-0 last:after:hidden">
      <span className="mb-[3px] block text-[11.5px] text-text-3">{label}</span>
      <div className="flex items-center gap-[7px] py-[3px] text-sm font-semibold tracking-[-0.015em]">{children}</div>
    </div>
  );
}
