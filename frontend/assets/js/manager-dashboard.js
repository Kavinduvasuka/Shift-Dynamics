    document.addEventListener(
    "DOMContentLoaded",
    () => {

        const sidebar =
            document.getElementById(
                "managerSidebar"
            );

        const sidebarOpen =
            document.getElementById(
                "sidebarOpen"
            );

        const sidebarClose =
            document.getElementById(
                "sidebarClose"
            );

        const sidebarOverlay =
            document.getElementById(
                "sidebarOverlay"
            );

        const pageTitle =
            document.getElementById(
                "pageTitle"
            );

        const navItems =
            document.querySelectorAll(
                ".sd-nav-item"
            );

        const sections =
            document.querySelectorAll(
                ".sd-content-section"
            );

        const quickLinks =
            document.querySelectorAll(
                "[data-go]"
            );

        const sectionTitles = {
            overview:
                "Operations Overview",

            "job-cards":
                "Job Card Review",

            "workshop-load":
                "Workshop Load",

            vendors:
                "Vendor Bid Comparison",

            orders:
                "Order Approvals",

            billing:
                "Billing Review",

            analytics:
                "Business Analytics",

            "vendor-registrations":
                "Vendor Registration Requests",

            users:
                "User Management"
        };

        function closeSidebar() {

            sidebar?.classList.remove(
                "open"
            );

            sidebarOverlay?.classList.remove(
                "show"
            );
        }

        function openSection(sectionId) {

            if (!sectionId) {
                return;
            }

            const target =
                document.getElementById(
                    sectionId
                );

            if (!target) {
                return;
            }

            sections.forEach(section => {

                section.classList.remove(
                    "active"
                );
            });

            target.classList.add(
                "active"
            );

            navItems.forEach(item => {

                item.classList.toggle(
                    "active",
                    item.dataset.section ===
                        sectionId
                );
            });

            if (pageTitle) {

                pageTitle.textContent =
                    sectionTitles[sectionId] ||
                    "Manager Dashboard";
            }

            closeSidebar();

            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });
        }

        navItems.forEach(item => {

            item.addEventListener(
                "click",
                () => {

                    openSection(
                        item.dataset.section
                    );
                }
            );
        });

        quickLinks.forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    openSection(
                        button.dataset.go
                    );
                }
            );
        });

        sidebarOpen?.addEventListener(
            "click",
            () => {

                sidebar?.classList.add(
                    "open"
                );

                sidebarOverlay?.classList.add(
                    "show"
                );
            }
        );

        sidebarClose?.addEventListener(
            "click",
            closeSidebar
        );

        sidebarOverlay?.addEventListener(
            "click",
            closeSidebar
        );

        document.addEventListener(
            "keydown",
            event => {

                if (event.key === "Escape") {
                    closeSidebar();
                }
            }
        );

        /* =================================================
           JOB CARD REVIEW + WORKSHOP ASSIGNMENT
           ================================================= */

        const managerJobTable =
            document.getElementById(
                "managerJobTable"
            );

        const assignmentForm =
            document.getElementById(
                "assignmentForm"
            );

        const mechanicSelect =
            document.getElementById(
                "mechanicSelect"
            );

        const baySelect =
            document.getElementById(
                "baySelect"
            );

        const managerNote =
            document.getElementById(
                "managerNote"
            );

        const assignJobButton =
            document.getElementById(
                "assignJobButton"
            );

        const assignmentMessage =
            document.getElementById(
                "assignmentMessage"
            );

        const selectedJobNumber =
            document.getElementById(
                "selectedJobNumber"
            );

        const selectedJobVehicle =
            document.getElementById(
                "selectedJobVehicle"
            );

        const assignmentTable =
            document.getElementById(
                "assignmentTable"
            );

        let selectedJob = null;

        const occupiedBays =
            new Set();

        function escapeHTML(value = "") {

            return String(value)
                .replaceAll("&", "&amp;")
                .replaceAll("<", "&lt;")
                .replaceAll(">", "&gt;")
                .replaceAll('"', "&quot;")
                .replaceAll("'", "&#039;");
        }

        /* =================================================
           SHARED STORE -> MANAGER JOB CARDS

           Incoming Advisor-created jobs are rendered into the
           existing Manager Job Card Review table.

           Existing Manager review/assignment logic is reused.
           ================================================= */

        function renderSharedManagerJobs() {

            if (
                !managerJobTable ||
                !window.ShiftDynamicsStore
            ) {
                return;
            }

            const sharedJobs =
                ShiftDynamicsStore.getJobs();

            managerJobTable.innerHTML = "";
            occupiedBays.clear();

            if (assignmentTable) {
                assignmentTable.innerHTML = "";
            }


            sharedJobs
                .slice()
                .reverse()
                .forEach(job => {

                    if (!job?.jobCardNumber) {
                        return;
                    }

                    const vehicleName =
                        [
                            job.vehicle?.make,
                            job.vehicle?.model
                        ]
                            .filter(Boolean)
                            .join(" ") ||
                        "Vehicle";

                    const plate =
                        job.vehicle?.plate ||
                        "--";

                    const service =
                        job.serviceConcern ||
                        "Service Job";

                    const priority =
                        job.diagnostic?.priority ||
                        "Normal";

                    const assigned =
                        Boolean(
                            job.assignment?.mechanicName
                        );

                    const completed =
                        String(
                            job.status ||
                            job.handover?.status ||
                            ""
                        ).toLowerCase() === "completed";

                    const displayStatus =
                        completed
                            ? "Completed"
                            : assigned
                            ? "Assigned"
                            : "Awaiting Assignment";

                    const priorityClass =
                        priority === "High"
                            ? "sd-priority-high"
                            : priority === "Medium"
                                ? "sd-priority-medium"
                                : "sd-priority-normal";

                    const statusClass =
                        completed
                            ? "sd-status sd-status-ready job-status"
                            : assigned
                            ? "sd-status sd-status-progress job-status"
                            : "sd-status sd-status-warning job-status";


                    const row =
                        document.createElement(
                            "tr"
                        );

                    row.dataset.sharedJob =
                        "true";

                    row.dataset.jobCard =
                        job.jobCardNumber;

                    row.dataset.vehicle =
                        vehicleName;

                    row.dataset.plate =
                        plate;

                    row.dataset.service =
                        service;

                    row.dataset.priority =
                        priority;


                    row.innerHTML = `
                        <td>
                            <strong>
                                ${escapeHTML(job.jobCardNumber)}
                            </strong>
                        </td>

                        <td>
                            ${escapeHTML(vehicleName)}
                            <small>
                                ${escapeHTML(plate)}
                            </small>
                        </td>

                        <td>
                            ${escapeHTML(service)}
                        </td>

                        <td>
                            <span class="${priorityClass}">
                                ${escapeHTML(priority)}
                            </span>
                        </td>

                        <td>
                            <span class="${statusClass}">
                                ${escapeHTML(displayStatus)}
                            </span>
                        </td>

                        <td>
                            <button
                                type="button"
                                class="sd-review-btn"
                                ${assigned || completed ? "disabled" : ""}
                            >
                                ${completed ? "Completed" : assigned ? "Assigned" : "Review"}
                            </button>
                        </td>
                    `;

                    managerJobTable.prepend(
                        row
                    );

                    if (
                        assigned &&
                        !completed &&
                        job.assignment?.bay
                    ) {
                        occupiedBays.add(job.assignment.bay);

                        if (assignmentTable) {
                            const assignmentRow = document.createElement("tr");
                            assignmentRow.dataset.liveAssignment = "true";
                            assignmentRow.innerHTML = `
                                <td><strong>${escapeHTML(job.jobCardNumber)}</strong></td>
                                <td>${escapeHTML(vehicleName)}</td>
                                <td>${escapeHTML(job.assignment.mechanicName)}</td>
                                <td>${escapeHTML(job.assignment.bay)}</td>
                                <td><span class="sd-status sd-status-progress">Assigned</span></td>
                            `;
                            assignmentTable.prepend(assignmentRow);
                        }
                    }
                });


            const countBadge =
                document.querySelector(
                    "#job-cards .sd-count-badge"
                );

            if (countBadge) {

                const totalRows =
                    sharedJobs.length;

                countBadge.textContent =
                    `${totalRows} Job Card${totalRows === 1 ? "" : "s"}`;
            }
        }

        function selectJob(row, button) {

            if (!row) {
                return;
            }

            selectedJob = {
                jobCard:
                    row.dataset.jobCard,

                vehicle:
                    row.dataset.vehicle,

                plate:
                    row.dataset.plate,

                service:
                    row.dataset.service,

                priority:
                    row.dataset.priority,

                row
            };

            document
                .querySelectorAll(
                    ".sd-review-btn"
                )
                .forEach(reviewButton => {

                    reviewButton.classList.remove(
                        "selected"
                    );

                    reviewButton.textContent =
                        "Review";
                });

            button.classList.add(
                "selected"
            );

            button.textContent =
                "Selected";

            selectedJobNumber.textContent =
                selectedJob.jobCard;

            selectedJobVehicle.textContent =
                `${selectedJob.vehicle} - ${selectedJob.plate}`;

            mechanicSelect.disabled = false;
            baySelect.disabled = false;
            managerNote.disabled = false;
            assignJobButton.disabled = false;

            assignmentMessage.textContent =
                "Job card selected. Choose a mechanic and available workshop bay.";

            assignmentMessage.className =
                "sd-form-message";
        }

        renderSharedManagerJobs();

        if (window.ShiftDynamicsStore) {

            ShiftDynamicsStore.subscribe(
                () => {
                    renderSharedManagerJobs();
                }
            );
        }


        managerJobTable?.addEventListener(
            "click",
            event => {

                const button =
                    event.target.closest(
                        ".sd-review-btn"
                    );

                if (!button) {
                    return;
                }

                const row =
                    button.closest("tr");

                selectJob(
                    row,
                    button
                );
            }
        );

        assignmentForm?.addEventListener(
            "submit",
            event => {

                event.preventDefault();

                if (!selectedJob) {

                    assignmentMessage.textContent =
                        "Select a job card first.";

                    assignmentMessage.className =
                        "sd-form-message error";

                    return;
                }

                const mechanic =
                    mechanicSelect.value;

                const bay =
                    baySelect.value;

                if (!mechanic || !bay) {

                    assignmentMessage.textContent =
                        "Select both a mechanic and workshop bay.";

                    assignmentMessage.className =
                        "sd-form-message error";

                    return;
                }

                if (occupiedBays.has(bay)) {

                    assignmentMessage.textContent =
                        `${bay} is already occupied. Select another workshop bay.`;

                    assignmentMessage.className =
                        "sd-form-message error";

                    return;
                }

                const existingStatus =
                    selectedJob.row.querySelector(
                        ".job-status"
                    );

                if (
                    existingStatus &&
                    existingStatus.textContent
                        .trim() === "Assigned"
                ) {

                    assignmentMessage.textContent =
                        `${selectedJob.jobCard} has already been assigned.`;

                    assignmentMessage.className =
                        "sd-form-message error";

                    return;
                }

                occupiedBays.add(bay);

                /* =================================================
                   MANAGER ASSIGNMENT -> SHARED STORE
                   Temporary frontend bridge.
                   Backend later replaces this with an API call.
                   ================================================= */

                if (
                    selectedJob.row.dataset.sharedJob === "true" &&
                    window.ShiftDynamicsStore
                ) {

                    const updatedSharedJob =
                        ShiftDynamicsStore.updateJob(
                            selectedJob.jobCard,
                            {
                                status: "Assigned",

                                assignment: {
                                    mechanicName:
                                        mechanic,

                                    bay:
                                        bay,

                                    managerNote:
                                        managerNote.value.trim(),

                                    assignedAt:
                                        new Date().toISOString()
                                }
                            }
                        );

                    if (!updatedSharedJob) {

                        occupiedBays.delete(bay);

                        assignmentMessage.textContent =
                            `Unable to update ${selectedJob.jobCard} in the shared workflow store.`;

                        assignmentMessage.className =
                            "sd-form-message error";

                        return;
                    }
                }

                if (existingStatus) {

                    existingStatus.textContent =
                        "Assigned";

                    existingStatus.className =
                        "sd-status sd-status-progress job-status";
                }

                const assignmentRow =
                    document.createElement(
                        "tr"
                    );

                assignmentRow.innerHTML = `
                    <td>
                        <strong>
                            ${escapeHTML(
                                selectedJob.jobCard
                            )}
                        </strong>
                    </td>

                    <td>
                        ${escapeHTML(
                            selectedJob.vehicle
                        )}
                    </td>

                    <td>
                        ${escapeHTML(mechanic)}
                    </td>

                    <td>
                        ${escapeHTML(bay)}
                    </td>

                    <td>
                        <span class="sd-status sd-status-progress">
                            Assigned
                        </span>
                    </td>
                `;

                if (
                    selectedJob.row.dataset.sharedJob !== "true"
                ) {
                    assignmentTable?.prepend(
                        assignmentRow
                    );
                }

                assignmentMessage.textContent =
                    `${selectedJob.jobCard} assigned to ${mechanic} in ${bay}.`;

                assignmentMessage.className =
                    "sd-form-message success";

                const selectedButton =
                    selectedJob.row.querySelector(
                        ".sd-review-btn"
                    );

                if (selectedButton) {

                    selectedButton.textContent =
                        "Assigned";

                    selectedButton.classList.remove(
                        "selected"
                    );

                    selectedButton.disabled = true;
                }

                mechanicSelect.value = "";
                baySelect.value = "";
                managerNote.value = "";

                mechanicSelect.disabled = true;
                baySelect.disabled = true;
                managerNote.disabled = true;
                assignJobButton.disabled = true;

                selectedJobNumber.textContent =
                    "Select a job card";

                selectedJobVehicle.textContent =
                    "Review a job card to begin.";

                selectedJob = null;
            }
        );

        /* =================================================
           LIVE VENDOR BID COMPARISON
           Shared store is the temporary frontend data source.
           ================================================= */

        const vendorQuoteGrid =
            document.getElementById(
                "vendorQuoteGrid"
            );

        const vendorQuoteCount =
            document.getElementById(
                "vendorQuoteCount"
            );

        const vendorSelection =
            document.getElementById(
                "vendorSelection"
            );

        const vendorApprovalNote =
            document.getElementById(
                "vendorApprovalNote"
            );

        const approveVendorButton =
            document.getElementById(
                "approveVendorButton"
            );

        const vendorApprovalMessage =
            document.getElementById(
                "vendorApprovalMessage"
            );

        let selectedVendorQuote = null;
        let vendorApproved = false;
        let managerVendorQuotes = [];

        function normalizeManagerIdentifier(value) {
            return String(value || "")
                .trim()
                .replace(/^#+/, "");
        }

        function formatLKR(value) {

            return Number(value)
                .toLocaleString("en-LK");
        }

        function renderSharedVendorQuotes() {
            if (
                !vendorQuoteGrid ||
                !window.ShiftDynamicsStore ||
                typeof ShiftDynamicsStore.getVendorQuotes !== "function"
            ) {
                return;
            }

            managerVendorQuotes =
                ShiftDynamicsStore.getVendorQuotes();

            const vendorRequests =
                typeof ShiftDynamicsStore.getVendorRequests === "function"
                    ? ShiftDynamicsStore.getVendorRequests()
                    : [];

            const partRequests =
                typeof ShiftDynamicsStore.getPartRequests === "function"
                    ? ShiftDynamicsStore.getPartRequests()
                    : [];

            if (vendorQuoteCount) {
                vendorQuoteCount.textContent =
                    `${managerVendorQuotes.length} Quote${
                        managerVendorQuotes.length === 1 ? "" : "s"
                    }`;
            }

            if (!managerVendorQuotes.length) {
                vendorQuoteGrid.innerHTML = `
                    <article class="sd-panel">
                        No vendor quotations awaiting management review.
                    </article>
                `;

                const requestSummary =
                    document.querySelector(".sd-request-summary");

                if (requestSummary) {
                    requestSummary.hidden = true;
                }

                return;
            }

            const requestSummary =
                document.querySelector(".sd-request-summary");

            if (requestSummary) {
                requestSummary.hidden = false;
            }

            vendorQuoteGrid.innerHTML =
                managerVendorQuotes.map(quote => {
                    const request = vendorRequests.find(
                        item => item.vendorRequestId === quote.vendorRequestId
                    );

                    const partRequest = partRequests.find(
                        item => item.requestId === request?.sourceRequestId
                    );

                    const managerApproved =
                        [
                            "Manager Approved",
                            "Purchase Order Approved"
                        ].includes(quote.status);

                    return `
                        <article
                            class="sd-vendor-quote ${managerApproved ? "approved" : ""}"
                            data-quote-id="${escapeHTML(quote.quoteId)}"
                            data-request-id="${escapeHTML(quote.vendorRequestId)}"
                            data-source-request="${escapeHTML(request?.sourceRequestId || "")}"
                            data-vendor="${escapeHTML(quote.vendorName || "Registered Vendor")}"
                            data-price="${escapeHTML(quote.price)}"
                            data-delivery="${escapeHTML(quote.delivery)}"
                        >
                            <div class="sd-vendor-top">
                                <div class="sd-vendor-icon">
                                    <i class="bi bi-shop"></i>
                                </div>

                                <div>
                                    <span>${escapeHTML(quote.quoteId)}</span>
                                    <h3>${escapeHTML(quote.vendorName || "Registered Vendor")}</h3>
                                </div>
                            </div>

                            <div class="sd-quote-details">
                                <div>
                                    <span>Part</span>
                                    <strong>${escapeHTML(quote.part)}</strong>
                                </div>

                                <div>
                                    <span>Quoted Price</span>
                                    <strong>LKR ${formatLKR(quote.price)}</strong>
                                </div>

                                <div>
                                    <span>Delivery</span>
                                    <strong>${escapeHTML(quote.delivery)}</strong>
                                </div>

                                <div>
                                    <span>Availability</span>
                                    <strong class="sd-available">${escapeHTML(quote.stock)}</strong>
                                </div>

                                <div>
                                    <span>Warranty</span>
                                    <strong>${escapeHTML(quote.warranty)}</strong>
                                </div>

                                <div>
                                    <span>Status</span>
                                    <strong>${escapeHTML(quote.status)}</strong>
                                </div>
                            </div>

                            <button
                                type="button"
                                class="sd-select-quote"
                                ${managerApproved ? "disabled" : ""}
                            >
                                <i class="bi bi-check2-circle"></i>
                                ${managerApproved ? escapeHTML(quote.status) : "Select Quote"}
                            </button>
                        </article>
                    `;
                }).join("");

            const firstQuote = managerVendorQuotes[0];
            const firstRequest = vendorRequests.find(
                item => item.vendorRequestId === firstQuote.vendorRequestId
            );
            const firstPartRequest = partRequests.find(
                item => item.requestId === firstRequest?.sourceRequestId
            );

            if (requestSummary) {
                requestSummary.innerHTML = `
                    <div class="sd-panel-header">
                        <div>
                            <span class="sd-panel-label">Parts Request</span>
                            <h3>#${escapeHTML(normalizeManagerIdentifier(
                                firstRequest?.sourceRequestId || firstQuote.vendorRequestId
                            ))}</h3>
                        </div>
                        <span class="sd-status sd-status-warning">Manager Review</span>
                    </div>

                    <div class="sd-request-grid">
                        <div><span>Job Card</span><strong>#${escapeHTML(normalizeManagerIdentifier(firstQuote.jobCardNumber))}</strong></div>
                        <div><span>Vehicle</span><strong>${escapeHTML(firstQuote.vehicle || firstRequest?.vehicle || "Vehicle")}</strong></div>
                        <div><span>Requested Part</span><strong>${escapeHTML(firstQuote.part)}</strong></div>
                        <div><span>Quantity</span><strong>${escapeHTML(firstQuote.quantity)}</strong></div>
                        <div><span>Requested By</span><strong>${escapeHTML(firstPartRequest?.mechanic || "Storekeeper")}</strong></div>
                        <div><span>Stock Status</span><strong class="sd-out-stock">External Vendor</strong></div>
                    </div>
                `;
            }

            const approvedQuote = managerVendorQuotes.find(
                quote => [
                    "Manager Approved",
                    "Purchase Order Approved"
                ].includes(quote.status)
            );

            vendorApproved = Boolean(approvedQuote);

            if (approvedQuote) {
                vendorSelection.className = "sd-vendor-selection-active";
                vendorSelection.innerHTML = `
                    <i class="bi bi-check2-circle"></i>
                    <div>
                        <strong>${escapeHTML(approvedQuote.vendorName || "Vendor")} Approved</strong>
                        <span>LKR ${formatLKR(approvedQuote.price)} - ${escapeHTML(approvedQuote.delivery)} delivery</span>
                    </div>
                `;
                vendorApprovalNote.disabled = true;
                approveVendorButton.disabled = true;
            }
        }

        function selectVendorQuote(card) {

            if (!card || vendorApproved) {
                return;
            }

            document
                .querySelectorAll(
                    ".sd-vendor-quote"
                )
                .forEach(quote => {

                    quote.classList.remove(
                        "selected"
                    );

                    const button =
                        quote.querySelector(
                            ".sd-select-quote"
                        );

                    if (button) {
                        button.innerHTML =
                            '<i class="bi bi-check2-circle"></i> Select Quote';
                    }
                });

            card.classList.add(
                "selected"
            );

            const button =
                card.querySelector(
                    ".sd-select-quote"
                );

            if (button) {

                button.innerHTML =
                    '<i class="bi bi-check2"></i> Selected';
            }

            selectedVendorQuote = {

                quoteId:
                    card.dataset.quoteId,

                requestId:
                    card.dataset.requestId,

                sourceRequestId:
                    card.dataset.sourceRequest,

                vendor:
                    card.dataset.vendor,

                price:
                    Number(
                        card.dataset.price
                    ),

                delivery:
                    card.dataset.delivery,

                card
            };

            vendorSelection.className =
                "sd-vendor-selection-active";

            vendorSelection.innerHTML = `
                <i class="bi bi-shop-window"></i>

                <div>
                    <strong>
                        ${escapeHTML(
                            selectedVendorQuote.vendor
                        )}
                    </strong>

                    <span>
                        LKR ${formatLKR(
                            selectedVendorQuote.price
                        )}
                        - Delivery:
                        ${escapeHTML(
                            selectedVendorQuote.delivery
                        )}
                    </span>
                </div>
            `;

            vendorApprovalNote.disabled =
                false;

            approveVendorButton.disabled =
                false;

            vendorApprovalMessage.textContent =
                "Quote selected. Review the price and delivery time before approval.";

            vendorApprovalMessage.className =
                "sd-form-message";
        }

        vendorQuoteGrid?.addEventListener(
            "click",
            event => {

                const button =
                    event.target.closest(
                        ".sd-select-quote"
                    );

                if (!button) {
                    return;
                }

                const card =
                    button.closest(
                        ".sd-vendor-quote"
                    );

                selectVendorQuote(card);
            }
        );

        approveVendorButton?.addEventListener(
            "click",
            () => {

                if (!selectedVendorQuote) {

                    vendorApprovalMessage.textContent =
                        "Select a vendor quote first.";

                    vendorApprovalMessage.className =
                        "sd-form-message error";

                    return;
                }

                if (vendorApproved) {

                    vendorApprovalMessage.textContent =
                        "A vendor has already been approved for this request.";

                    vendorApprovalMessage.className =
                        "sd-form-message error";

                    return;
                }

                if (
                    !window.ShiftDynamicsStore ||
                    typeof ShiftDynamicsStore.updateVendorQuote !== "function" ||
                    typeof ShiftDynamicsStore.updateVendorRequest !== "function"
                ) {
                    vendorApprovalMessage.textContent =
                        "Shared vendor approval workflow is unavailable.";

                    vendorApprovalMessage.className =
                        "sd-form-message error";

                    return;
                }

                vendorApproved = true;

                ShiftDynamicsStore.updateVendorQuote(
                    selectedVendorQuote.quoteId,
                    {
                        status: "Manager Approved",
                        managerApprovalNote:
                            vendorApprovalNote.value.trim(),
                        managerApprovedAt:
                            new Date().toISOString(),
                        managerApprovedBy:
                            "Operations Manager"
                    }
                );

                ShiftDynamicsStore.updateVendorRequest(
                    selectedVendorQuote.requestId,
                    {
                        status: "Manager Approved",
                        managerApprovedQuoteId:
                            selectedVendorQuote.quoteId,
                        managerApprovedAt:
                            new Date().toISOString()
                    }
                );

                if (
                    selectedVendorQuote.sourceRequestId &&
                    typeof ShiftDynamicsStore.updatePartRequest === "function"
                ) {
                    ShiftDynamicsStore.updatePartRequest(
                        selectedVendorQuote.sourceRequestId,
                        {
                            status: "manager-approved",
                            vendorQuoteId:
                                selectedVendorQuote.quoteId,
                            vendorName:
                                selectedVendorQuote.vendor,
                            vendorPrice:
                                selectedVendorQuote.price,
                            vendorDelivery:
                                selectedVendorQuote.delivery
                        }
                    );
                }

                selectedVendorQuote.card.classList.remove(
                    "selected"
                );

                selectedVendorQuote.card.classList.add(
                    "approved"
                );

                document
                    .querySelectorAll(
                        ".sd-select-quote"
                    )
                    .forEach(button => {

                        button.disabled = true;
                    });

                const approvedButton =
                    selectedVendorQuote.card
                        .querySelector(
                            ".sd-select-quote"
                        );

                if (approvedButton) {

                    approvedButton.innerHTML =
                        '<i class="bi bi-check2-circle"></i> Approved';
                }

                vendorSelection.className =
                    "sd-vendor-selection-active";

                vendorSelection.innerHTML = `
                    <i class="bi bi-check2-circle"></i>

                    <div>
                        <strong>
                            ${escapeHTML(
                                selectedVendorQuote.vendor
                            )} Approved
                        </strong>

                        <span>
                            LKR ${formatLKR(
                                selectedVendorQuote.price
                            )}
                            - ${escapeHTML(
                                selectedVendorQuote.delivery
                            )}
                            delivery
                        </span>
                    </div>
                `;

                vendorApprovalNote.disabled =
                    true;

                approveVendorButton.disabled =
                    true;

                vendorApprovalMessage.textContent =
                    `${selectedVendorQuote.vendor} approved for ${
                        selectedVendorQuote.sourceRequestId ||
                        selectedVendorQuote.requestId
                    }.`;

                vendorApprovalMessage.className =
                    "sd-form-message success";
            }
        );

        renderSharedVendorQuotes();

        if (
            window.ShiftDynamicsStore &&
            typeof ShiftDynamicsStore.subscribe === "function"
        ) {
            ShiftDynamicsStore.subscribe(() => {
                renderSharedVendorQuotes();
            });
        }

        /* =================================================
           LIVE PURCHASE ORDER APPROVAL
           Approved vendor quote -> shared purchase order.
           ================================================= */

        const confirmPurchaseOrder =
            document.getElementById(
                "confirmPurchaseOrder"
            );

        const approveOrderButton =
            document.getElementById(
                "approveOrderButton"
            );

        const orderApprovalMessage =
            document.getElementById(
                "orderApprovalMessage"
            );

        const purchaseOrderStatus =
            document.getElementById(
                "purchaseOrderStatus"
            );

        const orderApprovalNote =
            document.getElementById(
                "orderApprovalNote"
            );

        let purchaseOrderApproved = false;
        let currentPurchaseOrder = null;

        const ordersSection =
            document.getElementById("orders");

        const orderPanels =
            ordersSection?.querySelectorAll(
                ".sd-order-layout > .sd-panel"
            ) || [];

        const purchaseOrderPanel = orderPanels[0];
        const procurementDetailsPanel = orderPanels[1];

        function renderSharedPurchaseOrder() {
            const store = window.ShiftDynamicsStore;

            if (
                !ordersSection ||
                !store ||
                typeof store.getVendorQuotes !== "function" ||
                typeof store.getPurchaseOrders !== "function"
            ) {
                return;
            }

            const quotes = store.getVendorQuotes();
            const approvedQuote = quotes.find(quote =>
                [
                    "Manager Approved",
                    "Purchase Order Approved"
                ].includes(quote.status)
            );

            if (!approvedQuote) {
                currentPurchaseOrder = null;
                purchaseOrderApproved = false;

                if (purchaseOrderPanel) {
                    purchaseOrderPanel.innerHTML =
                        "No manager-approved vendor quotation is available.";
                }

                if (procurementDetailsPanel) {
                    procurementDetailsPanel.innerHTML =
                        "Approve a vendor quotation before creating a purchase order.";
                }

                if (purchaseOrderStatus) {
                    purchaseOrderStatus.textContent = "Waiting for Quote";
                    purchaseOrderStatus.className =
                        "sd-status sd-status-warning";
                }

                confirmPurchaseOrder.checked = false;
                confirmPurchaseOrder.disabled = true;
                approveOrderButton.disabled = true;
                orderApprovalNote.disabled = true;
                return;
            }

            const vendorRequests =
                typeof store.getVendorRequests === "function"
                    ? store.getVendorRequests()
                    : [];

            const partRequests =
                typeof store.getPartRequests === "function"
                    ? store.getPartRequests()
                    : [];

            const vendorRequest = vendorRequests.find(
                request =>
                    request.vendorRequestId ===
                    approvedQuote.vendorRequestId
            );

            const partRequest = partRequests.find(
                request =>
                    request.requestId ===
                    vendorRequest?.sourceRequestId
            );

            let purchaseOrders = store.getPurchaseOrders();
            currentPurchaseOrder = purchaseOrders.find(
                order => order.quoteId === approvedQuote.quoteId
            ) || null;

            if (
                !currentPurchaseOrder &&
                typeof store.createPurchaseOrder === "function"
            ) {
                currentPurchaseOrder = store.createPurchaseOrder({
                    quoteId: approvedQuote.quoteId,
                    vendorRequestId: approvedQuote.vendorRequestId,
                    sourceRequestId:
                        vendorRequest?.sourceRequestId || "",
                    jobCardNumber:
                        approvedQuote.jobCardNumber ||
                        vendorRequest?.jobCardNumber || "",
                    vehicle:
                        approvedQuote.vehicle ||
                        vendorRequest?.vehicle || "Vehicle",
                    part:
                        approvedQuote.part ||
                        vendorRequest?.part || "Part",
                    partNumber:
                        approvedQuote.partNumber ||
                        vendorRequest?.partNumber || "",
                    quantity:
                        Number(
                            approvedQuote.quantity ||
                            vendorRequest?.quantity || 1
                        ),
                    unitPrice: Number(approvedQuote.price || 0),
                    vendorName:
                        approvedQuote.vendorName ||
                        "Registered Vendor",
                    delivery: approvedQuote.delivery || "-",
                    availability: approvedQuote.stock || "-",
                    warranty: approvedQuote.warranty || "-",
                    requestedBy:
                        partRequest?.mechanic || "Storekeeper",
                    status: "Pending Approval"
                });
            }

            if (!currentPurchaseOrder) {
                return;
            }

            const order = currentPurchaseOrder;
            const quantity = Number(order.quantity || 1);
            const unitPrice = Number(order.unitPrice || 0);
            const total = quantity * unitPrice;

            if (purchaseOrderPanel) {
                purchaseOrderPanel.innerHTML = `
                    <div class="sd-panel-header">
                        <div>
                            <span class="sd-panel-label">Purchase Order</span>
                            <h3>#${escapeHTML(normalizeManagerIdentifier(order.purchaseOrderId))}</h3>
                        </div>
                        <span class="sd-count-badge">1 Item</span>
                    </div>

                    <div class="sd-order-meta-grid">
                        <div><span>Parts Request</span><strong>#${escapeHTML(normalizeManagerIdentifier(order.sourceRequestId))}</strong></div>
                        <div><span>Job Card</span><strong>#${escapeHTML(normalizeManagerIdentifier(order.jobCardNumber))}</strong></div>
                        <div><span>Vehicle</span><strong>${escapeHTML(order.vehicle)}</strong></div>
                        <div><span>Requested By</span><strong>${escapeHTML(order.requestedBy)}</strong></div>
                    </div>

                    <div class="sd-table-wrap">
                        <table class="sd-table">
                            <thead>
                                <tr>
                                    <th>Part</th>
                                    <th>Qty</th>
                                    <th>Unit Price</th>
                                    <th>Total</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>
                                        <strong>${escapeHTML(order.part)}</strong>
                                        <small>${escapeHTML(order.partNumber || "Approved quotation")}</small>
                                    </td>
                                    <td>${escapeHTML(quantity)}</td>
                                    <td>LKR ${formatLKR(unitPrice)}</td>
                                    <td><strong>LKR ${formatLKR(total)}</strong></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <div class="sd-order-total">
                        <span>Purchase Order Total</span>
                        <strong>LKR ${formatLKR(total)}</strong>
                    </div>
                `;
            }

            if (procurementDetailsPanel) {
                procurementDetailsPanel.innerHTML = `
                    <div class="sd-panel-header">
                        <div>
                            <span class="sd-panel-label">Approved Vendor</span>
                            <h3>Procurement Details</h3>
                        </div>
                    </div>

                    <div class="sd-approved-vendor-card">
                        <div class="sd-vendor-icon"><i class="bi bi-shop"></i></div>
                        <div>
                            <span>Vendor</span>
                            <strong>${escapeHTML(order.vendorName)}</strong>
                            <small>Quotation ${escapeHTML(order.quoteId)}</small>
                        </div>
                    </div>

                    <div class="sd-order-detail-list">
                        <div><span>Quoted Unit Price</span><strong>LKR ${formatLKR(unitPrice)}</strong></div>
                        <div><span>Delivery Lead Time</span><strong>${escapeHTML(order.delivery)}</strong></div>
                        <div><span>Availability</span><strong class="sd-available">${escapeHTML(order.availability)}</strong></div>
                        <div><span>Warranty</span><strong>${escapeHTML(order.warranty)}</strong></div>
                    </div>

                    <div class="sd-order-warning">
                        <i class="bi bi-info-circle"></i>
                        <p>Confirm that the vendor, price and delivery terms match the approved quotation before authorizing this order.</p>
                    </div>
                `;
            }

            purchaseOrderApproved = order.status === "Approved";

            if (purchaseOrderStatus) {
                purchaseOrderStatus.textContent = order.status;
                purchaseOrderStatus.className = purchaseOrderApproved
                    ? "sd-status sd-status-ready"
                    : "sd-status sd-status-warning";
            }

            if (purchaseOrderApproved) {
                orderApprovalNote.value = order.managerApprovalNote || "";
                orderApprovalNote.disabled = true;
                confirmPurchaseOrder.checked = true;
                confirmPurchaseOrder.disabled = true;
                approveOrderButton.disabled = true;
                orderApprovalMessage.textContent =
                    `Purchase Order #${normalizeManagerIdentifier(order.purchaseOrderId)} is approved.`;
                orderApprovalMessage.className =
                    "sd-form-message success";
            } else {
                orderApprovalNote.disabled = false;
                confirmPurchaseOrder.disabled = false;
                approveOrderButton.disabled =
                    !confirmPurchaseOrder.checked;
            }
        }

        confirmPurchaseOrder?.addEventListener(
            "change",
            () => {

                if (purchaseOrderApproved || !currentPurchaseOrder) {
                    return;
                }

                approveOrderButton.disabled =
                    !confirmPurchaseOrder.checked;
            }
        );

        approveOrderButton?.addEventListener(
            "click",
            () => {

                if (purchaseOrderApproved) {

                    orderApprovalMessage.textContent =
                        `Purchase Order #${normalizeManagerIdentifier(currentPurchaseOrder.purchaseOrderId)} has already been approved.`;

                    orderApprovalMessage.className =
                        "sd-form-message error";

                    return;
                }

                if (!confirmPurchaseOrder.checked) {

                    orderApprovalMessage.textContent =
                        "Confirm that you reviewed the purchase order before approval.";

                    orderApprovalMessage.className =
                        "sd-form-message error";

                    return;
                }

                const store = window.ShiftDynamicsStore;

                if (
                    !store ||
                    typeof store.updatePurchaseOrder !== "function"
                ) {
                    orderApprovalMessage.textContent =
                        "Shared purchase-order workflow is unavailable.";
                    orderApprovalMessage.className =
                        "sd-form-message error";
                    return;
                }

                const approvedAt = new Date().toISOString();

                const savedOrder = store.updatePurchaseOrder(
                    currentPurchaseOrder.purchaseOrderId,
                    {
                        status: "Approved",
                        managerApprovalNote:
                            orderApprovalNote.value.trim(),
                        managerApprovedAt: approvedAt,
                        managerApprovedBy: "Operations Manager"
                    }
                );

                if (!savedOrder) {
                    orderApprovalMessage.textContent =
                        "Purchase Order could not be saved.";
                    orderApprovalMessage.className =
                        "sd-form-message error";
                    return;
                }

                if (typeof store.updateVendorQuote === "function") {
                    store.updateVendorQuote(
                        currentPurchaseOrder.quoteId,
                        {
                            status: "Purchase Order Approved",
                            purchaseOrderId:
                                currentPurchaseOrder.purchaseOrderId,
                            purchaseOrderApprovedAt: approvedAt
                        }
                    );
                }

                if (typeof store.updateVendorRequest === "function") {
                    store.updateVendorRequest(
                        currentPurchaseOrder.vendorRequestId,
                        {
                            status: "Purchase Order Approved",
                            purchaseOrderId:
                                currentPurchaseOrder.purchaseOrderId,
                            purchaseOrderApprovedAt: approvedAt
                        }
                    );
                }

                if (
                    currentPurchaseOrder.sourceRequestId &&
                    typeof store.updatePartRequest === "function"
                ) {
                    store.updatePartRequest(
                        currentPurchaseOrder.sourceRequestId,
                        {
                            status: "purchase-order-approved",
                            purchaseOrderId:
                                currentPurchaseOrder.purchaseOrderId,
                            purchaseOrderApprovedAt: approvedAt
                        }
                    );
                }

                currentPurchaseOrder = savedOrder;
                purchaseOrderApproved = true;

                if (purchaseOrderStatus) {

                    purchaseOrderStatus.textContent =
                        "Approved";

                    purchaseOrderStatus.className =
                        "sd-status sd-status-ready";
                }

                approveOrderButton.disabled =
                    true;

                confirmPurchaseOrder.disabled =
                    true;

                if (orderApprovalNote) {
                    orderApprovalNote.disabled = true;
                }

                orderApprovalMessage.textContent =
                    `Purchase Order #${normalizeManagerIdentifier(currentPurchaseOrder.purchaseOrderId)} approved successfully. Procurement can continue.`;

                orderApprovalMessage.className =
                    "sd-form-message success";
            }
        );

        renderSharedPurchaseOrder();

        if (
            window.ShiftDynamicsStore &&
            typeof ShiftDynamicsStore.subscribe === "function"
        ) {
            ShiftDynamicsStore.subscribe(() => {
                renderSharedPurchaseOrder();
            });
        }

        /* =================================================
           LIVE FINAL BILLING APPROVAL
           Job estimate + approved purchase order -> invoice.
           ================================================= */

        const confirmBilling =
            document.getElementById(
                "confirmBilling"
            );

        const approveBillingButton =
            document.getElementById(
                "approveBillingButton"
            );

        const billingStatus =
            document.getElementById(
                "billingStatus"
            );

        const billingMessage =
            document.getElementById(
                "billingMessage"
            );

        const billingApprovalNote =
            document.getElementById(
                "billingApprovalNote"
            );

        let billingApproved = false;
        let currentBillingOrder = null;
        let currentBillingJob = null;
        let currentBillingTotal = 0;
        let currentBillingInvoiceNumber = "";

        const billingSection =
            document.getElementById("billing");

        const billingPanels =
            billingSection?.querySelectorAll(
                ".sd-billing-layout > .sd-panel"
            ) || [];

        const customerInvoicePanel = billingPanels[0];
        const billingChecksPanel = billingPanels[1];

        function renderSharedBillingReview() {
            const store = window.ShiftDynamicsStore;

            if (
                !billingSection ||
                !store ||
                typeof store.getPurchaseOrders !== "function" ||
                typeof store.getJobs !== "function"
            ) {
                return;
            }

            const approvedOrders = store
                .getPurchaseOrders()
                .filter(order => order.status === "Approved");

            currentBillingOrder = approvedOrders[0] || null;

            if (!currentBillingOrder) {
                currentBillingJob = null;
                billingApproved = false;

                if (customerInvoicePanel) {
                    customerInvoicePanel.innerHTML =
                        "No approved purchase order is ready for billing review.";
                }

                if (billingChecksPanel) {
                    billingChecksPanel.innerHTML =
                        "Approve a purchase order before reviewing final billing.";
                }

                billingStatus.textContent = "Waiting for Order";
                billingStatus.className =
                    "sd-status sd-status-warning";
                confirmBilling.checked = false;
                confirmBilling.disabled = true;
                approveBillingButton.disabled = true;
                billingApprovalNote.disabled = true;
                return;
            }

            const normalizedOrderJob = normalizeManagerIdentifier(
                currentBillingOrder.jobCardNumber
            ).toUpperCase();

            currentBillingJob = store.getJobs().find(job =>
                normalizeManagerIdentifier(
                    job?.jobCardNumber
                ).toUpperCase() === normalizedOrderJob
            ) || null;

            const order = currentBillingOrder;
            const job = currentBillingJob || {};
            const estimate = job.estimate || {};
            const invoice = job.invoice || {};

            const labour = Number(estimate.labour || 0);
            const internalParts = Number(estimate.parts || 0);
            const otherCharges = Number(estimate.other || 0);
            const vendorCost =
                Number(order.quantity || 1) *
                Number(order.unitPrice || 0);

            currentBillingTotal =
                labour +
                internalParts +
                otherCharges +
                vendorCost;

            const jobDigits = String(
                order.jobCardNumber || Date.now()
            ).replace(/\D/g, "");

            currentBillingInvoiceNumber =
                invoice.number || `#INV-${jobDigits || Date.now()}`;

            const vehicleName =
                [job.vehicle?.make, job.vehicle?.model]
                    .filter(Boolean)
                    .join(" ") ||
                order.vehicle ||
                "Vehicle";

            const customerName =
                job.customer?.name ||
                job.customerName ||
                "Customer";

            const plate = job.vehicle?.plate || "--";
            const service =
                job.serviceConcern ||
                estimate.description ||
                "Vehicle Service";

            if (customerInvoicePanel) {
                customerInvoicePanel.innerHTML = `
                    <div class="sd-panel-header">
                        <div>
                            <span class="sd-panel-label">Customer Invoice</span>
                            <h3>${escapeHTML(currentBillingInvoiceNumber)}</h3>
                        </div>
                        <span class="sd-count-badge">#${escapeHTML(normalizeManagerIdentifier(order.jobCardNumber))}</span>
                    </div>

                    <div class="sd-order-meta-grid">
                        <div><span>Customer</span><strong>${escapeHTML(customerName)}</strong></div>
                        <div><span>Vehicle</span><strong>${escapeHTML(vehicleName)}</strong></div>
                        <div><span>License Plate</span><strong>${escapeHTML(plate)}</strong></div>
                        <div><span>Service</span><strong>${escapeHTML(service)}</strong></div>
                    </div>

                    <div class="sd-table-wrap">
                        <table class="sd-table">
                            <thead>
                                <tr>
                                    <th>Description</th>
                                    <th>Type</th>
                                    <th>Amount</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>Workshop Labour</td>
                                    <td>Labour</td>
                                    <td>LKR ${formatLKR(labour)}</td>
                                </tr>
                                <tr>
                                    <td>Workshop Parts & Materials</td>
                                    <td>In-Stock Parts</td>
                                    <td>LKR ${formatLKR(internalParts)}</td>
                                </tr>
                                <tr>
                                    <td>${escapeHTML(order.part)}</td>
                                    <td>Vendor Part</td>
                                    <td>LKR ${formatLKR(vendorCost)}</td>
                                </tr>
                                ${otherCharges > 0 ? `
                                    <tr>
                                        <td>Other Service Charges</td>
                                        <td>Other</td>
                                        <td>LKR ${formatLKR(otherCharges)}</td>
                                    </tr>
                                ` : ""}
                            </tbody>
                        </table>
                    </div>

                    <div class="sd-billing-totals">
                        <div><span>Labour</span><strong>LKR ${formatLKR(labour)}</strong></div>
                        <div><span>In-Stock Parts</span><strong>LKR ${formatLKR(internalParts)}</strong></div>
                        <div><span>Vendor Parts</span><strong>LKR ${formatLKR(vendorCost)}</strong></div>
                        ${otherCharges > 0 ? `
                            <div><span>Other Charges</span><strong>LKR ${formatLKR(otherCharges)}</strong></div>
                        ` : ""}
                        <div class="sd-billing-grand-total">
                            <span>Final Invoice Total</span>
                            <strong>LKR ${formatLKR(currentBillingTotal)}</strong>
                        </div>
                    </div>
                `;
            }

            if (billingChecksPanel) {
                billingChecksPanel.innerHTML = `
                    <div class="sd-panel-header">
                        <div>
                            <span class="sd-panel-label">Verification</span>
                            <h3>Billing Checks</h3>
                        </div>
                    </div>

                    <div class="sd-billing-check-list">
                        <div><i class="bi bi-check2-circle"></i><div><strong>Labour Charges</strong><span>Job-card labour: LKR ${formatLKR(labour)}</span></div></div>
                        <div><i class="bi bi-check2-circle"></i><div><strong>Internal Parts</strong><span>Workshop stock: LKR ${formatLKR(internalParts)}</span></div></div>
                        <div><i class="bi bi-check2-circle"></i><div><strong>Vendor Cost</strong><span>${escapeHTML(order.purchaseOrderId)}: LKR ${formatLKR(vendorCost)}</span></div></div>
                        <div><i class="bi bi-check2-circle"></i><div><strong>Invoice Total</strong><span>Reconciled: LKR ${formatLKR(currentBillingTotal)}</span></div></div>
                    </div>

                    <div class="sd-billing-info">
                        <i class="bi bi-shield-check"></i>
                        <p>Final approval confirms that this live invoice is ready for the customer payment process.</p>
                    </div>
                `;
            }

            billingApproved =
                order.billingStatus === "Approved" ||
                invoice.managerBillingStatus === "Approved";

            if (billingApproved) {
                billingStatus.textContent = "Billing Approved";
                billingStatus.className =
                    "sd-status sd-status-ready";
                billingApprovalNote.value =
                    order.billingApprovalNote ||
                    invoice.managerApprovalNote ||
                    "";
                billingApprovalNote.disabled = true;
                confirmBilling.checked = true;
                confirmBilling.disabled = true;
                approveBillingButton.disabled = true;
                billingMessage.textContent =
                    `${currentBillingInvoiceNumber} is approved and ready for customer payment.`;
                billingMessage.className =
                    "sd-form-message success";
            } else {
                billingStatus.textContent = "Pending Review";
                billingStatus.className =
                    "sd-status sd-status-warning";
                billingApprovalNote.disabled = false;
                confirmBilling.disabled = false;
                approveBillingButton.disabled =
                    !confirmBilling.checked;
            }
        }

        confirmBilling?.addEventListener(
            "change",
            () => {

                if (billingApproved || !currentBillingOrder) {
                    return;
                }

                approveBillingButton.disabled =
                    !confirmBilling.checked;
            }
        );

        approveBillingButton?.addEventListener(
            "click",
            () => {

                if (billingApproved) {

                    billingMessage.textContent =
                        `${currentBillingInvoiceNumber} has already been approved.`;

                    billingMessage.className =
                        "sd-form-message error";

                    return;
                }

                if (!confirmBilling.checked) {

                    billingMessage.textContent =
                        "Review and confirm the billing details before approval.";

                    billingMessage.className =
                        "sd-form-message error";

                    return;
                }

                const store = window.ShiftDynamicsStore;

                if (
                    !store ||
                    typeof store.updatePurchaseOrder !== "function" ||
                    typeof store.updateJob !== "function" ||
                    !currentBillingJob
                ) {
                    billingMessage.textContent =
                        "Shared billing workflow is unavailable.";
                    billingMessage.className =
                        "sd-form-message error";
                    return;
                }

                const approvedAt = new Date().toISOString();
                const approvalNote =
                    billingApprovalNote.value.trim();

                const updatedJob = store.updateJob(
                    currentBillingJob.jobCardNumber,
                    {
                        invoice: {
                            ...(currentBillingJob.invoice || {}),
                            number: currentBillingInvoiceNumber,
                            status: "Finalized",
                            total: currentBillingTotal,
                            managerBillingStatus: "Approved",
                            managerApprovalNote: approvalNote,
                            managerApprovedAt: approvedAt,
                            managerApprovedBy: "Operations Manager",
                            purchaseOrderId:
                                currentBillingOrder.purchaseOrderId
                        }
                    }
                );

                if (!updatedJob) {
                    billingMessage.textContent =
                        "Customer invoice could not be saved.";
                    billingMessage.className =
                        "sd-form-message error";
                    return;
                }

                const updatedOrder = store.updatePurchaseOrder(
                    currentBillingOrder.purchaseOrderId,
                    {
                        billingStatus: "Approved",
                        billingApprovalNote: approvalNote,
                        billingApprovedAt: approvedAt,
                        billingApprovedBy: "Operations Manager",
                        invoiceNumber: currentBillingInvoiceNumber,
                        invoiceTotal: currentBillingTotal
                    }
                );

                if (!updatedOrder) {
                    billingMessage.textContent =
                        "Billing approval could not be saved.";
                    billingMessage.className =
                        "sd-form-message error";
                    return;
                }

                currentBillingJob = updatedJob;
                currentBillingOrder = updatedOrder;
                billingApproved = true;

                billingStatus.textContent =
                    "Billing Approved";

                billingStatus.className =
                    "sd-status sd-status-ready";

                confirmBilling.disabled = true;
                approveBillingButton.disabled = true;
                billingApprovalNote.disabled = true;

                billingMessage.textContent =
                    `${currentBillingInvoiceNumber} approved. It is ready for the customer payment process.`;

                billingMessage.className =
                    "sd-form-message success";
            }
        );

        renderSharedBillingReview();

        if (
            window.ShiftDynamicsStore &&
            typeof ShiftDynamicsStore.subscribe === "function"
        ) {
            ShiftDynamicsStore.subscribe(() => {
                renderSharedBillingReview();
            });
        }

        /* =================================================
           LIVE MANAGER OVERVIEW - SHARED WORKFLOW V7
           ================================================= */

        function overviewEscape(value) {
            return String(value ?? "")
                .replaceAll("&", "&amp;")
                .replaceAll("<", "&lt;")
                .replaceAll(">", "&gt;")
                .replaceAll('"', "&quot;")
                .replaceAll("'", "&#039;");
        }

        function overviewIdentifier(value) {
            return String(value || "").trim().replace(/^#+/, "");
        }

        function overviewVehicle(job) {
            return [job?.vehicle?.make, job?.vehicle?.model]
                .filter(Boolean)
                .join(" ") || job?.vehicleName || "Vehicle";
        }

        function overviewJobStatus(job) {
            const raw = String(
                job?.status ||
                job?.handover?.status ||
                job?.invoice?.status ||
                (job?.assignment?.mechanicName ? "Assigned" : "Awaiting Assignment")
            ).trim();
            return raw || "Awaiting Assignment";
        }

        function overviewStatusClass(status) {
            const value = String(status || "").toLowerCase();
            if (value.includes("complete") || value.includes("paid") || value.includes("ready")) {
                return "sd-status sd-status-ready";
            }
            if (value.includes("assign") || value.includes("progress") || value.includes("diagnostic")) {
                return "sd-status sd-status-progress";
            }
            return "sd-status sd-status-warning";
        }

        function renderLiveManagerOverview() {
            const overview = document.getElementById("overview");
            const store = window.ShiftDynamicsStore;
            if (!overview || !store || typeof store.getJobs !== "function") return;

            const jobs = store.getJobs();
            const quotes = typeof store.getVendorQuotes === "function"
                ? store.getVendorQuotes()
                : [];
            const orders = typeof store.getPurchaseOrders === "function"
                ? store.getPurchaseOrders()
                : [];
            const activeJobs = jobs.filter(job =>
                String(job?.status || job?.handover?.status || "").toLowerCase() !== "completed"
            );
            const awaitingAssignment = activeJobs.filter(job =>
                !job?.assignment?.mechanicName
            );
            const assignedMechanics = new Set(
                activeJobs
                    .map(job => job?.assignment?.mechanicName)
                    .filter(Boolean)
            );
            const pendingQuotes = quotes.filter(quote =>
                !["manager approved", "purchase order approved"].includes(
                    String(quote?.status || "").toLowerCase()
                )
            );
            const pendingOrders = orders.filter(order =>
                String(order?.status || "").toLowerCase() === "pending approval"
            );
            const pendingBilling = orders.filter(order =>
                String(order?.status || "").toLowerCase() === "approved" &&
                String(order?.billingStatus || "").toLowerCase() !== "approved"
            );
            const pendingApprovalCount =
                pendingQuotes.length + pendingOrders.length + pendingBilling.length;

            const statCards = overview.querySelectorAll(".sd-stat-card");
            const setStat = (index, label, value, detail) => {
                const card = statCards[index];
                if (!card) return;
                const labelNode = card.querySelector("span");
                const valueNode = card.querySelector("strong");
                const detailNode = card.querySelector("small");
                if (labelNode) labelNode.textContent = label;
                if (valueNode) valueNode.textContent = String(value).padStart(2, "0");
                if (detailNode) detailNode.textContent = detail;
            };
            setStat(0, "Active Job Cards", activeJobs.length, `${awaitingAssignment.length} awaiting assignment`);
            setStat(1, "Assigned Mechanics", assignedMechanics.size, `${activeJobs.length - awaitingAssignment.length} active assigned jobs`);
            setStat(2, "Vendor Quotes", pendingQuotes.length, "Pending manager comparison");
            setStat(3, "Pending Approvals", pendingApprovalCount, "Quotes, orders and billing");

            const bayList = overview.querySelector(".sd-bay-list");
            if (bayList) {
                const bays = ["Bay 01", "Bay 02", "Bay 03", "Bay 04"];
                bayList.innerHTML = bays.map(bay => {
                    const normalizedBay = bay.replace(/\D/g, "");
                    const job = activeJobs.find(item =>
                        String(item?.assignment?.bay || "").replace(/\D/g, "") === normalizedBay
                    );
                    const status = job ? overviewJobStatus(job) : "Open";
                    const progress = job ? 70 : 0;
                    return `<div class="sd-bay-item"><div class="sd-bay-info"><strong>${bay}</strong><span>${job ? overviewEscape(overviewVehicle(job)) : "Available"}</span></div><div class="sd-progress"><span style="width:${progress}%"></span></div><span class="${job ? overviewStatusClass(status) : "sd-status sd-status-open"}">${overviewEscape(status)}</span></div>`;
                }).join("");
            }

            const approvalPanel = [...overview.querySelectorAll(".sd-panel")]
                .find(panel => panel.querySelector("h3")?.textContent.trim() === "Approval Queue");
            const approvalBadge = approvalPanel?.querySelector(".sd-count-badge");
            const approvalList = approvalPanel?.querySelector(".sd-approval-list");
            if (approvalBadge) approvalBadge.textContent = `${pendingApprovalCount} Pending`;
            if (approvalList) {
                const items = [];
                pendingOrders.forEach(order => items.push({
                    go: "orders",
                    icon: "bi-box-seam",
                    title: "Vendor Parts Order",
                    detail: `#${overviewIdentifier(order.purchaseOrderId)} - ${order.part || "Vendor Part"}`
                }));
                pendingQuotes.forEach(quote => items.push({
                    go: "vendors",
                    icon: "bi-shop",
                    title: "Vendor Bid Review",
                    detail: `#${overviewIdentifier(quote.quoteId)} - ${quote.part || "Quotation"}`
                }));
                pendingBilling.forEach(order => items.push({
                    go: "billing",
                    icon: "bi-receipt",
                    title: "Final Billing Review",
                    detail: `#${overviewIdentifier(order.invoiceNumber || order.jobCardNumber)}`
                }));
                approvalList.innerHTML = items.length
                    ? items.slice(0, 5).map(item => `<button type="button" class="sd-approval-item" data-live-go="${item.go}"><div class="sd-approval-icon"><i class="bi ${item.icon}"></i></div><div><strong>${overviewEscape(item.title)}</strong><span>${overviewEscape(item.detail)}</span></div><i class="bi bi-chevron-right"></i></button>`).join("")
                    : '<div class="sd-empty-state">No pending manager approvals.</div>';
                approvalList.querySelectorAll("[data-live-go]").forEach(button => {
                    button.addEventListener("click", () => openSection(button.dataset.liveGo));
                });
            }

            const recentPanel = [...overview.querySelectorAll(".sd-panel")]
                .find(panel => panel.querySelector("h3")?.textContent.trim() === "Recent Job Cards");
            const recentBody = recentPanel?.querySelector("tbody");
            if (recentBody) {
                const recentJobs = jobs.slice().sort((a, b) =>
                    new Date(b?.updatedAt || b?.createdAt || 0) -
                    new Date(a?.updatedAt || a?.createdAt || 0)
                ).slice(0, 5);
                recentBody.innerHTML = recentJobs.length
                    ? recentJobs.map(job => {
                        const status = overviewJobStatus(job);
                        return `<tr><td><strong>#${overviewEscape(overviewIdentifier(job.jobCardNumber))}</strong></td><td>${overviewEscape(overviewVehicle(job))}</td><td>${overviewEscape(job.serviceConcern || job.service || "Service Job")}</td><td><span class="${overviewStatusClass(status)}">${overviewEscape(status)}</span></td></tr>`;
                    }).join("")
                    : '<tr><td colspan="4">No live job cards available.</td></tr>';
            }
        }

        renderLiveManagerOverview();

        if (
            window.ShiftDynamicsStore &&
            typeof window.ShiftDynamicsStore.subscribe === "function"
        ) {
            window.ShiftDynamicsStore.subscribe(renderLiveManagerOverview);
        }

        /* =================================================
           LIVE WORKSHOP LOAD - SHARED WORKFLOW V8
           ================================================= */

        function workshopProgress(job) {
            if (job?.invoice?.status === "Finalized") return 90;
            if (job?.estimate?.status === "Approved") return 70;
            if (job?.diagnostic) return 45;
            if (job?.assignment?.mechanicName) return 25;
            return 10;
        }

        function renderLiveWorkshopLoad() {
            const section = document.getElementById("workshop-load");
            const store = window.ShiftDynamicsStore;
            if (!section || !store || typeof store.getJobs !== "function") return;

            const jobs = store.getJobs();
            const activeAssigned = jobs.filter(job =>
                String(job?.status || job?.handover?.status || "").toLowerCase() !== "completed" &&
                Boolean(job?.assignment?.bay)
            );
            const bays = ["Bay 01", "Bay 02", "Bay 03", "Bay 04"];
            const occupied = new Map();
            activeAssigned.forEach(job => {
                const digits = String(job.assignment.bay || "").replace(/\D/g, "");
                const bay = bays.find(item => item.replace(/\D/g, "") === digits);
                if (bay && !occupied.has(bay)) occupied.set(bay, job);
            });

            const occupiedCount = occupied.size;
            const availableCount = bays.length - occupiedCount;
            const utilization = Math.round((occupiedCount / bays.length) * 100);
            const stats = section.querySelectorAll(".sd-workshop-stat");
            const setWorkshopStat = (index, value, detail) => {
                const card = stats[index];
                if (!card) return;
                const strong = card.querySelector("strong");
                const small = card.querySelector("small");
                if (strong) strong.textContent = value;
                if (small) small.textContent = detail;
            };
            setWorkshopStat(0, `${utilization}%`, `${occupiedCount} of ${bays.length} bays occupied`);
            setWorkshopStat(1, occupiedCount, "Active assigned jobs");
            setWorkshopStat(2, availableCount, "Ready for assignment");
            setWorkshopStat(3, 0, "No maintenance data recorded");

            const utilizationPanel = section.querySelector(".sd-workshop-utilization");
            const utilizationValue = utilizationPanel?.querySelector(".sd-workshop-utilization-head > strong");
            const utilizationBar = utilizationPanel?.querySelector(".sd-workshop-main-progress span");
            const capacityLabels = utilizationPanel?.querySelector(".sd-workshop-capacity-labels");
            if (utilizationValue) utilizationValue.textContent = `${utilization}%`;
            if (utilizationBar) utilizationBar.style.width = `${utilization}%`;
            if (capacityLabels) {
                capacityLabels.innerHTML = `<span>${occupiedCount} Occupied</span><span>${availableCount} Available</span><span>0 Maintenance</span>`;
            }

            const bayGrid = section.querySelector(".sd-workshop-bay-grid");
            if (bayGrid) {
                bayGrid.innerHTML = bays.map(bay => {
                    const job = occupied.get(bay);
                    if (!job) {
                        return `<article class="sd-workshop-bay-card"><div class="sd-workshop-bay-head"><div><span>WORKSHOP BAY</span><h3>${bay}</h3></div><span class="sd-status sd-status-ready">Available</span></div><div class="sd-workshop-empty-bay"><div class="sd-workshop-empty-icon"><i class="bi bi-check2-circle"></i></div><strong>Bay Ready</strong><p>No active job is currently assigned to this bay.</p><button type="button" class="sd-secondary-btn" data-live-workshop-go="job-cards"><i class="bi bi-plus-circle"></i> Assign Job</button></div></article>`;
                    }
                    const progress = workshopProgress(job);
                    const assignedAt = job.assignment?.assignedAt
                        ? new Date(job.assignment.assignedAt).toLocaleTimeString("en-LK", { hour: "2-digit", minute: "2-digit" })
                        : "--";
                    return `<article class="sd-workshop-bay-card"><div class="sd-workshop-bay-head"><div><span>WORKSHOP BAY</span><h3>${bay}</h3></div><span class="sd-status sd-status-progress">Occupied</span></div><div class="sd-workshop-job-number">#${overviewEscape(overviewIdentifier(job.jobCardNumber))}</div><div class="sd-workshop-bay-vehicle"><div class="sd-workshop-car-icon"><i class="bi bi-car-front"></i></div><div><strong>${overviewEscape(overviewVehicle(job))}</strong><span>${overviewEscape(job.vehicle?.plate || "--")}</span></div></div><div class="sd-workshop-bay-details"><div><span>Service</span><strong>${overviewEscape(job.serviceConcern || "Service Job")}</strong></div><div><span>Mechanic</span><strong>${overviewEscape(job.assignment?.mechanicName || "Unassigned")}</strong></div><div><span>Assigned</span><strong>${overviewEscape(assignedAt)}</strong></div><div><span>Status</span><strong>${overviewEscape(overviewJobStatus(job))}</strong></div></div><div class="sd-workshop-job-progress-head"><span>Workflow Progress</span><strong>${progress}%</strong></div><div class="sd-workshop-job-progress"><span style="width:${progress}%"></span></div></article>`;
                }).join("");
                bayGrid.querySelectorAll("[data-live-workshop-go]").forEach(button => {
                    button.addEventListener("click", () => openSection(button.dataset.liveWorkshopGo));
                });
            }

            const bottomPanels = section.querySelectorAll(".sd-workshop-bottom-grid .sd-panel");
            const jobsPanel = bottomPanels[0];
            const insightPanel = bottomPanels[1];
            const countBadge = jobsPanel?.querySelector(".sd-count-badge");
            const jobsBody = jobsPanel?.querySelector("tbody");
            if (countBadge) countBadge.textContent = `${activeAssigned.length} Active`;
            if (jobsBody) {
                jobsBody.innerHTML = activeAssigned.length
                    ? activeAssigned.map(job => `<tr><td><strong>#${overviewEscape(overviewIdentifier(job.jobCardNumber))}</strong></td><td>${overviewEscape(overviewVehicle(job))}</td><td>${overviewEscape(job.assignment?.mechanicName || "Unassigned")}</td><td>${overviewEscape(job.assignment?.bay || "--")}</td><td><span class="sd-status sd-status-progress">${workshopProgress(job)}%</span></td></tr>`).join("")
                    : '<tr><td colspan="5">No active workshop jobs.</td></tr>';
            }
            const insights = insightPanel?.querySelector(".sd-workshop-insights");
            if (insights) {
                insights.innerHTML = `<div class="sd-workshop-insight good"><i class="bi bi-check-circle"></i><div><strong>${availableCount} Bays Available</strong><p>${availableCount ? "Workshop capacity is available for new assignments." : "All workshop bays are occupied."}</p></div></div><div class="sd-workshop-insight ${utilization >= 75 ? "warning" : ""}"><i class="bi bi-speedometer2"></i><div><strong>Capacity at ${utilization}%</strong><p>Calculated from actual active bay assignments.</p></div></div><div class="sd-workshop-insight"><i class="bi bi-person-gear"></i><div><strong>${activeAssigned.length} Active Assignments</strong><p>Only live, non-completed shared jobs are included.</p></div></div>`;
            }
        }

        renderLiveWorkshopLoad();

        if (
            window.ShiftDynamicsStore &&
            typeof window.ShiftDynamicsStore.subscribe === "function"
        ) {
            window.ShiftDynamicsStore.subscribe(renderLiveWorkshopLoad);
        }

        /* =================================================
           MANAGER BUSINESS ANALYTICS
           Frontend demo data only.

           Production:
           .NET API -> Database -> Analytics data
           ================================================= */

        const analyticsPeriod =
            document.getElementById(
                "analyticsPeriod"
            );

        const analyticsRevenue =
            document.getElementById(
                "analyticsRevenue"
            );

        const analyticsJobs =
            document.getElementById(
                "analyticsJobs"
            );

        const analyticsUtilization =
            document.getElementById(
                "analyticsUtilization"
            );

        const analyticsVendor =
            document.getElementById(
                "analyticsVendor"
            );

        let revenueChart = null;
        let serviceChart = null;
        let mechanicChart = null;
        let vendorChart = null;

        const analyticsData = {

            7: {

                revenue:
                    486000,

                jobs:
                    24,

                utilization:
                    72,

                vendor:
                    124000,

                revenueLabels: [
                    "Mon",
                    "Tue",
                    "Wed",
                    "Thu",
                    "Fri",
                    "Sat",
                    "Sun"
                ],

                revenueValues: [
                    54000,
                    68000,
                    61000,
                    72000,
                    83000,
                    91000,
                    57000
                ],

                services: [
                    8,
                    5,
                    4,
                    4,
                    3
                ],

                mechanics: [
                    7,
                    6,
                    5,
                    4
                ],

                vendors: [
                    46000,
                    33000,
                    25000,
                    20000
                ]
            },

            30: {

                revenue:
                    1840000,

                jobs:
                    86,

                utilization:
                    78,

                vendor:
                    482000,

                revenueLabels: [
                    "Week 1",
                    "Week 2",
                    "Week 3",
                    "Week 4"
                ],

                revenueValues: [
                    398000,
                    438000,
                    471000,
                    533000
                ],

                services: [
                    28,
                    19,
                    16,
                    13,
                    10
                ],

                mechanics: [
                    25,
                    23,
                    20,
                    18
                ],

                vendors: [
                    168000,
                    132000,
                    104000,
                    78000
                ]
            },

            90: {

                revenue:
                    5180000,

                jobs:
                    241,

                utilization:
                    81,

                vendor:
                    1365000,

                revenueLabels: [
                    "Month 1",
                    "Month 2",
                    "Month 3"
                ],

                revenueValues: [
                    1580000,
                    1690000,
                    1910000
                ],

                services: [
                    78,
                    56,
                    43,
                    37,
                    27
                ],

                mechanics: [
                    68,
                    63,
                    58,
                    52
                ],

                vendors: [
                    475000,
                    364000,
                    298000,
                    228000
                ]
            }
        };

        function formatAnalyticsMoney(
            amount
        ) {

            if (amount >= 1000000) {

                return (
                    "LKR " +
                    (
                        amount /
                        1000000
                    ).toFixed(2) +
                    "M"
                );
            }

            if (amount >= 1000) {

                return (
                    "LKR " +
                    Math.round(
                        amount /
                        1000
                    ) +
                    "K"
                );
            }

            return (
                "LKR " +
                amount.toLocaleString(
                    "en-LK"
                )
            );
        }

        function destroyAnalyticsCharts() {

            revenueChart?.destroy();
            serviceChart?.destroy();
            mechanicChart?.destroy();
            vendorChart?.destroy();
        }

        function createAnalyticsCharts(
            period = "30"
        ) {

            if (
                typeof Chart ===
                "undefined"
            ) {

                console.warn(
                    "Chart.js is not available."
                );

                return;
            }

            const data =
                analyticsData[period] ||
                analyticsData[30];

            destroyAnalyticsCharts();

            Chart.defaults.font.family =
                'Inter, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';

            Chart.defaults.color =
                "#64748B";

            /* -----------------------------------------
               REVENUE LINE CHART
               ----------------------------------------- */

            const revenueCanvas =
                document.getElementById(
                    "revenueChart"
                );

            if (revenueCanvas) {

                revenueChart =
                    new Chart(
                        revenueCanvas,
                        {

                            type: "line",

                            data: {

                                labels:
                                    data
                                        .revenueLabels,

                                datasets: [
                                    {

                                        label:
                                            "Revenue (LKR)",

                                        data:
                                            data
                                                .revenueValues,

                                        borderColor:
                                            "#F97316",

                                        backgroundColor:
                                            "rgba(249, 115, 22, 0.10)",

                                        fill:
                                            true,

                                        tension:
                                            0.38,

                                        pointRadius:
                                            4,

                                        pointHoverRadius:
                                            6,

                                        pointBackgroundColor:
                                            "#F97316",

                                        borderWidth:
                                            3
                                    }
                                ]
                            },

                            options: {

                                responsive:
                                    true,

                                maintainAspectRatio:
                                    false,

                                interaction: {

                                    intersect:
                                        false,

                                    mode:
                                        "index"
                                },

                                plugins: {

                                    legend: {
                                        display:
                                            false
                                    },

                                    tooltip: {

                                        callbacks: {

                                            label:
                                                context => {

                                                    return (
                                                        " Revenue: LKR " +
                                                        Number(
                                                            context.raw
                                                        ).toLocaleString(
                                                            "en-LK"
                                                        )
                                                    );
                                                }
                                        }
                                    }
                                },

                                scales: {

                                    x: {

                                        grid: {
                                            display:
                                                false
                                        }
                                    },

                                    y: {

                                        beginAtZero:
                                            true,

                                        ticks: {

                                            callback:
                                                value => {

                                                    return (
                                                        "LKR " +
                                                        Math.round(
                                                            value /
                                                            1000
                                                        ) +
                                                        "K"
                                                    );
                                                }
                                        },

                                        grid: {

                                            color:
                                                "rgba(148, 163, 184, 0.15)"
                                        }
                                    }
                                }
                            }
                        }
                    );
            }

            /* -----------------------------------------
               SERVICE BAR CHART
               ----------------------------------------- */

            const serviceCanvas =
                document.getElementById(
                    "serviceChart"
                );

            if (serviceCanvas) {

                serviceChart =
                    new Chart(
                        serviceCanvas,
                        {

                            type:
                                "bar",

                            data: {

                                labels: [
                                    "General Service",
                                    "Diagnostics",
                                    "Brakes",
                                    "Engine",
                                    "Suspension"
                                ],

                                datasets: [
                                    {

                                        label:
                                            "Completed Jobs",

                                        data:
                                            data.services,

                                        backgroundColor: [
                                            "#F97316",
                                            "#FB923C",
                                            "#FDBA74",
                                            "#0B132B",
                                            "#1E2A48"
                                        ],

                                        borderRadius:
                                            7,

                                        borderSkipped:
                                            false
                                    }
                                ]
                            },

                            options: {

                                responsive:
                                    true,

                                maintainAspectRatio:
                                    false,

                                plugins: {

                                    legend: {
                                        display:
                                            false
                                    }
                                },

                                scales: {

                                    x: {

                                        grid: {
                                            display:
                                                false
                                        }
                                    },

                                    y: {

                                        beginAtZero:
                                            true,

                                        ticks: {
                                            precision:
                                                0
                                        },

                                        grid: {

                                            color:
                                                "rgba(148, 163, 184, 0.15)"
                                        }
                                    }
                                }
                            }
                        }
                    );
            }

            /* -----------------------------------------
               MECHANIC HORIZONTAL BAR
               ----------------------------------------- */

            const mechanicCanvas =
                document.getElementById(
                    "mechanicChart"
                );

            if (mechanicCanvas) {

                mechanicChart =
                    new Chart(
                        mechanicCanvas,
                        {

                            type:
                                "bar",

                            data: {

                                labels: [
                                    "Nimal Perera",
                                    "Kasun Silva",
                                    "Ruwan Fernando",
                                    "Dilan Jayasinghe"
                                ],

                                datasets: [
                                    {

                                        label:
                                            "Completed Jobs",

                                        data:
                                            data.mechanics,

                                        backgroundColor:
                                            "#F97316",

                                        borderRadius:
                                            7,

                                        borderSkipped:
                                            false
                                    }
                                ]
                            },

                            options: {

                                indexAxis:
                                    "y",

                                responsive:
                                    true,

                                maintainAspectRatio:
                                    false,

                                plugins: {

                                    legend: {
                                        display:
                                            false
                                    }
                                },

                                scales: {

                                    x: {

                                        beginAtZero:
                                            true,

                                        ticks: {
                                            precision:
                                                0
                                        },

                                        grid: {

                                            color:
                                                "rgba(148, 163, 184, 0.15)"
                                        }
                                    },

                                    y: {

                                        grid: {
                                            display:
                                                false
                                        }
                                    }
                                }
                            }
                        }
                    );
            }

            /* -----------------------------------------
               VENDOR DOUGHNUT
               ----------------------------------------- */

            const vendorCanvas =
                document.getElementById(
                    "vendorChart"
                );

            if (vendorCanvas) {

                vendorChart =
                    new Chart(
                        vendorCanvas,
                        {

                            type:
                                "doughnut",

                            data: {

                                labels: [
                                    "AutoParts Lanka",
                                    "MotorHub Spares",
                                    "Prime Auto Traders",
                                    "Other Vendors"
                                ],

                                datasets: [
                                    {

                                        data:
                                            data.vendors,

                                        backgroundColor: [
                                            "#F97316",
                                            "#FB923C",
                                            "#0B132B",
                                            "#94A3B8"
                                        ],

                                        borderWidth:
                                            0,

                                        hoverOffset:
                                            6
                                    }
                                ]
                            },

                            options: {

                                responsive:
                                    true,

                                maintainAspectRatio:
                                    false,

                                cutout:
                                    "66%",

                                plugins: {

                                    legend: {

                                        position:
                                            "bottom",

                                        labels: {

                                            usePointStyle:
                                                true,

                                            boxWidth:
                                                8,

                                            padding:
                                                14,

                                            font: {
                                                size:
                                                    10
                                            }
                                        }
                                    },

                                    tooltip: {

                                        callbacks: {

                                            label:
                                                context => {

                                                    return (
                                                        " " +
                                                        context.label +
                                                        ": LKR " +
                                                        Number(
                                                            context.raw
                                                        ).toLocaleString(
                                                            "en-LK"
                                                        )
                                                    );
                                                }
                                        }
                                    }
                                }
                            }
                        }
                    );
            }
        }

        function updateAnalyticsDashboard(
            period
        ) {

            const data =
                analyticsData[period] ||
                analyticsData[30];

            if (analyticsRevenue) {

                analyticsRevenue.textContent =
                    formatAnalyticsMoney(
                        data.revenue
                    );
            }

            if (analyticsJobs) {

                analyticsJobs.textContent =
                    data.jobs;
            }

            if (analyticsUtilization) {

                analyticsUtilization.textContent =
                    `${data.utilization}%`;
            }

            if (analyticsVendor) {

                analyticsVendor.textContent =
                    formatAnalyticsMoney(
                        data.vendor
                    );
            }

            createAnalyticsCharts(
                period
            );
        }

        analyticsPeriod?.addEventListener(
            "change",
            () => {

                updateAnalyticsDashboard(
                    analyticsPeriod.value
                );
            }
        );

        /* Initial analytics render */

        updateAnalyticsDashboard(
            analyticsPeriod?.value ||
            "30"
        );

        /* =================================================
           LIVE MANAGER ANALYTICS - SHARED WORKFLOW V6
           ================================================= */

        function analyticsNumber(value) {
            const number = Number(value);
            return Number.isFinite(number) ? number : 0;
        }

        function analyticsDate(...values) {
            for (const value of values) {
                if (!value) continue;
                const date = new Date(value);
                if (!Number.isNaN(date.getTime())) return date;
            }
            return null;
        }

        function analyticsInRange(date, start, end) {
            return Boolean(date && date >= start && date < end);
        }

        function analyticsJobDate(job) {
            return analyticsDate(
                job?.handover?.completedAt,
                job?.invoice?.paidAt,
                job?.invoice?.finalizedAt,
                job?.updatedAt,
                job?.createdAt
            );
        }

        function analyticsServiceName(job) {
            const text = String(
                job?.serviceConcern ||
                job?.service ||
                job?.diagnostic?.recommendedWork ||
                "Other Service"
            ).trim();
            return text || "Other Service";
        }

        function analyticsGroupTop(items, labelGetter, valueGetter, limit = 5) {
            const totals = new Map();
            items.forEach(item => {
                const label = String(labelGetter(item) || "Other").trim() || "Other";
                totals.set(label, (totals.get(label) || 0) + analyticsNumber(valueGetter(item)));
            });
            const sorted = [...totals.entries()].sort((a, b) => b[1] - a[1]);
            if (!sorted.length) return { labels: ["No live data"], values: [0] };
            const visible = sorted.slice(0, limit);
            if (sorted.length > limit) {
                visible.push(["Other", sorted.slice(limit).reduce((sum, entry) => sum + entry[1], 0)]);
            }
            return {
                labels: visible.map(entry => entry[0]),
                values: visible.map(entry => entry[1])
            };
        }

        function analyticsPercentChange(current, previous) {
            if (!previous) return current ? 100 : 0;
            return ((current - previous) / previous) * 100;
        }

        function analyticsMetricStatus(change, inverse = false) {
            const good = inverse ? change <= 0 : change >= 0;
            return {
                text: good ? "Good" : "Monitor",
                className: good
                    ? "sd-status sd-status-ready"
                    : "sd-status sd-status-warning"
            };
        }

        function buildLiveAnalytics(periodValue = "30") {
            const days = Math.max(1, analyticsNumber(periodValue) || 30);
            const now = new Date();
            const end = new Date(now.getTime() + 1);
            const start = new Date(now);
            start.setHours(0, 0, 0, 0);
            start.setDate(start.getDate() - (days - 1));
            const previousStart = new Date(start);
            previousStart.setDate(previousStart.getDate() - days);

            const store = window.ShiftDynamicsStore;
            const jobs = store && typeof store.getJobs === "function"
                ? store.getJobs()
                : [];
            const orders = store && typeof store.getPurchaseOrders === "function"
                ? store.getPurchaseOrders()
                : [];

            const currentJobs = jobs.filter(job =>
                analyticsInRange(analyticsJobDate(job), start, end)
            );
            const previousJobs = jobs.filter(job =>
                analyticsInRange(analyticsJobDate(job), previousStart, start)
            );
            const completedJobs = currentJobs.filter(job =>
                String(job?.status || job?.handover?.status || "").toLowerCase() === "completed"
            );
            const previousCompletedJobs = previousJobs.filter(job =>
                String(job?.status || job?.handover?.status || "").toLowerCase() === "completed"
            );
            const invoiceJobs = currentJobs.filter(job =>
                job?.invoice &&
                ["finalized", "paid"].includes(String(job.invoice.status || "").toLowerCase())
            );
            const previousInvoiceJobs = previousJobs.filter(job =>
                job?.invoice &&
                ["finalized", "paid"].includes(String(job.invoice.status || "").toLowerCase())
            );
            const currentOrders = orders.filter(order =>
                String(order?.status || "").toLowerCase() === "approved" &&
                analyticsInRange(
                    analyticsDate(order?.approvedAt, order?.updatedAt, order?.createdAt),
                    start,
                    end
                )
            );
            const previousOrders = orders.filter(order =>
                String(order?.status || "").toLowerCase() === "approved" &&
                analyticsInRange(
                    analyticsDate(order?.approvedAt, order?.updatedAt, order?.createdAt),
                    previousStart,
                    start
                )
            );

            const orderTotal = order =>
                analyticsNumber(order?.total) ||
                analyticsNumber(order?.totalAmount) ||
                analyticsNumber(order?.quantity) * analyticsNumber(order?.unitPrice);
            const revenue = invoiceJobs.reduce((sum, job) =>
                sum + analyticsNumber(job?.invoice?.total), 0
            );
            const previousRevenue = previousInvoiceJobs.reduce((sum, job) =>
                sum + analyticsNumber(job?.invoice?.total), 0
            );
            const vendor = currentOrders.reduce((sum, order) => sum + orderTotal(order), 0);
            const previousVendor = previousOrders.reduce((sum, order) => sum + orderTotal(order), 0);

            const activeJobs = currentJobs.filter(job =>
                String(job?.status || "").toLowerCase() !== "completed" &&
                Boolean(job?.assignment?.mechanicName || job?.assignment?.bay)
            );
            const usedBays = new Set(
                activeJobs.map(job => job?.assignment?.bay).filter(Boolean)
            ).size;
            const utilization = Math.min(100, Math.round((usedBays / 4) * 100));

            const labels = [];
            const revenueValues = [];
            if (days <= 7) {
                for (let offset = 0; offset < days; offset += 1) {
                    const bucketStart = new Date(start);
                    bucketStart.setDate(start.getDate() + offset);
                    const bucketEnd = new Date(bucketStart);
                    bucketEnd.setDate(bucketEnd.getDate() + 1);
                    labels.push(bucketStart.toLocaleDateString("en-LK", { weekday: "short" }));
                    revenueValues.push(invoiceJobs.reduce((sum, job) =>
                        sum + (analyticsInRange(analyticsJobDate(job), bucketStart, bucketEnd)
                            ? analyticsNumber(job?.invoice?.total)
                            : 0), 0));
                }
            } else {
                const bucketCount = days <= 30 ? 4 : 3;
                const bucketDays = Math.ceil(days / bucketCount);
                for (let bucket = 0; bucket < bucketCount; bucket += 1) {
                    const bucketStart = new Date(start);
                    bucketStart.setDate(start.getDate() + (bucket * bucketDays));
                    const bucketEnd = new Date(bucketStart);
                    bucketEnd.setDate(bucketEnd.getDate() + bucketDays);
                    if (bucketEnd > end) bucketEnd.setTime(end.getTime());
                    labels.push(days <= 30 ? `Week ${bucket + 1}` : `Period ${bucket + 1}`);
                    revenueValues.push(invoiceJobs.reduce((sum, job) =>
                        sum + (analyticsInRange(analyticsJobDate(job), bucketStart, bucketEnd)
                            ? analyticsNumber(job?.invoice?.total)
                            : 0), 0));
                }
            }

            const services = analyticsGroupTop(
                completedJobs,
                analyticsServiceName,
                () => 1
            );
            const mechanics = analyticsGroupTop(
                completedJobs,
                job => job?.assignment?.mechanicName || "Unassigned",
                () => 1,
                4
            );
            const vendors = analyticsGroupTop(
                currentOrders,
                order => order?.vendorName || "Registered Vendor",
                orderTotal,
                4
            );

            return {
                days,
                jobs: currentJobs,
                completedJobs,
                revenue,
                previousRevenue,
                vendor,
                previousVendor,
                utilization,
                previousCompletedCount: previousCompletedJobs.length,
                revenueLabels: labels,
                revenueValues,
                services,
                mechanics,
                vendors
            };
        }

        function renderLiveAnalyticsCharts(data) {
            if (typeof Chart === "undefined") return;
            destroyAnalyticsCharts();
            const commonScale = {
                beginAtZero: true,
                ticks: { precision: 0 },
                grid: { color: "rgba(148, 163, 184, 0.15)" }
            };
            const revenueCanvas = document.getElementById("revenueChart");
            if (revenueCanvas) {
                revenueChart = new Chart(revenueCanvas, {
                    type: "line",
                    data: {
                        labels: data.revenueLabels,
                        datasets: [{ label: "Revenue (LKR)", data: data.revenueValues, borderColor: "#F97316", backgroundColor: "rgba(249,115,22,.10)", fill: true, tension: .38, borderWidth: 3 }]
                    },
                    options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { x: { grid: { display: false } }, y: commonScale } }
                });
            }
            const serviceCanvas = document.getElementById("serviceChart");
            if (serviceCanvas) {
                serviceChart = new Chart(serviceCanvas, {
                    type: "bar",
                    data: { labels: data.services.labels, datasets: [{ label: "Completed Jobs", data: data.services.values, backgroundColor: "#F97316", borderRadius: 7 }] },
                    options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { x: { grid: { display: false } }, y: commonScale } }
                });
            }
            const mechanicCanvas = document.getElementById("mechanicChart");
            if (mechanicCanvas) {
                mechanicChart = new Chart(mechanicCanvas, {
                    type: "bar",
                    data: { labels: data.mechanics.labels, datasets: [{ label: "Completed Jobs", data: data.mechanics.values, backgroundColor: "#F97316", borderRadius: 7 }] },
                    options: { indexAxis: "y", responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { x: commonScale, y: { grid: { display: false } } } }
                });
            }
            const vendorCanvas = document.getElementById("vendorChart");
            if (vendorCanvas) {
                vendorChart = new Chart(vendorCanvas, {
                    type: "doughnut",
                    data: { labels: data.vendors.labels, datasets: [{ data: data.vendors.values, backgroundColor: ["#F97316", "#FB923C", "#0B132B", "#94A3B8", "#CBD5E1"], borderWidth: 0 }] },
                    options: { responsive: true, maintainAspectRatio: false, cutout: "66%", plugins: { legend: { position: "bottom", labels: { usePointStyle: true, boxWidth: 8, padding: 14, font: { size: 10 } } } } }
                });
            }
        }

        function renderLiveAnalyticsDetails(data) {
            const analyticsSection = document.getElementById("analytics");
            if (!analyticsSection) return;
            const bayList = analyticsSection.querySelector(".sd-bay-performance-list");
            const insightList = analyticsSection.querySelector(".sd-insight-list");
            const metricBody = analyticsSection.querySelector(".sd-table-wrap tbody");
            const bayCounts = new Map();
            data.jobs.forEach(job => {
                const bay = String(job?.assignment?.bay || "Unassigned");
                bayCounts.set(bay, (bayCounts.get(bay) || 0) + 1);
            });
            if (bayList) {
                const bays = ["Bay 01", "Bay 02", "Bay 03", "Bay 04"];
                bayList.innerHTML = bays.map(bay => {
                    const count = bayCounts.get(bay) || 0;
                    const percent = Math.min(100, Math.round((count / Math.max(1, data.jobs.length)) * 100));
                    return `<div class="sd-bay-performance"><div class="sd-bay-performance-head"><div><strong>${bay}</strong><span>${count} live job${count === 1 ? "" : "s"}</span></div><strong>${percent}%</strong></div><div class="sd-performance-bar"><span style="width:${percent}%"></span></div></div>`;
                }).join("");
            }
            const revenueChange = analyticsPercentChange(data.revenue, data.previousRevenue);
            const jobChange = analyticsPercentChange(data.completedJobs.length, data.previousCompletedCount);
            const vendorShare = data.revenue ? Math.round((data.vendor / data.revenue) * 100) : 0;
            const topMechanic = data.mechanics.values[0] > 0 ? data.mechanics.labels[0] : "No completed jobs";
            if (insightList) {
                insightList.innerHTML = `
                    <div class="sd-insight-item"><div class="sd-insight-icon"><i class="bi bi-graph-up-arrow"></i></div><div><strong>Live revenue</strong><p>${formatAnalyticsMoney(data.revenue)} recorded in the selected ${data.days}-day period (${revenueChange >= 0 ? "+" : ""}${revenueChange.toFixed(1)}%).</p></div></div>
                    <div class="sd-insight-item"><div class="sd-insight-icon"><i class="bi bi-speedometer2"></i></div><div><strong>Workshop utilization</strong><p>${data.utilization}% of the four workshop bays are currently assigned.</p></div></div>
                    <div class="sd-insight-item"><div class="sd-insight-icon"><i class="bi bi-person-check"></i></div><div><strong>Top mechanic</strong><p>${topMechanic} has the highest completed-job count in this period.</p></div></div>
                    <div class="sd-insight-item"><div class="sd-insight-icon"><i class="bi bi-box-seam"></i></div><div><strong>Vendor cost share</strong><p>Approved vendor spending represents ${vendorShare}% of recorded revenue.</p></div></div>`;
            }
            if (metricBody) {
                const rows = [
                    ["Revenue", formatAnalyticsMoney(data.revenue), formatAnalyticsMoney(data.previousRevenue), revenueChange, false],
                    ["Completed Jobs", data.completedJobs.length, data.previousCompletedCount, jobChange, false],
                    ["Bay Utilization", `${data.utilization}%`, "Live bays", 0, false],
                    ["Vendor Spending", formatAnalyticsMoney(data.vendor), formatAnalyticsMoney(data.previousVendor), analyticsPercentChange(data.vendor, data.previousVendor), true]
                ];
                metricBody.innerHTML = rows.map(([label, current, previous, change, inverse]) => {
                    const status = analyticsMetricStatus(change, inverse);
                    return `<tr><td><strong>${label}</strong></td><td>${current}</td><td>${previous}</td><td class="${change >= 0 ? "sd-table-positive" : "sd-table-warning"}">${change >= 0 ? "+" : ""}${Number(change).toFixed(1)}%</td><td><span class="${status.className}">${status.text}</span></td></tr>`;
                }).join("");
            }
        }

        function updateLiveAnalyticsDashboard() {
            const data = buildLiveAnalytics(analyticsPeriod?.value || "30");
            if (analyticsRevenue) analyticsRevenue.textContent = formatAnalyticsMoney(data.revenue);
            if (analyticsJobs) analyticsJobs.textContent = data.completedJobs.length;
            if (analyticsUtilization) analyticsUtilization.textContent = `${data.utilization}%`;
            if (analyticsVendor) analyticsVendor.textContent = formatAnalyticsMoney(data.vendor);
            renderLiveAnalyticsCharts(data);
            renderLiveAnalyticsDetails(data);
        }

        analyticsPeriod?.addEventListener("change", updateLiveAnalyticsDashboard);
        updateLiveAnalyticsDashboard();

        if (
            window.ShiftDynamicsStore &&
            typeof window.ShiftDynamicsStore.subscribe === "function"
        ) {
            window.ShiftDynamicsStore.subscribe(updateLiveAnalyticsDashboard);
        }

        /* =================================================
           MANAGER USER MANAGEMENT

           Frontend demo only.
           Production user accounts, roles and account
           status must be controlled by the .NET backend.
           ================================================= */

        const userTableBody =
            document.getElementById(
                "userTableBody"
            );

        const userSearch =
            document.getElementById(
                "userSearch"
            );

        const userRoleFilter =
            document.getElementById(
                "userRoleFilter"
            );

        const userStatusFilter =
            document.getElementById(
                "userStatusFilter"
            );

        const visibleUserCount =
            document.getElementById(
                "visibleUserCount"
            );

        const userEmptyState =
            document.getElementById(
                "userEmptyState"
            );

        const totalUserCount =
            document.getElementById(
                "totalUserCount"
            );

        const activeUserCount =
            document.getElementById(
                "activeUserCount"
            );

        const inactiveUserCount =
            document.getElementById(
                "inactiveUserCount"
            );

        const vendorUserCount =
            document.getElementById(
                "vendorUserCount"
            );

        const userModal =
            document.getElementById(
                "userModal"
            );

        const openAddUserButton =
            document.getElementById(
                "openAddUserButton"
            );

        const closeUserModalButton =
            document.getElementById(
                "closeUserModalButton"
            );

        const cancelUserButton =
            document.getElementById(
                "cancelUserButton"
            );

        const userForm =
            document.getElementById(
                "userForm"
            );

        const editingUserId =
            document.getElementById(
                "editingUserId"
            );

        const managedUserName =
            document.getElementById(
                "managedUserName"
            );

        const managedUserEmail =
            document.getElementById(
                "managedUserEmail"
            );

        const managedUserPhone =
            document.getElementById(
                "managedUserPhone"
            );

        const managedUserRole =
            document.getElementById(
                "managedUserRole"
            );

        const managedUserStatus =
            document.getElementById(
                "managedUserStatus"
            );

        const managedUserDepartment =
            document.getElementById(
                "managedUserDepartment"
            );

        const userModalTitle =
            document.getElementById(
                "userModalTitle"
            );

        const saveUserButtonText =
            document.getElementById(
                "saveUserButtonText"
            );

        const userFormMessage =
            document.getElementById(
                "userFormMessage"
            );

        let managedUsers = [];

        function getUserInitials(
            name
        ) {

            return name
                .split(" ")
                .filter(Boolean)
                .slice(0, 2)
                .map(part =>
                    part.charAt(0).toUpperCase()
                )
                .join("");
        }

        function updateUserStatistics() {

            const total =
                managedUsers.length;

            const active =
                managedUsers.filter(
                    user =>
                        user.status ===
                        "Active"
                ).length;

            const inactive =
                managedUsers.filter(
                    user =>
                        user.status ===
                        "Inactive"
                ).length;

            const vendors =
                managedUsers.filter(
                    user =>
                        user.role ===
                        "Vendor"
                ).length;

            if (totalUserCount) {
                totalUserCount.textContent =
                    total;
            }

            if (activeUserCount) {
                activeUserCount.textContent =
                    active;
            }

            if (inactiveUserCount) {
                inactiveUserCount.textContent =
                    inactive;
            }

            if (vendorUserCount) {
                vendorUserCount.textContent =
                    vendors;
            }
        }

        function getFilteredUsers() {

            const search =
                userSearch?.value
                    .trim()
                    .toLowerCase() || "";

            const role =
                userRoleFilter?.value ||
                "all";

            const status =
                userStatusFilter?.value ||
                "all";

            return managedUsers.filter(
                user => {

                    const matchesSearch =
                        user.name
                            .toLowerCase()
                            .includes(search) ||
                        user.email
                            .toLowerCase()
                            .includes(search);

                    const matchesRole =
                        role === "all" ||
                        user.role === role;

                    const matchesStatus =
                        status === "all" ||
                        user.status === status;

                    return (
                        matchesSearch &&
                        matchesRole &&
                        matchesStatus
                    );
                }
            );
        }

        function renderManagedUsers() {

            if (!userTableBody) {
                return;
            }

            const users =
                getFilteredUsers();

            userTableBody.innerHTML =
                users.map(
                    user => {

                        const statusClass =
                            user.status ===
                            "Active"
                                ? "sd-status-ready"
                                : "sd-status-warning";

                        const toggleIcon =
                            user.status ===
                            "Active"
                                ? "bi-person-dash"
                                : "bi-person-check";

                        const toggleTitle =
                            user.status ===
                            "Active"
                                ? "Deactivate account"
                                : "Activate account";

                        return `
                            <tr>

                                <td>

                                    <div class="sd-user-person">

                                        <div class="sd-user-avatar">
                                            ${escapeHTML(
                                                getUserInitials(
                                                    user.name
                                                )
                                            )}
                                        </div>

                                        <div>
                                            <strong>
                                                ${escapeHTML(
                                                    user.name
                                                )}
                                            </strong>

                                            <span>
                                                ${escapeHTML(
                                                    user.department ||
                                                    "Not specified"
                                                )}
                                            </span>
                                        </div>

                                    </div>

                                </td>

                                <td>

                                    <span class="sd-user-role">
                                        ${escapeHTML(
                                            user.role
                                        )}
                                    </span>

                                </td>

                                <td>

                                    <div class="sd-user-contact">

                                        <strong>
                                            ${escapeHTML(
                                                user.email
                                            )}
                                        </strong>

                                        <span>
                                            ${escapeHTML(
                                                user.phone
                                            )}
                                        </span>

                                    </div>

                                </td>

                                <td>

                                    <span
                                        class="sd-status ${statusClass}"
                                    >
                                        ${escapeHTML(
                                            user.status
                                        )}
                                    </span>

                                </td>

                                <td>
                                    ${escapeHTML(
                                        user.lastLogin
                                    )}
                                </td>

                                <td>

                                    <div class="sd-user-actions">

                                        <button
                                            type="button"
                                            class="sd-user-action-btn"
                                            data-edit-user="${user.id}"
                                            title="View / Edit user"
                                            aria-label="Edit ${escapeHTML(
                                                user.name
                                            )}"
                                        >
                                            <i class="bi bi-pencil-square"></i>
                                        </button>

                                        <button
                                            type="button"
                                            class="sd-user-action-btn ${
                                                user.status ===
                                                "Active"
                                                    ? "sd-danger"
                                                    : ""
                                            }"
                                            data-toggle-user="${user.id}"
                                            title="${toggleTitle}"
                                            aria-label="${toggleTitle}"
                                        >
                                            <i class="bi ${toggleIcon}"></i>
                                        </button>

                                    </div>

                                </td>

                            </tr>
                        `;
                    }
                )
                .join("");

            if (visibleUserCount) {

                visibleUserCount.textContent =
                    `${users.length} ${
                        users.length === 1
                            ? "User"
                            : "Users"
                    }`;
            }

            if (userEmptyState) {

                userEmptyState.hidden =
                    users.length !== 0;
            }

            updateUserStatistics();
        }

        function openUserModal(
            user = null
        ) {

            if (!userModal) {
                return;
            }

            userForm?.reset();

            userFormMessage.textContent = "";
            userFormMessage.className =
                "sd-form-message";

            if (user) {

                editingUserId.value =
                    user.id;

                managedUserName.value =
                    user.name;

                managedUserEmail.value =
                    user.email;

                managedUserPhone.value =
                    user.phone;

                managedUserRole.value =
                    user.role;

                managedUserStatus.value =
                    user.status;

                managedUserDepartment.value =
                    user.department || "";

                userModalTitle.textContent =
                    "Edit User Account";

                saveUserButtonText.textContent =
                    "Save Changes";

            } else {

                editingUserId.value = "";

                managedUserStatus.value =
                    "Active";

                userModalTitle.textContent =
                    "Add New User";

                saveUserButtonText.textContent =
                    "Create User";
            }

            userModal.classList.add(
                "open"
            );

            userModal.setAttribute(
                "aria-hidden",
                "false"
            );

            document.body.classList.add(
                "sd-modal-open"
            );

            setTimeout(
                () =>
                    managedUserName?.focus(),
                50
            );
        }

        function closeUserModal() {

            userModal?.classList.remove(
                "open"
            );

            userModal?.setAttribute(
                "aria-hidden",
                "true"
            );

            document.body.classList.remove(
                "sd-modal-open"
            );
        }

        function validateManagedUser() {

            const name =
                managedUserName.value.trim();

            const email =
                managedUserEmail.value
                    .trim()
                    .toLowerCase();

            const phone =
                managedUserPhone.value
                    .trim();

            const role =
                managedUserRole.value;

            if (
                !name ||
                !email ||
                !phone ||
                !role
            ) {

                return {
                    valid: false,
                    message:
                        "Complete all required account fields."
                };
            }

            const emailPattern =
                /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

            if (
                !emailPattern.test(
                    email
                )
            ) {

                return {
                    valid: false,
                    message:
                        "Enter a valid email address."
                };
            }

            const phonePattern =
                /^0[0-9]{9}$/;

            if (
                !phonePattern.test(
                    phone
                )
            ) {

                return {
                    valid: false,
                    message:
                        "Enter a valid 10-digit Sri Lankan mobile number."
                };
            }

            const currentId =
                Number(
                    editingUserId.value
                );

            const duplicateEmail =
                managedUsers.some(
                    user =>
                        user.email
                            .toLowerCase() ===
                            email &&
                        user.id !== currentId
                );

            if (duplicateEmail) {

                return {
                    valid: false,
                    message:
                        "Another account already uses this email address."
                };
            }

            return {
                valid: true
            };
        }

        userForm?.addEventListener(
            "submit",
            event => {

                event.preventDefault();

                const validation =
                    validateManagedUser();

                if (!validation.valid) {

                    userFormMessage.textContent =
                        validation.message;

                    userFormMessage.className =
                        "sd-form-message error";

                    return;
                }

                const id =
                    Number(
                        editingUserId.value
                    );

                const userData = {

                    name:
                        managedUserName
                            .value
                            .trim(),

                    email:
                        managedUserEmail
                            .value
                            .trim()
                            .toLowerCase(),

                    phone:
                        managedUserPhone
                            .value
                            .trim(),

                    role:
                        managedUserRole.value,

                    status:
                        managedUserStatus.value,

                    department:
                        managedUserDepartment
                            .value
                            .trim() ||
                        "Not specified"
                };

                if (id) {

                    const user =
                        managedUsers.find(
                            item =>
                                item.id === id
                        );

                    if (user) {

                        Object.assign(
                            user,
                            userData
                        );

                        userFormMessage.textContent =
                            "User account updated successfully.";
                    }

                } else {

                    const nextId =
                        managedUsers.length
                            ? Math.max(
                                ...managedUsers.map(
                                    user =>
                                        user.id
                                )
                            ) + 1
                            : 1;

                    managedUsers.unshift({

                        id:
                            nextId,

                        ...userData,

                        lastLogin:
                            "Never"

                    });

                    userFormMessage.textContent =
                        "User account created successfully.";
                }

                userFormMessage.className =
                    "sd-form-message success";

                renderManagedUsers();

                setTimeout(
                    closeUserModal,
                    650
                );
            }
        );

        userTableBody?.addEventListener(
            "click",
            event => {

                const editButton =
                    event.target.closest(
                        "[data-edit-user]"
                    );

                const toggleButton =
                    event.target.closest(
                        "[data-toggle-user]"
                    );

                if (editButton) {

                    const id =
                        Number(
                            editButton.dataset
                                .editUser
                        );

                    const user =
                        managedUsers.find(
                            item =>
                                item.id === id
                        );

                    if (user) {
                        openUserModal(
                            user
                        );
                    }

                    return;
                }

                if (toggleButton) {

                    const id =
                        Number(
                            toggleButton.dataset
                                .toggleUser
                        );

                    const user =
                        managedUsers.find(
                            item =>
                                item.id === id
                        );

                    if (!user) {
                        return;
                    }

                    user.status =
                        user.status ===
                        "Active"
                            ? "Inactive"
                            : "Active";

                    renderManagedUsers();
                }
            }
        );

        userSearch?.addEventListener(
            "input",
            renderManagedUsers
        );

        userRoleFilter?.addEventListener(
            "change",
            renderManagedUsers
        );

        userStatusFilter?.addEventListener(
            "change",
            renderManagedUsers
        );

        openAddUserButton?.addEventListener(
            "click",
            () => openUserModal()
        );

        closeUserModalButton?.addEventListener(
            "click",
            closeUserModal
        );

        cancelUserButton?.addEventListener(
            "click",
            closeUserModal
        );

        userModal?.addEventListener(
            "click",
            event => {

                if (
                    event.target.matches(
                        "[data-close-user-modal]"
                    )
                ) {

                    closeUserModal();
                }
            }
        );

        document.addEventListener(
            "keydown",
            event => {

                if (
                    event.key ===
                        "Escape" &&
                    userModal?.classList.contains(
                        "open"
                    )
                ) {

                    closeUserModal();
                }
            }
        );

        renderManagedUsers();

    }
);

