using System;

namespace FitnessClub.Models;

/// <summary>
/// Представляет тренера фитнес-клуба
/// </summary>
public class Trainer
{
    /// <summary>
    /// Идентификатор тренера
    /// </summary>
    public int TrainerId { get; set; }

    /// <summary>
    /// Имя тренера
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Фамилия тренера
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Специализация тренера
    /// </summary>
    public string? Specialization { get; set; }

    /// <summary>
    /// Полное имя тренера
    /// </summary>
    public string FullName => $"{LastName} {FirstName}";

    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public string? Certification { get; set; }
}  
