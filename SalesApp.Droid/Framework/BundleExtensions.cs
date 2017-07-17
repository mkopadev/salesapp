using Android.OS;
using Newtonsoft.Json;
using SalesApp.Core.Extensions;

namespace SalesApp.Droid.Framework
{
    public static class BundleExtensions
    {
        /// <summary>
        /// Retrieves an object from the bundle which previously has been stored using Json.
        /// </summary>
        /// <typeparam name="T">Type to return</typeparam>
        /// <param name="bundle">Bundle to check</param>
        /// <param name="key">Key to look up in the bundle</param>
        /// <returns>Object Deserialized from the bundle</returns>
        public static T GetJsonObject<T>(this Bundle bundle, string key)
        {
            if (bundle == null)
            {
                return default(T);
            }

            if (!bundle.ContainsKey(key))
            {
                return default(T);
            }

            string json = bundle.GetString(key);

            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Stores an complex object as Json string in the bundle.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="bundle">Bundle to store the object in</param>
        /// <param name="key">Key to store the object under</param>
        /// <param name="obj">Object to store</param>
        public static void PutJsonObject<T>(this Bundle bundle, string key, T obj)
        {
            if (bundle == null)
            {
                bundle = new Bundle();
            }

            bundle.PutString(key, JsonConvert.SerializeObject(obj));
        }

        /// <summary>
        /// Retrieves a Enum from the bundle (stored a string).
        /// </summary>
        /// <typeparam name="T">enum type</typeparam>
        /// <param name="bundle">Bundle to retrieve from</param>
        /// <param name="key">Key enum stored under</param>
        /// <returns>Enum value if present otherwise default(T)</returns>
        public static T GetEnum<T>(this Bundle bundle, string key)
        {
            var str = bundle.GetString(key);
            if (str == null)
            {
                return default(T);
            }

            return str.ToEnum<T>();
        }
    }
}