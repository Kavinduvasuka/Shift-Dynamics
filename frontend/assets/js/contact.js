document.addEventListener("DOMContentLoaded", () => {

    const form =
        document.getElementById("contactForm");

    const nameInput =
        document.getElementById("contactName");

    const emailInput =
        document.getElementById("contactEmail");

    const phoneInput =
        document.getElementById("contactPhone");

    const typeInput =
        document.getElementById("contactType");

    const subjectInput =
        document.getElementById("contactSubject");

    const messageInput =
        document.getElementById("contactMessageInput");

    const formMessage =
        document.getElementById("contactFormMessage");

    const submitButton =
        document.getElementById("contactSubmit");

    const submitText =
        document.getElementById("contactSubmitText");

    const submitIcon =
        document.getElementById("contactSubmitIcon");

    const counter =
        document.getElementById("messageCounter");

    const menuButton =
        document.getElementById("contactMenuButton");

    const mobileMenu =
        document.getElementById("contactMobileMenu");

    const year =
        document.getElementById("contactYear");


    if (year) {
        year.textContent =
            new Date().getFullYear();
    }


    /* =====================================================
       MOBILE MENU
    ===================================================== */

    if (menuButton && mobileMenu) {

        menuButton.addEventListener("click", () => {

            const open =
                mobileMenu.classList.toggle("open");

            menuButton.setAttribute(
                "aria-expanded",
                String(open)
            );

        });

    }


    /* =====================================================
       HELPERS
    ===================================================== */

    function validEmail(value) {

        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/
            .test(value);

    }


    function setError(
        input,
        errorId,
        message
    ) {

        input.classList.add(
            "sd-input-error"
        );

        const error =
            document.getElementById(errorId);

        if (error) {
            error.textContent = message;
        }

    }


    function clearError(
        input,
        errorId
    ) {

        input.classList.remove(
            "sd-input-error"
        );

        const error =
            document.getElementById(errorId);

        if (error) {
            error.textContent = "";
        }

    }


    function clearAllErrors() {

        clearError(
            nameInput,
            "contactNameError"
        );

        clearError(
            emailInput,
            "contactEmailError"
        );

        clearError(
            phoneInput,
            "contactPhoneError"
        );

        clearError(
            typeInput,
            "contactTypeError"
        );

        clearError(
            subjectInput,
            "contactSubjectError"
        );

        clearError(
            messageInput,
            "contactMessageError"
        );

    }


    function showFormMessage(
        message,
        type
    ) {

        if (!formMessage) {
            return;
        }

        formMessage.textContent = message;

        formMessage.className =
            "sd-contact-form-message " + type;

    }


    /* =====================================================
       CHARACTER COUNTER
    ===================================================== */

    if (messageInput && counter) {

        messageInput.setAttribute(
            "maxlength",
            "1000"
        );

        messageInput.addEventListener(
            "input",
            () => {

                counter.textContent =
                    `${messageInput.value.length} / 1000`;

                clearError(
                    messageInput,
                    "contactMessageError"
                );

            }
        );

    }


    /* =====================================================
       CLEAR ERROR WHILE TYPING
    ===================================================== */

    const fields = [
        [nameInput, "contactNameError"],
        [emailInput, "contactEmailError"],
        [phoneInput, "contactPhoneError"],
        [typeInput, "contactTypeError"],
        [subjectInput, "contactSubjectError"]
    ];


    fields.forEach(([field, errorId]) => {

        if (!field) {
            return;
        }

        field.addEventListener(
            "input",
            () => {
                clearError(field, errorId);
            }
        );

        field.addEventListener(
            "change",
            () => {
                clearError(field, errorId);
            }
        );

    });


    /* =====================================================
       SUBMIT
    ===================================================== */

    if (!form) {
        return;
    }


    form.addEventListener(
        "submit",
        event => {

            event.preventDefault();

            clearAllErrors();

            if (formMessage) {
                formMessage.className =
                    "sd-contact-form-message";

                formMessage.textContent = "";
            }


            const name =
                nameInput.value.trim();

            const email =
                emailInput.value.trim();

            const phone =
                phoneInput.value.trim();

            const type =
                typeInput.value;

            const subject =
                subjectInput.value.trim();

            const message =
                messageInput.value.trim();


            let valid = true;


            if (name.length < 2) {

                setError(
                    nameInput,
                    "contactNameError",
                    "Please enter your full name."
                );

                valid = false;

            }


            if (!email) {

                setError(
                    emailInput,
                    "contactEmailError",
                    "Please enter your email address."
                );

                valid = false;

            } else if (!validEmail(email)) {

                setError(
                    emailInput,
                    "contactEmailError",
                    "Please enter a valid email address."
                );

                valid = false;

            }


            if (
                phone &&
                phone.replace(/\D/g, "").length < 9
            ) {

                setError(
                    phoneInput,
                    "contactPhoneError",
                    "Please enter a valid phone number."
                );

                valid = false;

            }


            if (!type) {

                setError(
                    typeInput,
                    "contactTypeError",
                    "Please select an inquiry type."
                );

                valid = false;

            }


            if (subject.length < 3) {

                setError(
                    subjectInput,
                    "contactSubjectError",
                    "Please enter a subject."
                );

                valid = false;

            }


            if (message.length < 10) {

                setError(
                    messageInput,
                    "contactMessageError",
                    "Please enter at least 10 characters."
                );

                valid = false;

            }


            if (!valid) {

                showFormMessage(
                    "Please check the highlighted fields and try again.",
                    "error"
                );

                return;

            }


            /*
             * FRONTEND DEMO:
             *
             * Backend team will replace this timeout with
             * the Contact API / email submission request.
             */

            submitButton.disabled = true;

            submitText.textContent =
                "Sending...";

            submitIcon.className =
                "bi bi-arrow-repeat";


            window.setTimeout(() => {

                submitButton.disabled = false;

                submitText.textContent =
                    "Send Message";

                submitIcon.className =
                    "bi bi-send-check";


                showFormMessage(
                    "Your message has been received. Our team will contact you soon.",
                    "success"
                );


                form.reset();

                if (counter) {
                    counter.textContent =
                        "0 / 1000";
                }


                window.setTimeout(() => {

                    submitIcon.className =
                        "bi bi-send";

                }, 1500);

            }, 700);

        }
    );

});
