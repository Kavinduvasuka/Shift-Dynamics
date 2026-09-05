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
    }
);

