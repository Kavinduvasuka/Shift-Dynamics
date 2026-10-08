(() => {
  "use strict";

  const choices = [
    ["Alloy wheels", "alloy-wheel.jpg"],
    ["Headlamps", "headlamp.jpg"],
    ["Shock absorbers", "shock-absorber.jpg"],
    ["Car battery", "car-battery.jpg"],
    ["Brake pads", "brake-pads.jpg"],
    ["Oil filter", "oil-filter.jpg"],
    ["Engine mount", "engine-mount.jpg"],
    ["Radiator hose", "radiator-hose.jpg"]
  ];

  function enhance() {
    const section = document.getElementById("modifications");
    if (!section) return;

    section.querySelectorAll("form").forEach(form => {
      const type = form.querySelector('[name="requestType"]');
      const description = form.querySelector('[name="description"]');

      if (!type || form.dataset.pictureChoices) return;
      form.dataset.pictureChoices = "ready";

      const box = document.createElement("fieldset");
      box.className = "sd-picture-selector";

      const legend = document.createElement("legend");
      legend.textContent = "Choose what you want to modify";

      const note = document.createElement("p");
      note.textContent =
        "Choose a picture, select your vehicle and describe the changes you want. " +
        "The advisor will confirm availability, compatibility and price.";

      const grid = document.createElement("div");
      grid.className = "sd-picture-grid";
      grid.style.cssText =
        "display:grid;grid-template-columns:repeat(auto-fit,minmax(170px,1fr));gap:16px";

      const entries = [];
      let otherButton;

      function addChoice(title, source, value) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "sd-picture-option";
        button.setAttribute("aria-pressed", "false");

        if (source) {
          const image = document.createElement("img");
          image.src = source;
          image.alt = title;
          image.loading = "lazy";
          image.style.cssText =
            "width:100%;height:140px;object-fit:contain";

          image.onerror = () => {
            image.hidden = true;
          };

          button.append(image);
        }

        const label = document.createElement("strong");
        label.textContent = title;
        button.append(label);

        const entry = { button, value };
        entries.push(entry);

        button.onclick = () => {
          entries.forEach(item => {
            item.button.setAttribute(
              "aria-pressed",
              String(item === entry)
            );
          });

          type.value = value;
          type.dispatchEvent(
            new Event("change", { bubbles: true })
          );

          if (description) {
            description.placeholder = value
              ? `Describe the changes you want for ${title}.`
              : "Describe your custom modification.";
          }

          (value ? description : type)?.focus();
        };

        grid.insertBefore(button, otherButton || null);
        return button;
      }

      choices.forEach(([title, file]) => {
        addChoice(
          title,
          "../assets/images/parts/" + file,
          title
        );
      });

      otherButton = addChoice(
        "Other modification",
        null,
        ""
      );

      type.addEventListener("input", () => {
        entries.forEach(item => {
          item.button.setAttribute(
            "aria-pressed",
            String(item.value !== "" && item.value === type.value)
          );
        });
      });

      form.addEventListener("reset", () => {
        entries.forEach(item => {
          item.button.setAttribute("aria-pressed", "false");
        });
      });

      box.append(legend, note, grid);
      form.prepend(box);

      // Include images uploaded by the storekeeper too.
      if (window.ShiftApi) {
        ShiftApi.request("/api/parts").then(parts => {
          if (!form.isConnected || !Array.isArray(parts)) return;

          parts.forEach(part => {
            const validImage =
              /^\/uploads\/parts\/[a-f0-9]{32}\.(jpg|png|webp)$/i;

            if (!validImage.test(part.imageUrl || "")) return;

            const title = `${part.name} (${part.partNumber})`;

            addChoice(
              title,
              ShiftApi.base + part.imageUrl,
              title.slice(0, 100)
            );
          });
        }).catch(() => {
          // Local image choices remain available.
        });
      }
    });
  }

  function start() {
    enhance();

    const section = document.getElementById("modifications");

    if (section) {
      new MutationObserver(enhance).observe(section, {
        childList: true,
        subtree: true
      });
    }
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", start, {
      once: true
    });
  } else {
    start();
  }
})();