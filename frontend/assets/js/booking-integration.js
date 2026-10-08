(() => {
  "use strict";

  const form = document.getElementById("serviceBookingForm");
  if (!form || !window.ShiftApi) return;

  const vehicleSelect = document.getElementById("bookingVehicle");
  const serviceSelect = document.getElementById("serviceType");
  const dateInput = document.getElementById("bookingDate");
  const timeInput = document.getElementById("bookingTime");
  const notesInput = document.getElementById("bookingNotes");
  const message = document.getElementById("bookingMessage");
  const button = form.querySelector('button[type="submit"]');
  const packageInfo = document.getElementById("servicePackageInfo");

  let services = [];
  let vehicles = [];
  let ready = false;
  let submitting = false;

  function showMessage(text, error = false) {
    if (!message) return;

    message.textContent = text;
    message.classList.remove("sd-error", "sd-success");
    message.classList.add(error ? "sd-error" : "sd-success");
    message.hidden = false;
    message.style.display = "block";
    message.style.color = error ? "#B91C1C" : "#15803D";
    message.style.background = error ? "#FEF2F2" : "#F0FDF4";
    message.style.border = error
      ? "1px solid #FECACA"
      : "1px solid #BBF7D0";
  }

  function setOptions(select, items, label, getText) {
    select.replaceChildren(new Option(label, ""));

    items.forEach(item => {
      select.add(new Option(getText(item), item.id));
    });
  }

  function setText(id, text) {
    const element = document.getElementById(id);
    if (element) element.textContent = text || "";
  }

  function showPackage() {
    const service = services.find(item =>
      item.id === serviceSelect.value
    );

    if (packageInfo) packageInfo.hidden = !service;
    if (!service) return;

    setText("servicePackageTitle", service.name);
    setText(
      "servicePackageDescription",
      service.description || "No description available."
    );
    setText(
      "servicePackageDuration",
      service.estimatedDurationMinutes
        ? `${service.estimatedDurationMinutes} minutes`
        : "Duration will be confirmed"
    );
  }

  // Use the API package details instead of the demo package list.
  document.addEventListener("change", event => {
    if (event.target !== serviceSelect) return;

    event.stopImmediatePropagation();
    showPackage();
  }, true);

  async function loadServices() {
    const items = [];
    let page = 1;

    while (true) {
      const result = await ShiftApi.request(
        `/api/services?activeOnly=true&page=${page}&pageSize=100`
      );

      if (Array.isArray(result)) return result;

      if (!result || !Array.isArray(result.items)) {
        throw new Error("Unexpected service catalogue response.");
      }

      items.push(...result.items);

      if (
        result.items.length === 0 ||
        items.length >= result.totalCount ||
        result.items.length < 100
      ) {
        return items;
      }

      page++;
    }
  }

  async function initialize() {
    if (button) button.disabled = true;

    vehicleSelect.replaceChildren(new Option("Loading vehicles…", ""));
    serviceSelect.replaceChildren(new Option("Loading services…", ""));
    vehicleSelect.disabled = true;
    serviceSelect.disabled = true;

    if (packageInfo) packageInfo.hidden = true;

    const today = new Date();
    const localDate = [
      today.getFullYear(),
      String(today.getMonth() + 1).padStart(2, "0"),
      String(today.getDate()).padStart(2, "0")
    ].join("-");

    dateInput.min = localDate;
    if (notesInput) notesInput.maxLength = 1000;

    try {
      const auth = ShiftApi.auth();

      if (!auth || auth.role !== "Customer") {
        window.location.href = "../login.html";
        return;
      }

      [vehicles, services] = await Promise.all([
        ShiftApi.request("/api/vehicles"),
        loadServices()
      ]);

      if (!Array.isArray(vehicles)) {
        throw new Error("Unexpected vehicle response.");
      }

      setOptions(
        vehicleSelect,
        vehicles,
        "Select your vehicle",
        vehicle =>
          `${vehicle.make} ${vehicle.model} - ${vehicle.registrationNumber}`
      );

      setOptions(
        serviceSelect,
        services,
        "Select service package",
        service => service.name
      );

      vehicleSelect.disabled = vehicles.length === 0;
      serviceSelect.disabled = services.length === 0;

      if (!vehicles.length) {
        showMessage("Add a vehicle before booking a service.", true);
        return;
      }

      if (!services.length) {
        showMessage(
          "No active service packages are available. Please contact the garage.",
          true
        );
        return;
      }

      ready = true;
      if (button) button.disabled = false;
      if (message) message.textContent = "";
    } catch (error) {
      showMessage(error.message || "Unable to load booking options.", true);
    }
  }

  // Prevent the existing demo submission handler from running.
  document.addEventListener("submit", async event => {
    if (event.target !== form) return;

    event.preventDefault();
    event.stopImmediatePropagation();

    if (submitting) return;

    if (!ready) {
      showMessage("Booking options are not ready. Refresh and try again.", true);
      return;
    }

    if (!form.reportValidity()) return;

    const auth = ShiftApi.auth();
    const vehicle = vehicles.find(item => item.id === vehicleSelect.value);
    const service = services.find(item => item.id === serviceSelect.value);

    if (!auth?.customerId || auth.role !== "Customer") {
      showMessage("Please log in as a customer.", true);
      return;
    }

    if (!vehicle || !service) {
      showMessage("Please select a vehicle and service package.", true);
      return;
    }

    if (!dateInput.value || !timeInput.value) {
      showMessage("Please select a date and time.", true);
      return;
    }

    const selectedDate = new Date(
      `${dateInput.value}T${timeInput.value}`
    );

    if (
      Number.isNaN(selectedDate.getTime()) ||
      selectedDate <= new Date()
    ) {
      showMessage("Please select a future appointment date and time.", true);
      return;
    }

    const notes = notesInput?.value.trim() || "";

    if (notes.length > 1000) {
      showMessage("Notes cannot exceed 1000 characters.", true);
      return;
    }

    submitting = true;
    if (button) button.disabled = true;

    let saved = false;

    try {
      await ShiftApi.request("/api/appointments", {
        method: "POST",
        body: JSON.stringify({
          customerId: auth.customerId,
          vehicleId: vehicle.id,
          appointmentDate: selectedDate.toISOString(),
          serviceType: service.name,
          notes: notes || null
        })
      });

      saved = true;
      showMessage("Booking saved successfully.");
      window.location.href = "dashboard.html";
    } catch (error) {
      showMessage(error.message || "Unable to save the booking.", true);
    } finally {
      submitting = false;
      if (button && !saved) button.disabled = false;
    }
  }, true);

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, { once: true });
  } else {
    initialize();
  }
})();