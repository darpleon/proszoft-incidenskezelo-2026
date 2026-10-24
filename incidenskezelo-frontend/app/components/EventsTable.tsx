import type { IEvent } from "~/interfaces/IEvent";
import { formatTime } from "~/lib/format";
import { eventTypeLabels, severityLabels, severityTextColors } from "~/lib/labels";

type EventsTableProps = {
  events: IEvent[];
};

const headerCell = "sticky top-0 z-[2] bg-panel pt-2 pr-3 pb-2 text-left text-[11.5px] font-semibold whitespace-nowrap text-text-3 shadow-[0_1px_0_var(--color-line)]";
const cell = "border-t border-[#eff2f1] py-[11px] pr-3 align-top";

export function EventsTable({ events }: EventsTableProps) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[720px] border-collapse text-[13px]">
        <thead>
          <tr>
            <th className={headerCell}>ID</th>
            <th className={headerCell}>Occurred</th>
            <th className={headerCell}>Source</th>
            <th className={headerCell}>Event</th>
            <th className={headerCell}>Severity</th>
          </tr>
        </thead>
        <tbody>
          {events.map((event) => (
            <tr key={event.eventId} className="hover:bg-panel-2">
              <td className={`${cell} whitespace-nowrap text-text-2`}>EVT-{event.eventId}</td>
              <td className={`${cell} whitespace-nowrap text-text-2`}>{formatTime(event.occurredAtUtc)}</td>
              <td className={`${cell} font-medium whitespace-nowrap`}>{event.serviceCode}</td>
              <td className={cell}>
                {eventTypeLabels[event.eventType] ?? event.eventType}
                {event.summary && <span className="mt-1 block text-[11.5px] leading-snug text-text-3">{event.summary}</span>}
              </td>
              <td className={`${cell} font-semibold whitespace-nowrap ${severityTextColors[event.severity]}`}>
                {severityLabels[event.severity]}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
