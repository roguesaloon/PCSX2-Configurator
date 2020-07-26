﻿using System;
using System.IO;
using System.Linq;

namespace PCSX2_Configurator.Helpers
{
    public sealed class FileHelpers : IFileHelpers
    {
        void IFileHelpers.CopyWithoutException(string sourceFileName, string destFileName, bool overwrite)
        {
            try
            {
                File.Copy(sourceFileName, destFileName, overwrite);
            }
            catch (Exception e)
            {
                if (!(e is FileNotFoundException)) throw;
            }
        }

        void IFileHelpers.MergeDirectoriesAndOverwrite(string sourceDirectory, string destDirectory, params string[] dontOverwrite)
        {
            Directory.CreateDirectory(destDirectory);
            foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                var destPath = file.Replace(sourceDirectory + Path.DirectorySeparatorChar, "");
                var destFile = destDirectory + Path.DirectorySeparatorChar + destPath;
                if (dontOverwrite.Any(exclude => File.Exists(destFile) && destPath == exclude)) continue;
                if (File.Exists(destFile)) File.Delete(destFile);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                File.Move(file, destFile);
            }
            if(Directory.Exists(sourceDirectory)) Directory.Delete(sourceDirectory, true);
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
