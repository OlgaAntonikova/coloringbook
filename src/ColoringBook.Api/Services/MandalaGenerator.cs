using System.Globalization;
using System.Xml.Linq;

namespace ColoringBook.Api.Services
{
	public sealed record MandalaOptions(int Symmetry, int Rings, int Seed, int LineThickness)
	{
		public const int MinSymmetry = 4, MaxSymmetry = 24;
		public const int MinRings = 2, MaxRings = 8;
	}

	/// <summary>
	/// Draws a mandala coloring page: concentric rings, each filled with one motif
	/// (petals, scallops, teeth, beads, leaves or drops) repeated around the center.
	/// The same options always give the same picture, so a page can be shared by its seed.
	/// </summary>
	public sealed class MandalaGenerator
	{
		private const double Size = 800, Center = Size / 2, InnerRadius = 34, OuterRadius = 372;
		private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

		private enum Motif { Petal, Scallop, Teeth, Beads, Leaf, Drop }

		public string BuildSvg(MandalaOptions options)
		{
			Validate(options);

			var random = new Random(options.Seed);
			var root = new XElement(Svg + "svg",
				new XAttribute("viewBox", $"0 0 {Size} {Size}"),
				new XAttribute("width", Size),
				new XAttribute("height", Size));

			var radii = RingRadii(options.Rings, random);
			var step = 360.0 / options.Symmetry;

			// Center flower
			root.Add(Circle(Center, Center, radii[0], options.LineThickness));
			root.Add(Circle(Center, Center, radii[0] * 0.45, options.LineThickness));

			var motifCount = Enum.GetValues<Motif>().Length;
			Motif? previous = null;

			for (var ring = 0; ring < options.Rings; ring++)
			{
				double r0 = radii[ring], r1 = radii[ring + 1], width = r1 - r0;

				// Neighbouring rings never repeat a motif
				Motif motif;
				do motif = (Motif)random.Next(motifCount);
				while (motif == previous);
				previous = motif;

				var shapes = MotifShapes(motif, r0, r1, step / 2).ToList();

				// Wide rings get a smaller copy inside elongated motifs ...
				if (width > 45 && motif is Motif.Petal or Motif.Leaf or Motif.Teeth && random.NextDouble() < 0.6)
					shapes.AddRange(MotifShapes(motif, r0, r0 + width * 0.55, step / 2 * 0.55));

				// ... and sometimes a second layer between the main motifs
				var between = width > 50 && random.NextDouble() < 0.5
					? MotifShapes(random.NextDouble() < 0.5 ? Motif.Leaf : Motif.Beads, r0 + width * 0.08, r0 + width * 0.6, step / 2 * 0.45).ToList()
					: new List<string>();

				for (var copy = 0; copy < options.Symmetry; copy++)
				{
					root.Add(Rotated(copy * step, shapes, options.LineThickness));
					if (between.Count > 0)
						root.Add(Rotated(copy * step + step / 2, between, options.LineThickness));
				}

				root.Add(Circle(Center, Center, r1, options.LineThickness));
			}

			return root.ToString(SaveOptions.DisableFormatting);
		}

		private static void Validate(MandalaOptions o)
		{
			if (o.Symmetry is < MandalaOptions.MinSymmetry or > MandalaOptions.MaxSymmetry)
				throw new ArgumentOutOfRangeException(nameof(o.Symmetry), o.Symmetry,
					$"Symmetry must be {MandalaOptions.MinSymmetry}..{MandalaOptions.MaxSymmetry}.");
			if (o.Rings is < MandalaOptions.MinRings or > MandalaOptions.MaxRings)
				throw new ArgumentOutOfRangeException(nameof(o.Rings), o.Rings,
					$"Rings must be {MandalaOptions.MinRings}..{MandalaOptions.MaxRings}.");
			if (o.LineThickness is < 1 or > 10)
				throw new ArgumentOutOfRangeException(nameof(o.LineThickness), o.LineThickness, "Line thickness must be 1..10.");
		}

