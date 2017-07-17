using System;
using System.Diagnostics;
using System.IO;
using Android.Content;
using SalesApp.Core.Services.Interfaces;

namespace SalesApp.Droid.Services
{
    public class StorageService : IStorageService
    {
        public StorageService()
        {

        }

        private readonly Context _context;
        public StorageService(Context context)
        {
            this._context = context;
        }
        
        public string GetPathForFileAsync(string file)
        {
            string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            var path = Path.Combine(libraryPath, file);

            Debug.WriteLine("Android Database path is " + path);

            return path;
        }

        
        

        
    }
}