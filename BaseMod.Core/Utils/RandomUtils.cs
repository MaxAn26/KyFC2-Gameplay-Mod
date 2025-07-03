namespace BaseMod.Core.Utils;
public static class RandomUtils {
    public static Random DefaultRandom { get; } = Random.Shared;

    public static bool Chance( int chance ) => Chance( chance, true, false );
    public static T Chance<T>( int chance, T successValue, T defaultValue ) {
        if(chance < 0 || chance > 100)
            throw new ArgumentOutOfRangeException( nameof( chance ), $"Value '{nameof( chance )}' should be in range [0-100]" );

        double dblChance = chance / 100f;
        return Chance( dblChance, successValue, defaultValue );
    }

    public static bool Chance( double chance ) => Chance( chance, true, false );

    public static T Chance<T>( double chance, T successValue, T defaultValue ) {
        if(chance < 0f || chance > 1f)
            throw new ArgumentOutOfRangeException( nameof( chance ), $"Value '{nameof( chance )}' should be in range [0.0-1.0]" );

        if(chance == 0f)
            return defaultValue;

        if(chance == 1f)
            return successValue;

        double rnd = DefaultRandom.NextDouble();
        return chance >= rnd ? successValue : defaultValue;
    }

    public static int Int32( int max ) => Int32( 0, max );

    public static int Int32( int min, int max ) => DefaultRandom.Next( min, max + 1 );

    public static double Double( double max ) {
        return Double( 0.0, max );
    }

    public static double Double( double min, double max ) {
        return DefaultRandom.NextDouble() * (max - min) + min;
    }

    public static float Float( float min, float max ) {
        return DefaultRandom.NextSingle() * (max - min) + min;
    }

    public static T? Item<T>( IEnumerable<T> items ) => Item( items.ToList() );

    public static T? Item<T>( IList<T> items ) {
        if(items.Count == 0)
            return default;

        if(items.Count == 1)
            return items[0];

        return items[DefaultRandom.Next( items.Count )];
    }

    public static T? Flag<T>(T flags) 
        where T : Enum {
        List<T> list = new();
        foreach (T value in Enum.GetValues(typeof(T))) {
            if (flags.HasFlag(value))
                list.Add(value);
        }

        return Item(list);
    }
}