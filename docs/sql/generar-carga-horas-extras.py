# -*- coding: utf-8 -*-
"""
Genera el script de carga del modulo de Horas Extras desde la plantilla Excel.

POR QUE EXISTE ESTE ARCHIVO Y NO UN .sql CON LOS DATOS DENTRO:
la plantilla trae nombres, cedulas y sueldos de 64 personas, y este
repositorio es PUBLICO. Se versiona la logica de la carga -este archivo, que
se puede revisar- y no su resultado, que son datos personales.

La salida va a docs/sql/carga-generada/, que esta en .gitignore.

Uso:
    python docs/sql/generar-carga-horas-extras.py

La plantilla se espera en Actualizacion/Plantilla_Carga_Modulo_HE.xlsx, que
tambien esta ignorada.
"""
import os
import re
import sys
import zipfile
import xml.etree.ElementTree as ET
from datetime import date, timedelta
from collections import Counter

NS = {'m': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main',
      'r': 'http://schemas.openxmlformats.org/officeDocument/2006/relationships'}

RAIZ = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PLANTILLA = os.path.join(RAIZ, 'Actualizacion', 'Plantilla_Carga_Modulo_HE.xlsx')
SALIDA_DIR = os.path.join(RAIZ, 'docs', 'sql', 'carga-generada')
SALIDA = os.path.join(SALIDA_DIR, 'carga-horas-extras.sql')

# Misma fecha que @Desde en docs/sql/2026-09-15-horas-extras-fase1.sql (los
# parametros iniciales). No se unifican -son lenguajes distintos-, pero si
# cambia una hay que revisar la otra.
VIGENCIA_ROL = '2026-09-01'

EXCEL_ORIGEN = date(1899, 12, 30)

# Los unicos dos valores que acepta HE_Salario.Origen (CHECK en el DDL de la
# fase 1). Mapea version en mayusculas -> forma canonica que se escribe en
# el script generado, para que variaciones de formato de la plantilla
# ("AJUSTE", "ajuste ") no rompan la carga por una diferencia cosmetica,
# pero cualquier otra cosa ("Ajuste salarial") si se rechace.
ORIGENES_VALIDOS = {'ROL': 'Rol', 'AJUSTE': 'Ajuste'}


def origen_normalizado(crudo):
    """Vacio se trata como Rol (comportamiento historico de la plantilla,
    donde el 100% de las filas de rol no traen esta columna). Cualquier otra
    cosa que no sea exactamente Rol o Ajuste (case-insensitive, con trim)
    devuelve None: quien llama debe rechazar la fila."""
    t = (crudo or '').strip()
    if t == '':
        return 'Rol'
    return ORIGENES_VALIDOS.get(t.upper())


def longitud_valida(texto, maximo):
    """Que quepa en la columna VARCHAR sin truncar. Un valor mas largo que
    la columna dispara el error 2628 de SQL Server, y ese error trae el
    valor truncado dentro de su propio mensaje: eso es un dato personal
    saliendo por el CATCH. Se corta aqui, antes de que llegue a la base."""
    return len(texto or '') <= maximo


def fecha_de_excel(crudo, por_omision):
    """
    Excel guarda las fechas como dias desde el 30/12/1899, no como texto. La
    columna de vigencia de la plantilla trae numeros de serie: pasarlos tal
    cual a SQL Server hace fallar el CAST y aborta la carga entera.

    Devuelve siempre 'YYYY-MM-DD'. Si la celda viene vacia -el caso de los 64
    sueldos de rol- usa la fecha por omision.

    Si el texto no es un serial entero, ANTES esta funcion devolvia el texto
    crudo: SQL Server lo interpretaba segun el DATEFORMAT de la sesion y
    podia meter un dia equivocado SIN error (01/09/2026 se puede leer como
    9 de enero). Eso es peor que fallar ruidoso, asi que ese caso se valida
    y se rechaza ANTES de llegar aqui (ver filas_invalidas en main). Si de
    todos modos llega, es que la validacion previa se salto algo: se aborta
    en vez de adivinar.
    """
    texto = (crudo or '').strip()
    if not texto:
        return por_omision
    if texto.isdigit():
        return (EXCEL_ORIGEN + timedelta(days=int(texto))).isoformat()
    raise ValueError('fecha en formato ambiguo, no es un serial de Excel ni esta vacia')


