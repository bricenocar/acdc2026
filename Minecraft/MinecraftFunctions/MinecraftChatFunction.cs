using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using static System.Net.WebRequestMethods;

namespace MinecraftFunctions;

public class MinecraftChatFunction
{
    private readonly ILogger<MinecraftChatFunction> _logger;
    private static readonly HttpClient http = new HttpClient();

    public MinecraftChatFunction(ILogger<MinecraftChatFunction> logger)
    {
        _logger = logger;
    }

    [Function("MinecraftChatFunction")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        // Read request body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        if(requestBody == null || requestBody.Trim().Length == 0)
        {
            return new BadRequestObjectResult("Please pass a valid JSON body");
        }

        // Fix CS8600: Use 'var' and check for null after deserialization
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        if (data == null)
        {
            return new BadRequestObjectResult("Invalid JSON body.");
        }

        var player = data?.player;
        var message = data?.message;

        // TODO: Send message to Copilot Studio / GPT agent here
        // For testing now, we just log
        Console.WriteLine($"{player} said: {message}");

        // 2️ Call Power Automate Flow
        //var flowUrl = Environment.GetEnvironmentVariable("FLOW_URL");
        var flowUrl = "https://defaultf48af2fb465d4406993382cdc51358.e9.environment.api.powerplatform.com/powerautomate/automations/direct/workflows/a42b16aeabda495bb410fd729ba76237/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=kn_QDzYfhSkk62NMhYsX2f37b0wh5Ay4SWHc0p2eHnw";
        var json = JsonConvert.SerializeObject(new {message});
        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage flowResponse;
        try
        {
            flowResponse = await http.PostAsync(flowUrl, content);
        }
        catch (Exception ex)
        {
            return new StatusCodeResult(500);
        }

        if (!flowResponse.IsSuccessStatusCode)
        {
            return new StatusCodeResult((int)flowResponse.StatusCode);
        }

        // 3️ Read Copilot response (JSON)
        string copilotJson = await flowResponse.Content.ReadAsStringAsync();

        // 4️ Return JSON directly to Minecraft plugin
        return new ContentResult
        {
            Content = copilotJson,
            ContentType = "application/json",
            StatusCode = 200
        };

        // Return some dummy Minecraft commands
        /*var response = new
        {
            commands = new string[]
            {
                "/say Hello from Azure Function!",
                "/setblock ~ ~1 ~ stone"
            }
        };*/

        //return new OkObjectResult(response);
    }
}