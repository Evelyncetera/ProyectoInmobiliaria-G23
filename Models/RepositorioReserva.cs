using MySqlConnector;
using System.Data;

namespace Proyecto_Inmobiliaria.Models
{
    public class RepositorioReserva : RepositorioBase, IRepositorioReserva
    {
        public RepositorioReserva(IConfiguration configuration) : base(configuration)
        {

        }

        // ----- ALTA -----
        public int Alta(Reserva r)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO reserva
                                (id_inquilino, id_inmueble, fecha_desde, fecha_hasta,
                                monto_por_dia, anulada)
                                VALUES (@id_inquilino, @id_inmueble, @fecha_desde, @fecha_hasta,
                                        @monto_por_dia, @anulada)";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@id_inquilino", r.IdInquilino);
                    cmd.Parameters.AddWithValue("@id_inmueble", r.IdInmueble);
                    cmd.Parameters.AddWithValue("@fecha_desde", r.FechaDesde);
                    cmd.Parameters.AddWithValue("@fecha_hasta", r.FechaHasta);
                    cmd.Parameters.AddWithValue("@monto_por_dia", r.MontoPorDia);
                    cmd.Parameters.AddWithValue("@anulada", r.Anulada);

                    connection.Open();

                    cmd.ExecuteNonQuery();

                    res = (int)cmd.LastInsertedId;
                    r.IdReserva = res;

