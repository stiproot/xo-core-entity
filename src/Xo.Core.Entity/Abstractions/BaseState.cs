using System.ComponentModel.DataAnnotations;

namespace Xo.Core.Entity.Abstractions;

public abstract record BaseState
{
  [JsonPropertyName("id")]
  public int Id { get; set; }

  [Required]
  [JsonPropertyName("ts")]
  public DateTime Ts { get; set; } = DateTime.UtcNow;
}
