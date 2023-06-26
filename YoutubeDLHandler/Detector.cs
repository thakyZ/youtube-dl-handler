using System;
using System.IO;

namespace YouTubeDLHandler
{
    public static class Detector
    {
        public static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        private static string CheckFileExtension(string filePath, string[] extensions)
        {
            foreach (var extension in extensions)
            {
                var fullPath = string.Concat(filePath, extension);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");

            var extensions = Environment.GetEnvironmentVariable("PATHEXT").Split(Path.PathSeparator);

            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
                else if (Path.GetFileName(fileName).Split('.').Length == 1)
                    return CheckFileExtension(fullPath, extensions);

            }
            return null;
        }

        private static readonly string[] programsToCheck = new string[] {
            "youtube-dl", "yt-dlp"
        };
        
#pragma warning disable VSSpell001 // Spell Check
        public static string CheckForYouTubeDownloader()
        {
            foreach (string program in programsToCheck)
            {
                if (ExistsOnPath(program))
                {
                    return GetFullPath(program);
                }
            }

            return null;
        }
    }
}
