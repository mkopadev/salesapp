namespace SalesApp.Core.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Select next item in the array, based upon value given. Selects the first subsequent item when current is found.
        /// </summary>
        /// <param name="arr">Array to search</param>
        /// <param name="current">Current item to search for</param>
        /// <returns>Next item in the array if present, otherwise null</returns>
        public static string SelectNextItem(this string[] arr, string current)
        {
            // iterate through all items
            for (var x = 0; x < arr.Length; x++)
            {
                // if the current item is found and there is still a next item
                if (arr[x].Equals(current) && x < arr.Length - 1)
                {
                    return arr[x + 1];
                }
            }

            // no other item found
            return null;
        }

        /// <summary>
        /// Select previous item in the array, based upon value given. Selects the first previous item when current is found.
        /// </summary>
        /// <param name="arr">Array to search</param>
        /// <param name="current">Current item to search for</param>
        /// <returns>Previous item in the array if present, otherwise null</returns>
        public static string SelectPreviousItem(this string[] arr, string current)
        {
            // iterate through all items
            for (var x = 0; x < arr.Length; x++)
            {
                // if the current item is found and there is already a previous item
                if (arr[x].Equals(current) && x > 0)
                {
                    return arr[x - 1];
                }
            }

            // no other item found
            return null;
        }
    }
}
