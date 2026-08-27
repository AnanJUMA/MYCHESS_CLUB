using System.ComponentModel.DataAnnotations;

public class Tournament
{
    public int Id { get; set; }
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public DateTime Date { get; set; }
    [Required]
    [StringLength(100)]
    public string Location { get; set; } = string.Empty;
    [Required]
    public TournamentFormat Format { get; set; }
    [StringLength(200)]
    public string Eligibility { get; set; } = string.Empty;
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}

public enum TournamentFormat { Classical, Rapid, Blitz, Bullet }