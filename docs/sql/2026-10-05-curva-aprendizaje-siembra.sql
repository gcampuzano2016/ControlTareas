/* ============================================================================
   Curva de Aprendizaje: siembra del catalogo (5 etapas, 90 preguntas)
   ReporTarea  |  2026-10-05

   Copia literal de ESPECIFICACION-MODULO-WEBFORMS.md, seccion 5. Es contenido
   de negocio: NO reordenar las opciones, la primera de cada escala es la mas
   favorable para el indice (seccion 7.6).

   NO CORRER SI SE MIGRA DESDE EL SQLITE: la carga trae el catalogo con sus
   IDs originales (las respuestas migradas apuntan a ellos). Este script solo
   inserta si CA_etapas / CA_preguntas estan vacias (re-correrlo no duplica
   nada), y la carga se niega a correr si ya hay preguntas sembradas.

   Este archivo va en UTF-8 CON BOM, a diferencia del resto de docs/sql: el
   catalogo lleva tildes y enies, y con el BOM tanto SSMS como sqlcmd lo leen
   bien (run-sql.sh ademas pasa -f 65001).

   Requiere docs/sql/2026-10-05-curva-aprendizaje-esquema.sql.
   ============================================================================ */

USE [ReporTarea];
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('dbo.CA_preguntas','U') IS NULL
BEGIN
    RAISERROR('No existe dbo.CA_preguntas: correr primero 2026-10-05-curva-aprendizaje-esquema.sql. Script detenido.', 16, 1);
    SET NOEXEC ON;
END
GO

SET NOCOUNT ON;
IF NOT EXISTS (SELECT 1 FROM dbo.CA_etapas)
  INSERT INTO dbo.CA_etapas (codigo, nombre, dias_offset, orden) VALUES
  ('30', N'1er Mes', 30, 1),
  ('60', N'2do Mes', 60, 2),
  ('90', N'3er Mes - Periodo de Prueba', 90, 3),
  ('180', N'6to Mes', 180, 4),
  ('365', N'1 Año', 365, 5);

IF NOT EXISTS (SELECT 1 FROM dbo.CA_preguntas)
BEGIN
DECLARE @E30 INT = (SELECT id FROM dbo.CA_etapas WHERE codigo='30');
DECLARE @E60 INT = (SELECT id FROM dbo.CA_etapas WHERE codigo='60');
DECLARE @E90 INT = (SELECT id FROM dbo.CA_etapas WHERE codigo='90');
DECLARE @E180 INT = (SELECT id FROM dbo.CA_etapas WHERE codigo='180');
DECLARE @E365 INT = (SELECT id FROM dbo.CA_etapas WHERE codigo='365');
DECLARE @COMP NVARCHAR(MAX) = N'["Calidad de trabajo","Proactividad","Trabajo en equipo","Comunicación","Alineación cultural"]';

