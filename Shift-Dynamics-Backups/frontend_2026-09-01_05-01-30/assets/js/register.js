document.addEventListener("DOMContentLoaded", () => {

    const form = document.getElementById("customerRegisterForm");

    const fullName = document.getElementById("fullName");
    const email = document.getElementById("email");
    const phone = document.getElementById("phone");
    const password = document.getElementById("password");
    const confirmPassword = document.getElementById("confirmPassword");
    const terms = document.getElementById("terms");

    const fullNameError = document.getElementById("fullNameError");
    const emailError = document.getElementById("emailError");
    const phoneError = document.getElementById("phoneError");
    const passwordError = document.getElementById("passwordError");
    const confirmPasswordError = document.getElementById("confirmPasswordError");
    const termsError = document.getElementById("termsError");

    const passwordToggle = document.getElementById("passwordToggle");
    const confirmPasswordToggle = document.getElementById("confirmPasswordToggle");

    const registerButton = document.getElementById("registerButton");
    const registerMessage = document.getElementById("registerMessage");

    const currentYear = document.getElementById("currentYear");


    /* =====================================================
       YEAR
       ===================================================== */

    if (currentYear) {
        currentYear.textContent = new Date().getFullYear();
    }


    /* =====================================================
       PASSWORD VISIBILITY
       ===================================================== */

    function setupPasswordToggle(button, input) {

        if (!button || !input) {
            return;
        }

        button.addEventListener("click", () => {

            const isPassword = input.type === "password";

            input.type = isPassword ? "text" : "password";

            const icon = button.querySelector("i");

            if (icon) {
                icon.className = isPassword
                    ? "bi bi-eye-slash"
                    : "bi bi-eye";
            }

            button.setAttribute(
                "aria-label",
                isPassword
                    ? "Hide password"
                    : "Show password"
            );
        });
    }

    setupPasswordToggle(
        passwordToggle,
        password
    );

    setupPasswordToggle(
        confirmPasswordToggle,
        confirmPassword
    );


    /* =====================================================
       HELPERS
       ===================================================== */

    function getWrapper(input) {
        return input.closest(".sd-input-wrapper");
    }


    function setFieldError(input, errorElement, message) {

        const wrapper = getWrapper(input);

        if (wrapper) {
            wrapper.classList.add("sd-has-error");
        }

        if (errorElement) {
            errorElement.textContent = message;
        }
    }


    function clearFieldError(input, errorElement) {

        const wrapper = getWrapper(input);

        if (wrapper) {
            wrapper.classList.remove("sd-has-error");
        }

        if (errorElement) {
            errorElement.textContent = "";
        }
    }


    function showMessage(type, message) {

        registerMessage.className =
            `sd-register-message sd-${type}`;

        registerMessage.textContent = message;
    }


    function clearMessage() {

        registerMessage.className =
            "sd-register-message";

        registerMessage.textContent = "";
    }


    /* =====================================================
       VALIDATION
       ===================================================== */

    function validateFullName() {

        const value = fullName.value.trim();

        clearFieldError(
            fullName,
            fullNameError
        );

        if (!value) {

            setFieldError(
                fullName,
                fullNameError,
                "Please enter your full name."
            );

            return false;
        }

        if (value.length < 3) {

            setFieldError(
                fullName,
                fullNameError,
                "Full name must contain at least 3 characters."
            );

            return false;
        }

        return true;
    }


    function validateEmail() {

        const value = email.value.trim();

        clearFieldError(
            email,
            emailError
        );

        const emailPattern =
            /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!value) {

            setFieldError(
                email,
                emailError,
                "Please enter your email address."
            );

            return false;
        }

        if (!emailPattern.test(value)) {

            setFieldError(
                email,
                emailError,
                "Please enter a valid email address."
            );

            return false;
        }

        return true;
    }


    function validatePhone() {

        const value = phone.value.trim();

        clearFieldError(
            phone,
            phoneError
        );

        const normalizedPhone =
            value.replace(/[\s()-]/g, "");

        const phonePattern =
            /^(?:\+94|0)?7\d{8}$/;

        if (!value) {

            setFieldError(
                phone,
                phoneError,
                "Please enter your phone number."
            );

            return false;
        }

        if (!phonePattern.test(normalizedPhone)) {

            setFieldError(
                phone,
                phoneError,
                "Please enter a valid Sri Lankan mobile number."
            );

            return false;
        }

        return true;
    }


    function validatePassword() {

        const value = password.value;

        clearFieldError(
            password,
            passwordError
        );

        if (!value) {

            setFieldError(
                password,
                passwordError,
                "Please create a password."
            );

            return false;
        }

        if (value.length < 8) {

            setFieldError(
                password,
                passwordError,
                "Password must contain at least 8 characters."
            );

            return false;
        }

        if (!/[A-Z]/.test(value)) {

            setFieldError(
                password,
                passwordError,
                "Include at least one uppercase letter."
            );

            return false;
        }

        if (!/[a-z]/.test(value)) {

            setFieldError(
                password,
                passwordError,
                "Include at least one lowercase letter."
            );

            return false;
        }

        if (!/\d/.test(value)) {

            setFieldError(
                password,
                passwordError,
                "Include at least one number."
            );

            return false;
        }

        return true;
    }


    function validateConfirmPassword() {

        const value = confirmPassword.value;

        clearFieldError(
            confirmPassword,
            confirmPasswordError
        );

        if (!value) {

            setFieldError(
                confirmPassword,
                confirmPasswordError,
                "Please confirm your password."
            );

            return false;
        }

        if (value !== password.value) {

            setFieldError(
                confirmPassword,
                confirmPasswordError,
                "Passwords do not match."
            );

            return false;
        }

        return true;
    }


    function validateTerms() {

        termsError.textContent = "";

        if (!terms.checked) {

            termsError.textContent =
                "Please accept the Terms of Service and Privacy Policy.";

            return false;
        }

        return true;
    }


    /* =====================================================
       LIVE VALIDATION
       ===================================================== */

    fullName.addEventListener("blur", validateFullName);
    email.addEventListener("blur", validateEmail);
    phone.addEventListener("blur", validatePhone);
    password.addEventListener("blur", validatePassword);
    confirmPassword.addEventListener(
        "blur",
        validateConfirmPassword
    );


    fullName.addEventListener("input", () => {

        clearFieldError(
            fullName,
            fullNameError
        );

        clearMessage();
    });


    email.addEventListener("input", () => {

        clearFieldError(
            email,
            emailError
        );

        clearMessage();
    });


    phone.addEventListener("input", () => {

        clearFieldError(
            phone,
            phoneError
        );

        clearMessage();
    });


    password.addEventListener("input", () => {

        clearFieldError(
            password,
            passwordError
        );

        if (confirmPassword.value) {
            validateConfirmPassword();
        }

        clearMessage();
    });


    confirmPassword.addEventListener(
        "input",
        () => {

            clearFieldError(
                confirmPassword,
                confirmPasswordError
            );

            clearMessage();
        }
    );


    terms.addEventListener("change", () => {

        termsError.textContent = "";

        clearMessage();
    });


    /* =====================================================
       FORM SUBMIT
       ===================================================== */

    form.addEventListener("submit", async (event) => {

        event.preventDefault();

        clearMessage();


        const isFullNameValid =
            validateFullName();

        const isEmailValid =
            validateEmail();

        const isPhoneValid =
            validatePhone();

        const isPasswordValid =
            validatePassword();

        const isConfirmPasswordValid =
            validateConfirmPassword();

        const isTermsValid =
            validateTerms();


        const formIsValid =
            isFullNameValid &&
            isEmailValid &&
            isPhoneValid &&
            isPasswordValid &&
            isConfirmPasswordValid &&
            isTermsValid;


        if (!formIsValid) {

            showMessage(
                "error",
                "Please correct the highlighted fields before creating your account."
            );

            return;
        }


        /* =================================================
           CUSTOMER DATA

           IMPORTANT:
           Public registration always creates Customer role.
           Do not allow role selection from the browser.
           Backend must also enforce this rule.
           ================================================= */

        const customerData = {

            fullName:
                fullName.value.trim(),

            email:
                email.value.trim().toLowerCase(),

            phone:
                phone.value.trim(),

            password:
                password.value,

            role:
                "Customer"
        };


        /* =================================================
           FRONTEND DEMO STATE

           Backend API will replace this section later.
           ================================================= */

        registerButton.disabled = true;

        const originalButtonContent =
            registerButton.innerHTML;

        registerButton.innerHTML = `
            <span>Creating Account...</span>
            <i class="bi bi-arrow-repeat"></i>
        `;


        try {

            await new Promise(resolve => {
                setTimeout(resolve, 800);
            });


            console.log(
                "Customer registration data:",
                {
                    fullName: customerData.fullName,
                    email: customerData.email,
                    phone: customerData.phone,
                    role: customerData.role
                }
            );


            showMessage(
                "success",
                "Customer account details are valid. Backend registration will be connected next."
            );


            /*
             * Later, when the .NET backend is ready,
             * replace the demo section with something like:
             *
             * const response = await fetch(
             *     "https://localhost:xxxx/api/auth/register/customer",
             *     {
             *         method: "POST",
             *         headers: {
             *             "Content-Type": "application/json"
             *         },
             *         body: JSON.stringify(customerData)
             *     }
             * );
             *
             * const result = await response.json();
             *
             * if (!response.ok) {
             *     throw new Error(
             *         result.message || "Registration failed."
             *     );
             * }
             *
             * window.location.href = "login.html";
             */

        } catch (error) {

            console.error(error);

            showMessage(
                "error",
                error.message ||
                "Unable to create your account. Please try again."
            );

        } finally {

            registerButton.disabled = false;

            registerButton.innerHTML =
                originalButtonContent;
        }

    });

});