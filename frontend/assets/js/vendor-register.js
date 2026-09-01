document.addEventListener("DOMContentLoaded", () => {

    const form =
        document.getElementById("vendorRegisterForm");

    const submitButton =
        document.getElementById("vendorSubmitButton");

    const submitText =
        document.getElementById("vendorSubmitText");

    const formMessage =
        document.getElementById("vendorFormMessage");


    const fields = {
        businessName:
            document.getElementById("businessName"),

        contactPerson:
            document.getElementById("contactPerson"),

        vendorMobile:
            document.getElementById("vendorMobile"),

        vendorEmail:
            document.getElementById("vendorEmail"),

        businessAddress:
            document.getElementById("businessAddress"),

        specialization:
            document.getElementById("specialization"),

        vendorPassword:
            document.getElementById("vendorPassword"),

        confirmVendorPassword:
            document.getElementById("confirmVendorPassword"),

        vendorTerms:
            document.getElementById("vendorTerms")
    };


    function setError(fieldName, message) {

        const errorElement =
            document.querySelector(
                `[data-error-for="${fieldName}"]`
            );

        if (errorElement) {
            errorElement.textContent = message;
        }
    }


    function clearErrors() {

        document
            .querySelectorAll(".sd-error")
            .forEach(element => {
                element.textContent = "";
            });

        formMessage.textContent = "";
        formMessage.className =
            "sd-form-message";
    }


    function isValidEmail(email) {

        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/
            .test(email);
    }


    function isValidSriLankanMobile(number) {

        const cleanNumber =
            number.replace(/[\s-]/g, "");

        return /^07\d{8}$/
            .test(cleanNumber);
    }


    function validateForm() {

        clearErrors();

        let isValid = true;

        const businessName =
            fields.businessName.value.trim();

        const contactPerson =
            fields.contactPerson.value.trim();

        const mobile =
            fields.vendorMobile.value.trim();

        const email =
            fields.vendorEmail.value.trim();

        const address =
            fields.businessAddress.value.trim();

        const specialization =
            fields.specialization.value;

        const password =
            fields.vendorPassword.value;

        const confirmPassword =
            fields.confirmVendorPassword.value;


        if (!businessName) {

            setError(
                "businessName",
                "Business name is required."
            );

            isValid = false;
        }


        if (!contactPerson) {

            setError(
                "contactPerson",
                "Contact person is required."
            );

            isValid = false;
        }


        if (!mobile) {

            setError(
                "vendorMobile",
                "Mobile number is required."
            );

            isValid = false;

        } else if (
            !isValidSriLankanMobile(mobile)
        ) {

            setError(
                "vendorMobile",
                "Enter a valid Sri Lankan mobile number."
            );

            isValid = false;
        }


        if (!email) {

            setError(
                "vendorEmail",
                "Business email is required."
            );

            isValid = false;

        } else if (!isValidEmail(email)) {

            setError(
                "vendorEmail",
                "Enter a valid email address."
            );

            isValid = false;
        }


        if (!address) {

            setError(
                "businessAddress",
                "Business address is required."
            );

            isValid = false;
        }


        if (!specialization) {

            setError(
                "specialization",
                "Select a specialization."
            );

            isValid = false;
        }


        if (password.length < 8) {

            setError(
                "vendorPassword",
                "Password must contain at least 8 characters."
            );

            isValid = false;
        }


        if (
            password !== confirmPassword
        ) {

            setError(
                "confirmVendorPassword",
                "Passwords do not match."
            );

            isValid = false;
        }


        if (!fields.vendorTerms.checked) {

            setError(
                "vendorTerms",
                "Please confirm the registration information."
            );

            isValid = false;
        }


        return isValid;
    }


    document
        .querySelectorAll(
            "[data-toggle-password]"
        )
        .forEach(button => {

            button.addEventListener(
                "click",
                () => {

                    const targetId =
                        button.dataset.togglePassword;

                    const input =
                        document.getElementById(
                            targetId
                        );

                    const icon =
                        button.querySelector("i");


                    if (
                        input.type === "password"
                    ) {

                        input.type = "text";

                        icon.className =
                            "bi bi-eye-slash";

                        button.setAttribute(
                            "aria-label",
                            "Hide password"
                        );

                    } else {

                        input.type = "password";

                        icon.className =
                            "bi bi-eye";

                        button.setAttribute(
                            "aria-label",
                            "Show password"
                        );
                    }

                }
            );

        });


    form.addEventListener(
        "submit",
        event => {

            event.preventDefault();


            if (!validateForm()) {

                formMessage.textContent =
                    "Please correct the highlighted fields.";

                formMessage.className =
                    "sd-form-message error";

                return;
            }


            const vendorData = {

                businessName:
                    fields.businessName.value.trim(),

                contactPerson:
                    fields.contactPerson.value.trim(),

                mobile:
                    fields.vendorMobile.value
                        .replace(/[\s-]/g, ""),

                email:
                    fields.vendorEmail.value
                        .trim()
                        .toLowerCase(),

                businessAddress:
                    fields.businessAddress.value.trim(),

                specialization:
                    fields.specialization.value,

                role: "Vendor",

                status: "Pending Approval",

                submittedAt:
                    new Date().toISOString()

                /*
                    BACKEND INTEGRATION:

                    Later send vendorData to the
                    C# .NET registration API.

                    Password handling should be performed
                    securely by the backend.

                    Do not store plaintext passwords
                    in localStorage or frontend files.
                */
            };


            submitButton.disabled = true;

            submitText.textContent =
                "Submitting Registration...";


            setTimeout(
                () => {

                    console.log(
                        "Vendor registration request:",
                        vendorData
                    );


                    /*
                        FRONTEND DEMO WORKFLOW

                        Only business/contact information is stored.

                        Password is intentionally NOT stored in
                        localStorage.

                        Backend later replaces this with:

                        POST /api/vendor-registrations
                    */

                    const existingRequests =
                        JSON.parse(
                            localStorage.getItem(
                                "shiftDynamicsVendorRegistrations"
                            ) || "[]"
                        );


                    const duplicateIndex =
                        existingRequests.findIndex(
                            request =>
                                request.email ===
                                vendorData.email
                        );


                    const registrationRequest = {
                        id:
                            duplicateIndex >= 0
                                ? existingRequests[
                                    duplicateIndex
                                ].id
                                : `VR-${Date.now()}`,

                        ...vendorData
                    };


                    if (duplicateIndex >= 0) {

                        existingRequests[
                            duplicateIndex
                        ] = registrationRequest;

                    } else {

                        existingRequests.unshift(
                            registrationRequest
                        );
                    }


                    localStorage.setItem(
                        "shiftDynamicsVendorRegistrations",
                        JSON.stringify(
                            existingRequests
                        )
                    );


                    formMessage.textContent =
                        "Registration submitted successfully. Your vendor account is now pending manager approval.";

                    formMessage.className =
                        "sd-form-message success";

                    submitButton.disabled = false;

                    submitText.textContent =
                        "Submit Vendor Registration";

                    form.reset();

                },
                700
            );

        }
    );

});

