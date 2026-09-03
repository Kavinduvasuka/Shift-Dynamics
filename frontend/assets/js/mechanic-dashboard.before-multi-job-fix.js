document.addEventListener(
    "DOMContentLoaded",
    () => {

        const sidebar = document.getElementById("sidebar");

        const menuButton = document.getElementById("menuButton");

        const pageTitle = document.getElementById("pageTitle");

        const navItems =
            document.querySelectorAll(
                "[data-section]"
            );

        const sections =
            document.querySelectorAll(
                ".sd-content-section"
            );

        const goButtons =
            document.querySelectorAll(
                "[data-go]"
            );

const sectionTitles = {
            overview: "Mechanic Overview",
            "assigned-jobs": "Assigned Jobs",
            "active-job": "Active Job Station",
            diagnostics: "Diagnostics & Repair Notes",
            parts: "Parts Requisition",
            completed: "Completed Jobs"
        };

function openSection(sectionId) {

            const target =
                document.getElementById(sectionId);

            if (!target) {
                return;
            }

sections.forEach(
                section => {
                    section.classList.remove("active");
                }
            );

navItems.forEach(
                item => {
                    item.classList.remove("active");
                }
            );

target.classList.add("active");

const activeNav =
                document.querySelector(
                    `[data-section="${sectionId}"]`
                );

            if (activeNav) {
                activeNav.classList.add("active");
            }

pageTitle.textContent =
                sectionTitles[sectionId] ||
                "Mechanic Dashboard";

sidebar.classList.remove("open");

window.scrollTo({
                top: 0,
                behavior: "smooth"
            });
        }

navItems.forEach(
            item => {

                item.addEventListener(
                    "click",
                    () => {

                        openSection(
                            item.dataset.section
                        );

                    }
                );

            }
        );

goButtons.forEach(
            button => {

                button.addEventListener(
                    "click",
                    () => {

                        openSection(
                            button.dataset.go
                        );

                    }
                );

            }
        );

menuButton.addEventListener(
            "click",
            () => {
                sidebar.classList.toggle("open");
            }
        );

document.addEventListener(
            "keydown",
            event => {

                if (event.key === "Escape") {
                    sidebar.classList.remove("open");
                }

            }
        );

// Active Job Timer

        const jobTimer = document.getElementById("jobTimer");

        const startJobButton = document.getElementById("startJobButton");

        const endJobButton = document.getElementById("endJobButton");

        const pauseJobButton = document.getElementById("pauseJobButton");

        const jobStartTime = document.getElementById("jobStartTime");

        const jobEndTime = document.getElementById("jobEndTime");

        const activeJobStatus = document.getElementById("activeJobStatus");

        const jobTimerMessage = document.getElementById("jobTimerMessage");

        const workChecks = document.querySelectorAll(".sd-work-check");

        const workProgressText = document.getElementById("workProgressText");

        const workProgressBar = document.getElementById("workProgressBar");

        const mechanicWorkNotes = document.getElementById("mechanicWorkNotes");

        const saveWorkNotesButton = document.getElementById("saveWorkNotesButton");

        const workNotesMessage = document.getElementById("workNotesMessage");

        const completionRecord = document.getElementById("completionRecord");

        const recordStartTime = document.getElementById("recordStartTime");

        const recordEndTime = document.getElementById("recordEndTime");

        const recordDuration = document.getElementById("recordDuration");

let timerInterval = null;
        let jobStartedAt = null;
        let elapsedSeconds = 0;
        let jobRunning = false;
        let jobPaused = false;
        let jobCompleted = false;

        const mechanicJobs = {
            "JC-1052": {
                id: "JC-1052",
                vehicle: "Nissan X-Trail",
                plate: "WP CAX-4582",
                service: "Engine Repair",
                bay: "Bay 03",
                priority: "High",
                estimatedFinish: "02:00 PM",
                concern: "Engine vibration at idle with reduced acceleration and abnormal engine noise."
            },
            "JC-1057": {
                id: "JC-1057",
                vehicle: "Toyota Aqua",
                plate: "WP CBE-2714",
                service: "General Service",
                bay: "Bay 02",
                priority: "Normal",
                estimatedFinish: "03:30 PM",
                concern: "Scheduled general service and routine vehicle inspection."
            },
            "JC-1058": {
                id: "JC-1058",
                vehicle: "Honda Vezel",
                plate: "WP CBF-5678",
                service: "Diagnostic Check",
                bay: "Pending",
                priority: "High",
                estimatedFinish: "Pending",
                concern: "Vehicle requires diagnostic inspection before repair work begins."
            }
        };

        let currentMechanicJob = mechanicJobs["JC-1052"];

function formatClockTime(date) {

            return date.toLocaleTimeString(
                [],
                {
                    hour: "2-digit",
                    minute: "2-digit"
                }
            );

        }

function formatDuration(seconds) {

            const hours =
                Math.floor(seconds / 3600);

            const minutes =
                Math.floor((seconds % 3600) / 60);

            const secs =
                seconds % 60;

            return [
                hours,
                minutes,
                secs
            ]
                .map(value =>
                    String(value).padStart(2, "0")
                )
                .join(":");

        }

function updateTimerDisplay() {

            jobTimer.textContent =
                formatDuration(elapsedSeconds);

        }

function updateWorkProgress() {

            const checked =
                [...workChecks].filter(
                    item => item.checked
                ).length;

            const total = workChecks.length;

            const percentage =
                total
                    ? (checked / total) * 100
                    : 0;

            workProgressText.textContent =
                `${checked} / ${total}`;

            workProgressBar.style.width =
                `${percentage}%`;

        }

startJobButton.addEventListener(
            "click",
            () => {

                if (jobRunning || jobCompleted) {
                    return;
                }

                jobStartedAt = new Date();
                elapsedSeconds = 0;
                jobRunning = true;

                jobStartTime.textContent =
                    formatClockTime(jobStartedAt);

                jobEndTime.textContent = "--:--";

                activeJobStatus.textContent =
                    "In Progress";

                activeJobStatus.className =
                    "sd-status sd-status-progress";

                startJobButton.disabled = true;
                pauseJobButton.disabled = false;
                endJobButton.disabled = false;

                startJobButton.innerHTML =
                    '<i class="bi bi-play-fill"></i> Job Running';

                jobTimerMessage.textContent =
                    "Labour timer started successfully.";

                jobTimerMessage.className =
                    "sd-job-message success";

timerInterval = setInterval(
                    () => {

                        elapsedSeconds += 1;
                        function loadMechanicJob(jobId) {
            const job = mechanicJobs[jobId];
            if (!job) return;

            currentMechanicJob = job;

            window.dispatchEvent(new CustomEvent("mechanicJobLoaded", {
                detail: job
            }));

            clearInterval(timerInterval);
            timerInterval = null;
            jobStartedAt = null;
            elapsedSeconds = 0;
            jobRunning = false;
            jobPaused = false;
            jobCompleted = false;

            const activeSection = document.getElementById("active-job");

            if (activeSection) {
                const panels = activeSection.querySelectorAll(".sd-panel");
                const infoPanel = panels[0];

                if (infoPanel) {
                    const title = infoPanel.querySelector(".sd-panel-header h3");
                    const priority = infoPanel.querySelector(".sd-priority");
                    const vehicle = infoPanel.querySelector(".sd-active-vehicle strong");
                    const plate = infoPanel.querySelector(".sd-active-vehicle span");
                    const infoValues = infoPanel.querySelectorAll(".sd-job-info-grid strong");
                    const concern = infoPanel.querySelector(".sd-concern-box p");

                    if (title) title.textContent = `#${job.id}`;

                    if (priority) {
                        priority.textContent = `${job.priority} Priority`;
                        priority.className = `sd-priority ${job.priority.toLowerCase()}`;
                    }

                    if (vehicle) vehicle.textContent = job.vehicle;
                    if (plate) plate.textContent = job.plate;

                    if (infoValues[0]) infoValues[0].textContent = job.service;
                    if (infoValues[1]) infoValues[1].textContent = job.bay;
                    if (infoValues[2]) infoValues[2].textContent = "Workshop Manager";
                    if (infoValues[3]) infoValues[3].textContent = job.estimatedFinish;

                    if (concern) concern.textContent = job.concern;
                }
            }

            jobStartTime.textContent = "--:--";
            jobEndTime.textContent = "--:--";

            activeJobStatus.textContent = "Ready to Start";

            startJobButton.disabled = false;
            pauseJobButton.disabled = true;
            endJobButton.disabled = true;

            startJobButton.innerHTML = '<i class="bi bi-play-fill"></i> Start Job';
            pauseJobButton.innerHTML = '<i class="bi bi-pause-fill"></i> Pause Job';

            mechanicWorkNotes.value = "";
            workNotesMessage.textContent = "";
            jobTimerMessage.textContent = "";

            completionRecord.hidden = true;

            workChecks.forEach(item => {
                item.checked = false;
            });

            updateTimerDisplay();
            updateWorkProgress();

            const completionJob = completionRecord.querySelector(".sd-completion-grid strong");
            if (completionJob) {
                completionJob.textContent = `#${job.id}`;
            }

            openSection("active-job");
        }

        document.querySelectorAll(".sd-job-number").forEach(jobNumber => {
            const jobId = jobNumber.textContent.replace("#", "").trim();

            if (!mechanicJobs[jobId] || jobId === "JC-1052") return;

            const card = jobNumber.closest(".sd-job-card");
            if (!card) return;

            const button = card.querySelector("button");
            if (!button) return;

            button.textContent = "Start Job";

            button.addEventListener("click", () => {
                loadMechanicJob(jobId);
            });
        });
        updateTimerDisplay();

                    },
                    1000
                );

            }
        );

pauseJobButton.addEventListener(
            "click",
            () => {

                if (jobCompleted || !jobRunning) {
                    return;
                }

if (!jobPaused) {

                    clearInterval(timerInterval);
                    timerInterval = null;

                    jobPaused = true;

                    pauseJobButton.innerHTML =
                        '<i class="bi bi-play-fill"></i> Resume Job';

                    activeJobStatus.textContent =
                        "Paused";

                    jobTimerMessage.textContent =
                        "Labour timer paused. Paused time is not counted.";

                    jobTimerMessage.className =
                        "sd-job-message";

                }
                else {

                    jobPaused = false;

                    pauseJobButton.innerHTML =
                        '<i class="bi bi-pause-fill"></i> Pause Job';

                    activeJobStatus.textContent =
                        "In Progress";

                    jobTimerMessage.textContent =
                        "Labour timer resumed.";

                    jobTimerMessage.className =
                        "sd-job-message success";

timerInterval = setInterval(
                        () => {

                            elapsedSeconds += 1;
                            function loadMechanicJob(jobId) {
            const job = mechanicJobs[jobId];
            if (!job) return;

            currentMechanicJob = job;

            window.dispatchEvent(new CustomEvent("mechanicJobLoaded", {
                detail: job
            }));

            clearInterval(timerInterval);
            timerInterval = null;
            jobStartedAt = null;
            elapsedSeconds = 0;
            jobRunning = false;
            jobPaused = false;
            jobCompleted = false;

            const activeSection = document.getElementById("active-job");

            if (activeSection) {
                const panels = activeSection.querySelectorAll(".sd-panel");
                const infoPanel = panels[0];

                if (infoPanel) {
                    const title = infoPanel.querySelector(".sd-panel-header h3");
                    const priority = infoPanel.querySelector(".sd-priority");
                    const vehicle = infoPanel.querySelector(".sd-active-vehicle strong");
                    const plate = infoPanel.querySelector(".sd-active-vehicle span");
                    const infoValues = infoPanel.querySelectorAll(".sd-job-info-grid strong");
                    const concern = infoPanel.querySelector(".sd-concern-box p");

                    if (title) title.textContent = `#${job.id}`;

                    if (priority) {
                        priority.textContent = `${job.priority} Priority`;
                        priority.className = `sd-priority ${job.priority.toLowerCase()}`;
                    }

                    if (vehicle) vehicle.textContent = job.vehicle;
                    if (plate) plate.textContent = job.plate;

                    if (infoValues[0]) infoValues[0].textContent = job.service;
                    if (infoValues[1]) infoValues[1].textContent = job.bay;
                    if (infoValues[2]) infoValues[2].textContent = "Workshop Manager";
                    if (infoValues[3]) infoValues[3].textContent = job.estimatedFinish;

                    if (concern) concern.textContent = job.concern;
                }
            }

            jobStartTime.textContent = "--:--";
            jobEndTime.textContent = "--:--";

            activeJobStatus.textContent = "Ready to Start";

            startJobButton.disabled = false;
            pauseJobButton.disabled = true;
            endJobButton.disabled = true;

            startJobButton.innerHTML = '<i class="bi bi-play-fill"></i> Start Job';
            pauseJobButton.innerHTML = '<i class="bi bi-pause-fill"></i> Pause Job';

            mechanicWorkNotes.value = "";
            workNotesMessage.textContent = "";
            jobTimerMessage.textContent = "";

            completionRecord.hidden = true;

            workChecks.forEach(item => {
                item.checked = false;
            });

            updateTimerDisplay();
            updateWorkProgress();

            const completionJob = completionRecord.querySelector(".sd-completion-grid strong");
            if (completionJob) {
                completionJob.textContent = `#${job.id}`;
            }

            openSection("active-job");
        }

        document.querySelectorAll(".sd-job-number").forEach(jobNumber => {
            const jobId = jobNumber.textContent.replace("#", "").trim();

            if (!mechanicJobs[jobId] || jobId === "JC-1052") return;

            const card = jobNumber.closest(".sd-job-card");
            if (!card) return;

            const button = card.querySelector("button");
            if (!button) return;

            button.textContent = "Start Job";

            button.addEventListener("click", () => {
                loadMechanicJob(jobId);
            });
        });
        updateTimerDisplay();

                        },
                        1000
                    );

                }

            }
        );

endJobButton.addEventListener(
            "click",
            () => {

                if (!jobRunning || jobCompleted) {
                    return;
                }

const completedChecks =
                    [...workChecks].filter(
                        item => item.checked
                    ).length;

if (completedChecks !== workChecks.length) {

                    jobTimerMessage.textContent =
                        "Complete all work checklist items before ending the job.";

                    jobTimerMessage.className =
                        "sd-job-message error";

                    return;

                }

clearInterval(timerInterval);

                timerInterval = null;
                jobRunning = false;
                jobCompleted = true;

                const endedAt = new Date();

                jobEndTime.textContent =
                    formatClockTime(endedAt);

                activeJobStatus.textContent =
                    "Completed";

                activeJobStatus.className =
                    "sd-status sd-status-complete";

                endJobButton.disabled = true;
                pauseJobButton.disabled = true;
                jobPaused = false;

                jobTimerMessage.textContent =
                    "Job completed and labour duration recorded.";

                jobTimerMessage.className =
                    "sd-job-message success";

recordStartTime.textContent =
                    formatClockTime(jobStartedAt);

                recordEndTime.textContent =
                    formatClockTime(endedAt);

                recordDuration.textContent =
                    formatDuration(elapsedSeconds);

                completionRecord.hidden = false;

                window.dispatchEvent(new CustomEvent("mechanicJobCompleted", {
                    detail: {
                        id: currentMechanicJob.id,
                        vehicle: currentMechanicJob.vehicle,
                        plate: currentMechanicJob.plate,
                        service: currentMechanicJob.service,
                        completed: `Today · ${formatClockTime(endedAt)}`,
                        duration: formatDuration(elapsedSeconds),
                        bay: currentMechanicJob.bay,
                        mechanic: "Nimal Perera",
                        started: formatClockTime(jobStartedAt),
                        ended: formatClockTime(endedAt),
                        result: "Completed",
                        notes: mechanicWorkNotes.value.trim() || "Job completed successfully."
                    }
                }));

            }
        );

workChecks.forEach(
            checkbox => {

                checkbox.addEventListener(
                    "change",
                    updateWorkProgress
                );

            }
        );

saveWorkNotesButton.addEventListener(
            "click",
            () => {

                const notes =
                    mechanicWorkNotes.value.trim();

if (!notes) {

                    workNotesMessage.textContent =
                        "Enter work or repair notes before saving.";

                    workNotesMessage.className =
                        "sd-job-message error";

                    return;

                }

workNotesMessage.textContent =
                    "Work notes saved for this job.";

                workNotesMessage.className =
                    "sd-job-message success";

            }
        );

updateTimerDisplay();
        updateWorkProgress();

        // Diagnostics & Repair Notes

        const diagnosticForm = document.getElementById("diagnosticForm");

        const diagnosticArea = document.getElementById("diagnosticArea");

        const diagnosticSeverity = document.getElementById("diagnosticSeverity");

        const diagnosticFinding = document.getElementById("diagnosticFinding");

        const repairAction = document.getElementById("repairAction");

        const diagnosticResult = document.getElementById("diagnosticResult");

        const diagnosticRecommendation =
            document.getElementById(
                "diagnosticRecommendation"
            );

        const diagnosticMessage = document.getElementById("diagnosticMessage");

        const diagnosticList = document.getElementById("diagnosticList");

        const diagnosticCount = document.getElementById("diagnosticCount");

        const requestPartFromDiagnostic =
            document.getElementById(
                "requestPartFromDiagnostic"
            );

let diagnosticEntryCount = 1;

function escapeDiagnosticHTML(value) {

            return String(value)
                .replaceAll("&", "&amp;")
                .replaceAll("<", "&lt;")
                .replaceAll(">", "&gt;")
                .replaceAll('"', "&quot;")
                .replaceAll("'", "&#039;");

        }

function getDiagnosticTime() {

            return new Date().toLocaleTimeString(
                [],
                {
                    hour: "2-digit",
                    minute: "2-digit"
                }
            );

        }

function updateDiagnosticCount() {

            diagnosticCount.textContent =
                `${diagnosticEntryCount} ${
                    diagnosticEntryCount === 1
                        ? "Entry"
                        : "Entries"
                }`;

        }

diagnosticForm.addEventListener(
            "submit",
            event => {

                event.preventDefault();

const area = diagnosticArea.value;

                const severity = diagnosticSeverity.value;

                const finding =
                    diagnosticFinding.value.trim();

                const action =
                    repairAction.value.trim();

                const result = diagnosticResult.value;

                const recommendation =
                    diagnosticRecommendation.value.trim();

if (
                    !area ||
                    !severity ||
                    !finding ||
                    !action ||
                    !result
                ) {

                    diagnosticMessage.textContent =
                        "Complete all required diagnostic fields.";

                    diagnosticMessage.className =
                        "sd-job-message error";

                    return;

                }

const severityClass =
                    severity.toLowerCase();

const safeArea =
                    escapeDiagnosticHTML(area);

                const safeSeverity =
                    escapeDiagnosticHTML(severity);

                const safeFinding =
                    escapeDiagnosticHTML(finding);

                const safeAction =
                    escapeDiagnosticHTML(action);

                const safeResult =
                    escapeDiagnosticHTML(result);

                const safeRecommendation =
                    escapeDiagnosticHTML(
                        recommendation ||
                        "No additional recommendation"
                    );

const entry =
                    document.createElement("article");

                entry.className =
                    "sd-diagnostic-entry";

entry.innerHTML = `
                    <div class="sd-diagnostic-entry-head">

                        <div>
                            <strong>
                                ${safeArea} · Diagnostic Entry
                            </strong>

                            <span>
                                Nimal Perera · Today
                                ${getDiagnosticTime()}
                            </span>
                        </div>

                        <span
                            class="sd-diagnostic-severity ${severityClass}"
                        >
                            ${safeSeverity}
                        </span>

                    </div>

                    <div class="sd-diagnostic-entry-grid">

                        <div>
                            <span>Finding</span>
                            <p>${safeFinding}</p>
                        </div>

                        <div>
                            <span>Action Taken</span>
                            <p>${safeAction}</p>
                        </div>

                    </div>

                    <div class="sd-diagnostic-entry-footer">

                        <span>
                            Result:
                            <strong>
                                ${safeResult}
                            </strong>
                        </span>

                        <span>
                            Recommendation:
                            <strong>
                                ${safeRecommendation}
                            </strong>
                        </span>

                    </div>
                `;

diagnosticList.prepend(entry);

                diagnosticEntryCount += 1;

                updateDiagnosticCount();

diagnosticMessage.textContent =
                    "Diagnostic entry saved to Job Card #JC-1052.";

                diagnosticMessage.className =
                    "sd-job-message success";

diagnosticForm.reset();

            }
        );

requestPartFromDiagnostic.addEventListener(
            "click",
            () => {

                openSection("parts");

            }
        );

updateDiagnosticCount();
    }
);

