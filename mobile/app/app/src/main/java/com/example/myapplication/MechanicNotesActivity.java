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

public class MechanicNotesActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "notes";
    }

    @Override
    protected String screenRole() {
        return "Mechanic";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_mechanic_notes;
    }

    @Override
    protected void render() throws Exception {
        notes();
    }

    protected void notes() {
        btn(content, "Add diagnostic finding", () -> noteForm("diagnostics", "finding", "Finding", "severity", "Severity", null));
        btn(content, "Add repair action", () -> noteForm("repairs", "action", "Action performed", "notes", "Repair notes", null));
        btn(content, "Add recommendation", () -> noteForm("recommendations", "recommendation", "Recommendation", "priority", "Priority", null));
        jobRecords(rows("jobs"), false);
    }

    protected void noteForm(String path, String key, String label, String second, String secondLabel, String job) {
        if (job == null) job = getIntent().getStringExtra("jobId");
        form(label, fields(select("workOrderId", "Assigned job", rows("jobs")).value(job), f(key, label).type("textarea"), f(second, secondLabel).type(second.equals("notes") ? "textarea" : "text").optional().max(second.equals("notes") ? 2000 : 50)), (d, dialog) -> submit("/api/mechanic/" + path, "POST", d, dialog));
    }
}