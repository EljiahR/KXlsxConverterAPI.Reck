namespace KXlsxConverterAPI.Repositories;

public interface IGenericEFRepository<TEntity>
{
    TEntity GetById(int id);
    IEnumerable<TEntity> GetAll();
    void Add(TEntity entity);
    void Delete(TEntity entity);
    void Update(TEntity entity);
    void SaveChanges();
}
