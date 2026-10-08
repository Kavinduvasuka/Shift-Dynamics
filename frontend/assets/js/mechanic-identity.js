(() => {
  "use strict";

  async function updateIdentity() {
    const header = document.querySelector(".sd-topbar .sd-user");
    if (!header) return;

    const name = header.querySelector("strong");
    const email = header.querySelector("div:last-child > span");
    const avatar = header.querySelector(".sd-user-avatar");

    function render(user) {
      const fullName = String(user?.fullName || "").trim();

      if (name) name.textContent = fullName || "Mechanic";
      if (email) email.textContent = user?.email || "";

      if (avatar) {
        const words = fullName.split(/\s+/).filter(Boolean);
        avatar.textContent = words.length
          ? (
              Array.from(words[0])[0] +
              (words.length > 1
                ? Array.from(words[words.length - 1])[0]
                : "")
            ).toUpperCase()
          : "M";
      }
    }

    // Clear the hard-coded demo identity immediately.
    render(null);

    if (!window.ShiftApi) return;

    const auth = ShiftApi.auth();
    if (!auth || auth.role !== "Mechanic") return;

    render(auth);

    try {
      const currentUser = await ShiftApi.request("/api/auth/me");
      render(currentUser);
    } catch (error) {
      console.warn("Unable to refresh mechanic identity:", error.message);
    }
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", updateIdentity, {
      once: true
    });
  } else {
    updateIdentity();
  }
})();