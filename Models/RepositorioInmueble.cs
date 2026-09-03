using MySqlConnector;
using System.Data;

namespace Proyecto_Inmobiliaria.Models
{
    public class RepositorioInmueble : RepositorioBase, IRepositorioInmueble
    {
        public RepositorioInmueble(IConfiguration configuration) : base(configuration)
        {
            
        }
        // ----- ALTA ----- 
        public int Alta (Inmueble i)
        {
            int res = -1; 
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO inmueble
                                (id_propietario, id_tipo_inmueble, direccion, cupo,
                                latitud, longitud, precio_por_dia, porcentaje_reserva,
                                disponible)

                                VALUES (@id_propietario, @id_tipo_inmueble, @direccion, @cupo,
                                        @latitud, @longitud, @precio_por_dia, @porcentaje_reserva,
                                        @disponible)";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@id_propietario", i.IdPropietario);
                    cmd.Parameters.AddWithValue("@id_tipo_inmueble", i.IdTipoInmueble);
                    cmd.Parameters.AddWithValue("@direccion", i.Direccion);
                    cmd.Parameters.AddWithValue("@cupo", i.Cupo);
                    cmd.Parameters.AddWithValue("@latitud", i.Latitud);
                    cmd.Parameters.AddWithValue("@longitud", i.Longitud);
                    cmd.Parameters.AddWithValue("@precio_por_dia", i.PrecioPorDia);
                    cmd.Parameters.AddWithValue("@porcentaje_reserva", i.PorcentajeReserva);
                    cmd.Parameters.AddWithValue("@disponible", i.Disponible);
                    
                    connection.Open();

                    cmd.ExecuteNonQuery();

                    res = (int)cmd.LastInsertedId;
                    i.IdTipoInmueble = res;

                    connection.Close();
                }
            }
            return res;
        }

        // ---- BAJA ----
        public int Baja (int id)
        {
            int res = -1;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"DELETE FROM inmueble
                                WHERE id = @id";
                
                using (MySqlCommand cmd = new MySqlCommand (sql, connection))
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

        // ---- MODIFICACIÓN ----
        public int Modificacion(Inmueble i)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE inmueble 
					SET id_propietario = @id_propietario,
                        id_tipo_inmueble = @id_tipo_inmueble,
                        direccion = @direccion,
                        cupo = @cupo,
                        latitud = @latitud,
                        longitud = @longitud,
                        precio_por_dia = @precio_por_dia,
                        porcentaje_reserva = @porcentaje_reserva,
                        disponible = @disponible
                    WHERE id=@id";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@id_propietario", i.IdPropietario);
                    cmd.Parameters.AddWithValue("@id_tipo_inmueble", i.IdTipoInmueble);
                    cmd.Parameters.AddWithValue("@direccion", i.Direccion);
                    cmd.Parameters.AddWithValue("@cupo", i.Cupo);
                    cmd.Parameters.AddWithValue("@latitud", i.Latitud);
                    cmd.Parameters.AddWithValue("@longitud", i.Longitud);
                    cmd.Parameters.AddWithValue("@precio_por_dia", i.PrecioPorDia);
                    cmd.Parameters.AddWithValue("@porcentaje_reserva", i.PorcentajeReserva);
                    cmd.Parameters.AddWithValue("@disponible", i.Disponible);
                    cmd.Parameters.AddWithValue("@id", i.IdInmueble);
                    connection.Open();
                    res = cmd.ExecuteNonQuery();
                    connection.Close();
                }

            }
            return res;
        }

        // ---- OBTENER TODOS ----
        public IList<Inmueble> ObtenerTodos()
        {
            IList<Inmueble> inmuebles = new List<Inmueble>();


            string sql = @"SELECT id, id_propietario, id_tipo_inmueble,
                            direccion, cupo, latitud, longitud,
                            precio_por_dia, porcentaje_reserva, disponible
                        FROM inmueble;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {

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

        // ---- OBTENER POR ID ---- 

        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? i = null;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT id, id_propietario, id_tipo_inmueble,
                                direccion, cupo, latitud, longitud,
                                precio_por_dia, porcentaje_reserva, disponible
                            FROM inmueble
                            WHERE id = @id;";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            i = new Inmueble
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
                        }    
                    }
                }
            }
            return i;
        }
    }


}