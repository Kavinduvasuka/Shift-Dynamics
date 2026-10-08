package com.example.myapplication;

import okhttp3.*;

import org.json.*;

import java.io.IOException;
import java.util.*;
import java.util.concurrent.TimeUnit;

public final class ApiClient {
    private final OkHttpClient client = new OkHttpClient.Builder().connectTimeout(15, TimeUnit.SECONDS).readTimeout(30, TimeUnit.SECONDS).callTimeout(45, TimeUnit.SECONDS).retryOnConnectionFailure(false).followRedirects(false).build();

    public static class ApiException extends Exception {
        public final int code;

        ApiException(int c, String msg) {
            super(msg);
            code = c;
        }
    }

    public Object request(String base, String token, String path, String method, JSONObject data) throws Exception {
        if (!path.startsWith("/api/")) throw new Exception("Invalid API path.");
        Request.Builder builder = new Request.Builder().url(base + path).header("Accept", "application/json");
        if (!token.isEmpty()) builder.header("Authorization", "Bearer " + token);
        RequestBody body = null;
        if (!method.equals("GET"))
            body = RequestBody.create(data == null ? "{}" : data.toString(), MediaType.get("application/json; charset=utf-8"));
        builder.method(method, body);
        try (Response r = client.newCall(builder.build()).execute()) {
            String text = r.body() == null ? "" : r.body().string();
            Object payload = null;
            try {
                if (!text.isEmpty()) payload = new JSONTokener(text).nextValue();
            } catch (JSONException ignored) {
            }
            if (!r.isSuccessful()) {
                JSONObject p = payload instanceof JSONObject ? (JSONObject) payload : new JSONObject();
                String msg = p.optString("message", p.optString("title", "Request failed (" + r.code() + ")"));
                JSONObject errors = p.optJSONObject("errors");
                if (errors != null) {
                    StringBuilder details = new StringBuilder();
                    Iterator<String> k = errors.keys();
                    while (k.hasNext()) details.append(errors.opt(k.next())).append("\n");
                    msg = msg + "\n" + details;
                }
                throw new ApiException(r.code(), msg);
            }
            if (payload instanceof JSONObject && ((JSONObject) payload).has("data"))
                return ((JSONObject) payload).opt("data");
            return payload;
        } catch (IOException e) {
            throw new Exception("Cannot reach backend. Check Wi-Fi, API address, firewall and that the API is running.", e);
        }
    }

    public List<JSONObject> list(String base, String token, String path) throws Exception {
        List<JSONObject> all = new ArrayList<>();
        for (int page = 1; page <= 1000; page++) {
            Object raw = request(base, token, path + (path.contains("?") ? "&" : "?") + "page=" + page + "&pageSize=100", "GET", null);
            List<JSONObject> items = Domain.objects(raw);
            all.addAll(items);
            if (raw instanceof JSONArray) return all;
            JSONObject p = (JSONObject) raw;
            if (items.size() < 100 || p.has("totalCount") && all.size() >= p.optInt("totalCount"))
                return all;
        }
        throw new Exception("Too many result pages.");
    }
}