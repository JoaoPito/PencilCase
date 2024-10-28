using System;
using Microsoft.EntityFrameworkCore;

namespace PencilCase.Shared.Data.Database;

public class DAL<T> where T: class
{
    private readonly BlocksDbContext _context;
    public DAL(BlocksDbContext context)
    {
        this._context = context;
    }

    public IEnumerable<T> GetAll()
    {
        return _context.Set<T>().ToList();
    }

    public IEnumerable<T> GetAllBy(Func<T, bool> criteria)
    {
        return _context.Set<T>().Where(criteria).ToList();
    }

    public T? GetBy(Func<T, bool> criteria)
    {
        return _context.Set<T>().FirstOrDefault(criteria);
    }

    public async Task Add(T item)
    {
        _context.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task Update(T item)
    {
        _context.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(T item)
    {
        _context.Remove(item);
        await _context.SaveChangesAsync();
    }
}
