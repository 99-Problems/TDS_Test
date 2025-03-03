using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UniRx;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Data;
using Random = System.Random;

public static class Extension
{
    public static bool IsValid(this GameObject go)
    {
        return go != null && go.activeSelf;
    }

    public static Color GetHexarColor(string _hexar)
    {
        Color color;
        if (false == ColorUtility.TryParseHtmlString(_hexar, out color))
            return new Color();

        return color;
    }

    public static bool IsNullOrWhitespace(this string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            for (int index = 0; index < str.Length; ++index)
            {
                if (!char.IsWhiteSpace(str[index]))
                    return false;
            }
        }

        return true;
    }

    public static IEnumerable<T> Examine<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T obj in source)
        {
            action(obj);
            yield return obj;
        }
    }

    /// <summary>Perform an action on each item.</summary>
    /// <param name="source">The source.</param>
    /// <param name="action">The action to perform.</param>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T obj in source)
            action(obj);
        return source;
    }

    /// <summary>Perform an action on each item.</summary>
    /// <param name="source">The source.</param>
    /// <param name="action">The action to perform.</param>
    public static IEnumerable<T> ForEach<T>(
        this IEnumerable<T> source,
        Action<T, int> action)
    {
        int num = 0;
        foreach (T obj in source)
            action(obj, num++);
        return source;
    }

    /// <summary>Convert each item in the collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="converter">Func to convert the items.</param>
    public static IEnumerable<T> Convert<T>(
        this IEnumerable source,
        Func<object, T> converter)
    {
        foreach (object obj in source)
            yield return converter(obj);
    }

    /// <summary>Convert a colletion to a HashSet.</summary>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new HashSet<T>(source);

    /// <summary>Convert a colletion to a HashSet.</summary>
    public static HashSet<T> ToHashSet<T>(
        this IEnumerable<T> source,
        IEqualityComparer<T> comparer)
    {
        return new HashSet<T>(source, comparer);
    }

    /// <summary>Add an item to the beginning of a collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="prepend">Func to create the item to prepend.</param>
    public static IEnumerable<T> PrependWith<T>(
        this IEnumerable<T> source,
        Func<T> prepend)
    {
        yield return prepend();
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>Add an item to the beginning of a collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="prepend">The item to prepend.</param>
    public static IEnumerable<T> PrependWith<T>(this IEnumerable<T> source, T prepend)
    {
        yield return prepend;
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add a collection to the beginning of another collection.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="prepend">The collection to prepend.</param>
    public static IEnumerable<T> PrependWith<T>(
        this IEnumerable<T> source,
        IEnumerable<T> prepend)
    {
        foreach (T obj in prepend)
            yield return obj;
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">Func to create the item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(
        this IEnumerable<T> source,
        bool condition,
        Func<T> prepend)
    {
        if (condition)
            yield return prepend();
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(
        this IEnumerable<T> source,
        bool condition,
        T prepend)
    {
        if (condition)
            yield return prepend;
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add a collection to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The collection to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(
        this IEnumerable<T> source,
        bool condition,
        IEnumerable<T> prepend)
    {
        if (condition)
        {
            foreach (T obj in prepend)
                yield return obj;
        }

        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">Func to create the item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(
        this IEnumerable<T> source,
        Func<bool> condition,
        Func<T> prepend)
    {
        if (condition())
            yield return prepend();
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(
        this IEnumerable<T> source,
        Func<bool> condition,
        T prepend)
    {
        if (condition())
            yield return prepend;
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add a collection to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The collection to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(
        this IEnumerable<T> source,
        Func<bool> condition,
        IEnumerable<T> prepend)
    {
        if (condition())
        {
            foreach (T obj in prepend)
                yield return obj;
        }

        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">Func to create the item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(
        this IEnumerable<T> source,
        Func<IEnumerable<T>, bool> condition,
        Func<T> prepend)
    {
        if (condition(source))
            yield return prepend();
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The item to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(
        this IEnumerable<T> source,
        Func<IEnumerable<T>, bool> condition,
        T prepend)
    {
        if (condition(source))
            yield return prepend;
        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>
    /// Add a collection to the beginning of another collection, if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="prepend">The collection to prepend.</param>
    public static IEnumerable<T> PrependIf<T>(
        this IEnumerable<T> source,
        Func<IEnumerable<T>, bool> condition,
        IEnumerable<T> prepend)
    {
        if (condition(source))
        {
            foreach (T obj in prepend)
                yield return obj;
        }

        foreach (T obj in source)
            yield return obj;
    }

    /// <summary>Add an item to the end of a collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="append">Func to create the item to append.</param>
    public static IEnumerable<T> AppendWith<T>(
        this IEnumerable<T> source,
        Func<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        yield return append();
    }

    /// <summary>Add an item to the end of a collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="append">The item to append.</param>
    public static IEnumerable<T> AppendWith<T>(this IEnumerable<T> source, T append)
    {
        foreach (T obj in source)
            yield return obj;
        yield return append;
    }

    /// <summary>Add a collection to the end of another collection.</summary>
    /// <param name="source">The collection.</param>
    /// <param name="append">The collection to append.</param>
    public static IEnumerable<T> AppendWith<T>(
        this IEnumerable<T> source,
        IEnumerable<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        foreach (T obj in append)
            yield return obj;
    }

    /// <summary>
    /// Add an item to the end of a collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">Func to create the item to append.</param>
    public static IEnumerable<T> AppendIf<T>(
        this IEnumerable<T> source,
        bool condition,
        Func<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition)
            yield return append();
    }

    /// <summary>
    /// Add an item to the end of a collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">The item to append.</param>
    public static IEnumerable<T> AppendIf<T>(
        this IEnumerable<T> source,
        bool condition,
        T append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition)
            yield return append;
    }

    /// <summary>
    /// Add a collection to the end of another collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">The collection to append.</param>
    public static IEnumerable<T> AppendIf<T>(
        this IEnumerable<T> source,
        bool condition,
        IEnumerable<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition)
        {
            foreach (T obj in append)
                yield return obj;
        }
    }

    /// <summary>
    /// Add an item to the end of a collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">Func to create the item to append.</param>
    public static IEnumerable<T> AppendIf<T>(
        this IEnumerable<T> source,
        Func<bool> condition,
        Func<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition())
            yield return append();
    }

    /// <summary>
    /// Add an item to the end of a collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">The item to append.</param>
    public static IEnumerable<T> AppendIf<T>(
        this IEnumerable<T> source,
        Func<bool> condition,
        T append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition())
            yield return append;
    }

    /// <summary>
    /// Add a collection to the end of another collection if a condition is met.
    /// </summary>
    /// <param name="source">The collection.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="append">The collection to append.</param>
    public static IEnumerable<T> AppendIf<T>(
        this IEnumerable<T> source,
        Func<bool> condition,
        IEnumerable<T> append)
    {
        foreach (T obj in source)
            yield return obj;
        if (condition())
        {
            foreach (T obj in append)
                yield return obj;
        }
    }

    /// <summary>
    /// Returns and casts only the items of type <typeparamref name="T" />.
    /// </summary>
    /// <param name="source">The collection.</param>
    public static IEnumerable<T> FilterCast<T>(this IEnumerable source)
    {
        foreach (object obj1 in source)
        {
            if (obj1 is T obj)
                yield return obj;
        }
    }

    /// <summary>Adds a collection to a hashset.</summary>
    /// <param name="hashSet">The hashset.</param>
    /// <param name="range">The collection.</param>
    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
    {
        foreach (T obj in range)
            hashSet.Add(obj);
    }

    /// <summary>
    /// Returns <c>true</c> if the list is either null or empty. Otherwise <c>false</c>.
    /// </summary>
    /// <param name="list">The list.</param>
    public static bool IsNullOrEmpty<T>(this IList<T> list) => list == null || list.Count == 0;

    /// <summary>Sets all items in the list to the given value.</summary>
    /// <param name="list">The list.</param>
    /// <param name="item">The value.</param>
    public static void Populate<T>(this IList<T> list, T item)
    {
        int count = list.Count;
        for (int index = 0; index < count; ++index)
            list[index] = item;
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the IList&lt;T&gt;.
    /// </summary>
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
    {
        if (list is List<T>)
        {
            ((List<T>)list).AddRange(collection);
        }
        else
        {
            foreach (T obj in collection)
                list.Add(obj);
        }
    }

    /// <summary>Sorts an IList</summary>
    public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
    {
        if (list is List<T>)
        {
            ((List<T>)list).Sort(comparison);
        }
        else
        {
            List<T> objList = new List<T>((IEnumerable<T>)list);
            objList.Sort(comparison);
            for (int index = 0; index < list.Count; ++index)
                list[index] = objList[index];
        }
    }

    /// <summary>Sorts an IList</summary>
    public static void Sort<T>(this IList<T> list)
    {
        if (list is List<T>)
        {
            ((List<T>)list).Sort();
        }
        else
        {
            List<T> objList = new List<T>((IEnumerable<T>)list);
            objList.Sort();
            for (int index = 0; index < list.Count; ++index)
                list[index] = objList[index];
        }
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static IEnumerator Yield<T>(this IObservable<T> source, Action<T> callback)
    {
        source.Subscribe(callback);
        return source.ToYieldInstruction();
    }

    //πÈ∏∏∫–¿≤¿ª 0~1∞™¿∏∑Œ ∫Ø∞Ê
    public static float PPMToFloat(this Int64 src)
    {
        return src * 0.000001f;
    }

    public static float PPMToFloat(this UInt64 src)
    {
        return src * 0.000001f;
    }

    public static float PPMToFloat(this int src)
    {
        return src * 0.000001f;
    }

    public static int ToNumLength(this int number)
    {
        if (number == 0)
        {
            return 1;
        }

        return ((int)Math.Log10(number > 0L ? number : -number)) + 1;
    }

    public static void SetLayersRecursively(Transform trans, int layerIndex)
    {
        trans.gameObject.layer = layerIndex;
        foreach (Transform child in trans)
        {
            SetLayersRecursively(child, layerIndex);
        }
    }


    public static string ValueToString(this int value)
    {
        return String.Format("{0:#,##0}", value);
    }

    public static string ValueToString(this long value)
    {
        return String.Format("{0:#,##0}", value);
    }

    public static string ValueToString(this long value, string convertedUnit, long convertedValue = 1000000)
    {
        if (value < convertedValue)
            return String.Format("{0:#,##0}", value);

        switch (convertedUnit)
        {
            case "K":
            case "k":
                return String.Format("{0:#,##0} {1}", value / 1000, convertedUnit);
            default:
                return String.Format("{0:#,##0}", value);
        }
    }

    public static string ValueToString(this long value, Define.Notation convertedUnit, Define.DECIMALROUND decimalround = Define.DECIMALROUND.Round, long limitUnit = 1, string stringFormat = "N2")
    {
        switch (convertedUnit)
        {
            case Define.Notation.IsUnits:
                {
                    if (value >= limitUnit * 1000000)
                    {
                        return DecimalRound((double)value / 1000000, decimalround).ToString(stringFormat) + "M";
                        // return ((double)value / 1000000).ToString(stringFormat) + "M";
                    }
                    else if (value >= limitUnit * 1000)
                    {
                        return DecimalRound((double)value / 1000, decimalround).ToString(stringFormat) + "K";
                        // return ((double)value / 1000).ToString(stringFormat) + "K";
                    }
                    else
                    {
                        return String.Format("{0:#,##0}", value);
                    }
                }
            case Define.Notation.Percent:
                return DecimalRound((double)value / 10000, decimalround).ToString(stringFormat) + "%";
            default:
                return String.Format("{0:#,##0}", value);
        }
    }

    public static float DecimalRound(this float _float, Define.DECIMALROUND decimalround, int demicalPlaces = 2)
    {
        double _double = (double)_float;
        return (float)_double.DecimalRound(decimalround, demicalPlaces);
    }

    public static double DecimalRound(this double _double, Define.DECIMALROUND decimalround, int demicalPlaces = 2)
    {
        double demical = Math.Pow(10, demicalPlaces);
        switch (decimalround)
        {
            case Define.DECIMALROUND.Round:
                return Math.Round(_double, demicalPlaces);
            case Define.DECIMALROUND.RoundUp:
                return Math.Ceiling(_double * demical) / demical;
            case Define.DECIMALROUND.RoundDown:
                return Math.Floor(_double * demical) / demical;
            default:
                return _double;
        }
    }

    public static string ValueToString(this double value)
    {
        return String.Format("{0:#,#0.###}", value);
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    private static Random rng = new Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static int CountDbString(this string checkStr)
    {
        int ret = 0;
        for (int i = 0; i < checkStr.Length; i++)
        {
            var check = checkStr[i];

            if ((check >= 'a' && check <= 'z')
                || (check >= 'A' && check <= 'Z')
                || (check >= '0' && check <= '9')
                )
            {
                ret += 1;
            }
            else
                ret += 2;

        }
        return ret;
    }

    public static bool CheckSpecialChar(this string checkStr)
    {
        //{ // ¿œæÓ
        //    checkStr = Regex.Replace(checkStr, @"[™°-?´°-?]", "");
        //    checkStr = Regex.Replace(checkStr, @"[ÏÈ-?]", "");
        //    checkStr = Regex.Replace(checkStr, @"[?-?]", "");
        //    checkStr = Regex.Replace(checkStr, @"[?-À–]", "");
        //}

        checkStr = Regex.Replace(checkStr, @"[§°-§æ∞°-∆R]", "");
        checkStr = Regex.Replace(checkStr, @"[A-Za-z]", "");
        checkStr = Regex.Replace(checkStr, @"[0-9]", "");

        return checkStr.Length == 0;
    }

    /// <summary>
    /// value∞™¿« gameobject∏¶ destory «’¥œ¥Ÿ.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dics"></param>
    /// <param name="useDestroyImmediate"></param>
    public static void Destory<T1, T2>(this Dictionary<T1, T2> dics, bool useDestroyImmediate = false) where T2 : MonoBehaviour
    {
        var value = dics.Values;
        foreach (var item in value)
        {
            if (useDestroyImmediate)
            {
                GameObject.DestroyImmediate(item.gameObject);
            }
            else
            {
                GameObject.Destroy(item.gameObject);
            }
        }
    }

    /// <summary>
    /// Dictionary∞° Null¿Ã∏È ªı∑Œ ∏∏µÈ∞Ì Null¿Ã æ∆¥œ∏È Clear «’¥œ¥Ÿ.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dics"></param>
    public static void AdvClear<T1, T2>(this Dictionary<T1, T2> dics)
    {
        if (dics == null)
        {
            dics = new Dictionary<T1, T2>();
        }
        else
        {
            dics.Clear();
        }
    }

    public static void AddRange<T1, T2>(this Dictionary<T1, T2> dict, Dictionary<T1, T2> anotherDict)
    {
        if (dict == null)
        {
            dict = new Dictionary<T1, T2>();
        }
        foreach (var data in anotherDict)
        {
            dict.Add(data.Key, data.Value);
        }
    }

    /// <summary>
    /// Dictionary∞° Null¿Œ¡ˆ √º≈©«œ∞Ì Null¿Ã∏È ªı∑Œ∏∏µÏ¥œ¥Ÿ.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dics"></param>
    public static void AdvCheckNull<T1, T2>(this Dictionary<T1, T2> dics)
    {
        if (dics == null)
        {
            dics = new Dictionary<T1, T2>();
        }
    }

    public static bool IsNullOrEmpty<T1, T2>(this Dictionary<T1, T2> dics)
    {
        return dics == null || dics.Count == 0;
    }

    public static string Base64Encode(this string plainText)
    {
        return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
    }

    public static string Base64Decode(this string base64EncodedData)
    {
        return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(base64EncodedData));
    }

    public static T AddGameObject<T>(this List<T> list, T origin, Transform parent = null) where T : UnityEngine.Component
    {
        if (origin.gameObject != null)
        {
            T clone = parent == null ? UnityEngine.Object.Instantiate(origin) : UnityEngine.Object.Instantiate(origin, parent);
            list.Add(clone);
            return clone;
        }
        else
            return null;
    }

    public static T AddGameObject<T>(this List<T> list, T origin, GameObject parent = null) where T : UnityEngine.Component
    {
        if (origin.gameObject != null)
        {
            T clone = parent == null ? UnityEngine.Object.Instantiate(origin) : UnityEngine.Object.Instantiate(origin, parent.transform);
            list.Add(clone);
            return clone;
        }
        else
            return null;
    }

    /// <summary>
    /// default color = #ff4040 (Coral Red)
    /// </summary>
    /// <param name="_string"></param>
    /// <returns></returns>
    public static string ToColor(this string _string)
    {
        return $"<color=#ff4040>{_string}</color>";
    }
    public static string ToColor(this string _string, Color color)
    {
        // Color∏¶ Hex ƒ⁄µÂ πÆ¿⁄ø≠∑Œ ∫Ø»Ø
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        var _message = $"<color=#{hexColor}>{_string}</color>";
        return _message;
    }
}