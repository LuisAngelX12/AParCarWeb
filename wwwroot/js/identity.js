document.addEventListener("DOMContentLoaded", () => {

    document.querySelectorAll("a[href]").forEach(link => {

        if (
            link.href.includes("/Account/Login") ||
            link.href.includes("/Account/Register") ||
            link.href.includes("/Account/ForgotPassword")
        ) {
            link.addEventListener("click", e => {
                e.preventDefault();

                const isRegister = link.href.includes("Register");
                const isForgot = link.href.includes("ForgotPassword");

                document.body.classList.remove("identity-animate");
                document.body.classList.add(
                    isRegister || isForgot
                        ? "identity-leave-left"
                        : "identity-leave-right"
                );

                setTimeout(() => {
                    window.location = link.href;
                }, 300);
            });
        }
    });
});