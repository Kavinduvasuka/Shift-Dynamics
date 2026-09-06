/* =====================================================
   SHIFT DYNAMICS
   24/7 EMERGENCY ASSISTANCE
   GARAGE / SERVICE FINDER
   ===================================================== */

document.addEventListener("DOMContentLoaded", function () {

    /* =================================================
       ELEMENTS
       ================================================= */

    const locationButton =
        document.getElementById("getLocationButton");

    const locationStatus =
        document.getElementById("locationStatus");

    const garageList =
        document.getElementById("garageList");

    const garageCount =
        document.getElementById("garageCount");

    const mapStatus =
        document.getElementById("mapStatus");

    const mapElement =
        document.getElementById("garageMap");

    const mapPlaceholder =
        document.getElementById("mapPlaceholder");

    const serviceFilterButtons =
        document.querySelectorAll(".sd-service-filter");


    /* =================================================
       MAP / FILTER VARIABLES
       ================================================= */

    let map = null;

    let userMarker = null;

    let userCircle = null;

    let garageMarkers = [];

    let activeServiceFilter = "all";

    let currentGarageResults = [];


    /* =================================================
       DEMO SERVICES
       ================================================= */

    const demoGarages = [

        {
            name: "Shift Auto Care",
            type: "Full Service Garage",
            category: "garage",
            rating: 4.8,
            phone: "+1 555-0101",
            latOffset: 0.010,
            lngOffset: 0.008,
            open: true
        },

        {
            name: "Downtown Motor Works",
            type: "Vehicle Repair",
            category: "garage",
            rating: 4.6,
            phone: "+1 555-0102",
            latOffset: -0.014,
            lngOffset: 0.012,
            open: true
        },

        {
            name: "RapidFix Garage",
            type: "Mobile Mechanic",
            category: "mobile",
            rating: 4.7,
            phone: "+1 555-0103",
            latOffset: 0.019,
            lngOffset: -0.015,
            open: true
        },

        {
            name: "24/7 Motor Assist",
            type: "Emergency Towing",
            category: "towing",
            rating: 4.9,
            phone: "+1 555-0104",
            latOffset: -0.022,
            lngOffset: -0.018,
            open: true
        },

        {
            name: "City Auto Service",
            type: "General Repairs",
            category: "garage",
            rating: 4.5,
            phone: "+1 555-0105",
            latOffset: 0.030,
            lngOffset: 0.022,
            open: false
        }

    ];


    /* =================================================
       INITIAL STATE
       ================================================= */

    if (garageCount) {

        garageCount.textContent =
            "0 services";

    }


    /* =================================================
       LOCATION BUTTON
       ================================================= */

    if (locationButton) {

        locationButton.addEventListener(
            "click",
            function () {

                if (!navigator.geolocation) {

                    updateStatus(
                        "Location services are not supported by this browser.",
                        true
                    );

                    return;

                }


                locationButton.disabled = true;


                locationButton.innerHTML = `
                    <span
                        class="spinner-border spinner-border-sm"
                        aria-hidden="true">
                    </span>

                    <span>
                        Finding your location...
                    </span>
                `;


                updateStatus(
                    "Requesting your location..."
                );


                navigator.geolocation.getCurrentPosition(

                    handleLocationSuccess,

                    handleLocationError,

                    {
                        enableHighAccuracy: true,
                        timeout: 10000,
                        maximumAge: 0
                    }

                );

            }
        );

    }


    /* =================================================
       LOCATION SUCCESS
       ================================================= */

    function handleLocationSuccess(position) {

        const latitude =
            position.coords.latitude;

        const longitude =
            position.coords.longitude;


        updateStatus(
            "Location found. Showing nearby vehicle services."
        );


        if (mapStatus) {

            mapStatus.innerHTML = `
                <i class="bi bi-circle-fill"></i>
                Location found
            `;

        }


        initializeMap(
            latitude,
            longitude
        );


        generateGarageResults(
            latitude,
            longitude
        );


        resetLocationButton();

    }


    /* =================================================
       LOCATION ERROR
       ================================================= */

    function handleLocationError(error) {

        let message =
            "Unable to access your location.";


        switch (error.code) {

            case error.PERMISSION_DENIED:

                message =
                    "Location access was denied. Please allow location access and try again.";

                break;


            case error.POSITION_UNAVAILABLE:

                message =
                    "Your location could not be determined.";

                break;


            case error.TIMEOUT:

                message =
                    "Location request timed out. Please try again.";

                break;

        }


        updateStatus(
            message,
            true
        );


        if (mapStatus) {

            mapStatus.innerHTML = `
                <i class="bi bi-exclamation-circle"></i>
                Location unavailable
            `;

        }


        resetLocationButton();

    }


    /* =================================================
       STATUS MESSAGE
       ================================================= */

    function updateStatus(
        message,
        error = false
    ) {

        if (!locationStatus) {
            return;
        }


        locationStatus.textContent =
            message;


        locationStatus.classList.toggle(
            "error",
            error
        );

    }


    /* =================================================
       RESET LOCATION BUTTON
       ================================================= */

    function resetLocationButton() {

        if (!locationButton) {
            return;
        }


        locationButton.disabled = false;


        locationButton.innerHTML = `
            <i class="bi bi-crosshair"></i>

            <span>
                Use My Location
            </span>
        `;

    }


    /* =================================================
       MAP
       ================================================= */

    function initializeMap(
        latitude,
        longitude
    ) {

        if (
            !mapElement ||
            typeof L === "undefined"
        ) {

            updateStatus(
                "Map could not be loaded.",
                true
            );

            return;

        }


        /* ---------------------------------------------
           CREATE MAP
           --------------------------------------------- */

        if (!map) {

            mapElement.innerHTML = "";


            map = L.map(
                "garageMap",
                {
                    zoomControl: true
                }
            ).setView(
                [
                    latitude,
                    longitude
                ],
                14
            );


            L.tileLayer(
                "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                {
                    maxZoom: 19,

                    attribution:
                        "&copy; OpenStreetMap contributors"
                }
            ).addTo(map);

        } else {

            map.setView(
                [
                    latitude,
                    longitude
                ],
                14
            );


            clearGarageMarkers();

        }


        /* ---------------------------------------------
           USER MARKER
           --------------------------------------------- */

        const userIcon =
            L.divIcon({

                className:
                    "sd-user-map-marker",

                html: `
                    <div class="sd-user-marker">
                        <i class="bi bi-geo-alt-fill"></i>
                    </div>
                `,

                iconSize: [
                    40,
                    40
                ],

                iconAnchor: [
                    20,
                    40
                ]

            });


        if (userMarker) {

            userMarker.setLatLng(
                [
                    latitude,
                    longitude
                ]
            );

        } else {

            userMarker =
                L.marker(
                    [
                        latitude,
                        longitude
                    ],
                    {
                        icon: userIcon
                    }
                ).addTo(map);


            userMarker.bindPopup(`
                <strong>
                    Your Location
                </strong>

                <br>

                Shift Dynamics assistance location
            `);

        }


        /* ---------------------------------------------
           USER LOCATION CIRCLE
           --------------------------------------------- */

        if (userCircle) {

            userCircle.setLatLng(
                [
                    latitude,
                    longitude
                ]
            );

        } else {

            userCircle =
                L.circle(
                    [
                        latitude,
                        longitude
                    ],
                    {
                        radius: 1000
                    }
                ).addTo(map);

        }


        /* ---------------------------------------------
           SERVICE MARKERS
           --------------------------------------------- */

        demoGarages.forEach(
            function (garage) {

                const garageLatitude =
                    latitude +
                    garage.latOffset;


                const garageLongitude =
                    longitude +
                    garage.lngOffset;


                const distance =
                    calculateDistance(
                        latitude,
                        longitude,
                        garageLatitude,
                        garageLongitude
                    );


                garage.distance =
                    distance;


                const garageIcon =
                    L.divIcon({

                        className:
                            "sd-garage-map-marker",

                        html: `
                            <div
                                class="sd-garage-marker"
                                title="${garage.name}">

                                <i class="bi bi-tools"></i>

                            </div>
                        `,

                        iconSize: [
                            38,
                            38
                        ],

                        iconAnchor: [
                            19,
                            38
                        ]

                    });


                const marker =
                    L.marker(
                        [
                            garageLatitude,
                            garageLongitude
                        ],
                        {
                            icon: garageIcon
                        }
                    ).addTo(map);


                marker.bindPopup(`

                    <div class="sd-map-popup">

                        <strong>
                            ${garage.name}
                        </strong>

                        <span>
                            ${garage.type}
                        </span>

                        <span>
                            ⭐ ${garage.rating}
                        </span>

                        <span>
                            ${formatDistance(distance)} away
                        </span>

                        <span>
                            ${
                                garage.open
                                    ? "🟢 Open now"
                                    : "🔴 Closed"
                            }
                        </span>

                    </div>

                `);


                garageMarkers.push(
                    marker
                );

            }
        );


        /* ---------------------------------------------
           FIX LEAFLET RENDERING
           --------------------------------------------- */

        setTimeout(
            function () {

                map.invalidateSize();

            },
            200
        );

    }


    /* =================================================
       CLEAR GARAGE MARKERS
       ================================================= */

    function clearGarageMarkers() {

        garageMarkers.forEach(
            function (marker) {

                if (map) {

                    map.removeLayer(
                        marker
                    );

                }

            }
        );


        garageMarkers = [];

    }


    /* =================================================
       DISTANCE CALCULATOR
       ================================================= */

    function calculateDistance(
        lat1,
        lon1,
        lat2,
        lon2
    ) {

        const earthRadius =
            6371;


        const latitudeDifference =
            toRadians(
                lat2 - lat1
            );


        const longitudeDifference =
            toRadians(
                lon2 - lon1
            );


        const a =
            Math.sin(
                latitudeDifference / 2
            ) *
            Math.sin(
                latitudeDifference / 2
            ) +

            Math.cos(
                toRadians(lat1)
            ) *

            Math.cos(
                toRadians(lat2)
            ) *

            Math.sin(
                longitudeDifference / 2
            ) *
            Math.sin(
                longitudeDifference / 2
            );


        const c =
            2 *
            Math.atan2(
                Math.sqrt(a),
                Math.sqrt(1 - a)
            );


        return earthRadius * c;

    }


    /* =================================================
       DEGREES TO RADIANS
       ================================================= */

    function toRadians(degrees) {

        return (
            degrees *
            Math.PI /
            180
        );

    }


    /* =================================================
       FORMAT DISTANCE
       ================================================= */

    function formatDistance(distance) {

        if (distance < 1) {

            return (
                Math.round(
                    distance * 1000
                ) +
                " m"
            );

        }


        return (
            distance.toFixed(1) +
            " km"
        );

    }


    /* =================================================
       GENERATE RESULTS
       ================================================= */

    function generateGarageResults(
        latitude,
        longitude
    ) {

        if (!garageList) {
            return;
        }


        currentGarageResults =
            demoGarages.map(
                function (garage) {

                    const garageLatitude =
                        latitude +
                        garage.latOffset;


                    const garageLongitude =
                        longitude +
                        garage.lngOffset;


                    const distance =
                        calculateDistance(
                            latitude,
                            longitude,
                            garageLatitude,
                            garageLongitude
                        );


                    return {

                        ...garage,

                        distance:
                            distance,

                        latitude:
                            garageLatitude,

                        longitude:
                            garageLongitude

                    };

                }
            );


        /* ---------------------------------------------
           SORT NEAREST FIRST
           --------------------------------------------- */

        currentGarageResults.sort(
            function (a, b) {

                return (
                    a.distance -
                    b.distance
                );

            }
        );


        renderFilteredGarages();

    }


    /* =================================================
       RENDER FILTERED RESULTS
       ================================================= */

    function renderFilteredGarages() {

        if (!garageList) {
            return;
        }


        const filteredGarages =
            activeServiceFilter === "all"

                ? currentGarageResults

                : currentGarageResults.filter(
                    function (garage) {

                        return (
                            garage.category ===
                            activeServiceFilter
                        );

                    }
                );


        /* ---------------------------------------------
           UPDATE COUNT
           --------------------------------------------- */

        if (garageCount) {

            garageCount.textContent =
                `${filteredGarages.length} ${
                    filteredGarages.length === 1
                        ? "service"
                        : "services"
                }`;

        }


        garageList.innerHTML = "";


        /* ---------------------------------------------
           EMPTY FILTER RESULT
           --------------------------------------------- */

        if (filteredGarages.length === 0) {

            garageList.innerHTML = `

                <div class="sd-empty-garages">

                    <i class="bi bi-search"></i>

                    <h3>
                        No services found
                    </h3>

                    <p>
                        No nearby services match this filter.
                        Try another service type.
                    </p>

                </div>

            `;

            return;

        }


        /* ---------------------------------------------
           CREATE CARDS
           --------------------------------------------- */

        filteredGarages.forEach(
            function (
                garage,
                index
            ) {

                const card =
                    createGarageCard(
                        garage,
                        index
                    );


                garageList.appendChild(
                    card
                );

            }
        );

    }


    /* =================================================
       SERVICE FILTER BUTTONS
       ================================================= */

    serviceFilterButtons.forEach(
        function (button) {

            button.addEventListener(
                "click",
                function () {

                    serviceFilterButtons.forEach(
                        function (item) {

                            item.classList.remove(
                                "active"
                            );

                        }
                    );


                    button.classList.add(
                        "active"
                    );


                    activeServiceFilter =
                        button.dataset.filter ||
                        "all";


                    renderFilteredGarages();

                }
            );

        }
    );


    /* =================================================
       CREATE GARAGE CARD
       ================================================= */

    function createGarageCard(
        garage,
        index
    ) {

        const card =
            document.createElement(
                "div"
            );


        card.className =
            "sd-garage-card";


        /* ---------------------------------------------
           NEAREST BADGE
           --------------------------------------------- */

        const nearestBadge =
            index === 0

                ? `
                    <span class="sd-garage-nearest">

                        <i class="bi bi-check-circle-fill"></i>

                        Nearest

                    </span>
                `

                : "";


        /* ---------------------------------------------
           CARD HTML
           --------------------------------------------- */

        card.innerHTML = `

            <div class="sd-garage-card-top">

                <div class="sd-garage-icon">

                    <i class="bi bi-tools"></i>

                </div>


                <div class="sd-garage-main">

                    <h3>
                        ${garage.name}
                    </h3>

                    <p>
                        ${garage.type}
                    </p>

                    ${nearestBadge}

                </div>


                <div class="sd-garage-distance">

                    <strong>
                        ${formatDistance(
                            garage.distance
                        )}
                    </strong>

                </div>

            </div>


            <div class="sd-garage-meta">

                <span class="sd-garage-rating">

                    <i class="bi bi-star-fill"></i>

                    ${garage.rating}

                </span>


                <span class="${
                    garage.open
                        ? "sd-garage-open"
                        : "sd-garage-closed"
                }">

                    <i class="bi bi-circle-fill"></i>

                    ${
                        garage.open
                            ? "Open now"
                            : "Closed"
                    }

                </span>

            </div>


            <div class="sd-garage-actions">

                <a
                    href="tel:${garage.phone}"
                    class="sd-garage-call">

                    <i class="bi bi-telephone-fill"></i>

                    Call

                </a>


                <button
                    type="button"
                    class="sd-garage-directions">

                    <i class="bi bi-sign-turn-right-fill"></i>

                    Directions

                </button>

            </div>

        `;


        /* =================================================
           DIRECTIONS BUTTON
           ================================================= */

        const directionsButton =
            card.querySelector(
                ".sd-garage-directions"
            );


        if (directionsButton) {

            directionsButton.addEventListener(
                "click",
                function () {

                    const url =
                        `https://www.google.com/maps/dir/?api=1&destination=${garage.latitude},${garage.longitude}`;


                    window.open(
                        url,
                        "_blank"
                    );

                }
            );

        }


        /* =================================================
           CLICK CARD -> OPEN MAP MARKER
           ================================================= */

        card.addEventListener(
            "click",
            function (event) {

                /*
                 * Do not open marker when user clicks
                 * Call or Directions.
                 */

                if (
                    event.target.closest(
                        ".sd-garage-call"
                    ) ||
                    event.target.closest(
                        ".sd-garage-directions"
                    )
                ) {

                    return;

                }


                const marker =
                    garageMarkers.find(
                        function (item) {

                            const position =
                                item.getLatLng();


                            return (

                                Math.abs(
                                    position.lat -
                                    garage.latitude
                                ) < 0.000001 &&

                                Math.abs(
                                    position.lng -
                                    garage.longitude
                                ) < 0.000001

                            );

                        }
                    );


                if (
                    marker &&
                    map
                ) {

                    map.setView(
                        [
                            garage.latitude,
                            garage.longitude
                        ],
                        15,
                        {
                            animate: true
                        }
                    );


                    marker.openPopup();

                }

            }
        );


        return card;

    }

});