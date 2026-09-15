document.addEventListener("DOMContentLoaded", () => {

    // Navigation
    const navLinks = document.querySelectorAll(".sd-nav-link");
    const sections = document.querySelectorAll(".sd-content-section");
    const pageTitle = document.getElementById("pageTitle");
    const sidebar = document.getElementById("sidebar");
    const sidebarOverlay = document.getElementById("sidebarOverlay");
    const menuButton = document.getElementById("menuButton");
    const sidebarClose = document.getElementById("sidebarClose");

    const titles = {
        overview: "Vendor Overview",
        requests: "Incoming Requests",
        quotes: "Submitted Quotes",
        history: "Quote History"
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
            titles[sectionId] || "Vendor Portal";

        sidebar.classList.remove("open");
        sidebarOverlay.classList.remove("show");

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    }

    navLinks.forEach(link => {
        link.addEventListener("click", () => {
            openSection(link.dataset.section);
        });
    });

    document
        .querySelectorAll("[data-go-section]")
        .forEach(button => {
            button.addEventListener("click", () => {
                openSection(button.dataset.goSection);
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

    // Shared Storekeeper -> Vendor workflow
    const workflowStore = window.ShiftDynamicsStore;

    let vendorRequests = [];
    let submittedQuotes = [];
    let quoteHistory = [];

    function normalizeIdentifier(value) {
        return String(value || "")
            .trim()
            .replace(/^#+/, "")
            .toUpperCase();
    }

    function formatIdentifier(value) {
        const normalized = normalizeIdentifier(value);
        return normalized ? `#${normalized}` : "-";
    }

    function getRequestDisplayStatus(status) {
        const normalized = String(status || "").toLowerCase();

        if (
            normalized === "awaiting quotes" ||
            normalized === "pending"
        ) {
            return "pending";
        }

        if (normalized === "part not available") {
            return "not-available";
        }

        return "quoted";
    }

    function syncVendorData() {
        if (!workflowStore) {
            vendorRequests = [];
            submittedQuotes = [];
            quoteHistory = [];
            return;
        }

        const sharedRequests =
            typeof workflowStore.getVendorRequests === "function"
                ? workflowStore.getVendorRequests()
                : [];

        const partRequests =
            typeof workflowStore.getPartRequests === "function"
                ? workflowStore.getPartRequests()
                : [];

        const jobs =
            typeof workflowStore.getJobs === "function"
                ? workflowStore.getJobs()
                : [];

        vendorRequests = sharedRequests.map(request => {
            const sourceRequest = partRequests.find(
                item => item.requestId === request.sourceRequestId
            );

            const jobNumber =
                request.jobCardNumber ||
                sourceRequest?.jobCardNumber ||
                "";

            const job = jobs.find(item =>
                normalizeIdentifier(item.jobCardNumber) ===
                    normalizeIdentifier(jobNumber) ||
                normalizeIdentifier(item.id) ===
                    normalizeIdentifier(jobNumber)
            );

            const vehicle = job?.vehicle || {};

            return {
                ...request,
                id: request.vendorRequestId,
                job: jobNumber,
                vehicle:
                    request.vehicle ||
                    sourceRequest?.vehicle ||
                    [vehicle.make, vehicle.model]
                        .filter(Boolean)
                        .join(" ") ||
                    "Vehicle",
                year:
                    request.year ||
                    sourceRequest?.year ||
                    vehicle.year ||
                    "-",
                vin:
                    request.vin ||
                    sourceRequest?.vin ||
                    vehicle.vin ||
                    "-",
                part:
                    request.part ||
                    sourceRequest?.part ||
                    "Requested Part",
                partNumber:
                    request.partNumber ||
                    sourceRequest?.partNumber ||
                    "-",
                specification:
                    request.specification ||
                    sourceRequest?.reason ||
                    "No additional specification provided.",
                quantity: Number(
                    request.quantity ||
                    sourceRequest?.quantity ||
                    0
                ),
                urgency:
                    request.urgency ||
                    sourceRequest?.urgency ||
                    "Normal",
                status: getRequestDisplayStatus(request.status)
            };
        });

        submittedQuotes =
            typeof workflowStore.getVendorQuotes === "function"
                ? workflowStore.getVendorQuotes().map(quote => ({
                    ...quote,
                    requestId: quote.vendorRequestId
                }))
                : [];

        const unavailableItems = vendorRequests
            .filter(request => request.status === "not-available")
            .map(request => ({
                quoteId: "-",
                requestId: request.id,
                part: request.part,
                price: 0,
                delivery: "-",
                status: "Part Not Available",
                createdAt: request.respondedAt || request.updatedAt
            }));

        quoteHistory = [
            ...submittedQuotes,
            ...unavailableItems
        ].sort((a, b) =>
            new Date(b.updatedAt || b.createdAt || 0) -
            new Date(a.updatedAt || a.createdAt || 0)
        );
    }

    // Helpers
    function escapeHTML(value) {
        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function formatCurrency(value) {
        return new Intl.NumberFormat(
            "en-LK",
            {
                style: "currency",
                currency: "LKR",
                maximumFractionDigits: 0
            }
        ).format(value);
    }

    // Vendor Requests
    const vendorRequestList =
        document.getElementById("vendorRequestList");

    function renderVendorRequests() {
        const pending =
            vendorRequests.filter(
                request => request.status === "pending"
            );

        if (!pending.length) {
            vendorRequestList.innerHTML = `
                <article class="sd-panel">
                    No pending vendor requests at the moment.
                </article>
            `;

            return;
        }

        vendorRequestList.innerHTML =
            pending.map(request => `
                <article class="sd-request-card">

                    <div class="sd-card-top">

                        <div>
                            <span class="sd-eyebrow">
                                ${escapeHTML(request.id)}
                            </span>

                            <h3>
                                ${escapeHTML(request.part)}
                            </h3>

                            <p>
                                Job Card ${escapeHTML(formatIdentifier(request.job))}
                            </p>
                        </div>

                        <span class="sd-badge ${
                            request.urgency === "Urgent"
                                ? "urgent"
                                : "normal"
                        }">
                            ${escapeHTML(request.urgency)}
                        </span>

                    </div>

                    <div class="sd-card-data">

                        <div class="sd-data-box">
                            <span>Vehicle</span>
                            <strong>
                                ${escapeHTML(request.vehicle)}
                            </strong>
                        </div>

                        <div class="sd-data-box">
                            <span>Year</span>
                            <strong>
                                ${escapeHTML(request.year)}
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
                            <span>VIN</span>
                            <strong>
                                ${escapeHTML(request.vin)}
                            </strong>
                        </div>

                    </div>

                    <div
                        class="sd-data-box"
                        style="margin-top: 12px;"
                    >
                        <span>Part Specification</span>

                        <strong>
                            ${escapeHTML(request.specification)}
                        </strong>
                    </div>

                    <div class="sd-card-actions">

                        <button
                            type="button"
                            class="sd-primary-btn"
                            data-have-part="${escapeHTML(request.id)}"
                        >
                            <i class="bi bi-check-circle"></i>
                            Have Part
                        </button>

                        <button
                            type="button"
                            class="sd-danger-btn"
                            data-no-part="${escapeHTML(request.id)}"
                        >
                            <i class="bi bi-x-circle"></i>
                            Do Not Have
                        </button>

                    </div>

                </article>
            `).join("");

        attachRequestEvents();
    }

    function attachRequestEvents() {
        document
            .querySelectorAll("[data-have-part]")
            .forEach(button => {
                button.addEventListener("click", () => {
                    openQuoteModal(
                        button.dataset.havePart
                    );
                });
            });

        document
            .querySelectorAll("[data-no-part]")
            .forEach(button => {
                button.addEventListener("click", () => {
                    const request =
                        vendorRequests.find(
                            item =>
                                item.id === button.dataset.noPart
                        );

                    if (!request) {
                        return;
                    }

                    if (
                        !workflowStore ||
                        typeof workflowStore.updateVendorRequest !==
                            "function"
                    ) {
                        return;
                    }

                    workflowStore.updateVendorRequest(
                        request.id,
                        {
                            status: "Part Not Available",
                            respondedAt: new Date().toISOString()
                        }
                    );

                    syncVendorData();
                    renderAllVendorViews();
                });
            });
    }

    // Quote Modal
    const quoteModal =
        document.getElementById("quoteModal");

    const quoteForm =
        document.getElementById("quoteForm");

    const quoteRequestId =
        document.getElementById("quoteRequestId");

    const quoteEditId =
        document.getElementById("quoteEditId");

    const quoteModalTitle =
        document.getElementById("quoteModalTitle");

    const quotePrice =
        document.getElementById("quotePrice");

    const quoteDelivery =
        document.getElementById("quoteDelivery");

    const quoteWarranty =
        document.getElementById("quoteWarranty");

    const quoteStock =
        document.getElementById("quoteStock");

    const quoteNote =
        document.getElementById("quoteNote");

    const quoteFormMessage =
        document.getElementById("quoteFormMessage");

    function openQuoteModal(requestId) {
        const request =
            vendorRequests.find(
                item => item.id === requestId
            );

        if (!request) {
            return;
        }

        quoteForm.reset();
        quoteEditId.value = "";
        quoteRequestId.value = request.id;

        quoteModalTitle.textContent =
            `${request.part} · ${request.id}`;

        quoteFormMessage.textContent = "";
        quoteFormMessage.className =
            "sd-form-message";

        quoteModal.hidden = false;
    }

    function openEditQuoteModal(quoteId) {
        const quote =
            submittedQuotes.find(
                item => item.quoteId === quoteId
            );

        if (
            !quote ||
            quote.status !== "Pending Review"
        ) {
            return;
        }

        quoteForm.reset();

        quoteEditId.value =
            quote.quoteId;

        quoteRequestId.value =
            quote.requestId;

        quoteModalTitle.textContent =
            `Edit ${quote.quoteId} · ${quote.part}`;

        quotePrice.value =
            quote.price;

        quoteDelivery.value =
            quote.delivery;

        quoteWarranty.value =
            quote.warranty;

        quoteStock.value =
            quote.stock;

        quoteNote.value =
            quote.note || "";

        quoteFormMessage.textContent = "";
        quoteFormMessage.className =
            "sd-form-message";

        quoteModal.hidden = false;
    }

    function closeQuoteModal() {
        quoteModal.hidden = true;
    }

    document
        .querySelectorAll("[data-close-modal]")
        .forEach(button => {
            button.addEventListener(
                "click",
                closeQuoteModal
            );
        });

    document.addEventListener(
        "keydown",
        event => {
            if (
                event.key === "Escape" &&
                !quoteModal.hidden
            ) {
                closeQuoteModal();
            }
        }
    );

    // Submit Quote
    quoteForm.addEventListener(
        "submit",
        event => {
            event.preventDefault();

            const request =
                vendorRequests.find(
                    item =>
                        item.id === quoteRequestId.value
                );

            const price =
                Number(quotePrice.value);

            if (
                !request ||
                !price ||
                !quoteDelivery.value ||
                !quoteWarranty.value ||
                !quoteStock.value
            ) {
                quoteFormMessage.textContent =
                    "Please complete all required quotation fields.";

                quoteFormMessage.className =
                    "sd-form-message error";

                return;
            }

            const editingQuoteId =
                quoteEditId.value.trim();

            if (editingQuoteId) {
                const existingQuote =
                    submittedQuotes.find(
                        item =>
                            item.quoteId === editingQuoteId
                    );

                if (
                    !existingQuote ||
                    existingQuote.status !== "Pending Review"
                ) {
                    quoteFormMessage.textContent =
                        "Only pending quotations can be edited.";

                    quoteFormMessage.className =
                        "sd-form-message error";

                    return;
                }

                if (
                    !workflowStore ||
                    typeof workflowStore.updateVendorQuote !==
                        "function"
                ) {
                    quoteFormMessage.textContent =
                        "Shared quotation workflow is unavailable.";
                    quoteFormMessage.className =
                        "sd-form-message error";
                    return;
                }

                workflowStore.updateVendorQuote(
                    editingQuoteId,
                    {
                        price,
                        delivery: quoteDelivery.value,
                        warranty: quoteWarranty.value,
                        stock: quoteStock.value,
                        note: quoteNote.value.trim()
                    }
                );

                syncVendorData();
                renderAllVendorViews();

                closeQuoteModal();
                openSection("quotes");

                return;
            }

            if (
                !workflowStore ||
                typeof workflowStore.createVendorQuote !== "function" ||
                typeof workflowStore.updateVendorRequest !== "function"
            ) {
                quoteFormMessage.textContent =
                    "Shared quotation workflow is unavailable.";
                quoteFormMessage.className =
                    "sd-form-message error";
                return;
            }

            const savedQuote =
                workflowStore.createVendorQuote({
                    vendorRequestId: request.id,
                    sourceRequestId: request.sourceRequestId || null,
                    jobCardNumber: request.job,
                    part: request.part,
                    partNumber: request.partNumber,
                    quantity: request.quantity,
                    vehicle: request.vehicle,
                    price,
                    delivery: quoteDelivery.value,
                    warranty: quoteWarranty.value,
                    stock: quoteStock.value,
                    note: quoteNote.value.trim(),
                    vendorName: "AutoParts Lanka",
                    status: "Pending Review"
                });

            workflowStore.updateVendorRequest(
                request.id,
                {
                    status: "Quote Submitted",
                    quoteId: savedQuote.quoteId,
                    quotedAt: new Date().toISOString()
                }
            );

            syncVendorData();
            renderAllVendorViews();

            closeQuoteModal();
            openSection("quotes");
        }
    );

    // Submitted Quotes
    const submittedQuoteList =
        document.getElementById("submittedQuoteList");

    function renderSubmittedQuotes() {
        const activeQuotes = submittedQuotes.filter(
            quote => quote.status === "Pending Review"
        );

        if (!activeQuotes.length) {
            submittedQuoteList.innerHTML = `
                <article class="sd-panel">
                    No submitted quotations yet.
                </article>
            `;

            return;
        }

        submittedQuoteList.innerHTML =
            activeQuotes.map(quote => `
                <article class="sd-quote-card">

                    <div class="sd-card-top">

                        <div>
                            <span class="sd-eyebrow">
                                ${escapeHTML(quote.quoteId)}
                            </span>

                            <h3>
                                ${escapeHTML(quote.part)}
                            </h3>

                            <p>
                                Request ${escapeHTML(quote.requestId)}
                            </p>
                        </div>

                        <span class="sd-badge submitted">
                            ${escapeHTML(quote.status)}
                        </span>

                    </div>

                    <div class="sd-card-data">

                        <div class="sd-data-box">
                            <span>Unit Price</span>
                            <strong>
                                ${escapeHTML(
                                    formatCurrency(quote.price)
                                )}
                            </strong>
                        </div>

                        <div class="sd-data-box">
                            <span>Delivery</span>
                            <strong>
                                ${escapeHTML(quote.delivery)}
                            </strong>
                        </div>

                        <div class="sd-data-box">
                            <span>Warranty</span>
                            <strong>
                                ${escapeHTML(quote.warranty)}
                            </strong>
                        </div>

                        <div class="sd-data-box">
                            <span>Availability</span>
                            <strong>
                                ${escapeHTML(quote.stock)}
                            </strong>
                        </div>

                        <div class="sd-data-box">
                            <span>Status</span>
                            <strong>
                                ${escapeHTML(quote.status)}
                            </strong>
                        </div>

                    </div>

                    ${
                        quote.status === "Pending Review"
                            ? `
                                <div class="sd-card-actions">

                                    <button
                                        type="button"
                                        class="sd-secondary-btn"
                                        data-edit-quote="${escapeHTML(
                                            quote.quoteId
                                        )}"
                                    >
                                        <i class="bi bi-pencil-square"></i>
                                        Edit Quote
                                    </button>

                                </div>
                            `
                            : ""
                    }

                </article>
            `).join("");

        document
            .querySelectorAll("[data-edit-quote]")
            .forEach(button => {
                button.addEventListener(
                    "click",
                    () => {
                        openEditQuoteModal(
                            button.dataset.editQuote
                        );
                    }
                );
            });
    }

    // Quote History
    const quoteHistoryBody =
        document.getElementById("quoteHistoryBody");

    function getHistoryBadge(status) {
        if (status === "Accepted") {
            return "accepted";
        }

        if (
            status === "Not Selected" ||
            status === "Part Not Available"
        ) {
            return "rejected";
        }

        return "submitted";
    }

    function renderQuoteHistory() {
        quoteHistoryBody.innerHTML =
            quoteHistory.map(item => `
                <tr>

                    <td>
                        <strong>
                            ${escapeHTML(item.quoteId)}
                        </strong>
                    </td>

                    <td>
                        ${escapeHTML(item.requestId)}
                    </td>

                    <td>
                        ${escapeHTML(item.part)}
                    </td>

                    <td>
                        ${
                            item.price
                                ? escapeHTML(
                                    formatCurrency(item.price)
                                )
                                : "-"
                        }
                    </td>

                    <td>
                        ${escapeHTML(item.delivery)}
                    </td>

                    <td>
                        <span class="sd-badge ${getHistoryBadge(item.status)}">
                            ${escapeHTML(item.status)}
                        </span>
                    </td>

                </tr>
            `).join("");
    }

    function renderOverview() {
        const pendingRequests = vendorRequests.filter(
            request => request.status === "pending"
        );

        const acceptedQuotes = submittedQuotes.filter(
            quote => quote.status === "Accepted"
        );

        const pendingQuotes = submittedQuotes.filter(
            quote => quote.status === "Pending Review"
        );

        const notSelectedQuotes = submittedQuotes.filter(
            quote => quote.status === "Not Selected"
        );

        const setCount = (id, value, pad = true) => {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = pad
                    ? String(value).padStart(2, "0")
                    : String(value);
            }
        };

        setCount("incomingRequestNavCount", pendingRequests.length, false);
        setCount("newRequestCount", pendingRequests.length);
        setCount("submittedQuoteCount", submittedQuotes.length);
        setCount("acceptedQuoteCount", acceptedQuotes.length);
        setCount("performanceSubmittedCount", submittedQuotes.length);
        setCount("performanceAcceptedCount", acceptedQuotes.length);
        setCount("performancePendingCount", pendingQuotes.length);
        setCount("performanceNotSelectedCount", notSelectedQuotes.length);

        const responseTimes = submittedQuotes
            .map(quote => {
                const request = vendorRequests.find(
                    item => item.id === quote.vendorRequestId
                );

                const requestedAt = new Date(request?.createdAt || 0);
                const quotedAt = new Date(quote.createdAt || 0);
                const difference = quotedAt - requestedAt;

                return difference >= 0 && Number.isFinite(difference)
                    ? difference
                    : null;
            })
            .filter(value => value !== null);

        const averageResponseTime =
            document.getElementById("averageResponseTime");

        if (averageResponseTime) {
            if (!responseTimes.length) {
                averageResponseTime.textContent = "--";
            } else {
                const averageMinutes = Math.max(
                    1,
                    Math.round(
                        responseTimes.reduce((sum, value) => sum + value, 0) /
                        responseTimes.length /
                        60000
                    )
                );

                averageResponseTime.textContent = `${averageMinutes}m`;
            }
        }

        const latestBody =
            document.getElementById("latestVendorRequestBody");

        if (!latestBody) {
            return;
        }

        if (!pendingRequests.length) {
            latestBody.innerHTML = `
                <tr>
                    <td colspan="5">
                        No incoming vendor requests.
                    </td>
                </tr>
            `;
            return;
        }

        latestBody.innerHTML = pendingRequests.slice(0, 5).map(request => `
            <tr>
                <td>
                    <strong>${escapeHTML(formatIdentifier(request.id))}</strong>
                </td>
                <td>${escapeHTML(request.vehicle)}</td>
                <td>${escapeHTML(request.part)}</td>
                <td>${escapeHTML(request.quantity)}</td>
                <td>
                    <span class="sd-badge ${
                        request.urgency === "Urgent" ? "urgent" : "normal"
                    }">
                        ${escapeHTML(request.urgency)}
                    </span>
                </td>
            </tr>
        `).join("");
    }

    function renderAllVendorViews() {
        renderOverview();
        renderVendorRequests();
        renderSubmittedQuotes();
        renderQuoteHistory();
    }

    // Initial Render
    syncVendorData();
    renderAllVendorViews();

    if (
        workflowStore &&
        typeof workflowStore.subscribe === "function"
    ) {
        workflowStore.subscribe(() => {
            syncVendorData();
            renderAllVendorViews();
        });
    }

});
