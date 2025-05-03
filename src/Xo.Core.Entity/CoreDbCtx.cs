using Microsoft.EntityFrameworkCore;

namespace Xo.Core.Entity;

public class CoreDbCtx(DbContextOptions<CoreDbCtx> options) : DbContext(options)
{
	// public virtual DbSet<SomeEnt> Some { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Here we can define complex entity relationships like many-to-many for example, explicitly.
		base.OnModelCreating(modelBuilder);
	}
}
