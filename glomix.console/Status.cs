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

        public static void Register(Mix mix) => items.Add(new Status { Mix = mix });

        public static void UnRegister(Mix mix)
        {
            var sts = items.FirstOrDefault(status => status.Mix == mix);
            if( sts != null ) items.Remove(sts);
        }

        public static void Set(Mix mix, int percentage)
        {
            var sts = items.FirstOrDefault(status => status.Mix == mix);
            if( sts != null )
                sts.Percentage = percentage;
        }
        public static void Print()
        {
            var strBuilder = new StringBuilder();
            foreach( var item in items )
                strBuilder.Append($"[{item.Percentage}%] {item.Mix.Title.Substring(0, 5)}...");
            Console.Title = strBuilder.ToString();
        }
    }
}