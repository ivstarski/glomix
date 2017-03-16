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
                    case ConsoleKey.F1: Menu(); break;
                    case ConsoleKey.F2: Play(); break;
                    case ConsoleKey.F3: Control(); break;
                    case ConsoleKey.F4: Rebase(); break;
                    case ConsoleKey.Escape: Environment.Exit(0); break;
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

        private static void Page()
        {
            int index;
            if( menu.Try(key, out index) )
            {
                $"Load {menu[index]}. Waiting...".Print();
                page = Glomix.Page(menu[index].Url).Result;
                page.Print();
                $"Page {page.Paginator.Num} of {page.Paginator.Max}".Print();
            }
        }

        private static void Mix()
        {
            while( true )
            {
                Properties.Resources.MsgPage.Print();
                int index;
                switch( page.Try(out index) )
                {
                    case ResultType.Ok:
                        $"Select {page[index].Title}".Print();
                        mix = Glomix.Mix(page[index].Url).Result;
                        return;
                    case ResultType.Continue:
                        continue;
                    case ResultType.Menu:
                        Menu();
                        return;
                }
            }
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

                files.Print();
                Properties.Resources.MsgPage.Print();

                int index;
                switch( files.Try(out index) )
                {
                    case ResultType.Ok:
                        Process.Start("wmplayer.exe", "\"" + files[index].FullName + "\"");
                        return;
                    case ResultType.Menu:
                        Menu();
                        return;
                }
            }
            else $"Directory {path} is empty".Print(ConsoleColor.Red);
        }

        private static void Control()
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

                    int index;
                    switch( files.Try(out index) )
                    {
                        case ResultType.Ok:
                            File.Delete(files[index].FullName);
                            $"{files[index].FullName} deleted.".Print(ConsoleColor.Green);
                            continue;
                        case ResultType.Delete:
                            files.ForEach(info => File.Delete(info.FullName));
                            Menu();
                            return;
                        case ResultType.Continue:
                            continue;
                        case ResultType.Menu:
                            Menu();
                            return;
                    }
                }
            }
            $"Directory {directoryMp3} is empty".Print(ConsoleColor.Red);
            Menu();
        }

        private static void Rebase()
        {
            while( true )
            {
                if( string.IsNullOrEmpty(Properties.Settings.Default.RebasePath) )
                {
                    "Rebase path is not exists. Want you create? [y/n]".Print();
                    switch( Console.ReadKey().Key )
                    {
                        case ConsoleKey.F1: Menu(); return;
                        case ConsoleKey.Escape: Environment.Exit(0); break;
                        case ConsoleKey.N: Menu(); return;
                        case ConsoleKey.Y:
                            "Put existsing path of directory to rebase downloaded files and press ENTER".Print();
                            var path = Console.ReadLine();
                            if( Directory.Exists(path) )
                            {
                                Properties.Settings.Default.RebasePath = path;
                                Properties.Settings.Default.Save();
                            }
                            else "Entered path of directory not exists".Print(ConsoleColor.Red);
                            break;
                    }
                }

                var files = Directory.GetFiles(Properties.Resources.DirectoryMp3)
                    .Select(s => new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), s)))
                    .ToList();

                // if empty
                if( !files.Any() )
                {
                    $"Directory {Properties.Resources.DirectoryMp3} is empty".Print(ConsoleColor.Red);
                    Menu();
                    break;
                }

                files.Print();

                $"Select num mix to rebase, <DEL> for rebase all mixes or <C> for settings rebase dir\ncurrent dir [{Properties.Settings.Default.RebasePath}]".Print();

                int index;
                switch( files.Try(out index) )
                {
                    case ResultType.Ok:
                        File.Move(files[index].FullName,
                            Path.Combine(Properties.Settings.Default.RebasePath, files[index].Name));
                        $"{files[index].FullName} rebased.".Print(ConsoleColor.Green);
                        continue;
                    case ResultType.Home:
                        files.ForEach(info =>
                        {
                            File.Move(info.FullName, Path.Combine(Properties.Settings.Default.RebasePath, info.Name));
                            $"{info.FullName} rebased.".Print(ConsoleColor.Green);
                        });
                        "All files rebased.".Print(ConsoleColor.Green);
                        Menu();
                        return;
                    case ResultType.Continue:
                        continue;
                    case ResultType.Configure:
                        Properties.Settings.Default.RebasePath = "";
                        continue;
                    case ResultType.Menu:
                        Menu();
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
                        Status.Set(tempMix, (percentage = e.ProgressPercentage));
                        switch( e.ProgressPercentage )
                        {
                            case 0:
                                Status.Register(tempMix);
                                break;
                            case 100:
                                Status.UnRegister(tempMix);
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