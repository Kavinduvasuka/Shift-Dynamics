document.addEventListener(
    "DOMContentLoaded",
    () => {

        const sidebar =
            document.getElementById("sidebar");

        const menuButton =
            document.getElementById("menuButton");

        const pageTitle =
            document.getElementById("pageTitle");

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


        /* =============================================
           ACTIVE JOB TIMER
           ============================================= */

        const jobTimer =
            document.getElementById("jobTimer");

        const startJobButton =
            document.getElementById("startJobButton");

        const endJobButton =
            document.getElementById("endJobButton");

        const jobStartTime =
            document.getElementById("jobStartTime");

        const jobEndTime =
            document.getElementById("jobEndTime");

        const activeJobStatus =
            document.getElementById("activeJobStatus");

        const jobTimerMessage =
            document.getElementById("jobTimerMessage");

        const workChecks =
            document.querySelectorAll(".sd-work-check");

        const workProgressText =
            document.getElementById("workProgressText");

        const workProgressBar =
            document.getElementById("workProgressBar");

        const mechanicWorkNotes =
            document.getElementById("mechanicWorkNotes");

        const saveWorkNotesButton =
            document.getElementById("saveWorkNotesButton");

        const workNotesMessage =
            document.getElementById("workNotesMessage");

        const completionRecord =
            document.getElementById("completionRecord");

        const recordStartTime =
            document.getElementById("recordStartTime");

        const recordEndTime =
            document.getElementById("recordEndTime");

        const recordDuration =
            document.getElementById("recordDuration");


        let timerInterval = null;
        let jobStartedAt = null;
        let elapsedSeconds = 0;
        let jobRunning = false;
        let jobCompleted = false;


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

            const total =
                workChecks.length;

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
                        updateTimerDisplay();

                    },
                    1000
                );

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

        /* =============================================
           DIAGNOSTICS & REPAIR NOTES
           ============================================= */

        const diagnosticForm =
            document.getElementById("diagnosticForm");

        const diagnosticArea =
            document.getElementById("diagnosticArea");

        const diagnosticSeverity =
            document.getElementById("diagnosticSeverity");

        const diagnosticFinding =
            document.getElementById("diagnosticFinding");

        const repairAction =
            document.getElementById("repairAction");

        const diagnosticResult =
            document.getElementById("diagnosticResult");

        const diagnosticRecommendation =
            document.getElementById(
                "diagnosticRecommendation"
            );

        const diagnosticMessage =
            document.getElementById("diagnosticMessage");

        const diagnosticList =
            document.getElementById("diagnosticList");

        const diagnosticCount =
            document.getElementById("diagnosticCount");

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


                const area =
                    diagnosticArea.value;

                const severity =
                    diagnosticSeverity.value;

                const finding =
                    diagnosticFinding.value.trim();

                const action =
                    repairAction.value.trim();

                const result =
                    diagnosticResult.value;

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

    /* =====================================================
       MECHANIC PARTS CATALOG
       Frontend demo data.
       Later this should come from the C# / database API.
       ===================================================== */

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


    const partsCatalog =
        document.getElementById("partsCatalog");

    const partsCatalogCount =
        document.getElementById("partsCatalogCount");

    const filterButtons =
        document.querySelectorAll(".sd-part-filter");

    const selectedPartId =
        document.getElementById("selectedPartId");

    const selectedPartEmpty =
        document.getElementById("selectedPartEmpty");

    const selectedPartPreview =
        document.getElementById("selectedPartPreview");

    const selectedPartImage =
        document.getElementById("selectedPartImage");

    const selectedPartFallback =
        document.getElementById("selectedPartFallback");

    const selectedPartCategory =
        document.getElementById("selectedPartCategory");

    const selectedPartName =
        document.getElementById("selectedPartName");

    const selectedPartNumber =
        document.getElementById("selectedPartNumber");

    const partRequisitionForm =
        document.getElementById("partRequisitionForm");

    const partQuantity =
        document.getElementById("partQuantity");

    const partUrgency =
        document.getElementById("partUrgency");

    const partReason =
        document.getElementById("partReason");

    const partRequestMessage =
        document.getElementById("partRequestMessage");

    const partsRequestList =
        document.getElementById("partsRequestList");

    const partsRequestCount =
        document.getElementById("partsRequestCount");


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

            const urgency =
                partUrgency.value;

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
