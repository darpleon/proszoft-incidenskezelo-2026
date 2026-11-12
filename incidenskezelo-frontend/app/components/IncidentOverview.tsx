import { InlineEdit } from "./InlineEdit";
import type { IIncident } from "~/interfaces/IIncident";
import { INCIDENT_DESCRIPTION_MAX_LENGTH } from "~/lib/validation";

type IncidentOverviewProps = {
  incident: IIncident;
  onDescriptionSave: (description: string) => Promise<void>;
};

export function IncidentOverview({ incident, onDescriptionSave }: IncidentOverviewProps) {
  return (
    <div>
      <h3 className="mb-1 text-[11.5px] font-semibold text-text-3">Description</h3>
      <InlineEdit
        value={incident.description}
        label="Description"
        maxLength={INCIDENT_DESCRIPTION_MAX_LENGTH}
        onSave={onDescriptionSave}
        multiline
        inputClassName="text-sm leading-relaxed"
      >
        <p className="max-w-[70ch] text-sm leading-relaxed whitespace-pre-wrap">{incident.description}</p>
      </InlineEdit>
    </div>
  );
}
