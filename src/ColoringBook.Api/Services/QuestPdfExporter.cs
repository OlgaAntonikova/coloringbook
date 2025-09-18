using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using SKSvg = Svg.Skia.SKSvg;


namespace ColoringBook.Api.Services
{
	public class QuestPdfExporter : IPdfExporter
	{
        
		
		public async Task ExportSvgToPdfAsync(string svg, string outputPath)
		{
            // 1) Рендерим SVG в PNG (A4, ~300 DPI)
            var pngBytes = RenderSvgToPng(svg, 2480, 3508); // A4 300dpi
            var img = Image.FromBinaryData(pngBytes);

            // 2) Собираем PDF и вставляем PNG на страницу
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.Content().Image(img).FitArea();
                });
            });

            var bytes = doc.GeneratePdf();
            await File.WriteAllBytesAsync(outputPath, bytes);
        
		}

        private static byte[] RenderSvgToPng(string svgText, int widthPx, int heightPx)
        {
            var svg = new SKSvg();
            svg.FromSvg(svgText);


            using var surface = SKSurface.Create(new SKImageInfo(widthPx, heightPx, SKColorType.Bgra8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            var pic = svg.Picture ?? throw new InvalidOperationException("SVG picture is null");
            var vb = pic.CullRect;

            float scale = Math.Min(widthPx / vb.Width, heightPx / vb.Height);
            canvas.Translate((widthPx - vb.Width * scale) / 2f, (heightPx - vb.Height * scale) / 2f);
            canvas.Scale(scale);
            canvas.Translate(-vb.Left, -vb.Top);
            canvas.DrawPicture(pic);

            using var image = surface.Snapshot();
            using var png = image.Encode(SKEncodedImageFormat.Png, 100);
            return png.ToArray();
        }
    }
}
