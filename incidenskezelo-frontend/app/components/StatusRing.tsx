type StatusRingProps = {
  colorClass: string;
  filled?: boolean;
};

export function StatusRing({ colorClass, filled = true }: StatusRingProps) {
  return (
    <i
      className={`size-[9px] shrink-0 rounded-full border-current ${filled ? "border-[4.5px]" : "border-[2.5px]"} ${colorClass}`}
    />
  );
}
