document.addEventListener("DOMContentLoaded", () => {

    /* =====================================================
       ELEMENTS
       ===================================================== */

    const sidebar =
        document.getElementById("sidebar");

    const menuButton =
        document.getElementById("menuButton");

    const sidebarClose =
        document.getElementById("sidebarClose");

    const sidebarOverlay =
        document.getElementById("sidebarOverlay");

    const navItems =
        document.querySelectorAll(".sd-nav-item");

    const sections =
        document.querySelectorAll(".sd-dashboard-section");

    const shortcuts =
        document.querySelectorAll(".sd-section-shortcut");

    const pageTitle =
        document.getElementById("pageTitle");

    const pageSubtitle =
        document.getElementById("pageSubtitle");


    /* =====================================================
       SECTION INFORMATION
       ===================================================== */

    const sectionInfo = {

        overview: {
            title: "Dashboard Overview",
            subtitle: "Welcome back to Shift Dynamics."
        },

        vehicles: {
            title: "My Vehicles",
            subtitle: "Manage vehicles connected to your account."
        },

        booking: {
            title: "Service Booking",
            subtitle: "Request your next workshop appointment."
        },

        estimates: {
            title: "Digital Estimates",
            subtitle: "Review and approve service estimates."
        },

        tracker: {
            title: "Service Tracker",
            subtitle: "Follow your vehicle service progress."
        },

        modifications: {
            title: "Modification Catalog",
            subtitle: "Explore vehicle upgrade options."
        },

        payments: {
            title: "Payments & Invoices",
            subtitle: "Manage invoices and payment information."
        },

        profile: {
            title: "Profile Settings",
            subtitle: "Manage your account information."
        }

    };


    /* =====================================================
       SIDEBAR MOBILE
       ===================================================== */

    function openSidebar() {

        sidebar.classList.add("open");
        sidebarOverlay.classList.add("active");

        document.body.style.overflow =
            "hidden";
    }


    function closeSidebar() {

        sidebar.classList.remove("open");
        sidebarOverlay.classList.remove("active");

        document.body.style.overflow =
            "";
    }


    if (menuButton) {

        menuButton.addEventListener(
            "click",
            openSidebar
        );
    }


    if (sidebarClose) {

        sidebarClose.addEventListener(
            "click",
            closeSidebar
        );
    }


    if (sidebarOverlay) {

        sidebarOverlay.addEventListener(
            "click",
            closeSidebar
        );
    }


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


        const info =
            sectionInfo[sectionId];


        if (info) {

            pageTitle.textContent =
                info.title;

            pageSubtitle.textContent =
                info.subtitle;
        }


        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });


        if (
            window.innerWidth <= 900
        ) {
            closeSidebar();
        }
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


    shortcuts.forEach(button => {

        button.addEventListener(
            "click",
            () => {

                showSection(
                    button.dataset.target
                );

            }
        );

    });


    /* =====================================================
       VEHICLE MODAL
       ===================================================== */

    const addVehicleButton =
        document.getElementById(
            "addVehicleButton"
        );

    const vehicleModal =
        document.getElementById(
            "vehicleModal"
        );

    const vehicleModalClose =
        document.getElementById(
            "vehicleModalClose"
        );

    const modalCloseButtons =
        document.querySelectorAll(
            "[data-close-modal]"
        );


    function openVehicleModal() {

        vehicleModal.classList.add("open");

        vehicleModal.setAttribute(
            "aria-hidden",
            "false"
        );

        document.body.style.overflow =
            "hidden";
    }


    function closeVehicleModal() {

    vehicleModal.classList.remove(
        "open"
    );

    vehicleModal.setAttribute(
        "aria-hidden",
        "true"
    );

    document.body.style.overflow =
        "";

    resetVehicleForm();
    }


    if (addVehicleButton) {

    addVehicleButton.addEventListener(
        "click",
        () => {

            resetVehicleForm();
            openVehicleModal();

        }
    );
    }   


    if (vehicleModalClose) {

        vehicleModalClose.addEventListener(
            "click",
            closeVehicleModal
        );
    }


    modalCloseButtons.forEach(button => {

        button.addEventListener(
            "click",
            closeVehicleModal
        );

    });


    document.addEventListener(
        "keydown",
        event => {

            if (
                event.key === "Escape" &&
                vehicleModal.classList.contains(
                    "open"
                )
            ) {

                closeVehicleModal();

            }

        }
    );
