using Microsoft.AspNetCore.Mvc;
using Microsoft.Windows.AI.Generative;
using WindowsCopilotRuntimeServer.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            _logger.LogInformation("Processing chat completion request for model: {Model}", request.Model);

            var userMessage = request.Messages.LastOrDefault(m => m.Role == "user")?.Content;
            if (string.IsNullOrEmpty(userMessage))
            {
                return BadRequest(new { error = new { message = "No valid user message found in the request" } });
            }

            var options = new LanguageModelOptions
            {
                Skill = LanguageModelSkill.General,
                Temp = request.Temperature,
                Top_p = request.TopP,
                Top_k = request.TopK,
            };

            var result = await _languageModel.GenerateResponseAsync(options, userMessage);

            var response = new ChatCompletionResponse
            {
                Model = "PhiSlica",
                Choices = new List<Choice>
                {
                    new Choice
                    {
                        Index = 0,
                        Message = new Message 
                        { 
                            Role = "assistant", 
                            Content = result.Response 
                        },
                        FinishReason = "stop"
                    }
                },
                Usage = new Usage
                {
                    PromptTokens = userMessage.Length / 4, // Approximate
                    CompletionTokens = result.Response.Length / 4, // Approximate
                }
            };
            response.Usage.TotalTokens = response.Usage.PromptTokens + response.Usage.CompletionTokens;

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat completion request");
            return StatusCode(500, new { error = new { message = "Internal server error" } });
        }
    }
}
