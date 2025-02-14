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
    public int? MaxTokens { get; set; }

    [JsonPropertyName("presence_penalty")]
    public float PresencePenalty { get; set; } = 0.0f;

    [JsonPropertyName("frequency_penalty")]
    public float FrequencyPenalty { get; set; } = 0.0f;
}

public class Message
{
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
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
    public Message Message { get; set; } = new();
    public string FinishReason { get; set; } = "stop";
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}
