# -*- coding: utf-8 -*-
"""
Genera el script de carga de Curva de Aprendizaje desde la base SQLite del
sistema original (ESPECIFICACION-MODULO-WEBFORMS.md, seccion 20).

POR QUE EXISTE ESTE ARCHIVO Y NO UN .sql CON LOS DATOS DENTRO:
el SQLite trae evaluaciones de RRHH de personas reales y este repositorio es
PUBLICO. Se versiona la logica de la carga -este archivo- y nunca su salida,
que va a docs/sql/carga-generada/ (en .gitignore).

Uso:
    python docs/sql/generar-carga-curva-aprendizaje.py <origen>
    python docs/sql/generar-carga-curva-aprendizaje.py <origen> --indices

<origen> puede ser:
  - el .sqlite del sistema original, o
  - el export a SQL Server que genera el sistema original
    (curva_aprendizaje_sqlserver.sql). ESE ARCHIVO NO SE EJECUTA TAL CUAL en
    ReporTarea: crea tablas sin prefijo (dbo.empleados, dbo.etapas, ...) y la
    base es case-insensitive, asi que dbo.empleados ES dbo.Empleados, la
    ficha de personal de RRHH. Aqui solo se leen sus INSERT.

El .sqlite del sistema original esta en
%LOCALAPPDATA%\\DOS-CurvaAprendizaje\\data\\curva_aprendizaje.sqlite de la PC
donde corria. Usar una COPIA, nunca el archivo en uso.

Que hace la carga que genera:
  - Conserva TODOS los IDs (SET IDENTITY_INSERT), incluidos los de etapas y
    preguntas: las respuestas apuntan a ellos.
  - Copia valor_respuesta y opciones SIN transformar.
  - Pone token = NULL: los enlaces viejos apuntaban a otra URL.
  - Se niega a correr si CA_empleados ya tiene filas (no mezcla datos).
  - Si CA_preguntas tiene la siembra (2026-10-05-curva-aprendizaje-siembra.sql)
    y nadie la usa todavia, la borra y carga el catalogo del SQLite en su lugar.
  - Todo en una transaccion: o entra completo o no entra nada.

--indices imprime el indice de cada etapa y rol calculado con el algoritmo
del sistema original (seccion 7.6, redondeo de JavaScript). Sirve para la
prueba 19: despues de cargar, la ficha de cada colaborador tiene que mostrar
exactamente esos numeros.
"""
import json
import math
import os
import re
import sqlite3
import sys

RAIZ = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SALIDA_DIR = os.path.join(RAIZ, 'docs', 'sql', 'carga-generada')
SALIDA = os.path.join(SALIDA_DIR, 'carga-curva-aprendizaje.sql')

# Orden de carga respetando las referencias, y columnas destino de cada tabla
# (las del esquema 2026-10-05-curva-aprendizaje-esquema.sql). Las columnas que
# el SQLite no tenga quedan con su DEFAULT; las que el SQLite tenga de mas se
# ignoran y se avisan.
TABLAS = [
    ('etapas', ['id', 'codigo', 'nombre', 'dias_offset', 'orden']),
    ('preguntas', ['id', 'etapa_id', 'rol_evaluador', 'seccion', 'texto_pregunta', 'tipo_respuesta', 'opciones', 'orden', 'activa', 'created_at', 'updated_at']),
    ('empleados', ['id', 'nombre_completo', 'cargo', 'area', 'empresa', 'unidad_negocio', 'jefe_id', 'fecha_ingreso', 'estado', 'created_at']),
    ('procesos_onboarding', ['id', 'empleado_id', 'fecha_inicio', 'estado', 'resumen_ejecutivo_anual', 'created_at']),
    ('evaluaciones', ['id', 'proceso_id', 'etapa_id', 'evaluador_rol', 'estado', 'fecha_objetivo', 'fecha_completada', 'conclusion', 'resumen_ejecutivo', 'plan_accion_gth', 'token', 'created_at']),
    ('respuestas', ['id', 'evaluacion_id', 'pregunta_id', 'valor_respuesta', 'created_at']),
    ('decisiones_periodo_prueba', ['id', 'proceso_id', 'evaluacion_id', 'decision', 'nueva_fecha_fin', 'comentarios', 'fecha_decision']),
    ('compromisos', ['id', 'evaluacion_id', 'descripcion', 'categoria', 'responsable', 'fecha_limite', 'created_at']),
    ('compromiso_seguimientos', ['id', 'compromiso_id', 'evaluacion_id', 'estado', 'justificacion', 'fecha_revision']),
]
FECHAS = {'fecha_ingreso', 'fecha_inicio', 'fecha_objetivo', 'fecha_completada', 'nueva_fecha_fin', 'fecha_limite'}
FECHAS_HORA = {'created_at', 'updated_at', 'fecha_decision', 'fecha_revision'}


