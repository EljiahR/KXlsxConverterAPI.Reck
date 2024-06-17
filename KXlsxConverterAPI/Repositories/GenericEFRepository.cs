using Microsoft.EntityFrameworkCore;

namespace KXlsxConverterAPI.Repositories;

public class GenericEFRepository<TEntity, TContext> : IGenericEFRepository<TEntity>
    where TEntity : class
    where TContext : DbContext
{
    private readonly TContext _context;
    private readonly DbSet<TEntity> _dbSet;
    public GenericEFRepository(TContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }
    public void Add(TEntity entity)
    {
        _dbSet.Add(entity);
        SaveChanges();
    }

    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public IEnumerable<TEntity> GetAll()
    {
        return _dbSet.ToList();
    }

    public TEntity GetById(int id)
    {
        return _dbSet.Find(id);
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
        SaveChanges();
    }
}