                    connection.Close();
                }
            }
            return res;
        }

        // ----- BAJA LÓGICA (anula la reserva, conserva el historial) -----
        public int Baja(int id)
        {
            int res = -1;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE reserva
                                SET anulada = 1
                                WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@id", id);

                    connection.Open();
                    res = cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        // ----- MODIFICACIÓN -----
        public int Modificacion(Reserva r)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE reserva
                                SET id_inquilino = @id_inquilino,
                                    id_inmueble = @id_inmueble,
                                    fecha_desde = @fecha_desde,
                                    fecha_hasta = @fecha_hasta,
                                    monto_por_dia = @monto_por_dia
                                WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@id_inquilino", r.IdInquilino);
                    cmd.Parameters.AddWithValue("@id_inmueble", r.IdInmueble);
                    cmd.Parameters.AddWithValue("@fecha_desde", r.FechaDesde);
                    cmd.Parameters.AddWithValue("@fecha_hasta", r.FechaHasta);
                    cmd.Parameters.AddWithValue("@monto_por_dia", r.MontoPorDia);
                    cmd.Parameters.AddWithValue("@id", r.IdReserva);

                    connection.Open();
                    res = cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return res;
        }

        // ----- VERIFICAR DISPONIBILIDAD DEL INMUEBLE -----
        public bool EstaDisponible(int idInmueble, DateTime desde, DateTime hasta, int? exceptoId = null)
        {
            bool disponible = true;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT COUNT(*)
                                FROM reserva
                                WHERE anulada = 0
                                  AND id_inmueble = @id_inmueble
                                  AND fecha_desde <= @hasta
                                  AND fecha_hasta >= @desde
                                  AND (@exceptoId IS NULL OR id <> @exceptoId)";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@id_inmueble", idInmueble);
                    cmd.Parameters.AddWithValue("@desde", desde);
                    cmd.Parameters.AddWithValue("@hasta", hasta);
                    cmd.Parameters.AddWithValue("@exceptoId", exceptoId.HasValue ? exceptoId.Value : DBNull.Value);

                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int ocupadas = reader.GetInt32(0);
                            disponible = ocupadas == 0;
                        }
                    }
                    connection.Close();
                }
            }
            return disponible;
        }

        // ----- OBTENER TODOS -----
        public IList<Reserva> ObtenerTodos()
        {
            IList<Reserva> reservas = new List<Reserva>();

            string sql = @"SELECT r.id, r.id_inquilino, r.id_inmueble,
                            r.fecha_desde, r.fecha_hasta, r.monto_por_dia, r.anulada,
                            i.nombre, i.apellido, i.dni, inm.direccion
                        FROM reserva r
                        INNER JOIN inquilino i ON i.id = r.id_inquilino
                        INNER JOIN inmueble inm ON inm.id = r.id_inmueble
                        ORDER BY r.fecha_desde DESC;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservas.Add(LeerReserva(reader));
                        }
                    }
                }
            }
            return reservas;
        }

        // ----- OBTENER POR ID -----
        public Reserva? ObtenerPorId(int id)
        {
            Reserva? r = null;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT r.id, r.id_inquilino, r.id_inmueble,
                                r.fecha_desde, r.fecha_hasta, r.monto_por_dia, r.anulada,
                                i.nombre, i.apellido, i.dni, inm.direccion
                            FROM reserva r
                            INNER JOIN inquilino i ON i.id = r.id_inquilino
                            INNER JOIN inmueble inm ON inm.id = r.id_inmueble
                            WHERE r.id = @id;";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            r = LeerReserva(reader);
                        }
                    }
                }
            }
            return r;
        }

        // ----- RESERVAS VIGENTES (hoy dentro de [desde, hasta]) -----
        public IList<Reserva> ObtenerVigentes()
        {
            IList<Reserva> reservas = new List<Reserva>();

            string sql = @"SELECT r.id, r.id_inquilino, r.id_inmueble,
                            r.fecha_desde, r.fecha_hasta, r.monto_por_dia, r.anulada,
                            i.nombre, i.apellido, i.dni, inm.direccion
                        FROM reserva r
                        INNER JOIN inquilino i ON i.id = r.id_inquilino
                        INNER JOIN inmueble inm ON inm.id = r.id_inmueble
                        WHERE r.anulada = 0
                          AND r.fecha_desde <= CURDATE()
                          AND r.fecha_hasta >= CURDATE()
                        ORDER BY r.fecha_hasta ASC;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservas.Add(LeerReserva(reader));
                        }
                    }
                }
            }
            return reservas;
        }

        // ----- RESERVAS QUE TERMINAN EN X DÍAS -----
        public IList<Reserva> ObtenerPorTerminar(int dias)
        {
            IList<Reserva> reservas = new List<Reserva>();

            string sql = @"SELECT r.id, r.id_inquilino, r.id_inmueble,
                            r.fecha_desde, r.fecha_hasta, r.monto_por_dia, r.anulada,
                            i.nombre, i.apellido, i.dni, inm.direccion
                        FROM reserva r
                        INNER JOIN inquilino i ON i.id = r.id_inquilino
                        INNER JOIN inmueble inm ON inm.id = r.id_inmueble
                        WHERE r.anulada = 0
                          AND r.fecha_hasta BETWEEN CURDATE()
                              AND DATE_ADD(CURDATE(), INTERVAL @dias DAY)
                        ORDER BY r.fecha_hasta ASC;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@dias", dias);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservas.Add(LeerReserva(reader));
                        }
                    }
                }
            }
            return reservas;
        }

        // ----- INMUEBLES MÁS RESERVADOS EN LOS ÚLTIMOS 365 DÍAS -----
        public IList<InmuebleConReservas> ObtenerMasReservados()
        {
            IList<InmuebleConReservas> inmuebles = new List<InmuebleConReservas>();

            string sql = @"SELECT inm.id, inm.direccion, COUNT(r.id) AS cantidad
                        FROM inmueble inm
                        INNER JOIN reserva r ON r.id_inmueble = inm.id AND r.anulada = 0
                        WHERE r.fecha_desde >= DATE_SUB(CURDATE(), INTERVAL 365 DAY)
                        GROUP BY inm.id, inm.direccion
                        ORDER BY cantidad DESC;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InmuebleConReservas inm = new InmuebleConReservas
                            {
                                IdInmueble = reader.GetInt32("id"),
                                Direccion = reader.GetString("direccion"),
                                CantidadReservas = reader.GetInt32("cantidad")
                            };
                            inmuebles.Add(inm);
                        }
                    }
                }
            }
            return inmuebles;
        }

        // ----- INMUEBLES SIN RESERVAS EN LOS ÚLTIMOS X DÍAS -----
        public IList<Inmueble> ObtenerInmueblesSinReservas(int dias)
        {
            IList<Inmueble> inmuebles = new List<Inmueble>();

            string sql = @"SELECT inm.id, inm.id_propietario, inm.id_tipo_inmueble,
                            inm.direccion, inm.cupo, inm.latitud, inm.longitud,
                            inm.precio_por_dia, inm.porcentaje_reserva, inm.disponible
                        FROM inmueble inm
                        WHERE NOT EXISTS (
                            SELECT 1 FROM reserva r
                            WHERE r.id_inmueble = inm.id
                              AND r.anulada = 0
                              AND r.fecha_hasta >= DATE_SUB(CURDATE(), INTERVAL @dias DAY)
                              AND r.fecha_desde <= CURDATE()
                        );";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@dias", dias);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Inmueble i = new Inmueble
                            {
                                IdInmueble = reader.GetInt32("id"),
                                IdPropietario = reader.GetInt32("id_propietario"),
                                IdTipoInmueble = reader.GetInt32("id_tipo_inmueble"),
                                Direccion = reader.GetString("direccion"),
                                Cupo = reader.GetInt32("cupo"),
                                Latitud = reader.GetDecimal("latitud"),
                                Longitud = reader.GetDecimal("longitud"),
                                PrecioPorDia = reader.GetDecimal("precio_por_dia"),
                                PorcentajeReserva = reader.GetDecimal("porcentaje_reserva"),
                                Disponible = reader.GetBoolean("disponible")
                            };
                            inmuebles.Add(i);
                        }
                    }
                }
            }
            return inmuebles;
        }

        // ----- INMUEBLES LIBRES ENTRE DOS FECHAS -----
        public IList<Inmueble> ObtenerInmueblesDisponibles(DateTime desde, DateTime hasta)
        {
            IList<Inmueble> inmuebles = new List<Inmueble>();

            string sql = @"SELECT inm.id, inm.id_propietario, inm.id_tipo_inmueble,
                            inm.direccion, inm.cupo, inm.latitud, inm.longitud,
                            inm.precio_por_dia, inm.porcentaje_reserva, inm.disponible
                        FROM inmueble inm
                        WHERE NOT EXISTS (
                            SELECT 1 FROM reserva r
                            WHERE r.id_inmueble = inm.id
                              AND r.anulada = 0
                              AND r.fecha_desde <= @hasta
                              AND r.fecha_hasta >= @desde
                        );";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@desde", desde);
                    cmd.Parameters.AddWithValue("@hasta", hasta);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Inmueble i = new Inmueble
                            {
                                IdInmueble = reader.GetInt32("id"),
                                IdPropietario = reader.GetInt32("id_propietario"),
                                IdTipoInmueble = reader.GetInt32("id_tipo_inmueble"),
                                Direccion = reader.GetString("direccion"),
                                Cupo = reader.GetInt32("cupo"),
                                Latitud = reader.GetDecimal("latitud"),
                                Longitud = reader.GetDecimal("longitud"),
                                PrecioPorDia = reader.GetDecimal("precio_por_dia"),
                                PorcentajeReserva = reader.GetDecimal("porcentaje_reserva"),
                                Disponible = reader.GetBoolean("disponible")
                            };
                            inmuebles.Add(i);
                        }
                    }
                }
            }
            return inmuebles;
        }

        // ----- Helper: mapea una fila a Reserva (con datos enriquecidos) -----
        private static Reserva LeerReserva(MySqlDataReader reader)
        {
            return new Reserva
            {
                IdReserva = reader.GetInt32("id"),
                IdInquilino = reader.GetInt32("id_inquilino"),
                IdInmueble = reader.GetInt32("id_inmueble"),
                FechaDesde = reader.GetDateTime("fecha_desde"),
                FechaHasta = reader.GetDateTime("fecha_hasta"),
                MontoPorDia = reader.GetDecimal("monto_por_dia"),
                Anulada = reader.GetBoolean("anulada"),
                NombreInquilino = reader.GetString("nombre"),
                ApellidoInquilino = reader.GetString("apellido"),
                DniInquilino = reader.GetString("dni"),
                DireccionInmueble = reader.GetString("direccion")
            };
        }
    }
}