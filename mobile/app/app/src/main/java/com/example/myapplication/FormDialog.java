package com.example.myapplication;

import android.app.*;
import android.view.*;
import android.widget.*;
import android.text.*;

import org.json.*;

import java.math.BigDecimal;
import java.util.*;

public final class FormDialog {
    public interface Submit {
        void send(JSONObject data, FormDialog form) throws Exception;
    }

    public interface Change {
        void changed();
    }

    public static class Option {
        public final String id, label;

        public Option(String i, String l) {
            id = i;
            label = l;
        }

        @Override
        public String toString() {
            return label;
        }
    }

    public static class Field {
        String key, label, type = "text", value = "";
        boolean optional = false;
        int max = 2000;
        BigDecimal min = BigDecimal.ZERO;
        List<Option> options = new ArrayList<>();

        public Field(String k, String l) {
            key = k;
            label = l;
        }

        public Field type(String t) {
            type = t;
            return this;
        }

        public Field optional() {
            optional = true;
            return this;
        }

        public Field value(Object v) {
            value = v == null || v == JSONObject.NULL ? "" : String.valueOf(v);
            return this;
        }

        public Field max(int m) {
            max = m;
            return this;
        }

        public Field min(int m) {
            min = new BigDecimal(m);
            return this;
        }

        public Field choices(List<Option> o) {
            type = "select";
            options = o;
            return this;
        }
    }

    private final Activity activity;
    private final Map<String, View> inputs = new LinkedHashMap<>();
    private final Map<String, Field> fields = new LinkedHashMap<>();
    private final Map<String, List<Option>> options = new HashMap<>();
    private final AlertDialog dialog;
    private final TextView error;
    private final Submit submit;

    public FormDialog(Activity a, String title, List<Field> specs, Submit action) {
        activity = a;
        submit = action;
        View view = a.getLayoutInflater().inflate(R.layout.dialog_form, null);
        LinearLayout container = view.findViewById(R.id.formFields);
        error = view.findViewById(R.id.formError);
        for (Field f : specs) {
            fields.put(f.key, f);
            View input;
            if (f.type.equals("check")) {
                CheckBox c = (CheckBox) a.getLayoutInflater().inflate(R.layout.field_check, container, false);
                c.setText(f.label);
                c.setChecked(Boolean.parseBoolean(f.value));
                container.addView(c);
                input = c;
            } else if (f.type.equals("select")) {
                View row = a.getLayoutInflater().inflate(R.layout.field_select, container, false);
                ((TextView) row.findViewById(R.id.fieldLabel)).setText(f.label);
                input = row.findViewById(R.id.fieldSpinner);
                container.addView(row);
            } else {
                View row = a.getLayoutInflater().inflate(R.layout.field_text, container, false);
                ((TextView) row.findViewById(R.id.fieldLabel)).setText(f.label + (f.optional ? " (optional)" : ""));
                EditText e = row.findViewById(R.id.fieldInput);
                int type = android.text.InputType.TYPE_CLASS_TEXT | android.text.InputType.TYPE_TEXT_FLAG_CAP_SENTENCES;
                if (f.type.equals("number"))
                    type = android.text.InputType.TYPE_CLASS_NUMBER | android.text.InputType.TYPE_NUMBER_FLAG_DECIMAL;
                if (f.type.equals("integer")) type = android.text.InputType.TYPE_CLASS_NUMBER;
                if (f.type.equals("email"))
                    type = android.text.InputType.TYPE_CLASS_TEXT | android.text.InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS;
                if (f.type.equals("password"))
                    type = android.text.InputType.TYPE_CLASS_TEXT | android.text.InputType.TYPE_TEXT_VARIATION_PASSWORD;
                if (f.type.equals("phone")) type = android.text.InputType.TYPE_CLASS_PHONE;
                if (f.type.equals("textarea")) {
                    type |= android.text.InputType.TYPE_TEXT_FLAG_MULTI_LINE;
                    e.setMinLines(3);
                } else e.setSingleLine(true);
                e.setInputType(type);
                e.setFilters(new InputFilter[]{new InputFilter.LengthFilter(f.max)});
                e.setText(f.value);
                container.addView(row);
                input = e;
            }
            inputs.put(f.key, input);
            if (f.type.equals("select")) setOptions(f.key, f.options, f.value);
        }
        dialog = new AlertDialog.Builder(a).setTitle(title).setView(view).setPositiveButton("Save", null).setNegativeButton("Cancel", null).create();
        dialog.setOnShowListener(d -> dialog.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener(v -> {
            try {
                JSONObject data = collect();
                busy(true);
                submit.send(data, this);
            } catch (Exception ex) {
                fail(ex.getMessage());
            }
        }));
        dialog.show();
    }

