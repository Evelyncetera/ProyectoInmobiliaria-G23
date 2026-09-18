using MySqlConnector;

namespace Proyecto_Inmobiliaria.Models
{
    public class RepositorioInmueble : RepositorioBase, IRepositorioInmueble
    {
        public RepositorioInmueble(IConfiguration configuration) : base(configuration)
        {
        }

        private const string Seleccion = """
            SELECT i.*, CONCAT(p.apellido, ', ', p.nombre) AS propietario, t.nombre AS tipo,
                (SELECT im.id FROM inmueble_imagen im
                WHERE im.id_inmueble = i.id AND im.estado = 1 AND im.es_portada = 1 LIMIT 1) AS portada
            FROM inmueble i
            INNER JOIN propietario p ON p.id = i.id_propietario
            INNER JOIN tipo_inmueble t ON t.id = i.id_tipo_inmueble
            """;

        private static Inmueble Leer(MySqlDataReader r) => new()
        {
            IdInmueble = r.GetInt32("id"), IdPropietario = r.GetInt32("id_propietario"),
            IdTipoInmueble = r.GetInt32("id_tipo_inmueble"), Direccion = r.GetString("direccion"),
            Cupo = r.GetInt32("cupo"), Latitud = r.GetDecimal("latitud"), Longitud = r.GetDecimal("longitud"),
            PrecioPorDia = r.GetDecimal("precio_por_dia"), PorcentajeReserva = r.GetDecimal("porcentaje_reserva"),
            Disponible = r.GetBoolean("disponible"), Estado = r.GetBoolean("estado"),
            NombrePropietario = r.GetString("propietario"), NombreTipo = r.GetString("tipo"),
            IdPortada = r.IsDBNull(r.GetOrdinal("portada")) ? null : r.GetInt32("portada")
        };

        public IList<Inmueble> ObtenerTodos()
        {
            // Selector de nuevas reservas: no ofrecer suspendidos ni bajas.
            using var cn = new MySqlConnection(connectionString);
            using var cmd = new MySqlCommand(Seleccion + " WHERE i.estado = 1 AND i.disponible = 1 ORDER BY i.direccion, i.id", cn);
            cn.Open();
            using var reader = cmd.ExecuteReader();
            var items = new List<Inmueble>();
            while (reader.Read()) items.Add(Leer(reader));
            return items;
        }

        public Inmueble? ObtenerPorId(int id)
        {
            using var cn = new MySqlConnection(connectionString);
            using var cmd = new MySqlCommand(Seleccion + " WHERE i.id = @id", cn);
            cmd.Parameters.AddWithValue("@id", id);
            cn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Leer(reader) : null;
        }

        public InmueblesListadoViewModel ObtenerPagina(InmueblesListadoViewModel filtro)
        {
            filtro.TamanoPagina = filtro.TamanoPagina is 10 or 20 or 50 ? filtro.TamanoPagina : 10;
            filtro.Estado = filtro.Estado is "todos" or "bajas" ? filtro.Estado : "activos";
            filtro.Buscar = filtro.Buscar?.Trim();
            const string where = """
                    WHERE (@buscar = '' OR i.direccion LIKE @patron OR CONCAT(p.nombre, ' ', p.apellido) LIKE @patron)
                    AND (@propietario IS NULL OR i.id_propietario = @propietario)
                    AND (@disponible IS NULL OR i.disponible = @disponible)
                    AND (@estado IS NULL OR i.estado = @estado)
                """;
            using var cn = new MySqlConnection(connectionString);
            cn.Open();
            using var tx = cn.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM inmueble i INNER JOIN propietario p ON p.id = i.id_propietario" + where, cn, tx);
            cmd.Parameters.AddWithValue("@buscar", filtro.Buscar ?? "");
            cmd.Parameters.AddWithValue("@patron", "%" + (filtro.Buscar ?? "") + "%");
            cmd.Parameters.AddWithValue("@propietario", (object?)filtro.Propietario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@disponible", (object?)filtro.Disponible ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@estado", filtro.Estado == "todos" ? DBNull.Value : (object)(filtro.Estado == "activos"));
            filtro.Total = Convert.ToInt32(cmd.ExecuteScalar());
            filtro.Pagina = Math.Clamp(filtro.Pagina, 1, filtro.TotalPaginas);
            cmd.CommandText = Seleccion + where + " ORDER BY i.id DESC LIMIT @limite OFFSET @offset";
            cmd.Parameters.AddWithValue("@limite", filtro.TamanoPagina);
            cmd.Parameters.AddWithValue("@offset", (filtro.Pagina - 1) * filtro.TamanoPagina);
            filtro.Items = new List<Inmueble>();
            using (var reader = cmd.ExecuteReader())
                while (reader.Read()) filtro.Items.Add(Leer(reader));
            tx.Commit();
            return filtro;
        }

