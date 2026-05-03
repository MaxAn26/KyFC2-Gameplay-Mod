using BaseMod.Core.Utils;

namespace BaseMod.Core.Extensions;
public static class CollectionExtensions
{
    public static bool ContainsAny<T>(this IList<T> collection, IList<T> values)
    {
        if (values.Count == 0)
        {
            return false;
        }

        foreach (T value in values)
        {
            if (collection.Contains(value))
            {
                return true;
            }
        }

        return false;
    }

    public static T RandomItem<T>(this IEnumerable<T> collection) => RandomUtils.Item(collection);

    public static void Shuffle<T>(this IList<T> values)
    {
        for (int n = values.Count; n > 1;)
        {
            int k = RandomUtils.DefaultRandom.Next(n);
            --n;
            (values[k], values[n]) = (values[n], values[k]);
        }
    }

    public static IList<T> CommonElements<T>(this IList<T> mainList, IList<T> outList, bool returnNonEmptyListIfEmpty = false)
    {
        if (outList.Count == 0)
        {
            return returnNonEmptyListIfEmpty ? mainList : [];
        }
        else if (mainList.Count == 0)
        {
            return returnNonEmptyListIfEmpty ? outList : [];
        }

        return [.. mainList.Intersect(outList)];
    }

    public static void AddUniqueRange<T>(this IList<T> list, IEnumerable<T> newItems)
    {
        foreach (T item in newItems)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
    }
}
