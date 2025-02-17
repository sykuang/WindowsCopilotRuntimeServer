curl http://localhost:5001/v1/chat/completions -H "Content-Type: application/json"   -d '{
    "model": "gpt-4o",
    "messages": [
        {
            "role": "user",
            "content": "Can you send an email to ilan@example.com and katia@example.com saying hi?"
        }
    ],
    "tools": [
        {
            "type": "function",
            "function": {
                "name": "send_email",
                "description": "Send an email to a given recipient with a subject and message.",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "to": {
                            "type": "string",
                            "description": "The recipient email address."
                        },
                        "subject": {
                            "type": "string",
                            "description": "Email subject line."
                        },
                        "body": {
                            "type": "string",
                            "description": "Body of the email message."
                        }
                    },
                    "required": [
                        "to",
                        "subject",
                        "body"
                    ],
                    "additionalProperties": false
                },
                "strict": true
            }
        }
    ]
}'