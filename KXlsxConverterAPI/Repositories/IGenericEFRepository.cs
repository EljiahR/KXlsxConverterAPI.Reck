namespace KXlsxConverterAPI.Repositories
{
    public interface IGenericEFRepository<T>
    {
        T GetById(int id);
        IEnumerable<T> GetAll();
        void Add(T entity);
        void Delete(T entity);
        void Update(T entity);
        void SaveChanges();
    }
}
