package com.example.minecraft;

import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;

public class TelemetrySender {

    //private static final String AZURE_FUNCTION_URL = "https://cryptovolcanic-renate-inequitable.ngrok-free.dev/api/GameEventHandler";
    private static final String AZURE_FUNCTION_URL = "https://minecraft-acdc2026.azurewebsites.net/api/GameEventHandler";

    public static void send(String json) {
        try {
            URL url = new URL(AZURE_FUNCTION_URL);
            HttpURLConnection con = (HttpURLConnection) url.openConnection();

            con.setRequestMethod("POST");
            con.setRequestProperty("Content-Type", "application/json");
            con.setDoOutput(true);

            try (OutputStream os = con.getOutputStream()) {
                os.write(json.getBytes(StandardCharsets.UTF_8));
            }

            con.getResponseCode(); // fire & forget
            con.disconnect();

        } catch (Exception ex) {
            ex.printStackTrace();
        }
    }
}