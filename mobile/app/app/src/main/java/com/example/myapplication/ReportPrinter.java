package com.example.myapplication;

import android.app.Activity;
import android.content.Context;
import android.graphics.*;
import android.graphics.pdf.PdfDocument;
import android.os.*;
import android.print.*;

import java.io.*;
import java.util.*;

public final class ReportPrinter {
    public static void print(Activity activity, String title, String text) {
        PrintManager manager = (PrintManager) activity.getSystemService(Context.PRINT_SERVICE);
        manager.print(title, new PrintDocumentAdapter() {
            private final List<String> lines = wrap(text, 78);
            private final int pageCount = Math.max(1, (lines.size() + 47) / 48);

            @Override
            public void onLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes, CancellationSignal signal, LayoutResultCallback callback, Bundle extras) {
                if (signal.isCanceled()) {
                    callback.onLayoutCancelled();
                    return;
                }
                callback.onLayoutFinished(new PrintDocumentInfo.Builder(title + ".pdf").setContentType(PrintDocumentInfo.CONTENT_TYPE_DOCUMENT).setPageCount(pageCount).build(), true);
            }

            @Override
            public void onWrite(PageRange[] ranges, ParcelFileDescriptor destination, CancellationSignal signal, WriteResultCallback callback) {
                PdfDocument pdf = new PdfDocument();
                try {
                    Paint paint = new Paint(Paint.ANTI_ALIAS_FLAG);
                    paint.setColor(Color.BLACK);
                    paint.setTextSize(11);
                    for (int page = 0; page < pageCount; page++) {
                        if (signal.isCanceled()) {
                            callback.onWriteCancelled();
                            return;
                        }
                        boolean requested = false;
                        for (PageRange range : ranges)
                            if (page >= range.getStart() && page <= range.getEnd())
                                requested = true;
                        if (!requested) continue;
                        PdfDocument.Page sheet = pdf.startPage(new PdfDocument.PageInfo.Builder(595, 842, page + 1).create());
                        Canvas canvas = sheet.getCanvas();
                        canvas.drawText("FixFlow — " + title, 36, 36, paint);
                        canvas.drawText("Page " + (page + 1) + " / " + pageCount, 36, 810, paint);
                        for (int i = page * 48; i < Math.min(lines.size(), (page + 1) * 48); i++)
                            canvas.drawText(lines.get(i), 36, 66 + (i - page * 48) * 15, paint);
                        pdf.finishPage(sheet);
                    }
                    try (FileOutputStream out = new FileOutputStream(destination.getFileDescriptor())) {
                        pdf.writeTo(out);
                    }
                    callback.onWriteFinished(ranges);
                } catch (Exception e) {
                    callback.onWriteFailed(e.getMessage());
                } finally {
                    pdf.close();
                }
            }
        }, null);
    }

    private static List<String> wrap(String text, int width) {
        List<String> result = new ArrayList<>();
        for (String original : text.split("\n", -1)) {
            String line = original;
            while (line.length() > width) {
                int end = line.lastIndexOf(' ', width);
                if (end < 1) end = width;
                result.add(line.substring(0, end));
                line = line.substring(end).trim();
            }
            result.add(line);
        }
        return result;
    }

    private ReportPrinter() {
    }
}