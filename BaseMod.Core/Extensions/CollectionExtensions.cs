using BaseMod.Core.Utils;

namespace BaseMod.Core.Extensions;
public static class CollectionExtensions {
    public static bool ContainsAny<T>( this IList<T> collection, IList<T> values ) {
        if(values.Count == 0)
            return false;

        foreach(var value in values) {
            if(collection.Contains( value ))
                return true;
        }

        return false;
    }

    public static T? RandomItem<T>( this IEnumerable<T> collection ) => RandomUtils.Item( collection );

    public static void Shuffle<T>( this IList<T> values ) {
        for(int n = values.Count; n > 1;) {
            int k = RandomUtils.DefaultRandom.Next(n);
            --n;
            (values[k], values[n]) = (values[n], values[k]);
        }
    }

    public static IList<T> CommonElements<T>( this IList<T> mainList, IList<T> outList, bool returnNonEmptyListIfEmpty = false ) {
        if(outList.Count == 0)
            return returnNonEmptyListIfEmpty ? mainList : new List<T>();
        else if(mainList.Count == 0) {
            return returnNonEmptyListIfEmpty ? outList : new List<T>();
        }

        return mainList.Intersect( outList ).ToList();
    }
}