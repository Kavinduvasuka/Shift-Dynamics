# FixFlow installed into your MyApplication structure

Your existing Java package com.example.myapplication, Kotlin DSL .gradle.kts files, Gradle version catalog, wrapper scripts/JAR, SDK 36.1 configuration and launcher icons are retained. The app source is Java, with XML layouts. activity_main.xml is the login screen; activity_portal.xml is the advisor/mechanic portal container. Other XML layouts define cards and reusable form fields. MainActivity, ApiClient, Domain, FormDialog, Session and ReportPrinter are under app/src/main/java/com/example/myapplication.

Open the MyApplication root folder (the folder containing settings.gradle.kts and gradlew.bat), then sync. Your uploaded project sets its Gradle daemon to JDK 21; keep that setting. App Java source/target compatibility is 17, and minimum SDK is raised to 26 for java.time. Gradle/AGP versions and the SDK target are preserved. No Kotlin app code or WebView is introduced. Existing .gradle.kts files are build configuration.

To modify the MyApplication on your PC, download Install-FixFlow-MyApplication.ps1, open PowerShell inside that MyApplication folder and run:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
& "$env:USERPROFILE\Downloads\Install-FixFlow-MyApplication.ps1" -ProjectPath (Get-Location).Path
```

The installer backs up every overwritten file to .fixflow-backups, adds native files and configures the dependencies. It checks the expected namespace and makes additive Gradle edits to your current file. It leaves the web API and SQL database alone. Optional -GarageProjectPath identifies your existing Shift-Dynamics repository so its logo can be copied; it defaults to your previously supplied project path. The corrected ZIP is an alternative: extract it into a new folder and open its MyApplication root, without mixing it into the old FixFlowAndroid project.

Build from Android Studio, or run .\Build-Debug.ps1 in MyApplication. The build uses your existing gradlew.bat and runs the included unit tests. Android SDK Platform 36.1 and the matching build tools for your existing configuration must be installed. First sync/build needs internet for dependencies and, if needed, the daemon JDK download. The zip excludes your machine-specific local.properties, Gradle caches and IDE workspace settings; Android Studio can regenerate these.

## Connect to your backend
From the existing project root, start the API:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project '.\backend\ShiftDynamics.API\ShiftDynamics.API.csproj' --urls 'http://0.0.0.0:5174'
```

Phone and API PC must be on the same network, with TCP 5174 allowed through Windows Firewall on that trusted private network. In the app enter http://YOUR_API_PC_IP:5174, found using ipconfig. Do not enter localhost or the SQL server address unless the API actually runs on that computer. For the Android emulator use http://10.0.2.2:5174 when the API runs on its host. Your API must still connect to its existing SQL server. Live Server is not needed for the Android interfaces.

Use an existing ServiceAdvisor or Mechanic email/password. Other roles are rejected by mobile login. Role and ownership permissions remain enforced by the backend. Only the API address and email are remembered; access tokens live in process memory, passwords are not stored. Closing the process requires signing in again. Debug builds permit HTTP for same-network testing. Release builds require HTTPS and do not permit cleartext.

## Advisor functions
- Workshop overview with booking/job/estimate counts.
- Search customer bookings, confirm booking, create work order from confirmed booking.
- Register customer and add vehicle.
- Walkaround inspection checklist, preserving previously recorded checks/notes.
- Create job cards with customer-specific vehicle selection.
- View diagnostics, repair actions, recommendations and technician notes.
- Prepare estimates and send draft estimates to customer.
- Create invoices: only approved estimates for the selected work order appear. Selecting one fills cost fields; choosing None permits direct invoice compilation under existing API rules.
- View invoice/estimate details and print/save PDF through the Android print system.
- Handover checklist/notes with completed-job and paid-invoice checks enforced by API.
- Review/price/reject modifications; create or view job for an accepted quote.
- Update emergency request status.
- View notifications and mark read.

## Mechanic functions
- Overview, searchable assigned and completed jobs.
- Active job station and running timer display.
- Start/pause/resume/stop timers. A paused timer must be resumed before stopping or completing.
- Start/resume job and mark waiting for parts.
- Save work checklist and work notes as a repair record.
- Record diagnostics, repair actions and recommendations.
- Request parts with quantity/specification/urgency and view requisition status.
- Complete an in-progress job with required customer-visible technician notes.
- View all diagnostic, repair, recommendation and completion records.
- View notifications and mark read.

Use the native screen dropdown to navigate. Refresh reloads backend data. This app does not add offline saving, push notifications or automatic cross-device updates. Existing web portals show changes when refreshed. APIs under /api/wireframe must be present from your latest web integration; this package relies on the already installed WireframeController. It does not replace that controller.

## End-to-end checks on your phone
1. Login: advisor and mechanic can log in; customer/manager accounts are rejected; wrong password and unreachable API display errors.
2. Customer books service on web. Advisor sees and confirms on app. Refresh web to verify.
3. Advisor creates work order from booking; manager assigns mechanic on web.
4. Mechanic sees assignment, starts work/timer, pauses/resumes/stops timer. After refresh timer reflects server state.
5. Mechanic saves finding, repair, recommendation and work checklist. Advisor sees all on app and web.
6. Mechanic requests parts. Storekeeper approves/releases on web. Mechanic refreshes requisitions to see status.
7. Mechanic completes in-progress job with notes. Verify completed job, linked booking, and technician notes on web.
8. Advisor creates/sends estimate. Customer approves on web. Advisor creates invoice from that approved estimate. Estimate options must not include another job's estimate.
9. Manager issues invoice on web; customer pays through the existing website flow. Advisor saves handover after all invoices are paid. Customer sees notes in service history.
10. Customer submits modification on web; advisor quotes via app; customer accepts on web; advisor creates modification job from app. Repeat assignment/mechanic workflow.
11. Emergency status changes and read notifications persist between app and web.
12. Disconnect network before saving: error shown, no false success. Reconnect and verify records before retrying; do not assume an interrupted request never reached the API.
13. Token expiry signs user out. PDF printing contains invoice values across all pages.

## Verification performed and limits
Java source syntax was parsed and XML was validated. Resource references and existing API routes/payloads were checked against the supplied project. Nine JVM unit tests for address validation, role restrictions, assignment IDs, estimate/job matching, handover notes, descriptions and money validation are included under app/src/test. Build-Debug.ps1 runs them before building. These tests have not been executed in the authoring environment. The complete Android Gradle build, emulator and phone workflow were not run because the authoring environment has no Android SDK. Source checks cannot prove device behavior. Build and execute the end-to-end checks above before treating the app as ready.

Official references: https://developer.android.com/develop/ui/views/layout/declaring-layout ; https://developer.android.com/build/releases/agp-8-9-0-release-notes ; https://square.github.io/okhttp/
