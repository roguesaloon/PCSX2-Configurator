using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PCSX2_Configurator.Core
{
    internal static class FileHelpers
    {
        public static void CopyWithoutException(string sourceFileName, string destFileName)
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
    }
}
