namespace Momo.MathUtil;

/// <summary>
/// Extension methods for array and collection shuffling using the Fisher-Yates algorithm.
/// </summary>
public static class FisherYatesShuffleExtensions
{
    /// <summary>
    /// Shuffles an array in-place using the Fisher-Yates (Knuth) shuffle algorithm.
    /// This produces a uniform random permutation where every possible ordering is equally likely.
    /// Random.Shared will be used for the shuffle.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to shuffle.</param>
    public static void Shuffle<T>(this T[] array)
    {
        array.Shuffle(Random.Shared);
    }

    /// <summary>
    /// Shuffles an array in-place using the Fisher-Yates (Knuth) shuffle algorithm.
    /// This produces a uniform random permutation where every possible ordering is equally likely.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to shuffle.</param>
    /// <param name="array">The random object to use for the shuffle.</param>
    public static void Shuffle<T>(this T[] array, Random random)
    {
        if (array == null || array.Length <= 1) return;

        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    /// <summary>
    /// Returns a new shuffled list using the Fisher-Yates algorithm.
    /// The original list remains unchanged.
    /// Random.Shared will be used for the shuffle.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    /// <returns>A new list with elements in random order.</returns>
    public static List<T> ShuffleCopy<T>(this IList<T> list)
    {
        return list.ShuffleCopy<T>(Random.Shared);
    }

    /// <summary>
    /// Returns a new shuffled list using the Fisher-Yates algorithm.
    /// The original list remains unchanged.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    /// <param name="array">The array to shuffle.</param>
    /// <returns>A new list with elements in random order.</returns>
    public static List<T> ShuffleCopy<T>(this IList<T> list, Random? random = null)
    {
        if (random == null)
            random = Random.Shared;

        var result = new List<T>(list);
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }
        return result;
    }
}
