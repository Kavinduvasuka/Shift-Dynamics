package com.example.myapplication;

import org.json.*;

import java.math.BigDecimal;
import java.net.URI;
import java.util.*;

public final class Domain {
    public static String address(String text, boolean debug) throws Exception {
        URI u = new URI(text.trim());
        String scheme = u.getScheme();
        if (!("https".equals(scheme) || debug && "http".equals(scheme)) || u.getHost() == null || u.getUserInfo() != null || u.getQuery() != null || u.getFragment() != null || !(u.getPath().isEmpty() || u.getPath().equals("/")))
            throw new Exception(debug ? "Enter only an HTTP/HTTPS server address with its port." : "Enter an HTTPS server address.");
        if (Arrays.asList("localhost", "127.0.0.1", "0.0.0.0", "[::1]").contains(u.getHost()))
            throw new Exception("Use the backend PC's IP address. Emulator: use 10.0.2.2.");
        return scheme + "://" + u.getRawAuthority();
    }

    public static String id(JSONObject o) {
        return o.optString("workOrderId", o.optString("id"));
    }

    public static int status(JSONObject o, String... names) {
        String s = o.optString("status");
        try {
            return Integer.parseInt(s);
        } catch (Exception e) {
            for (int i = 0; i < names.length; i++) if (norm(names[i]).equals(norm(s))) return i;
            return -1;
        }
    }

    public static String norm(String s) {
        return s.replaceAll("[^A-Za-z0-9]", "").toLowerCase(Locale.ROOT);
    }

    public static final String[] JOB = {"Open", "Assigned", "In progress", "Waiting for parts", "Completed", "Cancelled"};
    public static final String[] APPOINTMENT = {"Scheduled", "Confirmed", "Completed", "Cancelled", "No show"};
    public static final String[] ESTIMATE = {"Draft", "Sent", "Approved", "Rejected", "Expired"};
    public static final String[] INVOICE = {"Draft", "Issued", "Partially paid", "Paid", "Overdue", "Cancelled"};
    public static final String[] MODIFICATION = {"Submitted", "Under review", "Quoted", "Approved", "Rejected", "Cancelled"};
    public static final String[] EMERGENCY = {"Pending", "Accepted", "In progress", "Completed", "Cancelled"};
    public static final String[] REQUISITION = {"Pending", "Approved", "Rejected", "Released", "Cancelled"};
    public static final String[] TIMER = {"Active", "Paused", "Ended"};

    public static String state(JSONObject o, String... names) {
        int s = status(o, names);
        return s >= 0 && s < names.length ? names[s] : o.optString("status", "Unknown");
    }

    public static JSONObject json(Object... pairs) {
        JSONObject o = new JSONObject();
        try {
            for (int i = 0; i < pairs.length; i += 2)
                o.put((String) pairs[i], pairs[i + 1] == null ? JSONObject.NULL : pairs[i + 1]);
        } catch (JSONException e) {
            throw new IllegalArgumentException(e);
        }
        return o;
    }

    public static List<JSONObject> objects(Object raw) throws Exception {
        JSONArray a = raw instanceof JSONArray ? (JSONArray) raw : ((JSONObject) raw).getJSONArray("items");
        List<JSONObject> all = new ArrayList<>();
        for (int i = 0; i < a.length(); i++) all.add(a.getJSONObject(i));
        return all;
    }

    public static String title(JSONObject o) {
        for (String k : new String[]{"workOrderNumber", "invoiceNumber", "estimateNumber", "requestType", "registrationNumber", "partNumber", "title", "name", "fullName"})
            if (!o.optString(k).isEmpty()) return o.optString(k);
        if (o.has("firstName")) return o.optString("firstName") + " " + o.optString("lastName");
        return "Record";
    }

    public static JSONObject block(Object raw) {
        try {
            return raw instanceof JSONObject ? (JSONObject) raw : new JSONObject(String.valueOf(raw));
        } catch (Exception e) {
            return new JSONObject();
        }
    }

    public static Object value(JSONObject o, String key) {
        return o.has(key) ? o.opt(key) : o.opt(Character.toUpperCase(key.charAt(0)) + key.substring(1));
    }

    public static String strip(String s) {
        return s.replaceAll("(?s)\\[WF:(INSPECTION|HANDOVER)\\].*?\\[/WF:\\1\\]", "").trim();
    }

    public static String label(String key) {
        return key.replaceAll("([a-z])([A-Z])", "$1 $2").replaceAll("^[a-z]", key.isEmpty() ? "" : key.substring(0, 1).toUpperCase(Locale.ROOT));
    }

    public static BigDecimal decimal(String v, BigDecimal min) throws Exception {
        try {
            BigDecimal n = new BigDecimal(v.trim());
            if (n.compareTo(min) < 0) throw new Exception("Must be at least " + min);
            return n;
        } catch (NumberFormatException e) {
            throw new Exception("Enter a valid number.");
        }
    }

    public static boolean approvedForJob(JSONObject estimate, String jobId) {
        return status(estimate, ESTIMATE) == 2 && estimate.optString("workOrderId").equals(jobId);
    }

    private Domain() {
    }
}