/* =========================================================
   SHIFT DYNAMICS
   FINAL MOUSE RESPONSIVE BACKGROUND SYSTEM
   ========================================================= */

document.addEventListener("DOMContentLoaded", () => {

    /* =====================================================
       DEVICE / ACCESSIBILITY
       ===================================================== */

    const reducedMotionQuery = window.matchMedia(
        "(prefers-reduced-motion: reduce)"
    );

    const coarsePointerQuery = window.matchMedia(
        "(hover: none), (pointer: coarse)"
    );

    if (
        reducedMotionQuery.matches ||
        coarsePointerQuery.matches
    ) {
        return;
    }


    /* =====================================================
       HOME HERO
       ===================================================== */

    const hero = document.querySelector(".sd-hero");

    if (hero) {
        initialiseHeroParallax(hero);
    }


    /* =====================================================
       EMERGENCY HERO
       ===================================================== */

    const emergencyHero =
        document.querySelector(".sd-emergency-hero");

    if (emergencyHero) {
        initialiseEmergencyParallax(emergencyHero);
    }


    /* =====================================================
       HOME HERO FUNCTION
       ===================================================== */

    function initialiseHeroParallax(element) {

        let targetX = 0;
        let targetY = 0;

        let currentX = 0;
        let currentY = 0;

        let targetPointerX = 50;
        let targetPointerY = 50;

        let currentPointerX = 50;
        let currentPointerY = 50;

        let animationFrameId = null;

        let isVisible = true;


        /* -------------------------------------------------
           POINTER MOVE
           ------------------------------------------------- */

        element.addEventListener(
            "pointermove",
            handlePointerMove,
            {
                passive: true
            }
        );


        /* -------------------------------------------------
           POINTER LEAVE
           ------------------------------------------------- */

        element.addEventListener(
            "pointerleave",
            () => {

                targetX = 0;
                targetY = 0;

                targetPointerX = 50;
                targetPointerY = 50;

            },
            {
                passive: true
            }
        );


        function handlePointerMove(event) {

            const rect =
                element.getBoundingClientRect();


            const normalizedX =
                (event.clientX - rect.left) /
                rect.width;


            const normalizedY =
                (event.clientY - rect.top) /
                rect.height;


            /*
               Convert:
               0 → 1
               into
               -1 → +1
            */

            targetX =
                (normalizedX - 0.5) * 2;

            targetY =
                (normalizedY - 0.5) * 2;


            /*
               Pointer glow location
            */

            targetPointerX =
                Math.max(
                    0,
                    Math.min(
                        100,
                        normalizedX * 100
                    )
                );


            targetPointerY =
                Math.max(
                    0,
                    Math.min(
                        100,
                        normalizedY * 100
                    )
                );
        }


        /* -------------------------------------------------
           VISIBILITY OPTIMIZATION
           ------------------------------------------------- */

        const observer =
            new IntersectionObserver(
                (entries) => {

                    entries.forEach(
                        (entry) => {

                            isVisible =
                                entry.isIntersecting;

                        }
                    );

                },
                {
                    threshold: 0
                }
            );


        observer.observe(element);


        /* -------------------------------------------------
           ANIMATION LOOP
           ------------------------------------------------- */

        function animate() {

            if (isVisible) {

                /*
                   Smooth interpolation
                */

                currentX +=
                    (targetX - currentX) *
                    0.055;


                currentY +=
                    (targetY - currentY) *
                    0.055;


                currentPointerX +=
                    (
                        targetPointerX -
                        currentPointerX
                    ) *
                    0.065;


                currentPointerY +=
                    (
                        targetPointerY -
                        currentPointerY
                    ) *
                    0.065;


                /* -----------------------------------------
                   Orange layer
                   ----------------------------------------- */

                const orangeX =
                    currentX * 42;


                const orangeY =
                    currentY * 30;


                /* -----------------------------------------
                   Navy layer - opposite direction
                   ----------------------------------------- */

                const navyX =
                    currentX * -28;


                const navyY =
                    currentY * -20;


                /* -----------------------------------------
                   Grid layer
                   ----------------------------------------- */

                const softX =
                    currentX * -10;


                const softY =
                    currentY * -7;


                /* -----------------------------------------
                   Apply CSS variables
                   ----------------------------------------- */

                element.style.setProperty(
                    "--sd-parallax-orange-x",
                    `${orangeX}px`
                );


                element.style.setProperty(
                    "--sd-parallax-orange-y",
                    `${orangeY}px`
                );


                element.style.setProperty(
                    "--sd-parallax-navy-x",
                    `${navyX}px`
                );


                element.style.setProperty(
                    "--sd-parallax-navy-y",
                    `${navyY}px`
                );


                element.style.setProperty(
                    "--sd-parallax-soft-x",
                    `${softX}px`
                );


                element.style.setProperty(
                    "--sd-parallax-soft-y",
                    `${softY}px`
                );


                element.style.setProperty(
                    "--sd-pointer-x",
                    `${currentPointerX}%`
                );


                element.style.setProperty(
                    "--sd-pointer-y",
                    `${currentPointerY}%`
                );
            }


            animationFrameId =
                requestAnimationFrame(animate);
        }


        animationFrameId =
            requestAnimationFrame(animate);


        /* -------------------------------------------------
           PAGE HIDDEN OPTIMIZATION
           ------------------------------------------------- */

        document.addEventListener(
            "visibilitychange",
            () => {

                if (
                    document.hidden &&
                    animationFrameId
                ) {
                    cancelAnimationFrame(
                        animationFrameId
                    );

                    animationFrameId = null;
                }

                else if (
                    !document.hidden &&
                    !animationFrameId
                ) {
                    animationFrameId =
                        requestAnimationFrame(
                            animate
                        );
                }

            }
        );
    }


    /* =====================================================
       EMERGENCY PAGE FUNCTION
       ===================================================== */

    function initialiseEmergencyParallax(element) {

        let targetX = 50;
        let targetY = 50;

        let currentX = 50;
        let currentY = 50;

        let active = true;


        element.addEventListener(
            "pointermove",
            (event) => {

                const rect =
                    element.getBoundingClientRect();


                targetX =
                    (
                        (
                            event.clientX -
                            rect.left
                        ) /
                        rect.width
                    ) * 100;


                targetY =
                    (
                        (
                            event.clientY -
                            rect.top
                        ) /
                        rect.height
                    ) * 100;

            },
            {
                passive: true
            }
        );


        element.addEventListener(
            "pointerleave",
            () => {

                targetX = 50;
                targetY = 50;

            }
        );


        const observer =
            new IntersectionObserver(
                (entries) => {

                    active =
                        entries[0]
                            .isIntersecting;

                }
            );


        observer.observe(element);


        function animateEmergency() {

            if (active) {

                currentX +=
                    (targetX - currentX) *
                    0.05;


                currentY +=
                    (targetY - currentY) *
                    0.05;


                element.style.setProperty(
                    "--sd-pointer-x",
                    `${currentX}%`
                );


                element.style.setProperty(
                    "--sd-pointer-y",
                    `${currentY}%`
                );
            }


            requestAnimationFrame(
                animateEmergency
            );
        }


        requestAnimationFrame(
            animateEmergency
        );
    }

});