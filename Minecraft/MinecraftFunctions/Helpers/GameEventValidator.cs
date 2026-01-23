using MinecraftFunctions.Models;

namespace MinecraftFunctions.Helpers;

public static class GameEventValidator
{
    public static void ValidateEvent(GameEvent e)
    {
        if (string.IsNullOrEmpty(e.EventType))
            throw new Exception("EventType is required");
        if (string.IsNullOrEmpty(e.PlayerName))
            throw new Exception("PlayerName is required");
        if (e.Timestamp <= 0)
            throw new Exception("Invalid timestamp");
    }
}
