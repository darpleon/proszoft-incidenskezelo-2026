import { IconCheck, IconX } from "@tabler/icons-react";
import type { ReactNode } from "react";
import { EventsTable } from "./EventsTable";
import type { IAiAnalysis } from "~/interfaces/IAiAnalysis";
import type { IEvent } from "~/interfaces/IEvent";
import { aiConfidenceTextColors } from "~/lib/labels";

type AiAnalysisResultProps = {
  analysis: IAiAnalysis;
  events: IEvent[];
};

function AiSection({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="border-t border-[#eff2f1] py-4 first:border-t-0 first:pt-0">
      <h3 className="mb-2 text-[11.5px] font-semibold text-text-3">{title}</h3>
      {children}
    </section>
  );
}

export function AiAnalysisResult({ analysis, events }: AiAnalysisResultProps) {
  const keyEvents = events.filter((event) => analysis.relevantEventIds.includes(event.eventId));

  return (
    <div>
      {analysis.summary.trim() !== "" && (
        <AiSection title="Summary">
          <p className="text-[15px] leading-relaxed">{analysis.summary}</p>
        </AiSection>
      )}

      {analysis.probableCauses.length > 0 && (
        <AiSection title="Likely causes">
          <ul>
            {analysis.probableCauses.map((cause, index) => (
              <li key={index} className="flex items-baseline gap-4 border-t border-[#eff2f1] py-2 text-sm first:border-t-0">
                {cause.description}
                <span className={`ml-auto text-[11.5px] whitespace-nowrap ${aiConfidenceTextColors[cause.confidence]}`}>
                  {cause.confidence}
                </span>
              </li>
            ))}
          </ul>
        </AiSection>
      )}

      {analysis.suggestedActions.length > 0 && (
        <AiSection title="Suggested next steps">
          <ol className="list-decimal pl-5 text-sm">
            {analysis.suggestedActions.map((action, index) => (
              <li key={index} className="mb-1.5 leading-relaxed">
                {action}
              </li>
            ))}
          </ol>
        </AiSection>
      )}

      {keyEvents.length > 0 && (
        <AiSection title="Key events">
          <EventsTable events={keyEvents} />
        </AiSection>
      )}

      {analysis.limitations.length > 0 && (
        <AiSection title="What the AI could not check">
          <ul className="list-disc rounded-lg bg-panel-2 py-3 pr-3.5 pl-8 text-[13px] leading-relaxed text-text-2">
            {analysis.limitations.map((limitation, index) => (
              <li key={index}>{limitation}</li>
            ))}
          </ul>
        </AiSection>
      )}

      {analysis.toolCalls.length > 0 && (
        <AiSection title="Data the AI used">
          <ul className="text-[13px]">
            {analysis.toolCalls.map((toolCall, index) => (
              <li key={index} className="flex items-center gap-2.5 py-1">
                <code className="text-xs">{toolCall.name}</code>
                {toolCall.succeeded ? (
                  <span className="ml-auto flex items-center gap-1 text-[11.5px] text-ok">
                    <IconCheck size={13} />
                    OK
                  </span>
                ) : (
                  <span className="ml-auto flex items-center gap-1 text-[11.5px] text-high">
                    <IconX size={13} />
                    Failed
                  </span>
                )}
              </li>
            ))}
          </ul>
        </AiSection>
      )}

      <p className="mt-1.5 border-t border-[#eff2f1] pt-3 text-[11.5px] text-text-3">
        These are AI suggestions, not verified facts. Check them before you act.
      </p>
    </div>
  );
}
