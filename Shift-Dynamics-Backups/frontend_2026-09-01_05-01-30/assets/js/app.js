document.addEventListener("DOMContentLoaded", function () {

    const menuButton =
        document.getElementById("mobileMenuButton");

    const mobileNavigation =
        document.getElementById("mobileNavigation");


    if (menuButton && mobileNavigation) {

        menuButton.addEventListener("click", function () {

            mobileNavigation.classList.toggle("open");

        });


        const mobileLinks =
            mobileNavigation.querySelectorAll("a");


        mobileLinks.forEach(function (link) {

            link.addEventListener("click", function () {

                mobileNavigation.classList.remove("open");

            });

        });

    }


    /*
     * Smooth reveal for elements when they
     * enter the screen.
     */

    const revealElements =
        document.querySelectorAll(
            ".sd-feature-card, .sd-about-card"
        );


    if ("IntersectionObserver" in window) {

        const observer =
            new IntersectionObserver(
                function (entries, observer) {

                    entries.forEach(function (entry) {

                        if (entry.isIntersecting) {

                            entry.target.style.opacity = "1";

                            entry.target.style.transform =
                                "translateY(0)";

                            observer.unobserve(entry.target);

                        }

                    });

                },
                {
                    threshold: 0.12
                }
            );


        revealElements.forEach(function (element) {

            element.style.opacity = "0";

            element.style.transform =
                "translateY(25px)";

            element.style.transition =
                "opacity 0.7s ease, transform 0.7s ease";

            observer.observe(element);

        });

    }
/* =====================================================
   LOGIN DROPDOWN
   ===================================================== */

const loginMenu =
    document.querySelector(".sd-login-menu");

const loginToggle =
    document.querySelector(".sd-login-toggle");


if (loginMenu && loginToggle) {

    loginToggle.addEventListener(
        "click",
        event => {

            event.stopPropagation();

            const isOpen =
                loginMenu.classList.toggle("open");

            loginToggle.setAttribute(
                "aria-expanded",
                String(isOpen)
            );

        }
    );


    document.addEventListener(
        "click",
        event => {

            if (
                !loginMenu.contains(event.target)
            ) {

                loginMenu.classList.remove("open");

                loginToggle.setAttribute(
                    "aria-expanded",
                    "false"
                );
            }

        }
    );


    document.addEventListener(
        "keydown",
        event => {

            if (event.key === "Escape") {

                loginMenu.classList.remove("open");

                loginToggle.setAttribute(
                    "aria-expanded",
                    "false"
                );
            }

        }
    );

}
});