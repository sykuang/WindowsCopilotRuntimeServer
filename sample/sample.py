from openai import OpenAI

client = OpenAI(
    base_url="http://localhost:5001/v1/",
    api_key="x" # required by API but not used
)

chat_completion = client.chat.completions.create(
    messages=[
        {
            "role": "user",
            "content": "What is the molecular formula for glucose?",
        }
    ],
    model="windows-local",
  
)

print(chat_completion.choices[0].message.content)