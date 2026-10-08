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

public class ActiveJobActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "active";
    }

    @Override
    protected String screenRole() {
        return "Mechanic";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_active_job;
    }

    @Override
    protected void render() throws Exception {
        String selected = getIntent().getStringExtra("jobId");
        JSONObject chosen = selected == null ? null : find(rows("jobs"), "workOrderId", selected);
        if (chosen == null && !rows("jobs").isEmpty()) chosen = rows("jobs").get(0);
        if (chosen == null) {
            card("Active job", "No active assignment. The manager assigns jobs on the web portal.");
            return;
        }
        active(Domain.id(chosen));
    }

    protected void active(String id) {
        activeId = id;
        epoch++;
        stopClock();
        content.removeAllViews();
        message.setText("");
        task((b, t) -> {
            JSONObject result = (JSONObject) api.request(b, t, "/api/wireframe/jobs/" + id, "GET", null);
            result.put("mobileSessions", new JSONArray(api.list(b, t, "/api/wireframe/mechanic/sessions")));
            return result;
        }, raw -> {
            JSONObject d = (JSONObject) raw;
            JSONObject session = null;
            JSONArray sessions = d.getJSONArray("mobileSessions");
            for (int i = 0; i < sessions.length(); i++) {
                JSONObject s = sessions.getJSONObject(i);
                if (s.optString("workOrderId").equals(id) && Domain.status(s, Domain.TIMER) != 2) {
                    session = s;
                    break;
                }
            }
            LinearLayout host = card(d.optString("workOrderNumber"), text(d, "customerName", "serviceName") + "\nStatus: " + Domain.state(d, Domain.JOB) + "\nRequirements: " + Domain.strip(d.optString("description")));
            btn(host, "View all records", () -> details(id));
            LinearLayout timer = card("Job timer", "");
            TextView clock = ((View) timer.getParent()).findViewById(R.id.cardText);
            final JSONObject timing = session;
            ticking = new Runnable() {
                public void run() {
                    try {
                        long seconds = 0;
                        if (timing != null) {
                            Instant end = Domain.status(timing, Domain.TIMER) == 1 ? instant(timing.optString("pausedAt")) : Instant.now();
                            seconds = Math.max(0, Duration.between(instant(timing.optString("startedAt")), end).getSeconds() - timing.optLong("pauseSeconds"));
                        }
                        clock.setText(String.format(Locale.ROOT, "%02d:%02d:%02d", seconds / 3600, seconds / 60 % 60, seconds % 60));
                    } catch (Exception e) {
                        clock.setText("Timer unavailable — tap Refresh.");
                    }
                    handler.postDelayed(this, 1000);
                }
            };
            ticking.run();
            if (session == null)
                btn(timer, "Start timer", () -> mutate("/api/mechanic/timer/start", "POST", Domain.json("workOrderId", id)));
            else {
                boolean paused = Domain.status(session, Domain.TIMER) == 1;
                btn(timer, paused ? "Resume timer" : "Pause timer", () -> mutate("/api/wireframe/mechanic/timer/" + id, "PATCH", Domain.json("resume", paused)));
                if (paused)
                    card("Paused timer", "Resume the timer before stopping it or completing this job.");
                else
                    btn(timer, "Stop timer", () -> mutate("/api/mechanic/timer/end", "POST", Domain.json("workOrderId", id)));
            }
            int status = Domain.status(d, Domain.JOB);
            if (status == 1 || status == 3)
                btn(host, "Start / resume work", () -> mutate("/api/mechanic/jobs/" + id + "/status", "PATCH", Domain.json("status", 2, "notes", null)));
            if (status == 2) {
                btn(host, "Waiting for parts", () -> mutate("/api/mechanic/jobs/" + id + "/status", "PATCH", Domain.json("status", 3, "notes", null)));
                btn(host, "Complete job", () -> form("Complete job — customer-visible technician notes", fields(f("notes", "Technician notes").type("textarea").max(4000)), (body, dialog) -> submit("/api/wireframe/mechanic/jobs/" + id + "/complete", "POST", body, dialog)));
            }
            btn(content, "Save work checklist / notes", () -> workRecord(id));
            btn(content, "Add diagnostic finding", () -> noteForm("diagnostics", "finding", "Finding", "severity", "Severity", id));
            btn(content, "Add repair action", () -> noteForm("repairs", "action", "Action performed", "notes", "Repair notes", id));
            btn(content, "Add recommendation", () -> noteForm("recommendations", "recommendation", "Recommendation", "priority", "Priority", id));
            btn(content, "Request parts", () -> partsForm(id));
        });
    }

    protected Instant instant(String s) {
        return OffsetDateTime.parse(s.endsWith("Z") || s.matches(".*[+-]\\d\\d:\\d\\d$") ? s : s + "Z").toInstant();
    }

    protected void workRecord(String id) {
        String[] labels = {"Initial inspection completed", "Diagnosis completed", "Repair work completed", "Parts installation checked", "Final checks completed"};
        List<FormDialog.Field> specs = new ArrayList<>();
        for (int i = 0; i < labels.length; i++)
            specs.add(f("check" + i, labels[i]).type("check").optional());
        specs.add(f("notes", "Repair / work performed").type("textarea").optional().max(1500));
        form("Save work record", specs, (d, dialog) -> {
            List<String> done = new ArrayList<>();
            for (int i = 0; i < labels.length; i++)
                if (d.optBoolean("check" + i)) done.add(labels[i]);
            if (done.isEmpty() && d.isNull("notes"))
                throw new Exception("Select completed checks or enter work notes.");
            JSONObject payload = Domain.json("workOrderId", id, "action", done.isEmpty() ? "Work progress record" : String.join("; ", done), "notes", d.opt("notes"));
            submit("/api/mechanic/repairs", "POST", payload, dialog);
        });
    }
}