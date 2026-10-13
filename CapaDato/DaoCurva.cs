using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Acceso a datos del modulo Curva de Aprendizaje: una llamada por
    /// procedimiento Sp_RTA_Curva* (docs/sql/2026-10-05-curva-aprendizaje-esquema.sql).
    ///
    /// No calcula ni decide nada: el indice, las alertas, la conclusion y el
    /// avance de etapa viven en CapaNegocio (CapaNegocio -> CapaDato, nunca al
    /// reves). Todas las llamadas reciben la DaoCurvaUnidad de quien orquesta.
    /// </summary>
    public static class DaoCurva
    {
        /* ============================================================ lecturas */

        public static List<EntCurvaEtapa> Etapas(DaoCurvaUnidad u)
        {
            var lista = new List<EntCurvaEtapa>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEtapas"))
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new EntCurvaEtapa
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        Codigo = Texto(dr, "codigo"),
                        Nombre = Texto(dr, "nombre"),
                        DiasOffset = Convert.ToInt32(dr["dias_offset"]),
                        Orden = Convert.ToInt32(dr["orden"])
                    });
                }
            }
            return lista;
        }

        public static List<EntCurvaPregunta> Preguntas(DaoCurvaUnidad u, int etapaId, string rol)
        {
            var lista = new List<EntCurvaPregunta>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaPreguntas"))
            {
                cmd.Parameters.Add("@EtapaId", SqlDbType.Int).Value = etapaId;
                cmd.Parameters.Add("@Rol", SqlDbType.VarChar, 12).Value = rol;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntCurvaPregunta
                        {
                            Id = Convert.ToInt32(dr["id"]),
                            EtapaId = Convert.ToInt32(dr["etapa_id"]),
                            RolEvaluador = Texto(dr, "rol_evaluador"),
                            Seccion = Texto(dr, "seccion"),
                            TextoPregunta = Texto(dr, "texto_pregunta"),
                            TipoRespuesta = Texto(dr, "tipo_respuesta"),
                            OpcionesJson = Texto(dr, "opciones"),
                            Orden = Convert.ToInt32(dr["orden"]),
                            Activa = Convert.ToBoolean(dr["activa"])
                        });
                    }
                }
            }
            return lista;
        }

        public static List<EntCurvaEmpleado> ListarEmpleados(DaoCurvaUnidad u)
        {
            var lista = new List<EntCurvaEmpleado>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEmpleadosListar"))
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read()) lista.Add(LeerEmpleado(dr, false));
            }
            return lista;
        }

        public static EntCurvaEmpleado EmpleadoPorId(DaoCurvaUnidad u, int id)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEmpleadoPorId"))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                using (var dr = cmd.ExecuteReader())
                {
                    return dr.Read() ? LeerEmpleado(dr, true) : null;
                }
            }
        }

        public static List<EntCurvaEmpleadoSistema> BuscarEmpleadoSistema(DaoCurvaUnidad u, string texto)
        {
            var lista = new List<EntCurvaEmpleadoSistema>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaBuscarEmpleadoSistema"))
            {
                cmd.Parameters.Add("@Texto", SqlDbType.NVarChar, 100).Value = texto ?? "";
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntCurvaEmpleadoSistema
                        {
                            IdEmpleado = Convert.ToInt64(dr["IdEmpleado"]),
                            Nombre = Texto(dr, "Nombre"),
                            Cargo = Texto(dr, "PuestoTrabajo"),
                            Area = Texto(dr, "AreaTrabajo"),
                            Empresa = Texto(dr, "Sociedad")
                        });
                    }
                }
            }
            return lista;
        }

        public static EntCurvaProceso ProcesoPorId(DaoCurvaUnidad u, int id)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaProcesoPorId"))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;
                    return new EntCurvaProceso
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        EmpleadoId = Convert.ToInt32(dr["empleado_id"]),
                        FechaInicio = Convert.ToDateTime(dr["fecha_inicio"]),
                        Estado = Texto(dr, "estado"),
                        ResumenEjecutivoAnual = Texto(dr, "resumen_ejecutivo_anual")
                    };
                }
            }
        }

        public static List<EntCurvaEvaluacion> EvaluacionesPorProceso(DaoCurvaUnidad u, int procesoId)
        {
            var lista = new List<EntCurvaEvaluacion>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEvaluacionesPorProceso"))
            {
                cmd.Parameters.Add("@ProcesoId", SqlDbType.Int).Value = procesoId;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read()) lista.Add(LeerEvaluacion(dr));
                }
            }
            return lista;
        }

        public static EntCurvaEvaluacion EvaluacionPorId(DaoCurvaUnidad u, int id)
        {
            return Evaluacion(u, id, null);
        }

        public static EntCurvaEvaluacion EvaluacionPorToken(DaoCurvaUnidad u, string token)
        {
            return Evaluacion(u, null, token);
        }

        private static EntCurvaEvaluacion Evaluacion(DaoCurvaUnidad u, int? id, string token)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEvaluacion"))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = (object)id ?? DBNull.Value;
                cmd.Parameters.Add("@Token", SqlDbType.Char, 48).Value = (object)token ?? DBNull.Value;
                using (var dr = cmd.ExecuteReader())
                {
                    return dr.Read() ? LeerEvaluacion(dr) : null;
                }
            }
        }

        public static List<EntCurvaRespuesta> RespuestasPorProceso(DaoCurvaUnidad u, int procesoId)
        {
            return Respuestas(u, procesoId, null);
        }

        public static List<EntCurvaRespuesta> RespuestasPorEvaluacion(DaoCurvaUnidad u, int evaluacionId)
        {
            return Respuestas(u, null, evaluacionId);
        }

        private static List<EntCurvaRespuesta> Respuestas(DaoCurvaUnidad u, int? procesoId, int? evaluacionId)
        {
            var lista = new List<EntCurvaRespuesta>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaRespuestas"))
            {
                cmd.Parameters.Add("@ProcesoId", SqlDbType.Int).Value = (object)procesoId ?? DBNull.Value;
                cmd.Parameters.Add("@EvaluacionId", SqlDbType.Int).Value = (object)evaluacionId ?? DBNull.Value;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntCurvaRespuesta
                        {
                            Id = Convert.ToInt32(dr["id"]),
                            EvaluacionId = Convert.ToInt32(dr["evaluacion_id"]),
                            PreguntaId = Convert.ToInt32(dr["pregunta_id"]),
                            ValorRespuesta = Texto(dr, "valor_respuesta"),
                            TextoPregunta = Texto(dr, "texto_pregunta"),
                            TipoRespuesta = Texto(dr, "tipo_respuesta"),
                            Seccion = Texto(dr, "seccion"),
                            Orden = Convert.ToInt32(dr["orden"]),
                            OpcionesJson = Texto(dr, "opciones")
                        });
                    }
                }
            }
            return lista;
        }

        public static List<EntCurvaCompromiso> CompromisosPorProceso(DaoCurvaUnidad u, int procesoId)
        {
            var lista = new List<EntCurvaCompromiso>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaCompromisos"))
            {
                cmd.Parameters.Add("@ProcesoId", SqlDbType.Int).Value = procesoId;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var c = LeerCompromiso(dr);
                        c.NSeguimientos = Convert.ToInt32(dr["n_seguimientos"]);
                        c.UltimoEstado = Texto(dr, "ultimo_estado");
                        lista.Add(c);
                    }
                }
            }
            return lista;
        }

        public static List<EntCurvaCompromiso> CompromisosAbiertos(DaoCurvaUnidad u, int evaluacionId)
        {
            var lista = new List<EntCurvaCompromiso>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaCompromisosAbiertos"))
            {
                cmd.Parameters.Add("@EvaluacionId", SqlDbType.Int).Value = evaluacionId;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var c = LeerCompromiso(dr);
                        c.EstadoActual = Texto(dr, "estado_actual");
                        c.JustificacionActual = Texto(dr, "justificacion_actual");
                        lista.Add(c);
                    }
                }
            }
            return lista;
        }

        public static List<EntCurvaSeguimiento> SeguimientosPorProceso(DaoCurvaUnidad u, int procesoId)
        {
            var lista = new List<EntCurvaSeguimiento>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaSeguimientos"))
            {
                cmd.Parameters.Add("@ProcesoId", SqlDbType.Int).Value = procesoId;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntCurvaSeguimiento
                        {
                            Id = Convert.ToInt32(dr["id"]),
                            CompromisoId = Convert.ToInt32(dr["compromiso_id"]),
                            EvaluacionId = Convert.ToInt32(dr["evaluacion_id"]),
                            Estado = Texto(dr, "estado"),
                            Justificacion = Texto(dr, "justificacion"),
                            FechaRevision = Convert.ToDateTime(dr["fecha_revision"]),
                            Descripcion = Texto(dr, "descripcion"),
                            Categoria = Texto(dr, "categoria")
                        });
                    }
                }
            }
            return lista;
        }

        public static List<EntCurvaDecision> DecisionesPorProceso(DaoCurvaUnidad u, int procesoId)
        {
            var lista = new List<EntCurvaDecision>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaDecisiones"))
            {
                cmd.Parameters.Add("@ProcesoId", SqlDbType.Int).Value = procesoId;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntCurvaDecision
                        {
                            Id = Convert.ToInt32(dr["id"]),
                            ProcesoId = Convert.ToInt32(dr["proceso_id"]),
                            EvaluacionId = Convert.ToInt32(dr["evaluacion_id"]),
                            Decision = Texto(dr, "decision"),
                            NuevaFechaFin = FechaNula(dr, "nueva_fecha_fin"),
                            Comentarios = Texto(dr, "comentarios"),
                            FechaDecision = Convert.ToDateTime(dr["fecha_decision"])
                        });
                    }
                }
            }
            return lista;
        }

        public static EntCurvaDashboardInsumos Dashboard(DaoCurvaUnidad u)
        {
            var d = new EntCurvaDashboardInsumos();
            var porEval = new Dictionary<int, EntCurvaInsumoEvaluacion>();
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaDashboard"))
            using (var dr = cmd.ExecuteReader())
            {
                if (dr.Read()) d.TotalColaboradores = Convert.ToInt32(dr["total"]);

                dr.NextResult();
                while (dr.Read())
                    d.Procesos.Add(new EntCurvaDashProceso
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        EmpleadoId = Convert.ToInt32(dr["empleado_id"]),
                        Estado = Texto(dr, "estado"),
                        NombreCompleto = Texto(dr, "nombre_completo"),
                        Area = Texto(dr, "area")
                    });

                dr.NextResult();
                while (dr.Read()) d.Decisiones.Add(new KeyValuePair<string, string>(Texto(dr, "decision"), Texto(dr, "area")));

                dr.NextResult();
                while (dr.Read())
                    d.Compromisos.Add(new EntCurvaDashCompromiso
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        Categoria = Texto(dr, "categoria"),
                        Area = Texto(dr, "area"),
                        UltimoEstado = Texto(dr, "ultimo_estado")
                    });

                dr.NextResult();
                while (dr.Read()) d.Competencias.Add(new KeyValuePair<string, string>(Texto(dr, "etapa_codigo"), Texto(dr, "valor_respuesta")));

                dr.NextResult();
                while (dr.Read())
                    d.Proximas.Add(new EntCurvaProxima
                    {
                        EvaluacionId = Convert.ToInt32(dr["id"]),
                        EmpleadoId = Convert.ToInt32(dr["empleado_id"]),
                        NombreCompleto = Texto(dr, "nombre_completo"),
                        Area = Texto(dr, "area"),
                        EtapaCodigo = Texto(dr, "etapa_codigo"),
                        EtapaNombre = Texto(dr, "etapa_nombre"),
                        Rol = Texto(dr, "evaluador_rol"),
                        FechaObjetivo = FechaNula(dr, "fecha_objetivo"),
                        Vencida = Convert.ToInt32(dr["vencida"]) == 1
                    });

                dr.NextResult();
                while (dr.Read())
                {
                    var ev = new EntCurvaInsumoEvaluacion
                    {
                        EvaluacionId = Convert.ToInt32(dr["id"]),
                        Rol = Texto(dr, "evaluador_rol"),
                        EtapaCodigo = Texto(dr, "etapa_codigo")
                    };
                    porEval[ev.EvaluacionId] = ev;
                    d.EvaluacionesEnCurso.Add(ev);
                    d.ProcesoDeEvaluacion[ev.EvaluacionId] = Convert.ToInt32(dr["proceso_id"]);
                }

                dr.NextResult();
                while (dr.Read())
                {
                    EntCurvaInsumoEvaluacion ev;
                    if (porEval.TryGetValue(Convert.ToInt32(dr["evaluacion_id"]), out ev))
                        ev.Respuestas.Add(new EntCurvaRespuesta
                        {
                            EvaluacionId = ev.EvaluacionId,
                            ValorRespuesta = Texto(dr, "valor_respuesta"),
                            TipoRespuesta = Texto(dr, "tipo_respuesta"),
                            OpcionesJson = Texto(dr, "opciones")
                        });
                }

                dr.NextResult();
                while (dr.Read())
                {
                    EntCurvaInsumoEvaluacion ev;
                    if (porEval.TryGetValue(Convert.ToInt32(dr["evaluacion_id"]), out ev))
                        ev.EstadosSeguimiento.Add(Texto(dr, "estado"));
                }
            }
            return d;
        }

        /* ========================================================== escrituras */

        public static int CrearEmpleado(DaoCurvaUnidad u, EntCurvaEmpleado e, string usuario)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEmpleadoCrear"))
            {
                cmd.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 200).Value = e.NombreCompleto;
                cmd.Parameters.Add("@Cargo", SqlDbType.NVarChar, 200).Value = Nulo(e.Cargo);
                cmd.Parameters.Add("@Area", SqlDbType.NVarChar, 150).Value = Nulo(e.Area);
                cmd.Parameters.Add("@Empresa", SqlDbType.NVarChar, 150).Value = Nulo(e.Empresa);
                cmd.Parameters.Add("@UnidadNegocio", SqlDbType.NVarChar, 150).Value = Nulo(e.UnidadNegocio);
                cmd.Parameters.Add("@FechaIngreso", SqlDbType.Date).Value = e.FechaIngreso.Date;
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.BigInt).Value = (object)e.IdEmpleado ?? DBNull.Value;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = Nulo(usuario);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int CambiarEstadoEmpleado(DaoCurvaUnidad u, int id, string estado)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEmpleadoEstado"))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@Estado", SqlDbType.VarChar, 10).Value = estado;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>Borra al colaborador con todo su historial. 0 = no existia.</summary>
        public static int EliminarEmpleado(DaoCurvaUnidad u, int id)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEmpleadoEliminar"))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int CrearProceso(DaoCurvaUnidad u, int empleadoId, DateTime fechaInicio)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaProcesoCrear"))
            {
                cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = empleadoId;
                cmd.Parameters.Add("@FechaInicio", SqlDbType.Date).Value = fechaInicio.Date;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void CambiarEstadoProceso(DaoCurvaUnidad u, int id, string estado)
        {
            ActualizarProceso(u, id, estado, false, null);
        }

        public static void GuardarResumenAnual(DaoCurvaUnidad u, int id, string resumen)
        {
            ActualizarProceso(u, id, null, true, resumen);
        }

        private static void ActualizarProceso(DaoCurvaUnidad u, int id, string estado, bool cambiarResumen, string resumen)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaProcesoActualizar"))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@Estado", SqlDbType.VarChar, 12).Value = Nulo(estado);
                cmd.Parameters.Add("@CambiarResumen", SqlDbType.Bit).Value = cambiarResumen;
                cmd.Parameters.Add("@Resumen", SqlDbType.NVarChar, -1).Value = Nulo(resumen);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Crea la evaluacion si no existe. Devuelve true si la creo.</summary>
        public static bool CrearEvaluacion(DaoCurvaUnidad u, int procesoId, int etapaId, string rol, DateTime fechaObjetivo)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEvaluacionCrear"))
            {
                cmd.Parameters.Add("@ProcesoId", SqlDbType.Int).Value = procesoId;
                cmd.Parameters.Add("@EtapaId", SqlDbType.Int).Value = etapaId;
                cmd.Parameters.Add("@Rol", SqlDbType.VarChar, 12).Value = rol;
                cmd.Parameters.Add("@FechaObjetivo", SqlDbType.Date).Value = fechaObjetivo.Date;
                using (var dr = cmd.ExecuteReader())
                {
                    return dr.Read() && Convert.ToInt32(dr["creada"]) == 1;
                }
            }
        }

        /// <summary>
        /// Pasa la evaluacion de pendiente a completada (fecha de hoy) y le
        /// fija la conclusion. Devuelve false si ya no estaba pendiente: es lo
        /// que frena un doble envio aunque lleguen dos a la vez.
        /// </summary>
        public static bool MarcarCompletada(DaoCurvaUnidad u, int id, string conclusion)
        {
            return ActualizarEvaluacion(u, id, true, true, conclusion, false, null, false, null) > 0;
        }

        /// <summary>Solo la conclusion; no toca estado ni fecha_completada (editar).</summary>
        public static void GuardarConclusion(DaoCurvaUnidad u, int id, string conclusion)
        {
            ActualizarEvaluacion(u, id, false, true, conclusion, false, null, false, null);
        }

        public static void GuardarResumenEvaluacion(DaoCurvaUnidad u, int id, string resumen)
        {
            ActualizarEvaluacion(u, id, false, false, null, true, resumen, false, null);
        }

        public static void GuardarPlanAccion(DaoCurvaUnidad u, int id, string plan)
        {
            ActualizarEvaluacion(u, id, false, false, null, false, null, true, plan);
        }

        private static int ActualizarEvaluacion(DaoCurvaUnidad u, int id, bool completar, bool cambiarConclusion, string conclusion,
            bool cambiarResumen, string resumen, bool cambiarPlan, string plan)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEvaluacionActualizar"))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@Completar", SqlDbType.Bit).Value = completar;
                cmd.Parameters.Add("@CambiarConclusion", SqlDbType.Bit).Value = cambiarConclusion;
                cmd.Parameters.Add("@Conclusion", SqlDbType.NVarChar, -1).Value = Nulo(conclusion);
                cmd.Parameters.Add("@CambiarResumen", SqlDbType.Bit).Value = cambiarResumen;
                cmd.Parameters.Add("@Resumen", SqlDbType.NVarChar, -1).Value = Nulo(resumen);
                cmd.Parameters.Add("@CambiarPlan", SqlDbType.Bit).Value = cambiarPlan;
                cmd.Parameters.Add("@Plan", SqlDbType.NVarChar, -1).Value = Nulo(plan);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>false si la evaluacion no existe o ya no esta pendiente.</summary>
        public static bool GuardarToken(DaoCurvaUnidad u, int id, string token)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEvaluacionToken"))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@Token", SqlDbType.Char, 48).Value = token;
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        /// <summary>0 bien, -1 no existe, -2 el proceso ya avanzo, -3 compromisos con seguimiento en otra evaluacion.</summary>
        public static int EliminarEvaluacion(DaoCurvaUnidad u, int id)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaEvaluacionEliminar"))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void GuardarRespuesta(DaoCurvaUnidad u, int evaluacionId, int preguntaId, string valor)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaRespuestaGuardar"))
            {
                cmd.Parameters.Add("@EvaluacionId", SqlDbType.Int).Value = evaluacionId;
                cmd.Parameters.Add("@PreguntaId", SqlDbType.Int).Value = preguntaId;
                cmd.Parameters.Add("@Valor", SqlDbType.NVarChar, -1).Value = Nulo(valor);
                cmd.ExecuteNonQuery();
            }
        }

        public static int CrearCompromiso(DaoCurvaUnidad u, int evaluacionId, EntCurvaCompromisoFila f)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaCompromisoCrear"))
            {
                cmd.Parameters.Add("@EvaluacionId", SqlDbType.Int).Value = evaluacionId;
                cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 1000).Value = f.Descripcion;
                cmd.Parameters.Add("@Categoria", SqlDbType.NVarChar, 200).Value = Nulo(f.Categoria);
                cmd.Parameters.Add("@Responsable", SqlDbType.NVarChar, 200).Value = Nulo(f.Responsable);
                cmd.Parameters.Add("@FechaLimite", SqlDbType.Date).Value = (object)f.FechaLimite ?? DBNull.Value;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int BorrarCompromisosSinSeguimiento(DaoCurvaUnidad u, int evaluacionId)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaCompromisosBorrarSinSeguimiento"))
            {
                cmd.Parameters.Add("@EvaluacionId", SqlDbType.Int).Value = evaluacionId;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>false si el compromiso no es del mismo proceso que la evaluacion.</summary>
        public static bool GuardarSeguimiento(DaoCurvaUnidad u, int evaluacionId, EntCurvaSeguimientoInput s)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaSeguimientoGuardar"))
            {
                cmd.Parameters.Add("@CompromisoId", SqlDbType.Int).Value = s.CompromisoId;
                cmd.Parameters.Add("@EvaluacionId", SqlDbType.Int).Value = evaluacionId;
                cmd.Parameters.Add("@Estado", SqlDbType.VarChar, 12).Value = s.Estado;
                cmd.Parameters.Add("@Justificacion", SqlDbType.NVarChar, 1000).Value = Nulo(s.Justificacion);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static void GuardarDecision(DaoCurvaUnidad u, int procesoId, int evaluacionId, EntCurvaDecisionInput d)
        {
            using (var cmd = u.Procedimiento("Sp_RTA_CurvaDecisionGuardar"))
            {
                cmd.Parameters.Add("@ProcesoId", SqlDbType.Int).Value = procesoId;
                cmd.Parameters.Add("@EvaluacionId", SqlDbType.Int).Value = evaluacionId;
                cmd.Parameters.Add("@Decision", SqlDbType.VarChar, 10).Value = d.Decision;
                cmd.Parameters.Add("@NuevaFechaFin", SqlDbType.Date).Value = (object)d.NuevaFechaFin ?? DBNull.Value;
                cmd.Parameters.Add("@Comentarios", SqlDbType.NVarChar, -1).Value = Nulo(d.Comentarios);
                cmd.ExecuteNonQuery();
            }
        }

        /* ============================================================ helpers */

        private static EntCurvaEmpleado LeerEmpleado(SqlDataReader dr, bool conJefe)
        {
            return new EntCurvaEmpleado
            {
                Id = Convert.ToInt32(dr["id"]),
                NombreCompleto = Texto(dr, "nombre_completo"),
                Cargo = Texto(dr, "cargo"),
                Area = Texto(dr, "area"),
                Empresa = Texto(dr, "empresa"),
                UnidadNegocio = Texto(dr, "unidad_negocio"),
                JefeId = dr["jefe_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["jefe_id"]),
                JefeNombre = conJefe ? Texto(dr, "jefe_nombre") : null,
                FechaIngreso = Convert.ToDateTime(dr["fecha_ingreso"]),
                Estado = Texto(dr, "estado"),
                CreatedAt = Convert.ToDateTime(dr["created_at"]),
                IdEmpleado = dr["IdEmpleado"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["IdEmpleado"]),
                ProcesoId = dr["proceso_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["proceso_id"]),
                ProcesoEstado = Texto(dr, "proceso_estado")
            };
        }

        private static EntCurvaEvaluacion LeerEvaluacion(SqlDataReader dr)
        {
            return new EntCurvaEvaluacion
            {
                Id = Convert.ToInt32(dr["id"]),
                ProcesoId = Convert.ToInt32(dr["proceso_id"]),
                EtapaId = Convert.ToInt32(dr["etapa_id"]),
                EmpleadoId = Convert.ToInt32(dr["empleado_id"]),
                EvaluadorRol = Texto(dr, "evaluador_rol"),
                Estado = Texto(dr, "estado"),
                FechaObjetivo = FechaNula(dr, "fecha_objetivo"),
                FechaCompletada = FechaNula(dr, "fecha_completada"),
                Conclusion = Texto(dr, "conclusion"),
                ResumenEjecutivo = Texto(dr, "resumen_ejecutivo"),
                PlanAccionGth = Texto(dr, "plan_accion_gth"),
                Token = Texto(dr, "token"),
                EtapaCodigo = Texto(dr, "etapa_codigo"),
                EtapaNombre = Texto(dr, "etapa_nombre"),
                EtapaOrden = Convert.ToInt32(dr["etapa_orden"])
            };
        }

        private static EntCurvaCompromiso LeerCompromiso(SqlDataReader dr)
        {
            return new EntCurvaCompromiso
            {
                Id = Convert.ToInt32(dr["id"]),
                EvaluacionId = Convert.ToInt32(dr["evaluacion_id"]),
                Descripcion = Texto(dr, "descripcion"),
                Categoria = Texto(dr, "categoria"),
                Responsable = Texto(dr, "responsable"),
                FechaLimite = FechaNula(dr, "fecha_limite"),
                CreatedAt = Convert.ToDateTime(dr["created_at"]),
                EtapaCodigo = Texto(dr, "etapa_codigo"),
                EtapaNombre = Texto(dr, "etapa_nombre"),
                CreadoPorRol = Texto(dr, "creado_por_rol")
            };
        }

        /// <summary>null-safe, SIN Trim: las opciones se comparan de forma exacta.</summary>
        private static string Texto(SqlDataReader dr, string col)
        {
            object v = dr[col];
            return v == DBNull.Value ? null : Convert.ToString(v);
        }

        private static DateTime? FechaNula(SqlDataReader dr, string col)
        {
            object v = dr[col];
            return v == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(v);
        }

        private static object Nulo(string s)
        {
            return s == null ? (object)DBNull.Value : s;
        }
    }
}
