using EducaMente.AccessData;
using EducaMente.DTO;
using EducaMente.Interface;
using EducaMente.Utilities;
using System.Data;
using System.Globalization;

namespace EducaMente.Repositories
{
    public class PsicoTestRepos : I_PsicoTest
    {
        private readonly AccesoData accessData;
        private readonly IConfiguration configuration;
        private readonly I_PsicoPreguntaBank _psicoPreguntaBankRepos;

        public PsicoTestRepos(AccesoData accessData, IConfiguration configuration, I_PsicoPreguntaBank psicoPreguntaBankRepos)
        {
            this.accessData = accessData;
            this.configuration = configuration;
            _psicoPreguntaBankRepos = psicoPreguntaBankRepos;
        }

        public async Task<Response1StringDTO> CrearTestUniversalAsync(PsicoTestUniversalAddDTO dto)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.PsicoTestCreate";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 600).Value = dto.Descripcion;
                cmd.Parameters.Add("@Alcance", SqlDbType.VarChar, 20).Value = "Universal";
                cmd.Parameters.Add("@TargetUsuarioId", SqlDbType.VarChar, 50).Value = DBNull.Value;
                cmd.Parameters.Add("@FechaCreacion", SqlDbType.DateTime).Value = UtilidadesTiempo.ObtenerFechaColombia();

                var outputParam = cmd.Parameters.Add("@NuevoTestId", SqlDbType.VarChar, 50);
                outputParam.Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                return new Response1StringDTO
                {
                    Id = outputParam.Value.ToString(),
                    Mensaje = "Test universal creado correctamente"
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Response1StringDTO> CrearTestPersonalizadoAsync(PsicoTestPersonalizadoAddDTO dto)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.PsicoTestCreate";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 600).Value = (object?)dto.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@Alcance", SqlDbType.VarChar, 20).Value = "Personalizado";
                cmd.Parameters.Add("@TargetUsuarioId", SqlDbType.VarChar, 50).Value = dto.TargetUsuarioId;
                cmd.Parameters.Add("@FechaCreacion", SqlDbType.DateTime).Value = UtilidadesTiempo.ObtenerFechaColombia();

