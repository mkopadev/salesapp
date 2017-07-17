using System.IO;
using Android.Content;
using SalesApp.Core.ViewModels.Modules.Facts;
using IOException = Java.IO.IOException;

namespace SalesApp.Droid.Views.Modules.Facts
{
    public class AndroidAssets : IAssets
    {
        private Context _context;

        public AndroidAssets(Context context)
        {
            this._context = context;
        }

        public string GetAssetAsString(string assetFileName)
        {
            try
            {
                using (StreamReader sr = new StreamReader(this._context.Assets.Open(assetFileName)))
                {
                    var factJson = sr.ReadToEnd();

                    return factJson;
                } 
            }
            catch (IOException ex)
            {
                return null;
            }
        }
    }
}