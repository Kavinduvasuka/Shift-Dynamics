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

public class PartsRequestActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "parts";
    }

    @Override
    protected String screenRole() {
        return "Mechanic";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_parts_request;
    }

    @Override
    protected void render() throws Exception {
        parts();
    }

    protected void parts() {
        btn(content, "Request parts", () -> partsForm(null));
        records("Parts requests", rows("rows"), new String[]{"partSpec", "qtyRequested", "qtyReleased", "reason", "createdAt"}, Domain.REQUISITION, null);
    }

    protected void partsForm(String passedId) {
        final String id = passedId == null ? getIntent().getStringExtra("jobId") : passedId;
        if (!data.containsKey("parts")) {
            task((b, t) -> api.list(b, t, "/api/integration/parts"), raw -> {
                data.put("parts", (List<JSONObject>) raw);
                partsForm(id);
            });
            return;
        }
        form("Parts requisition", fields(select("workOrderId", "Assigned job", rows("jobs")).value(id), select("partId", "Part", rows("parts")).optional(), f("partSpec", "Part specification").max(500), f("qtyRequested", "Quantity").type("integer").min(1).value(1), f("urgency", "Urgency").choices(numbered("Low", "Normal", "High", "Critical")).value(1), f("reason", "Reason").type("textarea").optional()), (d, dialog) -> {
            d.put("urgency", Integer.parseInt(d.getString("urgency")));
            submit("/api/mechanic/requisitions", "POST", d, dialog);
        });
    }
}