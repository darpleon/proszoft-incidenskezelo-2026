import { Link } from "react-router";
import { services } from "~/data/services";
import type { IEvent } from "~/interfaces/IEvent";
import { formatTime } from "~/lib/format";
import { eventTypeLabels, severityLabels, severityTextColors } from "~/lib/labels";

type EventsTableProps = {
  events: IEvent[];
  showIncident?: boolean;
};

const headerCell = "sticky top-0 z-[2] bg-panel pt-2 pr-3 pb-2 text-left text-[11.5px] font-semibold whitespace-nowrap text-text-3 shadow-[0_1px_0_var(--color-line)]";
const cell = "border-t border-[#eff2f1] py-[11px] pr-3 align-top";

function IncidentCell({ event }: { event: IEvent }) {
  if (event.processingState === "Duplicate") {
    return <span className="text-[11.5px] font-medium whitespace-nowrap text-high">Duplikátum, nem került be</span>;
  }

  if (event.incidentId === null) {
    return (
      <span className="inline-flex items-center gap-[7px] text-[11.5px] whitespace-nowrap text-text-3">
        <i className="size-[9px] rounded-full border-[1.5px] border-dashed border-text-3" />
        Nincs incidenshez kötve
      </span>
    );
  }

  return (
    <Link
      to={`/incidents/${event.incidentId}`}
      className="inline-flex items-center gap-[7px] rounded-[3px] bg-petrol-bg px-2 py-0.5 text-[11.5px] font-semibold whitespace-nowrap text-petrol hover:bg-[#dcedef]"
    >
      <i className="size-[5px] rounded-full bg-current" />
      INC-{event.incidentId}
    </Link>
  );
}

function ProcessingNote({ event }: { event: IEvent }) {
  if (event.processingState === "Duplicate") {
    return <span className="ml-[7px] rounded-[3px] bg-[#fdf7ec] px-1.5 py-px text-[11.5px] font-medium whitespace-nowrap text-high">ismételt, elvetve</span>;
  }

  if (event.processingState === "Failed") {
    return <span className="ml-[7px] rounded-[3px] bg-[#fdf2f1] px-1.5 py-px text-[11.5px] font-medium whitespace-nowrap text-crit">feldolgozás sikertelen</span>;
  }

  return null;
}

export function EventsTable({ events, showIncident = false }: EventsTableProps) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[720px] border-collapse text-[13px]">
        <thead>
          <tr>
            <th className={headerCell}>Azonosító</th>
            <th className={headerCell}>Időpont</th>
            <th className={headerCell}>Forrás</th>
            <th className={headerCell}>Esemény</th>
            <th className={headerCell}>Súlyosság</th>
            {showIncident && <th className={headerCell}>Incidens</th>}
          </tr>
        </thead>
        <tbody>
          {events.map((event) => {
            const service = services.find((item) => item.serviceId === event.serviceId);
            const isUnbound = showIncident && event.incidentId === null;

            return (
              <tr key={event.eventId} className={isUnbound ? "bg-panel-2 hover:bg-[#f1f4f3]" : "hover:bg-panel-2"}>
                <td className={`${cell} whitespace-nowrap text-text-2`}>EVT-{event.eventId}</td>
                <td className={`${cell} whitespace-nowrap text-text-2`}>{formatTime(event.occurredAtUtc)}</td>
                <td className={`${cell} font-medium whitespace-nowrap`}>{service?.code}</td>
                <td className={cell}>
                  {eventTypeLabels[event.eventType] ?? event.eventType}
                  <ProcessingNote event={event} />
                  <span className="mt-1 block text-[11.5px] leading-snug text-text-3">{event.summary}</span>
                </td>
                <td className={`${cell} font-semibold whitespace-nowrap ${severityTextColors[event.severity]}`}>
                  {severityLabels[event.severity]}
                </td>
                {showIncident && (
                  <td className={cell}>
                    <IncidentCell event={event} />
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
