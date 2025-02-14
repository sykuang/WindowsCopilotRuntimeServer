curl -X POST "http://localhost:5001/v1/chat/completions"      -H "Content-Type: application/json" -d '{
           "messages": [
             {
               "role": "user",
               "content": "what is the golden ratio?"
             }
           ],
           "model": "Phi-3-mini-4k-directml-int4-awq-block-128-onnx"
         }'