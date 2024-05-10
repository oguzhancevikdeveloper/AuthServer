using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AuthServer.Service.Services;

public class GenericService<TEntity, TDto> : IGenericService<TEntity, TDto> where TEntity : class where TDto : class
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<TEntity> _genericRepository;

    public GenericService(IUnitOfWork unitOfWork, IGenericRepository<TEntity> genericRepository)
    {
        _unitOfWork = unitOfWork;
        _genericRepository = genericRepository;
    }

    public async Task<Response<TDto>> AddAsync(TDto dto)
    {
        var newEntity = ObjectMapper.Mapper.Map<TEntity>(dto);
        await _genericRepository.AddAsync(newEntity);
        await _unitOfWork.CommitAsync();
        var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity);
        return Response<TDto>.Success(newDto, 200);
    }
    public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
    {
        var entityList = ObjectMapper.Mapper.Map<List<TDto>>(await _genericRepository.GetAllAsync());
        return Response<IEnumerable<TDto>>.Success(entityList, 200);
    }
    public async Task<Response<TDto>> GetByIdAsync(int id)
    {
        var entity = ObjectMapper.Mapper.Map<TDto>(await _genericRepository.GetByIdAsync(id));
        if (entity == null) return Response<TDto>.Fail("Id not found", 200, true);
        return Response<TDto>.Success(entity, 200);
    }
    public async Task<Response<NoDataDto>> Remove(int id)
    {
        var entity = await _genericRepository.GetByIdAsync(id);
        if (entity == null) return Response<NoDataDto>.Fail("Id Not Found", 401, true);
        _genericRepository.Remove(entity);
        await _unitOfWork.CommitAsync();
        return Response<NoDataDto>.Success(204);
    }

    public async Task<Response<NoDataDto>> Update(TDto dto, int id)
    {
        var entity = await _genericRepository.GetByIdAsync(id);
        if (entity == null) return Response<NoDataDto>.Fail("Id Not Found", 404, true);
        _genericRepository.Update(ObjectMapper.Mapper.Map<TEntity>(dto));
        await _unitOfWork.CommitAsync();
        return Response<NoDataDto>.Success(204);
    }
    public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
    {
        var entityList = _genericRepository.Where(predicate);
        return Response<IEnumerable<TDto>>.Success(ObjectMapper.Mapper.Map<IEnumerable<TDto>>(await entityList.ToListAsync()), 200);
    }
}
