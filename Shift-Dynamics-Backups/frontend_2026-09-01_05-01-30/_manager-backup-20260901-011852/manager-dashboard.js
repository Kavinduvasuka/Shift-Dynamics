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

    }
);
