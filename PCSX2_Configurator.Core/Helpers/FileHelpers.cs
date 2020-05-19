using System;
using System.IO;
using System.Linq;

namespace PCSX2_Configurator.Helpers
{
    public static class FileHelpers
    {
        internal static void CopyWithoutException(string sourceFileName, string destFileName)
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

        public static string[] GetFilesToDepth(string path, int depth)
        {
            var directories = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            return depth == (int)SearchOption.TopDirectoryOnly || !directories.Any() ? directories : GetFilesToDepth(path, depth - 1);
        }

        public static string GetFileNameSafeString(string fileName) => 
            Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));

        public static void SetFileToReadOnly(string fileName, bool @readonly) => new FileInfo(fileName)
        {
            IsReadOnly = @readonly
        };
    }
}
