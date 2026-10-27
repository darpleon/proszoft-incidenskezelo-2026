import { Checkbox, TextInput, UnstyledButton } from "@mantine/core";
import { useState } from "react";
import { EmptyState } from "./EmptyState";
import { OptionalLabel } from "./OptionalLabel";
import type { IEvent } from "~/interfaces/IEvent";
import { formatTime } from "~/lib/format";
import { eventTypeLabels, severityLabels, severityTextColors } from "~/lib/labels";

type EventPickerProps = {
  events: IEvent[];
  selectedIds: number[];
  onChange: (selectedIds: number[]) => void;
  loadError?: boolean;
};

function matchesSearch(event: IEvent, search: string) {
  const text = search.trim().toLowerCase();
  if (!text) return true;

  return `evt-${event.eventId}`.includes(text) || event.serviceCode.includes(text);
}

export function EventPicker({ events, selectedIds, onChange, loadError = false }: EventPickerProps) {
  const [search, setSearch] = useState("");
  const visibleEvents = events
    .filter((event) => matchesSearch(event, search))
    .sort((a, b) => b.occurredAtUtc.localeCompare(a.occurredAtUtc));

  const toggle = (eventId: number) => {
    onChange(selectedIds.includes(eventId) ? selectedIds.filter((id) => id !== eventId) : [...selectedIds, eventId]);
  };

  return (
    <div className="flex min-h-0 min-w-0 flex-col">
      <div className="mb-2 flex flex-wrap items-center gap-2.5">
        <span className="text-sm font-medium">
          <OptionalLabel>Related events</OptionalLabel>
        </span>
        <TextInput
          value={search}
          onChange={(event) => setSearch(event.currentTarget.value)}
          placeholder="Search by ID or source"
          aria-label="Search events"
          size="xs"
          className="ml-auto w-[200px] max-[680px]:ml-0 max-[680px]:w-full"
        />
      </div>

      <div className="min-h-[220px] flex-1 overflow-y-auto rounded-lg border border-line max-[1080px]:max-h-[460px] max-[1080px]:flex-none">
        <div className="sticky top-0 z-[2] border-b border-[#eff2f1] bg-panel-2 px-3 py-1.5 text-[11.5px] font-semibold text-text-2">
          Not linked to an incident <span className="font-normal text-text-3">{events.length}</span>
        </div>

        {loadError && <EmptyState>The events could not be loaded.</EmptyState>}
        {!loadError && events.length === 0 && <EmptyState>Every event is already linked to an incident.</EmptyState>}
        {events.length > 0 && visibleEvents.length === 0 && <EmptyState>No events match the search.</EmptyState>}

        {visibleEvents.map((event) => {
          const isSelected = selectedIds.includes(event.eventId);

          return (
            <label
              key={event.eventId}
              className={`flex cursor-pointer items-start gap-2.5 border-b border-[#eff2f1] px-3 py-2 ${
                isSelected ? "bg-petrol-bg" : "hover:bg-panel-2"
              }`}
            >
              <Checkbox checked={isSelected} onChange={() => toggle(event.eventId)} color="#0f717d" size="xs" mt={3} />
              <span className="min-w-0 flex-1">
                <span className="flex items-baseline gap-2 text-[13px]">
                  <span className="font-semibold">{eventTypeLabels[event.eventType] ?? event.eventType}</span>
                  <span className={`text-[11.5px] font-semibold ${severityTextColors[event.severity]}`}>
                    {severityLabels[event.severity]}
                  </span>
                  <span className="ml-auto text-[11.5px] whitespace-nowrap text-text-3">
                    EVT-{event.eventId} · {formatTime(event.occurredAtUtc)}
                  </span>
                </span>
                <span className="mt-0.5 flex gap-2 text-[11.5px]">
                  <span className="font-medium">{event.serviceCode}</span>
                  {event.summary && <span className="truncate text-text-3">{event.summary}</span>}
                </span>
              </span>
            </label>
          );
        })}
      </div>

      <div className="mt-2 flex items-center gap-2 text-[11.5px] text-text-3">
        {selectedIds.length === 0 ? (
          "No events selected"
        ) : (
          <>
            <span>
              <b className="font-semibold text-text">{selectedIds.length}</b> selected
            </span>
            <UnstyledButton onClick={() => onChange([])} className="text-[11.5px] font-medium text-petrol">
              Clear
            </UnstyledButton>
          </>
        )}
      </div>
    </div>
  );
}
