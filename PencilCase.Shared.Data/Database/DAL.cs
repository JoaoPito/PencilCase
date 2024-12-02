using System;
using Microsoft.EntityFrameworkCore;

namespace PencilCase.Shared.Data.Database;

public class DAL<T> where T: class
{
    protected readonly DbContext Context;

    protected DAL(DbContext context)
    {
        this.Context = context;
    }

    public IEnumerable<T> GetAll()
    {
        return Context.Set<T>().ToList();
    }

    public IEnumerable<T> GetAllBy(Func<T, bool> criteria)
    {
        return Context.Set<T>().Where(criteria).ToList();
    }

    public T? GetBy(Func<T, bool> criteria)
    {
        return Context.Set<T>().FirstOrDefault(criteria);
    }

    public async Task Add(T item)
    {
        Context.Add(item);
        await Context.SaveChangesAsync();
    }

    public async Task Update(T item)
    {
        Context.Update(item);
        await Context.SaveChangesAsync();
    }

    public async Task Delete(T item)
    {
        Context.Remove(item);
        await Context.SaveChangesAsync();
    }
}
