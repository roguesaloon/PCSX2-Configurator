using System;
using System.IO;
using System.Linq;

namespace PCSX2_Configurator.Core
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

        public static string GetFileNameSafeString(string fileName) => 
            Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
    }
}
