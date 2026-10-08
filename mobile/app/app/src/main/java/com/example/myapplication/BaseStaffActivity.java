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

public abstract class BaseStaffActivity extends Activity {
    protected interface Work {
        Object run(String base, String token) throws Exception;
    }

    protected interface Done {
        void accept(Object value) throws Exception;
    }

    protected interface RowAction {
        void actions(LinearLayout host, JSONObject row);
    }

    protected interface Filter {
        boolean keep(JSONObject row);
    }

    protected final ApiClient api = new ApiClient();
    protected final ExecutorService worker = Executors.newSingleThreadExecutor();
    protected final Handler handler = new Handler(Looper.getMainLooper());
    protected LinearLayout content;
    protected TextView message;
    protected ProgressBar progress;
    protected String module = "", activeId = "";
    protected int epoch = 0;
    protected boolean saving = false;
    protected Runnable ticking;
    protected FormDialog openForm;
    protected final Map<String, List<JSONObject>> data = new HashMap<>();

    protected abstract String screenId();

    protected abstract String screenRole();

    protected abstract int screenLayout();

    protected abstract void render() throws Exception;

    @Override
    public void onCreate(Bundle state) {
        super.onCreate(state);
        if (Session.token.isEmpty()) {
            login();
            return;
        }
        if (!screenRole().isEmpty() && !screenRole().equals(Session.role)) {
            dashboard();
            return;
        }
        module = screenId();
        setContentView(screenLayout());
        logo();
        content = findViewById(R.id.content);
        message = findViewById(R.id.message);
        progress = findViewById(R.id.progress);
        ((TextView) findViewById(R.id.identity)).setText(Session.name);
        ((TextView) findViewById(R.id.role)).setText((advisor() ? "Service Advisor" : "Mechanic") + " · " + Session.email);
        findViewById(R.id.logout).setOnClickListener(v -> new AlertDialog.Builder(this).setMessage("Sign out?").setPositiveButton("Sign out", (d, w) -> {
            Session.clear();
            login();
        }).setNegativeButton("Cancel", null).show());
        findViewById(R.id.refresh).setOnClickListener(v -> load());
        findViewById(R.id.home).setOnClickListener(v -> dashboard());
        bindNavigation();
        load();
    }

    protected void bindNavigation() {
    }

    protected void navigate(Class<?> target) {
        startActivity(new Intent(this, target));
    }

    protected void navigateJob(Class<?> target, String id) {
        Intent intent = new Intent(this, target);
        if (id != null) intent.putExtra("jobId", id);
        startActivity(intent);
    }

    protected void dashboard() {
        Intent next = new Intent(this, advisor() ? AdvisorDashboardActivity.class : MechanicDashboardActivity.class);
        next.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        startActivity(next);
        if (!(this instanceof AdvisorDashboardActivity) && !(this instanceof MechanicDashboardActivity))
            finish();
    }

    protected void login() {
        Session.clear();
        Intent next = new Intent(this, LoginActivity.class);
        next.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        startActivity(next);
        finish();
    }

    protected void active(String id) {
        navigateJob(ActiveJobActivity.class, id);
    }

    protected void noteForm(String path, String key, String label, String second, String secondLabel, String id) {
        navigateJob(MechanicNotesActivity.class, id);
    }

    protected void partsForm(String id) {
        navigateJob(PartsRequestActivity.class, id);
    }

    protected boolean advisor() {
        return Session.role.equals("ServiceAdvisor");
    }

    protected void logo() {
        ImageView image = findViewById(R.id.logo);
        try (InputStream in = getAssets().open("brand-logo.png")) {
            image.setImageDrawable(Drawable.createFromStream(in, "brand-logo"));
        } catch (Exception ignored) {
        }
    }

    protected void task(Work work, Done done) {
        int ticket = epoch;
        String base = Session.base, token = Session.token;
        progress.setVisibility(View.VISIBLE);
        worker.execute(() -> {
            try {
                Object value = work.run(base, token);
                runOnUiThread(() -> {
                    if (isDestroyed() || ticket != epoch) return;
                    progress.setVisibility(View.GONE);
                    try {
                        done.accept(value);
                    } catch (Exception e) {
                        error(e);
                    }
                });
            } catch (Exception e) {
                runOnUiThread(() -> {
                    if (isDestroyed() || ticket != epoch) return;
                    progress.setVisibility(View.GONE);
                    error(e);
                });
            }
        });
    }

