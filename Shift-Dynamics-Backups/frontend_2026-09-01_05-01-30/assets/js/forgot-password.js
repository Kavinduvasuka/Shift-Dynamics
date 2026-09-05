document.addEventListener("DOMContentLoaded", () => {

    const form =
        document.getElementById("forgotPasswordForm");

    const email =
        document.getElementById("email");

    const emailWrapper =
        document.getElementById("emailWrapper");

    const emailError =
        document.getElementById("emailError");

    const forgotMessage =
        document.getElementById("forgotMessage");

    const resetButton =
        document.getElementById("resetButton");

    const currentYear =
        document.getElementById("currentYear");


    /* =========================
       YEAR
       ========================= */

    if (currentYear) {
        currentYear.textContent =
            new Date().getFullYear();
    }


    /* =========================
       MESSAGE HELPERS
       ========================= */

    function showMessage(type, message) {

        forgotMessage.className =
            `sd-form-message sd-${type}`;

        forgotMessage.textContent =
            message;
    }


    function clearMessage() {

        forgotMessage.className =
            "sd-form-message";

        forgotMessage.textContent = "";
    }


    /* =========================
       FIELD HELPERS
       ========================= */

    function showEmailError(message) {

        emailWrapper.classList.add(
            "sd-has-error"
        );

        emailError.textContent =
            message;
    }


    function clearEmailError() {

        emailWrapper.classList.remove(
            "sd-has-error"
        );

        emailError.textContent = "";
    }


    /* =========================
       VALIDATION
       ========================= */

    function validateEmail() {

        const value =
            email.value.trim();

        clearEmailError();

        const emailPattern =
            /^[^\s@]+@[^\s@]+\.[^\s@]+$/;


        if (!value) {

            showEmailError(
                "Please enter your email address."
            );

            return false;
        }


        if (!emailPattern.test(value)) {

            showEmailError(
                "Please enter a valid email address."
            );

            return false;
        }


        return true;
    }


    /* =========================
       LIVE VALIDATION
       ========================= */

    email.addEventListener(
        "blur",
        validateEmail
    );


    email.addEventListener(
        "input",
        () => {

            clearEmailError();
            clearMessage();

        }
    );


    /* =========================
       FORM SUBMIT
       ========================= */

    form.addEventListener(
        "submit",
        async (event) => {

            event.preventDefault();

            clearMessage();


            if (!validateEmail()) {

                showMessage(
                    "error",
                    "Please enter a valid email address before continuing."
                );

                return;
            }


            const emailAddress =
                email.value.trim().toLowerCase();


            /* =====================
               FRONTEND DEMO ONLY
               ===================== */

            resetButton.disabled = true;


            const originalButtonContent =
                resetButton.innerHTML;


            resetButton.innerHTML = `
                <span>Processing...</span>
                <i class="bi bi-arrow-repeat"></i>
            `;


            try {

                await new Promise(
                    resolve => {

                        setTimeout(
                            resolve,
                            700
                        );

                    }
                );


                console.log(
                    "Forgot password request:",
                    emailAddress
                );


                /*
                 * Frontend prototype only.
                 *
                 * For security, use the same
                 * response whether the email
                 * exists or not.
                 *
                 * Backend will later:
                 *
                 * 1. Verify account.
                 * 2. Generate secure reset token.
                 * 3. Send reset email.
                 * 4. Open reset-password page.
                 */


                showMessage(
                    "success",
                    "If an account exists for this email, password reset instructions will be sent."
                );


                email.value = "";


            } catch (error) {

                console.error(error);


                showMessage(
                    "error",
                    "Unable to process the request. Please try again."
                );


            } finally {

                resetButton.disabled = false;

                resetButton.innerHTML =
                    originalButtonContent;

            }

        }
    );

});