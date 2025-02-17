namespace WindowsCopilotRuntimeServer.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

public class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "windows-local";

    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.9f;

    [JsonPropertyName("top_p")]
    public float TopP { get; set; } = 0.9f;

    [JsonPropertyName("top_k")]
    public uint TopK { get; set; } = 40;

    [JsonPropertyName("n")]
    public int N { get; set; } = 1;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("max_tokens")]
    [JsonIgnore]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("presence_penalty")]
    public float PresencePenalty { get; set; } = 0.0f;

    [JsonPropertyName("frequency_penalty")]
    public float FrequencyPenalty { get; set; } = 0.0f;

    [JsonPropertyName("tools")]
    public List<Tool> Tools { get; set; } = new();

    [JsonIgnore]
    public bool HasToolCalls => Tools?.Any() == true;
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; }


}

public class ResponseMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ToolCall> ToolCalls { get; set; } = null;

}
public class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = $"chatcmpl-{Guid.NewGuid():N}";

    [JsonPropertyName("object")]
    public string Object { get; set; } = "chat.completion";

    [JsonPropertyName("created")]
    public long Created { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = new();
}

public class Choice
{
    public int Index { get; set; }
    public ResponseMessage Message { get; set; } = new();
    public string FinishReason { get; set; } = "stop";
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}
