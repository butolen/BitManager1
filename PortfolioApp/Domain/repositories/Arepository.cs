using Domain.interfaces;
using Model;

namespace Domain.repositories;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


public abstract class ARepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> Table;

    protected ARepository(ApplicationDbContext context)
    {
        Context = context;
        Table = context.Set<TEntity>();
    }

    public TEntity Create(TEntity t)
    {
        Table.Add(t);
        Context.SaveChanges();
        return t;
    }

    public List<TEntity> CreateRange(List<TEntity> list)
    {
        Table.AddRange(list);
        Context.SaveChanges();
        return list;
    }

    public void Update(TEntity t)
    {
        Context.ChangeTracker.Clear();
        Table.Update(t);
        Context.SaveChanges();
    }

    public void UpdateRange(List<TEntity> list)
    {
        Table.UpdateRange(list);
        Context.SaveChanges();
    }
    public void DeleteRange(IEnumerable<TEntity> list)
    {
        Table.RemoveRange(list);
        Context.SaveChanges();
    }
    public TEntity? Read(int id) => Table.Find(id);

    public List<TEntity> Read(Expression<Func<TEntity, bool>> filter) =>
        Table.Where(filter).ToList();

    public List<TEntity> Read(int start, int count) =>
        Table.Skip(start).Take(count).ToList();

    public List<TEntity> ReadAll() => Table.ToList();

    public void Delete(TEntity t)
    {
        Table.Remove(t);
        Context.SaveChanges();
    }
}