def literal(col, v):
    if col == 'token' or v is None:
        return 'NULL'
    if isinstance(v, (int, float)) and not isinstance(v, bool):
        return str(int(v)) if float(v).is_integer() else repr(v)
    s = str(v)
    if col in FECHAS:
        s = s[:10]
    elif col in FECHAS_HORA:
        s = s.replace('T', ' ')[:19]
    return "N'" + s.replace("'", "''") + "'"


def generar(con):
    cur = con.cursor()
    lineas = []
    w = lineas.append
    w('/* Carga de Curva de Aprendizaje desde el sistema original. GENERADO por')
    w('   docs/sql/generar-carga-curva-aprendizaje.py: NO editar ni versionar. */')
    w('USE [ReporTarea];')
    w('SET NOCOUNT ON; SET XACT_ABORT ON; SET QUOTED_IDENTIFIER ON;')
    w('GO')
    w("IF EXISTS (SELECT 1 FROM dbo.CA_empleados)")
    w("BEGIN RAISERROR('CA_empleados ya tiene filas: la carga no mezcla datos. Script detenido.', 16, 1); SET NOEXEC ON; END")
    w("IF EXISTS (SELECT 1 FROM dbo.CA_respuestas) OR EXISTS (SELECT 1 FROM dbo.CA_evaluaciones)")
    w("BEGIN RAISERROR('Ya hay evaluaciones o respuestas: el catalogo esta en uso. Script detenido.', 16, 1); SET NOEXEC ON; END")
    w('GO')
    w('BEGIN TRANSACTION;')
    w('-- la siembra (si la hay) se reemplaza por el catalogo original, con sus IDs')
    w('DELETE FROM dbo.CA_preguntas;')
    w('DELETE FROM dbo.CA_etapas;')
    conteos = {}
    for tabla, columnas in TABLAS:
        cur.execute('SELECT * FROM %s' % tabla)
        origen = [d[0] for d in cur.description]
        cols = [c for c in columnas if c in origen]
        sobrantes = [c for c in origen if c not in columnas]
        if sobrantes:
            print('aviso: %s trae columnas que no se cargan: %s' % (tabla, ', '.join(sobrantes)))
        filas = cur.fetchall()
        conteos[tabla] = len(filas)
        destino = 'dbo.CA_' + tabla
        w('')
        w('-- %s: %d filas' % (tabla, len(filas)))
        if not filas:
            continue
        w('SET IDENTITY_INSERT %s ON;' % destino)
        idx = [origen.index(c) for c in cols]
        if tabla == 'empleados':
            # jefe_id se referencia a si misma: primero sin jefe, despues se completa
            j = cols.index('jefe_id') if 'jefe_id' in cols else -1
            for f in filas:
                vals = [literal(c, f[i]) if c != 'jefe_id' else 'NULL' for c, i in zip(cols, idx)]
                w('INSERT INTO %s (%s) VALUES (%s);' % (destino, ', '.join(cols), ', '.join(vals)))
            w('SET IDENTITY_INSERT %s OFF;' % destino)
            if j >= 0:
                for f in filas:
                    if f[idx[j]] is not None:
                        w('UPDATE dbo.CA_empleados SET jefe_id = %s WHERE id = %s;' % (int(f[idx[j]]), int(f[idx[0]])))
            continue
        for f in filas:
            vals = [literal(c, f[i]) for c, i in zip(cols, idx)]
            w('INSERT INTO %s (%s) VALUES (%s);' % (destino, ', '.join(cols), ', '.join(vals)))
        w('SET IDENTITY_INSERT %s OFF;' % destino)
    w('')
    w('COMMIT TRANSACTION;')
    w('GO')
    w('SET NOEXEC OFF;')
    w('GO')
    w('-- verificacion: los conteos tienen que coincidir con el origen')
    w('DECLARE @Fallos INT = 0;')
    for tabla, _ in TABLAS:
        w("IF (SELECT COUNT(*) FROM dbo.CA_%s) <> %d BEGIN PRINT 'FALLO: CA_%s no tiene %d filas.'; SET @Fallos += 1; END"
          % (tabla, conteos[tabla], tabla, conteos[tabla]))
    w("IF @Fallos = 0 PRINT 'OK: carga de Curva de Aprendizaje completa.' ELSE PRINT 'VEREDICTO: FALLO.';")
    w('GO')
    os.makedirs(SALIDA_DIR, exist_ok=True)
    with open(SALIDA, 'w', encoding='utf-8-sig', newline='\r\n') as f:
        f.write('\n'.join(lineas) + '\n')
    print('Generado %s' % SALIDA)
    for t, n in conteos.items():
        print('  %-28s %6d' % (t, n))


