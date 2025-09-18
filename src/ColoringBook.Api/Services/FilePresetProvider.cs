
using System.Text.RegularExpressions;

namespace ColoringBook.Api.Services
{
    public class FilePresetProvider : IPresetProvider
    {
        private readonly string _root;
        private static readonly Regex KeyRegex = new(@"^[a-z0-9\-_.]+$", RegexOptions.IgnoreCase);

        public FilePresetProvider(string contentRoot)
        {
            _root = Path.Combine(contentRoot, "Presets", "Animals");
            Directory.CreateDirectory(_root);
        }
        public string GetSvg(string key)
        {
            if (!KeyRegex.IsMatch(key)) throw new ArgumentException("Invalid preset key", nameof(key));
            
            var normalized = key.ToLowerInvariant() switch
            {
                "кот" or "кошка" => "cat",
                "собака" or "пёс" or "пес" => "dog",
                "заяц" or "кролик" => "rabbit",
                "лев" => "lion",
                "лиса" => "fox",
                "медведь" or "бурый медведь" => "bear",
                "черепаха" => "turtle",
                _ => key.ToLowerInvariant()
            };

            var path = Path.Combine(_root, $"{normalized}.svg");
            if (!File.Exists(path))
                throw new FileNotFoundException($"Preset '{normalized}' not found", path);

            return File.ReadAllText(path);
        }

        public IEnumerable<string> PresetsList() =>
            Directory.EnumerateFiles(_root, "*.svg")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(x => x)!;

    }
}
