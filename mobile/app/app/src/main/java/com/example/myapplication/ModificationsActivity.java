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

public class ModificationsActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "modifications";
    }

    @Override
    protected String screenRole() {
        return "ServiceAdvisor";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_modifications;
    }

    @Override
    protected void render() throws Exception {
        modifications();
    }

    protected void modifications() {
        records("Modifications", rows("rows"), new String[]{"customerName", "vehicleRegistration", "requestType", "description", "proposedCost", "advisorNotes"}, Domain.MODIFICATION, (host, r) -> {
            String id = r.optString("id");
            int s = Domain.status(r, Domain.MODIFICATION);
            if (s >= 0 && s < 3) {
                btn(host, "Review / quote", () -> form("Review modification", fields(f("proposedCost", "Proposed price (LKR)").type("number").optional().value(r.opt("proposedCost")), f("advisorNotes", "Advice and scope").type("textarea").optional().value(r.opt("advisorNotes"))), (d, dialog) -> {
                    d.put("status", d.isNull("proposedCost") ? 1 : 2);
                    submit("/api/modification-requests/" + id + "/review", "PATCH", d, dialog);
                }));
                btn(host, "Reject request", () -> form("Reject modification", fields(f("advisorNotes", "Reason").type("textarea")), (d, dialog) -> {
                    d.put("status", 4);
                    d.put("proposedCost", JSONObject.NULL);
                    submit("/api/modification-requests/" + id + "/review", "PATCH", d, dialog);
                }));
            }
            if (s == 3) {
                String number = "WO-MOD-" + id.replace("-", "").toLowerCase(Locale.ROOT);
                JSONObject existing = find(rows("jobs"), "workOrderNumber", number);
                if (existing != null)
                    btn(host, "View modification job", () -> details(existing.optString("id")));
                else
                    btn(host, "Create modification job", () -> form("Accepted modification job", fields(select("serviceId", "Service package", rows("services"))), (d, dialog) -> submit("/api/wireframe/modifications/" + id + "/job", "POST", d, dialog)));
            }
        });
    }
}