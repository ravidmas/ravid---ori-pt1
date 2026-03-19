using MauiApp8.Models;

namespace MauiApp8.Services;

/// <summary>
/// Interface for chord data operations
/// </summary>
public interface IChordService
{
    Task<List<Chord>> GetChordsByDifficultyAsync(string difficulty);
    Task<List<Chord>> GetAllChordsAsync();
    Task<List<Chord>> GetRandomChordsAsync(string difficulty, int count);
}
