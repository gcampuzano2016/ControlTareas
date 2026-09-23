#!/usr/bin/env bash
# Corre un script de docs/sql contra la base que usa la aplicacion.
#
# Toma el servidor, el usuario y la contrasena de ReporteTareas/connections.config,
# que no viaja en git: asi la clave no queda escrita en ningun otro lado, ni en la
# linea de comandos, ni en la configuracion de permisos.
#
#   bash docs/sql/run-sql.sh docs/sql/2026-09-07-catalogo-de-horarios.sql
#
# -I     QUOTED_IDENTIFIER ON, para que se comporte igual que SSMS.
# -f 65001  los .sql de este repo son UTF-8 sin BOM y algunos llevan tildes en
#           datos (el titulo del menu). Sin esto se guardarian rotos.
set -euo pipefail

raiz="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
archivo="${1:-}"

if [ -z "$archivo" ]; then
  echo "Uso: bash docs/sql/run-sql.sh <archivo.sql>" >&2
  exit 2
fi
if [ ! -f "$archivo" ]; then
  echo "No existe el archivo: $archivo" >&2
  exit 2
fi

cfg="$raiz/ReporteTareas/connections.config"
if [ ! -f "$cfg" ]; then
  echo "Falta $cfg (ver DESPLIEGUE.md seccion 2)." >&2
  exit 2
fi

cs=$(grep -io 'connectionString="[^"]*"' "$cfg" | head -1)
servidor=$(echo "$cs" | sed -n 's/.*[Dd]ata [Ss]ource=\([^;]*\).*/\1/p')
base=$(echo "$cs" | sed -n 's/.*[Ii]nitial [Cc]atalog=\([^;]*\).*/\1/p')
usuario=$(echo "$cs" | sed -n 's/.*[Uu]ser [Ii][Dd]=\([^;]*\).*/\1/p')
clave=$(echo "$cs" | sed -n 's/.*[Pp]assword=\([^;"]*\).*/\1/p')

echo "== $archivo"
echo "== servidor $servidor / base $base / usuario $usuario"
echo

sqlcmd -S "$servidor" -d "$base" -U "$usuario" -P "$clave" -I -f 65001 -W -i "$archivo"
