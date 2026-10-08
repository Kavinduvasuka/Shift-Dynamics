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

public class HandoverActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "handover";
    }

    @Override
    protected String screenRole() {
        return "ServiceAdvisor";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_handover;
    }

    @Override
    protected void render() throws Exception {
        checklists(true);
    }

    protected void checklists(boolean handover) {
        records(handover ? "Handover" : "Inspection", filter(rows("jobs"), r -> handover ? Domain.status(r, Domain.JOB) == 4 : Domain.status(r, Domain.JOB) < 4), new String[]{"customerName", "vehicleReg", "serviceName"}, Domain.JOB, (host, r) -> {
            btn(host, "View records", () -> details(Domain.id(r)));
            btn(host, handover ? "Save handover" : "Inspect vehicle", () -> checklistForm(r, handover));
        });
    }

    protected void checklistForm(JSONObject row, boolean handover) {
        String kind = handover ? "handover" : "inspection", id = Domain.id(row);
        task((b, t) -> api.request(b, t, "/api/wireframe/jobs/" + id, "GET", null), raw -> {
            JSONObject old = Domain.block(((JSONObject) raw).opt(kind));
            String[][] items = handover ? new String[][]{{"workComplete", "Workshop work completed"}, {"qualityChecked", "Final quality checked"}, {"customerInformed", "Customer informed"}, {"paymentConfirmed", "All invoices fully paid"}, {"documentsGiven", "Keys and documents handed over"}} : new String[][]{{"body", "Body checked"}, {"tires", "Tires checked"}, {"lights", "Lights checked"}, {"brakes", "Brakes checked"}, {"fluids", "Fluids checked"}, {"interior", "Interior checked"}};
            List<FormDialog.Field> specs = new ArrayList<>();
            for (String[] item : items) {
                FormDialog.Field f = f(item[0], item[1]).type("check").value(Domain.value(old, item[0]));
                if (!handover) f.optional();
                specs.add(f);
            }
            specs.add(f("notes", "Notes").type("textarea").optional().max(handover ? 300 : 400).value(Domain.value(old, "notes")));
            form(handover ? "Handover — completed job, paid invoices required" : "Vehicle inspection", specs, (d, dialog) -> {
                if (d.isNull("notes")) d.put("notes", "");
                submit("/api/wireframe/jobs/" + id + "/" + kind, "PUT", d, dialog);
            });
        });
    }
}