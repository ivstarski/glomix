namespace glomix.console
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using sdk;
    using sdk.model;

    internal class Program
    {
        private static ConsoleKeyInfo key;
        private static Menu menu;
        private static Page page;
        private static Mix mix;

        private static void Main()
        {
            Menu();
            do
            {
                switch( (key = Console.ReadKey(false)).Key )
                {
                    case ConsoleKey.F1:
                        Menu();
                        break;
                    case ConsoleKey.F2:
                        Play();
                        break;
                    case ConsoleKey.F3:
                        Manage();
                        break;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                    default:
                        Page();
                        Mix();
                        DownloadAsync();
                        Menu();
                        break;
                }
            } while( key.Key != ConsoleKey.Escape );
        }

        private static void Menu()
        {
            if( menu == null )
                menu = Glomix.Menu(Properties.Resources.Host).Result;
            menu.Print();
            Properties.Resources.MsgMenu.Print();
        }

        private static void Play()
        {
            var path = Properties.Resources.DirectoryMp3;
            if( Directory.Exists(path) )
            {
                var files = Directory.GetFiles(path)
                    .Select(s => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), s)))
                    .ToList();

                // if empty
                if( !files.Any() )
                {
                    $"Directory {path} is empty".Print(ConsoleColor.Red);
                    return;
                }

                Properties.Resources.MsgPage.Print();
                key = Console.ReadKey();
                switch( key.Key )
                {
                    case ConsoleKey.F1:
                        Menu();
                        return;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        return;
                }

                int number;
                if( files.Try(key, out number) )
                    Process.Start("wmplayer.exe", "\"" + files[number].FullName + "\"");
            }
            else $"Directory {path} is empty".Print(ConsoleColor.Red);
        }

        private static void Manage()
        {
            var directoryMp3 = Properties.Resources.DirectoryMp3;
            if( Directory.Exists(directoryMp3) )
            {
                while( true )
                {
                    var files = Directory.GetFiles(directoryMp3)
                        .Select(s => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), s)))
                        .ToList();

                    // if empty
                    if( !files.Any() ) break;

                    files.Print();
                    Properties.Resources.MsgManage.Print();
                    key = Console.ReadKey();
                    switch( key.Key )
                    {
                        case ConsoleKey.F1:
                            Menu();
                            return;
                        case ConsoleKey.Escape:
                            Environment.Exit(0);
                            return;
                        case ConsoleKey.Delete:
                            files.ForEach(info => File.Delete(info.FullName));
                            Menu();
                            return;
                    }
                    // delete mix
                    int number;
                    if( files.Try(key, out number) )
                        File.Delete(files[number].FullName);
                }
            }
            $"Directory {directoryMp3} is empty".Print(ConsoleColor.Red);
            Menu();
        }

        private static void Page()
        {
            int index;
            if( menu.Try(key, out index) )
            {
                $"Load {menu[index]}. Waiting...".Print();
                page = Glomix.Page(menu[index].Url).Result;
                page.Print();
                $"Page {page.Paginator.Num} of {page.Paginator.Max}".Print(ConsoleColor.DarkGreen);
            }
        }

        private static void Mix()
        {
            while( true )
            {
                Properties.Resources.MsgPage.Print();
                key = Console.ReadKey();
                switch( key.Key )
                {
                    case ConsoleKey.F1:
                        Menu();
                        return;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        return;
                }

                int index;
                if( page.Try(key, out index) )
                {
                    $"Select {page[index].Title}".Print();
                    mix = Glomix.Mix(page[index].Url).Result;
                    return;
                }
            }
        }

        private static async void DownloadAsync() => await Task.Run(() =>
        {
            var path = CheckMix();

            // if path is empty, file has been downloaded
            if( string.IsNullOrEmpty(path) )
                return;

            var tempMix = (Mix)mix.Clone();
            using( var wc = new WebClient { Proxy = null } )
            {
                var percentage = -1;
                wc.DownloadProgressChanged += (sender, e) =>
                {
                    if( e.ProgressPercentage != percentage )
                    {
                        Status.SetPercentage(tempMix, (percentage = e.ProgressPercentage));
                        switch( e.ProgressPercentage )
                        {
                            case 0:
                                Status.Add(tempMix);
                                break;
                            case 100:
                                Status.Remove(tempMix);
                                $"{tempMix.Title} downloaded.".Print(ConsoleColor.DarkMagenta);
                                break;
                        }
                        Status.Print();
                    }
                };
                $"Download mix {tempMix}".Print(ConsoleColor.Green);
                wc.DownloadFileAsync(new Uri(tempMix.Source), path);
            }
        });

        private static string CheckMix()
        {
            if( mix == null )
                return string.Empty;
            var directoryMp3 = Properties.Resources.DirectoryMp3;
            var path = $"{directoryMp3}/{mix.Title}.mp3";
            if( Directory.Exists(directoryMp3) == false )
                Directory.CreateDirectory(directoryMp3);
            if( File.Exists(path) )
            {
                while( true )
                {
                    $"{mix.Title} has been downloaded.\nCan You Overwrite Mix? [y/n]".Print(ConsoleColor.Red);
                    switch( Console.ReadKey().Key )
                    {
                        case ConsoleKey.N:
                            return string.Empty;
                        case ConsoleKey.Y:
                            return path;
                        case ConsoleKey.Escape:
                            Environment.Exit(0);
                            break;
                    }
                }
            }
            return path;
        }
    }
}