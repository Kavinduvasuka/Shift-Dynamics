(() => {
  "use strict";

  if (!window.ShiftApi) return;

  const auth = ShiftApi.auth();

  if (!auth || auth.role !== "Customer") {
    ShiftApi.clear();
    window.location.href = "../login.html";
    return;
  }

  const overview = document.getElementById("overview");
  const stats = overview
    ? overview.querySelectorAll(".sd-stat-card strong")
    : [];

  const currentJob = overview?.querySelector(".sd-current-job");
  const appointment = overview?.querySelector(".sd-appointment");

  const escapeHTML = value =>
    String(value ?? "").replace(/[&<>"']/g, character => ({
      "&": "&amp;",
      "<": "&lt;",
      ">": "&gt;",
      '"': "&quot;",
      "'": "&#39;"
    })[character]);

  function setText(selector, value) {
    document.querySelectorAll(selector).forEach(element => {
      element.textContent = value || "";
    });
  }

  function showIdentity(user) {
    setText(
      ".sd-sidebar-user strong, .sd-topbar-profile-text strong",
      user.fullName || "Customer"
    );

    setText(
      ".sd-sidebar-user > div:last-child > span",
      user.email
    );

    setText(
      ".sd-topbar-profile-text > span",
      user.email
    );

    if (overview) {
      setText(
        "#pageSubtitle",
        `Welcome back, ${user.fullName || "Customer"}.`
      );
    }
  }

  function statusName(value, names) {
    if (typeof value === "number") {
      return names[value] || "Unknown";
    }

    if (/^\d+$/.test(String(value))) {
      return names[Number(value)] || "Unknown";
    }

    return String(value || "Unknown");
  }

  const jobStatuses = [
    "Open",
    "Assigned",
    "InProgress",
    "WaitingForParts",
    "Completed",
    "Cancelled"
  ];

  const appointmentStatuses = [
    "Scheduled",
    "Confirmed",
    "Completed",
    "Cancelled",
    "NoShow"
  ];

  const estimateStatuses = [
    "Draft",
    "Sent",
    "Approved",
    "Rejected",
    "Expired"
  ];

  function renderJob(jobs) {
    if (!currentJob) return;

    const active = jobs
      .filter(job => !["Completed", "Cancelled"].includes(
        statusName(job.status, jobStatuses)
      ))
      .sort((a, b) =>
        new Date(b.createdAt) - new Date(a.createdAt)
      )[0];

    if (!active) {
      currentJob.innerHTML =
        '<p class="sd-api-state">No active service jobs.</p>';
      return;
    }

    const label = statusName(active.status, jobStatuses)
      .replace(/([a-z])([A-Z])/g, "$1 $2");

    currentJob.innerHTML = `
      <div class="sd-job-title">
        <div>
          <strong>${escapeHTML(active.workOrderNumber)}</strong>
          <span>${escapeHTML(active.vehicleReg)}</span>
        </div>
        <span class="sd-status-badge">${escapeHTML(label)}</span>
      </div>
      <div class="sd-progress-info">
        <span>${escapeHTML(active.serviceName)}</span>
      </div>
    `;
  }

  function renderAppointment(items) {
    if (!appointment) return;

    const upcoming = items
      .filter(item =>
        new Date(item.appointmentDate) > new Date() &&
        ["Scheduled", "Confirmed"].includes(
          statusName(item.status, appointmentStatuses)
        )
      )
      .sort((a, b) =>
        new Date(a.appointmentDate) - new Date(b.appointmentDate)
      );

    if (stats[1]) stats[1].textContent = upcoming.length;

    const next = upcoming[0];

    if (!next) {
      appointment.innerHTML =
        '<p class="sd-api-state">No upcoming bookings.</p>';
      return;
    }

    const date = new Date(next.appointmentDate);

    appointment.innerHTML = `
      <div class="sd-date-box">
        <span>
          ${escapeHTML(date.toLocaleDateString("en-GB", {
            month: "short"
          }).toUpperCase())}
        </span>
        <strong>
          ${escapeHTML(String(date.getDate()).padStart(2, "0"))}
        </strong>
      </div>
      <div>
        <strong>${escapeHTML(next.serviceType)}</strong>
        <!-- SD_BOOKING_STATUS_BADGE -->
        <div style="margin-top:8px">
          <span class="sd-status-badge">
            ${escapeHTML(statusName(next.status, appointmentStatuses))}
          </span>
        </div>
        <p>
          ${escapeHTML(date.toLocaleString("en-GB"))}
          ${next.vehicleRegistration
            ? ` • ${escapeHTML(next.vehicleRegistration)}`
            : ""}
        </p>
      </div>
    `;
  }

  async function loadOverview() {
    if (!overview) return;

    stats.forEach(element => element.textContent = "…");

    if (currentJob) {
      currentJob.innerHTML = '<p>Loading service jobs…</p>';
    }

    if (appointment) {
      appointment.innerHTML = '<p>Loading bookings…</p>';
    }

    const endpoints = [
      "/api/vehicles",
      "/api/appointments",
      "/api/work-orders",
      "/api/estimates"
    ];

    await Promise.allSettled(endpoints.map(async (endpoint, index) => {
      try {
        const items = await ShiftApi.request(endpoint);

        if (!Array.isArray(items)) {
          throw new Error("Unexpected API response.");
        }

        if (index === 0 && stats[0]) {
          stats[0].textContent = items.length;
        }

        if (index === 1) {
          renderAppointment(items);
        }

        if (index === 2) {
          const activeCount = items.filter(job =>
            !["Completed", "Cancelled"].includes(
              statusName(job.status, jobStatuses)
            )
          ).length;

          if (stats[2]) stats[2].textContent = activeCount;
          renderJob(items);
        }

        if (index === 3 && stats[3]) {
          stats[3].textContent = items.filter(estimate =>
            statusName(estimate.status, estimateStatuses) === "Sent"
          ).length;
        }
      } catch (error) {
        if (stats[index]) {
          stats[index].textContent = "Unavailable";
          stats[index].title = error.message;
        }

        if (index === 1 && appointment) {
          appointment.innerHTML = '<p>Unable to load bookings.</p>';
        }

        if (index === 2 && currentJob) {
          currentJob.innerHTML = '<p>Unable to load service jobs.</p>';
        }

        console.error(`Failed to load ${endpoint}:`, error);
      }
    }));
  }

  async function loadIdentity() {
    try {
      const user = await ShiftApi.request("/api/auth/me");

      if (user.role !== "Customer") {
        ShiftApi.clear();
        window.location.href = "../login.html";
        return;
      }

      // Preserve the login token and its original expiry.
      const saved = ShiftApi.auth();
      if (!saved) return;

      const updated = {
        ...saved,
        userId: user.userId,
        customerId: user.customerId,
        fullName: user.fullName,
        email: user.email,
        phone: user.phone,
        role: user.role,
        status: user.status
      };

      ShiftApi.save(updated);
      showIdentity(updated);
    } catch (error) {
      console.error("Unable to refresh customer details:", error);
    }
  }

  // Clear the session when the customer logs out.
  document.addEventListener("click", event => {
    if (!(event.target instanceof Element)) return;

    const logout = event.target.closest(".sd-logout-link");
    if (!logout) return;

    event.preventDefault();
    event.stopImmediatePropagation();
    ShiftApi.clear();
    window.location.href = "../login.html";
  }, true);

  function initialize() {
    showIdentity(auth);
    loadIdentity();
    loadOverview();

    window.addEventListener("shift:vehicles-updated", loadOverview);

    // SD_BOOKING_STATUS_REFRESH
    let refreshing = false;

    async function refreshVisibleOverview() {
      if (
        !overview ||
        document.hidden ||
        refreshing ||
        ShiftApi.auth()?.role !== "Customer"
      ) return;

      refreshing = true;

      try {
        await loadOverview();
      } finally {
        refreshing = false;
      }
    }

    window.addEventListener("focus", refreshVisibleOverview);
    document.addEventListener("visibilitychange", refreshVisibleOverview);
    window.setInterval(refreshVisibleOverview, 15000);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, {
      once: true
    });
  } else {
    initialize();
  }
})();