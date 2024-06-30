namespace TeamCity.MSBuild.Logger;

internal class DictionaryEntryKeyComparer : IComparer<Property>
{
    public static readonly IComparer<Property> Shared = new DictionaryEntryKeyComparer();

    private DictionaryEntryKeyComparer()
    {
    }

    public int Compare(Property x, Property y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
}