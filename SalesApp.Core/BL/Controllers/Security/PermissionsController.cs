using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Security;
using SalesApp.Core.Enums.Security;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers.Security
{
    public class PermissionsController : SQLiteDataService<Permission>
    {
        // private List<Permission> _permissions;
        private List<Permission> _cachedPermissions;

        public static PermissionsController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PermissionsController();
                }

                return _instance;
            }
        }

        private async Task<List<Permission>> GetCachedPermissions()
        {
            if (this._cachedPermissions == null)
            {
                this._cachedPermissions = await this.GetAllAsync();
            }

            return this._cachedPermissions;
        }

        private static PermissionsController _instance;

        private PermissionsController()
        {
        }

        /*async Task SaveNext(SQLiteConnection connTran, int index)
        {
            try
            {
                if (index >= _permissions.Count)
                {
                    return;
                }

                var savedPermission = await SaveAsync(connTran, _permissions[index]);
                Logger.Debug("Saved permission's id is ~".GetFormated(savedPermission.SavedModel.Id));
                index++;
                await SaveNext(connTran, index);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        private static readonly object Locker = new object();*/

        public async Task<bool> UpdatePermissionsAsync(List<Permission> permissions)
        {
            try
            {
                if (permissions == null)
                {
                    return true;
                }

                // _permissions = permissions;
                await DataAccess.Instance.Connection.RunInTransactionAsync(
                        async connTran =>
                        {
                            DataAccess.Instance.StartTransaction(connTran);
                            this.Logger.Debug("Deleting previous permissions");
                            connTran.DeleteAll<Permission>();
                            foreach (var p in permissions)
                            {
                                await this.SaveAsync(connTran, p);
                            }
                        });

                DataAccess.Instance.CommitTransaction();
                await this.RebuildCache();

                return true;
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
                throw;
            }
        }

        public async Task<bool> Allowed(int permissionToTest)
        {
            switch (permissionToTest)
            {
                case (int)Permissions.ScreenAppHome:
                case (int)Permissions.ActionHamburgerLogo:
                    return true;
            }

            List<Permission> allPermissions = await this.GetCachedPermissions();
            return allPermissions.Any(permission => permission.PermissionId == permissionToTest);
        }

        public async Task<bool> Allowed(Permissions permissionToTest)
        {
            return await this.Allowed((int)permissionToTest);
        }

        public async Task<bool> Allowed(Permissions[] permissionsToTest)
        {
            foreach (Permissions permission in permissionsToTest)
            {
                if (await this.Allowed(permission))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task RebuildCache()
        {
            _cachedPermissions = null;
            await this.GetCachedPermissions();
        }
    }
}