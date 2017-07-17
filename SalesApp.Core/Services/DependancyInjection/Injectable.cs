namespace SalesApp.Core.Services.DependancyInjection
{
    public abstract class Injectable
    {
        public abstract void Setup(params object[] args);

        /// <summary>
        /// Returns the FIRST object that matches the specified type.
        /// If you have a multiple arguments of the same type, this method will fail as it will return the same value for all arguments of that type.
        /// </summary>
        /// <typeparam name="TReturn">The type to return</typeparam>
        /// <param name="args">Array of arguments</param>
        /// <returns>Returns the FIRST object that matches the specified type. If no matching object is found, it returns null</returns>
        public TReturn GetArg<TReturn>(object[] args) where TReturn : class 
        {
            foreach (var obj in args)
            {
                TReturn result = obj as TReturn;
                if (result != null)
                {
                    return result;
                }

            }
            return null;
        }
    }
}