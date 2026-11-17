import type { IEvent } from "~/interfaces/IEvent";
import { formatTime } from "~/lib/format";
import { eventTypeLabels, severityLabels, severityTextColors } from "~/lib/labels";
import { tableCell, tableHeaderCell } from "~/lib/tableClasses";

type EventsTableProps = {
  events: IEvent[];
};

export function EventsTable({ events }: EventsTableProps) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[720px] border-collapse text-[13px]">
        <thead>
          <tr>
            <th className={tableHeaderCell}>ID</th>
            <th className={tableHeaderCell}>Occurred</th>
            <th className={tableHeaderCell}>Source</th>
            <th className={tableHeaderCell}>Event</th>
            <th className={tableHeaderCell}>Severity</th>
          </tr>
        </thead>
        <tbody>
          {events.map((event) => (
            <tr key={event.eventId} className="hover:bg-panel-2">
              <td className={`${tableCell} whitespace-nowrap text-text-2`}>EVT-{event.eventId}</td>
              <td className={`${tableCell} whitespace-nowrap text-text-2`}>{formatTime(event.occurredAtUtc)}</td>
              <td className={`${tableCell} font-medium whitespace-nowrap`}>{event.serviceCode}</td>
              <td className={tableCell}>
                {eventTypeLabels[event.eventType] ?? event.eventType}
                {event.summary && <span className="mt-1 block text-[11.5px] leading-snug text-text-3">{event.summary}</span>}
              </td>
              <td className={`${tableCell} font-semibold whitespace-nowrap ${severityTextColors[event.severity]}`}>
                {severityLabels[event.severity]}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