/* =====================================================
   VEHICLE MANAGEMENT
   ===================================================== */

const vehicleForm =
    document.getElementById("vehicleForm");

const vehicleGrid =
    document.getElementById("vehicleGrid");

const vehicleSubmitText =
    document.getElementById("vehicleSubmitText");


let editingVehicleCard = null;


/* =====================================================
   VEHICLE FORM SUBMIT
   ===================================================== */

if (vehicleForm) {

    vehicleForm.addEventListener(
        "submit",
        event => {

            event.preventDefault();


            const make =
                document
                    .getElementById("vehicleMake")
                    .value
                    .trim();

            const model =
                document
                    .getElementById("vehicleModel")
                    .value
                    .trim();

            const year =
                document
                    .getElementById("vehicleYear")
                    .value
                    .trim();

            const plate =
                document
                    .getElementById("vehiclePlate")
                    .value
                    .trim();

            const trim =
                document
                    .getElementById("vehicleTrim")
                    .value
                    .trim();

            const engine =
                document
                    .getElementById("vehicleEngine")
                    .value
                    .trim();

            const vin =
                document
                    .getElementById("vehicleVin")
                    .value
                    .trim();


            /* -----------------------------------------
               BASIC VALIDATION
               ----------------------------------------- */

            if (!make || !model || !year || !plate) {

                alert(
                    "Please enter Make, Model, Year and License Plate."
                );

                return;
            }


            const numericYear =
                Number(year);

            const currentYear =
                new Date().getFullYear();


            if (
                numericYear < 1950 ||
                numericYear > currentYear + 1
            ) {

                alert(
                    "Please enter a valid vehicle year."
                );

                return;
            }


            /* -----------------------------------------
               EDIT EXISTING VEHICLE
               ----------------------------------------- */

            if (editingVehicleCard) {

                updateVehicleCard(
                    editingVehicleCard,
                    {
                        make,
                        model,
                        year,
                        plate,
                        trim,
                        engine,
                        vin
                    }
                );


                editingVehicleCard = null;


                if (vehicleSubmitText) {
                    vehicleSubmitText.textContent =
                        "Add Vehicle";
                }


                vehicleForm.reset();

                closeVehicleModal();

                return;
            }


            /* -----------------------------------------
               CREATE NEW VEHICLE
               ----------------------------------------- */

            const card =
                createVehicleCard({
                    make,
                    model,
                    year,
                    plate,
                    trim,
                    engine,
                    vin
                });


            vehicleGrid.appendChild(card);


            vehicleForm.reset();

            closeVehicleModal();

        }
    );
}


/* =====================================================
   CREATE VEHICLE CARD
   ===================================================== */

function createVehicleCard(vehicle) {

    const card =
        document.createElement("article");


    card.className =
        "sd-vehicle-card";


    updateVehicleCard(
        card,
        vehicle
    );


    return card;
}


/* =====================================================
   UPDATE VEHICLE CARD
   ===================================================== */

function updateVehicleCard(
    card,
    vehicle
) {

    card.dataset.make =
        vehicle.make;

    card.dataset.model =
        vehicle.model;

    card.dataset.year =
        vehicle.year;

    card.dataset.plate =
        vehicle.plate;

    card.dataset.trim =
        vehicle.trim || "";

    card.dataset.engine =
        vehicle.engine || "";

    card.dataset.vin =
        vehicle.vin || "";


    card.innerHTML = `

        <div class="sd-vehicle-top">

            <div class="sd-vehicle-icon">
                <i class="bi bi-car-front-fill"></i>
            </div>

            <span class="sd-status-badge active">
                Vehicle
            </span>

        </div>


        <h3>
            ${escapeHTML(vehicle.make)}
            ${escapeHTML(vehicle.model)}
        </h3>


        <p>
            ${escapeHTML(vehicle.year)}
            ${vehicle.trim
                ? " • " + escapeHTML(vehicle.trim)
                : ""}
        </p>


        <div class="sd-vehicle-details">

            <div>
                <span>License Plate</span>

                <strong>
                    ${escapeHTML(vehicle.plate)}
                </strong>
            </div>


            <div>
                <span>Engine</span>

                <strong>
                    ${escapeHTML(
                        vehicle.engine ||
                        "Not specified"
                    )}
                </strong>
            </div>


            <div>
                <span>VIN</span>

                <strong>
                    ${escapeHTML(
                        vehicle.vin ||
                        "Not specified"
                    )}
                </strong>
            </div>


            <div>
                <span>Year</span>

                <strong>
                    ${escapeHTML(vehicle.year)}
                </strong>
            </div>

        </div>


        <div class="sd-vehicle-actions">

            <button
                class="sd-secondary-button sd-edit-vehicle"
                type="button"
            >
                <i class="bi bi-pencil"></i>
                Edit
            </button>


            <button
                class="sd-delete-button sd-delete-vehicle"
                type="button"
            >
                <i class="bi bi-trash3"></i>
                Delete
            </button>

        </div>
    `;

}


