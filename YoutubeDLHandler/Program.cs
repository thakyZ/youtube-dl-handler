using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace YouTubeDLHandler
{
    // watch?v= Regex v=(?'v'[a-zA-Z0-9-_]+)
    public class Program
    {
        private static readonly string DEFAULT_YOUTUBE_DL = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "youtube-dl", "youtube-dl.exe");
        private static readonly string DEFAULT_DESTINATION = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        private static string CURRENT_YOUTUBE_DL = DEFAULT_YOUTUBE_DL;
        private static string CURRENT_DESTINATION = DEFAULT_DESTINATION;

        internal static readonly string ASSEMBLY_LOCATION = Assembly.GetAssembly(typeof(Program))!.Location;

        private static string[] blacklist = new string[]
        {
            "--U", "--update", "--config-location", "--download-archive",
            "--external-downloader", "-a", "--batch-file", "-o", "--output",
            "--load-info-json", "--cookies", "--cache-dir", "--ffmpeg-location", "--exec"
        };

        [STAThread]
        public static void Main(string[] args)
        {
            /* Setup */
            if (args.Length == 0)
            {
                Setup();
                return;
            }
            foreach (var item in blacklist)
            {
                if (args.Contains(item))
                {
                    WaitAndExit("Please don't use option {0}.", true, item);
                    return;
                }
            }
            if (args.Length == 1 && args[0].Contains("%20")) args = args[0].Split(new string[] { "%20" }, StringSplitOptions.None);

            // Check youtube-dl
            if (!File.Exists(DEFAULT_YOUTUBE_DL))
            {
                CURRENT_YOUTUBE_DL = Detector.CheckForYouTubeDownloader() ?? string.Empty;
                if (!File.Exists(CURRENT_YOUTUBE_DL))
                {
                    Logger.WriteDebugLine("CURRENT_YOUTUBE_DL = \"{0}\"", CURRENT_YOUTUBE_DL);
                    WaitAndExit("youtube-dl.exe not found. Did you install the software correctly?");
                    return;
                }
            }

            /* Arguments */
            args[0] = args[0].Substring(args[0].IndexOf(':') + 1);
            string ytdlArgs = args.Where(p => !p.Contains("youtube.com/watch")).Aggregate("", (cur, next) => $"{cur} {next}").Trim();
            string yt = args.FirstOrDefault(p => p.Contains("youtube.com/watch")) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(yt))
            {
                Logger.WriteErrorLine(@"No youtube.com/watch URL found in parameters.\nPress any key to exit...");
                Console.ReadKey();
                return;
            }

            // output folder
            string destination = CURRENT_DESTINATION;
            if (!Directory.Exists(destination))
            {
                Logger.WriteErrorLine("Destination directory {0} does not exist. Please run the application without any arguments to configure the download destination.");
                Console.ReadKey();
                return;
            }

            // Run
            Console.WriteLine("Running youtube-dl...");
            string path = Path.Combine(destination, "%(title)s.%(ext)s");
            try
            {
                Handler(path, yt, ytdlArgs);
            }
            catch (Exception exc)
            {
                Logger.WriteErrorLine("Failed to run youtube-dl. Error: {0}", exc.ToString());
                Console.ReadKey();
                return;
            }

            // Show output
            ShowRecentFile(destination);

#if DEBUG
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
#endif
        }

        private static string GetProgramName()
        {
            return File.Exists(CURRENT_YOUTUBE_DL) ? Path.GetFileNameWithoutExtension(CURRENT_YOUTUBE_DL) : "unknown";
        }

        // Runs youtube-dl
        private static void Handler(string destination, string target, string args)
        {
            if (!File.Exists(CURRENT_YOUTUBE_DL)) throw new FileNotFoundException($"{GetProgramName()}.exe not found.");

            Process process = new Process();
            process.StartInfo.FileName = CURRENT_YOUTUBE_DL;
            string path = Path.Combine(destination, "%(title)s.%(ext)s");
            process.StartInfo.Arguments = $"--newline --no-playlist -o {destination} {args} \"{target}\"";

            Console.WriteLine(" {0}: {1}", GetProgramName(), Path.GetFullPath(process.StartInfo.FileName));
            Console.WriteLine(" arguments: {0}", process.StartInfo.Arguments);

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;

            // Handle output
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Logger.WriteErrorLine(e.Data ?? "DataRecievcedEventAgrs -> (string)Data returned null");

            // Wait for youtube-dl
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        // Because I don't know how to get the file from the Process, we get the last created file.
        private static void ShowRecentFile(string destination)
        {
            DirectoryInfo di = new DirectoryInfo(destination);
            var createdFile = di.GetFiles().OrderByDescending(f => f.CreationTime).First();

            // Copy to clipboard.
            StringCollection paths = new StringCollection { createdFile.FullName };
            Clipboard.SetFileDropList(paths);

            // Show in explorer.
            Process.Start("explorer.exe", $"/select, \"{createdFile.FullName}\"");
        }

        private static (int Left, int Top) CursorPositionZero => (0, 0);

        private static void Setup()
        {
            Console.Write(" ");
            // Download youtube-dl
            if (!File.Exists(DEFAULT_YOUTUBE_DL))
            {
                uint input = 0;
                (int Left, int Top) = CursorPositionZero;

                try
                {
                    (Left, Top) = Console.GetCursorPosition();
                }
                catch {}

                while (input == 0)
                {
                    Console.Write("Would you rather use YouTube-DL or YT-DLP? [1/2]: ");
                    var _input = Console.ReadLine();
                    switch (_input)
                    {
                        case "1":
                            input = 1;
                            break;
                        case "2":
                            input = 2;
                            break;
                        default:
                            Console.SetCursorPosition(Left, Top);
                            Console.Write(new string(' ', 50 + (_input?.Length ?? 0)));
                            Console.SetCursorPosition(Left, Top);
                            break;
                    }
                }

                if (input == 1)
                {
                    Console.WriteLine("Downloading youtube-dl...");
                    Downloader.Download("https://yt-dl.org/latest/youtube-dl.exe", DEFAULT_YOUTUBE_DL);
                    Console.WriteLine("Download complete.");
                }
                else
                {
                    Console.WriteLine("Downloading yt-dlp...");
                    Downloader.Download("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe", DEFAULT_YOUTUBE_DL);
                    Console.WriteLine("Download complete.");
                }
            }

            Console.WriteLine($"Download destination (empty for '{Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)}'):");

            if (!File.Exists(Path.Join(ASSEMBLY_LOCATION, "settings.ini")))
            {
                var newPath = Console.ReadLine() ?? string.Empty;

                if (newPath != string.Empty)
                {
                    CURRENT_DESTINATION = newPath;
                    try
                    {
                        using (FileStream fileStream = new FileStream(Path.Join(ASSEMBLY_LOCATION, "settings.ini"), FileMode.CreateNew))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(fileStream))
                            {
                                streamWriter.WriteLine($"DESTINATION=\"{newPath}\"");
                                streamWriter.Close();
                            }
                            fileStream.Close();
                        }
                    }
                    catch (Exception exception)
                    {
                        WaitAndExit("Error Writing Config.\n{0}", true, exception);
                        return;
                    }

                }
            }

            if (File.Exists(Path.Join(ASSEMBLY_LOCATION, "settings.ini")) && CURRENT_DESTINATION == DEFAULT_DESTINATION)
            {
                try
                {
                    using (FileStream fileStream = new FileStream(Path.Join(ASSEMBLY_LOCATION, "settings.ini"), FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            var fileContents = streamReader.ReadToEnd();
                            var fileLines = fileContents.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string line in fileLines)
                            {
                                if (line.StartsWith("DESTINATION=\"") && line.EndsWith("\""))
                                {
                                    var tempLine = line.Replace("DESTINATION=\"", "");
                                    tempLine = tempLine.Remove(tempLine.Length - 1, 1);
                                    CURRENT_DESTINATION = string.IsNullOrWhiteSpace(tempLine) ? DEFAULT_DESTINATION : tempLine;
                                }
                            }
                            streamReader.Close();
                        }
                        fileStream.Close();
                    }
                }
                catch (Exception exception)
                {
                    WaitAndExit("Error Reading Config.\n{0}", true, exception);
                    return;
                }
            }
        }

        private static void WaitAndExit(string message, bool error = false, params object[] args)
        {
            if (error)
                Logger.WriteErrorLine(message, args);
            else
                Console.WriteLine(message, args);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
