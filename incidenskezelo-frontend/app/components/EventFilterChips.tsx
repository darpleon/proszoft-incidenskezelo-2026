export type EventFilter = "all" | "bound" | "unbound" | "duplicate" | "failed";

type EventFilterChipsProps = {
  value: EventFilter;
  counts: Record<EventFilter, number>;
  onChange: (value: EventFilter) => void;
};

const filterLabels: Record<EventFilter, string> = {
  all: "Mind",
  bound: "Incidenshez kötve",
  unbound: "Nincs incidenshez kötve",
  duplicate: "Elvetett duplikátum",
  failed: "Feldolgozási hiba",
};

export function EventFilterChips({ value, counts, onChange }: EventFilterChipsProps) {
  return (
    <div className="flex flex-wrap gap-[5px]">
      {(Object.keys(filterLabels) as EventFilter[]).map((filter) => (
        <button
          key={filter}
          type="button"
          aria-pressed={value === filter}
          onClick={() => onChange(filter)}
          className="flex items-center gap-[7px] rounded-full border border-line bg-panel px-[11px] py-[5px] text-[13px] text-text-2 transition-colors hover:border-[#d5dad8] hover:bg-panel-2 aria-pressed:border-text aria-pressed:bg-text aria-pressed:font-semibold aria-pressed:text-white"
        >
          {filterLabels[filter]}
          <span className="text-[11.5px] opacity-70">{counts[filter]}</span>
        </button>
      ))}
    </div>
  );
}
