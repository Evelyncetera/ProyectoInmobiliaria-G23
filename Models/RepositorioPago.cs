using MySqlConnector;
using System.Data;

namespace Proyecto_Inmobiliaria.Models
{
    public class RepositorioPago : RepositorioBase, IRepositorioPago
    {
        public RepositorioPago(IConfiguration configuration) : base(configuration)
        {

        }

        public int Alta(Pago p, int idUsuarioCreador)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO pago (id_reserva, concepto, fecha_pago, importe, anulada, id_usuario_creador, fecha_creacion)
                            VALUES (@id_reserva, @concepto, CURDATE(), @importe, @anulada, @id_usuario_creador, CURRENT_TIMESTAMP)";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@id_reserva", p.IdReserva);
                    cmd.Parameters.AddWithValue("@concepto", p.Concepto);
                    cmd.Parameters.AddWithValue("@importe", p.Importe);
                    cmd.Parameters.AddWithValue("@anulada", p.Anulada);
                    cmd.Parameters.AddWithValue("@id_usuario_creador", idUsuarioCreador);

                    connection.Open();

                    cmd.ExecuteNonQuery();

                    res = (int)cmd.LastInsertedId;
                    p.IdPago = res;

                    connection.Close();
                }
            }
            return res;
        }

        // ----- BAJA LÓGICA (anula el pago de la reserva, conserva el historial) -----
        public int Baja(int id, int idUsuarioAnulador)
        {
            int res = -1;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {

                string sql = @"UPDATE pago
                                SET anulada = 1,
                                    id_usuario_anulador = @id_usuario_anulador,
                                    fecha_anulacion = CURRENT_TIMESTAMP
                                WHERE id = @id AND anulada = 0";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@id_usuario_anulador", idUsuarioAnulador);

                    connection.Open();
                    res = cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public int Modificacion(Pago p)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {

                string sql = @"UPDATE pago
                                SET concepto = @concepto
                                WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@concepto", p.Concepto);
                    cmd.Parameters.AddWithValue("@id", p.IdPago);

                    connection.Open();
                    res = cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        public IList<Pago> ObtenerTodos()
        {
            IList<Pago> pagos = new List<Pago>();

            string sql = @"SELECT p.id, p.id_reserva, p.concepto, p.fecha_pago, p.importe, p.anulada
                            FROM pago p
                            INNER JOIN reserva r ON r.id = p.id_reserva
                            ORDER BY p.fecha_pago DESC;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pagos.Add(LeerPagos(reader));
                        }
                    }
                }
            }
            return pagos;
        }

        public Pago? ObtenerPorId(int id)
        {
            Pago? p = null;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {

                string sql = @"SELECT p.id, p.id_reserva, p.concepto, p.fecha_pago, p.importe, p.anulada, 
                                p.id_usuario_creador, p.fecha_creacion,
                                p.id_usuario_anulador, p.fecha_anulacion,
                                CONCAT(uc.nombre, ' ', uc.apellido) AS nombre_usuario_creador,
                                CONCAT(ua.nombre, ' ', ua.apellido) AS nombre_usuario_anulador
                             FROM pago p
                             LEFT JOIN usuario uc ON uc.idUsuario = p.id_usuario_creador
                             LEFT JOIN usuario ua ON ua.idUsuario = p.id_usuario_anulador
                             WHERE p.id = @id;";
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            p = LeerPagos(reader);
                            p.IdUsuarioCreador = reader.IsDBNull(reader.GetOrdinal("id_usuario_creador"))
                                ? null
                                : reader.GetInt32("id_usuario_creador");
                            p.NombreUsuarioCreador = reader.IsDBNull(reader.GetOrdinal("nombre_usuario_creador"))
                                ? null
                                : reader.GetString("nombre_usuario_creador");
                            p.FechaCreacion = reader.IsDBNull(reader.GetOrdinal("fecha_creacion"))
                                ? default
                                : reader.GetDateTime("fecha_creacion");
                            p.IdUsuarioAnulador = reader.IsDBNull(reader.GetOrdinal("id_usuario_anulador"))
                                ? null
                                : reader.GetInt32("id_usuario_anulador");
                            p.NombreUsuarioAnulador = reader.IsDBNull(reader.GetOrdinal("nombre_usuario_anulador"))
                                ? null
                                : reader.GetString("nombre_usuario_anulador");
                            p.FechaAnulacion = reader.IsDBNull(reader.GetOrdinal("fecha_anulacion"))
                                ? null
                                : reader.GetDateTime("fecha_anulacion");
                        }
                    }
                }
            }
            return p;
        }

        public IList<Pago> ObtenerPorReserva(int idReserva)
        {
            IList<Pago> pagos = new List<Pago>();

            string sql = @"SELECT p.id, p.id_reserva, p.concepto, p.fecha_pago,
                            p.importe, p.anulada,
                            p.id_usuario_creador, p.fecha_creacion,
                            p.id_usuario_anulador, p.fecha_anulacion,
                            CONCAT(uc.nombre, ' ', uc.apellido) AS nombre_usuario_creador,
                            CONCAT(ua.nombre, ' ', ua.apellido) AS nombre_usuario_anulador
                        FROM pago p
                        LEFT JOIN usuario uc ON uc.idUsuario = p.id_usuario_creador
                        LEFT JOIN usuario ua ON ua.idUsuario = p.id_usuario_anulador
                        WHERE p.id_reserva = @id_reserva
                        ORDER BY p.fecha_pago DESC;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@id_reserva", idReserva);

                    connection.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pagos.Add(new Pago
                            {
                                IdPago = reader.GetInt32("id"),
                                IdReserva = reader.GetInt32("id_reserva"),
                                Concepto = reader.GetString("concepto"),
                                FechaPago = reader.GetDateTime("fecha_pago"),
                                Importe = reader.GetDecimal("importe"),
                                Anulada = reader.GetBoolean("anulada"),
                                IdUsuarioCreador = reader.IsDBNull(reader.GetOrdinal("id_usuario_creador"))
                                    ? null : reader.GetInt32("id_usuario_creador"),
                                NombreUsuarioCreador = reader.IsDBNull(reader.GetOrdinal("nombre_usuario_creador"))
                                    ? null : reader.GetString("nombre_usuario_creador"),
                                IdUsuarioAnulador = reader.IsDBNull(reader.GetOrdinal("id_usuario_anulador"))
                                    ? null : reader.GetInt32("id_usuario_anulador"),
                                NombreUsuarioAnulador = reader.IsDBNull(reader.GetOrdinal("nombre_usuario_anulador"))
                                    ? null : reader.GetString("nombre_usuario_anulador"),
                            });
                        }
                    }
                    return pagos;
                }
            }
        }

        public int RegistrarPenalizacionYTerminarReserva(Pago pago, DateTime fechaTerminacion,
            int idUsuario)
        {
            using MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            using MySqlTransaction transaction = connection.BeginTransaction();

            try
            {
                string sqlPago = @"INSERT INTO pago
                                    (id_reserva, concepto, fecha_pago, importe,
                                    anulada, id_usuario_creador, fecha_creacion)
                                VALUES
                                    (@id_reserva, @concepto, @fecha_pago,
                                    @importe, 0, @id_usuario_creador,
                                    CURRENT_TIMESTAMP);";

                using MySqlCommand cmdPago = new MySqlCommand(sqlPago, connection, transaction);

                cmdPago.Parameters.AddWithValue("@id_reserva", pago.IdReserva);

                cmdPago.Parameters.AddWithValue("@concepto", pago.Concepto);

                cmdPago.Parameters.AddWithValue("@fecha_pago", pago.FechaPago.Date);

                cmdPago.Parameters.AddWithValue("@importe", pago.Importe);

                cmdPago.Parameters.AddWithValue("@id_usuario_creador", idUsuario);

                cmdPago.ExecuteNonQuery();

                int idPago = (int)cmdPago.LastInsertedId;

                string sqlReserva = @"UPDATE reserva
                                    SET fecha_terminacion = @fecha_terminacion,
                                        id_usuario_terminador = @id_usuario_terminador
                                    WHERE id = @id_reserva
                                        AND anulada = 0
                                        AND fecha_terminacion IS NULL
                                        AND @fecha_terminacion >= fecha_desde
                                        AND @fecha_terminacion < fecha_hasta;";

                using MySqlCommand cmdReserva =
                    new MySqlCommand(sqlReserva, connection, transaction);

                cmdReserva.Parameters.AddWithValue("@fecha_terminacion", fechaTerminacion.Date);

                cmdReserva.Parameters.AddWithValue("@id_usuario_terminador", idUsuario);

                cmdReserva.Parameters.AddWithValue("@id_reserva", pago.IdReserva);

                int filasReserva = cmdReserva.ExecuteNonQuery();

                if (filasReserva != 1)
                {
                    throw new InvalidOperationException("La reserva no pudo terminarse anticipadamente.");
                }

                transaction.Commit();

                pago.IdPago = idPago;

                return idPago;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        private static Pago LeerPagos(MySqlDataReader reader)
        {
            return new Pago
            {
                IdPago = reader.GetInt32("id"),
                IdReserva = reader.GetInt32("id_reserva"),
                Concepto = reader.GetString("concepto"),
                FechaPago = reader.GetDateTime("fecha_pago"),
                Importe = reader.GetDecimal("importe"),
                Anulada = reader.GetBoolean("anulada"),
            };
        }


    }

}