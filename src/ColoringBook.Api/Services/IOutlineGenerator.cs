namespace ColoringBook.Api.Services
{
	public interface IOutlineGenerator
	{
		/// <summary>
		/// Возвращает SVG c контуром животного.
		/// </summary>
		string BuildSvg(string animal, int lineThickness, bool addLabels);
	}
}
