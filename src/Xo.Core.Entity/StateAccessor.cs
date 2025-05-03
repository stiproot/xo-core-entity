using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Xo.Core.Entity;

public class StateAccessor(
  DbCtxAdaptor db,
  IDbContextFactory<CoreDbCtx> dbFn,
  IMapper mapper
)
{
  private readonly DbCtxAdaptor _db = db;
  private readonly IDbContextFactory<CoreDbCtx> _dbCtxFn = dbFn;
  private readonly IMapper _mapper = mapper;

  public DbCtxAdaptor DbAdaptor => this._db;
  public IMapper Mapper => this._mapper;
  public IDbContextFactory<CoreDbCtx> DbCtxFn => this._dbCtxFn;

  public async Task<TState?> FirstOrDefaultAsync<TEnt, TState>(
    int id,
    IEnumerable<string>? incl = null
  )
    where TEnt : BaseKeyedEnt
    where TState : BaseState
  {
    var ent = await this._db.FirstOrDefaultAsync<TEnt>(id, incl);

    if (ent is null) return null;

    return this._mapper.Map<TEnt, TState>(ent);
  }

  public async Task<TState?> FirstOrDefaultAsync<TEnt, TState>(Expression<Func<TEnt, bool>> expr)
    where TEnt : BaseKeyedEnt
    where TState : BaseState
  {
    var ent = await this._db.FirstOrDefaultAsync(predExpr: expr);

    if (ent is null) return null;

    return this._mapper.Map<TEnt, TState>(ent);
  }

  public async Task<IEnumerable<TState>> ToListAsync<TEnt, TState>(
    Expression<Func<TEnt, bool>> expr,
    IEnumerable<string>? incl = null
  )
    where TEnt : BaseKeyedEnt
    where TState : BaseState
  {
    var ent = await this._db.ToListAsync(predExpr: expr, incl: incl);
    return this._mapper.Map<IEnumerable<TEnt>, IEnumerable<TState>>(ent);
  }

  public async Task<TState> FirstOrThrowAsync<TEnt, TState>(
    int id,
    IEnumerable<string>? incl = null
  )
    where TEnt : BaseKeyedEnt
    where TState : BaseState
  {
    return await this.FirstOrDefaultAsync<TEnt, TState>(id, incl) ?? throw new InvalidOperationException($"State not found for id: {id}");
  }

  public async Task<int> AddAsync<TState, TEnt>(TState state)
    where TState : BaseState
    where TEnt : BaseKeyedEnt
  {
    var ent = _mapper.Map<TState, TEnt>(state);
    return await _db.AddAsync(ent);
  }

  public async Task AddRangeAsync<TState, TEnt>(IEnumerable<TState> state)
    where TState : BaseState
    where TEnt : BaseKeyedEnt
  {
    await _db.AddRangeAsync(_mapper.Map<IEnumerable<TState>, IEnumerable<TEnt>>(state));
  }

  public async Task<int> TryAddAsync<TState, TEnt>(TState state)
    where TState : BaseState
    where TEnt : BaseKeyedEnt
  {
    return await _db.TryAddAsync(_mapper.Map<TState, TEnt>(state));
  }

  public async Task TryAddRangeAsync<TState, TEnt>(IEnumerable<TState> state)
    where TState : BaseState
    where TEnt : BaseKeyedEnt
  {
    await _db.TryAddRangeAsync(_mapper.Map<IEnumerable<TState>, IEnumerable<TEnt>>(state));
  }

  public async Task UpdateAsync<TState, TEnt>(TState state)
    where TState : BaseState
    where TEnt : BaseKeyedEnt
  {
    var ent = await _db.FirstOrThrowAsync<TEnt>(state.Id);
    _mapper.Map(state, ent);
    await _db.UpdateAsync(ent);
  }

  public async Task UpdateRangeAsync<TState, TEnt>(IEnumerable<TState> state)
    where TState : BaseState
    where TEnt : BaseKeyedEnt
  {
    var ents = _mapper.Map<IEnumerable<TState>, IEnumerable<TEnt>>(state);
    await _db.UpdateRangeAsync(ents);
  }

  public async Task<IEnumerable<TState>> AllAsync<TEnt, TState>(IEnumerable<string>? incl = null)
    where TEnt : BaseKeyedEnt
    where TState : BaseState
  {
    return this._mapper.Map<IEnumerable<TEnt>, IEnumerable<TState>>(await this._db.AllAsync<TEnt>(incl));
  }
}