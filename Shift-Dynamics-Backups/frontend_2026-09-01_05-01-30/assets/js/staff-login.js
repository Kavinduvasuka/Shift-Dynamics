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

                email:
                    emailInput.value
                        .trim()
                        .toLowerCase()

                /*
                    IMPORTANT:

                    We do NOT ask the user to select
                    Advisor / Manager / Mechanic /
                    Storekeeper / Vendor here.

                    Future C# .NET backend:

                    1. Validate email + password
                    2. Read account role
                    3. Return authenticated role
                    4. Frontend redirects to the
                       correct dashboard.

                    Never trust a frontend-selected
                    role for authorization.
                */
            };


            loginButton.disabled = true;

            loginButtonText.textContent =
                "Signing In...";


            /*
                FRONTEND DEMO ONLY.

                Real authentication will replace
                this timeout with a .NET API call.
            */

            /* =====================================================
   FRONTEND DEMO ROLE REDIRECT
   ===================================================== */

const email =
    emailInput.value.trim().toLowerCase();

let redirectPage = "";


if (email === "advisor@shiftdynamics.com") {

    redirectPage = "advisor/dashboard.html";

} else if (email === "manager@shiftdynamics.com") {

    redirectPage = "manager/dashboard.html";

} else if (email === "mechanic@shiftdynamics.com") {

    redirectPage = "mechanic/dashboard.html";

} else if (email === "storekeeper@shiftdynamics.com") {

    redirectPage = "storekeeper/dashboard.html";

} else if (email === "vendor@example.com") {

    redirectPage = "vendor/dashboard.html";

}


if (redirectPage) {

    loginMessage.textContent =
        "Login successful. Redirecting...";

    loginMessage.className =
        "sd-login-message success";


    setTimeout(() => {

        window.location.href = redirectPage;

    }, 800);

} else {

    loginMessage.textContent =
        "Account not found for this portal.";

    loginMessage.className =
        "sd-login-message error";


    loginButton.disabled = false;

    loginButtonText.textContent =
        "Sign In";
}
        }
    );

});
