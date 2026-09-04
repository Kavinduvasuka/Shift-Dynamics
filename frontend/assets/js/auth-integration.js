(() => {
  const rolePaths = { Customer: "customer/dashboard.html", Manager: "manager/dashboard.html", Mechanic: "mechanic/dashboard.html", ServiceAdvisor: "advisor/dashboard.html", Storekeeper: "storekeeper/dashboard.html", Vendor: "vendor/dashboard.html" };
  const message = (form, text, error=false) => { const el=form.querySelector(".sd-form-message, #loginMessage, #staffLoginMessage, #registerMessage"); if(el){el.textContent=text;el.className=`sd-form-message ${error?"sd-error":"sd-success"}`;} };
  document.addEventListener("submit", async event => {
    const form=event.target;
    if (!["loginForm","staffLoginForm","registerForm"].includes(form.id)) return;
    event.preventDefault(); event.stopImmediatePropagation();
    const button=form.querySelector('button[type="submit"]'); if(button)button.disabled=true;
    try {
      if(form.id === "registerForm") {
        const fullName=document.getElementById("fullName")?.value.trim(); const email=document.getElementById("email")?.value.trim(); const phone=document.getElementById("phone")?.value.trim(); const password=document.getElementById("password")?.value;
        const data=await ShiftApi.request("/api/auth/register",{method:"POST",body:JSON.stringify({fullName,email,phone,password})}); ShiftApi.save(data); location.href=rolePaths.Customer;
      } else {
        const email=(document.getElementById(form.id === "staffLoginForm" ? "staffEmail" : "email")?.value || "").trim(); const password=document.getElementById(form.id === "staffLoginForm" ? "staffPassword" : "loginPassword")?.value;
        const data=await ShiftApi.request("/api/auth/login",{method:"POST",body:JSON.stringify({email,password})});
        if(!rolePaths[data.role]) throw new Error("This account does not have portal access.");
        if(form.id === "loginForm" && data.role !== "Customer") throw new Error("Please use the staff portal for this account.");
        if(form.id === "staffLoginForm" && data.role === "Customer") throw new Error("Please use the customer login for this account.");
        ShiftApi.save(data); location.href=`${form.id === "staffLoginForm" ? "" : ""}${rolePaths[data.role]}`;
      }
    } catch(error) { message(form,error.message,true); } finally { if(button)button.disabled=false; }
  }, true);
})();