# ----------------------------------- origen: export a SQL Server del sistema

def _tupla(s, i):
    """Lee la tupla ( ... ) que empieza en s[i] y devuelve (valores, nuevo i)."""
    i += 1
    vals = []
    while True:
        while s[i] in ' \t\r\n,':
            i += 1
        if s[i] == ')':
            return vals, i + 1
        if s[i] in 'Nn' and s[i + 1] == "'":
            i += 1
        if s[i] == "'":
            j, buf = i + 1, []
            while True:
                if s[j] == "'":
                    if s[j + 1] == "'":
                        buf.append("'")
                        j += 2
                        continue
                    break
                buf.append(s[j])
                j += 1
            vals.append(''.join(buf))
            i = j + 1
        else:
            m = re.match(r'(NULL|-?\d+(\.\d+)?)', s[i:], re.I)
            tok = m.group(1)
            vals.append(None if tok.upper() == 'NULL' else (float(tok) if '.' in tok else int(tok)))
            i += len(tok)


def abrir_export_sql(ruta):
    """
    Pasa los INSERT del export a una base SQLite en memoria, con las mismas
    tablas y columnas, para que el resto del generador no distinga el origen.
    Solo se leen los INSERT INTO dbo.<tabla> (...) VALUES (...), (...): el
    CREATE TABLE del export se ignora entero.
    """
    texto = open(ruta, encoding='utf-8-sig').read()
    con = sqlite3.connect(':memory:')
    creadas = set()
    for m in re.finditer(r'INSERT INTO dbo\.(\w+) \(([^)]*)\) VALUES', texto):
        tabla, cols = m.group(1), [c.strip() for c in m.group(2).split(',')]
        if tabla not in creadas:
            con.execute('CREATE TABLE %s (%s)' % (tabla, ', '.join(cols)))
            creadas.add(tabla)
        i = m.end()
        while True:
            while texto[i] in ' \t\r\n,':
                i += 1
            if texto[i] != '(':
                break
            vals, i = _tupla(texto, i)
            if len(vals) != len(cols):
                sys.exit('El export trae una fila de %s con %d valores para %d columnas.' % (tabla, len(vals), len(cols)))
            con.execute('INSERT INTO %s (%s) VALUES (%s)' % (tabla, ', '.join(cols), ', '.join('?' * len(vals))), vals)
    faltan = [t for t, _ in TABLAS if t not in creadas]
    for t in faltan:   # una tabla sin filas no trae INSERT: se crea vacia
        con.execute('CREATE TABLE %s (%s)' % (t, ', '.join(dict(TABLAS)[t])))
    m = re.search(r'Registros: (.*)', texto)
    if m:
        for t, n in re.findall(r'(\w+)=(\d+)', m.group(1)):
            leidas = con.execute('SELECT COUNT(*) FROM %s' % t).fetchone()[0]
            if leidas != int(n):
                sys.exit('El encabezado del export dice %s filas en %s y se leyeron %d.' % (n, t, leidas))
    return con


