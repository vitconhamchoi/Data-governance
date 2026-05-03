using DataGovernance.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataGovernance.Infrastructure.Data.Repositories;

/// <summary>
/// Unit of Work implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DataGovernanceDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(DataGovernanceDbContext context)
    {
        _context = context;
        DataAssets = new DataAssetRepository(_context);
    }

    public IDataAssetRepository DataAssets { get; }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
