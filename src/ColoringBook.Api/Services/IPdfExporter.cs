namespace ColoringBook.Api.Services
{
	public interface IPdfExporter
	{
		Task ExportSvgToPdfAsync(string svg, string filePath);
	}
}
