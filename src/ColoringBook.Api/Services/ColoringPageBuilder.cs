using ColoringBook.Contracts;

namespace ColoringBook.Api.Services
{
	public sealed record ColoringPage(string Svg, int? Seed);

	/// <summary>Builds the SVG for a request: an animal outline or a generated mandala.</summary>
	public sealed class ColoringPageBuilder
	{
		private readonly IOutlineGenerator _outlines;
		private readonly MandalaGenerator _mandalas;

		public ColoringPageBuilder(IOutlineGenerator outlines, MandalaGenerator mandalas)
		{
			_outlines = outlines;
			_mandalas = mandalas;
		}

		/// <exception cref="ArgumentException">The request has invalid or missing values.</exception>
		public ColoringPage Build(DTOs.GenerateRequest request)
		{
			if (request.LineThickness is < 1 or > 10)
				throw new ArgumentOutOfRangeException(nameof(request.LineThickness), "Line thickness must be 1..10.");

			switch (request.Kind.ToLowerInvariant())
			{
				case "animal":
					if (string.IsNullOrWhiteSpace(request.Animal))
						throw new ArgumentException("Choose an animal.", nameof(request.Animal));
					try
					{
						return new ColoringPage(_outlines.BuildSvg(request.Animal, request.LineThickness, request.AddLabels), null);
					}
					catch (FileNotFoundException)
					{
						throw new ArgumentException($"Unknown animal '{request.Animal}'.", nameof(request.Animal));
					}

				case "mandala":
					var seed = request.Seed ?? Random.Shared.Next(1, 1_000_000);
					var svg = _mandalas.BuildSvg(new MandalaOptions(request.Symmetry, request.Rings, seed, request.LineThickness));
					return new ColoringPage(svg, seed);

				default:
					throw new ArgumentException("Kind must be 'animal' or 'mandala'.", nameof(request.Kind));
			}
		}
	}
}