document.addEventListener("DOMContentLoaded", () => {

    // Mechanic Parts Catalog - frontend demo data; backend API will replace this later.

    const parts = [

        {
            id: "PT-ENG-001",
            name: "Engine Mount",
            category: "Engine",
            partNumber: "EM-XTR-2018",
            compatibility: "Nissan X-Trail 2017–2021",
            image: "../assets/images/parts/engine-mount.jpg"
        },

        {
            id: "PT-BRK-002",
            name: "Front Brake Pad Set",
            category: "Brakes",
            partNumber: "BP-FR-8842",
            compatibility: "Toyota / Nissan / Honda applications",
            image: "../assets/images/parts/brake-pads.jpg"
        },

        {
            id: "PT-SUS-003",
            name: "Shock Absorber",
            category: "Suspension",
            partNumber: "SA-FR-2401",
            compatibility: "Front suspension assembly",
            image: "../assets/images/parts/shock-absorber.jpg"
        },

        {
            id: "PT-ELC-004",
            name: "12V Car Battery",
            category: "Electrical",
            partNumber: "BAT-12V-NS60",
            compatibility: "NS60 battery applications",
            image: "../assets/images/parts/car-battery.jpg"
        },

        {
            id: "PT-FLT-005",
            name: "Engine Oil Filter",
            category: "Filters",
            partNumber: "OF-90915",
            compatibility: "Petrol engine service applications",
            image: "../assets/images/parts/oil-filter.jpg"
        },

        {
            id: "PT-CLG-006",
            name: "Radiator Hose",
            category: "Cooling",
            partNumber: "RH-XTR-2240",
            compatibility: "Nissan X-Trail cooling system",
            image: "../assets/images/parts/radiator-hose.jpg"
        },

        {
            id: "PT-BDY-007",
            name: "Headlamp Assembly",
            category: "Body",
            partNumber: "HL-RH-8891",
            compatibility: "Right-side replacement assembly",
            image: "../assets/images/parts/headlamp.jpg"
        },

        {
            id: "PT-WHL-008",
            name: "17-inch Alloy Wheel",
            category: "Wheels",
            partNumber: "AW-17-5H114",
            compatibility: "17 inch · 5x114.3 fitment",
            image: "../assets/images/parts/alloy-wheel.jpg"
        }

    ];

const partsCatalog = document.getElementById("partsCatalog");

    const partsCatalogCount = document.getElementById("partsCatalogCount");

    const filterButtons = document.querySelectorAll(".sd-part-filter");

    const selectedPartId = document.getElementById("selectedPartId");

    const selectedPartEmpty = document.getElementById("selectedPartEmpty");

    const selectedPartPreview = document.getElementById("selectedPartPreview");

    const selectedPartImage = document.getElementById("selectedPartImage");

    const selectedPartFallback = document.getElementById("selectedPartFallback");

    const selectedPartCategory = document.getElementById("selectedPartCategory");

    const selectedPartName = document.getElementById("selectedPartName");

    const selectedPartNumber = document.getElementById("selectedPartNumber");

    const partRequisitionForm = document.getElementById("partRequisitionForm");

    const partQuantity = document.getElementById("partQuantity");

    const partUrgency = document.getElementById("partUrgency");

    const partReason = document.getElementById("partReason");

    const partRequestMessage = document.getElementById("partRequestMessage");

    const partsRequestList = document.getElementById("partsRequestList");

    const partsRequestCount = document.getElementById("partsRequestCount");

if (!partsCatalog) {
        return;
    }

let activeCategory = "All";
    let currentSelectedPart = null;
    let requestCount = 0;

function escapePartHTML(value) {

        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");

    }

function createPartCard(part) {

        const card =
            document.createElement("article");

        card.className = "sd-part-card";

        card.innerHTML = `
            <div class="sd-part-image-wrap">

                <span class="sd-part-category-badge">
                    ${escapePartHTML(part.category)}
                </span>

                <img
                    src="${escapePartHTML(part.image)}"
                    alt="${escapePartHTML(part.name)}"
                    loading="lazy"
                >

                <div class="sd-part-image-fallback">
                    <i class="bi bi-gear-wide-connected"></i>
                </div>

            </div>

            <div class="sd-part-card-body">

                <h4>
                    ${escapePartHTML(part.name)}
                </h4>

                <div class="sd-part-number">
                    ${escapePartHTML(part.partNumber)}
                </div>

                <p class="sd-part-compatibility">
                    ${escapePartHTML(part.compatibility)}
                </p>

                <button
                    type="button"
                    class="sd-request-part-btn"
                    data-part-id="${escapePartHTML(part.id)}"
                >
                    <i class="bi bi-plus-circle"></i>
                    Request Part
                </button>

            </div>
        `;

const image =
            card.querySelector("img");

        const fallback =
            card.querySelector(".sd-part-image-fallback");

fallback.hidden = true;

image.addEventListener("load", () => {
            fallback.hidden = true;
            image.hidden = false;
        });

image.addEventListener("error", () => {
            image.hidden = true;
            fallback.hidden = false;
        });

card
            .querySelector(".sd-request-part-btn")
            .addEventListener("click", () => {

                selectPart(part.id);

            });

return card;

    }

function renderParts() {

        const filteredParts =
            activeCategory === "All"
                ? parts
                : parts.filter(
                    part =>
                        part.category === activeCategory
                );

partsCatalog.innerHTML = "";

filteredParts.forEach(part => {

            partsCatalog.appendChild(
                createPartCard(part)
            );

        });

partsCatalogCount.textContent =
            `${filteredParts.length} ${
                filteredParts.length === 1
                    ? "Part"
                    : "Parts"
            }`;

    }

function selectPart(partId) {

        const part =
            parts.find(
                item => item.id === partId
            );

if (!part) {
            return;
        }

currentSelectedPart = part;

        selectedPartId.value = part.id;

        selectedPartEmpty.hidden = true;
        selectedPartPreview.hidden = false;

        selectedPartCategory.textContent =
            part.category;

        selectedPartName.textContent =
            part.name;

        selectedPartNumber.textContent =
            part.partNumber;

selectedPartImage.hidden = false;
        selectedPartFallback.hidden = true;

        selectedPartImage.src = part.image;
        selectedPartImage.alt = part.name;

selectedPartImage.onerror = () => {

            selectedPartImage.hidden = true;
            selectedPartFallback.hidden = false;

        };

selectedPartImage.onload = () => {

            selectedPartImage.hidden = false;
            selectedPartFallback.hidden = true;

        };

partRequestMessage.textContent =
            `${part.name} selected. Complete the requisition details.`;

        partRequestMessage.className =
            "sd-job-message success";

partRequisitionForm.scrollIntoView({
            behavior: "smooth",
            block: "center"
        });

    }

function updateRequestCount() {

        partsRequestCount.textContent =
            `${requestCount} ${
                requestCount === 1
                    ? "Request"
                    : "Requests"
            }`;

    }

function formatRequestTime() {

        return new Date().toLocaleTimeString(
            [],
            {
                hour: "2-digit",
                minute: "2-digit"
            }
        );

    }

filterButtons.forEach(button => {

        button.addEventListener("click", () => {

            activeCategory =
                button.dataset.category;

filterButtons.forEach(item => {
                item.classList.remove("active");
            });

button.classList.add("active");

            renderParts();

        });

    });

partRequisitionForm.addEventListener(
        "submit",
        event => {

            event.preventDefault();

if (!currentSelectedPart) {

                partRequestMessage.textContent =
                    "Select a part from the catalog first.";

                partRequestMessage.className =
                    "sd-job-message error";

                return;

            }

const quantity =
                Number(partQuantity.value);

            const urgency = partUrgency.value;

            const reason =
                partReason.value.trim();

if (
                !Number.isInteger(quantity) ||
                quantity < 1
            ) {

                partRequestMessage.textContent =
                    "Enter a valid quantity.";

                partRequestMessage.className =
                    "sd-job-message error";

                return;

            }

if (!reason) {

                partRequestMessage.textContent =
                    "Enter the reason this part is required.";

                partRequestMessage.className =
                    "sd-job-message error";

                return;

            }

requestCount += 1;

if (requestCount === 1) {
                partsRequestList.innerHTML = "";
            }

const requestNumber =
                `PR-${3021 + requestCount}`;

const row =
                document.createElement("article");

            row.className =
                "sd-part-request-row";

row.innerHTML = `
                <div class="sd-request-part-info">

                    <div class="sd-request-thumb">

                        <img
                            src="${escapePartHTML(currentSelectedPart.image)}"
                            alt="${escapePartHTML(currentSelectedPart.name)}"
                        >

                    </div>

                    <div>

                        <strong>
                            ${escapePartHTML(currentSelectedPart.name)}
                        </strong>

                        <span>
                            ${escapePartHTML(requestNumber)}
                            ·
                            ${escapePartHTML(currentSelectedPart.partNumber)}
                        </span>

                    </div>

                </div>

<div class="sd-request-data">

                    <span>Quantity</span>

                    <strong>
                        ${quantity}
                    </strong>

                </div>

<div class="sd-request-data">

                    <span>Urgency</span>

                    <strong>
                        ${escapePartHTML(urgency)}
                    </strong>

                </div>

<span class="sd-request-status">
                    Pending Storekeeper
                </span>
            `;

const historyImage =
                row.querySelector("img");

historyImage.addEventListener(
                "error",
                () => {

                    historyImage.style.display =
                        "none";

                }
            );

partsRequestList.prepend(row);

            updateRequestCount();

partRequestMessage.textContent =
                `${requestNumber} submitted to Storekeeper successfully.`;

            partRequestMessage.className =
                "sd-job-message success";

partQuantity.value = "1";
            partUrgency.value = "Normal";
            partReason.value = "";

        }
    );

renderParts();
    updateRequestCount();

});