/* =========================================================
   MANAGER CUSTOMER MESSAGES MODULE
   Frontend demo state.
   Backend will replace demo data with Contact Inquiry API.
========================================================= */

document.addEventListener("DOMContentLoaded", () => {

    const messageSection =
        document.getElementById("customer-messages");

    if (!messageSection) {
        return;
    }

    const messageList =
        document.getElementById("managerMessageList");

    const emptyState =
        document.getElementById("managerMessageEmpty");

    const searchInput =
        document.getElementById("managerMessageSearch");

    const statusFilter =
        document.getElementById("managerMessageStatusFilter");

    const unreadCount =
        document.getElementById("managerUnreadMessages");

    const navCount =
        document.getElementById("managerMessageCount");

    const modal =
        document.getElementById("managerMessageModal");

    const modalSubject =
        document.getElementById("managerMessageModalSubject");

    const modalDetails =
        document.getElementById("managerMessageDetails");

    const modalBody =
        document.getElementById("managerMessageBody");

    const resolveButton =
        document.getElementById("managerResolveMessage");

    let activeMessageId = null;

    /*
     * DEMO DATA ONLY
     *
     * Final flow:
     * contact.html -> POST Contact API -> Database
     * Manager Dashboard -> GET Contact Inquiry API
     */

    const customerMessages = [];

    function escapeManagerMessageHTML(value) {

        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");

    }

    function initials(name) {

        return String(name)
            .split(" ")
            .filter(Boolean)
            .slice(0, 2)
            .map(part => part[0])
            .join("")
            .toUpperCase();

    }

    function statusLabel(status) {

        if (status === "new") {
            return "New";
        }

        if (status === "resolved") {
            return "Resolved";
        }

        return "Read";

    }

    function updateMessageCounts() {

        const total =
            customerMessages.filter(
                item => item.status === "new"
            ).length;

        if (unreadCount) {
            unreadCount.textContent = total;
        }

        if (navCount) {

            navCount.textContent = total;

            navCount.hidden =
                total === 0;

        }

    }

    function renderManagerMessages() {

        const query =
            (searchInput?.value || "")
                .trim()
                .toLowerCase();

        const filter =
            statusFilter?.value || "all";

        const filtered =
            customerMessages.filter(item => {

                const matchesStatus =
                    filter === "all" ||
                    item.status === filter;

                const searchText = [
                    item.name,
                    item.email,
                    item.phone,
                    item.type,
                    item.subject,
                    item.message
                ]
                    .join(" ")
                    .toLowerCase();

                const matchesSearch =
                    !query ||
                    searchText.includes(query);

                return (
                    matchesStatus &&
                    matchesSearch
                );

            });

        if (emptyState) {
            emptyState.hidden =
                filtered.length !== 0;
        }

        messageList.innerHTML =
            filtered.map(item => `

                <article
                    class="sd-manager-message-card ${
                        item.status === "new"
                            ? "is-new"
                            : ""
                    }"
                >

                    <div class="sd-manager-message-card-top">

                        <div class="sd-manager-message-customer">

                            <div class="sd-manager-message-avatar">
                                ${escapeManagerMessageHTML(
                                    initials(item.name)
                                )}
                            </div>

                            <div>

                                <strong>
                                    ${escapeManagerMessageHTML(item.name)}
                                </strong>

                                <span>
                                    ${escapeManagerMessageHTML(item.email)}
                                </span>

                            </div>

                        </div>

                        <div class="sd-manager-message-meta">

                            <span
                                class="sd-manager-message-status ${
                                    escapeManagerMessageHTML(item.status)
                                }"
                            >
                                ${escapeManagerMessageHTML(
                                    statusLabel(item.status)
                                )}
                            </span>

                            <time>
                                ${escapeManagerMessageHTML(item.received)}
                            </time>

                        </div>

                    </div>

                    <div class="sd-manager-message-content">

                        <span class="sd-manager-message-type">
                            ${escapeManagerMessageHTML(item.type)}
                        </span>

                        <h3>
                            ${escapeManagerMessageHTML(item.subject)}
                        </h3>

                        <p>
                            ${escapeManagerMessageHTML(
                                item.message.length > 145
                                    ? item.message.slice(0,145) + "..."
                                    : item.message
                            )}
                        </p>

                    </div>

                    <div class="sd-manager-message-actions">

                        <button
                            type="button"
                            class="sd-manager-message-view"
                            data-view-message="${escapeManagerMessageHTML(item.id)}"
                        >
                            <i class="bi bi-eye"></i>
                            View Message
                        </button>

                        ${
                            item.status === "new"
                                ? `
                                    <button
                                        type="button"
                                        class="sd-manager-message-read"
                                        data-read-message="${escapeManagerMessageHTML(item.id)}"
                                    >
                                        <i class="bi bi-envelope-open"></i>
                                        Mark as Read
                                    </button>
                                `
                                : ""
                        }

                        ${
                            item.status !== "resolved"
                                ? `
                                    <button
                                        type="button"
                                        class="sd-manager-message-resolve"
                                        data-resolve-message="${escapeManagerMessageHTML(item.id)}"
                                    >
                                        <i class="bi bi-check2-circle"></i>
                                        Mark as Resolved
                                    </button>
                                `
                                : ""
                        }

                    </div>

                </article>

            `).join("");

        attachManagerMessageEvents();
        updateMessageCounts();

    }

    function markMessageRead(id) {

        const item =
            customerMessages.find(
                message => message.id === id
            );

        if (
            item &&
            item.status === "new"
        ) {
            item.status = "read";
        }

        renderManagerMessages();

    }

    function resolveMessage(id) {

        const item =
            customerMessages.find(
                message => message.id === id
            );

        if (item) {
            item.status = "resolved";
        }

        renderManagerMessages();

    }

    function openManagerMessage(id) {

        const item =
            customerMessages.find(
                message => message.id === id
            );

        if (!item) {
            return;
        }

        activeMessageId = item.id;

        if (item.status === "new") {
            item.status = "read";
        }

        modalSubject.textContent =
            item.subject;

        modalDetails.innerHTML = `

            <div class="sd-manager-message-detail">
                <span>Customer</span>
                <strong>
                    ${escapeManagerMessageHTML(item.name)}
                </strong>
            </div>

            <div class="sd-manager-message-detail">
                <span>Inquiry Type</span>
                <strong>
                    ${escapeManagerMessageHTML(item.type)}
                </strong>
            </div>

            <div class="sd-manager-message-detail">
                <span>Email</span>
                <strong>
                    ${escapeManagerMessageHTML(item.email)}
                </strong>
            </div>

            <div class="sd-manager-message-detail">
                <span>Phone</span>
                <strong>
                    ${escapeManagerMessageHTML(item.phone)}
                </strong>
            </div>

            <div class="sd-manager-message-detail">
                <span>Received</span>
                <strong>
                    ${escapeManagerMessageHTML(item.received)}
                </strong>
            </div>

            <div class="sd-manager-message-detail">
                <span>Status</span>
                <strong>
                    ${escapeManagerMessageHTML(
                        statusLabel(item.status)
                    )}
                </strong>
            </div>

        `;

        modalBody.textContent =
            item.message;

        if (resolveButton) {

            resolveButton.hidden =
                item.status === "resolved";

        }

        modal.hidden = false;

        renderManagerMessages();

    }

    function closeManagerMessage() {

        modal.hidden = true;
        activeMessageId = null;

    }

    function attachManagerMessageEvents() {

        document
            .querySelectorAll("[data-view-message]")
            .forEach(button => {

                button.addEventListener(
                    "click",
                    () => {

                        openManagerMessage(
                            button.dataset.viewMessage
                        );

                    }
                );

            });

        document
            .querySelectorAll("[data-read-message]")
            .forEach(button => {

                button.addEventListener(
                    "click",
                    () => {

                        markMessageRead(
                            button.dataset.readMessage
                        );

                    }
                );

            });

        document
            .querySelectorAll("[data-resolve-message]")
            .forEach(button => {

                button.addEventListener(
                    "click",
                    () => {

                        resolveMessage(
                            button.dataset.resolveMessage
                        );

                    }
                );

            });

    }

    document
        .querySelectorAll("[data-close-manager-message]")
        .forEach(button => {

            button.addEventListener(
                "click",
                closeManagerMessage
            );

        });

    if (resolveButton) {

        resolveButton.addEventListener(
            "click",
            () => {

                if (!activeMessageId) {
                    return;
                }

                resolveMessage(
                    activeMessageId
                );

                closeManagerMessage();

            }
        );

    }

    if (searchInput) {

        searchInput.addEventListener(
            "input",
            renderManagerMessages
        );

    }

    if (statusFilter) {

        statusFilter.addEventListener(
            "change",
            renderManagerMessages
        );

    }

    document.addEventListener(
        "keydown",
        event => {

            if (
                event.key === "Escape" &&
                modal &&
                !modal.hidden
            ) {
                closeManagerMessage();
            }

        }
    );

    renderManagerMessages();

});