def es_entero(texto):
    """Cadena vacia o un entero (sin signo, sin decimales). JornadaHorasDia
    y DivisorManual van crudos como literal INT al script generado: un
    texto que no sea esto rompe la sintaxis del SQL de salida."""
    t = (texto or '').strip()
    return t == '' or t.isdigit()


def es_numero(texto):
    """Cadena vacia o un numero con hasta un punto decimal. Monto va crudo
    como literal DECIMAL al script generado."""
    t = (texto or '').strip()
    if t == '':
        return True
    try:
        float(t)
        return True
    except ValueError:
        return False


def leer_hojas(ruta):
    z = zipfile.ZipFile(ruta)
    compartidas = []
    if 'xl/sharedStrings.xml' in z.namelist():
        raiz = ET.fromstring(z.read('xl/sharedStrings.xml'))
        for si in raiz.findall('m:si', NS):
            compartidas.append(''.join(t.text or '' for t in si.iter('{%s}t' % NS['m'])))

    wb = ET.fromstring(z.read('xl/workbook.xml'))
    rels = ET.fromstring(z.read('xl/_rels/workbook.xml.rels'))
    destino = {x.get('Id'): x.get('Target') for x in rels}

    def valor(c):
        v = c.find('m:v', NS)
        if v is None:
            inline = c.find('m:is', NS)
            if inline is None:
                return ''
            return ''.join(t.text or '' for t in inline.iter('{%s}t' % NS['m']))
        return compartidas[int(v.text)] if c.get('t') == 's' else (v.text or '')

    hojas = {}
    for sh in wb.find('m:sheets', NS):
        nombre = sh.get('name')
        ruta_hoja = destino[sh.get('{%s}id' % NS['r'])].lstrip('/')
        if not ruta_hoja.startswith('xl/'):
            ruta_hoja = 'xl/' + ruta_hoja
        hoja = ET.fromstring(z.read(ruta_hoja))
        filas = []
        for fila in hoja.iter('{%s}row' % NS['m']):
            celdas = {}
            for c in fila.findall('m:c', NS):
                v = valor(c)
                if v != '':
                    celdas[re.match(r'[A-Z]+', c.get('r')).group()] = v
            if celdas:
                filas.append(celdas)
        hojas[nombre] = filas
    return hojas


def tabla(hojas, nombre):
    filas = hojas[nombre]
    cabecera = filas[0]
    columnas = {v: k for k, v in cabecera.items()}
    return [{c: f.get(columnas[c], '') for c in columnas} for f in filas[1:]]


def q(texto):
    """Escapa una cadena para T-SQL doblando la comilla simple."""
    return "N'" + (texto or '').replace("'", "''") + "'"


def cedula_valida(c):
    """Digito verificador ecuatoriano. Una cedula mal escrita no debe cargarse."""
    if len(c) != 10 or not c.isdigit():
        return False
    if not (1 <= int(c[:2]) <= 24 or int(c[:2]) == 30):
        return False
    if int(c[2]) >= 6:
        return False
    suma = 0
    for i, ch in enumerate(c[:9]):
        n = int(ch) * (2 if i % 2 == 0 else 1)
        suma += n - 9 if n > 9 else n
    return (10 - (suma % 10)) % 10 == int(c[9])


