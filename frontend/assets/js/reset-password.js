(() => {
  "use strict";
  const query = new URLSearchParams(location.search);
  const token = query.get("token");
  const form = document.getElementById("resetPasswordForm");
  const message = document.getElementById("resetMessage");
  const email = document.getElementById("resetEmail");
  email.value = query.get("email") || "";
  // Keep the token out of browser history and referrers after reading it.
  history.replaceState(null, "", location.pathname);
  if (!token) {
    message.textContent = "Open the link from your password reset email. Request a new link if it has expired.";
    form.querySelector("button").disabled = true;
    return;
  }
  form.addEventListener("submit", async event => {
    event.preventDefault();
    const button = form.querySelector("button");
    if (button.disabled || !form.reportValidity()) return;
    const password = document.getElementById("newPassword").value;
    if (password !== document.getElementById("confirmPassword").value) {
      message.textContent = "Passwords do not match."; return;
    }
    button.disabled = true;
    try {
      await ShiftApi.request("/api/auth/reset-password", {method:"POST", body:JSON.stringify({email:email.value.trim(),token,newPassword:password})});
      ShiftApi.clear();
      message.textContent = "Password updated. Use the login link below to sign in with your new password.";
      form.hidden = true;
    } catch (error) { message.textContent = error.message; button.disabled = false; }
  });
})();