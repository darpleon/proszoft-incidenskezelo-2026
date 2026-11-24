import { IconSettings } from "@tabler/icons-react";
import { useState, type ReactNode } from "react";
import { EmptyState } from "./EmptyState";
import { UserAvatar } from "./UserAvatar";
import type { IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";
import type { IIncidentChange } from "~/interfaces/IIncidentChange";
import { formatDay, formatTime } from "~/lib/format";
import { priorityLabels, statusLabels } from "~/lib/labels";

type HistoryEntry = {
  change: IIncidentChange;
  eventIds: string[];
};

function groupEventLinks(changes: IIncidentChange[]): HistoryEntry[] {
  const entries: HistoryEntry[] = [];

  for (const change of changes) {
    const previous = entries.at(-1);
    const isSameLink =
      change.kind === "eventLinked" &&
      previous?.change.kind === "eventLinked" &&
      previous.change.occurredAtUtc === change.occurredAtUtc &&
      previous.change.actorUserId === change.actorUserId;

    if (isSameLink) {
      previous.eventIds.push(change.newValue ?? "");
    } else {
      entries.push({ change, eventIds: change.kind === "eventLinked" ? [change.newValue ?? ""] : [] });
    }
  }

  return entries;
}

function ValueChange({ from, to }: { from: string; to: string }) {
  return (
    <>
      <span className="text-text-3 line-through">{from}</span>
      <span className="mx-1 text-text-3">→</span>
      <span className="font-semibold text-text">{to}</span>
    </>
  );
}

function Value({ children }: { children: ReactNode }) {
  return <span className="font-semibold text-text">{children}</span>;
}

function statusLabel(value: string | null) {
  return statusLabels[value as IncidentStatus] ?? value ?? "";
}

function priorityLabel(value: string | null) {
  return priorityLabels[value as IncidentPriority] ?? value ?? "";
}

function AssigneeChange({ from, to }: { from: string | null; to: string | null }) {
  if (!from) return <>assigned it to <Value>{to}</Value></>;
  if (!to) return <>removed the assignee <span className="text-text-3 line-through">{from}</span></>;
  return <>reassigned it <ValueChange from={from} to={to} /></>;
}

function DescriptionChange({ from, to }: { from: string; to: string }) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <>
      edited the description
      <button type="button" className="mt-1 block text-xs font-medium text-petrol hover:underline" onClick={() => setIsOpen(!isOpen)}>
        {isOpen ? "Hide change" : "Show change"}
      </button>
      {isOpen && (
        <div className="mt-2 grid gap-1.5 text-text">
          <div className="rounded-[5px] bg-[#fbedeb] px-2.5 py-2 whitespace-pre-wrap">
            <span className="block text-[11px] font-semibold text-text-3">Before</span>
            {from}
          </div>
          <div className="rounded-[5px] bg-[#eaf4ee] px-2.5 py-2 whitespace-pre-wrap">
            <span className="block text-[11px] font-semibold text-text-3">After</span>
            {to}
          </div>
        </div>
      )}
    </>
  );
}

function ChangeText({ entry }: { entry: HistoryEntry }) {
  const { change, eventIds } = entry;

  switch (change.kind) {
    case "created":
      return <>created the incident</>;
    case "status":
      return <>changed status <ValueChange from={statusLabel(change.oldValue)} to={statusLabel(change.newValue)} /></>;
    case "priority":
      return <>changed priority <ValueChange from={priorityLabel(change.oldValue)} to={priorityLabel(change.newValue)} /></>;
    case "assignee":
      return <AssigneeChange from={change.oldValue} to={change.newValue} />;
    case "title":
      return <>renamed it <ValueChange from={change.oldValue ?? ""} to={change.newValue ?? ""} /></>;
    case "description":
      return <DescriptionChange from={change.oldValue ?? ""} to={change.newValue ?? ""} />;
    case "eventLinked":
      return (
        <>
          linked {eventIds.length === 1 ? "an event" : `${eventIds.length} events`}{" "}
          <Value>{eventIds.map((id) => `EVT-${id}`).join(", ")}</Value>
        </>
      );
    case "aiAnalysis":
      return <>ran an AI analysis with <Value>{change.newValue}</Value></>;
  }
}

function ActorAvatar({ name }: { name: string | null }) {
  if (name) return <UserAvatar name={name} size="md" />;

  return (
    <i className="grid size-7 shrink-0 place-items-center rounded-full bg-line text-text-3">
      <IconSettings size={15} />
    </i>
  );
}

export function IncidentHistory({ changes }: { changes: IIncidentChange[] }) {
  if (changes.length === 0) {
    return <EmptyState>No changes yet.</EmptyState>;
  }

  const entries = groupEventLinks(changes);

  return (
    <div>
      {entries.map((entry, index) => {
        const { change } = entry;
        const day = formatDay(change.occurredAtUtc);
        const isNewDay = index === 0 || day !== formatDay(entries[index - 1].change.occurredAtUtc);

        return (
          <div key={`${change.kind}-${change.occurredAtUtc}-${index}`}>
            {isNewDay && (
              <div className="mt-[18px] mb-1 text-[11.5px] font-semibold text-text-3 first:mt-0" suppressHydrationWarning>
                {day}
              </div>
            )}
            <div className="flex gap-3 py-2">
              <ActorAvatar name={change.actorName} />
              <div className="flex min-w-0 flex-1 items-baseline justify-between gap-3 pt-1 max-[680px]:flex-col max-[680px]:gap-0">
                <div className="min-w-0 text-sm text-text-2">
                  <span className="font-semibold text-text">{change.actorName ?? "System"}</span> <ChangeText entry={entry} />
                </div>
                <span className="shrink-0 text-[11.5px] text-text-3">{formatTime(change.occurredAtUtc)}</span>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