                var outputParam = cmd.Parameters.Add("@NuevoTestId", SqlDbType.VarChar, 50);
                outputParam.Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                return new Response1StringDTO
                {
                    Id = outputParam.Value.ToString(),
                    Mensaje = "Test personalizado creado correctamente"
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Response1StringDTO> CrearPreguntaYAgregarATestAsync(PsicoPreguntaBankAddToTestDTO dto)
        {
            // 1) Crear la pregunta en el banco
            var createResult = await _psicoPreguntaBankRepos.AddAsync(new PsicoPreguntaBankAddDTO
            {
                Texto = dto.Texto,
                Dimension = dto.Dimension,
                ScaleId = dto.ScaleId,
                Fuente = "Orientador"
            });

            if (string.IsNullOrWhiteSpace(createResult.Id))
                throw new Exception("No se pudo obtener el Id de la nueva pregunta.");

            // 2) Asociar la pregunta creada al test
            var linkResult = await AgregarPreguntaATestAsync(new PsicoTestPreguntaAddDTO
            {
                TestId = dto.TestId,
                PreguntaId = createResult.Id
            });

            // 3) Puedes devolver un mensaje combinado o solo el de creación/asociación
            return new Response1StringDTO
            {
                Id = createResult.Id, // Id de la pregunta creada
                Mensaje = "Pregunta creada y agregada al test correctamente"
            };
        }

        public async Task<Response1StringDTO> CrearPreguntaYAgregarATestByIAAsync(PsicoPreguntaBankAddToTestDTO dto)
        {
            // 1) Crear la pregunta en el banco
            var createResult = await _psicoPreguntaBankRepos.AddAsync(new PsicoPreguntaBankAddDTO
            {
                Texto = dto.Texto,
                Dimension = dto.Dimension,
                ScaleId = dto.ScaleId,
                Fuente = "IA"
            });

            if (string.IsNullOrWhiteSpace(createResult.Id))
                throw new Exception("No se pudo obtener el Id de la nueva pregunta.");

            // 2) Asociar la pregunta creada al test
            var linkResult = await AgregarPreguntaATestAsync(new PsicoTestPreguntaAddDTO
            {
                TestId = dto.TestId,
                PreguntaId = createResult.Id
            });

            // 3) Puedes devolver un mensaje combinado o solo el de creación/asociación
            return new Response1StringDTO
            {
                Id = createResult.Id, // Id de la pregunta creada
                Mensaje = "Pregunta creada y agregada al test correctamente"
            };
        }

        public async Task<Response1StringDTO> AgregarPreguntaATestAsync(PsicoTestPreguntaAddDTO dto)
        {
            try
            {
                using var connection = accessData.GetConnection();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "dbo.PsicoTestAddPregunta";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@TestId", SqlDbType.VarChar, 50).Value = dto.TestId;
                cmd.Parameters.Add("@PreguntaId", SqlDbType.VarChar, 50).Value = dto.PreguntaId;

                var outputParam = cmd.Parameters.Add("@NuevoId", SqlDbType.VarChar, 50);
                outputParam.Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                return new Response1StringDTO
                {
                    Id = outputParam.Value.ToString(),
                    Mensaje = "Pregunta agregada al test correctamente"
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<PsicoTestListDTO>> GetAllAsync()
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PsicoTestGetAll";
                cmd.CommandType = CommandType.StoredProcedure;

                using var reader = await cmd.ExecuteReaderAsync();
                var lista = new List<PsicoTestListDTO>();

                while (await reader.ReadAsync())
                {
                    var item = new PsicoTestListDTO
                    {
                        Id = reader["Id"]?.ToString() ?? string.Empty,
                        Descripcion = reader["Descripcion"] == DBNull.Value ? null : reader["Descripcion"]?.ToString(),
                        Alcance = reader["Alcance"]?.ToString() ?? string.Empty,
                        Estado = reader["Estado"]?.ToString() ?? string.Empty,
                        FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaCreacion"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        TargetUsuarioId = reader["TargetUsuarioId"] == DBNull.Value ? null : reader["TargetUsuarioId"]?.ToString(),
                        TargetUsuarioNombre = reader["TargetUsuarioNombre"] == DBNull.Value ? null : reader["TargetUsuarioNombre"]?.ToString()
                    };

                    lista.Add(item);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PsicoTestDetalleDTO?> ObtenerDetalleTestAsync(string testId)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PsicoTestGetDetalle";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@TestId", SqlDbType.VarChar, 50).Value = testId;

                using var reader = await cmd.ExecuteReaderAsync();

                PsicoTestDetalleDTO? detalle = null;

                //Primer resultset

                if (await reader.ReadAsync())
                {
                    detalle = new PsicoTestDetalleDTO
                    {
                        Id = reader["Id"].ToString() ?? string.Empty,
                        Descripcion = reader["Descripcion"] == DBNull.Value ? string.Empty : reader["Descripcion"].ToString(),
                        Alcance = reader["Alcance"].ToString() ?? string.Empty,
                        TargetUsuarioId = reader["TargetUsuarioId"] == DBNull.Value ? string.Empty : reader["TargetUsuarioId"].ToString(),
                        Estado = reader["Estado"].ToString() ?? string.Empty,
                        FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaCreacion"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                        Preguntas = new List<PsicoTestPreguntaDetalleDTO>()
                    };

                }

                if (detalle == null)
                    return null;

                //Segundo resultset

                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var pregunta = new PsicoTestPreguntaDetalleDTO
                        {
                            PsicoTestPreguntaId = reader["PsicoTestPreguntaId"].ToString()!,
                            Orden = reader["Orden"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Orden"]),
                            PreguntaId = reader["PreguntaId"].ToString()!,
                            Texto = reader["Texto"].ToString()!,
                            Dimension = reader["Dimension"].ToString()!,
                            ScaleId = reader["ScaleId"].ToString()!
                        };

                        detalle.Preguntas.Add(pregunta);
                    }
                }

                return detalle;
            }
            catch (Exception ex)
            {
                // Mantienes el mismo patrón que en el resto del repositorio
                throw new Exception(ex.Message);
            }
        }

        public async Task<UsuarioTestConRespuestasDTO?> ObtenerConRespuestasAsync(string usuarioTestId)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.UsuarioTestGetConRespuestas";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UsuarioTestId", SqlDbType.VarChar, 50).Value = usuarioTestId;

