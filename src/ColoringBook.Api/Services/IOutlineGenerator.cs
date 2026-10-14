namespace ColoringBook.Api.Services
{
	public interface IOutlineGenerator
	{
		/// <summary>
		/// Returns the animal outline as SVG, restyled for coloring.
		/// </summary>
		string BuildSvg(string animal, int lineThickness, bool addLabels);
	}
}