document.addEventListener("DOMContentLoaded", () => {

    // Completed Jobs - frontend demo data; backend API will replace this later.

    const completedJobs = [

        {
            id: "JC-1051",
            vehicle: "Toyota Corolla",
            plate: "WP CAB-1234",
            service: "Brake Repair",
            completed: "Today · 08:45 AM",
            duration: "02h 15m",
            bay: "Bay 01",
            mechanic: "Nimal Perera",
            started: "06:30 AM",
            ended: "08:45 AM",
            result: "Completed",
            notes:
                "Front brake pads replaced. Brake discs inspected and braking system tested successfully."
        },

        {
            id: "JC-1049",
            vehicle: "Honda Vezel",
            plate: "WP CBF-5678",
            service: "Diagnostics",
            completed: "Today · 11:20 AM",
            duration: "01h 30m",
            bay: "Bay 02",
            mechanic: "Nimal Perera",
            started: "09:50 AM",
            ended: "11:20 AM",
            result: "Completed",
            notes:
                "Diagnostic scan completed. Battery charging system and engine sensors tested. Fault code cleared after inspection."
        },

        {
            id: "JC-1046",
            vehicle: "Suzuki Swift",
            plate: "WP CAG-9021",
            service: "General Service",
            completed: "31 Aug · 04:10 PM",
            duration: "01h 45m",
            bay: "Bay 04",
            mechanic: "Nimal Perera",
            started: "02:25 PM",
            ended: "04:10 PM",
            result: "Completed",
            notes:
                "Engine oil and oil filter replaced. Fluid levels, tyres, brakes and general vehicle condition checked."
        },

        {
            id: "JC-1042",
            vehicle: "Nissan X-Trail",
            plate: "WP CAQ-7741",
            service: "Engine Repair",
            completed: "30 Aug · 03:35 PM",
            duration: "03h 20m",
            bay: "Bay 03",
            mechanic: "Nimal Perera",
            started: "12:15 PM",
            ended: "03:35 PM",
            result: "Completed",
            notes:
                "Engine mounting assembly replaced and vibration re-tested. Final road test completed successfully."
        }

    ];

const completedJobsList = document.getElementById("completedJobsList");

    const completedJobSearch = document.getElementById("completedJobSearch");

    const completedJobFilter = document.getElementById("completedJobFilter");

    const completedJobsEmpty = document.getElementById("completedJobsEmpty");

    const completedJobModal = document.getElementById("completedJobModal");

    const completedJobModalContent =
        document.getElementById(
            "completedJobModalContent"
        );

    const completedJobModalTitle =
        document.getElementById(
            "completedJobModalTitle"
        );

    const closeCompletedJobModal =
        document.getElementById(
            "closeCompletedJobModal"
        );

    const completedJobModalBackdrop =
        document.getElementById(
            "completedJobModalBackdrop"
        );

if (!completedJobsList) {
        return;
    }

function escapeCompletedHTML(value) {

        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");

    }

function openCompletedJob(jobId) {

        const job =
            completedJobs.find(
                item => item.id === jobId
            );

if (!job) {
            return;
        }

completedJobModalTitle.textContent =
            `Job ${job.id}`;

completedJobModalContent.innerHTML = `
            <div class="sd-summary-vehicle">

                <i class="bi bi-car-front"></i>

                <div>
                    <strong>
                        ${escapeCompletedHTML(job.vehicle)}
                    </strong>

                    <span>
                        ${escapeCompletedHTML(job.plate)}
                        ·
                        ${escapeCompletedHTML(job.service)}
                    </span>
                </div>

            </div>

<div class="sd-summary-grid">

                <div>
                    <span>Mechanic</span>
                    <strong>
                        ${escapeCompletedHTML(job.mechanic)}
                    </strong>
                </div>

                <div>
                    <span>Workshop Bay</span>
                    <strong>
                        ${escapeCompletedHTML(job.bay)}
                    </strong>
                </div>

                <div>
                    <span>Start Time</span>
                    <strong>
                        ${escapeCompletedHTML(job.started)}
                    </strong>
                </div>

                <div>
                    <span>End Time</span>
                    <strong>
                        ${escapeCompletedHTML(job.ended)}
                    </strong>
                </div>

                <div>
                    <span>Actual Labour</span>
                    <strong>
                        ${escapeCompletedHTML(job.duration)}
                    </strong>
                </div>

                <div>
                    <span>Status</span>
                    <strong>
                        ${escapeCompletedHTML(job.result)}
                    </strong>
                </div>

            </div>

<div class="sd-summary-notes">

                <span>
                    Repair / Completion Notes
                </span>

                <p>
                    ${escapeCompletedHTML(job.notes)}
                </p>

            </div>
        `;

completedJobModal.hidden = false;

        document.body.style.overflow = "hidden";

    }

function closeCompletedModal() {

        completedJobModal.hidden = true;

        document.body.style.overflow = "";

    }

function renderCompletedJobs() {

        const searchTerm =
            completedJobSearch.value
                .trim()
                .toLowerCase();

        const serviceFilter = completedJobFilter.value;

const filteredJobs =
            completedJobs.filter(job => {

                const matchesSearch =
                    job.id.toLowerCase().includes(searchTerm) ||
                    job.vehicle.toLowerCase().includes(searchTerm) ||
                    job.plate.toLowerCase().includes(searchTerm);

                const matchesService =
                    serviceFilter === "All" ||
                    job.service === serviceFilter;

return (
                    matchesSearch &&
                    matchesService
                );

            });

completedJobsList.innerHTML = "";

filteredJobs.forEach(job => {

            const row =
                document.createElement("article");

            row.className =
                "sd-completed-job";

row.innerHTML = `
                <div class="sd-completed-vehicle">

                    <div class="sd-completed-vehicle-icon">
                        <i class="bi bi-car-front"></i>
                    </div>

                    <div>

                        <strong>
                            ${escapeCompletedHTML(job.vehicle)}
                        </strong>

                        <span>
                            ${escapeCompletedHTML(job.id)}
                            ·
                            ${escapeCompletedHTML(job.plate)}
                        </span>

                    </div>

                </div>

<div class="sd-completed-data">

                    <span>Service</span>

                    <strong>
                        ${escapeCompletedHTML(job.service)}
                    </strong>

                </div>

<div class="sd-completed-data">

                    <span>Completed</span>

                    <strong>
                        ${escapeCompletedHTML(job.completed)}
                    </strong>

                </div>

<div class="sd-completed-data">

                    <span>Labour Time</span>

                    <strong>
                        ${escapeCompletedHTML(job.duration)}
                    </strong>

                </div>

<button
                    type="button"
                    class="sd-view-job-btn"
                    data-completed-job="${escapeCompletedHTML(job.id)}"
                >
                    View Summary
                </button>
            `;

row
                .querySelector("[data-completed-job]")
                .addEventListener(
                    "click",
                    () => {

                        openCompletedJob(job.id);

                    }
                );

completedJobsList.appendChild(row);

        });

completedJobsEmpty.hidden =
            filteredJobs.length !== 0;

    }

completedJobSearch.addEventListener(
        "input",
        renderCompletedJobs
    );

completedJobFilter.addEventListener(
        "change",
        renderCompletedJobs
    );

closeCompletedJobModal.addEventListener(
        "click",
        closeCompletedModal
    );

completedJobModalBackdrop.addEventListener(
        "click",
        closeCompletedModal
    );

document.addEventListener(
        "keydown",
        event => {

            if (
                event.key === "Escape" &&
                !completedJobModal.hidden
            ) {

                closeCompletedModal();

            }

        }
    );

    window.addEventListener("mechanicJobCompleted", event => {
        const completedJob = event.detail;

        if (!completedJob || completedJobs.some(job => job.id === completedJob.id)) {
            return;
        }

        completedJobs.unshift(completedJob);
        renderCompletedJobs();
    });

renderCompletedJobs();

});



