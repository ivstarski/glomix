namespace sdk.model
{
    using System.Collections.Generic;

    public class Menu : List<MenuItem>, IContent
    {
        public string Url { get; set; }

        public override bool Equals(object obj)
        {
            var another = obj as Menu;
            if (Count != another?.Count)
                return false;
            for (var i = 0; i < Count; i++)
                if (this[i].Equals(another[i]) == false)
                    return false;
            return true;
        }

        public override int GetHashCode() => 334.GetHashCode() ^ 56565.GetHashCode();
    }
}