(() => {
  "use strict";

  function initialize() {
    const section = document.getElementById("tracker");
    if (!section || !window.ShiftApi) return;

    const auth = ShiftApi.auth();

    if (!auth || auth.role !== "Customer") {
      window.location.href = "../login.html";
      return;
    }

    let container = document.getElementById("liveTrackerRecords");

    if (!container) {
      container = document.createElement("div");
      container.id = "liveTrackerRecords";
      container.setAttribute("aria-live", "polite");

      Array.from(section.children).forEach(element => {
        if (!element.classList.contains("sd-section-heading")) {
          element.remove();
        }
      });

      section.appendChild(container);
    }

    const refreshButton = document.createElement("button");
    refreshButton.type = "button";
    refreshButton.className = "sd-secondary-button";
    refreshButton.textContent = "Refresh Status";

    section.insertBefore(refreshButton, container);

    const statuses = [
      "Open",
      "Assigned",
      "InProgress",
      "WaitingForParts",
      "Completed",
      "Cancelled"
    ];

    const descriptions = {
      Open: "Your job card has been created.",
      Assigned: "A staff member has been assigned to your job.",
      InProgress: "Work on your vehicle is in progress.",
      WaitingForParts: "Work is waiting for the required spare parts.",
      Completed: "The workshop job is completed.",
      Cancelled: "This workshop job was cancelled."
    };

    function statusName(value) {
      if (
        typeof value === "number" ||
        /^\d+$/.test(String(value))
      ) {
        return statuses[Number(value)] || "Unknown";
      }

      return String(value || "Unknown");
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
      if (!value) return "Not recorded";

      const date = new Date(value);

      return Number.isNaN(date.getTime())
        ? "Not recorded"
        : date.toLocaleString("en-GB");
    }

    async function loadTracker() {
      refreshButton.disabled = true;

      container.innerHTML = `
        <div class="sd-tracker-card">
          <p>Loading your workshop jobs...</p>
        </div>
      `;

      try {
        const records = await ShiftApi.request("/api/work-orders");

        if (!Array.isArray(records)) {
          throw new Error("Unexpected job card response.");
        }

        const activeJobs = records
          .filter(job =>
            !["Completed", "Cancelled"].includes(statusName(job.status))
          )
          .sort((a, b) =>
            new Date(b.createdAt) - new Date(a.createdAt)
          );

        if (!activeJobs.length) {
          container.innerHTML = `
            <div class="sd-tracker-card">
              <h3>No active workshop jobs</h3>
              <p>
                Your job will appear after the service advisor creates
                a job card for your vehicle.
              </p>
              <p>
                Completed jobs are available in
                <a href="history.html">Service History</a>.
              </p>
            </div>
          `;
          return;
        }

        container.innerHTML = activeJobs.map(job => {
          const state = statusName(job.status);
          const label = state.replace(/([a-z])([A-Z])/g, "$1 $2");

          return `
            <article class="sd-tracker-card">
              <div class="sd-tracker-header">
                <div>
                  <span class="sd-panel-label">
                    ${escapeHTML(job.workOrderNumber)}
                  </span>
                  <h3>${escapeHTML(job.vehicleReg || "Vehicle")}</h3>
                  <p>${escapeHTML(job.serviceName || "Workshop Service")}</p>
                </div>

                <span class="sd-status-badge">
                  ${escapeHTML(label)}
                </span>
              </div>

              <p>
                ${escapeHTML(
                  descriptions[state] || "Status details are unavailable."
                )}
              </p>

              <div class="sd-vehicle-details">
                <div>
                  <span>Job Created</span>
                  <strong>${escapeHTML(formatDate(job.createdAt))}</strong>
                </div>

                ${job.startedAt ? `
                  <div>
                    <span>Work Started</span>
                    <strong>
                      ${escapeHTML(formatDate(job.startedAt))}
                    </strong>
                  </div>
                ` : ""}
              </div>

              ${job.description ? `
                <p>
                  <strong>Job Details:</strong>
                  ${escapeHTML(job.description)}
                </p>
              ` : ""}
            </article>
          `;
        }).join("");
      } catch (error) {
        container.innerHTML = `
          <div class="sd-tracker-card">
            <h3>Unable to load workshop jobs</h3>
            <p>${escapeHTML(error.message)}</p>
            <p>Use Refresh Status to try again.</p>
          </div>
        `;
      } finally {
        refreshButton.disabled = false;
      }
    }

    refreshButton.addEventListener("click", loadTracker);
    loadTracker();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, {
      once: true
    });
  } else {
    initialize();
  }
})();