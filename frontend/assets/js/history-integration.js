(() => {
  "use strict";

  function initialize() {
    const section = document.getElementById("history");
    if (!section) return;

    let container = document.getElementById("serviceHistoryRecords");

    if (!container) {
      container = document.createElement("div");
      container.id = "serviceHistoryRecords";
      container.setAttribute("aria-live", "polite");

      Array.from(section.children).forEach(element => {
        if (!element.classList.contains("sd-section-heading")) {
          element.remove();
        }
      });

      section.appendChild(container);
    }

    function escapeHTML(value) {
      return String(value ?? "").replace(/[&<>"']/g, character => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
      })[character]);
    }

    function formatDate(value) {
      const date = value ? new Date(value) : null;

      if (!date || Number.isNaN(date.getTime())) {
        return "Not recorded";
      }

      return date.toLocaleDateString("en-GB", {
        day: "2-digit",
        month: "short",
        year: "numeric"
      });
    }

    async function loadHistory() {
      container.innerHTML = `
        <div class="sd-dashboard-card">
          <p>Loading your service history...</p>
        </div>
      `;

      try {
        if (!window.ShiftApi) {
          throw new Error(
            "API client is missing. Load api-client.js before history-integration.js."
          );
        }

        const auth = ShiftApi.auth();

        if (!auth || auth.role !== "Customer") {
          window.location.href = "../login.html";
          return;
        }

        const records = await ShiftApi.request("/api/service-history");

        if (!Array.isArray(records)) {
          throw new Error("Unexpected service history response.");
        }

        if (!records.length) {
          container.innerHTML = `
            <div class="sd-dashboard-card">
              <h3>No completed services yet</h3>
              <p class="sd-muted-text">
                Your completed workshop jobs will appear here.
              </p>
            </div>
          `;
          return;
        }

        container.innerHTML = records.map(record => {
          const vehicle = record.vehicle || {};
          const service = record.service || {};

          return `
            <article class="sd-dashboard-card">
              <div class="sd-panel-header">
                <div>
                  <span class="sd-panel-label">
                    ${escapeHTML(record.workOrderNumber)}
                  </span>
                  <h3>
                    ${escapeHTML(service.name || "Workshop Service")}
                  </h3>
                </div>
                <span class="sd-status-badge">Completed</span>
              </div>

              <div class="sd-vehicle-details">
                <div>
                  <span>Vehicle</span>
                  <strong>
                    ${escapeHTML(
                      `${vehicle.make || ""} ${vehicle.model || ""}`.trim()
                      || "Not recorded"
                    )}
                  </strong>
                </div>

                <div>
                  <span>Registration Number</span>
                  <strong>
                    ${escapeHTML(
                      vehicle.registrationNumber || "Not recorded"
                    )}
                  </strong>
                </div>

                <div>
                  <span>Completed Date</span>
                  <strong>
                    ${escapeHTML(formatDate(record.completedAt))}
                  </strong>
                </div>
              </div>

              ${record.description ? `
                <p>
                  <strong>Service Details:</strong>
                  ${escapeHTML(record.description)}
                </p>
              ` : ""}

              ${record.technicianNotes ? `
                <p>
                  <strong>Technician Notes:</strong>
                  ${escapeHTML(record.technicianNotes)}
                </p>
              ` : ""}
            </article>
          `;
        }).join("");
      } catch (error) {
        container.innerHTML = `
          <div class="sd-dashboard-card">
            <h3>Unable to load service history</h3>
            <p>${escapeHTML(error.message)}</p>
            <button
              type="button"
              class="sd-secondary-button"
              id="retryServiceHistory"
            >
              Retry
            </button>
          </div>
        `;

        container.querySelector("#retryServiceHistory")
          .addEventListener("click", loadHistory);
      }
    }

    loadHistory();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, {
      once: true
    });
  } else {
    initialize();
  }
})();