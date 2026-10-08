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

public class DiagnosticRecordsActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "diagnostics";
    }

    @Override
    protected String screenRole() {
        return "ServiceAdvisor";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_diagnostic_records;
    }

    @Override
    protected void render() throws Exception {
        jobRecords(rows("jobs"), false);
    }
}