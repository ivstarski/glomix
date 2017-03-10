namespace glomix.console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using sdk;
    using sdk.model;

    internal class Program
    {
        static readonly IList<Status> stats = new List<Status>();
        static ConsoleKeyInfo key;
        static Menu menu;
        static Page page;
        static Mix mix;

        // ReSharper disable once FunctionRecursiveOnAllPaths
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            ShowMenu();
            do
            {
                switch( (key = Console.ReadKey(false)).Key )
                {
                    case ConsoleKey.F1:
                        ShowMenu();
                        break;
                    case ConsoleKey.F2:
                        PlayTrack();
                        break;
                    default:
                        ShowPage();
                        SelectMix();
                        GetMixAsync(CheckDir());
                        ShowMenu();
                        break;
                }
            } while( key.Key != ConsoleKey.Escape );
        }

        private static void ShowMenu()
        {
            if( menu == null )
                menu = Glomix.Menu(Properties.Resources.Host).Result;
            menu.Print();
            Properties.Resources.MsgMenu.Print();
        }

        private static void PlayTrack()
        {
            var dir = Properties.Resources.DirectoryMp3;
            if( Directory.Exists(dir) )
            {
                var files = Directory.GetFiles(dir).Print();
                Properties.Resources.MsgPage.Print();

                key = Console.ReadKey();
                if( key.Key == ConsoleKey.F1 )
                {
                    ShowMenu();
                    return;
                }

                int number;
                if( files.Try(key, out number) )
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), files[number].ToString());
                    Process.Start("wmplayer.exe", "\"" + path + "\"");
                }
            }
            else $"Directory {dir} is empty".Print();
        }

        private static void ShowPage()
        {
            int index;
            if( menu.Try(key, out index) )
            {
                $"Load {menu[index]}. Waiting...".Print();
                page = Glomix.Page(menu[index].Url).Result;
                page.Print();
                Properties.Resources.MsgPage.Print();
            }
        }

        private static void SelectMix()
        {
            while( true )
            {
                key = Console.ReadKey();
                if( key.Key == ConsoleKey.F1 )
                {
                    ShowMenu();
                    return;
                }
                if( key.Key == ConsoleKey.Escape )
                    Environment.Exit(0);

                int index;
                if( page.Try(key, out index) )
                {
                    $"Select {page[index].Title}".Print();
                    mix = Glomix.Mix(page[index].Url).Result;
                    return;
                }
                Properties.Resources.MsgPage.Print();
            }
        }

        private static string CheckDir()
        {
            var dir = Properties.Resources.DirectoryMp3;
            var path = $"{dir}/{mix.Title}.{dir}";

            if( Directory.Exists(dir) == false )
                Directory.CreateDirectory(dir);

            if( File.Exists(path) )
            {
                while( true )
                {
                    $"Mix {mix.Title} has been downloaded.\nCan You Overwrite Mix? [y/n] or delete mix [del]".Print(ConsoleColor.Red);
                    key = Console.ReadKey();
                    if( key.Key == ConsoleKey.N )
                        return string.Empty;
                    if( key.Key == ConsoleKey.Y )
                        break;
                    if( key.Key == ConsoleKey.Delete )
                    {
                        File.Delete(Path.Combine(Directory.GetCurrentDirectory(), path));
                        path = string.Empty;
                        break;
                    }
                    if( key.Key == ConsoleKey.Escape )
                        Environment.Exit(0);
                }
            }
            return path;
        }

        private static async void GetMixAsync(string path) => await Task.Run(() =>
        {
            if( string.IsNullOrEmpty(path) )
                return;
            $"Download mix {mix}".Print(ConsoleColor.Green);
            using( var wc = new WebClient { Proxy = null } )
            {
                var percentage = -1;
                wc.DownloadProgressChanged += (sender, e) =>
                {
                    if( e.ProgressPercentage != percentage )
                    {
                        percentage = e.ProgressPercentage;
                        var currentStatus = stats.FirstOrDefault(status => status.Mix == mix);
                        if( currentStatus != null )
                            currentStatus.Percentage = percentage;
                        switch( e.ProgressPercentage )
                        {
                            case 0:
                                stats.Add(new Status { Mix = mix });
                                break;
                            case 100:
                                stats.Remove(currentStatus);
                                $"{mix.Title} downloaded.".Print(ConsoleColor.DarkMagenta);
                                break;
                        }
                        stats.Print();
                    }
                };
                wc.DownloadFileAsync(new Uri(mix.Source), path);
            }
        });
    }
}