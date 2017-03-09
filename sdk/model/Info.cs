namespace sdk.model
{
    using System;

    public class Info : IContent
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Img { get; set; }
        public DateTime Date { get; set; }
        public override string ToString() => Title;
    }
}