/* =====================================================
   VEHICLE CARD ACTIONS
   ===================================================== */

if (vehicleGrid) {

    vehicleGrid.addEventListener(
        "click",
        event => {

            const editButton =
                event.target.closest(
                    ".sd-edit-vehicle"
                );


            const deleteButton =
                event.target.closest(
                    ".sd-delete-vehicle"
                );


            /* -----------------------------------------
               EDIT
               ----------------------------------------- */

            if (editButton) {

                const card =
                    editButton.closest(
                        ".sd-vehicle-card"
                    );


                startVehicleEdit(card);

                return;
            }


            /* -----------------------------------------
               DELETE
               ----------------------------------------- */

            if (deleteButton) {

                const card =
                    deleteButton.closest(
                        ".sd-vehicle-card"
                    );


                const vehicleName =
                    `${card.dataset.make || "Vehicle"} ${card.dataset.model || ""}`;


                const confirmed =
                    window.confirm(
                        `Delete ${vehicleName.trim()}?`
                    );


                if (!confirmed) {
                    return;
                }


                card.remove();

            }

        }
    );
}


/* =====================================================
   START VEHICLE EDIT
   ===================================================== */

function startVehicleEdit(card) {

    editingVehicleCard =
        card;


    document.getElementById(
        "vehicleMake"
    ).value =
        card.dataset.make || "";


    document.getElementById(
        "vehicleModel"
    ).value =
        card.dataset.model || "";


    document.getElementById(
        "vehicleYear"
    ).value =
        card.dataset.year || "";


    document.getElementById(
        "vehiclePlate"
    ).value =
        card.dataset.plate || "";


    document.getElementById(
        "vehicleTrim"
    ).value =
        card.dataset.trim || "";


    document.getElementById(
        "vehicleEngine"
    ).value =
        card.dataset.engine || "";


    document.getElementById(
        "vehicleVin"
    ).value =
        card.dataset.vin || "";


    if (vehicleSubmitText) {

        vehicleSubmitText.textContent =
            "Save Changes";
    }


    openVehicleModal();

}


/* =====================================================
   RESET VEHICLE FORM
   ===================================================== */

function resetVehicleForm() {

    editingVehicleCard = null;


    if (vehicleForm) {
        vehicleForm.reset();
    }


    if (vehicleSubmitText) {

        vehicleSubmitText.textContent =
            "Add Vehicle";
    }

}


/* =====================================================
   ESCAPE USER CONTENT
   ===================================================== */

