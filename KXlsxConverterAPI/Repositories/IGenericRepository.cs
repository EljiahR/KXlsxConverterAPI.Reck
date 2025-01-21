namespace KXlsxConverterAPI.Repositories;

public interface IGenericRepository<TEntity>
{
    Task<TEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task AddAsync(TEntity entity);
    Task AddAllAsync(IEnumerable<TEntity> entities);
    void Delete(TEntity entity);
    void DeleteAll();
    void Update(TEntity entity);
}
