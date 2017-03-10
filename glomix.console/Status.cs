namespace glomix.console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using sdk.model;

    internal class Status
    {
        public Mix Mix { get; set; }
        public int Percentage { get; set; }

        private static readonly IList<Status> items = new List<Status>();

        public static void Add(Mix mix) => items.Add(new Status { Mix = mix });

        public static void Remove(Mix mix)
        {
            var sts = items.FirstOrDefault(status => status.Mix == mix);
            if( sts != null ) items.Remove(sts);
        }

        public static void SetPercentage(Mix mix, int percentage)
        {
            var sts = items.FirstOrDefault(status => status.Mix == mix);
            if( sts != null )
                sts.Percentage = percentage;
        }
        public static void Print()
        {
            var strBuilder = new StringBuilder();
            for( var i = 0; i < items.Count; i++ )
                strBuilder.Append($"[{i}] {items[i].Mix.Title.Substring(0, 4)}...{items[i].Percentage}% ");
            Console.Title = strBuilder.ToString();
        }
    }
}