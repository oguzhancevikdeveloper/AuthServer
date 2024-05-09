using AuthServer.Shared.Dtos;
using System.Linq.Expressions;

namespace AuthServer.Core.Services;

public interface IServiceGeneric<TEntity, TDto> where TEntity : class where TDto : class
{
    Task<Response<TDto>> AddAsync(TEntity entity);
    Task<Response<NoDataDto>> Update(TEntity entity);
    Task<Response<NoDataDto>> Remove(TEntity entity);

    Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate);
    Task<Response<TDto>> GetByIdAsync(int id);
    Task<Response<TDto>> GetAllAsync();
}
