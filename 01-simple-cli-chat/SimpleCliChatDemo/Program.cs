using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

const string modelName = "granite4.1:3b";
const string apiKey = "YOUR-API-KEY";
const string apiUrl = "http://172.23.176.1:11434/v1";

IChatClient chatClient =
    new ChatClient(
        model: modelName,
        credential: new ApiKeyCredential(apiKey),
        options: new OpenAIClientOptions
        {
            Endpoint = new Uri(apiUrl)
        })
    .AsIChatClient();

AIAgent agent = new ChatClientAgent(chatClient);

Console.Write("\n=======================================================\n\n");

while (true)
{
    Console.Write("PROMPT:\t\t");

    var prompt = string.Empty;
    while (string.IsNullOrWhiteSpace(prompt))
    {
        prompt = Console.ReadLine();
    }

    Console.Write("RESPONSE:\t");

    await foreach (var update in agent.RunStreamingAsync(prompt))
    {
        Console.Write(update);
    }

    Console.Write("\n\n=======================================================\n\n");
}
