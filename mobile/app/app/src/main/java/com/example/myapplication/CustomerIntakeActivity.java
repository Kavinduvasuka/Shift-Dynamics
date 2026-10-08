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

public class CustomerIntakeActivity extends BaseStaffActivity {
    @Override
    protected String screenId() {
        return "intake";
    }

    @Override
    protected String screenRole() {
        return "ServiceAdvisor";
    }

    @Override
    protected int screenLayout() {
        return R.layout.activity_customer_intake;
    }

    @Override
    protected void render() throws Exception {
        intake();
    }

    protected void intake() {
        btn(content, "Register customer", () -> form("Customer intake", fields(f("fullName", "Full name").max(150), f("email", "Email").type("email").max(191), f("phone", "Phone").type("phone").max(30), f("address", "Address").type("textarea").optional().max(500), f("password", "Initial password").type("password").max(100)), (d, dialog) -> submit("/api/integration/customers", "POST", d, dialog)));
        btn(content, "Add customer vehicle", () -> {
            List<FormDialog.Field> specs = fields(select("customerId", "Customer", rows("customers")));
            specs.addAll(vehicleFields());
            form("Add vehicle", specs, (d, dialog) -> submit("/api/integration/vehicles", "POST", d, dialog));
        });
        records("Customers", rows("customers"), new String[]{"firstName", "lastName", "email", "phone", "address"}, null, null);
    }

    protected List<FormDialog.Field> vehicleFields() {
        return fields(f("registrationNumber", "Registration number").max(20), f("make", "Make").max(50), f("model", "Model").max(50), f("year", "Year").type("integer").min(1900).value(Year.now().getValue()), f("vin", "VIN").optional().max(50), f("color", "Colour").optional().max(30));
    }
}