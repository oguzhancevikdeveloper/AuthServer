﻿using AuthServer.Shared.Dtos;
using System.Linq.Expressions;

namespace AuthServer.Core.Services;

public interface IGenericService<TEntity, TDto> where TEntity : class where TDto : class
{
    Task<Response<TDto>> AddAsync(TDto dto);
    Task<Response<NoDataDto>> Update(TDto dto,int id);
    Task<Response<NoDataDto>> Remove(int id);

    Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate);
    Task<Response<TDto>> GetByIdAsync(int id);
    Task<Response<IEnumerable<TDto>>> GetAllAsync();
}
