namespace ColoringBook.Api.Services
{
    public interface IPresetProvider
    {
        string GetSvg(string key);
        
        IEnumerable<string> PresetsList();
    }
}