    protected void stopClock() {
        if (ticking != null) {
            handler.removeCallbacks(ticking);
            ticking = null;
        }
    }

    protected void load() {
        if (saving) return;
        epoch++;
        stopClock();
        content.removeAllViews();
        message.setText("");
        String chosen = module, selectedDetail = activeId;
        boolean isAdvisor = advisor();
        task((base, token) -> {
            Map<String, List<JSONObject>> result = new HashMap<>();
            if (chosen.equals("notifications"))
                result.put("rows", api.list(base, token, "/api/notifications"));
            else if (isAdvisor) {
                if (Arrays.asList("overview", "inspection", "jobs", "diagnostics", "handover", "estimates", "invoices", "modifications").contains(chosen))
                    result.put("jobs", api.list(base, token, "/api/work-orders"));
                if (Arrays.asList("overview", "bookings").contains(chosen))
                    result.put("bookings", api.list(base, token, "/api/integration/appointments"));
                if (Arrays.asList("bookings", "jobs", "modifications").contains(chosen))
                    result.put("services", api.list(base, token, "/api/services"));
                if (Arrays.asList("intake", "jobs").contains(chosen))
                    result.put("customers", api.list(base, token, "/api/customers"));
                if (chosen.equals("jobs"))
                    result.put("vehicles", api.list(base, token, "/api/vehicles"));
                if (Arrays.asList("overview", "estimates", "invoices").contains(chosen))
                    result.put("estimates", api.list(base, token, "/api/estimates"));
                if (chosen.equals("invoices"))
                    result.put("rows", api.list(base, token, "/api/invoices"));
                if (chosen.equals("modifications"))
                    result.put("rows", api.list(base, token, "/api/modification-requests"));
                if (chosen.equals("emergency"))
                    result.put("rows", api.list(base, token, "/api/integration/emergencies"));
            } else {
                if (Arrays.asList("overview", "assigned", "active", "notes", "parts").contains(chosen))
                    result.put("jobs", api.list(base, token, "/api/mechanic/jobs"));
                if (Arrays.asList("overview", "active").contains(chosen))
                    result.put("sessions", api.list(base, token, "/api/wireframe/mechanic/sessions"));
                if (Arrays.asList("overview", "completed").contains(chosen))
                    result.put("completed", api.list(base, token, "/api/mechanic/completed-jobs"));
                if (chosen.equals("parts")) {
                    result.put("parts", api.list(base, token, "/api/integration/parts"));
                    result.put("rows", api.list(base, token, "/api/wireframe/mechanic/requisitions"));
                }
            }
            return result;
        }, value -> {
            data.clear();
            data.putAll((Map<String, List<JSONObject>>) value);
            if (!selectedDetail.isEmpty() && !advisor() && find(rows("jobs"), "workOrderId", selectedDetail) != null)
                active(selectedDetail);
            else {
                activeId = "";
                render();
            }
        });
    }

    protected List<JSONObject> rows(String key) {
        return data.getOrDefault(key, Collections.emptyList());
    }

    protected List<JSONObject> filter(List<JSONObject> source, Filter f) {
        List<JSONObject> out = new ArrayList<>();
        for (JSONObject r : source) if (f.keep(r)) out.add(r);
        return out;
    }

    protected List<FormDialog.Option> choices(List<JSONObject> source) {
        List<FormDialog.Option> out = new ArrayList<>();
        for (JSONObject r : source)
            out.add(new FormDialog.Option(r.has("workOrderNumber") ? Domain.id(r) : r.optString("id"), Domain.title(r) + " · " + r.optString("vehicleReg", r.optString("vehicle", r.optString("email", "")))));
        return out;
    }

    protected List<FormDialog.Option> numbered(String... labels) {
        List<FormDialog.Option> out = new ArrayList<>();
        for (int i = 0; i < labels.length; i++) out.add(new FormDialog.Option("" + i, labels[i]));
        return out;
    }

    protected JSONObject find(List<JSONObject> source, String key, String value) {
        for (JSONObject r : source) if (r.optString(key).equals(value)) return r;
        return null;
    }

    protected FormDialog.Field f(String key, String label) {
        return new FormDialog.Field(key, label);
    }

    protected FormDialog.Field select(String key, String label, List<JSONObject> options) {
        return f(key, label).choices(choices(options));
    }

    protected List<FormDialog.Field> fields(FormDialog.Field... items) {
        return new ArrayList<>(Arrays.asList(items));
    }

