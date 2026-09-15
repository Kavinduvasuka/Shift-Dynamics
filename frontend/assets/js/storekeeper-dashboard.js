document.addEventListener("DOMContentLoaded", () => {

    // Navigation
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

    // Initial inventory catalogue. It is migrated to the shared store once.
    let inventory = [
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

    let requisitions = [];
    let vendorRequests = [];
    let vendorQuotes = [];
    let stockMovements = [];

    const workflowStore =
        window.ShiftDynamicsStore;

    function syncStorekeeperData() {
        if (!workflowStore) {
            inventory = [];
            requisitions = [];
            vendorRequests = [];
            vendorQuotes = [];
            stockMovements = [];
            return;
        }

        if (typeof workflowStore.getInventory === "function") {
            const savedInventory = workflowStore.getInventory();

            if (savedInventory.length) {
                inventory = savedInventory;
            } else if (typeof workflowStore.saveInventory === "function") {
                inventory = workflowStore.saveInventory(inventory);
            }
        }

        requisitions =
            typeof workflowStore.getPartRequests === "function"
                ? workflowStore.getPartRequests().map(request => ({
                    ...request,
                    id: request.requestId,
                    job: request.jobCardNumber,
                    partNumber:
                        request.inventoryNumber || request.partNumber
                }))
                : [];

        vendorRequests =
            typeof workflowStore.getVendorRequests === "function"
                ? workflowStore.getVendorRequests().map(request => ({
                    ...request,
                    id: request.vendorRequestId,
                    job: request.jobCardNumber,
                    sourceRequest: request.sourceRequestId
                }))
                : [];

        vendorQuotes =
            typeof workflowStore.getVendorQuotes === "function"
                ? workflowStore.getVendorQuotes()
                : [];

        stockMovements =
            typeof workflowStore.getStockMovements === "function"
                ? workflowStore.getStockMovements().map(movement => ({
                    ...movement,
                    time:
                        movement.time ||
                        new Date(movement.createdAt).toLocaleString(),
                    reference:
                        movement.reference ||
                        movement.jobCardNumber ||
                        "",
                    user: movement.user || "Store Keeper"
                }))
                : [];
    }

    // Security Helper
    function escapeHTML(value) {

        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    // Keep stored identifiers consistent when some records already include "#".
    function formatIdentifier(value) {
        const normalized = String(value || "")
            .trim()
            .replace(/^#+/, "");

        return normalized ? `#${normalized}` : "-";
    }

    // Inventory
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

    // Requisitions
    const requisitionList =
        document.getElementById("requisitionList");

    function renderRequisitions() {

        const pendingRequests =
            requisitions.filter(
                request => request.status === "pending"
            );

        if (!pendingRequests.length) {
            requisitionList.innerHTML = `
                <div class="sd-live-empty">
                    No pending parts requisitions.
                </div>
            `;
            return;
        }

        requisitionList.innerHTML =
            pendingRequests.map(request => {

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
                                    ${escapeHTML(formatIdentifier(request.job))}
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
                                ${
                                    !available ||
                                    request.status !== "pending"
                                        ? "disabled"
                                        : ""
                                }
                            >
                                <i class="bi bi-box-arrow-up-right"></i>
                                Release Part
                            </button>

                            <button
                                type="button"
                                class="sd-danger-btn"
                                data-out-stock="${escapeHTML(request.id)}"
                                ${
                                    request.status !== "pending"
                                        ? "disabled"
                                        : ""
                                }
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

    // Release Part
    function releasePart(requestId) {

        const request =
            requisitions.find(
                item => item.id === requestId
            );

        if (
            !request ||
            request.status !== "pending"
        ) {
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

        inventoryItem.quantity -=
            request.quantity;

        if (inventoryItem.quantity === 0) {
            inventoryItem.status = "out";

        } else if (inventoryItem.quantity <= 2) {
            inventoryItem.status = "low";

        } else {
            inventoryItem.status = "available";
        }

        request.status = "released";

        workflowStore.saveInventory(inventory);

        workflowStore.updatePartRequest(
            request.id,
            {
                status: "released",
                releasedAt: new Date().toISOString()
            }
        );

        workflowStore.createStockMovement({
            time: "Just now",
            part: request.part,
            reference: request.job,
            movement: "Released",
            quantity: `-${request.quantity}`,
            user: "Store Keeper"
        });

        syncStorekeeperData();

        renderRequisitions();
        renderInventory();
        renderMovements();
    }

    // Out of Stock / Vendor Request
    function markOutOfStock(requestId) {

        const request =
            requisitions.find(
                item => item.id === requestId
            );

        if (
            !request ||
            request.status !== "pending"
        ) {
            return;
        }

        request.status = "vendor";

        workflowStore.updatePartRequest(
            request.id,
            {
                status: "vendor",
                vendorRequestedAt: new Date().toISOString()
            }
        );

        const exists =
            vendorRequests.some(
                item =>
                    item.sourceRequest === request.id
            );

        if (!exists) {

            workflowStore.createVendorRequest({
                sourceRequestId:
                    request.id,

                jobCardNumber:
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

        syncStorekeeperData();

        renderRequisitions();
        renderVendorRequests();
    }

    // Vendor Requests Rendering
    const vendorRequestList =
        document.getElementById("vendorRequestList");

    function renderVendorRequests() {

        if (!vendorRequests.length) {
            vendorRequestList.innerHTML = `
                <div class="sd-live-empty">
                    No vendor requests available.
                </div>
            `;
            return;
        }

        vendorRequestList.innerHTML =
            vendorRequests.map(request => {
                const quote = vendorQuotes.find(
                    item =>
                        item.vendorRequestId === request.id ||
                        item.quoteId === request.quoteId
                );

                const canReview =
                    quote?.status === "Pending Review";

                return `
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
                                ${escapeHTML(formatIdentifier(request.job))}
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

                    ${quote ? `
                        <div class="sd-card-data" style="margin-top: 12px;">
                            <div class="sd-data-box">
                                <span>Quotation</span>
                                <strong>${escapeHTML(quote.quoteId)}</strong>
                            </div>

                            <div class="sd-data-box">
                                <span>Unit Price</span>
                                <strong>${escapeHTML(
                                    new Intl.NumberFormat(
                                        "en-LK",
                                        {
                                            style: "currency",
                                            currency: "LKR",
                                            maximumFractionDigits: 0
                                        }
                                    ).format(quote.price)
                                )}</strong>
                            </div>

                            <div class="sd-data-box">
                                <span>Delivery</span>
                                <strong>${escapeHTML(quote.delivery)}</strong>
                            </div>

                            <div class="sd-data-box">
                                <span>Warranty</span>
                                <strong>${escapeHTML(quote.warranty)}</strong>
                            </div>

                            <div class="sd-data-box">
                                <span>Availability</span>
                                <strong>${escapeHTML(quote.stock)}</strong>
                            </div>
                        </div>

                        ${quote.note ? `
                            <div class="sd-data-box" style="margin-top: 12px;">
                                <span>Vendor Note</span>
                                <strong>${escapeHTML(quote.note)}</strong>
                            </div>
                        ` : ""}

                        ${canReview ? `
                            <div class="sd-card-actions">
                                <button
                                    type="button"
                                    class="sd-primary-btn"
                                    data-accept-quote="${escapeHTML(quote.quoteId)}"
                                >
                                    <i class="bi bi-check-circle"></i>
                                    Accept Quote
                                </button>

                                <button
                                    type="button"
                                    class="sd-danger-btn"
                                    data-reject-quote="${escapeHTML(quote.quoteId)}"
                                >
                                    <i class="bi bi-x-circle"></i>
                                    Reject Quote
                                </button>
                            </div>
                        ` : ""}
                    ` : `
                        <p class="sd-card-message warning">
                            Waiting for an external vendor quotation.
                        </p>
                    `}

                </article>
                `;
            }).join("");

        attachVendorQuoteEvents();
    }

    function attachVendorQuoteEvents() {
        document
            .querySelectorAll("[data-accept-quote]")
            .forEach(button => {
                button.addEventListener("click", () => {
                    reviewVendorQuote(
                        button.dataset.acceptQuote,
                        "Accepted"
                    );
                });
            });

        document
            .querySelectorAll("[data-reject-quote]")
            .forEach(button => {
                button.addEventListener("click", () => {
                    reviewVendorQuote(
                        button.dataset.rejectQuote,
                        "Not Selected"
                    );
                });
            });
    }

    function reviewVendorQuote(quoteId, decision) {
        const quote = vendorQuotes.find(
            item => item.quoteId === quoteId
        );

        if (
            !quote ||
            quote.status !== "Pending Review" ||
            typeof workflowStore.updateVendorQuote !== "function" ||
            typeof workflowStore.updateVendorRequest !== "function"
        ) {
            return;
        }

        const request = vendorRequests.find(
            item => item.id === quote.vendorRequestId
        );

        workflowStore.updateVendorQuote(
            quoteId,
            {
                status: decision,
                reviewedAt: new Date().toISOString(),
                reviewedBy: "Store Keeper"
            }
        );

        workflowStore.updateVendorRequest(
            quote.vendorRequestId,
            {
                status:
                    decision === "Accepted"
                        ? "Quote Accepted"
                        : "Quote Rejected",
                reviewedAt: new Date().toISOString()
            }
        );

        if (
            request?.sourceRequest &&
            typeof workflowStore.updatePartRequest === "function"
        ) {
            workflowStore.updatePartRequest(
                request.sourceRequest,
                {
                    status:
                        decision === "Accepted"
                            ? "vendor-approved"
                            : "vendor-rejected",
                    vendorQuoteId: quoteId,
                    vendorDecision: decision
                }
            );
        }

        syncStorekeeperData();
        renderOverview();
        renderVendorRequests();
    }

    // Stock Movements
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
                        ${escapeHTML(formatIdentifier(item.reference))}
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

    function renderOverview() {
        const pending = requisitions.filter(
            request => request.status === "pending"
        );

        const healthyStock = inventory.filter(
            item => Number(item.quantity) > 2
        );

        const lowStock = inventory.filter(
            item =>
                Number(item.quantity) > 0 &&
                Number(item.quantity) <= 2
        );

        const outOfStock = inventory.filter(
            item => Number(item.quantity) <= 0
        );

        const healthPercentage = inventory.length
            ? Math.round(
                (healthyStock.length / inventory.length) * 100
            )
            : 0;

        const setCount = (id, value) => {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = String(value).padStart(2, "0");
            }
        };

        setCount("pendingRequisitionCount", pending.length);
        setCount("inventoryItemCount", inventory.length);
        setCount("lowStockCount", lowStock.length);
        setCount("vendorRequestCount", vendorRequests.length);

        const setText = (id, value) => {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = value;
            }
        };

        setText("pendingNavCount", String(pending.length));
        setText(
            "healthyStockSummaryCount",
            `${String(healthyStock.length).padStart(2, "0")} Items`
        );
        setText(
            "lowStockSummaryCount",
            `${String(lowStock.length).padStart(2, "0")} Items`
        );
        setText(
            "outOfStockSummaryCount",
            `${String(outOfStock.length).padStart(2, "0")} Items`
        );
        setText("healthyStockPercentage", `${healthPercentage}%`);

        const healthProgress =
            document.getElementById("inventoryHealthProgress");

        if (healthProgress) {
            healthProgress.style.width = `${healthPercentage}%`;
        }

        const body = document.getElementById("overviewRequisitionBody");
        if (!body) {
            return;
        }

        if (!pending.length) {
            body.innerHTML = `
                <tr>
                    <td colspan="5" class="sd-live-empty">
                        No pending parts requisitions.
                    </td>
                </tr>
            `;
            return;
        }

        body.innerHTML = pending.slice(0, 5).map(request => `
            <tr>
                <td><strong>#${escapeHTML(request.id)}</strong></td>
                <td>${escapeHTML(formatIdentifier(request.job))}</td>
                <td>${escapeHTML(request.part)}</td>
                <td>${escapeHTML(request.quantity)}</td>
                <td>
                    <span class="sd-badge ${
                        request.urgency === "Urgent" ? "high" : "normal"
                    }">
                        ${escapeHTML(request.urgency)}
                    </span>
                </td>
            </tr>
        `).join("");
    }

    // Initial shared-store render
    syncStorekeeperData();
    renderOverview();
    renderInventory();
    renderRequisitions();
    renderVendorRequests();
    renderMovements();

    if (
        workflowStore &&
        typeof workflowStore.subscribe === "function"
    ) {
        workflowStore.subscribe(() => {
            syncStorekeeperData();
            renderOverview();
            renderInventory();
            renderRequisitions();
            renderVendorRequests();
            renderMovements();
        });
    }

});
