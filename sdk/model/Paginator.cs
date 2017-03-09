namespace sdk.model
{
    public class Paginator : System.Collections.Generic.List<Paginator.PaginatorItem>
    {
        public class PaginatorItem : IContent
        {
            public string Number { get; set; }
            public string Url { get; set; }
            public bool IsActive { get; set; }
        }

        public int Max { get; set; }
        public int Min { get; set; }
    }
}