namespace PencilCase.Shared.Data.Database;

public interface IDal<T> where T : class
{
    public IEnumerable<T> GetAll();
    public IEnumerable<T> GetAllBy(Func<T, bool> criteria);
    public T? GetBy(Func<T, bool> criteria);
    public Task Add(T item);
    public Task Update(T item);
    public Task Delete(T item);
}