def main():
    if not os.path.exists(PLANTILLA):
        print('No se encontro la plantilla en %s' % PLANTILLA)
        return 1

    hojas = leer_hojas(PLANTILLA)
    colaboradores = tabla(hojas, 'Colaboradores')
    salarios = tabla(hojas, 'Salarios')

    malas = [c['Cedula'].strip() for c in colaboradores if not cedula_valida(c['Cedula'].strip())]
    if malas:
        print('Hay %d cedulas que no pasan el digito verificador. No se genera nada.' % len(malas))
        return 1

    conocidas = set(c['Cedula'].strip() for c in colaboradores)
    huerfanos = [s for s in salarios if s['Cedula'].strip() not in conocidas]
    if huerfanos:
        print('Hay %d salarios cuya cedula no esta en Colaboradores. No se genera nada.' % len(huerfanos))
        return 1

    # Fila/columna en el mensaje, nunca el valor de la celda: puede ser un
    # dato personal (sueldo, nota de RRHH) y este generador no lo imprime ni
    # en un error.
    filas_invalidas = []
    for i, c in enumerate(colaboradores, start=2):
        if not es_entero(c['JornadaHorasDia']):
            filas_invalidas.append('hoja Colaboradores, fila %d, columna JornadaHorasDia: no es un entero.' % i)
        if not es_entero(c['DivisorManual']):
            filas_invalidas.append('hoja Colaboradores, fila %d, columna DivisorManual: no es un entero.' % i)
        if not longitud_valida(c['Cedula'], 20):
            filas_invalidas.append('hoja Colaboradores, fila %d, columna Cedula: supera 20 caracteres.' % i)
        if not longitud_valida(c['Empresa'], 120):
            filas_invalidas.append('hoja Colaboradores, fila %d, columna Empresa: supera 120 caracteres.' % i)
        if not longitud_valida(c['Situacion'], 40):
            filas_invalidas.append(
                'hoja Colaboradores, fila %d, columna Situacion: supera 40 caracteres '
                '(se guarda en MotivoNoAplica).' % i)
    for i, s in enumerate(salarios, start=2):
        if not es_numero(s['Monto']):
            filas_invalidas.append('hoja Salarios, fila %d, columna Monto: no es un numero.' % i)
        texto_fecha = (s['FechaVigenciaDesde'] or '').strip()
        if texto_fecha and not texto_fecha.isdigit():
            filas_invalidas.append(
                'hoja Salarios, fila %d, columna FechaVigenciaDesde: fecha en formato '
                'ambiguo, no es un serial de Excel. No se adivina el formato: corrija '
                'la celda con el formato de fecha nativo de Excel.' % i)
        if not longitud_valida(s['Cedula'], 20):
            filas_invalidas.append('hoja Salarios, fila %d, columna Cedula: supera 20 caracteres.' % i)
        if not longitud_valida(s['Observacion'], 400):
            filas_invalidas.append('hoja Salarios, fila %d, columna Observacion: supera 400 caracteres.' % i)
        origen_valido = origen_normalizado(s['Origen'])
        if origen_valido is None:
            filas_invalidas.append(
                'hoja Salarios, fila %d, columna Origen: no es "Rol" ni "Ajuste".' % i)
        elif not longitud_valida(origen_valido, 20):
            filas_invalidas.append('hoja Salarios, fila %d, columna Origen: supera 20 caracteres.' % i)
    if filas_invalidas:
        print('Hay %d filas con datos que no se pueden interpretar con seguridad. No se genera nada.' % len(filas_invalidas))
        for msg in filas_invalidas:
            print('  - %s' % msg)
        return 1

    if not os.path.isdir(SALIDA_DIR):
        os.makedirs(SALIDA_DIR)

    L = []
    L.append('/* ' + '=' * 74)
    L.append('   CARGA DEL MODULO DE HORAS EXTRAS - GENERADO, NO EDITAR A MANO')
    L.append('')
    L.append('   Lo produce docs/sql/generar-carga-horas-extras.py desde la plantilla.')
    L.append('   CONTIENE DATOS PERSONALES: nombres, cedulas y sueldos. NO SE COMMITEA.')
    L.append('')
    L.append('   Idempotente: concilia por cedula contra Empleados y no duplica.')
    L.append('   ' + '=' * 74 + ' */')
    L.append('')
    L.append('SET NOCOUNT ON;')
    L.append('GO')
    L.append('SET QUOTED_IDENTIFIER ON;')
    L.append('SET ANSI_NULLS ON;')
    L.append('GO')
    L.append('')
    L.append('BEGIN TRY')
    L.append('BEGIN TRANSACTION;')
    L.append('')
    L.append('/* La conciliacion es por cedula contra Empleados, UNA SOLA VEZ. A partir')
    L.append('   de aqui la llave es IdEmpleado: hay una cedula que en R_Usuarios')
    L.append('   comparten seis usuarios activos, y elegir uno seria inventar. */')
    L.append('DECLARE @Faltantes INT;')
    L.append('/* Sin columna de nombre: HE_ColaboradorParametro no la tiene, el MERGE')
    L.append('   nunca la leyo, y son 64 nombres completos de peso muerto dentro del')
    L.append('   unico archivo de la fase que contiene datos personales. */')
    L.append('CREATE TABLE #C (Cedula VARCHAR(20), Empresa VARCHAR(120), Jornada INT,')
    L.append('                 DivisorManual INT NULL, AplicaHE BIT, Motivo VARCHAR(40));')
    L.append('CREATE TABLE #S (Cedula VARCHAR(20), Monto DECIMAL(18,2), Desde DATE, Origen VARCHAR(20),')
    L.append('                 Observacion VARCHAR(400) NULL);')
    L.append('')

    for c in colaboradores:
        aplica = '1' if c['AplicaHE'].strip().upper() == 'SI' else '0'
        situacion = c['Situacion'].strip()
        motivo = 'NULL' if aplica == '1' and situacion == 'Activo' else q(situacion)
        divisor = c['DivisorManual'].strip()
        divisor_sql = divisor if divisor else 'NULL'
        L.append('INSERT INTO #C VALUES (%s, %s, %s, %s, %s, %s);' % (
            q(c['Cedula'].strip()), q(c['Empresa'].strip()),
            c['JornadaHorasDia'].strip() or '0', divisor_sql, aplica, motivo))

    L.append('')
    for s in salarios:
        desde = fecha_de_excel(s['FechaVigenciaDesde'], VIGENCIA_ROL)
        origen = origen_normalizado(s['Origen'])
        L.append('INSERT INTO #S VALUES (%s, %s, %s, %s, %s);' % (
            q(s['Cedula'].strip()), s['Monto'].strip() or '0', q(desde), q(origen),
            q(s['Observacion'].strip())))

    L.append('')
    L.append('/* Si alguna cedula no esta en Empleados, no se carga nada: media carga')
    L.append('   es peor que ninguna. */')
    L.append('SELECT @Faltantes = COUNT(*) FROM #C c')
    L.append(' WHERE NOT EXISTS (SELECT 1 FROM dbo.Empleados e WHERE LTRIM(RTRIM(e.Cedula)) = c.Cedula);')
    L.append('IF @Faltantes > 0')
    L.append('BEGIN')
    L.append("    -- severidad 16 dentro de TRY: el control salta al CATCH, que revierte.")
    L.append("    RAISERROR('FALLO: %d cedulas de la plantilla no estan en Empleados. No se cargo nada.', 16, 1, @Faltantes);")
    L.append('END')
    L.append('')
    L.append('MERGE dbo.HE_ColaboradorParametro AS d')
    L.append('USING (SELECT e.IdEmpleado, c.Jornada, c.DivisorManual, c.AplicaHE, c.Motivo, c.Empresa')
    L.append('         FROM #C c JOIN dbo.Empleados e ON LTRIM(RTRIM(e.Cedula)) = c.Cedula) AS o')
    L.append('   ON d.IdEmpleado = o.IdEmpleado')
    L.append(' WHEN MATCHED THEN UPDATE SET d.JornadaHorasDia = o.Jornada, d.DivisorManual = o.DivisorManual,')
    L.append('                              d.AplicaHE = o.AplicaHE, d.MotivoNoAplica = o.Motivo,')
    L.append('                              d.Empresa = o.Empresa, d.Fec_Modificacion = SYSDATETIME(),')
    L.append("                              d.Usu_Modificacion = 'carga-plantilla'")
    L.append(' WHEN NOT MATCHED THEN INSERT (IdEmpleado, JornadaHorasDia, DivisorManual, AplicaHE, MotivoNoAplica, Empresa, Usu_Modificacion)')
    L.append("                        VALUES (o.IdEmpleado, o.Jornada, o.DivisorManual, o.AplicaHE, o.Motivo, o.Empresa, 'carga-plantilla');")
    L.append('')
    L.append('/* Los sueldos no se pisan: se insertan los que falten. Un historial no se')
    L.append('   reescribe, se le agregan filas. */')
    L.append('INSERT INTO dbo.HE_Salario (IdEmpleado, Monto, FechaVigenciaDesde, Origen, Observacion, Usu_Modificacion)')
    L.append("SELECT e.IdEmpleado, s.Monto, s.Desde, s.Origen, s.Observacion, 'carga-plantilla'")
    L.append('  FROM #S s JOIN dbo.Empleados e ON LTRIM(RTRIM(e.Cedula)) = s.Cedula')
    L.append(' WHERE NOT EXISTS (SELECT 1 FROM dbo.HE_Salario h')
    L.append('                    WHERE h.IdEmpleado = e.IdEmpleado AND h.FechaVigenciaDesde = s.Desde')
    L.append('                      AND h.Monto = s.Monto);')
    L.append('')
    L.append('DROP TABLE #C; DROP TABLE #S;')
    L.append('COMMIT TRANSACTION;')
    L.append("PRINT 'Carga de colaboradores y sueldos terminada.';")
    L.append('END TRY')
    L.append('BEGIN CATCH')
    L.append('    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;')
    L.append('    /* 2628 (truncamiento de string) y 8152 (truncamiento numerico)')
    L.append('       incluyen el valor truncado DENTRO de su propio ERROR_MESSAGE().')
    L.append('       Aqui ese valor puede ser un dato personal -una nota de RRHH, un')
    L.append('       sueldo-, y el generador valida longitudes antes de llegar hasta')
    L.append('       aqui precisamente para que esto no pase; esto es la ultima linea')
    L.append('       de defensa. Numero y linea del error se imprimen siempre; el')
    L.append('       mensaje completo, solo cuando NO es uno de esos dos. */')
    L.append("    PRINT 'Error, la carga quedo revertida. ERROR_NUMBER=' + CONVERT(VARCHAR(12), ERROR_NUMBER())")
    L.append("                                          + ' ERROR_LINE=' + CONVERT(VARCHAR(12), ERROR_LINE());")
    L.append('    IF ERROR_NUMBER() NOT IN (2628, 8152)')
    L.append("        PRINT 'Detalle: ' + ERROR_MESSAGE();")
    L.append('END CATCH')
    L.append('GO')

    # utf-8-sig: el BOM evita el mojibake de nombres con tildes y enies que
    # ya paso una vez en este proyecto (MiPerfil.aspx), pero esta vez el
    # destino es la base de datos, no una pantalla.
    with open(SALIDA, 'w', encoding='utf-8-sig') as f:
        f.write('\n'.join(L) + '\n')

    print('Generado: %s' % SALIDA)
    print('  colaboradores: %d' % len(colaboradores))
    print('  salarios:      %d' % len(salarios))
    print('NO COMMITEAR ese archivo: contiene datos personales.')

    fechas_por_cedula = Counter()
    for s in salarios:
        fechas_por_cedula[(s['Cedula'].strip(),
                           fecha_de_excel(s['FechaVigenciaDesde'], VIGENCIA_ROL))] += 1
    empatados = sum(1 for _, n in fechas_por_cedula.items() if n > 1)
    personas_con_dos = len([c for c, n in Counter(
        s['Cedula'].strip() for s in salarios).items() if n > 1])

    print('  personas con mas de un salario: %d' % personas_con_dos)
    print('  pares (cedula, fecha) repetidos: %d' % empatados)
    if personas_con_dos and not empatados:
        print('  OK: los salarios de una misma persona tienen fechas distintas.')
    else:
        print('  AVISO: hay personas con dos salarios en la MISMA fecha de vigencia.')
        print('         Cual gana lo decide NegHorasExtras.SalarioVigente, no el orden de la hoja.')

    return 0


if __name__ == '__main__':
    sys.exit(main())
