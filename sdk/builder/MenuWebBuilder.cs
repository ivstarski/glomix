namespace sdk.builder
{
    using System.Linq;
    using System.Threading.Tasks;
    using CsQuery;
    using model;

    class MenuWebBuilder : WebBuilder
    {
        public override async Task<IContent> Build(string url) => await Task.Run(() =>
        {
            var html = Html(url);

            if( string.IsNullOrEmpty(html) )
                return null;

            var menu = new Menu();
            foreach( var li in CQ.Create(html)[".nav.navbar-nav.mainmenu.navbar-left"].Children() )
            {
                var menuItem = new MenuItem();
                var childs = li.ChildElements.ToArray();

                // get menu
                foreach( var a in childs.OfType<IHTMLAnchorElement>() )
                {
                    menuItem.Header = a.InnerText.ToUpper();
                    menuItem.Url = $"{Properties.Resources.HOST}{a.Href}";
                }

                // get sub menu
                if( childs.Length > 1 )
                {
                    foreach( var a in childs[1].ChildElements
                        .Where(element => element.FirstElementChild is IHTMLAnchorElement)
                        .Select(element => element.FirstChild as IHTMLAnchorElement)
                        .Where(a => a != null) )
                    {
                        menuItem.Items.Add(new MenuItem
                        {
                            Header = a.InnerText.ToUpper(),
                            Url = $"{Properties.Resources.HOST}{a.Href}"
                        });
                    }
                }
                menu.Add(menuItem);
            }
            return menu;
        });
    }
}