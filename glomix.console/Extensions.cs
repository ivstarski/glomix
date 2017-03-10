namespace glomix.console
{
    using System;
    using System.Collections;

    internal static class Extensions
    {
        public static IList Print(this IList list, ConsoleColor color = ConsoleColor.White)
        {
            for( var i = 0; i < list.Count; i++ )
                Print($"{i:D2}. {list[i]}", color);
            return list;
        }

        public static void Print(this string message, ConsoleColor color = ConsoleColor.Yellow)
        {
            Console.ForegroundColor = color;
            Console.WriteLine('\r' + message);
            Console.ResetColor();
        }

        public static void Print(this char message, ConsoleColor color = ConsoleColor.Yellow)
        {
            Console.ForegroundColor = color;
            Console.WriteLine('\r' + message);
            Console.ResetColor();
        }

        public static bool Try(this IList list, ConsoleKeyInfo key, out int n)
            => int.TryParse(key.KeyChar.ToString(), out n) && n >= 0 && n < list.Count;
    }
}