		/// <summary>Ring boundaries from the center outwards, with slightly random widths.</summary>
		private static double[] RingRadii(int rings, Random random)
		{
			var weights = Enumerable.Range(0, rings).Select(_ => 0.7 + random.NextDouble() * 0.6).ToArray();
			var total = weights.Sum();

			var radii = new double[rings + 1];
			radii[0] = InnerRadius;
			for (var i = 0; i < rings; i++)
				radii[i + 1] = radii[i] + (OuterRadius - InnerRadius) * weights[i] / total;

			return radii;
		}

		/// <summary>Path data of one motif pointing straight up, spanning ±half degrees.</summary>
		private static IEnumerable<string> MotifShapes(Motif motif, double r0, double r1, double half)
		{
			var rm = (r0 + r1) / 2;

			switch (motif)
			{
				case Motif.Petal:
					yield return $"M {P(r0, -half * 0.85)} Q {P(rm, -half * 1.1)} {P(r1, 0)} Q {P(rm, half * 1.1)} {P(r0, half * 0.85)}";
					break;

				case Motif.Scallop:
					yield return $"M {P(r1, -half)} Q {P(r0 + (r1 - r0) * 0.15, 0)} {P(r1, half)}";
					yield return $"M {P(r1, -half * 0.55)} Q {P(rm, 0)} {P(r1, half * 0.55)}";
					break;

				case Motif.Teeth:
					yield return $"M {P(r0, -half)} L {P(r1, 0)} L {P(r0, half)}";
					yield return $"M {P(r0, 0)} L {P(r0 + (r1 - r0) * 0.6, 0)}";
					break;

				case Motif.Beads:
					var bead = Math.Min((r1 - r0) * 0.35, rm * Math.Sin(half * Math.PI / 180) * 0.8);
					yield return CirclePath(rm, 0, bead);
					yield return CirclePath(rm, 0, bead * 0.45);
					break;

				case Motif.Leaf:
					yield return $"M {P(r0, 0)} Q {P(rm, -half * 0.9)} {P(r1, 0)} Q {P(rm, half * 0.9)} {P(r0, 0)}";
					yield return $"M {P(r0, 0)} L {P(r1 - (r1 - r0) * 0.12, 0)}";
					break;

				case Motif.Drop:
					yield return $"M {P(r1, 0)} Q {P(r0, -half * 1.2)} {P(r0 + (r1 - r0) * 0.25, 0)} Q {P(r0, half * 1.2)} {P(r1, 0)}";
					var dot = Math.Min((r1 - r0) * 0.12, rm * Math.Sin(half * Math.PI / 180) * 0.3);
					yield return CirclePath(r0 + (r1 - r0) * 0.45, 0, dot);
					break;
			}
		}

		/// <summary>Point at radius r and angle a (degrees, 0 = up), as "x y".</summary>
		private static string P(double r, double a)
		{
			var rad = a * Math.PI / 180;
			return $"{F(Center + r * Math.Sin(rad))} {F(Center - r * Math.Cos(rad))}";
		}

		private static string CirclePath(double r, double a, double radius)
		{
			var rad = a * Math.PI / 180;
			double cx = Center + r * Math.Sin(rad), cy = Center - r * Math.Cos(rad);
			return $"M {F(cx - radius)} {F(cy)} A {F(radius)} {F(radius)} 0 1 0 {F(cx + radius)} {F(cy)} " +
			       $"A {F(radius)} {F(radius)} 0 1 0 {F(cx - radius)} {F(cy)}";
		}

		private static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

		private static XElement Rotated(double angle, IEnumerable<string> shapes, int thickness) =>
			new(Svg + "g",
				new XAttribute("transform", $"rotate({angle.ToString("0.###", CultureInfo.InvariantCulture)} {Center} {Center})"),
				shapes.Select(d => Path(d, thickness)));

		private static XElement Path(string d, int thickness) =>
			new(Svg + "path", new XAttribute("d", d), Stroke(thickness));

		private static XElement Circle(double cx, double cy, double r, int thickness) =>
			new(Svg + "circle",
				new XAttribute("cx", F(cx)), new XAttribute("cy", F(cy)), new XAttribute("r", F(r)),
				Stroke(thickness));

		private static object[] Stroke(int thickness) => new object[]
		{
			new XAttribute("fill", "none"),
			new XAttribute("stroke", "black"),
			new XAttribute("stroke-width", thickness),
			new XAttribute("stroke-linecap", "round"),
			new XAttribute("stroke-linejoin", "round")
		};
	}
}