    protected List<FormDialog.Field> prices() {
        return fields(f("laborCost", "Labour cost (LKR)").type("number").value(0), f("partsCost", "Parts cost (LKR)").type("number").value(0), f("taxAmount", "Tax (LKR)").type("number").value(0), f("discountAmount", "Discount (LKR)").type("number").value(0), f("notes", "Notes").type("textarea").optional());
    }

    protected void form(String title, List<FormDialog.Field> specs, FormDialog.Submit submit) {
        openForm = new FormDialog(this, title, specs, submit);
    }

    protected void submit(String path, String method, JSONObject payload, FormDialog form) {
        if (saving) {
            form.fail("A save is already in progress.");
            return;
        }
        saving = true;
        String base = Session.base, token = Session.token;
        int ticket = epoch;
        worker.execute(() -> {
            try {
                api.request(base, token, path, method, payload);
                runOnUiThread(() -> {
                    saving = false;
                    if (isDestroyed()) return;
                    form.close();
                    if (ticket == epoch) {
                        Toast.makeText(this, "Saved successfully", Toast.LENGTH_SHORT).show();
                        load();
                    }
                });
            } catch (Exception e) {
                runOnUiThread(() -> {
                    saving = false;
                    if (isDestroyed()) return;
                    if (ticket != epoch) {
                        form.close();
                        return;
                    }
                    if (e instanceof ApiClient.ApiException && ((ApiClient.ApiException) e).code == 401) {
                        form.close();
                        error(e);
                    } else if (form.showing()) form.fail(e.getMessage());
                });
            }
        });
    }

    protected void mutate(String path, String method, JSONObject payload) {
        if (saving) return;
        new AlertDialog.Builder(this).setMessage("Confirm this action?").setPositiveButton("Confirm", (d, w) -> {
            if (saving) return;
            saving = true;
            String base = Session.base, token = Session.token;
            int ticket = epoch;
            progress.setVisibility(View.VISIBLE);
            worker.execute(() -> {
                try {
                    api.request(base, token, path, method, payload);
                    runOnUiThread(() -> {
                        saving = false;
                        if (isDestroyed() || ticket != epoch) return;
                        Toast.makeText(this, "Saved successfully", Toast.LENGTH_SHORT).show();
                        load();
                    });
                } catch (Exception e) {
                    runOnUiThread(() -> {
                        saving = false;
                        if (isDestroyed() || ticket != epoch) return;
                        progress.setVisibility(View.GONE);
                        error(e);
                    });
                }
            });
        }).setNegativeButton("Cancel", null).show();
    }

    protected void btn(LinearLayout host, String text, Runnable action) {
        Button b = new Button(this);
        b.setText(text);
        b.setAllCaps(false);
        b.setMinHeight(48);
        b.setOnClickListener(v -> {
            if (!saving) action.run();
        });
        host.addView(b);
    }

    protected LinearLayout card(String title, String text) {
        LinearLayout c = (LinearLayout) getLayoutInflater().inflate(R.layout.record_card, content, false);
        ((TextView) c.findViewById(R.id.cardTitle)).setText(title);
        ((TextView) c.findViewById(R.id.cardText)).setText(text);
        content.addView(c);
        return c.findViewById(R.id.cardActions);
    }

    protected String text(JSONObject r, String... keys) {
        StringBuilder out = new StringBuilder();
        for (String key : keys) {
            Object value = r.opt(key);
            if (value != null && value != JSONObject.NULL && !(value instanceof JSONObject) && !(value instanceof JSONArray))
                out.append(Domain.label(key)).append(": ").append(value).append("\n");
        }
        return out.toString().trim();
    }

    protected void records(String title, List<JSONObject> list, String[] columns, String[] statuses, RowAction action) {
        if (list.isEmpty()) {
            card(title, "No records yet.");
            return;
        }
        EditText search = new EditText(this);
        search.setHint("Search " + title);
        search.setSingleLine(true);
        content.addView(search);
        List<View> views = new ArrayList<>();
        for (JSONObject r : list) {
            String summary = text(r, columns);
            if (statuses != null) summary += "\nStatus: " + Domain.state(r, statuses);
            LinearLayout host = card(Domain.title(r), summary);
            View view = (View) host.getParent();
            view.setTag((Domain.title(r) + " " + summary).toLowerCase(Locale.ROOT));
            views.add(view);
            if (action != null) action.actions(host, r);
        }
        search.addTextChangedListener(new TextWatcher() {
            public void beforeTextChanged(CharSequence s, int st, int c, int a) {
            }

            public void onTextChanged(CharSequence s, int st, int b, int c) {
                for (View v : views)
                    v.setVisibility(v.getTag().toString().contains(s.toString().toLowerCase(Locale.ROOT)) ? View.VISIBLE : View.GONE);
            }

            public void afterTextChanged(Editable e) {
            }
        });
    }

