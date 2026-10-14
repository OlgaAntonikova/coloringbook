using System.Text.RegularExpressions;

namespace ColoringBook.Api.Storage
{
	public class LocalFileStorage : IFileStorage
	{
		// Only names this storage generates: a 32-character hex id plus a known extension
		private static readonly Regex FileNameRegex = new(@"^[0-9a-f]{32}\.(svg|pdf)$", RegexOptions.Compiled);

		private readonly string _root;

		public LocalFileStorage(string root)
		{
			_root = Path.GetFullPath(root);
			Directory.CreateDirectory(_root);
		}

		public string GetPath(string id, string ext) => Path.Combine(_root, $"{id}.{ext}");

		/// <summary>
		/// Maps a requested file name to a path inside the storage folder, or returns null for
		/// anything that is not a generated file name (e.g. "..\appsettings.json").
		/// </summary>
		public string? Resolve(string fileName)
		{
			if (!FileNameRegex.IsMatch(fileName))
				return null;

			var full = Path.GetFullPath(Path.Combine(_root, fileName));
			return full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? full : null;
		}
	}
}
