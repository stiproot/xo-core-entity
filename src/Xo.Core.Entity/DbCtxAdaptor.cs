using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Xo.Core.Entity;

public class DbCtxAdaptor(IDbContextFactory<CoreDbCtx> dbFn)
{
  private readonly IDbContextFactory<CoreDbCtx> dbFn = dbFn;

  public async Task<TEnt?> FirstOrDefaultAsync<TEnt>(
    int id,
    IEnumerable<string>? incl = null
  ) where TEnt : BaseKeyedEnt
  {
    return await this.FirstOrDefaultAsync<TEnt>(predExpr: e => e.Id == id, incl: incl);
  }

  public async Task<TEnt> FirstOrThrowAsync<TEnt>(
    int id,
    IEnumerable<string>? incl = null
  ) where TEnt : BaseKeyedEnt
  {
    return await this.FirstOrDefaultAsync<TEnt>(id, incl) ?? throw new InvalidOperationException($"Entity not found for id: {id}");
  }

  public async Task<TEnt> FirstOrThrowAsync<TEnt>(
    Expression<Func<TEnt, bool>> expr,
    IEnumerable<string>? incl = null
  ) where TEnt : BaseKeyedEnt
  {
    return await this.FirstOrDefaultAsync(expr, incl) ?? throw new InvalidOperationException($"Entity not found.");
  }

  public async Task<int> AddAsync<TEnt>(TEnt ent) where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    db.Add(ent);

    await db.SaveChangesAsync();

    return ent.Id!;
  }

  public async Task<int> TryAddAsync<TEnt>(TEnt ent) where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    TEnt? existing = await this.FirstOrDefaultAsync<TEnt>(ent.Id);

    if (existing is not null) return existing.Id;

    db.Add(ent);

    await db.SaveChangesAsync();

    return ent.Id!;
  }

  public async Task AddRangeAsync<TEnt>(IEnumerable<TEnt> ents) where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    await db.AddRangeAsync(ents);

    await db.SaveChangesAsync();
  }

  public async Task TryAddRangeAsync<TEnt>(IEnumerable<TEnt> ents) where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    TEnt[] persistable = [.. ents.Where(e => e.Id == 0)];
    if (persistable.Length == 0) return;

    await db.AddRangeAsync(ents);
    await db.SaveChangesAsync();
  }

  public async Task UpdateAsync<TEnt>(TEnt ent) where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    if (db.Entry(ent).State == EntityState.Detached) db.Attach(ent);

    db.Entry(ent).State = EntityState.Modified;

    await db.SaveChangesAsync();
  }

  public async Task UpdateRangeAsync<TEnt>(IEnumerable<TEnt> ents) where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    foreach (var ent in ents)
    {
      if (db.Entry(ent).State == EntityState.Detached) db.Attach(ent);
      db.Entry(ent).State = EntityState.Modified;
    }

    await db.SaveChangesAsync();
  }

  public async Task<IEnumerable<TEnt>> AllAsync<TEnt>(IEnumerable<string>? incl = null) where TEnt : BaseKeyedEnt
  {
    return await this.ToListAsync<TEnt>(incl: incl);
  }

  public async Task RemoveAsync<TEnt>(TEnt ent) where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    DbSet<TEnt> dbSet = db.Set<TEnt>();
    dbSet.Remove(ent);

    await db.SaveChangesAsync();
  }

  public async Task ClearAsync<TEnt>() where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    DbSet<TEnt> dbSet = db.Set<TEnt>();

    await dbSet.ExecuteDeleteAsync();
  }

  public async Task TryRemoveAsync<TEnt>(int id) where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    DbSet<TEnt> dbSet = db.Set<TEnt>();
    var ent = await dbSet.FindAsync(id);
    if (ent is null) return;

    dbSet.Remove(ent!);

    await db.SaveChangesAsync();
  }

  public async Task TryRemoveAsync<TEnt>(int[] ids) where TEnt : BaseKeyedEnt
  {
    using var db = dbFn.CreateDbContext();

    DbSet<TEnt> dbSet = db.Set<TEnt>();
    var ents = await dbSet
      .Where(e => ids.Contains(e.Id))
      .ToListAsync();

    dbSet.RemoveRange(ents);

    await db.SaveChangesAsync();
  }

  public async Task<IEnumerable<TEnt>> ToListAsync<TEnt>(
    Expression<Func<TEnt, bool>>? predExpr = null,
    IEnumerable<string>? incl = null,
    Expression<Func<TEnt, object>>? orderByExpr = null,
    Expression<Func<TEnt, object>>? orderByDescExpr = null,
    int? take = null
  ) where TEnt : BaseKeyedEnt
  {
    if (orderByExpr is not null && orderByDescExpr is not null) throw new ArgumentException("Both order by and order by descending functions cannot be provided");

    using var db = dbFn.CreateDbContext();

    var qry = Qry(
      db,
      predExpr,
      incl,
      orderByExpr,
      orderByDescExpr,
      take
    );

    return await qry.ToListAsync();
  }

  public async Task<TEnt?> FirstOrDefaultAsync<TEnt>(
    Expression<Func<TEnt, bool>> predExpr,
    IEnumerable<string>? incl = null,
    Expression<Func<TEnt, object>>? orderByExpr = null,
    Expression<Func<TEnt, object>>? orderByDescExpr = null,
    int? take = null
  ) where TEnt : BaseKeyedEnt
  {
    if (orderByExpr is not null && orderByDescExpr is not null) throw new ArgumentException("Both order by and order by descending functions cannot be provided");

    using var db = dbFn.CreateDbContext();

    var qry = Qry(
      db,
      predExpr,
      incl,
      orderByExpr,
      orderByDescExpr,
      take
    );

    return await qry.FirstOrDefaultAsync();
  }

  private static IQueryable<TEnt> Qry<TEnt>(
    CoreDbCtx ctx,
    Expression<Func<TEnt, bool>>? predExpr = null,
    IEnumerable<string>? incl = null,
    Expression<Func<TEnt, object>>? orderByExpr = null,
    Expression<Func<TEnt, object>>? orderByDescExpr = null,
    int? take = null
  ) where TEnt : BaseKeyedEnt
  {
    if (orderByExpr is not null && orderByDescExpr is not null) throw new ArgumentException("Both order by and order by descending functions cannot be provided");

    IQueryable<TEnt> qry = ctx.Set<TEnt>().AsQueryable();

    if (orderByExpr is not null) qry = qry.OrderBy(orderByExpr).AsQueryable();
    if (orderByDescExpr is not null) qry = qry.OrderByDescending(orderByDescExpr).AsQueryable();
    if (incl is not null) incl?.ToList().ForEach(i => qry = qry.Include(i));
    if (predExpr is not null) qry = qry.Where(predExpr);
    if (take is not null) qry = qry.Take((int)take);

    return qry;
  }
}