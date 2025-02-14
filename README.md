# Windows Copilot Runtime Server

A REST API server that provides OpenAI-compatible endpoints for Windows Copilot+ PC, allowing you to use local AI models through standard OpenAI client libraries.

## Features

- OpenAI-compatible REST API
- Support for chat completions
- Local model inference using Windows Copilot Runtime
- Configurable parameters (temperature, top_p, top_k)
- CORS support for web applications

## Prerequisites

- Windows 11 (Build 22621 or later)
- .NET 8.0
- Windows App SDK 1.7-experimental3
- Windows Copilot Runtime

## Quick Start

1. Build and run the server:
```bash
dotnet run
```

2. The server will start at `http://localhost:5001`

3. Use with OpenAI clients:
```python
from openai import OpenAI

client = OpenAI(
    base_url="http://localhost:5001/v1",
    api_key="not-needed"  # required by client but not used
)

chat_completion = client.chat.completions.create(
    model="windows-local",
    messages=[{
        "role": "user",
        "content": "What is the golden ratio?"
    }],
    temperature=0.7
)

print(chat_completion.choices[0].message.content)
```

## API Reference

### Chat Completions

`POST /v1/chat/completions`

Request body:
```json
{
    "model": "windows-local",
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

## Development

- Written in C# using ASP.NET Core
- Uses Windows App SDK for AI features
- JSON source generation for optimal performance
- Swagger UI available at `/swagger`

## License

This project is licensed under the MIT License.
