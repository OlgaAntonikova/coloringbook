namespace ColoringBook.Api.Storage
{
	public class LocalFileStorage : IFileStorage
	{
		private readonly string _root;
		public LocalFileStorage(string root)
		{
			_root = root;
			Directory.CreateDirectory(_root);
		}

		public string GetPath(string id, string ext) => Path.Combine(_root, $"{id}.{ext}");
		public string Resolve(string fileName) => Path.Combine(_root, fileName);
	}
}
