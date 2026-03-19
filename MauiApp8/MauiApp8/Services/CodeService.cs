using System.Net.Http.Json;
using MauiApp8.Models;

namespace MauiApp8.Services
{
    public class ChordService : IChordService
    {
        private readonly HttpClient _httpClient;

        public ChordService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppConfig.ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(AppConfig.HttpTimeoutSeconds)
            };
        }

        // Get chords by difficulty
        public async Task<List<Chord>> GetChordsByDifficultyAsync(string difficulty)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Chord>>(
                    $"/getChords?difficulty={difficulty.ToLower()}");

                return response ?? new List<Chord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chords: {ex.Message}");
                return new List<Chord>();
            }
        }

        // Get all chords
        public async Task<List<Chord>> GetAllChordsAsync()
        {
            return await GetChordsByDifficultyAsync("all");
        }

        // Get random chords by difficulty and count
        public async Task<List<Chord>> GetRandomChordsAsync(string difficulty, int count)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Chord>>(
                    $"/getChords?difficulty={difficulty.ToLower()}&count={count}");

                return response ?? new List<Chord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching random chords: {ex.Message}");
                return new List<Chord>();
            }
        }
    }
}