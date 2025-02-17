using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using WindowsCopilotRuntimeServer.Models;

namespace WindowsCopilotRuntimeServer;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(ChatCompletionRequest))]
[JsonSerializable(typeof(ChatCompletionResponse))]
[JsonSerializable(typeof(List<Message>))]
[JsonSerializable(typeof(List<Choice>))]
[JsonSerializable(typeof(List<Tool>))]
[JsonSerializable(typeof(List<ToolCall>))]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(Choice))]
[JsonSerializable(typeof(Usage))]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(ToolFunction))]
[JsonSerializable(typeof(ToolCall))]
[JsonSerializable(typeof(ToolCallFunction))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(List<Dictionary<string, object>>))]
// Add ASP.NET Core types
[JsonSerializable(typeof(ValidationProblemDetails))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
public partial class JsonSourceGenerationContext : JsonSerializerContext
{
}
