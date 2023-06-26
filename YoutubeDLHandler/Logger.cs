using System;

namespace YouTubeDLHandler
{
    public static class Logger
    {
        public static void WriteErrorLine(string err, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(err))
            {
                Console.Error.WriteLine();
                return;
            }

            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(err, args);
            Console.ForegroundColor = old;
        }
    }
}
