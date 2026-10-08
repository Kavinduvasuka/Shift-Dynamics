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

public class EmergencyRequestsActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "emergency";
    }

    @Override
    protected String screenRole() {
        return "ServiceAdvisor";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_emergency_requests;
    }

    @Override
    protected void render() throws Exception {
        emergency();
    }

    protected void emergency() {
        records("Emergency requests", rows("rows"), new String[]{"customerName", "vehicleRegistration", "location", "problemDescription", "contactName", "contactPhone", "requestedAt"}, Domain.EMERGENCY, (host, r) -> {
            int s = Domain.status(r, Domain.EMERGENCY);
            if (s < 3) {
                for (int state = 1; state <= 4; state++) {
                    final int next = state;
                    btn(host, Domain.EMERGENCY[state], () -> mutate("/api/integration/emergencies/" + r.optString("id"), "PATCH", Domain.json("status", next)));
                }
            }
        });
    }
}