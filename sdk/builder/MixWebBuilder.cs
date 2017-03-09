namespace sdk.builder
{
    using System;
    using System.Threading.Tasks;
    using CefSharp;
    using CefSharp.OffScreen;
    using CsQuery;
    using model;

    class MixWebBuilder : WebBuilder
    {
        public override async Task<IContent> Build(string url)
        {
            if( url == null )
                return null;

            var cq = CQ.Create(Html(url))[".top-single.clearfix .col-md-3"];
            var mix = new Mix
            {
                Title    = cq[".header-title a"][0].InnerText,
                Genre    = (cq[".genre-single-item.single-item-field a"])[0].InnerText,
                Artist   = (cq[".artist-single-item.single-item-field a"])[0].InnerText,
                Duration = (cq[".duration-single-item.single-item-field .prop-value"])[0].InnerText,
                Quality  = (cq[".quality-single-item.single-item-field .prop-value"])[0].InnerText,
                Size     = (cq[".size-single-item.single-item-field .prop-value"])[0].InnerText,
                PostDate = (cq[".post-date-single-item.single-item-field .prop-value"])[0].InnerText,
                RecDate  = (cq[".rec-date-single-item.single-item-field .prop-value"])[0].InnerText,
                Href     = (cq[".download-link-zippy.z-external-link.download-animate-link"])[0].Attributes["href"]
            };
            mix.Source = await GetMp3Source(mix.Href);
            return mix;
        }

        private static async Task<string> GetMp3Source(string url)
        {
            if( !Cef.IsInitialized )
            {
                var settings = new CefSettings
                {
                    LogSeverity = LogSeverity.Disable,
                    WindowlessRenderingEnabled = true
                };
                settings.SetOffScreenRenderingBestPerformanceArgs();
                settings.CefCommandLineArgs.Add("enable-logging", "0");
                settings.CefCommandLineArgs.Add("disable-extensions", "1");
                settings.CefCommandLineArgs.Add("disable-plugins-discovery", "1");
                settings.CefCommandLineArgs.Add("disable-pdf-extension", "1");
                settings.CefCommandLineArgs.Add("no-proxy-server", "1");
                settings.CefCommandLineArgs.Add("debug-plugin-loading", "0");
                settings.CefCommandLineArgs.Add("enable-media-stream", "0");
                Cef.Initialize(settings);
            }

            using( var browser = new ChromiumWebBrowser(url) )
            {
                var tcs = new TaskCompletionSource<string>();
                EventHandler<LoadingStateChangedEventArgs> handler = null;
                handler = async (sender, args) =>
                {
                    if( args.IsLoading == false && sender is ChromiumWebBrowser )
                    {
                        var br = (ChromiumWebBrowser)sender;
                        br.LoadingStateChanged -= handler;
                        tcs.TrySetResult((await br.EvaluateScriptAsync("document.getElementById('dlbutton').href;")).Result.ToString());
                    }
                };
                browser.LoadingStateChanged += handler;
                return await tcs.Task;
            }
        }
    }
}