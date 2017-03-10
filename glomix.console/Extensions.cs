namespace glomix.console
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    internal static class Extensions
    {
        public static IList Print(this IList list, ConsoleColor color = ConsoleColor.White)
        {
            for( var i = 0; i < list.Count; i++ )
                Print($"{i:D2}. {list[i]}", color);
            return list;
        }


        public static void Print(this IList<Status> list)
        {
            var strBuilder = new StringBuilder();
            for( var i = 0; i < list.Count; i++ )
                strBuilder.Append($"[{i}] {list[i].Mix.Title.Substring(0, 4)}...{list[i].Percentage}% ");
            Console.Title = strBuilder.ToString();
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