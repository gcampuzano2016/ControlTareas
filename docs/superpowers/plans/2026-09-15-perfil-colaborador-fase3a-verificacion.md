# Fase 3a — comprobación manual

Las pruebas automatizadas cubren lo que se puede probar sin base de datos ni
sesión: el escapado del HTML del CV, qué extensión se acepta, qué imagen se
acepta. Todo lo demás —la sesión, el disco, IIS— se comprueba a mano.

El entorno de desarrollo no permite iniciar sesión: el login va por Active
Directory. Esta lista la corre el usuario después de desplegar.

## Antes

- [ ] El script `docs/sql/2026-09-15-perfil-colaborador-fase3a.sql` está aplicado
      en producción, sin `RAISERROR` en la salida.
- [ ] Los binarios de la **fase 2** y los de esta están desplegados. La fase 2
      quedó con el SQL aplicado y los binarios sin publicar.
- [ ] La carpeta `descargas/perfil/` existe en el servidor **con su `web.config`
      dentro**. Si se publicó sin sincronizar y el archivo no viajó, cópialo a
      mano: sin él, los documentos quedan servidos por IIS a quien acierte el
      nombre.

## Foto

- [ ] Subir un JPG grande de un teléfono: la foto aparece redonda en la barra
      lateral y las iniciales desaparecen.
- [ ] Recargar la pantalla: la foto sigue ahí.
- [ ] Subir un PNG: también funciona, y se guarda convertido a JPEG.
- [ ] Elegir **el mismo archivo dos veces seguidas**: la segunda vez también
      responde. (El input se limpia en cada `change` justo por esto.)
- [ ] «Quitar»: vuelven las iniciales.

## Documentos

- [ ] Adjuntar un PDF a una certificación: aparece en su fila, con su nombre.
- [ ] Hacer clic en el nombre: el archivo se descarga y se abre.
- [ ] Adjuntar un JPG a una carga familiar: igual.
- [ ] Dos personas distintas suben un archivo llamado igual: los dos se
      descargan correctos. (El nombre en disco lleva un GUID.)
- [ ] Intentar adjuntar un `.exe` o un `.zip`: lo rechaza con un mensaje.
- [ ] Quitar un documento: desaparece de la fila.
- [ ] **Pedir el documento de otra persona.** Con un `IdDocumento` que no sea
      suyo, abrir `DescargarPerfil.ashx?doc=<ese id>`: tiene que responder
      «No se encontró ese documento», no el archivo.
- [ ] **Pedir el archivo por su ruta directa.** Copiar el nombre en disco de un
      documento —se ve en `Perfil_Documento.NombreArchivoCodigo`— y abrir
      `<sitio>/descargas/perfil/<ese nombre>`: IIS **no** debe entregarlo.

## CV

- [ ] «Descargar mi hoja de vida» con el perfil lleno: el PDF trae nombre,
      cargo, datos, formación, certificaciones y experiencia.
- [ ] Con el perfil vacío —alguien de los 119 sin ficha, sin estudios ni
      experiencia—: el PDF sale igual, con el encabezado y sin secciones vacías.
- [ ] El PDF **no** trae contactos de emergencia ni cargas familiares.
- [ ] Alguien con una experiencia sin año de fin: el período dice «Actual».
- [ ] Generarlo **dos veces seguidas**: las dos funcionan. (Es la prueba de que
      no estamos con Pechkin, que falla una de cada dos.)

## Los casos que dictaron los datos

- [ ] Uno **de los 4 con `Cod_Usuario` repetido**: no puede subir foto ni
      documentos, y el CV le dice que escriba a Talento Humano. No ve datos de
      la otra persona.
- [ ] Uno **de los 119 sin ficha de empleado**: sube foto y documentos con
      normalidad. Nada de esto depende de `Empleados`.
