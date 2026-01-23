using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Logging;
using MinecraftFunctions.Models;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftFunctions.Services;

public class EventHubService
{
    private readonly ILogger _logger;
    private readonly string _eventHubConnectionString;
    private readonly string _eventHubName;

    private EventHubProducerClient _producer;

    public EventHubService(ILogger logger)
    {
        _logger = logger;

        _eventHubConnectionString = Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION");
        _eventHubName = Environment.GetEnvironmentVariable("EVENTHUB_NAME");

        if (string.IsNullOrEmpty(_eventHubConnectionString) || string.IsNullOrEmpty(_eventHubName))
            throw new Exception("Event Hub connection string or name is missing");
    }

    private EventHubProducerClient GetProducer()
    {
        if (_producer != null)
            return _producer;

        _producer = new EventHubProducerClient(
            _eventHubConnectionString,
            _eventHubName);

        _logger.LogInformation("EventHubProducerClient created");

        return _producer;
    }

    public async Task SendToEventHub(GameEvent e)
    {
        var producer = GetProducer();

        string json = JsonConvert.SerializeObject(e);
        using EventDataBatch batch = await producer.CreateBatchAsync();

        if (!batch.TryAdd(new EventData(Encoding.UTF8.GetBytes(json))))
            throw new Exception("Failed to add event to EventHub batch");

        await producer.SendAsync(batch);
    }
}
