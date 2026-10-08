(() => {
  "use strict";

  const form = document.getElementById("vehicleForm");
  const modal = document.getElementById("vehicleModal");
  const grid = document.getElementById("vehicleGrid");
  const submitText = document.getElementById("vehicleSubmitText");

  if (!form || !modal || !grid || !window.ShiftApi) return;

  let editingId = null;
  let existingColor = null;
  let submitting = false;

  const value = id =>
    document.getElementById(id)?.value.trim() || "";

  const setValue = (id, text) => {
    const input = document.getElementById(id);
    if (input) input.value = text ?? "";
  };

  const escapeHTML = text =>
    String(text ?? "").replace(/[&<>"']/g, character => ({
      "&": "&amp;",
      "<": "&lt;",
      ">": "&gt;",
      '"': "&quot;",
      "'": "&#39;"
    })[character]);

  function resetForm() {
    editingId = null;
    existingColor = null;
    form.reset();
    if (submitText) submitText.textContent = "Add Vehicle";
  }

  function openModal() {
    modal.classList.add("open");
    modal.setAttribute("aria-hidden", "false");
    document.body.style.overflow = "hidden";
  }

  function closeModal() {
    modal.classList.remove("open");
    modal.setAttribute("aria-hidden", "true");
    document.body.style.overflow = "";
    resetForm();
  }

  async function refreshVehicles() {
    const vehicles = await ShiftApi.request("/api/vehicles");

    grid.innerHTML = vehicles.length
      ? vehicles.map(vehicle => `
        <article class="sd-vehicle-card"
                 data-id="${escapeHTML(vehicle.id)}">
          <div class="sd-vehicle-top">
            <div class="sd-vehicle-icon">
              <i class="bi bi-car-front-fill"></i>
            </div>
          </div>
          <h3>${escapeHTML(vehicle.make)} ${escapeHTML(vehicle.model)}</h3>
          <p>
            ${escapeHTML(vehicle.year)}
            ${vehicle.color ? ` • ${escapeHTML(vehicle.color)}` : ""}
          </p>
          <div class="sd-vehicle-details">
            <div>
              <span>License Plate</span>
              <strong>${escapeHTML(vehicle.registrationNumber)}</strong>
            </div>
            <div>
              <span>VIN</span>
              <strong>
                ${escapeHTML(vehicle.vin ?? vehicle.VIN ?? "Not specified")}
              </strong>
            </div>
          </div>
          <div class="sd-vehicle-actions">
            <button type="button"
                    class="sd-secondary-button sd-edit-vehicle"
                    data-id="${escapeHTML(vehicle.id)}">
              <i class="bi bi-pencil"></i> Edit
            </button>
            <button type="button"
                    class="sd-delete-button sd-delete-vehicle"
                    data-id="${escapeHTML(vehicle.id)}">
              <i class="bi bi-trash3"></i> Delete
            </button>
          </div>
        </article>
      `).join("")
      : '<p class="sd-api-state">No vehicles registered yet.</p>';
  }

  document.addEventListener("click", async event => {
    if (!(event.target instanceof Element)) return;

    if (event.target.closest("#addVehicleButton")) {
      event.preventDefault();
      event.stopImmediatePropagation();
      if (submitting) return;
      resetForm();
      openModal();
      return;
    }

    const close = event.target.closest(
      "#vehicleModalClose, [data-close-modal]"
    );

    if (close && modal.contains(close)) {
      event.preventDefault();
      event.stopImmediatePropagation();
      if (!submitting) closeModal();
      return;
    }

    const action = event.target.closest(
      ".sd-edit-vehicle, .sd-delete-vehicle"
    );

    if (!action || !grid.contains(action)) return;

    event.preventDefault();
    event.stopImmediatePropagation();

    if (submitting || action.disabled) return;

    const id = action.dataset.id ||
      action.closest(".sd-vehicle-card")?.dataset.id;

    if (!id) {
      alert("This vehicle has no database ID. Refresh the page.");
      return;
    }

    action.disabled = true;

    try {
      if (action.classList.contains("sd-edit-vehicle")) {
        const vehicle = await ShiftApi.request(
          `/api/vehicles/${encodeURIComponent(id)}`
        );

        resetForm();
        editingId = vehicle.id;
        existingColor = vehicle.color ?? null;

        setValue("vehicleMake", vehicle.make);
        setValue("vehicleModel", vehicle.model);
        setValue("vehicleYear", vehicle.year);
        setValue("vehiclePlate", vehicle.registrationNumber);
        setValue("vehicleVin", vehicle.vin ?? vehicle.VIN);
        setValue("vehicleColor", vehicle.color);

        if (submitText) submitText.textContent = "Save Changes";
        openModal();
      } else {
        if (!confirm("Delete this vehicle from your account?")) return;

        await ShiftApi.request(
          `/api/vehicles/${encodeURIComponent(id)}`,
          { method: "DELETE" }
        );

        await refreshVehicles(); window.dispatchEvent(new Event("shift:vehicles-updated"));
      }
    } catch (error) {
      alert(error.message || "Unable to complete the vehicle action.");
    } finally {
      action.disabled = false;
    }
  }, true);

  document.addEventListener("submit", async event => {
    if (event.target !== form) return;

    event.preventDefault();
    event.stopImmediatePropagation();

    if (submitting) return;

    const auth = ShiftApi.auth();

    if (!auth?.customerId || auth.role !== "Customer") {
      alert("Please log in as a customer.");
      return;
    }

    if (!form.reportValidity()) return;

    const make = value("vehicleMake");
    const model = value("vehicleModel");
    const registrationNumber = value("vehiclePlate");
    const year = Number(value("vehicleYear"));
    const vin = value("vehicleVin");

    if (!make || !model || !registrationNumber) {
      alert("Please enter Make, Model and License Plate.");
      return;
    }

    if (
      !Number.isInteger(year) ||
      year < 1950 ||
      year > new Date().getFullYear() + 1
    ) {
      alert("Please enter a valid vehicle year.");
      return;
    }

    if (
      make.length > 100 ||
      model.length > 100 ||
      registrationNumber.length > 30 ||
      vin.length > 50
    ) {
      alert(
        "Make and Model: maximum 100 characters. " +
        "License Plate: maximum 30. VIN: maximum 50."
      );
      return;
    }

    const colorInput = document.getElementById("vehicleColor");
    const color = colorInput
      ? colorInput.value.trim() || null
      : existingColor;

    if (color && color.length > 50) {
      alert("Color cannot exceed 50 characters.");
      return;
    }

    const payload = {
      customerId: auth.customerId,
      make,
      model,
      registrationNumber,
      year,
      vin: vin || null,
      color
    };

    const id = editingId;
    const button = form.querySelector('button[type="submit"]');

    submitting = true;
    if (button) button.disabled = true;

    try {
      await ShiftApi.request(
        id ? `/api/vehicles/${encodeURIComponent(id)}` : "/api/vehicles",
        {
          method: id ? "PUT" : "POST",
          body: JSON.stringify(payload)
        }
      );

      closeModal();

      try {
        await refreshVehicles(); window.dispatchEvent(new Event("shift:vehicles-updated"));
      } catch {
        alert("Vehicle saved. Refresh the page to reload the list.");
      }
    } catch (error) {
      alert(error.message || "Unable to save the vehicle.");
    } finally {
      submitting = false;
      if (button) button.disabled = false;
    }
  }, true);

  document.addEventListener("keydown", event => {
    if (event.key === "Escape" && modal.classList.contains("open")) {
      event.preventDefault();
      event.stopImmediatePropagation();
      if (!submitting) closeModal();
    }
  }, true);
})();