using System;
using System.IO;

namespace YouTubeDLHandler
{
    internal static class Detector
    {
        internal static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        private static string? CheckFileExtension(string filePath, string[] extensions)
        {
            foreach (var extension in extensions)
            {
                var fullPath = string.Concat(filePath, extension);
                if (File.Exists(fullPath)) {
                    Logger.WriteDebugLine("fullPath = \"{0}\"", fullPath);
                    return fullPath;
                } else
                    Logger.WriteDebugLine("fullPath = \"{0}\"", fullPath);
            }
            return null;
        }

        internal static string? GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            string values = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

            string[] extensions = Environment.GetEnvironmentVariable("PATHEXT")?.Split(Path.PathSeparator) ?? new string[] { ".EXE" };

            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath)) {
                    Logger.WriteDebugLine("fullPath = \"{0}\"", fullPath);
                    return fullPath;
                } else if (Path.GetFileName(fileName).Split('.').Length == 1) {
                    Logger.WriteDebugLine("fileName.Length = \"{0}\"", Path.GetFileName(fileName).Split('.').Length);
                    Logger.WriteDebugLine("fullPath = \"{0}\"", fullPath);
                    var fileExtension = CheckFileExtension(fullPath, extensions);
                    if (File.Exists(fileExtension))
                        return fileExtension;
                }
            }
            return null;
        }

        private static readonly string[] programsToCheck = new string[] {
            "youtube-dl", "yt-dlp"
        };
        
        internal static string? CheckForYouTubeDownloader()
        {
            foreach (string program in programsToCheck)
            {
                if (ExistsOnPath(program))
                {
                    Logger.WriteDebugLine("program = \"{0}\"", program);
                    return GetFullPath(program);
                }
            }

            return null;
        }
    }
}
