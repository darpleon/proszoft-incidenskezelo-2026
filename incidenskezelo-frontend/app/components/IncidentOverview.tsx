import type { IIncident } from "~/interfaces/IIncident";

type IncidentOverviewProps = {
  incident: IIncident;
};

export function IncidentOverview({ incident }: IncidentOverviewProps) {
  return (
    <div>
      <h3 className="mb-2 text-[11.5px] font-semibold text-text-3">Leírás</h3>
      <p className="max-w-[70ch] text-sm leading-relaxed">{incident.description}</p>
    </div>
  );
}