document.addEventListener("DOMContentLoaded", () => {
    const greeting = document.getElementById("mechanicGreeting");

    if (!greeting) return;

    const hour = new Date().getHours();
    let message = "Good evening";

    if (hour < 12) {
        message = "Good morning";
    } else if (hour < 17) {
        message = "Good afternoon";
    }

    greeting.textContent = `${message}, Nimal.`;
});
document.addEventListener("DOMContentLoaded", () => {
    window.addEventListener("mechanicJobCompleted", event => {
        const completedJob = event.detail;
        if (!completedJob || completedJob.id !== "JC-1052") return;

        const jobNumbers = document.querySelectorAll(".sd-job-number");

        jobNumbers.forEach(jobNumber => {
            if (!jobNumber.textContent.includes("JC-1052")) return;

            const card = jobNumber.closest(".sd-job-card");
            if (!card) return;

            const status = card.querySelector(".sd-status");

            if (status) {
                status.textContent = "Completed";
                status.className = "sd-status sd-status-complete";
            }

            const continueButton = card.querySelector("[data-go='active-job']");

            if (continueButton) {
                continueButton.disabled = true;
                continueButton.innerHTML = '<i class="bi bi-check-circle"></i> Completed';
            }
        });
    });
});


document.addEventListener("DOMContentLoaded", () => {
    // genericAssignedJobCompletion
    window.addEventListener("mechanicJobCompleted", event => {
        const completedJob = event.detail;
        if (!completedJob) return;

        document.querySelectorAll(".sd-job-number").forEach(jobNumber => {
            if (!jobNumber.textContent.includes(completedJob.id)) return;

            const card = jobNumber.closest(".sd-job-card");
            if (!card) return;

            const status = card.querySelector(".sd-status");

            if (status) {
                status.textContent = "Completed";
                status.className = "sd-status sd-status-complete";
            }

            const button = card.querySelector("button");

            if (button) {
                button.disabled = true;
                button.innerHTML = '<i class="bi bi-check-circle"></i> Completed';
            }
        });
    });
});
document.addEventListener("DOMContentLoaded", () => {
    const workChecks = document.querySelectorAll(".sd-work-check");
    const startJobButton = document.getElementById("startJobButton");
    const endJobButton = document.getElementById("endJobButton");

    let mechanicJobStarted = false;

    workChecks.forEach(check => {
        check.disabled = true;
    });

    startJobButton.addEventListener("click", () => {
        mechanicJobStarted = true;

        workChecks.forEach(check => {
            check.disabled = false;
        });
    });

    endJobButton.addEventListener("click", () => {
        if (!mechanicJobStarted) return;

        const allCompleted = [...workChecks].every(check => check.checked);

        if (allCompleted) {
            workChecks.forEach(check => {
                check.disabled = true;
            });

            mechanicJobStarted = false;
        }
    });

    window.addEventListener("mechanicJobCompleted", () => {
        workChecks.forEach(check => {
            check.disabled = true;
        });

        mechanicJobStarted = false;
    });
});
document.addEventListener("DOMContentLoaded", () => {
    const workChecks = document.querySelectorAll(".sd-work-check");

    const overviewSection = document.getElementById("overview");
    if (!overviewSection || !workChecks.length) return;

    const overviewProgressText = overviewSection.querySelector(".sd-job-progress-head strong");
    const overviewProgressBar = overviewSection.querySelector(".sd-progress span");

    function syncOverviewProgress() {
        const completed = [...workChecks].filter(check => check.checked).length;
        const total = workChecks.length;
        const percentage = total ? Math.round((completed / total) * 100) : 0;

        if (overviewProgressText) {
            overviewProgressText.textContent = `${percentage}%`;
        }

        if (overviewProgressBar) {
            overviewProgressBar.style.width = `${percentage}%`;
        }
    }

    workChecks.forEach(check => {
        check.addEventListener("change", syncOverviewProgress);
    });

    syncOverviewProgress();
});
document.addEventListener("DOMContentLoaded", () => {
    const overviewSection = document.getElementById("overview");
    if (!overviewSection) return;

    const statCards = overviewSection.querySelectorAll(".sd-stat-card");
    if (statCards.length < 4) return;

    const assignedValue = statCards[0].querySelector("strong");
    const activeValue = statCards[1].querySelector("strong");
    const completedValue = statCards[2].querySelector("strong");
    const bayValue = statCards[3].querySelector("strong");

    let completedToday = Number(completedValue?.textContent.trim()) || 0;

    function setCurrentBay(job) {
        if (!bayValue || !job) return;

        const bayNumber = String(job.bay || "")
            .replace(/Bay/i, "")
            .trim();

        bayValue.textContent = bayNumber || "--";
    }

    function syncOverviewForJob(job) {
        if (assignedValue) assignedValue.textContent = "04";
        if (activeValue) activeValue.textContent = "01";

        setCurrentBay(job);
    }

    window.addEventListener("mechanicJobCompleted", event => {
        const completedJob = event.detail;
        if (!completedJob) return;

        completedToday += 1;

        if (completedValue) {
            completedValue.textContent = String(completedToday).padStart(2, "0");
        }

        if (activeValue) {
            activeValue.textContent = "00";
        }

        if (bayValue) {
            bayValue.textContent = "--";
        }
    });

    document.addEventListener("click", event => {
        const button = event.target.closest(".sd-job-card button");
        if (!button) return;

        const card = button.closest(".sd-job-card");
        const jobNumber = card?.querySelector(".sd-job-number");

        if (!jobNumber) return;

        const jobId = jobNumber.textContent.replace("#", "").trim();

        if (typeof mechanicJobs !== "undefined" && mechanicJobs[jobId]) {
            setTimeout(() => {
                syncOverviewForJob(mechanicJobs[jobId]);
            }, 0);
        }
    });
});
