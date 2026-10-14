using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace ColoringBook.Api.Services
{
	/// <summary>
	/// Puts the SVG on one A4 page as vector graphics, so lines stay sharp at any print size
	/// and the file stays small.
	/// </summary>
	public class QuestPdfExporter : IPdfExporter
	{
		public async Task ExportSvgToPdfAsync(string svg, string outputPath)
		{
			var bytes = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(20);
					page.Content().AlignCenter().AlignMiddle().Svg(svg).FitArea();
				});
			}).GeneratePdf();

			await File.WriteAllBytesAsync(outputPath, bytes);
		}
	}
}
