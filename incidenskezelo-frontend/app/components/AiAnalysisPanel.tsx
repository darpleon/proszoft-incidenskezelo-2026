import { Loader, Skeleton } from "@mantine/core";
import { useLocalStorage } from "@mantine/hooks";
import { IconSparkles } from "@tabler/icons-react";
import { isAxiosError } from "axios";
import { useState } from "react";
import { AiAnalysisResult } from "./AiAnalysisResult";
import { AiRunControl, aiModels } from "./AiRunControl";
import { FormAlert } from "./FormAlert";
import api from "~/api/api";
import { aiModelKeyName } from "~/constants/constants";
import type { AiModel, IAiAnalysis } from "~/interfaces/IAiAnalysis";
import type { IEvent } from "~/interfaces/IEvent";
import { formatTime } from "~/lib/format";

type AiAnalysisPanelProps = {
  incidentId: string;
  events: IEvent[];
};

type AiRun = {
  analysis: IAiAnalysis;
  finishedAtUtc: string;
  durationSeconds: number;
};

const listFormat = new Intl.ListFormat("en", { type: "disjunction" });

function withDefaults(analysis: IAiAnalysis): IAiAnalysis {
  return {
    ...analysis,
    summary: analysis.summary ?? "",
    probableCauses: analysis.probableCauses ?? [],
    suggestedActions: analysis.suggestedActions ?? [],
    relevantEventIds: analysis.relevantEventIds ?? [],
    limitations: analysis.limitations ?? [],
    toolCalls: analysis.toolCalls ?? [],
  };
}

function missingParts(analysis: IAiAnalysis) {
  return [
    analysis.summary.trim() === "" && "summary",
    analysis.probableCauses.length === 0 && "likely causes",
    analysis.suggestedActions.length === 0 && "next steps",
  ].filter((part) => part !== false);
}

function aiErrorMessage(error: unknown, model: AiModel) {
  if (!isAxiosError(error)) return "The analysis could not be finished. Please try again.";

  const status = error.response?.status;
  if (status === 504 || error.code === "ECONNABORTED" || error.code === "ETIMEDOUT") {
    return `${model} did not answer in time. Try again, or pick the other model.`;
  }
  if (status === 503) return `${model} is not available right now. Pick the other model, or try again later.`;
  if (status === 502) return `${model} gave an answer the app could not use. Try again, or pick the other model.`;
  if (status === 403) return "You don't have permission to run an AI analysis.";
  return "The analysis could not be finished. Please try again.";
}

export function AiAnalysisPanel({ incidentId, events }: AiAnalysisPanelProps) {
  const [storedModel, setModel] = useLocalStorage<AiModel>({ key: aiModelKeyName, defaultValue: "Qwen3" });
  const model = aiModels.includes(storedModel) ? storedModel : "Qwen3";
  const [lastRun, setLastRun] = useState<AiRun | null>(null);
  const [running, setRunning] = useState<{ model: AiModel; startedAtUtc: string } | null>(null);
  const [failure, setFailure] = useState<string | null>(null);

  const run = async () => {
    const startedAt = new Date();
    setRunning({ model, startedAtUtc: startedAt.toISOString() });
    setFailure(null);

    try {
      const response = await api.Ai.analyze(incidentId, model);
      setLastRun({
        analysis: withDefaults(response.data),
        finishedAtUtc: new Date().toISOString(),
        durationSeconds: Math.round((Date.now() - startedAt.getTime()) / 1000),
      });
    } catch (error) {
      setFailure(aiErrorMessage(error, model));
    }
    setRunning(null);
  };

  if (!lastRun && !running && !failure) {
    return (
      <div className="py-9">
        <div className="mb-3 grid size-9 place-items-center rounded-lg bg-petrol-bg text-petrol">
          <IconSparkles size={19} />
        </div>
        <b className="mb-1 block text-[15px] font-semibold">No AI analysis yet</b>
        <p className="mb-4 text-sm leading-relaxed text-text-2">
          The AI reads this incident and its events, then suggests likely causes and next steps. It does not change anything in the
          incident.
        </p>
        <AiRunControl model={model} label="Run AI analysis" primary onModelChange={setModel} onRun={run} />
      </div>
    );
  }

  const missing = lastRun ? missingParts(lastRun.analysis) : [];

  return (
    <div>
      <div className="mb-[18px] flex flex-wrap items-center gap-3">
        <span className="text-[11.5px] text-text-3">
          {running && `Started at ${formatTime(running.startedAtUtc)}`}
          {!running && lastRun && (
            <>
              Analyzed with <b className="font-semibold text-text-2">{lastRun.analysis.model}</b> at {formatTime(lastRun.finishedAtUtc)} · took{" "}
              {lastRun.durationSeconds} s
            </>
          )}
        </span>
        <div className="ml-auto">
          <AiRunControl
            model={model}
            label={failure ? "Try again" : "Run again"}
            disabled={running !== null}
            onModelChange={setModel}
            onRun={run}
          />
        </div>
      </div>

      {running ? (
        <div className="rounded-lg border border-line p-4">
          <div className="flex items-center gap-2.5 text-sm font-semibold">
            <Loader size={14} color="#0f717d" />
            Analyzing incident with {running.model}…
          </div>
          <p className="mt-1 mb-3.5 text-[12.5px] text-text-2">This can take up to 30 seconds. You can switch tabs, the result will wait here.</p>
          <Skeleton height={10} width="92%" mb={9} />
          <Skeleton height={10} width="78%" mb={9} />
          <Skeleton height={10} width="40%" />
        </div>
      ) : (
        <>
          {failure && <FormAlert title="The analysis could not be finished" message={failure} />}
          {missing.length > 0 && (
            <FormAlert
              color="yellow"
              title="The AI answer was incomplete"
              message={`It had no ${listFormat.format(missing)}. The rest is shown below. You can run it again.`}
            />
          )}
          {lastRun && <AiAnalysisResult analysis={lastRun.analysis} events={events} />}
        </>
      )}
    </div>
  );
}
