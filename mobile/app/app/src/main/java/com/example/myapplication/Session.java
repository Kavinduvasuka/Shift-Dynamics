package com.example.myapplication;

import org.json.JSONObject;

public final class Session {
    public static String token = "", role = "", name = "", email = "", base = "";

    public static void login(JSONObject auth, String address) throws Exception {
        String r = auth.optString("role");
        if (!r.equals("ServiceAdvisor") && !r.equals("Mechanic"))
            throw new Exception("Use a Service Advisor or Mechanic account.");
        String t = auth.optString("accessToken");
        if (t.isEmpty()) throw new Exception("Login response has no access token.");
        token = t;
        role = r;
        name = auth.optString("fullName");
        email = auth.optString("email");
        base = address;
    }

    public static void clear() {
        token = "";
        role = "";
        name = "";
        email = "";
        base = "";
    }

    private Session() {
    }
}