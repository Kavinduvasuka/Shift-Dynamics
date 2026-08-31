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

    }
);

