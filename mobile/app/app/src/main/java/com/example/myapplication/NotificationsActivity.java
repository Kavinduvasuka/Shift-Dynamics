package com.example.myapplication;

import android.app.*;
import android.content.Intent;
import android.os.*;
import android.view.*;
import android.widget.*;
import android.graphics.drawable.Drawable;
import android.text.*;

import org.json.*;

import java.util.*;
import java.util.concurrent.*;
import java.io.InputStream;
import java.time.*;
import java.time.format.DateTimeFormatter;

public class NotificationsActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "notifications";
    }

    @Override
    protected String screenRole() {
        return "";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_notifications;
    }

    @Override
    protected void render() throws Exception {
        notifications();
    }

    protected void notifications() {
        records("Notifications", rows("rows"), new String[]{"body", "createdAt", "isRead"}, null, (host, r) -> {
            if (!r.optBoolean("isRead"))
                btn(host, "Mark read", () -> mutate("/api/notifications/" + r.optString("id") + "/read", "PATCH", null));
        });
    }
}