/* =========================================================
   MANAGER VENDOR REGISTRATION APPROVAL MODULE

   Frontend demo workflow:
   Vendor Registration
        -> Pending Approval
        -> Manager Approve / Reject

   Backend later replaces localStorage with API + database.

   IMPORTANT:
   Vendor passwords are NOT stored here.
========================================================= */

document.addEventListener("DOMContentLoaded", () => {

    const MODULE_KEY =
        "shiftDynamicsVendorRegistrations";

    const list =
        document.getElementById(
            "vendorRegistrationList"
        );

    const emptyState =
        document.getElementById(
            "vendorRegistrationEmpty"
        );

    const searchInput =
        document.getElementById(
            "vendorRegistrationSearch"
        );

    const filter =
        document.getElementById(
            "vendorRegistrationFilter"
        );

    const pendingCount =
        document.getElementById(
            "vendorRegistrationPendingCount"
        );

    const navCount =
        document.getElementById(
            "vendorRegistrationNavCount"
        );

    if (!list) {
        return;
    }

    function escapeVendorHTML(value) {

        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function readRequests() {

        try {

            return JSON.parse(
                localStorage.getItem(
                    MODULE_KEY
                ) || "[]"
            );

        } catch (error) {

            console.error(
                "Unable to read vendor registrations:",
                error
            );

            return [];
        }
    }

    function saveRequests(requests) {

        localStorage.setItem(
            MODULE_KEY,
            JSON.stringify(requests)
        );
    }

    function formatSubmittedDate(value) {

        if (!value) {
            return "Not available";
        }

        const date =
            new Date(value);

        if (
            Number.isNaN(
                date.getTime()
            )
        ) {
            return "Not available";
        }

        return date.toLocaleString();
    }

    function getStatusClass(status) {

        if (status === "Approved") {
            return "approved";
        }

        if (status === "Rejected") {
            return "rejected";
        }

        return "pending";
    }

    function getFilteredRequests() {

        const requests =
            readRequests();

        const search =
            searchInput?.value
                .trim()
                .toLowerCase() || "";

        const selectedStatus =
            filter?.value || "all";

        return requests.filter(
            request => {

                const searchable =
                    [
                        request.businessName,
                        request.contactPerson,
                        request.email,
                        request.mobile,
                        request.specialization
                    ]
                    .join(" ")
                    .toLowerCase();

                const matchesSearch =
                    searchable.includes(
                        search
                    );

                const matchesStatus =
                    selectedStatus === "all" ||
                    request.status ===
                        selectedStatus;

                return (
                    matchesSearch &&
                    matchesStatus
                );
            }
        );
    }

    function updateCounts() {

        const requests =
            readRequests();

        const pending =
            requests.filter(
                request =>
                    request.status ===
                    "Pending Approval"
            ).length;

        if (pendingCount) {
            pendingCount.textContent =
                pending;
        }

        if (navCount) {

            navCount.textContent =
                pending;

            navCount.hidden =
                pending === 0;
        }
    }

    function renderVendorRegistrations() {

        const requests =
            getFilteredRequests();

        list.innerHTML =
            requests.map(
                request => {

                    const status =
                        request.status ||
                        "Pending Approval";

                    const statusClass =
                        getStatusClass(
                            status
                        );

                    const canReview =
                        status ===
                        "Pending Approval";

                    return `

                        <article
                            class="sd-vendor-registration-card"
                        >

                            <div
                                class="sd-vendor-registration-card-head"
                            >

                                <div
                                    class="sd-vendor-registration-business"
                                >

                                    <div
                                        class="sd-vendor-registration-icon"
                                    >
                                        <i class="bi bi-shop"></i>
                                    </div>

                                    <div>

                                        <span>
                                            Vendor Application
                                        </span>

                                        <h3>
                                            ${escapeVendorHTML(
                                                request.businessName
                                            )}
                                        </h3>

                                        <small>
                                            ${escapeVendorHTML(
                                                request.id
                                            )}
                                        </small>

                                    </div>

                                </div>

                                <span
                                    class="sd-vendor-registration-status ${statusClass}"
                                >
                                    ${escapeVendorHTML(
                                        status
                                    )}
                                </span>

                            </div>

                            <div
                                class="sd-vendor-registration-details"
                            >

                                <div>

                                    <span>
                                        Contact Person
                                    </span>

                                    <strong>
                                        ${escapeVendorHTML(
                                            request.contactPerson
                                        )}
                                    </strong>

                                </div>

                                <div>

                                    <span>
                                        Business Email
                                    </span>

                                    <strong>
                                        ${escapeVendorHTML(
                                            request.email
                                        )}
                                    </strong>

                                </div>

                                <div>

                                    <span>
                                        Mobile
                                    </span>

                                    <strong>
                                        ${escapeVendorHTML(
                                            request.mobile
                                        )}
                                    </strong>

                                </div>

                                <div>

                                    <span>
                                        Specialization
                                    </span>

                                    <strong>
                                        ${escapeVendorHTML(
                                            request.specialization
                                        )}
                                    </strong>

                                </div>

                                <div
                                    class="sd-vendor-registration-address"
                                >

                                    <span>
                                        Business Address
                                    </span>

                                    <strong>
                                        ${escapeVendorHTML(
                                            request.businessAddress
                                        )}
                                    </strong>

                                </div>

                                <div>

                                    <span>
                                        Submitted
                                    </span>

                                    <strong>
                                        ${escapeVendorHTML(
                                            formatSubmittedDate(
                                                request.submittedAt
                                            )
                                        )}
                                    </strong>

                                </div>

                            </div>

                            ${
                                request.reviewedAt
                                    ? `
                                        <div
                                            class="sd-vendor-review-note"
                                        >
                                            <i class="bi bi-clock-history"></i>

                                            Reviewed:
                                            ${escapeVendorHTML(
                                                formatSubmittedDate(
                                                    request.reviewedAt
                                                )
                                            )}
                                        </div>
                                    `
                                    : ""
                            }

                            <div
                                class="sd-vendor-registration-actions"
                            >

                                ${
                                    canReview
                                        ? `

                                            <button
                                                type="button"
                                                class="sd-vendor-reject-btn"
                                                data-reject-vendor="${escapeVendorHTML(
                                                    request.id
                                                )}"
                                            >
                                                <i class="bi bi-x-circle"></i>
                                                Reject
                                            </button>

                                            <button
                                                type="button"
                                                class="sd-vendor-approve-btn"
                                                data-approve-vendor="${escapeVendorHTML(
                                                    request.id
                                                )}"
                                            >
                                                <i class="bi bi-check-circle"></i>
                                                Approve Vendor
                                            </button>

                                        `
                                        : `

                                            <span
                                                class="sd-vendor-review-complete"
                                            >
                                                <i class="bi bi-check2-circle"></i>
                                                Review completed
                                            </span>

                                        `
                                }

                            </div>

                        </article>

                    `;
                }
            )
            .join("");

        if (emptyState) {
            emptyState.hidden =
                requests.length !== 0;
        }

        updateCounts();
    }

    function changeVendorStatus(
        id,
        newStatus
    ) {

        const requests =
            readRequests();

        const request =
            requests.find(
                item =>
                    item.id === id
            );

        if (!request) {
            return;
        }

        request.status =
            newStatus;

        request.reviewedAt =
            new Date().toISOString();

        /*
            BACKEND LATER:

            APPROVE:
            PUT /api/vendor-registrations/{id}/approve

            REJECT:
            PUT /api/vendor-registrations/{id}/reject

            When approved, backend should:

            1. Activate vendor account
            2. Save approval in database
            3. Send approval email
            4. Allow Staff & Vendor Login
        */

        saveRequests(
            requests
        );

        renderVendorRegistrations();

        if (
            newStatus ===
            "Approved"
        ) {

            alert(
                "Vendor approved successfully.\n\n" +
                "Frontend demo: approval status has been saved.\n" +
                "The .NET backend will later send the approval email."
            );

        } else {

            alert(
                "Vendor registration rejected."
            );
        }
    }

    list.addEventListener(
        "click",
        event => {

            const approveButton =
                event.target.closest(
                    "[data-approve-vendor]"
                );

            const rejectButton =
                event.target.closest(
                    "[data-reject-vendor]"
                );

            if (approveButton) {

                changeVendorStatus(
                    approveButton.dataset
                        .approveVendor,
                    "Approved"
                );

                return;
            }

            if (rejectButton) {

                const confirmed =
                    confirm(
                        "Reject this vendor registration?"
                    );

                if (!confirmed) {
                    return;
                }

                changeVendorStatus(
                    rejectButton.dataset
                        .rejectVendor,
                    "Rejected"
                );
            }
        }
    );

    searchInput?.addEventListener(
        "input",
        renderVendorRegistrations
    );

    filter?.addEventListener(
        "change",
        renderVendorRegistrations
    );

    window.addEventListener(
        "storage",
        event => {

            if (
                event.key ===
                MODULE_KEY
            ) {

                renderVendorRegistrations();
            }
        }
    );

    renderVendorRegistrations();

});

document.addEventListener("DOMContentLoaded", () => {
    // dynamicGreeting-managerGreeting
    const greeting = document.getElementById("managerGreeting");
    if (!greeting) return;

    const currentName = greeting.textContent
        .replace(/Good morning,?/i, "")
        .replace(/Good afternoon,?/i, "")
        .replace(/Good evening,?/i, "")
        .replace(/\./g, "")
        .trim();

    const hour = new Date().getHours();
    let message = "Good evening";

    if (hour < 12) {
        message = "Good morning";
    } else if (hour < 17) {
        message = "Good afternoon";
    }

    greeting.textContent = currentName
        ? message + ", " + currentName + "."
        : message + ".";
});
