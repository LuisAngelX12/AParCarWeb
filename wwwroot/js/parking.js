document.addEventListener("DOMContentLoaded", () => {

    document.querySelectorAll(".parking-spot.disponible")
        .forEach(spot => {

            spot.addEventListener("click", () => {
                const espacioId = spot.dataset.id;
                document.getElementById("espacioId").value = espacioId;

                const modal = new bootstrap.Modal(
                    document.getElementById("reservaModal")
                );
                modal.show();
            });

        });
});
