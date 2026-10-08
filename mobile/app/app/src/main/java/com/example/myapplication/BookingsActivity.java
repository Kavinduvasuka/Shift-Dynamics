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

public class BookingsActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "bookings";
    }

    @Override
    protected String screenRole() {
        return "ServiceAdvisor";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_bookings;
    }

    @Override
    protected void render() throws Exception {
        bookings();
    }

    protected void bookings() {
        records("Bookings", rows("bookings"), new String[]{"customerName", "vehicleRegistration", "serviceType", "appointmentDate", "notes", "hasJob"}, Domain.APPOINTMENT, (host, r) -> {
            int s = Domain.status(r, Domain.APPOINTMENT);
            if (s == 0)
                btn(host, "Confirm booking", () -> mutate("/api/appointments/" + r.optString("id") + "/confirm", "POST", null));
            if (s == 1 && !r.optBoolean("hasJob")) btn(host, "Create job card", () -> createJob(r));
        });
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