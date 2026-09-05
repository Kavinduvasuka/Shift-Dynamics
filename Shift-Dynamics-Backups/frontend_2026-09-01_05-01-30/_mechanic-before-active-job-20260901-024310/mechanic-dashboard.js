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

    }
);
