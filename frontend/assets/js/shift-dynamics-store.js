/* =========================================================
   SHIFT DYNAMICS - SHARED WORKFLOW STORE

   Temporary frontend persistence layer.

   Current:
       Dashboard -> ShiftDynamicsStore -> localStorage

   Future:
       Dashboard -> API Service -> .NET API -> Database

   IMPORTANT:
   localStorage is NOT the final source of truth.
   ========================================================= */

window.ShiftDynamicsStore = (() => {

    const STORAGE_KEY =
        "shiftDynamicsWorkflowState";

    const STATE_VERSION = 1;


    function createInitialState() {

        return {
            version: STATE_VERSION,
            jobs: [],
            bookings: [],
            customerProfile: null,
            vehicles: [],
            emergencyRequests: [],
            inventory: [],
            partRequests: [],
            vendorRequests: [],
            vendorQuotes: [],
            stockMovements: []
        };
    }


    function readState() {

        try {

            const raw =
                localStorage.getItem(
                    STORAGE_KEY
                );

            if (!raw) {
                return createInitialState();
            }

            const parsed =
                JSON.parse(raw);

            if (
                !parsed ||
                !Array.isArray(parsed.jobs)
            ) {
                return createInitialState();
            }

            return {
                version:
                    parsed.version ||
                    STATE_VERSION,

                jobs:
                    parsed.jobs,

                bookings:
                    Array.isArray(parsed.bookings)
                        ? parsed.bookings
                        : [],

                customerProfile:
                    parsed.customerProfile &&
                    typeof parsed.customerProfile === "object"
                        ? parsed.customerProfile
                        : null,

                vehicles:
                    Array.isArray(parsed.vehicles)
                        ? parsed.vehicles
                        : [],

                emergencyRequests:
                    Array.isArray(parsed.emergencyRequests)
                        ? parsed.emergencyRequests
                        : [],

                inventory:
                    Array.isArray(parsed.inventory)
                        ? parsed.inventory
                        : [],

                partRequests:
                    Array.isArray(parsed.partRequests)
                        ? parsed.partRequests
                        : [],

                vendorRequests:
                    Array.isArray(parsed.vendorRequests)
                        ? parsed.vendorRequests
                        : [],

                vendorQuotes:
                    Array.isArray(parsed.vendorQuotes)
                        ? parsed.vendorQuotes
                        : [],

                stockMovements:
                    Array.isArray(parsed.stockMovements)
                        ? parsed.stockMovements
                        : []
            };

        } catch (error) {

            console.error(
                "Unable to read Shift Dynamics workflow state:",
                error
            );

            return createInitialState();
        }
    }


    function writeState(state) {

        const nextState = {
            version: STATE_VERSION,
            jobs:
                Array.isArray(state?.jobs)
                    ? state.jobs
                    : [],

            bookings:
                Array.isArray(state?.bookings)
                    ? state.bookings
                    : [],

            customerProfile:
                state?.customerProfile &&
                typeof state.customerProfile === "object"
                    ? state.customerProfile
                    : null,

            vehicles:
                Array.isArray(state?.vehicles)
                    ? state.vehicles
                    : [],

            emergencyRequests:
                Array.isArray(state?.emergencyRequests)
                    ? state.emergencyRequests
                    : [],

            inventory:
                Array.isArray(state?.inventory)
                    ? state.inventory
                    : [],

            partRequests:
                Array.isArray(state?.partRequests)
                    ? state.partRequests
                    : [],

            vendorRequests:
                Array.isArray(state?.vendorRequests)
                    ? state.vendorRequests
                    : [],

            vendorQuotes:
                Array.isArray(state?.vendorQuotes)
                    ? state.vendorQuotes
                    : [],

            stockMovements:
                Array.isArray(state?.stockMovements)
                    ? state.stockMovements
                    : []
        };

        localStorage.setItem(
            STORAGE_KEY,
            JSON.stringify(nextState)
        );

        window.dispatchEvent(
            new CustomEvent(
                "shiftDynamicsStoreChanged",
                {
                    detail: nextState
                }
            )
        );

        return nextState;
    }


    function getState() {
        return readState();
    }


    function getJobs() {
        return [...readState().jobs];
    }


    function getJob(jobCardNumber) {

        if (!jobCardNumber) {
            return null;
        }

        return (
            readState()
                .jobs
                .find(
                    job =>
                        job.jobCardNumber ===
                        jobCardNumber
                ) || null
        );
    }


    function createJob(job) {

        if (
            !job ||
            !job.jobCardNumber
        ) {
            throw new Error(
                "jobCardNumber is required."
            );
        }

        const state =
            readState();

        const exists =
            state.jobs.some(
                item =>
                    item.jobCardNumber ===
                    job.jobCardNumber
            );

        if (exists) {
            throw new Error(
                `Job ${job.jobCardNumber} already exists.`
            );
        }

        const now =
            new Date().toISOString();

        const newJob = {
            ...job,

            status:
                job.status ||
                "Created",

            createdAt:
                job.createdAt ||
                now,

            updatedAt:
                now
        };

        state.jobs.push(
            newJob
        );

        writeState(
            state
        );

        return newJob;
    }


    function getBookings() {

        return [
            ...(readState().bookings || [])
        ];
    }


    function getBooking(bookingId) {

        if (!bookingId) {
            return null;
        }

        return (
            (readState().bookings || [])
                .find(
                    booking =>
                        booking.bookingId ===
                        bookingId
                ) || null
        );
    }


    function createBooking(booking) {

        if (!booking) {
            throw new Error(
                "Booking data is required."
            );
        }

        const state =
            readState();

        const now =
            new Date().toISOString();

        const bookingId =
            booking.bookingId ||
            `BK-${Date.now()
                .toString(36)
                .toUpperCase()}-${Math.random()
                .toString(36)
                .slice(2, 6)
                .toUpperCase()}`;

        const exists =
            (state.bookings || [])
                .some(
                    item =>
                        item.bookingId ===
                        bookingId
                );

        if (exists) {
            throw new Error(
                `Booking ${bookingId} already exists.`
            );
        }

        const newBooking = {
            ...booking,

            bookingId,

            status:
                booking.status ||
                "Submitted",

            createdAt:
                booking.createdAt ||
                now,

            updatedAt:
                now
        };

        state.bookings =
            state.bookings || [];

        state.bookings.push(
            newBooking
        );

        writeState(
            state
        );

        return newBooking;
    }


    function updateBooking(
        bookingId,
        patch
    ) {

        if (!bookingId) {
            return null;
        }

        const state =
            readState();

        state.bookings =
            state.bookings || [];

        const index =
            state.bookings.findIndex(
                booking =>
                    booking.bookingId ===
                    bookingId
            );

        if (index === -1) {
            return null;
        }

        const current =
            state.bookings[index];

        const updated = {
            ...current,
            ...(patch || {}),

            bookingId:
                current.bookingId,

            updatedAt:
                new Date().toISOString()
        };

        state.bookings[index] =
            updated;

        writeState(
            state
        );

        return updated;
    }

    function updateJob(
        jobCardNumber,
        patch
    ) {

        if (!jobCardNumber) {
            return null;
        }

        const state =
            readState();

        const index =
            state.jobs.findIndex(
                job =>
                    job.jobCardNumber ===
                    jobCardNumber
            );

        if (index === -1) {
            return null;
        }

        const current =
            state.jobs[index];

        const updated = {
            ...current,
            ...patch,

            updatedAt:
                new Date().toISOString()
        };

        state.jobs[index] =
            updated;

        writeState(
            state
        );

        return updated;
    }


    function getCustomerProfile() {

        const profile =
            readState().customerProfile;

        return profile
            ? { ...profile }
            : null;
    }


    function saveCustomerProfile(profile) {

        if (
            !profile ||
            typeof profile !== "object"
        ) {
            throw new Error(
                "Customer profile data is required."
            );
        }

        const state =
            readState();

        const now =
            new Date().toISOString();

        const current =
            state.customerProfile || {};

        state.customerProfile = {
            ...current,
            ...profile,

            createdAt:
                current.createdAt ||
                profile.createdAt ||
                now,

            updatedAt:
                now
        };

        writeState(
            state
        );

        return {
            ...state.customerProfile
        };
    }


    function getVehicles() {

        return [
            ...(readState().vehicles || [])
        ];
    }


    function getVehicle(vehicleId) {

        if (!vehicleId) {
            return null;
        }

        return (
            (readState().vehicles || [])
                .find(
                    vehicle =>
                        vehicle.vehicleId ===
                        vehicleId
                ) || null
        );
    }


    function createVehicle(vehicle) {

        if (!vehicle) {
            throw new Error(
                "Vehicle data is required."
            );
        }

        const state =
            readState();

        const now =
            new Date().toISOString();

        const vehicleId =
            vehicle.vehicleId ||
            `VH-${Date.now()
                .toString(36)
                .toUpperCase()}-${Math.random()
                .toString(36)
                .slice(2, 6)
                .toUpperCase()}`;

        state.vehicles =
            state.vehicles || [];

        const exists =
            state.vehicles.some(
                item =>
                    item.vehicleId ===
                    vehicleId
            );

        if (exists) {
            throw new Error(
                `Vehicle ${vehicleId} already exists.`
            );
        }

        const newVehicle = {
            ...vehicle,

            vehicleId,

            createdAt:
                vehicle.createdAt ||
                now,

            updatedAt:
                now
        };

        state.vehicles.push(
            newVehicle
        );

        writeState(
            state
        );

        return {
            ...newVehicle
        };
    }


    function updateVehicle(
        vehicleId,
        patch
    ) {

        if (!vehicleId) {
            return null;
        }

        const state =
            readState();

        state.vehicles =
            state.vehicles || [];

        const index =
            state.vehicles.findIndex(
                vehicle =>
                    vehicle.vehicleId ===
                    vehicleId
            );

        if (index === -1) {
            return null;
        }

        const current =
            state.vehicles[index];

        const updated = {
            ...current,
            ...(patch || {}),

            vehicleId:
                current.vehicleId,

            updatedAt:
                new Date().toISOString()
        };

        state.vehicles[index] =
            updated;

        writeState(
            state
        );

        return {
            ...updated
        };
    }


    function deleteVehicle(vehicleId) {

        if (!vehicleId) {
            return false;
        }

        const state =
            readState();

        state.vehicles =
            state.vehicles || [];

        const before =
            state.vehicles.length;

        state.vehicles =
            state.vehicles.filter(
                vehicle =>
                    vehicle.vehicleId !==
                    vehicleId
            );

        if (
            state.vehicles.length ===
            before
        ) {
            return false;
        }

        writeState(
            state
        );

        return true;
    }

    /* =====================================================
       EMERGENCY REQUESTS
       ===================================================== */

    function getEmergencyRequests() {

        return [
            ...(readState().emergencyRequests || [])
        ];
    }


    function getEmergencyRequest(
        requestId
    ) {

        if (!requestId) {
            return null;
        }

        return (
            (readState().emergencyRequests || [])
                .find(
                    request =>
                        request.requestId ===
                        requestId
                ) || null
        );
    }


    function createEmergencyRequest(
        request
    ) {

        if (!request) {
            throw new Error(
                "Emergency request data is required."
            );
        }

        const state =
            readState();

        state.emergencyRequests =
            state.emergencyRequests || [];

        const now =
            new Date().toISOString();

        const requestId =
            request.requestId ||
            `#EMG-${
                Date.now()
                    .toString()
                    .slice(-6)
            }${
                Math.floor(
                    10 + Math.random() * 90
                )
            }`;


        const exists =
            state.emergencyRequests.some(
                item =>
                    item.requestId ===
                    requestId
            );

        if (exists) {
            throw new Error(
                `Emergency request ${requestId} already exists.`
            );
        }


        const newRequest = {
            ...request,

            requestId,

            status:
                request.status ||
                "Requested",

            createdAt:
                request.createdAt ||
                now,

            updatedAt:
                now
        };


        state.emergencyRequests.push(
            newRequest
        );

        writeState(
            state
        );

        return {
            ...newRequest
        };
    }


    function updateEmergencyRequest(
        requestId,
        patch
    ) {

        if (!requestId) {
            return null;
        }

        const state =
            readState();

        state.emergencyRequests =
            state.emergencyRequests || [];

        const index =
            state.emergencyRequests.findIndex(
                request =>
                    request.requestId ===
                    requestId
            );

        if (index === -1) {
            return null;
        }


        const current =
            state.emergencyRequests[index];

        const updated = {
            ...current,
            ...(patch || {}),

            requestId:
                current.requestId,

            updatedAt:
                new Date().toISOString()
        };


        state.emergencyRequests[index] =
            updated;

        writeState(
            state
        );

        return {
            ...updated
        };
    }


    /* =====================================================
       SHARED STOREKEEPER WORKFLOW
       ===================================================== */

    function copyList(value) {
        return JSON.parse(JSON.stringify(Array.isArray(value) ? value : []));
    }

    function getInventory() {
        return copyList(readState().inventory);
    }

    function saveInventory(items) {
        const state = readState();
        state.inventory = copyList(items);
        writeState(state);
        return copyList(state.inventory);
    }

    function getPartRequests() {
        return copyList(readState().partRequests);
    }

    function createPartRequest(request) {
        if (!request || typeof request !== "object") {
            throw new Error("Part request data is required.");
        }

        const state = readState();
        const now = new Date().toISOString();
        const requestId =
            request.requestId || `PR-${Date.now().toString().slice(-8)}`;

        if (state.partRequests.some(item => item.requestId === requestId)) {
            return null;
        }

        const created = {
            ...request,
            requestId,
            status: request.status || "pending",
            createdAt: request.createdAt || now,
            updatedAt: now
        };

        state.partRequests.unshift(created);
        writeState(state);
        return { ...created };
    }

    function updatePartRequest(requestId, patch) {
        const state = readState();
        const index = state.partRequests.findIndex(
            item => item.requestId === requestId
        );

        if (index < 0) {
            return null;
        }

        const current = state.partRequests[index];
        const updated = {
            ...current,
            ...(patch || {}),
            requestId: current.requestId,
            updatedAt: new Date().toISOString()
        };

        state.partRequests[index] = updated;
        writeState(state);
        return { ...updated };
    }

    function getVendorRequests() {
        return copyList(readState().vendorRequests);
    }

    function createVendorRequest(request) {
        if (!request || typeof request !== "object") {
            throw new Error("Vendor request data is required.");
        }

        const state = readState();
        const existing = state.vendorRequests.find(
            item =>
                request.sourceRequestId &&
                item.sourceRequestId === request.sourceRequestId
        );

        if (existing) {
            return { ...existing };
        }

        const now = new Date().toISOString();
        const created = {
            ...request,
            vendorRequestId:
                request.vendorRequestId ||
                `VR-${Date.now().toString().slice(-8)}`,
            status: request.status || "Awaiting Quotes",
            createdAt: request.createdAt || now,
            updatedAt: now
        };

        state.vendorRequests.unshift(created);
        writeState(state);
        return { ...created };
    }

    function updateVendorRequest(vendorRequestId, patch) {
        const state = readState();
        const index = state.vendorRequests.findIndex(
            item => item.vendorRequestId === vendorRequestId
        );

        if (index < 0) {
            return null;
        }

        const current = state.vendorRequests[index];
        const updated = {
            ...current,
            ...(patch || {}),
            vendorRequestId: current.vendorRequestId,
            updatedAt: new Date().toISOString()
        };

        state.vendorRequests[index] = updated;
        writeState(state);
        return { ...updated };
    }

    function getVendorQuotes() {
        return copyList(readState().vendorQuotes);
    }

    function createVendorQuote(quote) {
        if (!quote || typeof quote !== "object") {
            throw new Error("Vendor quotation data is required.");
        }

        if (!quote.vendorRequestId) {
            throw new Error("vendorRequestId is required.");
        }

        const state = readState();
        const existing = state.vendorQuotes.find(
            item => item.vendorRequestId === quote.vendorRequestId
        );

        if (existing) {
            return { ...existing };
        }

        const now = new Date().toISOString();
        const created = {
            ...quote,
            quoteId:
                quote.quoteId ||
                `QT-${Date.now().toString().slice(-8)}`,
            status: quote.status || "Pending Review",
            createdAt: quote.createdAt || now,
            updatedAt: now
        };

        state.vendorQuotes.unshift(created);
        writeState(state);
        return { ...created };
    }

    function updateVendorQuote(quoteId, patch) {
        const state = readState();
        const index = state.vendorQuotes.findIndex(
            item => item.quoteId === quoteId
        );

        if (index < 0) {
            return null;
        }

        const current = state.vendorQuotes[index];
        const updated = {
            ...current,
            ...(patch || {}),
            quoteId: current.quoteId,
            vendorRequestId: current.vendorRequestId,
            updatedAt: new Date().toISOString()
        };

        state.vendorQuotes[index] = updated;
        writeState(state);
        return { ...updated };
    }

    function getStockMovements() {
        return copyList(readState().stockMovements);
    }

    function createStockMovement(movement) {
        if (!movement || typeof movement !== "object") {
            throw new Error("Stock movement data is required.");
        }

        const state = readState();
        const created = {
            ...movement,
            movementId:
                movement.movementId ||
                `SM-${Date.now().toString().slice(-8)}`,
            createdAt: movement.createdAt || new Date().toISOString()
        };

        state.stockMovements.unshift(created);
        writeState(state);
        return { ...created };
    }


    function subscribe(callback) {

        if (
            typeof callback !==
            "function"
        ) {
            return () => {};
        }

        const customHandler =
            event => {
                callback(
                    event.detail ||
                    readState()
                );
            };

        const storageHandler =
            event => {

                if (
                    event.key ===
                    STORAGE_KEY
                ) {
                    callback(
                        readState()
                    );
                }
            };


        window.addEventListener(
            "shiftDynamicsStoreChanged",
            customHandler
        );

        window.addEventListener(
            "storage",
            storageHandler
        );


        return () => {

            window.removeEventListener(
                "shiftDynamicsStoreChanged",
                customHandler
            );

            window.removeEventListener(
                "storage",
                storageHandler
            );
        };
    }


    return {
        getState,
        getJobs,
        getJob,
        createJob,
        updateJob,

        getBookings,
        getBooking,
        createBooking,
        updateBooking,

        getCustomerProfile,
        saveCustomerProfile,

        getVehicles,
        getVehicle,
        createVehicle,
        updateVehicle,
        deleteVehicle,

        
        getEmergencyRequests,
        getEmergencyRequest,
        createEmergencyRequest,
        updateEmergencyRequest,

        getInventory,
        saveInventory,

        getPartRequests,
        createPartRequest,
        updatePartRequest,

        getVendorRequests,
        createVendorRequest,
        updateVendorRequest,

        getVendorQuotes,
        createVendorQuote,
        updateVendorQuote,

        getStockMovements,
        createStockMovement,

subscribe
    };

})();