    public void setOptions(String key, List<Option> choices, String selected) {
        Field f = fields.get(key);
        List<Option> all = new ArrayList<>();
        all.add(new Option("", f.optional ? "None" : "Select an option"));
        all.addAll(choices);
        options.put(key, all);
        Spinner s = (Spinner) inputs.get(key);
        s.setAdapter(new ArrayAdapter<>(activity, android.R.layout.simple_spinner_dropdown_item, all));
        int index = 0;
        for (int i = 0; i < all.size(); i++) if (all.get(i).id.equals(selected)) index = i;
        s.setSelection(index);
    }

    public void change(String key, Change c) {
        ((Spinner) inputs.get(key)).setOnItemSelectedListener(new android.widget.AdapterView.OnItemSelectedListener() {
            public void onItemSelected(android.widget.AdapterView<?> p, View v, int pos, long id) {
                c.changed();
            }

            public void onNothingSelected(android.widget.AdapterView<?> p) {
            }
        });
    }

    public String selected(String key) {
        Spinner s = (Spinner) inputs.get(key);
        Object o = s.getSelectedItem();
        return o instanceof Option ? ((Option) o).id : "";
    }

    public void text(String key, Object value) {
        ((EditText) inputs.get(key)).setText(value == null || value == JSONObject.NULL ? "" : String.valueOf(value));
    }

    private JSONObject collect() throws Exception {
        JSONObject out = new JSONObject();
        for (Field f : fields.values()) {
            View input = inputs.get(f.key);
            if (f.type.equals("check")) {
                boolean checked = ((CheckBox) input).isChecked();
                if (!f.optional && !checked) throw new Exception("Confirm: " + f.label);
                out.put(f.key, checked);
                continue;
            }
            String s = f.type.equals("select") ? selected(f.key) : ((EditText) input).getText().toString();
            if (!f.type.equals("password")) s = s.trim();
            if (s.isEmpty()) {
                if (!f.optional) throw new Exception(f.label + " is required.");
                out.put(f.key, JSONObject.NULL);
                continue;
            }
            if (f.type.equals("number") || f.type.equals("integer")) {
                BigDecimal number = Domain.decimal(s, f.min);
                if (f.type.equals("integer")) try {
                    out.put(f.key, number.intValueExact());
                } catch (ArithmeticException ex) {
                    throw new Exception(f.label + ": enter a whole number.");
                }
                else out.put(f.key, number);
            } else {
                if (f.type.equals("email") && !android.util.Patterns.EMAIL_ADDRESS.matcher(s).matches())
                    throw new Exception("Enter a valid email address.");
                if (f.type.equals("password") && s.length() < 8)
                    throw new Exception("Password must contain at least 8 characters.");
                out.put(f.key, s);
            }
        }
        return out;
    }

    public void busy(boolean value) {
        dialog.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(!value);
        dialog.getButton(AlertDialog.BUTTON_NEGATIVE).setEnabled(!value);
        dialog.setCancelable(!value);
        error.setText(value ? "Saving…" : "");
    }

    public void fail(String text) {
        busy(false);
        error.setText(text);
    }

    public void close() {
        dialog.dismiss();
    }

    public boolean showing() {
        return dialog.isShowing();
    }
}