document.addEventListener("DOMContentLoaded", () => {

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
       SECTION NAVIGATION
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


            const customerName =
                document
                    .getElementById("customerName")
                    .value
                    .trim();

            const vehicleMake =
                document
                    .getElementById("vehicleMake")
                    .value
                    .trim();

            const vehicleModel =
                document
                    .getElementById("vehicleModel")
                    .value
                    .trim();

            const vehiclePlate =
                document
                    .getElementById("vehiclePlate")
                    .value
                    .trim();


            intakeMessage.textContent =
                `Intake completed for ${customerName} - ${vehicleMake} ${vehicleModel} (${vehiclePlate}).`;

            intakeMessage.className =
                "sd-form-message success";


            /*
                FRONTEND DEMO ONLY.

                Future .NET API should:

                1. Find/create customer
                2. Find/create vehicle
                3. Save customer complaint
                4. Generate official intake record
            */

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


            inspectionMessage.textContent =
                "Walkaround inspection completed successfully.";

            inspectionMessage.className =
                "sd-form-message success";

        }
    );



    /* =====================================================
       JOB CARDS
       ===================================================== */

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

            if (
                !diagnosticNotes
                    .value
                    .trim()
            ) {

                diagnosticMessage.textContent =
                    "Enter diagnostic findings before saving.";

                diagnosticMessage.className =
                    "sd-form-message error";

                diagnosticNotes.focus();

                return;
            }


            diagnosticMessage.textContent =
                "Diagnostic notes saved for this job card.";

            diagnosticMessage.className =
                "sd-form-message success";

        }
    );



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


        document.getElementById(
            "labourDisplay"
        ).textContent =
            formatLKR(labour);


        document.getElementById(
            "partsDisplay"
        ).textContent =
            formatLKR(parts);


        document.getElementById(
            "vendorDisplay"
        ).textContent =
            formatLKR(vendor);


        document.getElementById(
            "otherDisplay"
        ).textContent =
            formatLKR(other);


        document.getElementById(
            "estimateTotal"
        ).textContent =
            formatLKR(total);

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

            estimateMessage.textContent =
                "Estimate prepared and marked as awaiting customer approval.";

            estimateMessage.className =
                "sd-form-message success";


            /*
                Future backend flow:

                Advisor prepares estimate
                        ↓
                .NET API stores estimate
                        ↓
                Customer receives estimate
                        ↓
                Customer Approve / Request Changes
            */

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

            invoiceMessage.textContent =
                "Invoice finalized for frontend demonstration.";

            invoiceMessage.className =
                "sd-form-message success";

        }
    );



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

            handoverMessage.textContent =
                "Vehicle handover completed successfully.";

            handoverMessage.className =
                "sd-form-message success";

        }
    );


    /* =====================================================
       INITIAL STATE
       ===================================================== */

    updateInspectionProgress();
    updateEstimate();
    updateHandoverState();

});
