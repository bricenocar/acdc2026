using MinecraftFunctions.Models;

namespace MinecraftFunctions.Helpers;

public static class CreditCalculator
{
    public static int CalculateCredits(GameEvent e) => e.EventType switch
    {
        "MobKilled" => e.Target switch
        {
            "ZOMBIE" => 5,
            "SKELETON" => 7,
            "SPIDER" => 6,
            "CREEPER" => 10,
            _ => 1
        },
        "BlockPlaced" => e.BlockType switch
        {
            "STONE" => 2,
            "OAK_PLANKS" => 1,
            "GLASS" => 2,
            _ => 0
        },
        "PlayerKilled" => 9,
        "PlayerDied" => -5,
        _ => 0
    };
}
