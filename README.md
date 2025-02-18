# Windows Copilot Runtime Server

A REST API server that provides OpenAI-compatible endpoints for Windows Copilot+ PC, allowing you to use local AI models through standard OpenAI client libraries.

## Quick Installation

1. Download the latest release:
   - [WindowsCopilotRuntimeServer.zip](https://github.com/sykuang/WindowsCopilotRuntimeServer/releases)

2. Extract the ZIP file to your preferred location

3. Run PowerShell as Administrator and execute:
```powershell
cd <extraction-path>
.\Install.ps1
```

4. Start the server from Start Menu and search for "Windows Copilot Runtime Server"

## Prerequisites

- Windows 11 Insider Preview Build 26120.3073 (Dev and Beta Channels) or later must be installed on your device.

## API Support Status

| Feature | Status | Notes |
|---------|--------|-------|
| Chat Completions | ✅ | Full support with OpenAI compatibility |
| Tool Calling | ✅ | Supports OpenAI function calling format |
| Vision | ❌ | Planned |
| Streaming | ✅ | Supports SSE with token-by-token streaming |
| Custom Model Loading | ❌ | Not supported by runtime |
| System Messages | ✅ | Supported in chat context |
| Temperature | ✅ | Range 0-1 |
| Top P | ✅ | Range 0-1 |
| Top K | ✅ | Windows Copilot specific parameter |
| Max Tokens | ❌ | Not supported by runtime |
| Stop Sequences | ❌ | Not supported by runtime |

## Usage with OpenAI Clients

### Python with OpenAI Client
```python
from openai import OpenAI

client = OpenAI(
    base_url="http://localhost:5001/v1",
    api_key="not-needed"  # required by client but not used
)

chat_completion = client.chat.completions.create(
    model="PhiSlica",  # or use "windows-local"
    messages=[{
        "role": "user",
        "content": "What is the golden ratio?"
    }],
    temperature=0.7,
    top_p=0.9,
    top_k=40
)

print(chat_completion.choices[0].message.content)
```

### PowerShell
```powershell
# Simple request using Invoke-RestMethod
$headers = @{
    "Content-Type" = "application/json"
}

$body = @{
    model = "PhiSlica"
    messages = @(
        @{
            role = "user"
            content = "What is the golden ratio?"
        }
    )
    temperature = 0.7
    top_p = 0.9
    top_k = 40
} | ConvertTo-Json

$response = Invoke-RestMethod -Method Post `
    -Uri "http://localhost:5001/v1/chat/completions" `
    -Headers $headers `
    -Body $body

$response.choices[0].message.content

# Stream response using curl
curl -X POST "http://localhost:5001/v1/chat/completions" `
     -H "Content-Type: application/json" `
     -d '{
           "model": "PhiSlica",
           "messages": [{"role": "user", "content": "What is the golden ratio?"}],
           "temperature": 0.7,
           "top_p": 0.9,
           "top_k": 40
         }'
```

### Streaming Support

```python
from openai import OpenAI

client = OpenAI(
    base_url="http://localhost:5001/v1",
    api_key="not-needed"
)

stream = client.chat.completions.create(
    model="PhiSlica",
    messages=[{"role": "user", "content": "Write a story about a cat"}],
    stream=True  # Enable streaming
)

for chunk in stream:
    if chunk.choices[0].delta.content:
        print(chunk.choices[0].delta.content, end="")
```

Using curl:
```bash
curl http://localhost:5001/v1/chat/completions \
  -H "Content-Type: application/json" \
  -d '{
    "model": "PhiSlica",
    "messages": [{"role": "user", "content": "Write a story about a cat"}],
    "stream": true
  }'
```

## API Reference

### Chat Completions

`POST /v1/chat/completions`

Request body:
```json
{
    "model": "PhiSlica",
    "messages": [
        {
            "role": "user",
            "content": "Your prompt here"
        }
    ],
    "temperature": 0.9,
    "top_p": 0.9,
    "top_k": 40
}
```

## Features

- OpenAI-compatible REST API
- Local model inference using Windows Copilot Runtime
- CORS support for web applications
- Swagger UI at `/swagger`
- JSON source generation for optimal performance

## Troubleshooting

If you encounter any issues:
1. Make sure Windows App SDK 1.7-experimental3 is installed
2. Check if Windows Copilot Runtime is properly installed
3. Run PowerShell as Administrator when executing Install.ps1
4. Check the logs in the application directory

## License

This project is licensed under the MIT License.