                using var reader = await cmd.ExecuteReaderAsync();

                // ===============================
                // RS1: Cabecera
                // ===============================
                if (!await reader.ReadAsync())
                {
                    // El SP ya valida existencia, pero por seguridad
                    return null;
                }

                var detalle = new UsuarioTestConRespuestasDTO
                {
                    UsuarioTestId = reader["UsuarioTestId"].ToString()!,
                    UsuarioId = reader["UsuarioId"].ToString()!,
                    NombreUsuario = reader["NombreUsuario"]?.ToString() ?? string.Empty,
                    TestId = reader["TestId"].ToString()!,
                    DescripcionTest = reader["DescripcionTest"] == DBNull.Value ? string.Empty : reader["DescripcionTest"].ToString(),
                    Alcance = reader["Alcance"].ToString()!,
                    TargetUsuarioId = reader["TargetUsuarioId"] == DBNull.Value ? string.Empty : reader["TargetUsuarioId"].ToString(),
                    Estado = reader["Estado"].ToString()!,
                    FechaInicio = reader["FechaInicio"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaInicio"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                    FechaFin = reader["FechaFin"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaFin"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO"))
                };

                // ===============================
                // RS2: Items + opciones + respuestas
                // ===============================
                var itemsDict = new Dictionary<string, UsuarioTestItemConRespuestasDTO>();

                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var itemId = reader["UsuarioTestItemId"].ToString()!;
                        if (!itemsDict.TryGetValue(itemId, out var item))
                        {
                            item = new UsuarioTestItemConRespuestasDTO
                            {
                                UsuarioTestItemId = itemId,
                                UsuarioTestId = reader["UsuarioTestId"].ToString()!,
                                PreguntaId = reader["PreguntaId"].ToString()!,
                                TextoSnapshot = reader["TextoSnapshot"].ToString()!,
                                DimensionSnapshot = reader["DimensionSnapshot"].ToString()!,
                                ScaleIdSnapshot = reader["ScaleIdSnapshot"].ToString()!,
                                OrdenPregunta = Convert.ToInt32(reader["OrdenPregunta"])
                            };

                            itemsDict[itemId] = item;
                        }

                        // Puede haber items sin opciones (teóricamente no, pero por seguridad)
                        if (reader["UsuarioTestItemOpcionId"] != DBNull.Value)
                        {
                            var opcion = new UsuarioTestOpcionRespuestaDetalleDTO
                            {
                                UsuarioTestItemOpcionId = reader["UsuarioTestItemOpcionId"].ToString()!,
                                OrdenOpcion = reader["OrdenOpcion"] == DBNull.Value ? 0 : Convert.ToInt32(reader["OrdenOpcion"]),
                                TextoOpcion = reader["TextoOpcion"]?.ToString() ?? string.Empty,
                                ValorOpcion = reader["ValorOpcion"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ValorOpcion"]),
                                UsuarioRespuestaId = reader["UsuarioRespuestaId"] == DBNull.Value ? null : reader["UsuarioRespuestaId"].ToString(),
                                ValorRespuesta = reader["ValorRespuesta"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["ValorRespuesta"]),
                                FechaRespuesta = reader["FechaRespuesta"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaRespuesta"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO")),
                                EsSeleccionada = reader["EsSeleccionada"] != DBNull.Value && Convert.ToInt32(reader["EsSeleccionada"]) == 1
                            };

                            item.Opciones.Add(opcion);
                        }
                    }
                }

                detalle.Items = itemsDict.Values.OrderBy(i => i.OrdenPregunta).ToList();

                return detalle;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UsuarioTestDetalleDTO> UsuarioTestStartAsync(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentException("El UsuarioId no puede ser nulo o vacío.");

            // 1) Obtener TestId automáticamente según el usuario
            var testId = await GetPsicoTestIdByUsuarioAsync(usuarioId);

            // 2) Construir DTO requerido por el SP de inicio del test
            var dto = new UsuarioTestStartDTO
            {
                UsuarioId = usuarioId,
                TestId = testId
            };

            // 3) Iniciar el intento (SP: UsuarioTestStart)
            var usuarioTestId = await StartUsuarioTestAsync(dto);

            // 4) Obtener detalle completo del intento (preguntas, opciones, etc.)
            var detalle = await GetUsuarioTestDetalleAsync(usuarioTestId);

            return detalle;
        }

