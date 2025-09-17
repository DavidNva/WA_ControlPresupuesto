function inicializarFormularioTransacciones(urlObtenerCategorias) {
    $("#TipoOperacionId").change(async function () {
        const valorSeleccionadoTipoOperacionId = $(this).val();

        const respuesta = await fetch(urlObtenerCategorias, {
            method: 'POST',
            body: valorSeleccionadoTipoOperacionId,
            headers: {
                'Content-Type': 'application/json'//
            }
        });

        const json = await respuesta.json();
        const opciones = json.map(categoria => `<option value=${categoria.value}>${categoria.text}</option>`);
        $("#CategoriaId").html(opciones);
    })

}


