document.addEventListener("DOMContentLoaded", () => {

    /* =========================================================
       NAVIGATION
    ========================================================= */

    const navLinks =
        document.querySelectorAll(".sd-nav-link");

    const sections =
        document.querySelectorAll(".sd-content-section");

    const pageTitle =
        document.getElementById("pageTitle");

    const sidebar =
        document.getElementById("sidebar");

    const sidebarOverlay =
        document.getElementById("sidebarOverlay");

    const menuButton =
        document.getElementById("menuButton");

    const sidebarClose =
        document.getElementById("sidebarClose");


    const titles = {
        overview: "Storekeeper Overview",
        requisitions: "Pending Requisitions",
        inventory: "Inventory Stock",
        "vendor-requests": "Vendor Requests",
        movements: "Stock Movements"
    };


    function openSection(sectionId) {

        sections.forEach(section => {
            section.classList.toggle(
                "active",
                section.id === sectionId
            );
        });


        navLinks.forEach(link => {
            link.classList.toggle(
                "active",
                link.dataset.section === sectionId
            );
        });


        pageTitle.textContent =
            titles[sectionId] || "Storekeeper Dashboard";


        sidebar.classList.remove("open");
        sidebarOverlay.classList.remove("show");

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    }


    navLinks.forEach(link => {

        link.addEventListener("click", () => {

            openSection(
                link.dataset.section
            );

        });

    });


    document
        .querySelectorAll("[data-go-section]")
        .forEach(button => {

            button.addEventListener("click", () => {

                openSection(
                    button.dataset.goSection
                );

            });

        });


    menuButton.addEventListener("click", () => {

        sidebar.classList.add("open");
        sidebarOverlay.classList.add("show");

    });


    sidebarClose.addEventListener("click", () => {

        sidebar.classList.remove("open");
        sidebarOverlay.classList.remove("show");

    });


    sidebarOverlay.addEventListener("click", () => {

        sidebar.classList.remove("open");
        sidebarOverlay.classList.remove("show");

    });



    /* =========================================================
       DEMO INVENTORY DATA
    ========================================================= */

    const inventory = [

        {
            number: "PT-ENG-001",
            part: "Engine Mount",
            category: "Engine",
            location: "Rack A-03",
            quantity: 3,
            status: "available"
        },

        {
            number: "PT-BRK-002",
            part: "Front Brake Pad Set",
            category: "Brakes",
            location: "Rack B-02",
            quantity: 8,
            status: "available"
        },

        {
            number: "PT-SUS-003",
            part: "Shock Absorber",
            category: "Suspension",
            location: "Rack C-04",
            quantity: 2,
            status: "low"
        },

        {
            number: "PT-ELC-004",
            part: "12V Car Battery",
            category: "Electrical",
            location: "Battery Zone",
            quantity: 4,
            status: "available"
        },

        {
            number: "PT-FLT-005",
            part: "Engine Oil Filter",
            category: "Filters",
            location: "Rack D-01",
            quantity: 14,
            status: "available"
        },

        {
            number: "PT-CLG-006",
            part: "Radiator Hose",
            category: "Cooling",
            location: "Rack A-06",
            quantity: 1,
            status: "low"
        },

        {
            number: "PT-BDY-007",
            part: "Headlamp Assembly",
            category: "Body",
            location: "Rack E-02",
            quantity: 0,
            status: "out"
        },

        {
            number: "PT-WHL-008",
            part: "17-inch Alloy Wheel",
            category: "Wheels",
            location: "Wheel Zone",
            quantity: 0,
            status: "out"
        }

    ];



    /* =========================================================
       MECHANIC REQUISITIONS
    ========================================================= */

    const requisitions = [

        {
            id: "PR-3024",
            job: "JC-1052",
            mechanic: "Nimal Perera",
            vehicle: "Nissan X-Trail",
            partNumber: "PT-ENG-001",
            part: "Engine Mount",
            quantity: 1,
            urgency: "Urgent",
            status: "pending"
        },

        {
            id: "PR-3025",
            job: "JC-1058",
            mechanic: "Kasun Silva",
            vehicle: "Honda Vezel",
            partNumber: "PT-ELC-004",
            part: "12V Car Battery",
            quantity: 1,
            urgency: "Normal",
            status: "pending"
        },

        {
            id: "PR-3026",
            job: "JC-1057",
            mechanic: "Dilan Jayasinghe",
            vehicle: "Toyota Aqua",
            partNumber: "PT-FLT-005",
            part: "Engine Oil Filter",
            quantity: 2,
            urgency: "Normal",
            status: "pending"
        }

    ];



    /* =========================================================
       VENDOR REQUESTS
    ========================================================= */

    const vendorRequests = [

        {
            id: "VR-4018",
            job: "JC-1048",
            part: "Headlamp Assembly",
            partNumber: "PT-BDY-007",
            quantity: 1,
            vehicle: "Toyota Corolla",
            status: "Awaiting Quotes"
        },

        {
            id: "VR-4019",
            job: "JC-1050",
            part: "17-inch Alloy Wheel",
            partNumber: "PT-WHL-008",
            quantity: 2,
            vehicle: "Honda Vezel",
            status: "Awaiting Quotes"
        }

    ];



    /* =========================================================
       STOCK MOVEMENTS
    ========================================================= */

    const stockMovements = [

        {
            time: "Today · 09:12 AM",
            part: "Front Brake Pad Set",
            reference: "JC-1051",
            movement: "Released",
            quantity: "-1",
            user: "Store Keeper"
        },

        {
            time: "Today · 08:45 AM",
            part: "Engine Oil Filter",
            reference: "STK-8821",
            movement: "Stock In",
            quantity: "+10",
            user: "Store Keeper"
        },

        {
            time: "31 Aug · 04:20 PM",
            part: "Radiator Hose",
            reference: "JC-1047",
            movement: "Released",
            quantity: "-1",
            user: "Store Keeper"
        }

    ];



    /* =========================================================
       SECURITY HELPER

       Dynamic values inserted with innerHTML are escaped first.
    ========================================================= */

    function escapeHTML(value) {

        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");

    }



    /* =========================================================
       INVENTORY RENDERING
    ========================================================= */

    const inventoryTableBody =
        document.getElementById("inventoryTableBody");

    const inventorySearch =
        document.getElementById("inventorySearch");

    const inventoryFilter =
        document.getElementById("inventoryFilter");


    function getStatusLabel(status) {

        if (status === "available") {
            return "Available";
        }

        if (status === "low") {
            return "Low Stock";
        }

        return "Out of Stock";
    }


    function renderInventory() {

        const search =
            inventorySearch.value
                .trim()
                .toLowerCase();

        const filter =
            inventoryFilter.value;


        const filtered =
            inventory.filter(item => {

                const searchable =
                    `${item.number} ${item.part} ${item.category}`
                        .toLowerCase();

                const searchMatch =
                    searchable.includes(search);

                const filterMatch =
                    filter === "all" ||
                    item.status === filter;

                return searchMatch && filterMatch;

            });


        inventoryTableBody.innerHTML =
            filtered.map(item => `

                <tr>

                    <td>
                        <strong>
                            ${escapeHTML(item.number)}
                        </strong>
                    </td>

                    <td>
                        ${escapeHTML(item.part)}
                    </td>

                    <td>
                        ${escapeHTML(item.category)}
                    </td>

                    <td>
                        ${escapeHTML(item.location)}
                    </td>

                    <td>
                        <strong>
                            ${escapeHTML(item.quantity)}
                        </strong>
                    </td>

                    <td>
                        <span class="sd-badge ${escapeHTML(item.status)}">
                            ${escapeHTML(getStatusLabel(item.status))}
                        </span>
                    </td>

                </tr>

            `).join("");


        if (!filtered.length) {

            inventoryTableBody.innerHTML = `

                <tr>
                    <td colspan="6">
                        No matching inventory items found.
                    </td>
                </tr>

            `;

        }

    }


    inventorySearch.addEventListener(
        "input",
        renderInventory
    );


    inventoryFilter.addEventListener(
        "change",
        renderInventory
    );



    /* =========================================================
       REQUISITION RENDERING
    ========================================================= */

    const requisitionList =
        document.getElementById("requisitionList");


    function renderRequisitions() {

        requisitionList.innerHTML =
            requisitions.map(request => {

                const inventoryItem =
                    inventory.find(
                        item =>
                            item.number === request.partNumber
                    );


                const available =
                    inventoryItem &&
                    inventoryItem.quantity >= request.quantity;


                return `

                    <article class="sd-requisition-card">

                        <div class="sd-card-top">

                            <div>

                                <span class="sd-eyebrow">
                                    ${escapeHTML(request.id)}
                                </span>

                                <h3>
                                    ${escapeHTML(request.part)}
                                </h3>

                                <p>
                                    Requested by
                                    ${escapeHTML(request.mechanic)}
                                </p>

                            </div>

                            <span class="sd-badge ${
                                request.urgency === "Urgent"
                                    ? "high"
                                    : "normal"
                            }">
                                ${escapeHTML(request.urgency)}
                            </span>

                        </div>


                        <div class="sd-card-data">

                            <div class="sd-data-box">
                                <span>Job Card</span>
                                <strong>
                                    #${escapeHTML(request.job)}
                                </strong>
                            </div>

                            <div class="sd-data-box">
                                <span>Vehicle</span>
                                <strong>
                                    ${escapeHTML(request.vehicle)}
                                </strong>
                            </div>

                            <div class="sd-data-box">
                                <span>Part Number</span>
                                <strong>
                                    ${escapeHTML(request.partNumber)}
                                </strong>
                            </div>

                            <div class="sd-data-box">
                                <span>Requested Qty</span>
                                <strong>
                                    ${escapeHTML(request.quantity)}
                                </strong>
                            </div>

                            <div class="sd-data-box">
                                <span>Available Stock</span>
                                <strong>
                                    ${
                                        inventoryItem
                                            ? escapeHTML(inventoryItem.quantity)
                                            : "0"
                                    }
                                </strong>
                            </div>

                        </div>


                        <div class="sd-card-actions">

                            <button
                                type="button"
                                class="sd-primary-btn"
                                data-release="${escapeHTML(request.id)}"
                                ${!available || request.status !== "pending"
                                    ? "disabled"
                                    : ""}
                            >
                                <i class="bi bi-box-arrow-up-right"></i>
                                Release Part
                            </button>


                            <button
                                type="button"
                                class="sd-danger-btn"
                                data-out-stock="${escapeHTML(request.id)}"
                                ${request.status !== "pending"
                                    ? "disabled"
                                    : ""}
                            >
                                <i class="bi bi-exclamation-circle"></i>
                                Out of Stock
                            </button>

                        </div>


                        <p
                            class="sd-card-message ${
                                request.status === "released"
                                    ? "success"
                                    : request.status === "vendor"
                                        ? "warning"
                                        : ""
                            }"
                        >

                            ${
                                request.status === "released"
                                    ? "Part released to mechanic."
                                    : request.status === "vendor"
                                        ? "Out of stock. Vendor request created."
                                        : available
                                            ? "Requested quantity is available in inventory."
                                            : "Requested quantity is not currently available."
                            }

                        </p>

                    </article>

                `;

            }).join("");


        attachRequisitionEvents();

    }



    function attachRequisitionEvents() {

        document
            .querySelectorAll("[data-release]")
            .forEach(button => {

                button.addEventListener(
                    "click",
                    () => releasePart(
                        button.dataset.release
                    )
                );

            });


        document
            .querySelectorAll("[data-out-stock]")
            .forEach(button => {

                button.addEventListener(
                    "click",
                    () => markOutOfStock(
                        button.dataset.outStock
                    )
                );

            });

    }



    /* =========================================================
       RELEASE PART
    ========================================================= */

    function releasePart(requestId) {

        const request =
            requisitions.find(
                item => item.id === requestId
            );


        if (!request || request.status !== "pending") {
            return;
        }


        const inventoryItem =
            inventory.find(
                item =>
                    item.number === request.partNumber
            );


        if (
            !inventoryItem ||
            inventoryItem.quantity < request.quantity
        ) {
            return;
        }


        inventoryItem.quantity -= request.quantity;


        if (inventoryItem.quantity === 0) {
            inventoryItem.status = "out";
        }
        else if (inventoryItem.quantity <= 2) {
            inventoryItem.status = "low";
        }
        else {
            inventoryItem.status = "available";
        }


        request.status = "released";


        stockMovements.unshift({

            time: "Just now",
            part: request.part,
            reference: request.job,
            movement: "Released",
            quantity: `-${request.quantity}`,
            user: "Store Keeper"

        });


        renderRequisitions();
        renderInventory();
        renderMovements();

    }



    /* =========================================================
       OUT OF STOCK -> VENDOR REQUEST
    ========================================================= */

    function markOutOfStock(requestId) {

        const request =
            requisitions.find(
                item => item.id === requestId
            );


        if (!request || request.status !== "pending") {
            return;
        }


        request.status = "vendor";


        const exists =
            vendorRequests.some(
                item =>
                    item.sourceRequest === request.id
            );


        if (!exists) {

            vendorRequests.unshift({

                id:
                    `VR-${4020 + vendorRequests.length}`,

                sourceRequest:
                    request.id,

                job:
                    request.job,

                part:
                    request.part,

                partNumber:
                    request.partNumber,

                quantity:
                    request.quantity,

                vehicle:
                    request.vehicle,

                status:
                    "Awaiting Quotes"

            });

        }


        renderRequisitions();
        renderVendorRequests();

    }



    /* =========================================================
       VENDOR REQUEST RENDERING
    ========================================================= */

    const vendorRequestList =
        document.getElementById("vendorRequestList");


    function renderVendorRequests() {

        vendorRequestList.innerHTML =
            vendorRequests.map(request => `

                <article class="sd-vendor-card">

                    <div class="sd-card-top">

                        <div>

                            <span class="sd-eyebrow">
                                ${escapeHTML(request.id)}
                            </span>

                            <h3>
                                ${escapeHTML(request.part)}
                            </h3>

                            <p>
                                External vendor sourcing request
                            </p>

                        </div>

                        <span class="sd-badge normal">
                            ${escapeHTML(request.status)}
                        </span>

                    </div>


                    <div class="sd-card-data">

                        <div class="sd-data-box">
                            <span>Job Card</span>
                            <strong>
                                #${escapeHTML(request.job)}
                            </strong>
                        </div>

                        <div class="sd-data-box">
                            <span>Vehicle</span>
                            <strong>
                                ${escapeHTML(request.vehicle)}
                            </strong>
                        </div>

                        <div class="sd-data-box">
                            <span>Part Number</span>
                            <strong>
                                ${escapeHTML(request.partNumber)}
                            </strong>
                        </div>

                        <div class="sd-data-box">
                            <span>Required Qty</span>
                            <strong>
                                ${escapeHTML(request.quantity)}
                            </strong>
                        </div>

                        <div class="sd-data-box">
                            <span>Status</span>
                            <strong>
                                ${escapeHTML(request.status)}
                            </strong>
                        </div>

                    </div>

                </article>

            `).join("");

    }



    /* =========================================================
       STOCK MOVEMENT RENDERING
    ========================================================= */

    const movementTableBody =
        document.getElementById("movementTableBody");


    function renderMovements() {

        movementTableBody.innerHTML =
            stockMovements.map(item => `

                <tr>

                    <td>
                        ${escapeHTML(item.time)}
                    </td>

                    <td>
                        <strong>
                            ${escapeHTML(item.part)}
                        </strong>
                    </td>

                    <td>
                        #${escapeHTML(item.reference)}
                    </td>

                    <td>
                        ${escapeHTML(item.movement)}
                    </td>

                    <td>
                        <strong>
                            ${escapeHTML(item.quantity)}
                        </strong>
                    </td>

                    <td>
                        ${escapeHTML(item.user)}
                    </td>

                </tr>

            `).join("");

    }



    /* =========================================================
       INITIAL RENDER
    ========================================================= */

    renderInventory();
    renderRequisitions();
    renderVendorRequests();
    renderMovements();

});
