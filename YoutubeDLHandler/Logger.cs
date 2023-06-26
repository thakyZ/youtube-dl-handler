using System;

namespace YouTubeDLHandler
{
    internal static class Logger
    {
        internal static void WriteErrorLine(string err, params object[] args)
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
        internal static void WriteDebugLine(string err, params object[] args)
        {
#if DEBUG
            if (string.IsNullOrWhiteSpace(err))
            {
                Console.WriteLine();
                return;
            }

            var old = Console.ForegroundColor;
            Console.Write("[ ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("DEBUG");
            Console.ForegroundColor = old;
            Console.Write(value: " ] ");
            Console.Write(err, args);
            Console.Write(value: "\n");
            Console.ForegroundColor = old;
#endif
            return;
        }
    }
}
