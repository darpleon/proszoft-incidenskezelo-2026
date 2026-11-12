export type AiModel = "Qwen3" | "Gemini";

export type AiConfidence = "High" | "Medium" | "Low";

export interface IAiCause {
  description: string;
  confidence: AiConfidence;
}

export interface IAiToolCall {
  name: string;
  succeeded: boolean;
}

export interface IAiAnalysis {
  incidentId: string;
  model: AiModel;
  summary: string;
  probableCauses: IAiCause[];
  suggestedActions: string[];
  relevantEventIds: number[];
  limitations: string[];
  toolCalls: IAiToolCall[];
}
