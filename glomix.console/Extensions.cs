namespace glomix.console
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    internal static class Extensions
    {
        public static IList Print(this IList list, ConsoleColor color = ConsoleColor.White)
        {
            for( var i = 0; i < list.Count; i++ )
                Print($"{i:D2}. {list[i]}", color);
            return list;
        }

        public static List<FileInfo> Print(this List<FileInfo> list, ConsoleColor color = ConsoleColor.White)
        {
            for( var i = 0; i < list.Count; i++ )
                Print($"{i:D2}. {list[i].Name}", color);
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

        public static bool Try(this IList list, string str, out int n)
            => int.TryParse(str, out n) && n >= 0 && n < list.Count;

        public static ResultType Try(this IList list, out int n)
        {
            n = 0;
            var key1 = Console.ReadKey();
            switch( key1.Key )
            {
                case ConsoleKey.F1: return ResultType.Menu;
                case ConsoleKey.Delete: return ResultType.Delete;
                case ConsoleKey.Escape: Environment.Exit(0); break;
            }

            var key2 = Console.ReadKey();
            switch( key2.Key )
            {
                case ConsoleKey.F1: return ResultType.Menu; ;
                case ConsoleKey.Delete: return ResultType.Delete;
                case ConsoleKey.Escape: Environment.Exit(0); break;
            }

            int index;
            return list.Try(string.Concat(key1.KeyChar, key2.KeyChar), out index) ? ResultType.Ok : ResultType.Continue;
        }
    }
}