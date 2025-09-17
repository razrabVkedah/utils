namespace Rusleo.Utils.Editor.Windows.PlayerPrefsVisualizer.Internal
{
    internal interface IImporter
    {
        /// <summary>Attempts to add/update entries in the index. Returns count imported.</summary>
        int TryImportIntoIndex(PrefIndex index);
        string DisplayName { get; }
    }
}