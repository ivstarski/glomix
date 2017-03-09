namespace sdk.builder
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using CsQuery;
    using model;

    class PageWebBuilder : WebBuilder
    {
        private const string IMAGE_DIRECTORY = "images";

        public override async Task<IContent> Build(string url) => await Task.Run(() =>
        {
            var html = Html(url);

            if( string.IsNullOrEmpty(html) )
                return null;

            var cq = CQ.Create(html);
            var page = new Page();
            // parse mixes
            foreach( var section in cq[".mixes-list-wrapper.clearfix .item-mix section"] )
            {
                var elements = section.ChildElements.ToArray();
                // div mix-image
                var mixImagesElements = elements[0].ChildElements.ToArray();
                var imgUrl = Properties.Resources.HOST + mixImagesElements[0].Attributes["href"];
                var img = Properties.Resources.HOST + "/" + mixImagesElements[0].FirstChild.Attributes["src"];

                // div mix-title
                var mixTitleElements = elements[1].ChildElements.ToArray();
                var title = mixTitleElements[0].InnerHTML;

                // div date-wrapper
                var mixDateElements = elements[2].ChildElements.ToArray();
                var date = mixDateElements[1].InnerHTML;

                page.Add(new Info
                {
                    Title = title,
                    Url = imgUrl,
                    Img = GetImage(img),
                    Date = DateTime.Parse(date)
                });
            }

            // parse paginator items
            foreach( var paginationItem in cq[".pagination"].Children() )
            {
                var item = new Paginator.PaginatorItem();

                if( paginationItem.Classes.Contains("active") )
                    item.IsActive = true;

                var anchor = paginationItem.ChildElements.First();
                item.Url = Properties.Resources.HOST + anchor.Attributes["href"];
                item.Number = anchor.InnerText;

                page.Paginator.Add(item);
            }

            // parse paginator max/min page count
            var paginationItems = cq[".pager-info"].Children().ToArray();
            if( paginationItems.Length == 4 )
            {
                page.Paginator.Min = int.Parse(paginationItems[1].InnerText);
                page.Paginator.Max = int.Parse(paginationItems[3].InnerText);
            }
            return page;
        });

        private static string GetImage(string url)
        {
            // get mix image if set variable IsDownoadImage
            if( Properties.Settings.Default.IsDownoadImage == false )
                return null;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                Path.Combine(IMAGE_DIRECTORY, Path.GetFileName(new Uri(url).LocalPath)));

            if( File.Exists(path) )
                return path;

            if( Directory.Exists(IMAGE_DIRECTORY) == false )
                Directory.CreateDirectory(IMAGE_DIRECTORY);

            using( var wc = new WebClient { Proxy = null } )
                wc.DownloadFile(new Uri(url), path);

            return path;
        }
    }
}