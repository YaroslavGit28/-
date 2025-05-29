namespace FitnessClub.Models;

/// <summary>
/// Представляет абонемент фитнес-клуба
/// </summary>
public class Membership
{
    /// <summary>
    /// Идентификатор абонемента
    /// </summary>
    public int MembershipId { get; set; }

    /// <summary>
    /// Тип абонемента
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Длительность абонемента в днях
    /// </summary>
    public int DurationDays { get; set; }

    /// <summary>
    /// Стоимость абонемента
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Описание преимуществ абонемента
    /// </summary>
    public string? Benefits { get; set; }
}  
