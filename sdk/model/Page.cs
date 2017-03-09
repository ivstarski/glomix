namespace sdk.model
{
    public class Page : System.Collections.Generic.List<Info>, IContent
    {
        public Paginator Paginator { get; set; } = new Paginator();
        public string Url { get; set; }
        public string Number { get; set; }

        protected bool Equals(Page other) => string.Equals(Url, other.Url) && string.Equals(Number, other.Number);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Paginator != null ? Paginator.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Url != null ? Url.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Number != null ? Number.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}