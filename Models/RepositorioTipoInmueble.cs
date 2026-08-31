using MySqlConnector;
using System.Data;

namespace Proyecto_Inmobiliaria.Models
{
    public class RepositorioTipoInmueble : RepositorioBase, IRepositorioTipoInmueble
    {
        public RepositorioTipoInmueble(IConfiguration configuration) : base(configuration)
        {
            
        }
        // ----- ALTA ----- 
        public int Alta (TipoInmueble t)
        {
            int res = -1; 
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO tipo_inmueble
                                (nombre)
                                VALUES (@nombre)";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@nombre", t.Nombre);

                    connection.Open();

                    cmd.ExecuteNonQuery();

                    res = (int)cmd.LastInsertedId;
                    t.IdTipoInmueble = res;

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
                string sql = @"DELETE FROM tipo_inmueble
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
        public int Modificacion(TipoInmueble t)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE tipo_inmueble 
					SET nombre=@nombre
                    WHERE id=@id";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@nombre", t.Nombre);
                    cmd.Parameters.AddWithValue("@id", t.IdTipoInmueble);

                    connection.Open();
                    res = cmd.ExecuteNonQuery();
                    connection.Close();
                }

            }
            return res;
        }

        // ---- OBTENER TODOS ----
        public IList<TipoInmueble> ObtenerTodos()
        {
            IList<TipoInmueble> tipos = new List<TipoInmueble>();


            string sql = @"SELECT id, nombre 
                        FROM tipo_inmueble;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {

                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            TipoInmueble t = new TipoInmueble
                            {
                                IdTipoInmueble = reader.GetInt32("id"),
                                Nombre = reader.GetString("nombre"),

                            };
                            tipos.Add(t);
                        }
                    }
                }
            }

            return tipos;
        }

        // ---- OBTENER POR ID ---- 

        public TipoInmueble? ObtenerPorId(int id)
        {
            TipoInmueble? t = null;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT id, nombre 
                    FROM tipo_inmueble
                    WHERE id=@id";

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            t = new TipoInmueble
                            {
                                IdTipoInmueble= reader.GetInt32("id"),
                                Nombre = reader.GetString("Nombre"),
                            };
                        }    
                    }
                }
            }
            return t;
        }
    }


}