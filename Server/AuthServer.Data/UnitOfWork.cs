using AuthServer.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext  _dbContext;

    public UnitOfWork(AppDbContext appDbContext)
    {
        _dbContext = appDbContext;
    }

    public void Commit()
    {
        _dbContext.SaveChanges();
    }
    public async Task CommitAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
