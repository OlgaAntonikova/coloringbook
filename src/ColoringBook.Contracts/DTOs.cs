namespace ColoringBook.Contracts;

public class DTOs
{
	/// <param name="Kind">"animal" (an outline from the presets) or "mandala" (generated).</param>
	/// <param name="Animal">Preset key for Kind = "animal", e.g. "cat".</param>
	/// <param name="Seed">Mandala seed; the same seed and settings give the same page. Random when omitted.</param>
	public record GenerateRequest(
		string? Animal = null,
		int LineThickness = 3,
		bool AddLabels = false,
		string Format = "pdf", // "pdf" | "svg"
		string Kind = "animal",
		int Symmetry = 12,
		int Rings = 5,
		int? Seed = null
	);

	/// <param name="Seed">The mandala seed that was used, so the page can be recreated.</param>
	public record GenerateResult(
		string Id,
		string Url,
		string Format,
		int? Seed = null
	);
}
