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

public class JobCardsActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "jobs";
    }

    @Override
    protected String screenRole() {
        return "ServiceAdvisor";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_job_cards;
    }

    @Override
    protected void render() throws Exception {
        btn(content, "Create job card", () -> createJob(null));
        jobRecords(rows("jobs"), false);
    }

    protected void createJob(JSONObject appointment) {
        List<FormDialog.Field> specs = new ArrayList<>();
        if (appointment == null) {
            specs.add(select("customerId", "Customer", rows("customers")));
            specs.add(select("vehicleId", "Customer vehicle", Collections.emptyList()));
        }
        specs.add(select("serviceId", "Service package", rows("services")));
        specs.add(f("description", "Customer requirements").type("textarea").optional().max(1400).value(appointment == null ? "" : appointment.optString("notes", appointment.optString("serviceType"))));
        form("Create job card", specs, (d, dialog) -> {
            if (appointment != null) {
                d.put("customerId", appointment.optString("customerId"));
                d.put("vehicleId", appointment.optString("vehicleId"));
                d.put("appointmentId", appointment.optString("id"));
            } else d.put("appointmentId", JSONObject.NULL);
            submit("/api/work-orders", "POST", d, dialog);
        });
        if (appointment == null)
            openForm.change("customerId", () -> openForm.setOptions("vehicleId", choices(filter(rows("vehicles"), v -> v.optString("customerId").equals(openForm.selected("customerId")))), ""));
    }
}