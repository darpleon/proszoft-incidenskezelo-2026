import { Button } from "@mantine/core";
import { IconRefresh } from "@tabler/icons-react";
import { DetailMenu, DetailMenuItem } from "./DetailMenu";
import type { AiModel } from "~/interfaces/IAiAnalysis";

type AiRunControlProps = {
  model: AiModel;
  label: string;
  primary?: boolean;
  disabled?: boolean;
  onModelChange: (model: AiModel) => void;
  onRun: () => void;
};

export const aiModels: AiModel[] = ["Qwen3", "Gemini"];

const modelNotes: Record<AiModel, string> = {
  Qwen3: "Local model via Ollama",
  Gemini: "Google, cloud API",
};

export function AiRunControl({ model, label, primary, disabled, onModelChange, onRun }: AiRunControlProps) {
  return (
    <div className="flex">
      <Button
        size="xs"
        variant={primary ? "filled" : "default"}
        color={primary ? "#0f717d" : undefined}
        leftSection={primary ? undefined : <IconRefresh size={14} />}
        disabled={disabled}
        onClick={onRun}
        className="rounded-r-none"
      >
        {label}
      </Button>
      <DetailMenu
        label="Model"
        variant="button"
        position="bottom-end"
        disabled={disabled}
        trigger={
          <span className="flex items-center gap-2">
            <span className="text-[11.5px] font-medium text-text-3">Model</span>
            {model}
          </span>
        }
      >
        {aiModels.map((option) => (
          <DetailMenuItem key={option} selected={option === model} leftSection={null} onClick={() => onModelChange(option)}>
            {option}
            <span className="block text-[11.5px] font-normal text-text-3">{modelNotes[option]}</span>
          </DetailMenuItem>
        ))}
      </DetailMenu>
    </div>
  );
}
