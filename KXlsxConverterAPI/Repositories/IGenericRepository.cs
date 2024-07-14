namespace KXlsxConverterAPI.Repositories;

public interface IGenericRepository<TEntity>
{
    TEntity? GetById(int id);
    IEnumerable<TEntity> GetAll();
    void Add(TEntity entity);
    void AddAll(IEnumerable<TEntity> entities);
    void Delete(TEntity entity);
    void Update(TEntity entity);
    void SaveChanges();
}
