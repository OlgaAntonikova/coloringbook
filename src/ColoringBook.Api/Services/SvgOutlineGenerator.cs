using System.Text;
using System.Xml.Linq;

namespace ColoringBook.Api.Services
{
	public class SvgOutlineGenerator : IOutlineGenerator
	{
        private readonly IPresetProvider _presets;
        public SvgOutlineGenerator(IPresetProvider presets) => _presets = presets;

        public string BuildSvg(string animal, int lineThickness, bool addLabels)
		{
            // исходный SVG пресета
            var raw = _presets.GetSvg(animal);

            // атрибуты/стили
            var doc = XDocument.Parse(raw);

            var svg = doc.Root ?? throw new InvalidOperationException("Invalid SVG");
            if (svg.Attribute("viewBox") == null)
                svg.SetAttributeValue("viewBox", "0 0 800 800");

            // stroke для всех базовых фигур
            foreach (var el in svg.Descendants())
            {
                if (el.Name.LocalName is "path" or "circle" or "rect" or "ellipse" or "line" or "polyline" or "polygon")
                {
                    el.SetAttributeValue("fill", "none");
                    el.SetAttributeValue("stroke", "black");
                    el.SetAttributeValue("stroke-width", lineThickness);
                    el.SetAttributeValue("stroke-linecap", "round");
                    el.SetAttributeValue("stroke-linejoin", "round");
                }
            }

            // подпись
            if (addLabels)
            {
                var label = new XElement(svg.GetDefaultNamespace() + "text",
                    new XAttribute("x", "40"),
                    new XAttribute("y", "780"),
                    new XAttribute("font-family", "Arial"),
                    new XAttribute("font-size", "36"),
                    new XAttribute("fill", "black"),
                    animal.ToUpperInvariant());
                svg.Add(label);
            }

            return doc.ToString(SaveOptions.DisableFormatting);
        }
	}
}
