namespace ColoringBook.Contracts;

public class DTOs
{
	public record GenerateRequest(
		string Animal,
		int LineThickness = 3,
		bool AddLabels = false,
		string Format = "pdf" // "pdf" | "svg"
	);

	public record GenerateResult(
		string Id,
		string Url,
		string Format
	);
}
