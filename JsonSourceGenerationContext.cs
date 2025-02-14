using System.Collections.Generic;
using System.Text.Json.Serialization;
using WindowsCopilotRuntimeServer.Models;

namespace WindowsCopilotRuntimeServer;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ChatCompletionRequest))]
[JsonSerializable(typeof(ChatCompletionResponse))]
[JsonSerializable(typeof(List<Message>))]
[JsonSerializable(typeof(List<Choice>))]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(Choice))]
[JsonSerializable(typeof(Usage))]
public partial class JsonSourceGenerationContext : JsonSerializerContext
{
}
