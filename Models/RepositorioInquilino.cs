using MySqlConnector;
using System.Data;

namespace Proyecto_Inmobiliaria.Models
{
    public class RepositorioInquilino : RepositorioBase, IRepositorioInquilino
    {
        public RepositorioInquilino(IConfiguration configuration) : base(configuration)
        {

        }
        public int Alta(Inquilino i)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO inquilino 
					(dni, nombre, apellido, telefono, email)
					VALUES (@dni, @nombre, @apellido, @telefono, @email)";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@dni", i.Dni);
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@telefono", i.Telefono);
                    command.Parameters.AddWithValue("@email", i.Email);
                    connection.Open();
                    command.ExecuteNonQuery();
                    res = (int)command.LastInsertedId;
                    i.IdInquilino = res;
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
                string sql = @"DELETE FROM inquilino WHERE id = @id";

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

        public int Modificacion(Inquilino i)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE inquilino 
					SET dni=@dni, nombre=@nombre, apellido=@apellido, telefono=@telefono, email=@email 
                    WHERE id = @id";

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@dni", i.Dni);
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@telefono", i.Telefono);
                    command.Parameters.AddWithValue("@email", i.Email);
                    command.Parameters.AddWithValue("@id", i.IdInquilino);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                    connection.Close();
                }

            }

            return res;
        }


        public IList<Inquilino> ObtenerTodos()
        {
            IList<Inquilino> inquilinos = new List<Inquilino>();


            string sql = @"SELECT id, dni, nombre, apellido, telefono, email 
                        FROM inquilino;";


            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    connection.Open();


                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            Inquilino i = new Inquilino
                            {

                                IdInquilino = reader.GetInt32("id"), 
                                Dni = reader.GetString("dni"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email")
                            };

                            inquilinos.Add(i);
                        }
                    }
                }
            }

            return inquilinos;
        }


        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? i = null;


            string sql = @"SELECT id, dni, nombre, apellido, telefono, email 
                            FROM inquilino 
                            WHERE id = @id;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {

                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {

                            i = new Inquilino
                            {
                                IdInquilino = reader.GetInt32("id"),
                                Dni = reader.GetString("dni"),
                                Nombre = reader.GetString("nombre"),
                                Apellido = reader.GetString("apellido"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email")
                            };
                        }
                    }
                }
            }

            return i;
        }


    }
}