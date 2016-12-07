using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace Site
{
    public static class Ext
    {
        public static bool Is(this string txt, string txt2)
        {
            return String.Equals(txt, txt2, StringComparison.CurrentCultureIgnoreCase);
        }
        public static bool Is(this Object one, Object two)
        {
            return Is(one == null ? String.Empty : one.ToString(), two == null ? String.Empty : two.ToString());
        }

        public static bool In(this String one, params String[] two)
        {
            return two.Any(one.Is);
        }
        public static bool Contains(this String txt, params string[] arr)
        {
            if (!txt.HasValue() || arr.Length == 0) return false;
            return arr.Any(txt.Contains);
        }

        public static bool HasValue(this object obj)
        {
            if (obj == null) return false;
            if (obj is String) return HasValue(obj as String);
            return true;
        }
        public static bool HasValue(this DateTime date)
        {
            return date != DateTime.MinValue;
        }
        public static bool HasValue(this DateTime? date)
        {
            return date.HasValue && date != DateTime.MinValue;
        }
        public static bool HasValue(this String txt)
        {
            return !String.IsNullOrWhiteSpace(txt);
        }

        public static string Or(this String first, params string[] next)
        {
            if (first.HasValue()) return first;
            foreach (var item in next)
            {
                if (item.HasValue()) return item;
            }
            return String.Empty;
        }
        public static T Or<T>(this T first, T second, params T[] next)
        {
            if (first != null) return first;
            if (second != null) return second;
            foreach (var item in next)
            {
                if (item != null) return item;
            }
            return Activator.CreateInstance<T>();
        }
        public static string Or(this Object obj, params object[] vals)
        {
            if (obj != null && obj.ToString().HasValue()) return obj.ToString();
            foreach (var v in vals)
            {
                if (v != null && v.ToString().HasValue()) return v.ToString();
            }
            return "";
        }

        public static List<T> Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items == null)
                return new List<T>();

            var list = items.ToList();
            list.ForEach(action);

            return list;
        }

        public static KeyValuePair<K, V> KeyValuePair<K, V>(K key, V value)
        {
            return new KeyValuePair<K, V>(key, value);
        }

        public static Dictionary<K, V> ToDict<K, V>(this IEnumerable<V> items, Func<V, K> keySelector, string dupMsg = "Duplicate key '{0}'")
        {
            var dict = new Dictionary<K, V>();
            foreach (var item in items)
            {
                var key = keySelector(item);
                try
                {
                    dict.Add(key, item);
                }
                catch (ArgumentException)
                {
                    throw new ApplicationException(string.Format(dupMsg, key));
                }
            }
            return dict;
        }

        public static Dictionary<K, I> ToDict<K, V, I>(this IEnumerable<V> items, Func<V, K> keySelector, Func<V, I> itemSelector, string dupMsg = "Duplicate key '{0}'")
        {
            var dict = new Dictionary<K, I>();
            foreach (var item in items)
            {
                var key = keySelector(item);
                try
                {
                    dict.Add(key, itemSelector(item));
                }
                catch (ArgumentException)
                {
                    throw new ApplicationException(string.Format(dupMsg, key));
                }
            }
            return dict;
        }

        public static V Get<K, V>(this IDictionary<K, V> dict, K key, V def = default(V), bool add = false)
        {
            if (dict == null || key == null)
            {
                return def;
            }
            else if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            else
            {
                if (add) dict[key] = def;
                return def;
            }
        }

        public static IDictionary<TKey, TValue> Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            dict[key] = value;
            return dict;
        }

        public static List<Type> ImplementorsOf<T>(IEnumerable<Assembly> assemblies = null)
        {
            var interfaceType = typeof(T);

            if (assemblies == null)
                assemblies = new[] { interfaceType.Assembly };

            return assemblies.SelectMany(x => x.GetTypes())
                             .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface)
                             .ToList();
        }

        public static string ToString(this DateTime? date, string format)
        {
            return date.HasValue && date != DateTime.MinValue ? date.Value.ToString(format) : String.Empty;
        }
        public static String ToDateString(this DateTime date)
        {
            return date != DateTime.MinValue ? date.ToString("M/d/yyyy") : string.Empty;
        }
        public static String ToDateString(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToDateString() : string.Empty;
        }
        public static String ToDateTimeString(this DateTime date)
        {
            return date != DateTime.MinValue ? date.ToString("M/d/yyyy h:mm tt") : string.Empty;
        }
        public static String ToDateTimeString(this DateTime? date)
        {
            return date.HasValue && date != DateTime.MinValue ? date.Value.ToDateTimeString() : string.Empty;
        }

        //REF: http://www.yoda.arachsys.com/csharp/readbinary.html
        public static byte[] ReadAllBytes(this System.IO.Stream stream)
        {
            if (stream == null)
                return null;

            if (stream is System.IO.MemoryStream)
                return (stream as System.IO.MemoryStream).ToArray();

            var buffer = new byte[32768];
            using (var ms = new System.IO.MemoryStream())
            {
                while (true)
                {
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0) return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public static int[] UpTo(this int start, int end, int step = 1)
        {
            var list = new List<int>();
            for (var i = start; i <= end; i += step)
            {
                list.Add(i);
            }
            return list.ToArray();
        }
        public static int[] DownTo(this int start, int end)
        {
            var list = new List<int>();
            for (var i = start; i >= end; i -= 1)
            {
                list.Add(i);
            }
            return list.ToArray();
        }

        public static bool CanConvertFrom<T>(this PropertyInfo prop)
        {
            return CanConvertFrom<T>(prop.PropertyType);
        }
        public static bool CanConvertFrom<T>(this Type type)
        {
            return CanConvertFrom(type, typeof(T));
        }
        public static bool CanConvertFrom(this Type to, Type from)
        {
            return TypeDescriptor.GetConverter(to).CanConvertFrom(from);
        }

        public static bool IsSimpleType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType)
            {
                type = Nullable.GetUnderlyingType(type);
                if (type == null) return false;
            }

            if (type.IsPrimitive)
                return true;

            if (type == typeof(string))
                return true;

            if (type == typeof(DateTime))
                return true;

            if (type == typeof(decimal))
                return true;

            return false;
        }

        public static T Get<T>(this Object obj, PropertyInfo prop)
        {
            return prop.GetValue(obj, null).To<T>();
        }
        public static T Get<T>(this Object obj, String name)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            if (obj == null) return default(T);
            var prop = obj.GetType().GetProperty(name, flags);
            if (prop != null) return prop.GetValue(obj, null).To<T>();
            var field = obj.GetType().GetField(name, flags);
            if (field != null) return field.GetValue(obj).To<T>();
            return default(T);
        }
        public static Object Get(this Object obj, String name)
        {
            if (obj == null) return null;
            var prop = obj.GetType().GetProperty(name);
            if (prop == null) throw new Exception(name + " property not found");
            return Get(obj, prop);
        }
        public static Object Get(this Object obj, PropertyInfo prop)
        {
            return prop.GetValue(obj, null);
        }

        public static void Set(this Object obj, string name, object value)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var prop = obj.GetType().GetProperty(name, flags);
            if (prop != null)
            {
                prop.SetValue(obj, value.To(prop.PropertyType), null);
                return;
            }
            var field = obj.GetType().GetField(name, flags);
            if (field != null)
            {
                field.SetValue(obj, value.To(field.FieldType));
                return;
            }
        }
        public static void Set(this Object obj, PropertyInfo prop, object value)
        {
            if (prop != null)
            {
                if (prop.PropertyType.IsPrimitive && value == null) return;
                prop.SetValue(obj, value.To(prop.PropertyType), null);
            }
        }
        public static void Set(this Object obj, FieldInfo field, object value)
        {
            if (field != null)
            {
                field.SetValue(obj, value.To(field.FieldType));
            }
        }

        public static T To<T>(this Object obj)
        {
            return (T)To(obj, typeof(T), null);
        }
        public static T To<T>(this Object obj, T def)
        {
            return (T)To(obj, typeof(T), def);
        }
        public static Object To(this Object obj, Type type)
        {
            return To(obj, type, null);
        }
        public static Object To(this Object obj, Type type, Object def)
        {
            if (obj != null && obj.GetType() == type) return obj;
            if (obj is DBNull) obj = null;
            if (type.IsGenericType)
            {
                if (obj == null || (obj is String && obj.Equals(String.Empty))) return def;
                var nullableConverter = new NullableConverter(type);
                type = nullableConverter.UnderlyingType;
            }
            else if (!type.IsClass && (obj == null || (obj is String && obj.ToString() == String.Empty)))
            {
                return def;
            }
            if (obj is String && type.FullName.Contains("Boolean"))
            {
                if ((obj as String).HasValue() && obj.ToString().Contains("True", "true", "on", "ON")) return true; else return false;
            }
            else if (obj != null && type == typeof(int))
            {
                int result;
                return int.TryParse(obj.ToString().Replace(",", ""), out result) ? result : def;
            }
            else if (obj is String && (string)obj != null && (obj as String).Contains("$"))
            {
                return Convert.ChangeType(obj.ToString().Replace("$", ""), type);
            }
            else if (obj is DateTime && type.FullName.Contains("String"))
            {
                if ((DateTime)obj == DateTime.MinValue) return null;
            }
            else if (type.FullName.Contains("DateTime") && obj is String && obj != null)
            {
                if (obj.ToString().HasValue() && !IsDate(obj.ToString())) throw new FormatException("Invalid date '" + obj + "', please enter the format MM/DD/YYYY"); // return Activator.CreateInstance(type);
            }
            else if (obj != null && type.IsInstanceOfType(obj))
            {
                return obj;
            }
            try
            {
                return Convert.ChangeType(obj, type);
            }
            catch (FormatException ex)
            {
                throw new FormatException("Cannot convert '{0}' to '{1}'".format(obj, type), ex);
            }
        }

        public static String format(this String fmt, params Object[] args)
        {
            return args.Length == 0 ? fmt : String.Format(fmt, args);
        }

        public static bool IsDate(Object date)
        {
            if (date == null) return false;
            DateTime dt;
            if (!DateTime.TryParse(date.ToString(), out dt)) return false;
            if ((dt < DateTime.Today.AddYears(-100)) || (dt > DateTime.Today.AddYears(50))) return false;
            return true;
        }
        public static bool IsNumeric(Object num)
        {
            if (num == null) return false;
            return Microsoft.VisualBasic.Information.IsNumeric(num);
        }

        public static void Error(this ViewDataDictionary data, Exception ex)
        {
            Error(data, ex.Message);
        }
        public static void Error(this ViewDataDictionary data, string error)
        {
            data["ErrorMessage"] = error;
        }

        //REF: http://stackoverflow.com/questions/323314/best-way-to-convert-pascal-case-to-a-sentence
        public static string DeWiki(this string str, bool lower = false)
        {
            if (!str.HasValue()) return str;
            var retVal = new StringBuilder(32);
            retVal.Append(char.ToUpper(str[0]));
            for (var i = 1; i < str.Length; i++)
            {
                if (char.IsLower(str[i]) || (!lower && i > 0 && char.IsUpper(str[i])) && char.IsUpper(str[i - 1]))
                {
                    retVal.Append(str[i]);
                }
                else
                {
                    retVal.Append(" ");
                    retVal.Append(lower ? char.ToLower(str[i]) : str[i]);
                }
            }
            return retVal.ToString();
        }
    };
}
