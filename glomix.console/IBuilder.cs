// ----------------------------------------------------------------------
// <copyright file="IBuilder.cs" company="Gazprom Space Systems">
// Copyright statement. All right reserved 
// Developer:   Ivan Starski
// Date: 07/03/2017 11:23
// </copyright>
// ----------------------------------------------------------------------

namespace glomix.console
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using sdk;
    using sdk.model;

    interface IBuilder
    {
        IBuilder Menu();
        IBuilder Page();
        IBuilder Mix();
        void BuildAsync();
    }

    class BuilderImp : IBuilder
    {
        private const string HOST = "https://globaldjmix.com";
        private const string MSG_CHOICE = "Choice item and press ENTER...";
        private const string MSG_CHOICE_MIX = "Choice item and press ENTER or put 'menu' to enter in main menu";
        private const string MSG_MENU = "menu";
        private const string DIRECTORY_MP3 = "mp3";
        private const char SYMBOL_RETURN = '\r';
        private const char SYMBOL_NEWLINE = '\n';

        private static readonly IList<Status> stats = new List<Status>();

        private Menu menu;
        private Page page;
        private Mix mix;

        public IBuilder Menu()
        {
            menu = Glomix.Menu(HOST).Result;
            menu.Print();
            MSG_CHOICE.Print();
            return this;
        }

        public IBuilder Page()
        {
            while( true )
            {
                SYMBOL_RETURN.Print();
                int menuIndex;
                if( int.TryParse(Console.ReadKey().KeyChar.ToString(), out menuIndex) && menu.InRange(menuIndex) )
                {
                    $"{SYMBOL_RETURN}Get data for {menu[menuIndex]}. Waiting...".Print();
                    page = Glomix.Page(menu[menuIndex].Url).Result;
                    page.Print();
                    MSG_CHOICE_MIX.Print();
                    return this;
                }
            }
        }

        public IBuilder Mix()
        {
            while( true )
            {
                SYMBOL_RETURN.Print();
                int number;
                var line = Console.ReadLine();
                if( int.TryParse(line, out number) && page.InRange(number) )
                {
                    mix = Glomix.Mix(page[number].Url).Result;
                    return this;
                }
                // to main menu
                if( !string.IsNullOrEmpty(line) && string.Equals(line.ToLower(), MSG_MENU) )
                    return null;
                MSG_CHOICE_MIX.Print();
            }
        }

        public async void BuildAsync() => await Task.Run(() =>
        {
            if( Directory.Exists(DIRECTORY_MP3) == false )
                Directory.CreateDirectory(DIRECTORY_MP3);

            string fileName = $"{DIRECTORY_MP3}/{mix.Title}.{DIRECTORY_MP3}";
            if( File.Exists(fileName) )
            {
                while( true )
                {
                    $"Mix {mix.Title} has been downloaded. Can You Overwrite Mix? [y/n] ".Print(ConsoleColor.Red);
                    var key = Console.ReadKey().Key;
                    if( key == ConsoleKey.N )
                        return;
                    if( key == ConsoleKey.Y )
                        break;
                }
            }

            using( var wc = new WebClient { Proxy = null } )
            {
                var lastPercentage = -1;
                wc.DownloadProgressChanged += (sender, e) =>
                {
                    if( e.ProgressPercentage != lastPercentage )
                    {
                        lastPercentage = e.ProgressPercentage;
                        var currentStatus = stats.FirstOrDefault(status => status.Mix == mix);
                        if( currentStatus != null )
                            currentStatus.Percentage = lastPercentage;
                        switch( e.ProgressPercentage )
                        {
                            case 0:
                                stats.Add(new Status { Mix = mix });
                                $"Download mix {mix}{SYMBOL_NEWLINE}".Print(ConsoleColor.Green);
                                break;
                            case 100:
                                stats.Remove(currentStatus);
                                $"{mix.Title} downloaded.".Print(ConsoleColor.DarkMagenta);
                                break;
                        }
                        stats.Print();
                    }
                };
                wc.DownloadFileAsync(new Uri(mix.Source), fileName);
            }
        });
    }

    class Status
    {
        public Mix Mix { get; set; }
        public int Percentage { get; set; }
    }

    static class Extensions
    {
        public static void Print(this IList list)
        {
            for( var i = 0; i < list.Count; i++ )
                Console.WriteLine($"{i:D2}. {list[i]}");
        }

        public static void Print(this IList<Status> list)
        {
            var strBuilder = new StringBuilder();
            for( var i = 0; i < list.Count; i++ )
                strBuilder.Append($"{i}. {list[i].Mix.Title.Substring(0, 4)}...{list[i].Percentage}% ");
            Console.Title = strBuilder.ToString();
        }

        public static void Print(this string message, ConsoleColor color = ConsoleColor.Yellow)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Print(this char message, ConsoleColor color = ConsoleColor.Yellow)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static bool InRange(this IList list, int value) => value >= 0 && value < list.Count;
    }
}