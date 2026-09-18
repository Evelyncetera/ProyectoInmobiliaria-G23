(() => {
    document.querySelectorAll("[data-selector-remoto]").forEach(contenedor => {
        const entrada = contenedor.querySelector("[data-busqueda]");
        const selector = contenedor.querySelector("[data-resultados]");
        const boton = contenedor.querySelector("[data-buscar]");
        const mensaje = contenedor.querySelector("[data-mensaje]");
        let peticion;
        const buscar = async () => {
            peticion?.abort();
            peticion = new AbortController();
            const actual = peticion;
            const url = new URL(selector.dataset.url, window.location.origin);
            url.searchParams.set("buscar", entrada.value.trim());
            mensaje.textContent = "Buscando…";
            try {
                const respuesta = await fetch(url, { signal: actual.signal, headers: { Accept: "application/json" } });
                if (!respuesta.ok || respuesta.redirected) throw new Error();
                const opciones = await respuesta.json();
                const elegida = selector.value ? selector.options[selector.selectedIndex].cloneNode(true) : null;
                const vacia = selector.options[0].cloneNode(true);
                selector.replaceChildren(vacia);
                if (elegida) selector.add(elegida);
                opciones.forEach(opcion => {
                    if (!elegida || String(opcion.id) !== elegida.value)
                        selector.add(new Option(opcion.texto, opcion.id));
                });
                selector.value = elegida?.value ?? "";
                mensaje.textContent = opciones.length ? "Seleccioná una coincidencia. Se muestran hasta 20 resultados." : "No se encontraron coincidencias.";
            } catch (error) {
                if (error.name !== "AbortError") mensaje.textContent = "No se pudo buscar. Intentá nuevamente.";
            }
        };
        boton.addEventListener("click", buscar);
        entrada.addEventListener("keydown", e => {
            if (e.key === "Enter") { e.preventDefault(); buscar(); }
        });
    });

    const archivos = document.querySelector("[data-imagenes]");
    const vista = document.querySelector("[data-previsualizaciones]");
    let urls = [];
    archivos?.addEventListener("change", () => {
        urls.forEach(url => URL.revokeObjectURL(url));
        urls = [];
        vista.replaceChildren();
        const seleccion = Array.from(archivos.files);
        const invalida = seleccion.length > 12 || seleccion.some(f => f.size === 0 || f.size > 5 * 1024 * 1024)
            || seleccion.reduce((s, f) => s + f.size, 0) > 30 * 1024 * 1024;
        archivos.setCustomValidity(invalida ? "Hasta 12 imágenes, 5 MB por imagen y 30 MB por envío." : "");
        if (invalida) { archivos.reportValidity(); return; }
        seleccion.forEach(archivo => {
            const url = URL.createObjectURL(archivo);
            urls.push(url);
            const columna = document.createElement("div");
            columna.className = "col-6 col-md-3";
            const img = document.createElement("img");
            img.src = url;
            img.alt = archivo.name;
            img.className = "img-thumbnail";
            img.style.cssText = "width:100%;height:140px;object-fit:cover";
            columna.append(img);
            vista.append(columna);
        });
    });
})();
