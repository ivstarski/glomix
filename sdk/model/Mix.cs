namespace sdk.model
{
    using System;

    public class Mix : IContent, ICloneable
    {
        public string Source { get; set; }
        public string Url { get; set; }
        public string Genre { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        public string Quality { get; set; }
        public string Size { get; set; }
        public string PostDate { get; set; }
        public string RecDate { get; set; }
        public string Href { get; set; }

        public override string ToString() => $"Genre: {Genre}, Artist: {Artist}, Size: {Size}";

        public object Clone() => MemberwiseClone();
    }
}