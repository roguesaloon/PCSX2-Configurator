namespace PCSX2_Configurator.Helpers
{
    public interface IFileHelpers
    {
        internal void CopyWithoutException(string sourceFileName, string destFileName, bool overwrite = true);
        internal void MergeDirectoriesAndOverwrite(string sourceDirectory, string destDirectory, params string[] dontOverwrite);
        string[] GetFilesToDepth(string path, int depth);
        string GetFileNameSafeString(string fileName);
        void SetFileToReadOnly(string fileName, bool @readonly);
    }
}