        public int Alta(Inmueble i) => Guardar(i, Array.Empty<string>(), true);
        public int Modificacion(Inmueble i) => Guardar(i, Array.Empty<string>(), false);

        public int Guardar(Inmueble i, IReadOnlyList<string> imagenes, bool nuevo)
        {
            using var cn = new MySqlConnection(connectionString);
            cn.Open();
            using var tx = cn.BeginTransaction();
            if (!nuevo && !BloquearActivo(cn, tx, i.IdInmueble))
                throw new InvalidOperationException("El inmueble no existe o está dado de baja.");
            using var cmd = new MySqlCommand(nuevo ? """
                INSERT INTO inmueble (id_propietario, id_tipo_inmueble, direccion, cupo, latitud, longitud,
                    precio_por_dia, porcentaje_reserva, disponible, estado)
                VALUES (@propietario, @tipo, @direccion, @cupo, @latitud, @longitud, @precio, @porcentaje, @disponible, 1)
                """ : """
                UPDATE inmueble SET id_propietario = @propietario, id_tipo_inmueble = @tipo,
                    direccion = @direccion, cupo = @cupo, latitud = @latitud, longitud = @longitud,
                    precio_por_dia = @precio, porcentaje_reserva = @porcentaje, disponible = @disponible
                WHERE id = @id AND estado = 1
                """, cn, tx);
            cmd.Parameters.AddWithValue("@id", i.IdInmueble);
            cmd.Parameters.AddWithValue("@propietario", i.IdPropietario);
            cmd.Parameters.AddWithValue("@tipo", i.IdTipoInmueble);
            cmd.Parameters.AddWithValue("@direccion", i.Direccion);
            cmd.Parameters.AddWithValue("@cupo", i.Cupo);
            cmd.Parameters.AddWithValue("@latitud", i.Latitud);
            cmd.Parameters.AddWithValue("@longitud", i.Longitud);
            cmd.Parameters.AddWithValue("@precio", i.PrecioPorDia);
            cmd.Parameters.AddWithValue("@porcentaje", i.PorcentajeReserva);
            cmd.Parameters.AddWithValue("@disponible", i.Disponible);
            cmd.ExecuteNonQuery();
            int id = nuevo ? checked((int)cmd.LastInsertedId) : i.IdInmueble;
            using var count = new MySqlCommand("SELECT COUNT(*) FROM inmueble_imagen WHERE id_inmueble = @id AND estado = 1", cn, tx);
            count.Parameters.AddWithValue("@id", id);
            int existentes = Convert.ToInt32(count.ExecuteScalar());
            if (existentes + imagenes.Count > 12)
                throw new InvalidOperationException("Cada inmueble admite hasta 12 imágenes. Quitá alguna antes de agregar más.");
            foreach (string base64 in imagenes)
            {
                using var insert = new MySqlCommand("INSERT INTO inmueble_imagen (id_inmueble, contenido_base64, es_portada) VALUES (@id, @contenido, @portada)", cn, tx);
                insert.Parameters.AddWithValue("@id", id);
                insert.Parameters.AddWithValue("@contenido", base64);
                insert.Parameters.AddWithValue("@portada", existentes++ == 0);
                insert.ExecuteNonQuery();
            }
            tx.Commit();
            i.IdInmueble = id;
            return nuevo ? id : 1;
        }

        public int Baja(int id)
        {
            using var cn = new MySqlConnection(connectionString);
            using var cmd = new MySqlCommand("UPDATE inmueble SET estado = 0, disponible = 0 WHERE id = @id AND estado = 1", cn);
            cmd.Parameters.AddWithValue("@id", id);
            cn.Open();
            return cmd.ExecuteNonQuery();
        }

