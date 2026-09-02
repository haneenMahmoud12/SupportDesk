using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Interfaces.Repositories;
using SupportDesk.Infrastructure.Data;

namespace SupportDesk.Infrastructure.Repositories;

public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> Entities = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        await Entities.FindAsync([id], cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await Entities.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default) =>
        await Entities.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity) => Entities.Update(entity);

    public virtual void Remove(T entity) => Entities.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Context.SaveChangesAsync(cancellationToken);
}
