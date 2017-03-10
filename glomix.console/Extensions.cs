namespace glomix.console
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    static class Extensions
    {
        public static void Print(this IList list, ConsoleColor color = ConsoleColor.White)
        {
            for( var i = 0; i < list.Count; i++ )
                Print($"{i:D2}. {list[i]}", color);
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
            Console.WriteLine('\r'+ message);
            Console.ResetColor();
        }

        public static bool InRange(this IList list, int value) => value >= 0 && value < list.Count;
    }
}