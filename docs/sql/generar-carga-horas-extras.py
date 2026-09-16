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

VIGENCIA_ROL = '2026-09-01'

EXCEL_ORIGEN = date(1899, 12, 30)


def fecha_de_excel(crudo, por_omision):
    """
    Excel guarda las fechas como dias desde el 30/12/1899, no como texto. La
    columna de vigencia de la plantilla trae numeros de serie: pasarlos tal
    cual a SQL Server hace fallar el CAST y aborta la carga entera.

    Devuelve siempre 'YYYY-MM-DD'. Si la celda viene vacia -el caso de los 64
    sueldos de rol- usa la fecha por omision.
    """
    texto = (crudo or '').strip()
    if not texto:
        return por_omision
    if texto.isdigit():
        return (EXCEL_ORIGEN + timedelta(days=int(texto))).isoformat()
    return texto


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
    L.append('CREATE TABLE #C (Cedula VARCHAR(20), Nombre NVARCHAR(400), Empresa VARCHAR(120),')
    L.append('                 Jornada INT, AplicaHE BIT, Motivo VARCHAR(40));')
    L.append('CREATE TABLE #S (Cedula VARCHAR(20), Monto DECIMAL(18,2), Desde DATE, Origen VARCHAR(20));')
    L.append('')

    for c in colaboradores:
        aplica = '1' if c['AplicaHE'].strip().upper() == 'SI' else '0'
        situacion = c['Situacion'].strip()
        motivo = 'NULL' if aplica == '1' and situacion == 'Activo' else q(situacion)
        L.append('INSERT INTO #C VALUES (%s, %s, %s, %s, %s, %s);' % (
            q(c['Cedula'].strip()), q(c['NombreCompleto'].strip()), q(c['Empresa'].strip()),
            c['JornadaHorasDia'].strip() or '0', aplica, motivo))

    L.append('')
    for s in salarios:
        desde = fecha_de_excel(s['FechaVigenciaDesde'], VIGENCIA_ROL)
        L.append('INSERT INTO #S VALUES (%s, %s, %s, %s);' % (
            q(s['Cedula'].strip()), s['Monto'].strip() or '0', q(desde), q(s['Origen'].strip() or 'Rol')))

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
    L.append('USING (SELECT e.IdEmpleado, c.Jornada, c.AplicaHE, c.Motivo, c.Empresa')
    L.append('         FROM #C c JOIN dbo.Empleados e ON LTRIM(RTRIM(e.Cedula)) = c.Cedula) AS o')
    L.append('   ON d.IdEmpleado = o.IdEmpleado')
    L.append(' WHEN MATCHED THEN UPDATE SET d.JornadaHorasDia = o.Jornada, d.AplicaHE = o.AplicaHE,')
    L.append('                              d.MotivoNoAplica = o.Motivo, d.Empresa = o.Empresa,')
    L.append("                              d.Fec_Modificacion = SYSDATETIME(), d.Usu_Modificacion = 'carga-plantilla'")
    L.append(' WHEN NOT MATCHED THEN INSERT (IdEmpleado, JornadaHorasDia, AplicaHE, MotivoNoAplica, Empresa, Usu_Modificacion)')
    L.append("                        VALUES (o.IdEmpleado, o.Jornada, o.AplicaHE, o.Motivo, o.Empresa, 'carga-plantilla');")
    L.append('')
    L.append('/* Los sueldos no se pisan: se insertan los que falten. Un historial no se')
    L.append('   reescribe, se le agregan filas. */')
    L.append('INSERT INTO dbo.HE_Salario (IdEmpleado, Monto, FechaVigenciaDesde, Origen, Usu_Modificacion)')
    L.append("SELECT e.IdEmpleado, s.Monto, s.Desde, s.Origen, 'carga-plantilla'")
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
    L.append("    PRINT 'Error, la carga quedo revertida: ' + ERROR_MESSAGE();")
    L.append('END CATCH')
    L.append('GO')

    with open(SALIDA, 'w', encoding='utf-8') as f:
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
