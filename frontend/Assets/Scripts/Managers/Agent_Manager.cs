using Anthropic;
using Anthropic.Core;
using Anthropic.Models.Beta;
using Anthropic.Models.Beta.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using UnityEngine;

public class Agent_Manager : MonoBehaviour
{
    [SerializeField]
    string MPC_SERVER_URL;
    [SerializeField]
    public string apiKey;
    AnthropicClient client;
    List<BetaMessageParam> messages = new();
    TTS_Manager ttsManager;

    public void Start()
    {
        client = new AnthropicClient
        {
            ApiKey = apiKey,
        };
        ttsManager = this.gameObject.GetComponent<TTS_Manager>();
        STT_Manager.OnUserSpoke += UserRequest;
    }

    public async void UserRequest(string request)
    {
        Debug.Log("Usuario habló: " + request);
        this.messages.Add(new BetaMessageParam { Role = Role.User, Content = request });

        var parameters = new MessageCreateParams
        {
            Model = Anthropic.Models.Messages.Model.ClaudeHaiku4_5_20251001,
            MaxTokens = 1000,
            Messages = this.messages,
            McpServers = new List<BetaRequestMcpServerUrlDefinition>
            {
                new()
                {
                    Url = MPC_SERVER_URL,
                    Name = "Req2Seq",
                }
            },
            Tools = new List<BetaToolUnion>
            {
                new BetaMcpToolset("Req2Seq")
            },
            Betas = new List<ApiEnum<string, AnthropicBeta>>
            {
                AnthropicBeta.McpClient2025_11_20
            }
        };
        var result = await client.Beta.Messages.Create(parameters);
        var message = JsonSerializer.Deserialize<Dictionary<string, string>>(
            result.Content.Last().Value.ToString())["text"];
        this.messages.Add(new BetaMessageParam { Role = Role.Assistant, Content = message });
        ttsManager.Speak(message);
    }
}
