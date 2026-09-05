document.addEventListener("DOMContentLoaded", () => {

    // Form Elements
    const form =
        document.getElementById("customerRegisterForm");

    const fullName =
        document.getElementById("fullName");

    const email =
        document.getElementById("email");

    const phone =
        document.getElementById("phone");

    const password =
        document.getElementById("password");

    const confirmPassword =
        document.getElementById("confirmPassword");

    const terms =
        document.getElementById("terms");

    const fullNameError =
        document.getElementById("fullNameError");

    const emailError =
        document.getElementById("emailError");

    const phoneError =
        document.getElementById("phoneError");

    const passwordError =
        document.getElementById("passwordError");

    const confirmPasswordError =
        document.getElementById("confirmPasswordError");

    const termsError =
        document.getElementById("termsError");

    const passwordToggle =
        document.getElementById("passwordToggle");

    const confirmPasswordToggle =
        document.getElementById("confirmPasswordToggle");

    const registerButton =
        document.getElementById("registerButton");

    const registerMessage =
        document.getElementById("registerMessage");

    const currentYear =
        document.getElementById("currentYear");


    // Current Year
    if (currentYear) {
        currentYear.textContent =
            new Date().getFullYear();
    }


    // Password Visibility
    function setupPasswordToggle(button, input) {

        if (!button || !input) {
            return;
        }

        button.addEventListener("click", () => {

            const isPassword =
                input.type === "password";

            input.type =
                isPassword
                    ? "text"
                    : "password";

            const icon =
                button.querySelector("i");

            if (icon) {
                icon.className =
                    isPassword
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


    // Helpers
    function getWrapper(input) {
        return input.closest(
            ".sd-input-wrapper"
        );
    }


    function setFieldError(
        input,
        errorElement,
        message
    ) {

        const wrapper =
            getWrapper(input);

        if (wrapper) {
            wrapper.classList.add(
                "sd-has-error"
            );
        }

        if (errorElement) {
            errorElement.textContent =
                message;
        }
    }


    function clearFieldError(
        input,
        errorElement
    ) {

        const wrapper =
            getWrapper(input);

        if (wrapper) {
            wrapper.classList.remove(
                "sd-has-error"
            );
        }

        if (errorElement) {
            errorElement.textContent = "";
        }
    }


    function showMessage(type, message) {

        registerMessage.className =
            `sd-register-message sd-${type}`;

        registerMessage.textContent =
            message;
    }


    function clearMessage() {

        registerMessage.className =
            "sd-register-message";

        registerMessage.textContent = "";
    }


    // Validation
    function validateFullName() {

        const value =
            fullName.value.trim();

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

        const value =
            email.value.trim();

        const emailPattern =
            /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        clearFieldError(
            email,
            emailError
        );

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

        const value =
            phone.value.trim();

        const normalizedPhone =
            value.replace(
                /[\s()-]/g,
                ""
            );

        const phonePattern =
            /^(?:\+94|0)?7\d{8}$/;

        clearFieldError(
            phone,
            phoneError
        );

        if (!value) {

            setFieldError(
                phone,
                phoneError,
                "Please enter your phone number."
            );

            return false;
        }

        if (
            !phonePattern.test(
                normalizedPhone
            )
        ) {

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

        const value =
            password.value;

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

        const value =
            confirmPassword.value;

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


    // Live Validation
    fullName.addEventListener(
        "blur",
        validateFullName
    );

    email.addEventListener(
        "blur",
        validateEmail
    );

    phone.addEventListener(
        "blur",
        validatePhone
    );

    password.addEventListener(
        "blur",
        validatePassword
    );

    confirmPassword.addEventListener(
        "blur",
        validateConfirmPassword
    );


    fullName.addEventListener(
        "input",
        () => {

            clearFieldError(
                fullName,
                fullNameError
            );

            clearMessage();
        }
    );


    email.addEventListener(
        "input",
        () => {

            clearFieldError(
                email,
                emailError
            );

            clearMessage();
        }
    );


    phone.addEventListener(
        "input",
        () => {

            clearFieldError(
                phone,
                phoneError
            );

            clearMessage();
        }
    );


    password.addEventListener(
        "input",
        () => {

            clearFieldError(
                password,
                passwordError
            );

            if (confirmPassword.value) {
                validateConfirmPassword();
            }

            clearMessage();
        }
    );


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


    terms.addEventListener(
        "change",
        () => {

            termsError.textContent = "";
            clearMessage();
        }
    );


    // Form Submit
    form.addEventListener(
        "submit",
        async event => {

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


            /*
             * Public registration always creates
             * a Customer account.
             * The backend must enforce the role.
             */
            const customerData = {
                fullName:
                    fullName.value.trim(),

                email:
                    email.value
                        .trim()
                        .toLowerCase(),

                phone:
                    phone.value.trim(),

                password:
                    password.value,

                role:
                    "Customer"
            };


            // Frontend Demo
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

                /*
                 * Do not log the password.
                 * Backend will later receive customerData
                 * through the registration API.
                 */
                console.log(
                    "Customer registration data:",
                    {
                        fullName:
                            customerData.fullName,

                        email:
                            customerData.email,

                        phone:
                            customerData.phone,

                        role:
                            customerData.role
                    }
                );

                showMessage(
                    "success",
                    "Customer account details are valid. Backend registration will be connected next."
                );

                /*
                 * Backend integration:
                 *
                 * POST /api/auth/register/customer
                 *
                 * Send customerData as JSON.
                 * The C# .NET backend should validate,
                 * securely hash the password, create
                 * the Customer account and return
                 * the registration result.
                 */

            } catch (error) {

                console.error(error);

                showMessage(
                    "error",
                    error.message ||
                    "Unable to create your account. Please try again."
                );

            } finally {

                registerButton.disabled =
                    false;

                registerButton.innerHTML =
                    originalButtonContent;
            }
        }
    );

});