# ------------------------------------------------- indices (prueba 19)

def parsear(crudo):
    if crudo is None:
        return None
    try:
        return json.loads(crudo)
    except ValueError:
        return crudo


def redondear_js(x):
    return int(math.floor(x + 0.5))


def indices(con):
    cur = con.cursor()
    score = {'realizado': 1.0, 'en_progreso': 0.5, 'pendiente': 0.25, 'no_realizado': 0.0}
    cur.execute("""SELECT ev.id, ev.proceso_id, ev.evaluador_rol, et.codigo, emp.nombre_completo
                     FROM evaluaciones ev JOIN etapas et ON et.id = ev.etapa_id
                     JOIN procesos_onboarding p ON p.id = ev.proceso_id
                     JOIN empleados emp ON emp.id = p.empleado_id
                    WHERE ev.estado = 'completada' ORDER BY emp.nombre_completo, et.orden, ev.evaluador_rol""")
    for ev_id, proceso, rol, etapa, nombre in cur.fetchall():
        c2 = con.cursor()
        c2.execute("""SELECT r.valor_respuesta, p.tipo_respuesta, p.opciones FROM respuestas r
                        JOIN preguntas p ON p.id = r.pregunta_id WHERE r.evaluacion_id = ?""", (ev_id,))
        resp = c2.fetchall()
        c2.execute('SELECT estado FROM compromiso_seguimientos WHERE evaluacion_id = ?', (ev_id,))
        segs = [score[e] for (e,) in c2.fetchall() if e in score]
        pos = []
        for valor, tipo, opciones in resp:
            if tipo not in ('escala', 'opcion_unica'):
                continue
            ops = parsear(opciones)
            if not isinstance(ops, list) or len(ops) < 2:
                continue
            v = parsear(valor)
            v = v if isinstance(v, str) else valor
            if v in ops:
                pos.append(1 - ops.index(v) / (len(ops) - 1))
        if rol == 'colaborador':
            vals = pos + segs
            ind = redondear_js(sum(vals) / len(vals) * 100) if vals else None
        else:
            vals = [p * 5 for p in pos]
            comp = next((parsear(v) for v, t, _ in resp if t == 'competencias_multi'), None)
            if isinstance(comp, dict):
                vals += [float(x) for x in comp.values() if isinstance(x, (int, float))]
            vals += [s * 5 for s in segs]
            ind = redondear_js(sum(vals) / len(vals) / 5 * 100) if vals else None
        print('%-40s proceso %-4s etapa %-4s %-12s %s' % (nombre, proceso, etapa, rol, '-' if ind is None else '%d/100' % ind))


if __name__ == '__main__':
    if len(sys.argv) < 2 or not os.path.isfile(sys.argv[1]):
        print(__doc__)
        sys.exit(2)
    if sys.argv[1].lower().endswith('.sql'):
        conexion = abrir_export_sql(sys.argv[1])
    else:
        conexion = sqlite3.connect('file:%s?mode=ro' % sys.argv[1], uri=True)
    if '--indices' in sys.argv:
        indices(conexion)
    else:
        generar(conexion)
