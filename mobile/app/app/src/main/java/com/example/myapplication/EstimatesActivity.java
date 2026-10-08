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

public class EstimatesActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "estimates";
    }

    @Override
    protected String screenRole() {
        return "ServiceAdvisor";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_estimates;
    }

    @Override
    protected void render() throws Exception {
        estimates();
    }

    protected void estimates() {
        btn(content, "Prepare estimate", () -> {
            List<FormDialog.Field> specs = fields(select("workOrderId", "Job card", filter(rows("jobs"), r -> Domain.status(r, Domain.JOB) != 5)));
            specs.addAll(prices());
            form("Prepare estimate", specs, (d, dialog) -> submit("/api/estimates", "POST", d, dialog));
        });
        records("Estimates", rows("estimates"), new String[]{"laborCost", "partsCost", "taxAmount", "discountAmount", "totalAmount", "notes"}, Domain.ESTIMATE, (host, r) -> {
            btn(host, "View estimate", () -> document(r, "Estimate"));
            if (Domain.status(r, Domain.ESTIMATE) == 0)
                btn(host, "Send to customer", () -> mutate("/api/estimates/" + r.optString("id") + "/send", "POST", null));
        });
    }
}