namespace sdk.builder
{
    using System;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using model;

    abstract class WebBuilder : IWebBuilder
    {
        public abstract Task<IContent> Build(string url);

        protected string Html(string url)
        {
            try
            {
                using( var wc = new WebClient { Proxy = null, Encoding = Encoding.UTF8} )
                    return wc.DownloadString(new Uri(url));
            }
            catch
            {
                return null;
            }
        }
    }
}