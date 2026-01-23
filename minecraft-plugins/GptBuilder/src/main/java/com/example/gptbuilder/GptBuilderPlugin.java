package com.example.gptbuilder;

import com.google.gson.*;
import org.bukkit.Bukkit;
import org.bukkit.plugin.java.JavaPlugin;
import org.bukkit.event.Listener;
import org.bukkit.event.EventHandler;
import org.bukkit.event.player.AsyncPlayerChatEvent;

import java.io.*;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;

public class GptBuilderPlugin extends JavaPlugin implements Listener {

    //private static final String AZURE_FUNCTION_URL = "https://cryptovolcanic-renate-inequitable.ngrok-free.dev/api/MinecraftChatFunction";
    private static final String AZURE_FUNCTION_URL = "https://minecraft-acdc2026.azurewebsites.net/api/MinecraftChatFunction";
    private final String keywordPrefix = "datablocks";

    @Override
    public void onEnable() {
        getLogger().info("GptBuilder ENABLED");
        getServer().getPluginManager().registerEvents(this, this);
    }

    @EventHandler
    public void onPlayerChat(AsyncPlayerChatEvent event) {
        String player = event.getPlayer().getName();
        String message = event.getMessage();

        // KEYWORD CHECK
        if (!message.toLowerCase().startsWith(keywordPrefix)) {
            return; // normal chat, do nothing
        }

        // Run HTTP call async (IMPORTANT)
        new Thread(() -> sendToAzure(player, message)).start();
    }

    private void sendToAzure(String player, String message) {
        try {
            URL url = new URL(AZURE_FUNCTION_URL);
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();

            conn.setRequestMethod("POST");
            conn.setRequestProperty("Content-Type", "application/json");
            conn.setDoOutput(true);

            String jsonBody = String.format(
                "{\"player\":\"%s\",\"message\":\"%s\"}",
                player, message.replace("\"", "'")
            );

            try (OutputStream os = conn.getOutputStream()) {
                os.write(jsonBody.getBytes(StandardCharsets.UTF_8));
            }

            // Read response
            InputStream responseStream = conn.getInputStream();
            String response = new BufferedReader(new InputStreamReader(responseStream))
                    .lines()
                    .reduce("", (a, b) -> a + b);

            getLogger().info("Azure response: " + response);

            executeCommands(response);

            conn.disconnect();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    // VERY IMPORTANT: Minecraft commands must run on main thread
    private void executeCommands(String jsonResponse) {
        JsonObject obj = JsonParser.parseString(jsonResponse).getAsJsonObject();
        JsonArray commands = obj.getAsJsonArray("commands");

        Bukkit.getScheduler().runTask(this, () -> {
            for (JsonElement cmd : commands) {
                String command = cmd.getAsString();
                getLogger().info("Executing: " + command);
                Bukkit.dispatchCommand(Bukkit.getConsoleSender(), command.replaceFirst("^/", ""));
            }
        });
    }
}