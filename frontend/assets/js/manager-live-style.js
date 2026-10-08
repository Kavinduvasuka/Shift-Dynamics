(() => {
  "use strict";

  function formatOverview() {
    document.querySelectorAll(".all-module .all-card").forEach(card => {
      const heading = card.querySelector("h3");

      if (
        !heading ||
        heading.textContent.trim().toLowerCase() !== "workshop overview"
      ) return;

      const list = Array.from(card.children).find(
        element => element.tagName === "DL"
      );

      if (!list || list.classList.contains("all-metric-grid")) return;

      const entries = Array.from(list.children);
      if (!entries.length || entries.length % 2 !== 0) return;

      // Confirm the existing definition-list structure before changing it.
      for (let i = 0; i < entries.length; i += 2) {
        if (
          entries[i].tagName !== "DT" ||
          entries[i + 1].tagName !== "DD"
        ) return;
      }

      const fragment = document.createDocumentFragment();

      for (let i = 0; i < entries.length; i += 2) {
        const label = entries[i];
        const value = entries[i + 1];
        const metric = document.createElement("div");

        metric.className = "all-metric";

        const text = label.textContent.trim();
        label.textContent = text
          ? text.charAt(0).toUpperCase() + text.slice(1)
          : "";

        metric.append(label, value);
        fragment.append(metric);
      }

      list.classList.add("all-metric-grid");
      list.replaceChildren(fragment);
    });
  }

  function initialize() {
    const main = document.querySelector("main");
    if (!main) return;

    formatOverview();

    // Reapply after API loading, Refresh, or returning to Overview.
    const observer = new MutationObserver(formatOverview);
    observer.observe(main, { childList: true, subtree: true });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, { once: true });
  } else {
    initialize();
  }
})();