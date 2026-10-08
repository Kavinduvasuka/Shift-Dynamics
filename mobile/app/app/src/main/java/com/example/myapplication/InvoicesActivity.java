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

public class InvoicesActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "invoices";
    }

    @Override
    protected String screenRole() {
        return "ServiceAdvisor";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_invoices;
    }

    @Override
    protected void render() throws Exception {
        invoices();
    }

    protected void invoices() {
        btn(content, "Create invoice", () -> {
            List<JSONObject> approved = filter(rows("estimates"), r -> Domain.status(r, Domain.ESTIMATE) == 2);
            List<FormDialog.Field> specs = fields(select("workOrderId", "Job card", filter(rows("jobs"), r -> Domain.status(r, Domain.JOB) != 5)), select("estimateId", "Approved estimate", Collections.emptyList()).optional());
            specs.addAll(prices());
            form("Create invoice", specs, (d, dialog) -> {
                if (!d.isNull("estimateId")) {
                    JSONObject estimate = find(approved, "id", d.optString("estimateId"));
                    if (estimate == null || !Domain.approvedForJob(estimate, d.optString("workOrderId")))
                        throw new Exception("Select an approved estimate belonging to this job.");
                }
                submit("/api/invoices", "POST", d, dialog);
            });
            final FormDialog dialog = openForm;
            dialog.change("workOrderId", () -> {
                dialog.setOptions("estimateId", choices(filter(approved, r -> Domain.approvedForJob(r, dialog.selected("workOrderId")))), "");
                for (String key : new String[]{"laborCost", "partsCost", "taxAmount", "discountAmount"})
                    dialog.text(key, 0);
                dialog.text("notes", "");
            });
            dialog.change("estimateId", () -> {
                JSONObject e = find(approved, "id", dialog.selected("estimateId"));
                for (String key : new String[]{"laborCost", "partsCost", "taxAmount", "discountAmount"})
                    dialog.text(key, e == null ? 0 : e.opt(key));
                dialog.text("notes", e == null ? "" : e.optString("notes"));
            });
        });
        records("Invoices", rows("rows"), new String[]{"laborCost", "partsCost", "taxAmount", "discountAmount", "totalAmount", "amountPaid", "balanceDue", "notes"}, Domain.INVOICE, (host, r) -> btn(host, "View / print invoice", () -> document(r, "Invoice")));
    }
}