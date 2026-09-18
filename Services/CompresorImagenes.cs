using SkiaSharp;

namespace Proyecto_Inmobiliaria.Services
{
    public class CompresorImagenes
    {
        public const long MaxArchivo = 5 * 1024 * 1024;
        public const long MaxTotal = 30 * 1024 * 1024;

        public async Task<IReadOnlyList<string>> ComprimirAsync(IReadOnlyList<IFormFile> archivos, CancellationToken ct)
        {
            if (archivos.Count > 12 || archivos.Sum(a => a.Length) > MaxTotal)
                throw new ArgumentException("Podés subir hasta 12 imágenes y 30 MB en total por envío.");
            var resultados = new List<string>();
            foreach (var archivo in archivos)
            {
                ct.ThrowIfCancellationRequested();
                if (archivo.Length == 0 || archivo.Length > MaxArchivo)
                    throw new ArgumentException("Cada imagen debe pesar entre 1 byte y 5 MB.");
                using var entrada = archivo.OpenReadStream();
                using var memoria = new MemoryStream();
                await entrada.CopyToAsync(memoria, ct);
                memoria.Position = 0;
                using var codec = SKCodec.Create(memoria);
                if (codec == null || codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp))
                    throw new ArgumentException("Seleccioná imágenes JPG, PNG o WebP válidas.");
                if (codec.Info.Width <= 0 || codec.Info.Height <= 0 || (long)codec.Info.Width * codec.Info.Height > 40_000_000)
                    throw new ArgumentException("La imagen supera el límite de 40 megapíxeles.");
                using var original = new SKBitmap(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
                if (codec.GetPixels(original.Info, original.GetPixels()) != SKCodecResult.Success)
                    throw new ArgumentException("No se pudo leer la imagen completa.");

                // Aplicar la orientación EXIF antes de descartar los metadatos.
                var origen = codec.EncodedOrigin;
                bool girada = origen is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
                int ancho = girada ? original.Height : original.Width;
                int alto = girada ? original.Width : original.Height;
                double escala = Math.Min(1d, 1600d / Math.Max(ancho, alto));
                using var salida = new SKBitmap(Math.Max(1, (int)Math.Round(ancho * escala)), Math.Max(1, (int)Math.Round(alto * escala)), SKColorType.Rgba8888, SKAlphaType.Premul);
                using (var canvas = new SKCanvas(salida))
                {
                    canvas.Clear(SKColors.Transparent);
                    canvas.Scale((float)salida.Width / ancho, (float)salida.Height / alto);
                    switch (origen)
                    {
                        case SKEncodedOrigin.TopRight: canvas.Translate(ancho, 0); canvas.Scale(-1, 1); break;
                        case SKEncodedOrigin.BottomRight: canvas.Translate(ancho, alto); canvas.RotateDegrees(180); break;
                        case SKEncodedOrigin.BottomLeft: canvas.Translate(0, alto); canvas.Scale(1, -1); break;
                        case SKEncodedOrigin.LeftTop: canvas.RotateDegrees(90); canvas.Scale(1, -1); break;
                        case SKEncodedOrigin.RightTop: canvas.Translate(ancho, 0); canvas.RotateDegrees(90); break;
                        case SKEncodedOrigin.RightBottom: canvas.Translate(ancho, alto); canvas.RotateDegrees(90); canvas.Scale(-1, 1); break;
                        case SKEncodedOrigin.LeftBottom: canvas.Translate(0, alto); canvas.RotateDegrees(-90); break;
                    }
                    using var imagen = SKImage.FromBitmap(original);
                    canvas.DrawImage(imagen, 0, 0, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
                }
                using var webp = salida.Encode(SKEncodedImageFormat.Webp, 80);
                if (webp == null || webp.Size > 2 * 1024 * 1024)
                    throw new ArgumentException("La imagen comprimida es demasiado grande; elegí una imagen más pequeña.");
                resultados.Add(Convert.ToBase64String(webp.ToArray()));
            }
            return resultados;
        }
    }
}
