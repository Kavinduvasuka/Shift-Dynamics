/* =========================================================
   SHIFT DYNAMICS
   LOGIN PAGE
   ========================================================= */

document.addEventListener("DOMContentLoaded", () => {

    /* =====================================================
       ELEMENTS
       ===================================================== */

    const loginForm = document.getElementById("loginForm");

    const emailInput = document.getElementById("loginEmail");
    const passwordInput = document.getElementById("loginPassword");

    const emailError = document.getElementById("emailError");
    const passwordError = document.getElementById("passwordError");

    const passwordToggle = document.getElementById("passwordToggle");
    const passwordToggleIcon = document.getElementById("passwordToggleIcon");

    const loginSubmit = document.getElementById("loginSubmit");
    const loginButtonText = document.getElementById("loginButtonText");
    const loginButtonIcon = document.getElementById("loginButtonIcon");

    const loginMessage = document.getElementById("loginMessage");

    const forgotPasswordLink =
        document.getElementById("forgotPasswordLink");

    const registerLink =
        document.getElementById("registerLink");

    const currentYear =
        document.getElementById("currentYear");


    if (
        !loginForm ||
        !emailInput ||
        !passwordInput
    ) {
        return;
    }


    /* =====================================================
       CURRENT YEAR
       ===================================================== */

    if (currentYear) {
        currentYear.textContent =
            new Date().getFullYear();
    }


    /* =====================================================
       SHOW / HIDE PASSWORD
       ===================================================== */

    if (passwordToggle) {

        passwordToggle.addEventListener("click", () => {

            const passwordVisible =
                passwordInput.type === "text";


            passwordInput.type =
                passwordVisible
                    ? "password"
                    : "text";


            if (passwordToggleIcon) {

                passwordToggleIcon.className =
                    passwordVisible
                        ? "bi bi-eye"
                        : "bi bi-eye-slash";
            }


            passwordToggle.setAttribute(
                "aria-label",
                passwordVisible
                    ? "Show password"
                    : "Hide password"
            );


            passwordToggle.setAttribute(
                "aria-pressed",
                String(!passwordVisible)
            );

        });
    }


    /* =====================================================
       HELPERS
       ===================================================== */

    function isValidEmail(email) {

        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

    }


    function setFieldError(
        input,
        errorElement,
        message
    ) {

        const wrapper =
            input.closest(".sd-input-wrapper");


        if (wrapper) {
            wrapper.classList.add(
                "sd-has-error"
            );
        }


        input.setAttribute(
            "aria-invalid",
            "true"
        );


        if (errorElement) {
            errorElement.textContent = message;
        }

    }


    function clearFieldError(
        input,
        errorElement
    ) {

        const wrapper =
            input.closest(".sd-input-wrapper");


        if (wrapper) {
            wrapper.classList.remove(
                "sd-has-error"
            );
        }


        input.removeAttribute(
            "aria-invalid"
        );


        if (errorElement) {
            errorElement.textContent = "";
        }

    }


    function clearMessage() {

        if (!loginMessage) {
            return;
        }


        loginMessage.textContent = "";

        loginMessage.classList.remove(
            "sd-error",
            "sd-success"
        );

    }


    function showMessage(
        message,
        type
    ) {

        if (!loginMessage) {
            return;
        }


        loginMessage.textContent = message;


        loginMessage.classList.remove(
            "sd-error",
            "sd-success"
        );


        loginMessage.classList.add(
            type === "success"
                ? "sd-success"
                : "sd-error"
        );

    }


    /* =====================================================
       EMAIL VALIDATION
       ===================================================== */

    function validateEmail() {

        const email =
            emailInput.value.trim();


        clearFieldError(
            emailInput,
            emailError
        );


        if (!email) {

            setFieldError(
                emailInput,
                emailError,
                "Please enter your email address."
            );

            return false;
        }


        if (!isValidEmail(email)) {

            setFieldError(
                emailInput,
                emailError,
                "Please enter a valid email address."
            );

            return false;
        }


        return true;
    }


    /* =====================================================
       PASSWORD VALIDATION
       ===================================================== */

    function validatePassword() {

        const password =
            passwordInput.value;


        clearFieldError(
            passwordInput,
            passwordError
        );


        if (!password) {

            setFieldError(
                passwordInput,
                passwordError,
                "Please enter your password."
            );

            return false;
        }


        if (password.length < 6) {

            setFieldError(
                passwordInput,
                passwordError,
                "Password must contain at least 6 characters."
            );

            return false;
        }


        return true;
    }


    /* =====================================================
       LIVE VALIDATION
       ===================================================== */

    emailInput.addEventListener(
        "input",
        () => {

            clearMessage();

            if (emailInput.value.trim()) {

                clearFieldError(
                    emailInput,
                    emailError
                );
            }

        }
    );


    passwordInput.addEventListener(
        "input",
        () => {

            clearMessage();

            if (passwordInput.value) {

                clearFieldError(
                    passwordInput,
                    passwordError
                );
            }

        }
    );


    emailInput.addEventListener(
        "blur",
        validateEmail
    );


    passwordInput.addEventListener(
        "blur",
        validatePassword
    );


    /* =====================================================
       LOADING STATE
       ===================================================== */

    function setLoading(isLoading) {

        if (!loginSubmit) {
            return;
        }


        loginSubmit.disabled = isLoading;


        if (loginButtonText) {

            loginButtonText.textContent =
                isLoading
                    ? "Signing In..."
                    : "Sign In";
        }


        if (loginButtonIcon) {

            loginButtonIcon.className =
                isLoading
                    ? "bi bi-arrow-repeat"
                    : "bi bi-arrow-right";
        }

    }


    /* =====================================================
       LOGIN SUBMIT
       ===================================================== */

    loginForm.addEventListener(
        "submit",
        (event) => {

            event.preventDefault();


            clearMessage();


            const emailValid =
                validateEmail();


            const passwordValid =
                validatePassword();


            if (
                !emailValid ||
                !passwordValid
            ) {

                showMessage(
                    "Please check the highlighted fields and try again.",
                    "error"
                );

                return;
            }


            /*
             * Frontend-only stage.
             *
             * Later this section will call the backend
             * authentication API.
             */

            setLoading(true);


            window.setTimeout(() => {
                setLoading(false);

                showMessage(
                    "Login successful. Redirecting to your dashboard...",
                    "success"
                );

                window.setTimeout(() => {
                    window.location.href = "customer/dashboard.html";
                }, 500);

            }, 700);

        }
    );
});