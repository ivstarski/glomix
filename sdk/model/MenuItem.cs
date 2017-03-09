namespace sdk.model
{
    using System.Collections.Generic;
    using System.Linq;

    public class MenuItem : IContent
    {
        public override bool Equals(object o)
        {
            var other = o as MenuItem;
            if (other == null)
                return false;
            return Items.Where((t, j) => !t.Equals(other.Items[j])).Any() == false
                   && string.Equals(Header, other.Header)
                   && string.Equals(Url, other.Url);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Header?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (Url?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Items?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
        public string Header { get; set; }
        public string Url { get; set; }
        public List<MenuItem> Items { get; } = new List<MenuItem>();
        public override string ToString() => Header;
    }
}