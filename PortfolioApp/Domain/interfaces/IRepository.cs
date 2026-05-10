using System.Linq.Expressions;

namespace Domain.interfaces;


public interface IRepository<TEntity> where TEntity : class
{
    TEntity Create(TEntity t);
    List<TEntity> CreateRange(List<TEntity> list);
    void Update(TEntity t);
    void DeleteRange(IEnumerable<TEntity> list);
    void UpdateRange(List<TEntity> list);
    TEntity? Read(int id);
    List<TEntity> Read(Expression<Func<TEntity, bool>> filter);
    List<TEntity> Read(int start, int count);
    List<TEntity> ReadAll();
    void Delete(TEntity t);
}