function escapeHTML(value) {

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");

}
    /* =====================================================
       SERVICE BOOKING
       ===================================================== */

    const serviceBookingForm =
        document.getElementById("serviceBookingForm");

    const bookingVehicle =
        document.getElementById("bookingVehicle");

    const serviceType =
        document.getElementById("serviceType");
    const servicePackageInfo =
        document.getElementById(
            "servicePackageInfo"
        );

    const servicePackageIcon =
        document.getElementById(
            "servicePackageIcon"
        );

    const servicePackageTitle =
        document.getElementById(
            "servicePackageTitle"
        );

    const servicePackageDescription =
        document.getElementById(
            "servicePackageDescription"
        );

    const servicePackageDuration =
        document.getElementById(
            "servicePackageDuration"
        );


    const servicePackages = {

        "General Service": {
            icon: "bi-wrench-adjustable",
            duration: "Approx. 1–2 hours",
            description:
                "Routine maintenance package covering essential service checks and general vehicle care."
        },

        "Vehicle Inspection": {
            icon: "bi-search",
            duration: "Approx. 45–90 minutes",
            description:
                "A detailed vehicle condition inspection covering major safety and mechanical areas."
        },

        "Engine Diagnostic": {
            icon: "bi-speedometer2",
            duration: "Approx. 1–2 hours",
            description:
                "Diagnostic assessment for engine warning lights, performance issues and related faults."
        },

        "Brake Service": {
            icon: "bi-disc",
            duration: "Approx. 1–2 hours",
            description:
                "Brake system inspection and service covering braking components and overall condition."
        },

        "Modification Consultation": {
            icon: "bi-tools",
            duration: "Approx. 30–60 minutes",
            description:
                "Consultation for vehicle upgrades such as exhausts, rims, tuning and other modifications."
        }

    };


    function updateServicePackageInfo() {

        if (
            !serviceType ||
            !servicePackageInfo
        ) {
            return;
        }


        const selectedPackage =
            servicePackages[
                serviceType.value
            ];


        if (!selectedPackage) {

            servicePackageInfo.hidden =
                true;

            return;
        }


        servicePackageInfo.hidden =
            false;


        if (servicePackageTitle) {

            servicePackageTitle.textContent =
                serviceType.value;
        }


        if (servicePackageDescription) {

            servicePackageDescription.textContent =
                selectedPackage.description;
        }


        if (servicePackageDuration) {

            servicePackageDuration.textContent =
                selectedPackage.duration;
        }


        if (servicePackageIcon) {

            servicePackageIcon.className =
                `bi ${selectedPackage.icon}`;
        }
    }


    if (serviceType) {

        serviceType.addEventListener(
            "change",
            updateServicePackageInfo
        );

    }
    const bookingDate =
        document.getElementById("bookingDate");

    const bookingTime =
        document.getElementById("bookingTime");

    const bookingNotes =
        document.getElementById("bookingNotes");

    const bookingMessage =
        document.getElementById("bookingMessage");


    /* =====================================================
       BOOKING SETTINGS
       ===================================================== */

    const workshopOpeningTime = "08:00";
    const workshopClosingTime = "17:00";


    /* =====================================================
       SET MINIMUM BOOKING DATE
       Prevent past date selection
       ===================================================== */

    if (bookingDate) {

        const today =
            new Date();

        const year =
            today.getFullYear();

        const month =
            String(
                today.getMonth() + 1
            ).padStart(2, "0");

        const day =
            String(
                today.getDate()
            ).padStart(2, "0");


        bookingDate.min =
            `${year}-${month}-${day}`;
    }


    /* =====================================================
       WORKSHOP TIME RANGE
       ===================================================== */

    if (bookingTime) {

        bookingTime.min =
            workshopOpeningTime;

        bookingTime.max =
            workshopClosingTime;
    }


    /* =====================================================
       UPDATE VEHICLE DROPDOWN
       Reads vehicles from My Vehicles cards
       ===================================================== */

    function updateBookingVehicleOptions() {

        if (!bookingVehicle) {
            return;
        }


        const currentValue =
            bookingVehicle.value;


        bookingVehicle.innerHTML = `
            <option value="">
                Select vehicle
            </option>
        `;


        const vehicleCards =
            document.querySelectorAll(
                "#vehicleGrid .sd-vehicle-card"
            );


        vehicleCards.forEach(card => {

            const make =
                card.dataset.make || "";

            const model =
                card.dataset.model || "";

            const plate =
                card.dataset.plate || "";


            if (!make || !model) {
                return;
            }


            const option =
                document.createElement("option");


            option.value =
                `${make} ${model}`;


            option.textContent =
                plate
                    ? `${make} ${model} - ${plate}`
                    : `${make} ${model}`;


            bookingVehicle.appendChild(
                option
            );

        });


        const existingOption =
            Array.from(
                bookingVehicle.options
            ).find(
                option =>
                    option.value === currentValue
            );


        if (existingOption) {

            bookingVehicle.value =
                currentValue;
        }
    }


    /* =====================================================
       INITIAL VEHICLE DROPDOWN
       ===================================================== */

    updateBookingVehicleOptions();


    /* =====================================================
       UPDATE DROPDOWN WHEN VEHICLES CHANGE
       ===================================================== */

    if (vehicleGrid) {

        const vehicleObserver =
            new MutationObserver(() => {

                updateBookingVehicleOptions();

            });


        vehicleObserver.observe(
            vehicleGrid,
            {
                childList: true,
                subtree: true
            }
        );
    }


    /* =====================================================
       MESSAGE HELPERS
       ===================================================== */

    function showBookingError(message) {

        if (!bookingMessage) {
            return;
        }


        bookingMessage.className =
            "sd-form-message";


        bookingMessage.style.display =
            "block";


        bookingMessage.style.background =
            "#FEF2F2";


        bookingMessage.style.border =
            "1px solid #FECACA";


        bookingMessage.style.color =
            "#B91C1C";


        bookingMessage.textContent =
            message;
    }


    function showBookingSuccess(message) {

        if (!bookingMessage) {
            return;
        }


        bookingMessage.removeAttribute(
            "style"
        );


        bookingMessage.className =
            "sd-form-message success";


        bookingMessage.textContent =
            message;
    }


    function clearBookingMessage() {

        if (!bookingMessage) {
            return;
        }


        bookingMessage.removeAttribute(
            "style"
        );


        bookingMessage.className =
            "sd-form-message";


        bookingMessage.textContent =
            "";
    }


    /* =====================================================
       VALIDATE BOOKING DATE
       ===================================================== */

    function isValidBookingDate(dateValue) {

        if (!dateValue) {
            return false;
        }


        const selectedDate =
            new Date(
                `${dateValue}T00:00:00`
            );


        const today =
            new Date();


        today.setHours(
            0,
            0,
            0,
            0
        );


        return selectedDate >= today;
    }


    /* =====================================================
       VALIDATE WORKSHOP TIME
       ===================================================== */

    function isValidWorkshopTime(
        timeValue
    ) {

        if (!timeValue) {
            return false;
        }


        return (
            timeValue >= workshopOpeningTime &&
            timeValue <= workshopClosingTime
        );
    }


    /* =====================================================
       FORMAT BOOKING DATE
       ===================================================== */

    function formatBookingDate(
        dateValue
    ) {

        const date =
            new Date(
                `${dateValue}T00:00:00`
            );


        return date.toLocaleDateString(
            "en-GB",
            {
                day: "2-digit",
                month: "short",
                year: "numeric"
            }
        );
    }


    /* =====================================================
       FORMAT BOOKING TIME
       ===================================================== */

    function formatBookingTime(
        timeValue
    ) {

        const [
            hours,
            minutes
        ] =
            timeValue
                .split(":")
                .map(Number);


        const date =
            new Date();


        date.setHours(
            hours,
            minutes,
            0,
            0
        );


        return date.toLocaleTimeString(
            "en-US",
            {
                hour: "numeric",
                minute: "2-digit"
            }
        );
    }


    /* =====================================================
       SERVICE BOOKING SUBMIT
       ===================================================== */

    if (serviceBookingForm) {

        serviceBookingForm.addEventListener(
            "submit",
            event => {

                event.preventDefault();

                clearBookingMessage();


                /* -----------------------------------------
                   GET VALUES
                   ----------------------------------------- */

                const selectedVehicle =
                    bookingVehicle
                        ? bookingVehicle.value.trim()
                        : "";


                const selectedService =
                    serviceType
                        ? serviceType.value.trim()
                        : "";


                const selectedDate =
                    bookingDate
                        ? bookingDate.value
                        : "";


                const selectedTime =
                    bookingTime
                        ? bookingTime.value
                        : "";


                const notes =
                    bookingNotes
                        ? bookingNotes.value.trim()
                        : "";


                /* -----------------------------------------
                   VEHICLE VALIDATION
                   ----------------------------------------- */

                if (!selectedVehicle) {

                    showBookingError(
                        "Please select a vehicle."
                    );

                    if (bookingVehicle) {
                        bookingVehicle.focus();
                    }

                    return;
                }


                /* -----------------------------------------
                   SERVICE VALIDATION
                   ----------------------------------------- */

                if (!selectedService) {

                    showBookingError(
                        "Please select a service type."
                    );

                    if (serviceType) {
                        serviceType.focus();
                    }

                    return;
                }


                /* -----------------------------------------
                   DATE VALIDATION
                   ----------------------------------------- */

                if (!selectedDate) {

                    showBookingError(
                        "Please select your preferred booking date."
                    );

                    if (bookingDate) {
                        bookingDate.focus();
                    }

                    return;
                }


                if (
                    !isValidBookingDate(
                        selectedDate
                    )
                ) {

                    showBookingError(
                        "Please select today or a future booking date."
                    );

                    if (bookingDate) {
                        bookingDate.focus();
                    }

                    return;
                }


                /* -----------------------------------------
                   TIME VALIDATION
                   ----------------------------------------- */

                if (!selectedTime) {

                    showBookingError(
                        "Please select your preferred booking time."
                    );

                    if (bookingTime) {
                        bookingTime.focus();
                    }

                    return;
                }


                if (
                    !isValidWorkshopTime(
                        selectedTime
                    )
                ) {

                    showBookingError(
                        "Please select a workshop time between 8:00 AM and 5:00 PM."
                    );

                    if (bookingTime) {
                        bookingTime.focus();
                    }

                    return;
                }


                /* -----------------------------------------
                   CREATE FRONTEND BOOKING DATA

                   Later this object can be sent
                   to the .NET backend API.
                   ----------------------------------------- */

                const bookingData = {

                    vehicle:
                        selectedVehicle,

                    service:
                        selectedService,

                    preferredDate:
                        selectedDate,

                    preferredTime:
                        selectedTime,

                    notes:
                        notes
                };


                /* -----------------------------------------
                   SUBMIT BUTTON LOADING STATE
                   ----------------------------------------- */

                const submitButton =
                    serviceBookingForm.querySelector(
                        'button[type="submit"]'
                    );


                const originalButtonHTML =
                    submitButton
                        ? submitButton.innerHTML
                        : "";


                if (submitButton) {

                    submitButton.disabled =
                        true;


                    submitButton.innerHTML = `
                        <span
                            class="spinner-border spinner-border-sm"
                            aria-hidden="true"
                        ></span>
                        Submitting...
                    `;
                }


                /* -----------------------------------------
                   FRONTEND DEMO SUBMIT
                   ----------------------------------------- */

                window.setTimeout(
                    () => {

                        const formattedDate =
                            formatBookingDate(
                                selectedDate
                            );


                        const formattedTime =
                            formatBookingTime(
                                selectedTime
                            );


                        showBookingSuccess(
                            `Booking request submitted successfully for ${selectedVehicle} — ${selectedService} on ${formattedDate} at ${formattedTime}.`
                        );


                        /*
                         * Backend integration later:
                         *
                         * fetch("/api/bookings", {
                         *     method: "POST",
                         *     headers: {
                         *         "Content-Type":
                         *             "application/json"
                         *     },
                         *     body:
                         *         JSON.stringify(
                         *             bookingData
                         *         )
                         * });
                         */


                        console.log(
                            "Frontend booking:",
                            bookingData
                        );


                        serviceBookingForm.reset();
                        if (servicePackageInfo) {
                            servicePackageInfo.hidden = true;
}

                        /* Reset minimum values
                           after form reset */

                        if (bookingTime) {

                            bookingTime.min =
                                workshopOpeningTime;

                            bookingTime.max =
                                workshopClosingTime;
                        }


                        updateBookingVehicleOptions();


                        if (submitButton) {

                            submitButton.disabled =
                                false;


                            submitButton.innerHTML =
                                originalButtonHTML;
                        }

                    },
                    700
                );

            }
        );

    }
    /* =====================================================
       ESTIMATE APPROVAL
       ===================================================== */

    const approveButtons =
        document.querySelectorAll(
            ".approve-estimate"
        );


    approveButtons.forEach(button => {

        button.addEventListener(
            "click",
            () => {

                const estimateCard =
                    button.closest(
                        ".sd-estimate-card"
                    );


                const status =
                    estimateCard.querySelector(
                        ".sd-status-badge"
                    );


                status.className =
                    "sd-status-badge paid";


                status.textContent =
                    "Approved";


                button.innerHTML = `
                    <i class="bi bi-check2-circle"></i>
                    Estimate Approved
                `;


                button.disabled = true;

            }
        );

    });


    /* =====================================================
       PROFILE
       ===================================================== */

    const profileForm =
        document.getElementById(
            "profileForm"
        );

    const profileMessage =
        document.getElementById(
            "profileMessage"
        );


    if (profileForm) {

        profileForm.addEventListener(
            "submit",
            event => {

                event.preventDefault();


                profileMessage.className =
                    "sd-form-message success";


                profileMessage.textContent =
                    "Profile changes saved in the frontend demo.";

            }
        );

    }

});