using System.Collections.Generic;

public static class ListExensions
{
    public static void Shuffle<T>(this IList<T> list, RandomInt rnd)
    {
        for (var i = 0; i < list.Count; i++)
            list.Swap(i, rnd.GetValue(i, list.Count));
    }
    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }
}
