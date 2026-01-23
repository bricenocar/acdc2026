using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MinecraftFunctions.Helpers;
using MinecraftFunctions.Models;
using MinecraftFunctions.Services;
using Newtonsoft.Json;

namespace MinecraftFunctions.Functions;

public class GameEventHandler
{
    private readonly ILogger<GameEventHandler> _logger;
    private readonly EventHubService _eventHubService;
    private readonly DataverseService _dataverseService;

    public GameEventHandler(ILogger<GameEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Manual instantiation – SAME behavior as original code
        _eventHubService = new EventHubService(logger);
        _dataverseService = new DataverseService(logger);
    }

    [Function("GameEventHandler")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("Started GameEventHandler");

        try
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
                return new BadRequestObjectResult("Empty payload");

            GameEvent gameEvent = JsonConvert.DeserializeObject<GameEvent>(body)!;

            GameEventValidator.ValidateEvent(gameEvent);

            await _eventHubService.SendToEventHub(gameEvent);
            await _dataverseService.UpdateDataverseIfNeeded(gameEvent);

            return new OkObjectResult(new
            {
                status = "ok",
                eventType = gameEvent.EventType,
                player = gameEvent.PlayerName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GameEventHandler failed");
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