    protected void jobRecords(List<JSONObject> jobs, boolean active) {
        records("Jobs", jobs, new String[]{"customerName", "customer", "vehicleReg", "vehicle", "serviceName", "service", "assignedAt", "completedAt"}, Domain.JOB, (host, r) -> {
            btn(host, "View records", () -> details(Domain.id(r)));
            if (active) btn(host, "Open active job", () -> active(Domain.id(r)));
        });
    }

    protected void document(JSONObject row, String kind) {
        String text = kind + " " + Domain.title(row) + "\n";
        JSONObject job = row.optJSONObject("workOrder");
        if (job != null) text += "Job: " + job.optString("workOrderNumber") + "\n";
        text += text(row, "laborCost", "partsCost", "taxAmount", "discountAmount", "totalAmount", "amountPaid", "balanceDue", "notes") + "\nStatus: " + Domain.state(row, kind.equals("Invoice") ? Domain.INVOICE : Domain.ESTIMATE);
        final String report = text;
        new AlertDialog.Builder(this).setTitle(Domain.title(row)).setMessage(report).setPositiveButton("Print / PDF", (d, w) -> ReportPrinter.print(this, Domain.title(row), report)).setNegativeButton("Close", null).show();
    }

    protected void details(String id) {
        task((b, t) -> api.request(b, t, "/api/wireframe/jobs/" + id, "GET", null), raw -> {
            JSONObject d = (JSONObject) raw;
            StringBuilder text = new StringBuilder(text(d, "workOrderNumber", "customerName", "serviceName", "technicianNotes", "startedAt", "completedAt"));
            text.append("\nStatus: ").append(Domain.state(d, Domain.JOB));
            JSONObject v = d.optJSONObject("vehicle");
            if (v != null)
                text.append("\n").append(text(v, "registrationNumber", "make", "model", "year"));
            text.append("\nRequirements: ").append(Domain.strip(d.optString("description")));
            for (String type : new String[]{"diagnostics", "repairs", "recommendations"}) {
                text.append("\n\n").append(Domain.label(type)).append("\n");
                JSONArray a = d.optJSONArray(type);
                if (a == null || a.length() == 0) text.append("No records.\n");
                else for (int i = 0; i < a.length(); i++)
                    text.append(text(a.getJSONObject(i), "finding", "severity", "action", "notes", "recommendation", "priority", "createdAt")).append("\n\n");
            }
            for (String type : new String[]{"inspection", "handover"}) {
                JSONObject block = Domain.block(d.opt(type));
                if (block.length() > 0) {
                    text.append("\n").append(Domain.label(type)).append("\n");
                    Iterator<String> keys = block.keys();
                    while (keys.hasNext()) {
                        String key = keys.next();
                        text.append(Domain.label(key)).append(": ").append(block.opt(key)).append("\n");
                    }
                }
            }
            TextView view = new TextView(this);
            view.setPadding(24, 16, 24, 16);
            view.setTextIsSelectable(true);
            view.setText(text);
            ScrollView scroll = new ScrollView(this);
            scroll.addView(view);
            new AlertDialog.Builder(this).setTitle(d.optString("workOrderNumber", "Workshop records")).setView(scroll).setPositiveButton("Close", null).show();
        });
    }

    @Override
    protected void onPause() {
        if (ticking != null) handler.removeCallbacks(ticking);
        super.onPause();
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (ticking != null) handler.post(ticking);
    }

    @Override
    protected void onDestroy() {
        stopClock();
        if (openForm != null && openForm.showing()) openForm.close();
        worker.shutdown();
        super.onDestroy();
    }

    protected void error(Exception error) {
        if (error instanceof ApiClient.ApiException && ((ApiClient.ApiException) error).code == 401) {
            Toast.makeText(this, "Session expired. Sign in again.", Toast.LENGTH_LONG).show();
            login();
        } else message.setText(error.getMessage());
    }
}