using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.Services.Interfaces
{
    public interface IDataService<T> where T : BusinessEntityBase
    {
        Task<SaveResponse<T>> SaveAsync(T model);

        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(Guid id);

        Task<int> DeleteAsync(Guid id);

        Task<int> DeleteAsync(T model);

        Task<T> GetSingleByCriteria(CriteriaBuilder criteriaBuilder);

        Task<List<T>> GetManyByCriteria(CriteriaBuilder criteriaBuilder, [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
    }
}
