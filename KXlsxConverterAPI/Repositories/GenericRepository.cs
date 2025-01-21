using Microsoft.EntityFrameworkCore;

namespace KXlsxConverterAPI.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }
    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        SaveChanges();
    }
    public async Task AddAllAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        SaveChanges();
    }
    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
        SaveChanges();
    }

    public void DeleteAll()
    {
        _context.RemoveRange(_dbSet);
        SaveChanges();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
        SaveChanges();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
