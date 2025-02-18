using Microsoft.AspNetCore.Mvc;
using Microsoft.Windows.AI.Generative;
using WindowsCopilotRuntimeServer.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.IO;
using Windows.Foundation;  // Add this line

namespace WindowsCopilotRuntimeServer.Controllers;

using System;
[ApiController]
[Route("v1/chat/completions")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly LanguageModel _languageModel;

    public ChatController(ILogger<ChatController> logger, LanguageModel languageModel)
    {
        _logger = logger;
        _languageModel = languageModel;
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateChatCompletion([FromBody] ChatCompletionRequest request)
    {
        try
        {
            if (request.Stream)
            {
                return StreamingChatCompletion(request);
            }

            _logger.LogInformation("Processing request with messages: {Count}", request.Messages.Count);

            // Combine all messages into a single prompt
            var promptBuilder = new StringBuilder();
            
            // Add function schema if tools are present
            if (request.Tools?.Any() == true)
            {
                promptBuilder.AppendLine("<|functions_schema|>");
                var schema = request.Tools.Select(t => new
                {
                    t.Function.Name,
                    t.Function.Description,
                    t.Function.Parameters
                });
                promptBuilder.AppendLine(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }));
                promptBuilder.AppendLine();
                
                promptBuilder.AppendLine("When the user asks you a question, if you need to use functions, provide ONLY ONE OF THE function call:");
                promptBuilder.AppendLine("<function_calls>");
                promptBuilder.AppendLine("[");
                promptBuilder.AppendLine("    { \"name\": \"function_name\", \"parameters\": {}, \"output\": \"\"}");
                promptBuilder.AppendLine("]");
                promptBuilder.AppendLine("<end_function_calls>");
                promptBuilder.AppendLine();
            }

            // Add conversation history
            foreach (var message in request.Messages)
            {
                promptBuilder.AppendLine($"{message.Role}: {message.Content}");
            }

            var fullPrompt = promptBuilder.ToString();
            _logger.LogInformation("Combined prompt: {Message}", fullPrompt);

            var options = new LanguageModelOptions
            {
                Skill = LanguageModelSkill.General,
                Temp = request.Temperature,
                Top_p = request.TopP,
                Top_k = request.TopK,
            };

            var result = await _languageModel.GenerateResponseAsync(options, fullPrompt);
            var toolCalls = ParseToolCallsFromResponse(result.Response);

            var response = new ChatCompletionResponse
            {
                Model = "PhiSlica",
                Choices = new List<Choice>
                {
                    new Choice
                    {
                        Index = 0,
                        Message = new ResponseMessage 
                        { 
                            Role = "assistant",
                            Content = toolCalls?.Any() == true ? null : result.Response,
                            ToolCalls = toolCalls
                        },
                        FinishReason = toolCalls?.Any() == true ? "tool_calls" : "stop"
                    }
                },
                Usage = new Usage
                {
                    PromptTokens = fullPrompt.Length / 4,
                    CompletionTokens = result.Response.Length / 4,
                }
            };
            response.Usage.TotalTokens = response.Usage.PromptTokens + response.Usage.CompletionTokens;

            _logger.LogInformation("Response: {string}", result.Response); 
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat completion request");
            return StatusCode(500, new { error = new { message = "Internal server error" } });
        }
    }

    private IActionResult StreamingChatCompletion(ChatCompletionRequest request)
    {
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        return new StreamingResult(async (stream, cancellationToken) =>
        {
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            var responseId = $"chatcmpl-{Guid.NewGuid():N}";
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Combine all messages into a single prompt
            var promptBuilder = new StringBuilder();
            
            // Add function schema if tools are present
            if (request.Tools?.Any() == true)
            {
                promptBuilder.AppendLine("<|functions_schema|>");
                var schema = request.Tools.Select(t => new
                {
                    t.Function.Name,
                    t.Function.Description,
                    t.Function.Parameters
                });
                promptBuilder.AppendLine(JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true }));
                promptBuilder.AppendLine();
                
                promptBuilder.AppendLine("When you need to use functions, provide ONLY ONE OF THE function call:");
                promptBuilder.AppendLine("<function_calls>");
                promptBuilder.AppendLine("[");
                promptBuilder.AppendLine("    { \"name\": \"function_name\", \"parameters\": {}, \"output\": \"\"}");
                promptBuilder.AppendLine("]");
                promptBuilder.AppendLine("<end_function_calls>");
                promptBuilder.AppendLine();
            }

            // Add conversation history
            foreach (var message in request.Messages)
            {
                promptBuilder.AppendLine($"{message.Role}: {message.Content}");
            }

            var fullPrompt = promptBuilder.ToString();
            _logger.LogInformation("Combined prompt: {Message}", fullPrompt);

            var options = new LanguageModelOptions
            {
                Skill = LanguageModelSkill.General,
                Temp = request.Temperature,
                Top_p = request.TopP,
                Top_k = request.TopK,
            };

            // Send initial role message
            var roleChunk = new ChatCompletionChunk
            {
                Id = responseId,
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = "PhiSlica",
                Choices = new List<StreamingChoice>
                {
                    new StreamingChoice
                    {
                        Index = 0,
                        Delta = new DeltaMessage { Role = "assistant" }
                    }
                }
            };

            await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(roleChunk)}");
            await writer.WriteLineAsync();
            await writer.FlushAsync();

            var tokenCount = 0;
            
            AsyncOperationProgressHandler<LanguageModelResponse, string> progressHandler = async (_, delta) =>
            {
                if (cts.Token.IsCancellationRequested) return;

                var chunk = new ChatCompletionChunk
                {
                    Id = responseId,
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Model = "PhiSlica",
                    Choices = new List<StreamingChoice>
                    {
                        new StreamingChoice
                        {
                            Index = 0,
                            Delta = new DeltaMessage { Content = delta }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(chunk);
                await writer.WriteLineAsync($"data: {json}");
                await writer.WriteLineAsync();
                await writer.FlushAsync();
                tokenCount++;
            };

            try
            {
                var asyncOp = _languageModel.GenerateResponseWithProgressAsync(options, fullPrompt);
                asyncOp.Progress = progressHandler;
                var response = await asyncOp;

                // if (response.Status == LanguageModelResponseStatus.Complete)
                {
                    var toolCalls = ParseToolCallsFromResponse(response.Response);
                    if (toolCalls?.Any() == true)
                    {
                        // Send tool calls in a chunk
                        var toolCallChunk = new ChatCompletionChunk
                        {
                            Id = responseId,
                            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            Model = "PhiSlica",
                            Choices = new List<StreamingChoice>
                            {
                                new StreamingChoice
                                {
                                    Index = 0,
                                    Delta = new DeltaMessage { ToolCalls = toolCalls },
                                    FinishReason = "tool_calls"
                                }
                            }
                        };

                        await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(toolCallChunk)}");
                        await writer.WriteLineAsync();
                    }
                    else
                    {
                        // Send finish marker
                        var finalChunk = new ChatCompletionChunk
                        {
                            Id = responseId,
                            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            Model = "PhiSlica",
                            Choices = new List<StreamingChoice>
                            {
                                new StreamingChoice
                                {
                                    Index = 0,
                                    Delta = new DeltaMessage(),
                                    FinishReason = "stop"
                                }
                            }
                        };

                        await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(finalChunk)}");
                        await writer.WriteLineAsync();
                    }
                }
                _logger.LogInformation("Response: {response.Status}", response.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in streaming response");
                if (!cts.Token.IsCancellationRequested)
                {
                    await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(new { error = ex.Message })}");
                    await writer.WriteLineAsync();
                }
            }

            await writer.WriteLineAsync("data: [DONE]");
            await writer.FlushAsync();
        });
    }

    private List<ToolCall> ParseToolCallsFromResponse(string response)
    {
        try
        {
            var match = Regex.Match(response, @"<function_calls>\s*(\[[\s\S]*?\])\s*<end_function_calls>");
            if (!match.Success)
            {
                return null;
            }

            _logger.LogInformation("Parsing tool calls from response: {response}", response);
            var json = match.Groups[1].Value;
            _logger.LogInformation("Parsing tool calls from response: {string}", json);
            var calls = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            
            if (!calls?.Any() == true)
            {
                return null;
            }

            var toolCalls = calls.Select(call => new ToolCall
            {
                Id = $"call_{Guid.NewGuid():N}",
                Type = "function",
                Function = new ToolCallFunction
                {
                    Name = call["name"].ToString(),
                    Arguments = JsonSerializer.Serialize(call["parameters"])
                }
            }).ToList();

            // Clean response
            response = Regex.Replace(response, @"<function_calls>[\s\S]*?<end_function_calls>", "").Trim();

            return toolCalls;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing tool calls from response");
            return null;
        }
    }
}

public class StreamingResult : IActionResult
{
    private readonly Func<Stream, CancellationToken, Task> _callback;

    public StreamingResult(Func<Stream, CancellationToken, Task> callback)
    {
        _callback = callback;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.StatusCode = 200;
        await _callback(context.HttpContext.Response.Body, context.HttpContext.RequestAborted);
    }
}
