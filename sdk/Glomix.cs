// ----------------------------------------------------------------------
// <copyright file="Glomix.cs" company="Gazprom Space Systems">
// Copyright statement. All right reserved 
// Developer:   Ivan Starski
// Date: 16/02/2017 12:09
// </copyright>
// ----------------------------------------------------------------------

namespace sdk
{
    using System.Threading.Tasks;
    using builder;
    using model;

    public static class Glomix
    {
        public static async Task<Menu> Menu(string url) => (Menu)(await new MenuWebBuilder().Build(url));
        public static async Task<Page> Page(string url) => (Page)(await new PageWebBuilder().Build(url));
        public static async Task<Mix> Mix(string url) => (Mix)(await new MixWebBuilder().Build(url));
    }
}