package com.example.minecraft;

import com.google.gson.JsonObject;
import org.bukkit.Location;
import org.bukkit.entity.Player;

public class TelemetryEventFactory {

    public static JsonObject baseEvent(
        String eventType,
        Player player,
        Location loc
    ) {
        JsonObject obj = new JsonObject();

        obj.addProperty("eventType", eventType);
        obj.addProperty("playerName", player.getName());
        obj.addProperty("world", loc.getWorld().getName());
        obj.addProperty("x", loc.getBlockX());
        obj.addProperty("y", loc.getBlockY());
        obj.addProperty("z", loc.getBlockZ());
        obj.addProperty("timestamp", System.currentTimeMillis());

        return obj;
    }
}
