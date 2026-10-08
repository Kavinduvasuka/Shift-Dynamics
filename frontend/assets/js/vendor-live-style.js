(() => {
  "use strict";

  function formatProfile() {
    document.querySelectorAll(".all-module .all-card").forEach(card => {
      const title = card.querySelector("h3");

      if (
        !title ||
        title.textContent.trim().toLowerCase() !== "business profile"
      ) return;

      const list = Array.from(card.children).find(
        element => element.tagName === "DL"
      );

      if (!list || list.classList.contains("vendor-profile-grid")) return;

      const items = Array.from(list.children);
      if (!items.length || items.length % 2 !== 0) return;

      for (let i = 0; i < items.length; i += 2) {
        if (
          items[i].tagName !== "DT" ||
          items[i + 1].tagName !== "DD"
        ) return;
      }

      const fragment = document.createDocumentFragment();

      for (let i = 0; i < items.length; i += 2) {
        const field = document.createElement("div");
        field.className = "vendor-profile-field";

        items[i].textContent = items[i].textContent
          .trim()
          .replace(/\b[a-z]/g, letter => letter.toUpperCase());

        field.append(items[i], items[i + 1]);
        fragment.append(field);
      }

      list.classList.add("vendor-profile-grid");
      list.replaceChildren(fragment);
    });
  }

  function initialize() {
    const main = document.querySelector("main");
    if (!main) return;

    formatProfile();

    const observer = new MutationObserver(formatProfile);
    observer.observe(main, { childList: true, subtree: true });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, { once: true });
  } else {
    initialize();
  }
})();