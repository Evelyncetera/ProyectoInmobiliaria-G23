using MySqlConnector;

namespace Proyecto_Inmobiliaria.Models
{
    public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
    {
        public RepositorioUsuario(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(Usuario u)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO usuario (nombre, apellido, email, clave, avatarUrl, rol, estado) 
                               VALUES (@nombre, @apellido, @email, @clave, @avatarUrl, @rol, @estado);
                               SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", u.Nombre);
                    command.Parameters.AddWithValue("@apellido", u.Apellido);
                    command.Parameters.AddWithValue("@email", u.Email);
                    command.Parameters.AddWithValue("@clave", u.Clave);
                    command.Parameters.AddWithValue("@avatarUrl", (object?)u.AvatarUrl ?? DBNull.Value);
                    command.Parameters.AddWithValue("@rol", u.Rol);
                    command.Parameters.AddWithValue("@estado", u.Estado);

                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    u.IdUsuario = res;
                }
            }
            return res;
        }

        public int Baja(int id)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                // Baja lógica cambiando estado a 0
                string sql = @"UPDATE usuario SET estado = 0 WHERE idUsuario = @id;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int Modificacion(Usuario u)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE usuario 
                               SET nombre = @nombre, apellido = @apellido, email = @email, rol = @rol 
                               WHERE idUsuario = @idUsuario;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", u.Nombre);
                    command.Parameters.AddWithValue("@apellido", u.Apellido);
                    command.Parameters.AddWithValue("@email", u.Email);
                    command.Parameters.AddWithValue("@rol", u.Rol);
                    command.Parameters.AddWithValue("@idUsuario", u.IdUsuario);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public IList<Usuario> ObtenerTodos()
        {
            var res = new List<Usuario>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT idUsuario, nombre, apellido, email, clave, avatarUrl, rol, estado 
                               FROM usuario WHERE estado = 1;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            res.Add(MapearUsuario(reader));
                        }
                    }
                }
            }
            return res;
        }

        public Usuario? ObtenerPorId(int id)
        {
            Usuario? u = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT idUsuario, nombre, apellido, email, clave, avatarUrl, rol, estado 
                               FROM usuario WHERE idUsuario = @id;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            u = MapearUsuario(reader);
                        }
                    }
                }
            }
            return u;
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            Usuario? u = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT idUsuario, nombre, apellido, email, clave, avatarUrl, rol, estado 
                               FROM usuario WHERE email = @email AND estado = 1;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            u = MapearUsuario(reader);
                        }
                    }
                }
            }
            return u;
        }

        public int CambiarClave(int idUsuario, string claveNueva)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE usuario SET clave = @clave WHERE idUsuario = @idUsuario;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@clave", claveNueva);
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int ActualizarAvatar(int idUsuario, string avatarUrl)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE usuario SET avatarUrl = @avatarUrl WHERE idUsuario = @idUsuario;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@avatarUrl", avatarUrl);
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int ActualizarPerfil(int idUsuario, string nombre, string apellido, string email)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = @"UPDATE usuario
                                     SET nombre = @nombre, apellido = @apellido, email = @email
                                     WHERE idUsuario = @idUsuario AND estado = 1;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", nombre);
                    command.Parameters.AddWithValue("@apellido", apellido);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@idUsuario", idUsuario);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        
        private static Usuario MapearUsuario(MySqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32(nameof(Usuario.IdUsuario)),
                Nombre = reader.GetString(nameof(Usuario.Nombre)),
                Apellido = reader.GetString(nameof(Usuario.Apellido)),
                Email = reader.GetString(nameof(Usuario.Email)),
                Clave = reader.GetString(nameof(Usuario.Clave)),
                AvatarUrl = reader.IsDBNull(reader.GetOrdinal(nameof(Usuario.AvatarUrl))) 
                    ? null 
                    : reader.GetString(nameof(Usuario.AvatarUrl)),
                Rol = reader.GetString(nameof(Usuario.Rol)),
                Estado = reader.GetInt32(nameof(Usuario.Estado))
            };
        }
    }
}
