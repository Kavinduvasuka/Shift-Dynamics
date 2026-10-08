# Separate Java Activities and XML layouts — FixFlow Staff

The supplied ZIP was a blank Java Android starter; it contained no Print Xpress screen implementations. This update follows your requested one-Activity/one-layout structure using your existing com.example.myapplication package, .gradle.kts files and Gradle wrapper.

Each screen's business actions live in its corresponding Activity. BaseStaffActivity shares network/background loading, cards, common forms, login guards, printing and lifecycle code. It is not a single navigation screen. Navigation between screens uses Android Intents. Reusable header/field/card XML layouts are shared with includes/inflation. Backend data creates searchable record cards at runtime, so the layout editor cannot show real records before the app connects.

| Activity | XML layout |
|---|---|
| LoginActivity.java | activity_login.xml |
| AdvisorDashboardActivity.java | activity_advisor_dashboard.xml |
| BookingsActivity.java | activity_bookings.xml |
| CustomerIntakeActivity.java | activity_customer_intake.xml |
| InspectionActivity.java | activity_inspection.xml |
| JobCardsActivity.java | activity_job_cards.xml |
| DiagnosticRecordsActivity.java | activity_diagnostic_records.xml |
| EstimatesActivity.java | activity_estimates.xml |
| InvoicesActivity.java | activity_invoices.xml |
| HandoverActivity.java | activity_handover.xml |
| ModificationsActivity.java | activity_modifications.xml |
| EmergencyRequestsActivity.java | activity_emergency_requests.xml |
| MechanicDashboardActivity.java | activity_mechanic_dashboard.xml |
| AssignedJobsActivity.java | activity_assigned_jobs.xml |
| ActiveJobActivity.java | activity_active_job.xml |
| MechanicNotesActivity.java | activity_mechanic_notes.xml |
| PartsRequestActivity.java | activity_parts_request.xml |
| CompletedJobsActivity.java | activity_completed_jobs.xml |
| NotificationsActivity.java | activity_notifications.xml |

MainActivity is the launcher and forwards to LoginActivity. All Activities are registered in AndroidManifest.xml. Java app source remains Java; .gradle.kts is build configuration. Minimum Android version is 8 (API 26), needed for java.time. The SDK/Gradle versions come from your supplied project. Your daemon configuration requests JDK 21; keep Android Studio's Gradle JDK compatible with it.

## Apply to your existing Android project
Download Install-FixFlow-SeparateScreens.ps1 and run it from inside MyApplication, the folder containing app and settings.gradle.kts. It backs up overwritten files under .fixflow-backups. The script does not edit the website, API source or SQL database. It can also target the prior FixFlowAndroid structure by converting its .gradle files to the supplied .gradle.kts setup and moving the old lk.fixflow.staff source tree into the backup, preventing stale Java references. For that conversion it uses the existing Gradle 8.11.1 bootstrap once to regenerate a Gradle 9.4.1 wrapper.

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
& "$env:USERPROFILE\Downloads\Install-FixFlow-SeparateScreens.ps1" -ProjectPath (Get-Location).Path
```

Open the exact target project root in Android Studio, sync, and run. From MyApplication, .\Build-Debug.ps1 uses the included Gradle wrapper to run the nine domain tests and build the debug APK. Initial build needs Android SDK Platform 36.1 matching your existing setup, build tools and internet for dependencies. Emulator API address: http://10.0.2.2:5174 when the API runs on the same PC. Use an existing advisor or mechanic account. Run the ASP.NET Core API in Development on http://0.0.0.0:5174. The existing /api/wireframe endpoints must remain installed.

All earlier advisor/mechanic API actions are retained: booking confirmation/job creation, customer/vehicle intake, inspection, job details, diagnostics/repairs/recommendations, estimates, invoices with approved-estimate/job matching, PDF printing, handover, modifications, emergency status, mechanic timers, work records, parts requests, completion notes, completed jobs and notifications. Refresh reloads shared server data. No offline saving or push notifications are added. Tokens are memory-only; release builds require HTTPS.

## Verify on emulator
Customer books on web → advisor confirms/creates job in mobile → manager assigns on web → mechanic starts timer, records findings/actions/recommendations and requests parts → storekeeper releases on web → mechanic completes with technician notes → advisor sends estimate/creates invoice → manager issues invoice and customer pays on web → advisor saves handover → customer views notes in service history. Refresh each interface to see changes. Test role restrictions, token expiry, network failures, matching estimate selection and timer pause/resume.

Java source syntax, XML validity, resource references and manifest Activity registrations are checked. Android SDK build, unit-test execution and emulator workflow could not run in this authoring environment; these must be performed on your PC.