/* =========================================================
   SHIFT DYNAMICS - SHARED DASHBOARD STATES

   Usage examples:

   ShiftDashboardState.loading(container);

   ShiftDashboardState.empty(
       container,
       "No job cards found",
       "There are no job cards to display."
   );

   ShiftDashboardState.error(
       container,
       "Could not load job cards",
       "Please try again.",
       () => loadJobCards()
   );

   ShiftDashboardState.clear(container);
========================================================= */

window.ShiftDashboardState = (() => {

    function resolveContainer(target) {

        if (typeof target === "string") {
            return document.querySelector(target);
        }

        return target;

    }


    function escapeHTML(value) {

        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");

    }


    function loading(
        target,
        title = "Loading data...",
        message = "Please wait while we retrieve the latest information."
    ) {

        const container = resolveContainer(target);

        if (!container) {
            return;
        }


        container.innerHTML = `

            <div class="sd-api-state sd-api-state-loading">

                <div class="sd-api-state-inner">

                    <div class="sd-api-state-icon">

                        <div
                            class="sd-api-spinner"
                            aria-hidden="true"
                        ></div>

                    </div>

                    <h3>
                        ${escapeHTML(title)}
                    </h3>

                    <p>
                        ${escapeHTML(message)}
                    </p>

                    <div
                        class="sd-api-skeleton"
                        aria-hidden="true"
                    >
                        <div class="sd-api-skeleton-line"></div>
                        <div class="sd-api-skeleton-line"></div>
                        <div class="sd-api-skeleton-line"></div>
                    </div>

                </div>

            </div>

        `;

    }


    function empty(
        target,
        title = "No records found",
        message = "There is nothing to display right now."
    ) {

        const container = resolveContainer(target);

        if (!container) {
            return;
        }


        container.innerHTML = `

            <div class="sd-api-state sd-api-state-empty">

                <div class="sd-api-state-inner">

                    <div class="sd-api-state-icon">
                        <i class="bi bi-inbox"></i>
                    </div>

                    <h3>
                        ${escapeHTML(title)}
                    </h3>

                    <p>
                        ${escapeHTML(message)}
                    </p>

                </div>

            </div>

        `;

    }


    function error(
        target,
        title = "Something went wrong",
        message = "We could not load this information.",
        retryCallback = null
    ) {

        const container = resolveContainer(target);

        if (!container) {
            return;
        }


        container.innerHTML = `

            <div class="sd-api-state sd-api-state-error">

                <div class="sd-api-state-inner">

                    <div class="sd-api-state-icon">
                        <i class="bi bi-exclamation-triangle"></i>
                    </div>

                    <h3>
                        ${escapeHTML(title)}
                    </h3>

                    <p>
                        ${escapeHTML(message)}
                    </p>

                    ${
                        typeof retryCallback === "function"
                            ? `
                                <button
                                    type="button"
                                    class="sd-api-state-retry"
                                >
                                    <i class="bi bi-arrow-clockwise"></i>
                                    Retry
                                </button>
                            `
                            : ""
                    }

                </div>

            </div>

        `;


        if (
            typeof retryCallback === "function"
        ) {

            const retryButton =
                container.querySelector(
                    ".sd-api-state-retry"
                );


            retryButton?.addEventListener(
                "click",
                retryCallback
            );

        }

    }


    function clear(target) {

        const container = resolveContainer(target);

        if (!container) {
            return;
        }

        container.innerHTML = "";

    }


    return {
        loading,
        empty,
        error,
        clear
    };

})();
