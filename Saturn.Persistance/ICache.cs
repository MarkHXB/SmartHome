namespace Saturn.Persistance
{
    public interface ICache<T>
    {
        Task Save(T entity);
        Task<T> Load(T entity);
    }
}
