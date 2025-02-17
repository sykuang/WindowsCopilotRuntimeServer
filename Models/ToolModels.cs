using System;
using System.Text.Json.Serialization;

namespace WindowsCopilotRuntimeServer.Models;

public class Tool
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public ToolFunction Function { get; set; }
}

public class ToolFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("parameters")]
    public object Parameters { get; set; }
}

public class ToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public ToolCallFunction Function { get; set; } = new();
}

public class ToolCallFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = null;
}
