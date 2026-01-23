package com.example.minecraft;

import org.bukkit.Bukkit;
import org.bukkit.event.EventHandler;
import org.bukkit.event.Listener;
import org.bukkit.event.entity.*;
import org.bukkit.event.player.PlayerJoinEvent;
import org.bukkit.event.block.BlockBreakEvent;
import org.bukkit.event.block.BlockPlaceEvent;
import org.bukkit.plugin.java.JavaPlugin;
import org.bukkit.entity.Player;
import org.bukkit.entity.Monster;
import com.google.gson.JsonObject;

public class MinecraftTelemetryPlugin extends JavaPlugin implements Listener {

    @Override
    public void onEnable() {
        Bukkit.getPluginManager().registerEvents(this, this);
        getLogger().info("Minecraft Telemetry Plugin ENABLED");
    }

    // =====================================================
    // PLAYER LOGIN (CREATE PLAYER IN DATAVERSE)
    // =====================================================
    @EventHandler
    public void onPlayerJoin(PlayerJoinEvent event) {

        getLogger().info("onPlayerJoin running...");

        Player player = event.getPlayer();

        JsonObject payload = TelemetryEventFactory.baseEvent(
                "PlayerJoined",
                player,
                player.getLocation());

        payload.addProperty("loginType", "Join");

        TelemetrySender.send(payload.toString());
    }

    // =====================================================
    // PLAYER KILL (Player kills another Player)
    // =====================================================
    @EventHandler
    public void onPlayerKill(EntityDeathEvent event) {

        if (!(event.getEntity() instanceof Player))
            return;

        Player killed = (Player) event.getEntity();
        Player killer = killed.getKiller();

        if (killer == null || killer.equals(killed))
            return; // Ignore suicides or environment kills

        getLogger().info("onPlayerKill running...");

        JsonObject payload = TelemetryEventFactory.baseEvent(
                "PlayerKilled",
                killer,
                killed.getLocation());

        payload.addProperty("victim", killed.getName());

        TelemetrySender.send(payload.toString());
    }

    // MOB KILL
    @EventHandler
    public void onMobKilled(EntityDeathEvent event) {

        getLogger().info("onMobKilled running...");

        if (!(event.getEntity() instanceof Monster))
            return;

        Player killer = event.getEntity().getKiller();
        if (killer == null)
            return;

        JsonObject payload = TelemetryEventFactory.baseEvent(
                "MobKilled",
                killer,
                event.getEntity().getLocation());

        payload.addProperty("target", event.getEntity().getType().name());

        TelemetrySender.send(payload.toString());
    }

    // PLAYER DEATH
    @EventHandler
    public void onPlayerDeath(PlayerDeathEvent event) {

        getLogger().info("onPlayerDeath running...");

        Player player = event.getEntity();

        JsonObject payload = TelemetryEventFactory.baseEvent(
                "PlayerDied",
                player,
                player.getLocation());

        TelemetrySender.send(payload.toString());
    }

    // BLOCK PLACE (BUILD TELEMETRY)
    @EventHandler
    public void onBlockPlace(BlockPlaceEvent event) {

        getLogger().info("onBlockPlace running...");

        JsonObject payload = TelemetryEventFactory.baseEvent(
                "BlockPlaced",
                event.getPlayer(),
                event.getBlock().getLocation());

        payload.addProperty("blockType", event.getBlock().getType().name());
        payload.addProperty("quantity", 1);

        TelemetrySender.send(payload.toString());
    }

    // BLOCK BREAK
    @EventHandler
    public void onBlockBreak(BlockBreakEvent event) {

        getLogger().info("onBlockBreak running...");

        JsonObject payload = TelemetryEventFactory.baseEvent(
                "BlockBroken",
                event.getPlayer(),
                event.getBlock().getLocation());

        payload.addProperty("blockType", event.getBlock().getType().name());
        payload.addProperty("quantity", 1);

        TelemetrySender.send(payload.toString());
    }
}