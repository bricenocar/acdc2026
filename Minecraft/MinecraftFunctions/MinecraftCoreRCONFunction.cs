using CoreRCON;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace MinecraftFunctions;

public class MinecraftCoreRCONFunction
{
    private readonly ILogger<MinecraftCoreRCONFunction> _logger;
    private readonly string pass = "MineCraft2026@";

    public MinecraftCoreRCONFunction(ILogger<MinecraftCoreRCONFunction> logger)
    {
        _logger = logger;
    }

    [Function("MinecraftCoreRCONFunction")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        // Read request body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        if (requestBody == null || requestBody.Trim().Length == 0)
        {
            return new BadRequestObjectResult("Please pass a valid JSON body");
        }

        // Fix CS8600: Use 'var' and check for null after deserialization
        dynamic? data = JsonConvert.DeserializeObject(requestBody);
        if (data == null)
        {
            return new BadRequestObjectResult("Invalid JSON body.");
        }
                
        var message = data?.message.Value;

        if (message == null || message?.Trim().Length == 0)
        {
            return new BadRequestObjectResult("Please pass a valid JSON body");
        }      

        try
        {
            // 20.229.184.148
            var ipAddress = new IPAddress([20, 229, 184, 148]); // Replace with your server's IP address
            var endpoint = new IPEndPoint(ipAddress, 25575);
            //var endpoint = new IPEndPoint(IPAddress.Loopback, 25575);
            var rcon = new RCON(endpoint, pass, 5000);

            await rcon.ConnectAsync();
            await Task.Delay(200); // Small delay to ensure connection is established

            var result = await rcon.SendCommandAsync(message);
            Console.WriteLine(result);
            rcon.Dispose();
        }
        catch (Exception ex)
        {
            return new StatusCodeResult(500);
        }        

        return new OkObjectResult("Command sent to Minecraft successfully!");
    }
}