INSERT INTO dbo.CA_preguntas (etapa_id, rol_evaluador, seccion, texto_pregunta, tipo_respuesta, opciones, orden) VALUES
-- ===================== 30 días · colaborador =====================
(@E30,'colaborador',N'Integración y Adaptación Esencial',N'¿Qué tan fácil o difícil te ha resultado adaptarte al nuevo entorno de trabajo?','escala',N'["Muy fácil","Fácil","Neutral","Difícil","Muy difícil"]',1),
(@E30,'colaborador',N'Integración y Adaptación Esencial',N'¿Qué desafíos has encontrado durante este primer mes en términos de adaptación?','opcion_multiple',N'["Comprender la cultura de la empresa","Aprender los procesos internos","Utilizar las herramientas/sistemas","Gestionar la carga de trabajo","Establecer relaciones con compañeros","Ninguno","Otros"]',2),
(@E30,'colaborador',N'Integración y Adaptación Esencial',N'¿Cómo calificarías el proceso de inducción o bienvenida que recibiste?','escala',N'["Excelente","Bueno","Regular","Malo","Muy malo"]',3),
(@E30,'colaborador',N'Integración y Adaptación Esencial',N'¿Qué aspectos del proceso de inducción mejorarías?','texto_abierto',NULL,4),
(@E30,'colaborador',N'Comprensión del Rol y Responsabilidades Clave',N'¿Sientes que tienes claridad sobre tus funciones y responsabilidades?','escala',N'["Sí, totalmente","Sí, en general","Parcialmente","No mucho","No, nada"]',5),
(@E30,'colaborador',N'Comprensión del Rol y Responsabilidades Clave',N'¿Qué necesitas para tener mayor claridad sobre tu rol?','texto_abierto',NULL,6),
(@E30,'colaborador',N'Comprensión del Rol y Responsabilidades Clave',N'¿Qué tipo de apoyo has necesitado para comprender mejor tu rol?','opcion_multiple',N'["Explicaciones del líder del área","Capacitación adicional","Documentación/manuales","Observación de compañeros","Ninguno","Otro"]',7),
(@E30,'colaborador',N'Aprendizaje de Habilidades y Procesos Fundamentales',N'¿Qué nuevas habilidades o conocimientos has tenido que adquirir en este primer mes?','texto_abierto',NULL,8),
(@E30,'colaborador',N'Aprendizaje de Habilidades y Procesos Fundamentales',N'¿Qué procesos o procedimientos te han resultado más fáciles o difíciles de aprender?','texto_abierto',NULL,9),
(@E30,'colaborador',N'Relaciones y Comunicación Básica',N'¿Cómo ha sido tu interacción con tus compañeros de equipo o áreas relacionadas?','escala',N'["Muy positiva","Positiva","Neutral","Negativa","Muy negativa"]',10),
(@E30,'colaborador',N'Relaciones y Comunicación Básica',N'¿Qué ha facilitado o dificultado esa interacción?','texto_abierto',NULL,11),
(@E30,'colaborador',N'Relaciones y Comunicación Básica',N'¿Cómo calificarías la comunicación que has recibido de tu supervisor/a?','escala',N'["Excelente","Bueno","Regular","Malo","Muy malo"]',12),
(@E30,'colaborador',N'Relaciones y Comunicación Básica',N'¿Qué tipo de comunicación te ha sido más útil?','texto_abierto',NULL,13),
(@E30,'colaborador',N'Motivación y Desafíos Principales',N'¿Qué te ha motivado más de tu nuevo trabajo hasta ahora?','texto_abierto',NULL,14),
(@E30,'colaborador',N'Motivación y Desafíos Principales',N'¿Qué sugerencias tienes para mejorar la experiencia de los nuevos colaboradores en su primer mes?','texto_abierto',NULL,15),
(@E30,'colaborador',N'Retroalimentación Adicional',N'¿Hay algo más que te gustaría compartir sobre tu experiencia de aprendizaje durante este primer mes?','texto_abierto',NULL,16),
(@E30,'colaborador',N'Cierre',N'Compromisos y acciones específicas a implementar de cara a los próximos meses','compromisos',NULL,17),
-- ===================== 60 días · colaborador =====================
(@E60,'colaborador',N'Consolidación de Aprendizajes del Primer Mes',N'De las habilidades o procesos que identificaste como difíciles en tu primer mes, ¿cuáles sientes que ya dominas? ¿Cuáles siguen pendientes?','texto_abierto',NULL,1),
(@E60,'colaborador',N'Consolidación de Aprendizajes del Primer Mes',N'¿Con qué frecuencia todavía necesitas preguntar cosas que esperabas ya saber a esta altura?','escala',N'["Nunca","Rara vez","Frecuentemente","Constantemente"]',2),
(@E60,'colaborador',N'Expectativa vs. Realidad del Rol',N'Comparando lo que te comunicaron sobre el puesto durante el reclutamiento con tu experiencia real hasta ahora, ¿qué tan alineado está?','escala',N'["Totalmente alineado","Mayormente alineado","Hay diferencias notables","Es muy distinto a lo esperado"]',3),
(@E60,'colaborador',N'Expectativa vs. Realidad del Rol',N'¿En qué tipo de aspectos notas diferencias entre lo esperado y la realidad? (marca los que apliquen)','opcion_multiple',N'["Responsabilidades del puesto","Nivel de autonomía","Carga de trabajo","Compensación o beneficios","Cultura o ambiente de equipo","Modalidad de trabajo (presencial/remoto/híbrido)","Ninguna diferencia relevante","Otro"]',4),
(@E60,'colaborador',N'Expectativa vs. Realidad del Rol',N'Cuéntanos más sobre esa diferencia (opcional)','texto_abierto',NULL,5),
(@E60,'colaborador',N'Integración al Equipo (segunda medición)',N'¿Sientes que tu relación con el equipo ha mejorado, se mantiene igual o ha empeorado desde tu primer mes?','escala',N'["Ha mejorado claramente","Se mantiene igual","Ha empeorado"]',6),
(@E60,'colaborador',N'Integración al Equipo (segunda medición)',N'¿Hay alguna persona o rol con quien todavía no tienes claridad de cómo o cuándo interactuar?','texto_abierto',NULL,7),
(@E60,'colaborador',N'Carga y Sostenibilidad',N'¿Cómo describirías tu carga de trabajo actual?','escala',N'["Muy ligera","Adecuada","Alta pero manejable","Insostenible"]',8),
(@E60,'colaborador',N'Carga y Sostenibilidad',N'¿Sientes que el ritmo al que estás aprendiendo es sostenible para ti?','escala',N'["Sí, totalmente","Sí, en general","No mucho","No, en absoluto"]',9),
(@E60,'colaborador',N'Señal Temprana de Período de Prueba',N'Si hoy tuvieras que calificar tu propia adaptación al puesto, ¿qué tan preparado/a te sientes para la evaluación de los 90 días?','escala',N'["1 - Muy poco preparado/a","2","3","4","5 - Totalmente preparado/a"]',10),
(@E60,'colaborador',N'Señal Temprana de Período de Prueba',N'¿Qué necesitarías en las próximas 4 semanas para llegar mejor preparado/a a esa evaluación?','texto_abierto',NULL,11),
(@E60,'colaborador',N'Cierre',N'Compromisos y acciones específicas a implementar de cara a los próximos meses','compromisos',NULL,12),
-- ===================== 90 días · colaborador =====================
(@E90,'colaborador',N'Comprensión del Rol y Responsabilidades',N'¿Cómo describirías tus principales responsabilidades y tareas diarias?','opcion_unica',N'["Puedo enumerar la mayoría de mis responsabilidades principales con claridad","Entiendo mis responsabilidades generales, pero aún estoy aprendiendo los detalles de algunas tareas.","Tengo una idea general de lo que debo hacer, pero todavía necesito mucha guía.","Aún no estoy seguro/a de todas mis responsabilidades."]',1),
(@E90,'colaborador',N'Comprensión del Rol y Responsabilidades',N'Claridad de objetivos y alineación con el equipo/departamento','opcion_unica',N'["Los objetivos de mi puesto son muy claros y entiendo cómo contribuyo al equipo/departamento.","Entiendo mis objetivos principales, pero la conexión con los objetivos más amplios podría ser más clara.","Tengo una comprensión básica de mis objetivos, pero no estoy seguro/a de cómo encajo en el panorama general.","No tengo claridad sobre mis objetivos o cómo contribuyen al equipo/departamento."]',2),
(@E90,'colaborador',N'Comprensión del Rol y Responsabilidades',N'Nivel de autonomía en tareas esenciales','opcion_unica',N'["Me siento muy cómodo/a realizando la mayoría de las tareas esenciales de forma independiente.","Puedo realizar algunas tareas esenciales de forma independiente, pero todavía necesito supervisión en otras.","Necesito supervisión y guía en la mayoría de las tareas esenciales.","No me siento cómodo/a realizando la mayoría de las tareas esenciales sin ayuda."]',3),
(@E90,'colaborador',N'Adquisición de Habilidades y Conocimientos Técnicos',N'Ritmo de adquisición de habilidades técnicas','opcion_unica',N'["Siento que he adquirido las habilidades técnicas rápidamente y me adapto bien.","He aprendido algunas habilidades técnicas, pero el proceso ha sido gradual.","Siento que el aprendizaje de las habilidades técnicas está tomando más tiempo de lo esperado.","Todavía me siento muy por detrás en la adquisición de las habilidades técnicas necesarias."]',4),
(@E90,'colaborador',N'Adquisición de Habilidades y Conocimientos Técnicos',N'Utilidad de recursos y capacitación recibidos','opcion_multiple',N'["Las capacitaciones formales fueron muy útiles.","El apoyo y la guía de mi supervisor/compañeros de equipo fueron cruciales.","Los materiales de documentación y autoaprendizaje fueron suficientes.","Ninguno de los recursos proporcionados ha sido particularmente útil.","Otros (especificar)"]',5),
(@E90,'colaborador',N'Integración al Equipo y Cultura',N'Nivel de integración al equipo y cultura','opcion_unica',N'["Me siento completamente integrado/a y parte del equipo.","Me siento bastante bien integrado/a, aunque todavía estoy conociendo a todos.","Todavía me siento un poco ajeno/a al equipo y a la cultura.","No me siento integrado/a al equipo ni a la cultura de la empresa."]',6),
(@E90,'colaborador',N'Integración al Equipo y Cultura',N'Comodidad para buscar ayuda','opcion_unica',N'["Me siento muy cómodo/a buscando ayuda cuando la necesito.","Generalmente me siento cómodo/a, pero dudo en preguntar demasiado.","Me siento un poco incómodo/a buscando ayuda."]',7),
(@E90,'colaborador',N'Integración al Equipo y Cultura',N'Comunicación y retroalimentación del supervisor','opcion_unica',N'["La comunicación es clara y la retroalimentación es regular y útil.","La comunicación es generalmente buena, pero la retroalimentación podría ser más frecuente o específica.","La comunicación es inconsistente y la retroalimentación es escasa.","La comunicación es pobre y no he recibido retroalimentación significativa."]',8),
(@E90,'colaborador',N'Desafíos y Apoyo Requerido',N'Principales desafíos enfrentados','opcion_multiple',N'["Ningún desafío significativo.","Aprender los procesos internos.","Adquirir las habilidades técnicas necesarias.","Integrarme al equipo.","Gestionar la carga de trabajo.","Otros (especificar)"]',9),
(@E90,'colaborador',N'Desafíos y Apoyo Requerido',N'Apoyo adicional para mejorar el aprendizaje','opcion_multiple',N'["Más capacitación formal.","Mayor mentoría o guía individualizada.","Documentación más detallada o recursos de autoaprendizaje.","Mayor claridad en las expectativas y objetivos.","Más oportunidades para colaborar con otros equipos.","Ningún apoyo adicional es necesario.","Otros (especificar)"]',10),
(@E90,'colaborador',N'Desafíos y Apoyo Requerido',N'Nivel general de confianza en tu desempeño','escala',N'["1 (Muy baja confianza)","2","3","4","5 (Muy alta confianza)"]',11),
(@E90,'colaborador',N'Retroalimentación Adicional',N'¿Hay algo más que te gustaría compartir sobre tu experiencia de aprendizaje durante estos primeros 3 meses?','texto_abierto',NULL,12),
(@E90,'colaborador',N'Cierre',N'Compromisos y acciones específicas a implementar de cara a los próximos meses','compromisos',NULL,13),
-- ===================== 180 días · colaborador =====================
(@E180,'colaborador',N'Dominio y Autonomía en el Rol',N'¿Con qué frecuencia necesitas supervisión directa para completar tus tareas principales?','opcion_unica',N'["Rara vez o nunca, puedo manejar la mayoría de las tareas de forma independiente.","Ocasionalmente, para tareas nuevas o complejas.","Frecuentemente, necesito orientación para la mayoría de mis tareas.","Constantemente, necesito apoyo para casi todo."]',1),
(@E180,'colaborador',N'Dominio y Autonomía en el Rol',N'¿Cómo calificarías tu conocimiento de los procesos y procedimientos clave de tu área?','opcion_unica',N'["Excelente, conozco la mayoría de los procesos a fondo.","Bueno, conozco los procesos principales y puedo consultarlos si tengo dudas.","Básico, aún me cuesta recordar y aplicar algunos procesos importantes.","Limitado, aún dependo mucho de la guía para los procesos."]',2),
(@E180,'colaborador',N'Adaptación e Integración al Equipo',N'¿Qué tan fácil te ha resultado adaptarte a la cultura y dinámicas de comunicación del equipo?','opcion_unica',N'["Muy fácil, me siento completamente integrado.","Fácil, me he adaptado bien aunque a veces tengo pequeñas dudas.","Moderado, aún estoy en proceso de comprender algunas dinámicas.","Difícil, me cuesta un poco encajar o entender las formas de comunicación."]',3),
(@E180,'colaborador',N'Adaptación e Integración al Equipo',N'¿Con qué nivel de confianza te sientes al pedir ayuda o feedback a tus compañeros o líder cuando lo necesitas?','opcion_unica',N'["Muy alto, me siento cómodo pidiendo ayuda en cualquier situación.","Alto, no tengo problema en pedir ayuda para la mayoría de las cosas.","Moderado, a veces me siento un poco inseguro al pedir ayuda.","Bajo, me cuesta mucho pedir ayuda o feedback."]',4),
(@E180,'colaborador',N'Iniciativa y Proactividad',N'¿Con qué frecuencia identificas y propones mejoras en los procesos o en la forma de trabajar en tu área?','opcion_unica',N'["Muy a menudo, siempre busco formas de optimizar.","Ocasionalmente, cuando veo una oportunidad clara.","Raramente, me enfoco principalmente en completar mis tareas asignadas.","Nunca, aún no me siento capaz de proponer mejoras."]',5),
(@E180,'colaborador',N'Iniciativa y Proactividad',N'Cuando te enfrentas a un problema inesperado en tu trabajo, ¿cuál es tu primera reacción?','opcion_unica',N'["Intento resolverlo por mi cuenta utilizando mis conocimientos y recursos disponibles.","Busco soluciones en línea o en documentos, y luego consulto si es necesario.","Pregunto directamente a un compañero o a mi líder para que me guíen.","Me bloqueo y espero a que alguien me diga qué hacer."]',6),
(@E180,'colaborador',N'Desarrollo y Expectativas Futuras',N'¿Cómo calificarías tu progreso general en el rol durante estos 6 meses?','opcion_unica',N'["He superado mis expectativas y he aprendido mucho.","He cumplido mis expectativas y he tenido un buen progreso.","Mi progreso ha sido más lento de lo que esperaba.","Siento que mi progreso ha sido limitado."]',7),
(@E180,'colaborador',N'Desarrollo y Expectativas Futuras',N'En los próximos 3-6 meses, ¿en qué aspecto te gustaría enfocar tu desarrollo para seguir creciendo en tu rol?','opcion_unica',N'["Profundizar en mis habilidades técnicas y conocimientos específicos del área.","Desarrollar habilidades blandas como la comunicación, el liderazgo o la resolución de conflictos.","Asumir más responsabilidades o proyectos de mayor complejidad.","Aprender sobre otras áreas o funciones dentro de la empresa."]',8),
(@E180,'colaborador',N'Retroalimentación Adicional',N'Comentarios adicionales','texto_abierto',NULL,9),
(@E180,'colaborador',N'Cierre',N'Compromisos y acciones específicas a implementar de cara al cierre del primer año','compromisos',NULL,10),
-- ===================== 365 días · colaborador =====================
(@E365,'colaborador',N'Balance del Primer Año',N'Mirando atrás, ¿cuál fue el momento o hito en el que sentiste que "ya dominabas" tu rol?','texto_abierto',NULL,1),
(@E365,'colaborador',N'Balance del Primer Año',N'¿Qué fue lo que más te costó aprender y cómo lo resolviste finalmente?','texto_abierto',NULL,2),
(@E365,'colaborador',N'Contribución y Valor',N'¿Qué logro o aporte de este año te genera más orgullo?','texto_abierto',NULL,3),
(@E365,'colaborador',N'Contribución y Valor',N'¿Sientes que tu rol actual aprovecha tus habilidades?','escala',N'["Totalmente","Parcialmente","Poco","Nada"]',4),
(@E365,'colaborador',N'Contribución y Valor',N'¿Qué capacidades tuyas sientes que la empresa no está aprovechando? (opcional)','texto_abierto',NULL,5),
(@E365,'colaborador',N'Cultura y Pertenencia',N'Después de un año, ¿qué tan alineado te sientes con la cultura y valores de DOS?','escala',N'["1 - Nada alineado","2","3","4","5 - Totalmente alineado"]',6),
(@E365,'colaborador',N'Cultura y Pertenencia',N'En una escala de 0 a 10, ¿qué tan probable es que recomiendes a alguien cercano venir a trabajar aquí?','escala',N'["0","1","2","3","4","5","6","7","8","9","10"]',7),
(@E365,'colaborador',N'Cultura y Pertenencia',N'¿Por qué diste esa calificación?','texto_abierto',NULL,8),
(@E365,'colaborador',N'Desarrollo y Proyección',N'¿Dónde te ves en tu desarrollo dentro de DOS en los próximos 1-2 años?','texto_abierto',NULL,9),
(@E365,'colaborador',N'Desarrollo y Proyección',N'¿Qué tipo de crecimiento priorizas?','opcion_unica',N'["Profundizar en mi especialidad","Liderazgo de personas","Explorar otra área dentro de DOS","Aún no lo tengo claro"]',10),
(@E365,'colaborador',N'Desarrollo y Proyección',N'¿Qué te haría considerar dejar la empresa? (marca los que apliquen)','opcion_multiple',N'["Mejor oferta salarial","Falta de oportunidades de crecimiento","Ambiente o cultura de equipo","Carga de trabajo insostenible","Falta de reconocimiento","Cambio de carrera o industria","Ninguna, me veo aquí a largo plazo","Otro"]',11),
(@E365,'colaborador',N'Desarrollo y Proyección',N'Cuéntanos más sobre tu respuesta anterior (opcional)','texto_abierto',NULL,12),
(@E365,'colaborador',N'Evaluación del Proceso de Onboarding Completo',N'Mirando todo el proceso desde el día 1, ¿qué parte de la curva de aprendizaje de DOS cambiarías para el próximo colaborador que llegue a tu área?','texto_abierto',NULL,13),
(@E365,'colaborador',N'Evaluación del Proceso de Onboarding Completo',N'¿Qué tan bien te acompañó tu jefe directo durante este primer año?','escala',N'["1 - Muy poco","2","3","4","5 - Muy bien"]',14),
-- ===================== 30 días · jefe =====================
(@E30,'jefe',N'Desempeño Inicial Observado',N'¿Cómo calificarías la adaptación del colaborador al puesto en su primer mes?','escala',N'["Excelente","Buena","Regular","Preocupante"]',1),
(@E30,'jefe',N'Desempeño Inicial Observado',N'¿El colaborador ha demostrado comprender sus responsabilidades principales?','escala',N'["Sí, totalmente","Sí, en general","Parcialmente","No"]',2),
(@E30,'jefe',N'Autonomía y Aprendizaje',N'¿Con qué nivel de supervisión ha necesitado trabajar hasta ahora?','escala',N'["Ninguna","Ocasional","Frecuente","Constante"]',3),
(@E30,'jefe',N'Autonomía y Aprendizaje',N'¿Qué tan rápido ha adquirido las habilidades técnicas necesarias comparado con lo esperado para el puesto?','escala',N'["Más rápido de lo esperado","Según lo esperado","Más lento de lo esperado","Muy por debajo de lo esperado"]',4),
(@E30,'jefe',N'Integración',N'¿Cómo describirías la integración del colaborador con el equipo?','escala',N'["Excelente","Buena","Regular","Preocupante"]',5),
(@E30,'jefe',N'Integración',N'¿Has identificado alguna señal de alerta temprana (actitud, comunicación, desempeño)?','si_no',N'["Sí","No"]',6),
(@E30,'jefe',N'Integración',N'Si respondiste que sí, detalla la señal de alerta identificada','texto_abierto',NULL,7),
(@E30,'jefe',N'Compromisos y Plan',N'Compromisos de capacitación/desarrollo acordados para los próximos 60 días','compromisos',NULL,8),
(@E30,'jefe',N'Cierre',N'Conclusión y recomendación general','texto_abierto',NULL,9),
-- ===================== 90 días · jefe =====================
(@E90,'jefe',N'Desempeño y Cumplimiento de Expectativas',N'¿El colaborador cumple con el nivel de desempeño esperado para su cargo a esta altura?','escala',N'["Supera lo esperado","Cumple lo esperado","Por debajo de lo esperado","Muy por debajo de lo esperado"]',1),
(@E90,'jefe',N'Desempeño y Cumplimiento de Expectativas',N'¿Qué tan autónomo es hoy en sus tareas principales?','escala',N'["Totalmente autónomo","Autónomo en la mayoría de tareas","Requiere apoyo frecuente","Requiere apoyo constante"]',2),
(@E90,'jefe',N'Evaluación por Competencias',N'Califica cada competencia de 1 (bajo) a 5 (excelente)','competencias_multi',@COMP,3),
(@E90,'jefe',N'Decisión de Período de Prueba',N'Decisión formal sobre el período de prueba','decision_periodo_prueba',N'["aprobado","extendido","rechazado"]',4),
(@E90,'jefe',N'Plan hacia Adelante',N'Compromisos y enfoque para los próximos meses (si el período de prueba fue aprobado o extendido)','compromisos',NULL,5),
(@E90,'jefe',N'Cierre',N'Conclusión','texto_abierto',NULL,6),
-- ===================== 365 días · jefe =====================
(@E365,'jefe',N'Desempeño Consolidado',N'¿Cómo calificarías el desempeño global del colaborador en su primer año?','escala',N'["Sobresaliente","Bueno","Regular","Insuficiente"]',1),
(@E365,'jefe',N'Desempeño Consolidado',N'Califica cada competencia de 1 (bajo) a 5 (excelente)','competencias_multi',@COMP,2),
(@E365,'jefe',N'Desarrollo y Proyección',N'¿Recomendarías al colaborador para mayor responsabilidad, promoción o cambio de rol en el próximo año?','texto_abierto',NULL,3),
(@E365,'jefe',N'Desarrollo y Proyección',N'¿Qué necesita este colaborador para seguir creciendo?','texto_abierto',NULL,4),
(@E365,'jefe',N'Retención',N'¿Qué riesgo de fuga percibes en este colaborador?','escala',N'["Bajo","Medio","Alto"]',5),
(@E365,'jefe',N'Retención',N'¿Por qué percibes ese nivel de riesgo? (marca los que apliquen)','opcion_multiple',N'["Comentarios directos del colaborador","Bajo desempeño o desmotivación observada","Mejores ofertas en el mercado para su perfil","Conflictos con el equipo o liderazgo","Carga de trabajo o condiciones del puesto","Sin señales concretas, es una apreciación general","Otro"]',6),
(@E365,'jefe',N'Retención',N'Detalles adicionales (opcional)','texto_abierto',NULL,7),
(@E365,'jefe',N'Cierre',N'Plan de desarrollo sugerido para el año 2','texto_abierto',NULL,8),
(@E365,'jefe',N'Cierre',N'Conclusión','texto_abierto',NULL,9);
END
GO

