using System.Collections.Generic;
using SalesApp.Core.BL;

//using Android.App;
//using Android.Content;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;

namespace SalesApp.Core.DAL
{
    public class BroadcastMessageRepository
    {
        //private SQLiteDataService<Message> dataService;
        
        public BroadcastMessageRepository()
        {
            //StorageService androidStorageService = new StorageService();
            //ISQLitePlatform androidSQlitePlatform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();

            //dataService = new SQLiteDataService(androidSQlitePlatform, androidStorageService);
        }

        public IEnumerable<Message> GetItems()
        {
            //return Db.GetItems<T>();
            return null;
        }

        public void SaveItem(Message item)
        {
            //return Db.SaveItem<T>(item);
        }
    }
}