        public bool CambiarDisponibilidad(int id, bool disponible)
        {
            using var cn = new MySqlConnection(connectionString);
            using var cmd = new MySqlCommand("UPDATE inmueble SET disponible = @disponible WHERE id = @id AND estado = 1", cn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@disponible", disponible);
            cn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public IList<OpcionInmueble> BuscarPropietarios(string? buscar, int? seleccionado = null) => BuscarOpciones("""
            SELECT id, CONCAT(apellido, ', ', nombre, ' (', dni, ')') AS texto FROM propietario
            WHERE (@seleccionado IS NOT NULL AND id = @seleccionado)
               OR (@seleccionado IS NULL AND (CONCAT(nombre, ' ', apellido) LIKE @buscar OR dni LIKE @buscar))
            ORDER BY apellido, nombre, id LIMIT 20
            """, buscar, seleccionado);

        public IList<OpcionInmueble> BuscarTipos(string? buscar, int? seleccionado = null) => BuscarOpciones("""
            SELECT id, nombre AS texto FROM tipo_inmueble
            WHERE (@seleccionado IS NOT NULL AND id = @seleccionado)
               OR (@seleccionado IS NULL AND nombre LIKE @buscar)
            ORDER BY nombre, id LIMIT 20
            """, buscar, seleccionado);

        private IList<OpcionInmueble> BuscarOpciones(string sql, string? buscar, int? seleccionado)
        {
            using var cn = new MySqlConnection(connectionString);
            using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@buscar", "%" + (buscar?.Trim() ?? "") + "%");
            cmd.Parameters.AddWithValue("@seleccionado", (object?)seleccionado ?? DBNull.Value);
            cn.Open();
            using var reader = cmd.ExecuteReader();
            var opciones = new List<OpcionInmueble>();
            while (reader.Read()) opciones.Add(new(reader.GetInt32("id"), reader.GetString("texto")));
            return opciones;
        }

        public IList<ImagenInmueble> ObtenerImagenes(int idInmueble)
        {
            using var cn = new MySqlConnection(connectionString);
            using var cmd = new MySqlCommand("SELECT id, es_portada FROM inmueble_imagen WHERE id_inmueble = @id AND estado = 1 ORDER BY es_portada DESC, id", cn);
            cmd.Parameters.AddWithValue("@id", idInmueble);
            cn.Open();
            using var reader = cmd.ExecuteReader();
            var imagenes = new List<ImagenInmueble>();
            while (reader.Read()) imagenes.Add(new(reader.GetInt32("id"), reader.GetBoolean("es_portada")));
            return imagenes;
        }

        public string? ObtenerImagenBase64(int id)
        {
            using var cn = new MySqlConnection(connectionString);
            using var cmd = new MySqlCommand("SELECT contenido_base64 FROM inmueble_imagen WHERE id = @id AND estado = 1", cn);
            cmd.Parameters.AddWithValue("@id", id);
            cn.Open();
            return cmd.ExecuteScalar() as string;
        }

        private static bool BloquearActivo(MySqlConnection cn, MySqlTransaction tx, int id)
        {
            using var cmd = new MySqlCommand("SELECT id FROM inmueble WHERE id = @id AND estado = 1 FOR UPDATE", cn, tx);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteScalar() != null;
        }

        public bool CambiarPortada(int idInmueble, int idImagen) => ModificarImagen(idInmueble, idImagen, false);
        public bool BajaImagen(int idInmueble, int idImagen) => ModificarImagen(idInmueble, idImagen, true);

        private bool ModificarImagen(int idInmueble, int idImagen, bool baja)
        {
            using var cn = new MySqlConnection(connectionString);
            cn.Open();
            using var tx = cn.BeginTransaction();
            if (!BloquearActivo(cn, tx, idInmueble)) return false;
            using var cmd = new MySqlCommand("SELECT es_portada FROM inmueble_imagen WHERE id = @imagen AND id_inmueble = @inmueble AND estado = 1", cn, tx);
            cmd.Parameters.AddWithValue("@imagen", idImagen);
            cmd.Parameters.AddWithValue("@inmueble", idInmueble);
            var portada = cmd.ExecuteScalar();
            if (portada == null) return false;
            if (baja)
            {
                cmd.CommandText = "UPDATE inmueble_imagen SET estado = 0, es_portada = 0 WHERE id = @imagen AND id_inmueble = @inmueble";
                cmd.ExecuteNonQuery();
                if (Convert.ToBoolean(portada))
                {
                    cmd.CommandText = "UPDATE inmueble_imagen SET es_portada = 1 WHERE id_inmueble = @inmueble AND estado = 1 ORDER BY id LIMIT 1";
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                cmd.CommandText = "UPDATE inmueble_imagen SET es_portada = 0 WHERE id_inmueble = @inmueble AND es_portada = 1";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "UPDATE inmueble_imagen SET es_portada = 1 WHERE id = @imagen AND id_inmueble = @inmueble AND estado = 1";
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
            return true;
        }
    }
}
