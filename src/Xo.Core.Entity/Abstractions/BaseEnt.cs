using System.ComponentModel.DataAnnotations;

namespace Xo.Core.Entity.Abstractions;

public abstract class BaseEnt
{
	[Required]
	public bool Deleted { get; set; } = false;

	[Required]
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
