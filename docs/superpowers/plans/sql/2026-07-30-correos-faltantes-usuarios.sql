/* =============================================================================
   Correos faltantes / mal formados en R_Usuarios.

   Mientras estas direcciones no se completen, estas personas marcan bien pero
   NO reciben la notificacion. Desde el arreglo del 2026-07-30 la pantalla se lo
   dice en el momento ("...comuniquese con el administrador del sistema") y queda
   el renglon en dbo.RegistroCorreoMarcacion con Resultado = SIN_CORREO / FALLIDO.

   NO se ejecuta nada de forma automatica: el dominio no se puede deducir.
   Hay gente de la misma Empresa con @dos.com.ec y con @compuequip.com, asi que
   Talento Humano debe confirmar direccion por direccion.
   ============================================================================= */

USE ReporTarea;
GO

/* ---------- 1. Ver el estado actual antes de tocar nada ---------- */
SELECT u.Id_Usuario, u.Cod_Usuario, u.Nom_Usuario, u.Empresa,
       ISNULL(u.E_Mail, '<NULL>') AS E_Mail_Actual
FROM dbo.R_Usuarios u
WHERE u.Id_Usuario IN (1241,1203,1198,1200,1257,1243,104,1194,1265,74,1189,
                       1242,1249,1233,1253,1216,1231,1240,   /* sin correo */
                       1251,1219,70)                          /* mal formados */
ORDER BY u.Nom_Usuario;
GO

/* ---------- 2. Correos mal formados: corregir ----------
   aarreaga@dos.com,ec  -> coma en vez de punto (typo evidente)
   haltamirano          -> le falta el dominio; confirmar cual
   NO_TIENE_CORREO      -> texto de relleno, no es una direccion
   ------------------------------------------------------- */

-- UPDATE dbo.R_Usuarios SET E_Mail = 'aarreaga@dos.com.ec' WHERE Id_Usuario = 1219;  -- Andres Arreaga Guanin
-- UPDATE dbo.R_Usuarios SET E_Mail = 'haltamirano@?????'   WHERE Id_Usuario = 1251;  -- ALTAMIRANO CALLE HENRY SAUL  <-- CONFIRMAR DOMINIO
-- UPDATE dbo.R_Usuarios SET E_Mail = NULL                  WHERE Id_Usuario = 70;    -- CORDOVA ALDAS MARIUXI (o la direccion real)

/* ---------- 3. Los 18 sin correo: completar ----------
   Reemplazar cada '???' por la direccion real y descomentar.
   ---------------------------------------------------- */

-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1241;  -- ACURIO NOROÑA LISSETTE ESTEFANIA
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1203;  -- CANTOS LEON OSCAR ANTONIO
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1198;  -- CHUCHUCA CORONEL WILSON SANTIAGO
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1200;  -- CORONEL DELGADO PEDRO XAVIER
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1257;  -- DANNY FABIAN CARRILLO CRIOLLO
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1243;  -- HIDALGO JUMBO MARIUXI GISSELLE
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 104;   -- JUMA CHIMBO MARITZA ELIZABETH
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1194;  -- MACAS MACAS LUIS ROLANDO
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1265;  -- MARTHA CAROLINA ANDRADE SALCEDO
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 74;    -- MEJIA MONTENEGRO MARIA AUGUSTA
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1189;  -- MORA MOREJON DEVORA ESTELA
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1242;  -- OSCULIO LANDI DIANA MISHEL
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1249;  -- POMBO AGUILAR FERNANDO DAVID
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1233;  -- PUPIALES CUENCA ERIKA MARIELA
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1253;  -- SANTAMARIA ROSARIO RICARDO ANDRES
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1216;  -- SUAREZ CARVAJAL MICHAEL ALEXIS
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1231;  -- TAY LASSO ENRIQUE LUIS
-- UPDATE dbo.R_Usuarios SET E_Mail = '???' WHERE Id_Usuario = 1240;  -- TORRES NOGALES ANGIE LISBETH

/* ---------- 4. Verificar que ya no queda nadie roto ---------- */
SELECT COUNT(*) AS UsuariosQueMarcanSinCorreoUtil
FROM (SELECT DISTINCT Id_Usuario
      FROM dbo.RegistroBiometrico
      WHERE FechaRegistro >= DATEADD(day, -7, CONVERT(date, GETDATE()))) r
JOIN dbo.R_Usuarios u ON u.Id_Usuario = r.Id_Usuario
WHERE u.E_Mail IS NULL
   OR LTRIM(RTRIM(u.E_Mail)) = ''
   OR u.E_Mail NOT LIKE '%_@_%._%'
   OR u.E_Mail LIKE '%,%'
   OR u.E_Mail LIKE '% %';
GO
