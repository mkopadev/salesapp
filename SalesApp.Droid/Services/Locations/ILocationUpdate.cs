namespace SalesApp.Droid.Services.Locations
{
    public interface ILocationUpdate
    {
        void StartLocationUpdates(LocationUpdateType type);

        string GetLocation();
    }
}