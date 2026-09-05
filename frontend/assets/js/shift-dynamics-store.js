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
            jobs: []
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
                    parsed.jobs
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
        subscribe
    };

})();