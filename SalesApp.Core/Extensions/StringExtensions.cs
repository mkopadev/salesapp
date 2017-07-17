using System;

namespace SalesApp.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool AreEqual(this string str,string testString)
        {
            return string.Equals(str,testString,StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsBlank(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Returns the value of an enum that matches a given names
        /// </summary>
        /// <typeparam name="T">The type of the enum</typeparam>
        /// <param name="name">The name of the enum's value</param>
        /// <returns>The value of an enum that matches the given name</returns>
        /// <exception cref="InvalidOperationException">Throws an InvalidOperationException if the name does not exist in the enum's set of values</exception>
        public static T ToEnumValue<T>(this string name)
        {
            return (T)ToEnumValue(name, typeof(T));
        }

        public static TimeSpan ToTimeSpan(this string val, TimeSpan defaultVal)
        {
            TimeSpan ts;
            if (!TimeSpan.TryParse(val, out ts))
            {
                return defaultVal;
            }

            return ts;
        }

        public static object ToEnumValue(this string name, Type t)
        {
            var vals = Enum.GetValues(t);
            foreach (var value in vals)
            {
                if (Enum.GetName(t, value).AreEqual(name))
                {
                    return (int)value;
                }
            }

            return 0;
        }

        public static T ToEnum<T>(this string val)
        {
            return (T)Enum.Parse(typeof(T), val);
        }

        /// <summary>
        /// Extension shorthand for string.Format(str, args);
        /// </summary>
        /// <param name="str">String to inject args</param>
        /// <param name="args">Args to inject</param>
        /// <returns>String with arguments injected</returns>
        public static string Formatted(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        [Obsolete("Use renamed and safer: Formatted")]
        public static string GetFormated(this string str, params object[] args)
        {
            try
            {
                string[] bits = str.Split(new char[] {'~'});
                string joined = "";
                for (int i = 0; i < bits.Length - 1; i++)
                {
                    joined += bits[i] + args[i];
                }
                return joined + bits[bits.Length - 1];
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string ToTitleCase(this string str)
        {
            if (str.IsBlank())
            {
                return str;
            }
            if (str.Length == 1)
            {
                return str.ToUpper();
            }
            return string.Format
                (
                    "{0}{1}"
                    , str.Substring(0, 1).ToUpper()
                    , str.Substring(1).ToLower()
                );
        }

        public static string StripFileExtension(this string fileName)
        {
            if (fileName == null)
            {
                return null;
            }

            int lastIndexOfDot = fileName.LastIndexOf(".", StringComparison.Ordinal);

            if (lastIndexOfDot == -1)
            {
                return fileName;
            }

            return fileName.Substring(0, lastIndexOfDot);
        }
    }
}
