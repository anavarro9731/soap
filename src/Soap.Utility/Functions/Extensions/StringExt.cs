namespace Soap.Utility.Functions.Extensions
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;

    public static class StringExt
    {
        
        public static T FromJson<T>(this string json, SerialiserIds serialiserId, string actualSerialisedTypeWhenDifferentFromT = null) 
        {
            var hasUnderlyingType = !string.IsNullOrEmpty(actualSerialisedTypeWhenDifferentFromT); 
            
            var obj = serialiserId switch
            {
                var x when x == SerialiserIds.JsonDotNetDefault  => hasUnderlyingType ? (T)JsonConvert.DeserializeObject(json, Type.GetType(actualSerialisedTypeWhenDifferentFromT)): JsonConvert.DeserializeObject<T>(json),
                var x when x == SerialiserIds.ClientSideMessageSchemaGeneraton => hasUnderlyingType ? (T)JsonConvert.DeserializeObject(json, Type.GetType(actualSerialisedTypeWhenDifferentFromT), JsonNetSettings.MessageSchemaSerialiserSettings) : JsonConvert.DeserializeObject<T>(json, JsonNetSettings.MessageSchemaSerialiserSettings),
                var x when x == SerialiserIds.ApiBusMessage => hasUnderlyingType ? (T)JsonConvert.DeserializeObject(json, Type.GetType(actualSerialisedTypeWhenDifferentFromT), JsonNetSettings.ApiMessageSerialiserSettings) : JsonConvert.DeserializeObject<T>(json, JsonNetSettings.ApiMessageSerialiserSettings),
                _ => throw new ApplicationException($"Serialiser Id Not Found. Valid values are {SerialiserIds.GetAllInstances().Select(x => x.Key).Aggregate((x,y) => $"{x},{y}")}")
            };

            return obj;
        }
        
        
        
        public static string ToCamelCase(this string source)
        {
            return Char.ToLowerInvariant(source[0]) + source.Substring(1);
        }
        
        /// <summary>
        ///     Trims a full string (rather than an array of possible characters) from the start of a string
        /// </summary>
        public static string TrimStart(this string source, string value, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            int valueLength = value.Length;
            int startIndex = 0;
            while (source.IndexOf(value, startIndex, comparisonType) == startIndex)
            {
                startIndex += valueLength;
            }

            return source.Substring(startIndex);
        }

        /// <summary>
        ///     Trims a full string (rather than an array of possible characters) from the end of a string
        /// </summary>
        public static string TrimEnd(this string source, string value, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            int sourceLength = source.Length;
            int valueLength = value.Length;
            int count = sourceLength;
            while (source.LastIndexOf(value, count, comparisonType) == count - valueLength)
            {
                count -= valueLength;
            }

            return source.Substring(0, count);
        }
        
        public static string AppendTimestamp(string value) => $"{value}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        /// <summary>
        ///     Return the substring after to but not including the first instance of 'c'.
        ///     If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringAfter(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length - 1, src.IndexOf(c) + 1);
            if (idx < 0) return string.Empty;
            return src.Substring(idx);
        }

        /// <summary>
        ///     Return the substring after to but not including the first instance of 'c'.
        ///     If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringAfter(this string src, string s)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length - 1, src.IndexOf(s) + s.Length);
            if (idx < 0) return string.Empty;
            return src.Substring(idx);
        }

        /// <summary>
        ///     Return the substring after to but not including the last instance of 'c'.
        ///     If 'c' is not found, an empty string is returned.
        /// </summary>
        public static string SubstringAfterLast(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;
            var fidx = src.LastIndexOf(c);
            if (fidx < 0) return string.Empty;

            var idx = Math.Min(src.Length - 1, fidx + 1);
            return src.Substring(idx);
        }

        public static string SubstringAfterLast(this string src, string s)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;
            var fidx = src.LastIndexOf(s);
            if (fidx < 0) return string.Empty;

            var idx = Math.Min(src.Length - 1, fidx + s.Length);
            return src.Substring(idx);
        }

        /// <summary>
        ///     Return the substring up to but not including the first instance of 'c'.
        ///     If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringBefore(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length, src.IndexOf(c));
            if (idx < 0) return src;
            return src.Substring(0, idx);
        }

        public static string SubstringBefore(this string src, string s)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length, src.IndexOf(s));
            if (idx < 0) return src;
            return src.Substring(0, idx);
        }

        /// <summary>
        ///     Return the substring up to but not including the last instance of 'c'.
        ///     If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringBeforeLast(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length, src.LastIndexOf(c));
            if (idx < 0) return src;
            return src.Substring(0, idx);
        }

        public static string SubstringBeforeLast(this string src, string s)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length, src.LastIndexOf(s));
            if (idx < 0) return src;
            return src.Substring(0, idx);
        }
    }
}
