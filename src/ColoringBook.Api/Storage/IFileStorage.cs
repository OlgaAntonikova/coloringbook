namespace ColoringBook.Api.Storage
{
	public interface IFileStorage
	{
		string GetPath(string id, string ext);
		string? Resolve(string fileName);
	}
}
