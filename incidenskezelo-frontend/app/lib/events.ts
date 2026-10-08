import type { EventFilter } from "~/components/EventFilterChips";
import type { IEvent } from "~/interfaces/IEvent";

export function matchesEventFilter(event: IEvent, filter: EventFilter) {
  switch (filter) {
    case "all":
      return true;
    case "bound":
      return event.incidentId !== null && event.processingState !== "Duplicate";
    case "unbound":
      return event.incidentId === null;
    case "duplicate":
      return event.processingState === "Duplicate";
    case "failed":
      return event.processingState === "Failed";
  }
}
