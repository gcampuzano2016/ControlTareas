# Fase 3b — comprobación manual

El entorno de desarrollo no permite iniciar sesión: el login va por Active
Directory. Esta lista la corre el usuario después de desplegar.

## Antes

- [ ] El script `docs/sql/2026-09-15-perfil-colaborador-fase3b.sql` está aplicado
      en producción, sin `RAISERROR` en la salida.
- [ ] La demostración `docs/sql/2026-09-15-prueba-guarda-jefatura.sql` corrió con
      sus cuatro `OK` y terminó revirtiendo la transacción.
- [ ] Los binarios de las fases 2, 3a y 3b están desplegados.

## La pestaña aparece cuando debe

- [ ] Entrar con alguien que **no** tiene gente a cargo: la pestaña «Mi equipo»
      **no** se ve.
- [ ] Entrar con uno de los 22 jefes: la pestaña se ve y trae su equipo cargado.
- [ ] El jefe con **49 reportes**: la lista los muestra sin trabarse y el
      buscador filtra por nombre, por código y por cargo.

## Lo que la jefatura ve y lo que no

- [ ] Abrir la ficha de alguien del equipo: se ven cargo, área, ciudad, correo
      de notificación, jefe inmediato, horario, foto, contactos de emergencia,
      formación, certificaciones y experiencia.
- [ ] En esa misma ficha **no** aparecen: cédula, fecha de nacimiento, edad,
      domicilio, correo ni teléfono personales, estado civil, cargas familiares
      ni documentos de respaldo.
- [ ] No hay ningún botón que permita editar nada de esa ficha.
- [ ] No hay forma de descargar el CV de un subordinado.

## La guarda, desde el navegador

- [ ] Con las herramientas de desarrollador, repetir la petición de
      `PerfilEquipo` cambiando el `codUsuario` por el de **alguien que no es de
      su equipo**: la respuesta trae `PerfilEncontrado` en `false` y ningún dato.
- [ ] Repetirla con un `codUsuario` **que no existe**: el resultado es el mismo,
      sin que nada permita distinguir un caso del otro.

## Los casos que dictaron los datos

- [ ] Un jefe **de los 22** cuyo subordinado no tiene ficha de empleado: la ficha
      sale con lo disponible, no vacía.
- [ ] Un subordinado que todavía no registró nada de su hoja de vida: las cuatro
      tablas dicen «Sin registros.» en vez de quedarse en blanco.
