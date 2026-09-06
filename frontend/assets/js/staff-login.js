document.addEventListener("DOMContentLoaded", () => {

    const form =
        document.getElementById("staffLoginForm");

    const emailInput =
        document.getElementById("staffEmail");

    const passwordInput =
        document.getElementById("staffPassword");

    const emailError =
        document.getElementById("staffEmailError");

    const passwordError =
        document.getElementById("staffPasswordError");

    const passwordToggle =
        document.getElementById("staffPasswordToggle");

    const loginButton =
        document.getElementById("staffLoginButton");

    const loginButtonText =
        document.getElementById("staffLoginButtonText");

    const loginMessage =
        document.getElementById("staffLoginMessage");


    /* =====================================================
       PASSWORD VISIBILITY
       ===================================================== */

    passwordToggle.addEventListener(
        "click",
        () => {

            const showing =
                passwordInput.type === "text";

            passwordInput.type =
                showing
                    ? "password"
                    : "text";


            const icon =
                passwordToggle.querySelector("i");

            icon.className =
                showing
                    ? "bi bi-eye"
                    : "bi bi-eye-slash";


            passwordToggle.setAttribute(
                "aria-label",
                showing
                    ? "Show password"
                    : "Hide password"
            );

        }
    );


    /* =====================================================
       VALIDATION
       ===================================================== */

    function clearErrors() {

        emailError.textContent = "";
        passwordError.textContent = "";

        loginMessage.textContent = "";
        loginMessage.className =
            "sd-login-message";
    }


    function validEmail(email) {

        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/
            .test(email);

    }


    function validate() {

        clearErrors();

        let valid = true;

        const email =
            emailInput.value.trim();

        const password =
            passwordInput.value;


        if (!email) {

            emailError.textContent =
                "Email address is required.";

            valid = false;

        } else if (!validEmail(email)) {

            emailError.textContent =
                "Enter a valid email address.";

            valid = false;
        }


        if (!password) {

            passwordError.textContent =
                "Password is required.";

            valid = false;
        }


        return valid;
    }



    /* =====================================================
       LOGIN
       ===================================================== */

    form.addEventListener(
        "submit",
        event => {

            event.preventDefault();


            if (!validate()) {

                loginMessage.textContent =
                    "Please check your login details.";

                loginMessage.className =
                    "sd-login-message error";

                return;
            }


            const loginData = {
                email: emailInput.value.trim().toLowerCase(),
                password: passwordInput.value
            };

            loginButton.disabled = true;
            loginButtonText.textContent = "Signing In...";

            try {
                const data = await ShiftApi.request("/api/auth/login", {
                    method: "POST",
                    body: JSON.stringify(loginData)
                });

                if (!data?.accessToken) {
                    throw new Error("Authentication succeeded but no access token was returned.");
                }

                if (!data?.role) {
                    throw new Error("Authentication succeeded but no user role was returned.");
                }

                ShiftApi.save(data);

                const roleRoutes = {
                    ServiceAdvisor: "advisor/dashboard.html",
                    Manager: "manager/dashboard.html",
                    Mechanic: "mechanic/dashboard.html",
                    Storekeeper: "storekeeper/dashboard.html",
                    Vendor: "vendor/dashboard.html"
                };

                const redirectPage = roleRoutes[data.role];

                if (!redirectPage) {
                    ShiftApi.clear();
                    throw new Error(`The ${data.role} role does not have a configured staff dashboard.`);
                }

                loginMessage.textContent = "Login successful. Redirecting...";
                loginMessage.className = "sd-login-message success";

                setTimeout(() => {
                    window.location.href = redirectPage;
                }, 500);

            } catch (error) {
                console.error("Staff login failed:", error);

                loginMessage.textContent =
                    error?.message || "Invalid email or password.";

                loginMessage.className =
                    "sd-login-message error";

                loginButton.disabled = false;
                loginButtonText.textContent = "Sign In";
            }
        }
    );

});


