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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
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
        private static readonly IList<Status> stats = new List<Status>();

        private Menu menu;
        private Page page;
        private Mix mix;

        public IBuilder Menu()
        {
            "Loading menu. Waiting...".Print();
            menu = Glomix.Menu(Properties.Resources.Host).Result;
            menu.Print();
            Properties.Resources.MsgMenu.Print();
            return this;
        }

        public IBuilder Page()
        {
            while( true )
            {
                var key = Console.ReadKey();

                // show available tracks,
                // if the user has a choice of a track - return to the menu
                if( key.Key == ConsoleKey.F2 )
                {
                    if( ShowTracks() )
                        return null;
                }

                // show page with tracks
                else if( ShowPage(key) )
                    break;
            }
            return this;
        }

        private static bool ShowTracks()
        {
            if( !Directory.Exists(Properties.Resources.DirectoryMp3) )
            {
                $"Directory {Properties.Resources.DirectoryMp3} is empty".Print();
                return true;
            }
            var files = Directory.GetFiles(Properties.Resources.DirectoryMp3);
            files.Print();
            Properties.Resources.MsgPage.Print();
            return PlayTrack(files);
        }

        private static bool PlayTrack(string[] files)
        {
            // num of file
            var line = Console.ReadLine();

            // IF PUT 'menu' - return to menu
            if( !string.IsNullOrEmpty(line) && string.Equals(line.ToLower(), "menu") )
                return true;

            int number;
            if( int.TryParse(line, out number) && files.InRange(number) )
            {
                Process.Start("wmplayer.exe", "\"" + Path.Combine(Directory.GetCurrentDirectory(), files[number]) + "\"");
                return true;
            }
            return false;
        }

        private bool ShowPage(ConsoleKeyInfo key)
        {
            int menuIndex;
            if( int.TryParse(key.KeyChar.ToString(), out menuIndex) && menu.InRange(menuIndex) )
            {
                $"Load {menu[menuIndex]}. Waiting...".Print();
                page = Glomix.Page(menu[menuIndex].Url).Result;
                page.Print();
                Properties.Resources.MsgPage.Print();
                return true;
            }
            return false;
        }

        public IBuilder Mix()
        {
            while( true )
            {
                var line = Console.ReadLine();
                int number;
                if( int.TryParse(line, out number) && page.InRange(number) )
                {
                    mix = Glomix.Mix(page[number].Url).Result;
                    return this;
                }
                if( !string.IsNullOrEmpty(line) && string.Equals(line.ToLower(), "menu") )
                    return null;
                Properties.Resources.MsgPage.Print();
            }
        }

        public async void BuildAsync() => await Task.Run(() =>
        {
            var dir = Properties.Resources.DirectoryMp3;

            if( Directory.Exists(Properties.Resources.DirectoryMp3) == false )
                Directory.CreateDirectory(dir);

            string fileName = $"{dir}/{mix.Title}.{dir}";
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
                $"Download mix {mix}".Print(ConsoleColor.Green);
            }
        });
    }
}