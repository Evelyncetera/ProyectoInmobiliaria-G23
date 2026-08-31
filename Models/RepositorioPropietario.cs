using MySqlConnector;
using System.Data;

namespace Proyecto_Inmobiliaria.Models
{
    public class RepositorioPropietario : RepositorioBase, IRepositorioPropietario
    {
        public RepositorioPropietario(IConfiguration configuration) : base(configuration)
        {

        }
        public int Alta(Propietario p)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO propietario 
					(dni, nombre, apellido, telefono, email)
					VALUES (@dni, @nombre, @apellido, @telefono, @email)";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@dni", p.Dni);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@telefono", p.Telefono);
                    command.Parameters.AddWithValue("@email", p.Email);
                    connection.Open();
                    command.ExecuteNonQuery();
                    res = (int)command.LastInsertedId;
                    p.IdPropietario = res;
                    connection.Close();
                }

            }
            return res;
        }

        public int Baja(int id)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"DELETE FROM propietario WHERE id = @id";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }

            }
            return res;
        }

        public int Modificacion(Propietario p)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE propietario 
					SET dni=@dni, nombre=@nombre, apellido=@apellido, telefono=@telefono, email=@email 
                    WHERE id=@id";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@dni", p.Dni);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@telefono", p.Telefono);
                    command.Parameters.AddWithValue("@email", p.Email);
                    command.Parameters.AddWithValue("@id", p.IdPropietario);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }

            }

            return res;

        }

        public Propietario? ObtenerPorId(int id)
        {
            Propietario? p = null;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT id, dni, nombre, apellido, telefono, email 
                    FROM propietario
                    WHERE id=@id";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        p = new Propietario
                        {
                            IdPropietario = reader.GetInt32("id"),
                            Nombre = reader.GetString("Nombre"),
                            Apellido = reader.GetString("Apellido"),
                            Dni = reader.GetString("Dni"),
                            Telefono = reader.GetString("Telefono"),
                            Email = reader.GetString("Email"),
                        };
                    }
                    connection.Close();
                }
            }
            return p;
        }


        public IList<Propietario> ObtenerTodos()
        {
            IList<Propietario> propietarios = new List<Propietario>();


            string sql = @"SELECT id, dni, nombre, apellido, telefono, email 
                        FROM propietario;";


            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {

                    connection.Open();


                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            Propietario p = new Propietario
                            {
                                IdPropietario = reader.GetInt32("id"),
                                Dni = reader.GetString("dni"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email")
                            };
                            propietarios.Add(p);
                        }
                    }
                }
            }

            return propietarios;
        }
    }
}