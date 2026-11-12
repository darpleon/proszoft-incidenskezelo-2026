using System.Text.Json;

namespace incidenskezelo_backend.Models.AI;

public sealed record AiToolDefinition(
    string Name,
    string Description,
    string ParametersSchemaJson);

public sealed record AiToolCall(
    string Id,
    string Name,
    JsonElement Arguments);

public sealed record AiToolResult(
    string ToolCallId,
    string ToolName,
    JsonElement Result);

public sealed record AiTurnRequest(
    string? Prompt,
    JsonElement? ProviderState,
    IReadOnlyList<AiToolDefinition> Tools,
    IReadOnlyList<AiToolResult> ToolResults);

public sealed record AiTurnResponse(
    string? Text,
    IReadOnlyList<AiToolCall> ToolCalls,
    JsonElement? ProviderState);

public sealed record AiToolCallStatus(string Name, bool Succeeded);

public sealed record ProbableCause(
    string Description,
    string Confidence);

public sealed record IncidentAnalysisResult(
    string Model,
    string Summary,
    IReadOnlyList<ProbableCause> ProbableCauses,
    IReadOnlyList<string> SuggestedActions,
    IReadOnlyList<long> RelevantEventIds,
    IReadOnlyList<string> Limitations,
    IReadOnlyList<AiToolCallStatus> ToolCalls);
