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

public class MechanicDashboardActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "overview";
    }

    @Override
    protected String screenRole() {
        return "Mechanic";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_mechanic_dashboard;
    }

    @Override
    protected void render() throws Exception {
        overview();
    }

    @Override
    protected void bindNavigation() {
        findViewById(R.id.navAssigned).setOnClickListener(v -> navigate(AssignedJobsActivity.class));
        findViewById(R.id.navActive).setOnClickListener(v -> navigate(ActiveJobActivity.class));
        findViewById(R.id.navNotes).setOnClickListener(v -> navigate(MechanicNotesActivity.class));
        findViewById(R.id.navParts).setOnClickListener(v -> navigate(PartsRequestActivity.class));
        findViewById(R.id.navCompleted).setOnClickListener(v -> navigate(CompletedJobsActivity.class));
        findViewById(R.id.navNotifications).setOnClickListener(v -> navigate(NotificationsActivity.class));
    }

    protected void overview() {
        List<JSONObject> jobs = rows("jobs");
        if (advisor()) {
            card("Workshop overview", "Scheduled bookings: " + filter(rows("bookings"), r -> Domain.status(r, Domain.APPOINTMENT) == 0).size() + "\nActive jobs: " + filter(jobs, r -> Domain.status(r, Domain.JOB) < 4).size() + "\nEstimates awaiting customer: " + filter(rows("estimates"), r -> Domain.status(r, Domain.ESTIMATE) == 1).size() + "\nCompleted jobs: " + filter(jobs, r -> Domain.status(r, Domain.JOB) == 4).size());
        } else {
            card("Mechanic overview", "Assigned jobs: " + jobs.size() + "\nIn progress: " + filter(jobs, r -> Domain.status(r, Domain.JOB) == 2).size() + "\nWaiting for parts: " + filter(jobs, r -> Domain.status(r, Domain.JOB) == 3).size() + "\nCompleted jobs: " + rows("completed").size());
            records("Labor sessions", rows("sessions"), new String[]{"startedAt", "pausedAt", "endedAt", "durationSeconds"}, Domain.TIMER, null);
        }
        jobRecords(jobs, !advisor());
    }
}