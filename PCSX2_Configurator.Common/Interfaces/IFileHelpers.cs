namespace PCSX2_Configurator.Helpers
{
    public interface IFileHelpers
    {
        internal void CopyWithoutException(string sourceFileName, string destFileName);
        string[] GetFilesToDepth(string path, int depth);
        string GetFileNameSafeString(string fileName);
        void SetFileToReadOnly(string fileName, bool @readonly);
    }
}
