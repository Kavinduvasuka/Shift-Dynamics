document.addEventListener("DOMContentLoaded", () => {

    /* =====================================================
       FRONTEND DEMO STATE

       Important:
       This data exists only while this page is open.
       Future .NET backend will store customer, vehicle,
       inspection and job-card records in the database.
       ===================================================== */

    let currentIntake = null;

    /*
        Selected diagnostic information is temporary
        frontend state.

        Future .NET backend:
        JobCard -> DiagnosticRecord -> Estimate
    */
    let currentDiagnostic = null;

    /*
        Temporary estimate workflow state.

        Real application:
        Estimate + approval status will come from
        the .NET backend/database.
    */
    let currentEstimate = null;

    /*
        Temporary frontend workflow state.

        Real application:
        invoice finalization and handover permission
        must be stored and validated by the .NET backend.
    */
    let invoiceFinalized = false;


    /* =====================================================
       ELEMENTS
       ===================================================== */

    const navItems =
        document.querySelectorAll(".sd-nav-item");

    const sections =
        document.querySelectorAll(".sd-section");

    const pageTitle =
        document.getElementById("pageTitle");

    const sidebar =
        document.getElementById("advisorSidebar");

    const sidebarOpen =
        document.getElementById("sidebarOpen");

    const sidebarClose =
        document.getElementById("sidebarClose");

    const sidebarOverlay =
        document.getElementById("sidebarOverlay");


    const sectionTitles = {

        overview:
            "Workshop Overview",

        "customer-intake":
            "Customer Intake",

        inspection:
            "Walkaround Inspection",

        "job-cards":
            "Official Job Cards",

        diagnostics:
            "Diagnostic Notes",

        estimates:
            "Customer Estimates",

        invoice:
            "Invoice Compilation",

        handover:
            "Final Handover"
    };


    /* =====================================================
       NAVIGATION
       ===================================================== */

    function showSection(sectionId) {

        sections.forEach(section => {

            section.classList.toggle(
                "active",
                section.id === sectionId
            );

        });


        navItems.forEach(item => {

            item.classList.toggle(
                "active",
                item.dataset.section === sectionId
            );

        });


        if (pageTitle) {

            pageTitle.textContent =
                sectionTitles[sectionId] ||
                "Service Advisor Portal";
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

                showSection(
                    item.dataset.section
                );

            }
        );

    });


    document
        .querySelectorAll("[data-go-section]")
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    showSection(
                        button.dataset.goSection
                    );

                }
            );

        });



    /* =====================================================
       MOBILE SIDEBAR
       ===================================================== */

    function openSidebar() {

        sidebar?.classList.add("open");

        sidebarOverlay?.classList.add("open");

    }


    function closeSidebar() {

        sidebar?.classList.remove("open");

        sidebarOverlay?.classList.remove("open");

    }


    sidebarOpen?.addEventListener(
        "click",
        openSidebar
    );


    sidebarClose?.addEventListener(
        "click",
        closeSidebar
    );


    sidebarOverlay?.addEventListener(
        "click",
        closeSidebar
    );



    /* =====================================================
       HELPER FUNCTIONS
       ===================================================== */

    function escapeHTML(value = "") {

        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }


    function generateJobCardNumber() {

        const randomNumber =
            Math.floor(
                1000 +
                Math.random() * 9000
            );

        return `#JC-${randomNumber}`;
    }



    /* =====================================================
       CUSTOMER INTAKE
       ===================================================== */

    const customerIntakeForm =
        document.getElementById(
            "customerIntakeForm"
        );

    const intakeMessage =
        document.getElementById(
            "intakeMessage"
        );


    customerIntakeForm?.addEventListener(
        "submit",
        event => {

            event.preventDefault();


            if (
                !customerIntakeForm
                    .checkValidity()
            ) {

                intakeMessage.textContent =
                    "Please complete all required intake fields.";

                intakeMessage.className =
                    "sd-form-message error";

                customerIntakeForm
                    .reportValidity();

                return;
            }


            currentIntake = {

                customerName:
                    document
                        .getElementById("customerName")
                        .value
                        .trim(),

                customerMobile:
                    document
                        .getElementById("customerMobile")
                        .value
                        .trim(),

                customerEmail:
                    document
                        .getElementById("customerEmail")
                        .value
                        .trim(),

                customerNic:
                    document
                        .getElementById("customerNic")
                        .value
                        .trim(),

                vehicleMake:
                    document
                        .getElementById("vehicleMake")
                        .value
                        .trim(),

                vehicleModel:
                    document
                        .getElementById("vehicleModel")
                        .value
                        .trim(),

                vehicleYear:
                    document
                        .getElementById("vehicleYear")
                        .value,

                vehiclePlate:
                    document
                        .getElementById("vehiclePlate")
                        .value
                        .trim(),

                vehicleVin:
                    document
                        .getElementById("vehicleVin")
                        .value
                        .trim(),

                vehicleMileage:
                    document
                        .getElementById("vehicleMileage")
                        .value,

                serviceConcern:
                    document
                        .getElementById("serviceConcern")
                        .value
                        .trim()
            };


            intakeMessage.textContent =
                "Customer intake completed. Opening vehicle inspection...";

            intakeMessage.className =
                "sd-form-message success";


            updateInspectionVehicleSummary();


            setTimeout(
                () => {

                    showSection(
                        "inspection"
                    );

                },
                550
            );

        }
    );



    /* =====================================================
       WALKAROUND INSPECTION
       ===================================================== */

    const inspectionChecks =
        document.querySelectorAll(
            ".inspection-check"
        );

    const inspectionProgressText =
        document.getElementById(
            "inspectionProgressText"
        );


    function updateInspectionVehicleSummary() {

        if (!currentIntake) {
            return;
        }


        const summary =
            document.querySelector(
                ".sd-vehicle-summary"
            );


        if (!summary) {
            return;
        }


        const strong =
            summary.querySelector("strong");

        const span =
            summary.querySelector("span");


        if (strong) {

            strong.textContent =
                `${currentIntake.vehicleMake} ${currentIntake.vehicleModel} ${currentIntake.vehicleYear}`;
        }


        if (span) {

            const mileage =
                currentIntake.vehicleMileage
                    ? `${Number(currentIntake.vehicleMileage).toLocaleString()} km`
                    : "Mileage not recorded";


            span.textContent =
                `${currentIntake.vehiclePlate} · ${mileage}`;
        }
    }


    function updateInspectionProgress() {

        let completed = 0;


        inspectionChecks.forEach(check => {

            const card =
                check.closest(
                    ".sd-check-card"
                );

            const icon =
                card?.querySelector(
                    ".sd-check-icon i"
                );


            if (check.checked) {

                completed++;


                card?.classList.add(
                    "checked"
                );


                if (icon) {

                    icon.className =
                        "bi bi-check-circle-fill";
                }

            } else {

                card?.classList.remove(
                    "checked"
                );


                if (icon) {

                    icon.className =
                        "bi bi-circle";
                }
            }

        });


        if (inspectionProgressText) {

            inspectionProgressText.textContent =
                `${completed} / ${inspectionChecks.length}`;
        }
    }


    inspectionChecks.forEach(check => {

        check.addEventListener(
            "change",
            updateInspectionProgress
        );

    });


    const saveInspection =
        document.getElementById(
            "saveInspection"
        );

    const inspectionMessage =
        document.getElementById(
            "inspectionMessage"
        );


    saveInspection?.addEventListener(
        "click",
        () => {

            if (!currentIntake) {

                inspectionMessage.textContent =
                    "Complete Customer Intake before creating an inspection.";

                inspectionMessage.className =
                    "sd-form-message error";

                return;
            }


            const checkedCount =
                document.querySelectorAll(
                    ".inspection-check:checked"
                ).length;


            if (
                checkedCount !==
                inspectionChecks.length
            ) {

                inspectionMessage.textContent =
                    "Complete all walkaround checkpoints before saving.";

                inspectionMessage.className =
                    "sd-form-message error";

                return;
            }


            const inspectionNotes =
                document
                    .getElementById(
                        "inspectionNotes"
                    )
                    ?.value
                    .trim() || "";


            /*
                Prevent duplicate Job Cards.

                Once this intake already has a Job Card,
                reuse the same Job Card number instead of
                creating another record.
            */

            if (currentIntake.jobCardNumber) {

                inspectionMessage.textContent =
                    `Job Card ${currentIntake.jobCardNumber} has already been created for this intake.`;

                inspectionMessage.className =
                    "sd-form-message success";

                setTimeout(
                    () => {

                        showSection(
                            "job-cards"
                        );

                    },
                    500
                );

                return;
            }


            const jobCardNumber =
                generateJobCardNumber();


            currentIntake.jobCardNumber =
                jobCardNumber;


            addJobCard({
                jobCardNumber,
                vehicle:
                    `${currentIntake.vehicleMake} ${currentIntake.vehicleModel}`,
                plate:
                    currentIntake.vehiclePlate,
                service:
                    currentIntake.serviceConcern,
                inspectionNotes
            });


            inspectionMessage.textContent =
                `${jobCardNumber} created successfully.`;

            inspectionMessage.className =
                "sd-form-message success";


            setTimeout(
                () => {

                    showSection(
                        "job-cards"
                    );

                },
                650
            );

        }
    );



    /* =====================================================
       JOB CARDS
       ===================================================== */

    const jobCardTable =
        document.getElementById(
            "jobCardTable"
        );


    function addJobCard(job) {

        if (!jobCardTable) {
            return;
        }


        const row =
            document.createElement("tr");


        row.innerHTML = `

            <td>
                <strong>
                    ${escapeHTML(job.jobCardNumber)}
                </strong>
            </td>

            <td>
                ${escapeHTML(job.vehicle)}

                <small>
                    ${escapeHTML(job.plate)}
                </small>
            </td>

            <td>
                ${escapeHTML(job.service)}
            </td>

            <td>
                Service Advisor
            </td>

            <td>
                <span class="sd-status sd-status-progress">
                    Awaiting Diagnosis
                </span>
            </td>

            <td>
                <button
                    type="button"
                    class="sd-table-button"
                >
                    View
                </button>
            </td>
        `;


        jobCardTable.prepend(row);


        const viewButton =
            row.querySelector(
                ".sd-table-button"
            );


        viewButton?.addEventListener(
            "click",
            () => {

                showSection(
                    "diagnostics"
                );

            }
        );


        addDiagnosticJobOption(job);
    }


    function addDiagnosticJobOption(job) {

        const diagnosticJob =
            document.getElementById(
                "diagnosticJob"
            );


        if (!diagnosticJob) {
            return;
        }


        const option =
            document.createElement(
                "option"
            );


        option.value =
            job.jobCardNumber;


        option.textContent =
            `${job.jobCardNumber} · ${job.vehicle}`;


        diagnosticJob.prepend(option);

        diagnosticJob.value =
            job.jobCardNumber;
    }


    const newJobCardButton =
        document.getElementById(
            "newJobCardButton"
        );


    newJobCardButton?.addEventListener(
        "click",
        () => {

            showSection(
                "customer-intake"
            );

        }
    );


    document
        .querySelectorAll(".sd-table-button")
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    showSection(
                        "diagnostics"
                    );

                }
            );

        });



    /* =====================================================
       DIAGNOSTIC NOTES
       ===================================================== */

    const saveDiagnostic =
        document.getElementById(
            "saveDiagnostic"
        );

    const diagnosticNotes =
        document.getElementById(
            "diagnosticNotes"
        );

    const diagnosticMessage =
        document.getElementById(
            "diagnosticMessage"
        );


    saveDiagnostic?.addEventListener(
        "click",
        () => {

            const diagnosticJob =
                document.getElementById(
                    "diagnosticJob"
                );

            const diagnosticPriority =
                document.getElementById(
                    "diagnosticPriority"
                );


            const finding =
                diagnosticNotes
                    .value
                    .trim();


            if (!finding) {

                diagnosticMessage.textContent =
                    "Enter diagnostic findings before saving.";

                diagnosticMessage.className =
                    "sd-form-message error";

                diagnosticNotes.focus();

                return;
            }


            if (!diagnosticJob?.value) {

                diagnosticMessage.textContent =
                    "Select a Job Card before saving diagnostic notes.";

                diagnosticMessage.className =
                    "sd-form-message error";

                return;
            }


            currentDiagnostic = {

                jobCard:
                    diagnosticJob.value,

                jobLabel:
                    diagnosticJob
                        .options[
                            diagnosticJob.selectedIndex
                        ]
                        ?.textContent
                        ?.trim() || diagnosticJob.value,

                priority:
                    diagnosticPriority?.value ||
                    "Normal",

                finding:
                    finding
            };


            diagnosticMessage.textContent =
                `Diagnostic notes saved for ${currentDiagnostic.jobCard}.`;

            diagnosticMessage.className =
                "sd-form-message success";


            /*
                FRONTEND WORKFLOW:

                Job Card
                    ↓
                Diagnostic Finding
                    ↓
                Customer Estimate

                Real system:
                this record will be saved through
                the .NET API/database.
            */


            let prepareEstimateButton =
                document.getElementById(
                    "prepareEstimateFromDiagnostic"
                );


            if (!prepareEstimateButton) {

                prepareEstimateButton =
                    document.createElement(
                        "button"
                    );


                prepareEstimateButton.type =
                    "button";


                prepareEstimateButton.id =
                    "prepareEstimateFromDiagnostic";


                prepareEstimateButton.className =
                    "sd-primary-button";


                prepareEstimateButton.innerHTML =
                    '<i class="bi bi-receipt"></i> Prepare Customer Estimate';


                const diagnosticPanel =
                    saveDiagnostic.closest(
                        ".sd-panel"
                    );


                const formActions =
                    saveDiagnostic.closest(
                        ".sd-form-actions"
                    );


                if (formActions) {

                    formActions.appendChild(
                        prepareEstimateButton
                    );

                } else if (diagnosticPanel) {

                    diagnosticPanel.appendChild(
                        prepareEstimateButton
                    );
                }


                prepareEstimateButton.addEventListener(
                    "click",
                    () => {

                        prepareEstimateFromDiagnostic();

                    }
                );
            }

        }
    );


    function prepareEstimateFromDiagnostic() {

        if (!currentDiagnostic) {

            diagnosticMessage.textContent =
                "Save diagnostic notes before preparing an estimate.";

            diagnosticMessage.className =
                "sd-form-message error";

            return;
        }


        const estimateDescription =
            document.getElementById(
                "estimateDescription"
            );


        if (estimateDescription) {

            estimateDescription.value =
                `Job Card: ${currentDiagnostic.jobCard}

Diagnostic Priority: ${currentDiagnostic.priority}

Diagnostic Finding:
${currentDiagnostic.finding}

Recommended Work:
`;
        }


        const estimateMessage =
            document.getElementById(
                "estimateMessage"
            );


        if (estimateMessage) {

            estimateMessage.textContent =
                `Preparing estimate for ${currentDiagnostic.jobCard}.`;

            estimateMessage.className =
                "sd-form-message success";
        }


        showSection(
            "estimates"
        );
    }



    /* =====================================================
       ESTIMATE CALCULATOR
       ===================================================== */

    const estimateLabour =
        document.getElementById(
            "estimateLabour"
        );

    const estimateParts =
        document.getElementById(
            "estimateParts"
        );

    const estimateVendor =
        document.getElementById(
            "estimateVendor"
        );

    const estimateOther =
        document.getElementById(
            "estimateOther"
        );


    function numberValue(input) {

        return Number(
            input?.value || 0
        );

    }


    function formatLKR(value) {

        return (
            "LKR " +
            Number(value)
                .toLocaleString("en-LK")
        );

    }


    function updateEstimate() {

        const labour =
            numberValue(
                estimateLabour
            );

        const parts =
            numberValue(
                estimateParts
            );

        const vendor =
            numberValue(
                estimateVendor
            );

        const other =
            numberValue(
                estimateOther
            );


        const total =
            labour +
            parts +
            vendor +
            other;


        const labourDisplay =
            document.getElementById(
                "labourDisplay"
            );

        const partsDisplay =
            document.getElementById(
                "partsDisplay"
            );

        const vendorDisplay =
            document.getElementById(
                "vendorDisplay"
            );

        const otherDisplay =
            document.getElementById(
                "otherDisplay"
            );

        const estimateTotal =
            document.getElementById(
                "estimateTotal"
            );


        if (labourDisplay) {
            labourDisplay.textContent =
                formatLKR(labour);
        }

        if (partsDisplay) {
            partsDisplay.textContent =
                formatLKR(parts);
        }

        if (vendorDisplay) {
            vendorDisplay.textContent =
                formatLKR(vendor);
        }

        if (otherDisplay) {
            otherDisplay.textContent =
                formatLKR(other);
        }

        if (estimateTotal) {
            estimateTotal.textContent =
                formatLKR(total);
        }
    }


    [
        estimateLabour,
        estimateParts,
        estimateVendor,
        estimateOther

    ].forEach(input => {

        input?.addEventListener(
            "input",
            updateEstimate
        );

    });


    const sendEstimate =
        document.getElementById(
            "sendEstimate"
        );

    const estimateMessage =
        document.getElementById(
            "estimateMessage"
        );


    sendEstimate?.addEventListener(
        "click",
        () => {

            const labour =
                numberValue(estimateLabour);

            const parts =
                numberValue(estimateParts);

            const vendor =
                numberValue(estimateVendor);

            const other =
                numberValue(estimateOther);

            const total =
                labour +
                parts +
                vendor +
                other;


            currentEstimate = {

                jobCard:
                    currentDiagnostic?.jobCard ||
                    "Demo Job",

                description:
                    document
                        .getElementById("estimateDescription")
                        ?.value
                        .trim() || "",

                labour,
                parts,
                vendor,
                other,
                total,

                status:
                    "Awaiting Customer Approval"
            };


            estimateMessage.innerHTML = `
                Estimate sent successfully.
                <br>
                <strong>Status:</strong>
                Awaiting Customer Approval
            `;

            estimateMessage.className =
                "sd-form-message success";



        }
    );


    function approveEstimate() {

        if (!currentEstimate) {
            return;
        }


        currentEstimate.status =
            "Approved";


        estimateMessage.innerHTML = `
            Customer approved this estimate.
            <br>
            <strong>Status:</strong>
            Approved
        `;

        estimateMessage.className =
            "sd-form-message success";


        populateInvoiceFromEstimate();


        setTimeout(
            () => {

                showSection(
                    "invoice"
                );

            },
            650
        );
    }



    function populateInvoiceFromEstimate() {

        if (!currentEstimate) {
            return;
        }


        const invoiceTable =
            document.querySelector(
                "#invoice .sd-table tbody"
            );


        if (!invoiceTable) {
            return;
        }


        invoiceTable.innerHTML = `

            <tr>
                <td>
                    Labour Charges
                </td>

                <td>
                    Labour
                </td>

                <td>
                    1
                </td>

                <td>
                    ${formatLKR(currentEstimate.labour)}
                </td>
            </tr>


            <tr>
                <td>
                    In-stock Parts
                </td>

                <td>
                    Part
                </td>

                <td>
                    1
                </td>

                <td>
                    ${formatLKR(currentEstimate.parts)}
                </td>
            </tr>


            <tr>
                <td>
                    External Vendor Charges
                </td>

                <td>
                    Vendor
                </td>

                <td>
                    1
                </td>

                <td>
                    ${formatLKR(currentEstimate.vendor)}
                </td>
            </tr>


            <tr>
                <td>
                    Other Charges
                </td>

                <td>
                    Other
                </td>

                <td>
                    1
                </td>

                <td>
                    ${formatLKR(currentEstimate.other)}
                </td>
            </tr>
        `;


        const invoiceTotal =
            document.querySelector(
                ".sd-invoice-total strong"
            );


        if (invoiceTotal) {

            invoiceTotal.textContent =
                formatLKR(
                    currentEstimate.total
                );
        }


        const finalizeInvoice =
            document.getElementById(
                "finalizeInvoice"
            );


        if (finalizeInvoice) {

            finalizeInvoice.disabled =
                currentEstimate.status !==
                "Approved";
        }
    }


    /*
        DEVELOPMENT TEST ONLY

        Ctrl + Shift + A simulates the customer approval
        response while the backend is not connected.

        This shortcut must be removed when the .NET API
        provides the real customer approval status.
    */

    document.addEventListener(
        "keydown",
        event => {

            if (
                event.ctrlKey &&
                event.shiftKey &&
                event.key.toLowerCase() === "a"
            ) {

                if (
                    currentEstimate &&
                    currentEstimate.status ===
                    "Awaiting Customer Approval"
                ) {

                    approveEstimate();
                }
            }

        }
    );


    /* =====================================================
       INVOICE
       ===================================================== */

    const finalizeInvoice =
        document.getElementById(
            "finalizeInvoice"
        );

    const invoiceMessage =
        document.getElementById(
            "invoiceMessage"
        );


    finalizeInvoice?.addEventListener(
        "click",
        () => {

            if (
                !currentEstimate ||
                currentEstimate.status !==
                "Approved"
            ) {

                invoiceMessage.textContent =
                    "Customer approval is required before finalizing the invoice.";

                invoiceMessage.className =
                    "sd-form-message error";

                return;
            }


            invoiceFinalized = true;


            invoiceMessage.textContent =
                `Invoice finalized successfully for ${formatLKR(currentEstimate.total)}. Vehicle is ready for final handover checks.`;

            invoiceMessage.className =
                "sd-form-message success";


            prepareFinalHandover();


            setTimeout(
                () => {

                    /*
                        Use the dashboard's existing navigation
                        handler instead of changing the section
                        directly.
                    */

                    const handoverNav =
                        document.querySelector(
                            '[data-section="handover"]'
                        );


                    if (handoverNav) {

                        handoverNav.click();

                    } else {

                        showSection(
                            "handover"
                        );
                    }

                },
                650
            );

        }
    );


    function prepareFinalHandover() {

        const handoverMessage =
            document.getElementById(
                "handoverMessage"
            );


        if (handoverMessage) {

            handoverMessage.textContent =
                "Invoice finalized. Complete all handover checks before releasing the vehicle.";

            handoverMessage.className =
                "sd-form-message success";
        }


        /*
            Reset the handover checklist whenever a newly
            finalized invoice enters the handover stage.
        */

        handoverChecks.forEach(
            checkbox => {

                checkbox.checked = false;

            }
        );


        if (completeHandover) {

            completeHandover.disabled =
                true;
        }


        updateHandoverState();
    }



    /* =====================================================
       FINAL HANDOVER
       ===================================================== */

    const handoverChecks =
        document.querySelectorAll(
            ".handover-check"
        );

    const completeHandover =
        document.getElementById(
            "completeHandover"
        );

    const handoverMessage =
        document.getElementById(
            "handoverMessage"
        );


    function updateHandoverState() {

        const checked =
            document.querySelectorAll(
                ".handover-check:checked"
            ).length;


        if (completeHandover) {

            completeHandover.disabled =
                checked !==
                handoverChecks.length;
        }
    }


    handoverChecks.forEach(check => {

        check.addEventListener(
            "change",
            updateHandoverState
        );

    });


    completeHandover?.addEventListener(
        "click",
        () => {

            if (!invoiceFinalized) {

                handoverMessage.textContent =
                    "Finalize the approved invoice before completing vehicle handover.";

                handoverMessage.className =
                    "sd-form-message error";

                return;
            }


            const allChecksCompleted =
                [...handoverChecks].every(
                    checkbox =>
                        checkbox.checked
                );


            if (!allChecksCompleted) {

                handoverMessage.textContent =
                    "Complete all final handover checks before releasing the vehicle.";

                handoverMessage.className =
                    "sd-form-message error";

                return;
            }


            handoverMessage.textContent =
                "Vehicle handover completed successfully. Job workflow is complete.";

            handoverMessage.className =
                "sd-form-message success";


            completeHandover.disabled =
                true;


            invoiceFinalized =
                false;

        }
    );



    /* =====================================================
       INITIAL STATE
       ===================================================== */

    updateInspectionProgress();

    updateEstimate();

    updateHandoverState();

});







