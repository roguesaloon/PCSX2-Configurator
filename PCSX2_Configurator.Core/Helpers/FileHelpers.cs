using System;
using System.IO;
using System.Linq;

namespace PCSX2_Configurator.Helpers
{
    public sealed class FileHelpers : IFileHelpers
    {
        void IFileHelpers.CopyWithoutException(string sourceFileName, string destFileName)
        {
            try
            {
                File.Copy(sourceFileName, destFileName);
            }
            catch (Exception e)
            {
                if (!(e is FileNotFoundException)) throw;
            }
        }

        public string[] GetFilesToDepth(string path, int depth)
        {
            var directories = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            return depth == (int)SearchOption.TopDirectoryOnly || !directories.Any() ? directories : GetFilesToDepth(path, depth - 1);
        }

        public string GetFileNameSafeString(string fileName) => 
            Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

        public void SetFileToReadOnly(string fileName, bool @readonly) => new FileInfo(fileName)
        {
            IsReadOnly = @readonly
        };
    }
}
