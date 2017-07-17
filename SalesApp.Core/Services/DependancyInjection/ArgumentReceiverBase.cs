using System;
using System.Reflection;

namespace SalesApp.Core.Services.DependancyInjection
{
    public abstract class ArgumentReceiverBase
    {
        public void Receive(object args)
        {
            foreach (var sourcePropInfo in args.GetType().GetRuntimeProperties())
            {
                PropertyInfo targetPropertyInfo = this.GetType().GetRuntimeProperty(sourcePropInfo.Name);
                if (targetPropertyInfo.Equals(default(PropertyInfo)))
                {
                    throw new Exception(
                        string.Format(
                            "Cannot inject property {0} into type {1} as this type does not have a property with a matching name. (We care about case sensitivity)",
                            sourcePropInfo.Name, this.GetType().FullName));
                }

                targetPropertyInfo.SetValue(this,sourcePropInfo.GetValue(args));
            }
        }
    }
}