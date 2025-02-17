using Microsoft.AspNetCore.Mvc;
using Microsoft.Windows.AI.Generative;
using WindowsCopilotRuntimeServer.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
            _logger.LogInformation("Processing request with messages: {Count}", request.Messages.Count);

            // Build conversation history
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
                promptBuilder.AppendLine("    { \"name\": \"function_name\", \"parameters\": {}, \"output\": \"\"},");
                promptBuilder.AppendLine("    { \"name\": \"function_name\", \"parameters\": {\"param_1\": \"value_1\", \"param_2\": \"value_2\"}, \"output\": \"\"},");
                promptBuilder.AppendLine("    { \"name\": \"function_name\", \"parameters\": {\"param_3\": \"value_3\"}, \"output\": \"\"}");
                promptBuilder.AppendLine("]");
                promptBuilder.AppendLine("<end_function_calls>");
                promptBuilder.AppendLine();
            }

            // Add conversation history
            foreach (var msg in request.Messages)
            {
                promptBuilder.AppendLine($"{msg.Role}: {msg.Content}");
            }

            var userMessage = promptBuilder.ToString();

            var options = new LanguageModelOptions
            {
                Skill = LanguageModelSkill.General,
                Temp = request.Temperature,
                Top_p = request.TopP,
                Top_k = request.TopK,
            };
            var result = await _languageModel.GenerateResponseAsync(options, userMessage);

            // Parse tool calls from the response
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
                    PromptTokens = userMessage.Length / 4, // Approximate
                    CompletionTokens = result.Response.Length / 4, // Approximate
                }
            };
            response.Usage.TotalTokens = response.Usage.PromptTokens + response.Usage.CompletionTokens;

            _logger.LogInformation("Responding with completion: {string}", result.Response); 
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat completion request");
            return StatusCode(500, new { error = new { message = "Internal server error" } });
        }
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