/* ------------------------------------------------------ verificacion --- */
SET NOCOUNT ON;
DECLARE @Fallos INT = 0;
IF (SELECT COUNT(*) FROM dbo.CA_etapas) <> 5 BEGIN PRINT 'FALLO: se esperaban 5 etapas.'; SET @Fallos += 1; END
IF (SELECT COUNT(*) FROM dbo.CA_preguntas) <> 90 BEGIN PRINT 'FALLO: se esperaban 90 preguntas.'; SET @Fallos += 1; END
DECLARE @Esperado TABLE (codigo VARCHAR(5), rol VARCHAR(12), n INT);
INSERT INTO @Esperado VALUES ('30','colaborador',17),('60','colaborador',12),('90','colaborador',13),
  ('180','colaborador',10),('365','colaborador',14),('30','jefe',9),('90','jefe',6),('365','jefe',9);
IF EXISTS (SELECT 1 FROM @Esperado x
            WHERE x.n <> (SELECT COUNT(*) FROM dbo.CA_preguntas p JOIN dbo.CA_etapas e ON e.id = p.etapa_id
                           WHERE e.codigo = x.codigo AND p.rol_evaluador = x.rol))
BEGIN PRINT 'FALLO: el reparto de preguntas por etapa y rol no es el de la seccion 5.'; SET @Fallos += 1; END
IF NOT EXISTS (SELECT 1 FROM dbo.CA_etapas WHERE codigo = '365' AND nombre = N'1 A' + NCHAR(241) + N'o')
BEGIN PRINT 'FALLO: la etapa 365 no dice "1 Anio" con enie: el archivo se leyo con la codificacion equivocada.'; SET @Fallos += 1; END
IF @Fallos = 0 PRINT 'OK: catalogo de Curva de Aprendizaje sembrado.'
ELSE PRINT 'VEREDICTO: FALLO (' + CAST(@Fallos AS VARCHAR(10)) + ' comprobaciones). Revisar arriba.';
GO
SET NOEXEC OFF;
GO