        public async Task<Response1StringDTO> GuardarRespuestaAsync(UsuarioRespuestaSaveDTO dto)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.UsuarioRespuestaSave";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioTestItemId", SqlDbType.VarChar, 50).Value = dto.UsuarioTestItemId;
                cmd.Parameters.Add("@OpcionId", SqlDbType.VarChar, 50).Value = dto.OpcionId;
                cmd.Parameters.Add("@Now", SqlDbType.DateTime).Value = UtilidadesTiempo.ObtenerFechaColombia();

                var outputParam = cmd.Parameters.Add("@UsuarioRespuestaId", SqlDbType.VarChar, 50);
                outputParam.Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                return new Response1StringDTO
                {
                    Id = outputParam.Value?.ToString() ?? string.Empty,
                    Mensaje = "Respuesta registrada correctamente"
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UsuarioTestResultadoDTO> FinalizarAsync(string usuarioTestId)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.UsuarioTestFinalize";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioTestId", SqlDbType.VarChar, 50).Value = usuarioTestId;
                cmd.Parameters.Add("@Now", SqlDbType.DateTime).Value = UtilidadesTiempo.ObtenerFechaColombia();

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new UsuarioTestResultadoDTO
                    {
                        UsuarioTestId = usuarioTestId,
                        AnsiedadScore = reader["AnsiedadScore"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AnsiedadScore"]),
                        EstresScore = reader["EstresScore"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["EstresScore"]),
                        MotivacionScore = reader["MotivacionScore"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["MotivacionScore"]),
                        AutoestimaScore = reader["AutoestimaScore"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AutoestimaScore"]),
                        PropositoScore = reader["PropositoScore"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["PropositoScore"]),
                        NivelRiesgo = reader["NivelRiesgo"]?.ToString() ?? "",
                        EstadoEmocional = reader["EstadoEmocional"]?.ToString() ?? ""
                    };
                }

                throw new Exception("No se pudo obtener el resultado del test.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> CambiarEstadoAsync(PsicoTestCambiarEstadoDTO dto)
        {
            try
            {
                var estadosPermitidos = new[]
                {
                    "Borrador",
                    "Activo",
                    "Inactivo"
                };

                if (!estadosPermitidos.Contains(dto.NuevoEstado, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException("El estado proporcionado no es válido para el test.");

                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PsicoTestCambiarEstado";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@TestId", SqlDbType.VarChar, 50).Value = dto.TestId;
                cmd.Parameters.Add("@NuevoEstado", SqlDbType.VarChar, 20).Value = dto.NuevoEstado;

                await cmd.ExecuteNonQueryAsync();

                return "Estado del test actualizado correctamente";
            }
            catch (Exception ex)
            {
                // Mantengo tu patrón
                throw new Exception(ex.Message);
            }
        }

        private async Task<string> StartUsuarioTestAsync(UsuarioTestStartDTO dto)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.UsuarioTestStart";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioId", SqlDbType.VarChar, 50).Value = dto.UsuarioId;
                cmd.Parameters.Add("@TestId", SqlDbType.VarChar, 50).Value = dto.TestId;
                cmd.Parameters.Add("@Now", SqlDbType.DateTime).Value = UtilidadesTiempo.ObtenerFechaColombia();

                var outputParam = cmd.Parameters.Add("@UsuarioTestId", SqlDbType.VarChar, 50);
                outputParam.Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();

                var usuarioTestId = outputParam.Value?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(usuarioTestId))
                    throw new Exception("No se pudo obtener el identificador del intento de test.");

                return usuarioTestId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<string> GetPsicoTestIdByUsuarioAsync(string usuarioId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuarioId))
                    throw new ArgumentException("El UsuarioId no puede ser nulo o vacío.");

                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.PsicoTestGetByUsuarioId";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioId", SqlDbType.VarChar, 50).Value = usuarioId;

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return reader["Id"]?.ToString()
                           ?? throw new Exception("El SP no devolvió ningún TestId.");
                }

                throw new Exception("No se encontró un TestId para el usuario especificado.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<UsuarioTestDetalleDTO> GetUsuarioTestDetalleAsync(string usuarioTestId)
        {
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.GetUsuarioTest";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UsuarioTestId", SqlDbType.VarChar, 50).Value = usuarioTestId;

                using var reader = await cmd.ExecuteReaderAsync();

                var detalle = new UsuarioTestDetalleDTO();

                // RS1: cabecera
                if (await reader.ReadAsync())
                {
                    detalle.UsuarioTestId = reader["UsuarioTestId"].ToString() ?? "";
                    detalle.UsuarioId = reader["UsuarioId"].ToString() ?? "";
                    detalle.TestId = reader["TestId"].ToString() ?? "";
                    detalle.Estado = reader["Estado"].ToString() ?? "";
                    detalle.FechaInicio = reader["FechaInicio"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaInicio"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO"));
                    detalle.FechaFin = reader["FechaFin"] == DBNull.Value ? string.Empty : Convert.ToDateTime(reader["FechaFin"]).ToString("dd/MM/yy hh:mm tt", new CultureInfo("es-CO"));
                    detalle.DescripcionTest = reader["DescripcionTest"] == DBNull.Value ? "" : reader["DescripcionTest"].ToString();
                    detalle.Alcance = reader["Alcance"].ToString() ?? "";
                    detalle.TargetUsuarioId = reader["TargetUsuarioId"] == DBNull.Value ? "" : reader["TargetUsuarioId"].ToString();
                    detalle.ConteoPreguntas = reader["ConteoPreguntas"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ConteoPreguntas"]);
                }
                else
                {
                    // El SP hace THROW si no existe, así que en teoría nunca llegas aquí.
                    throw new Exception("No se encontró información para el UsuarioTest especificado.");
                }

                // RS2: items + opciones
                var itemsDict = new Dictionary<string, UsuarioTestItemDTO>();

                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var itemId = reader["UsuarioTestItemId"].ToString() ?? "";

                        if (!itemsDict.TryGetValue(itemId, out var item))
                        {
                            item = new UsuarioTestItemDTO
                            {
                                UsuarioTestItemId = itemId,
                                PreguntaId = reader["PreguntaId"].ToString() ?? "",
                                TextoSnapshot = reader["TextoSnapshot"].ToString() ?? "",
                                DimensionSnapshot = reader["DimensionSnapshot"].ToString() ?? "",
                                ScaleIdSnapshot = reader["ScaleIdSnapshot"].ToString() ?? "",
                                OrdenPregunta = reader["OrdenPregunta"] == DBNull.Value ? 0 : Convert.ToInt32(reader["OrdenPregunta"])
                            };

                            itemsDict[itemId] = item;
                        }

                        if (reader["UsuarioTestItemOpcionId"] != DBNull.Value)
                        {
                            var opcion = new UsuarioTestOpcionDTO
                            {
                                UsuarioTestItemOpcionId = reader["UsuarioTestItemOpcionId"].ToString() ?? "",
                                OrdenOpcion = reader["OrdenOpcion"] == DBNull.Value ? 0 : Convert.ToInt32(reader["OrdenOpcion"]),
                                TextoOpcion = reader["TextoOpcion"].ToString() ?? "",
                                ValorOpcion = reader["ValorOpcion"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ValorOpcion"])
                            };

                            item.Opciones.Add(opcion);
                        }
                    }
                }

                detalle.Items = itemsDict.Values.OrderBy(i => i.OrdenPregunta).ToList();

                return detalle;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> SetObservacionPerfilAsync(UpdateObservacionTestUsuarioDTO dto)
        {
            string message = string.Empty;
            try
            {
                using var connection = accessData.GetConnection();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "dbo.UsuarioPerfilSetObservacion";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioTestId", SqlDbType.VarChar, 50).Value = dto.UsuarioTestId;
                cmd.Parameters.Add("@Observacion", SqlDbType.NVarChar, -1).Value = dto.Observacion;

                await cmd.ExecuteNonQueryAsync();

                return message = "Observación registrada correctamente";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
