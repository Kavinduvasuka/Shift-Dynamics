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

            workshop:
                "Workshop Load",

            vendors:
                "Vendor Bid Comparison",

            orders:
                "Order Approvals",

            billing:
                "Billing Review",

            analytics:
                "Business Analytics",

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
            new Set(["Bay 03"]);


        function escapeHTML(value = "") {

            return String(value)
                .replaceAll("&", "&amp;")
                .replaceAll("<", "&lt;")
                .replaceAll(">", "&gt;")
                .replaceAll('"', "&quot;")
                .replaceAll("'", "&#039;");
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
                `${selectedJob.vehicle} · ${selectedJob.plate}`;


            mechanicSelect.disabled = false;
            baySelect.disabled = false;
            managerNote.disabled = false;
            assignJobButton.disabled = false;


            assignmentMessage.textContent =
                "Job card selected. Choose a mechanic and available workshop bay.";

            assignmentMessage.className =
                "sd-form-message";
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


                assignmentTable?.prepend(
                    assignmentRow
                );


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
           VENDOR BID COMPARISON
           Frontend demo only.
           Real quotes will come from the .NET API.
           ================================================= */

        const vendorQuoteGrid =
            document.getElementById(
                "vendorQuoteGrid"
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


        function formatLKR(value) {

            return Number(value)
                .toLocaleString("en-LK");
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
                        · Delivery:
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


                vendorApproved = true;


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
                            · ${escapeHTML(
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
                    `${selectedVendorQuote.vendor} approved for Parts Request #PR-3021.`;

                vendorApprovalMessage.className =
                    "sd-form-message success";
            }
        );


        /* =================================================
           PURCHASE ORDER APPROVAL
           Frontend demo only.
           Backend must persist manager authorization.
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


        confirmPurchaseOrder?.addEventListener(
            "change",
            () => {

                if (purchaseOrderApproved) {
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
                        "Purchase Order #PO-2048 has already been approved.";

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
                    "Purchase Order #PO-2048 approved successfully. Procurement can continue.";

                orderApprovalMessage.className =
                    "sd-form-message success";
            }
        );


        /* =================================================
           FINAL BILLING APPROVAL
           Frontend demo only.
           Real authorization/persistence belongs in .NET.
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


        confirmBilling?.addEventListener(
            "change",
            () => {

                if (billingApproved) {
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
                        "Invoice #INV-1062 has already been approved.";

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


                billingApproved = true;


                billingStatus.textContent =
                    "Billing Approved";

                billingStatus.className =
                    "sd-status sd-status-ready";


                confirmBilling.disabled = true;
                approveBillingButton.disabled = true;
                billingApprovalNote.disabled = true;


                billingMessage.textContent =
                    "Invoice #INV-1062 approved. It is ready for the customer payment process.";

                billingMessage.className =
                    "sd-form-message success";
            }
        );


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


        let managedUsers = [

            {
                id: 1,
                name: "Amal Fernando",
                email: "advisor@shiftdynamics.com",
                phone: "0771234567",
                role: "Service Advisor",
                status: "Active",
                department: "Customer Service",
                lastLogin: "Today, 08:42"
            },

            {
                id: 2,
                name: "Ravindu Silva",
                email: "manager@shiftdynamics.com",
                phone: "0712345678",
                role: "Manager",
                status: "Active",
                department: "Management",
                lastLogin: "Today, 07:58"
            },

            {
                id: 3,
                name: "Nimal Perera",
                email: "nimal@shiftdynamics.com",
                phone: "0763456789",
                role: "Mechanic",
                status: "Active",
                department: "Engine & Mechanical",
                lastLogin: "Yesterday, 17:21"
            },

            {
                id: 4,
                name: "Kasun Silva",
                email: "kasun@shiftdynamics.com",
                phone: "0754567890",
                role: "Mechanic",
                status: "Active",
                department: "Diagnostics",
                lastLogin: "Yesterday, 16:54"
            },

            {
                id: 5,
                name: "Tharindu Jayasekara",
                email: "storekeeper@shiftdynamics.com",
                phone: "0745678901",
                role: "Storekeeper",
                status: "Active",
                department: "Inventory",
                lastLogin: "Today, 08:11"
            },

            {
                id: 6,
                name: "AutoParts Lanka",
                email: "vendor@example.com",
                phone: "0726789012",
                role: "Vendor",
                status: "Active",
                department: "Brake & Mechanical Parts",
                lastLogin: "Aug 30, 14:30"
            },

            {
                id: 7,
                name: "MotorHub Spares",
                email: "sales@motorhub.lk",
                phone: "0707890123",
                role: "Vendor",
                status: "Inactive",
                department: "General Spare Parts",
                lastLogin: "Aug 18, 11:15"
            }
        ];


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






