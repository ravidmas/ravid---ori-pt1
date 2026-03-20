using MauiApp8.Models;

namespace MauiApp8.Services
{
    /// <summary>
    /// Local chord data provider with embedded chord definitions.
    /// No API dependency — all chords are available offline.
    /// </summary>
    public class ChordService : IChordService
    {
        private static readonly List<Chord> AllChords = new()
        {
            // ── Easy Chords ──
            new Chord
            {
                Name = "Em",
                Difficulty = "easy",
                Description = "E Minor - one of the easiest guitar chords. Place middle and ring fingers on 2nd fret of A and D strings.",
                Frets = new[] { 0, 2, 2, 0, 0, 0 },
                Fingers = new[] { 0, 2, 3, 0, 0, 0 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1358[kb]cleanchord-Em.wav.mp3"
            },
            new Chord
            {
                Name = "E",
                Difficulty = "easy",
                Description = "E Major - a full-sounding open chord. Similar to Em but add your index finger on the 1st fret of G string.",
                Frets = new[] { 0, 2, 2, 1, 0, 0 },
                Fingers = new[] { 0, 2, 3, 1, 0, 0 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1266[kb]cleanchord-E.wav.mp3"
            },
            new Chord
            {
                Name = "Am",
                Difficulty = "easy",
                Description = "A Minor - a melancholy sounding chord. Mute the low E string.",
                Frets = new[] { -1, 0, 2, 2, 1, 0 },
                Fingers = new[] { 0, 0, 2, 3, 1, 0 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1633[kb]cleanchord-Am.wav.mp3"
            },
            new Chord
            {
                Name = "A",
                Difficulty = "easy",
                Description = "A Major - a bright, happy sounding chord. Three fingers on the 2nd fret.",
                Frets = new[] { -1, 0, 2, 2, 2, 0 },
                Fingers = new[] { 0, 0, 1, 2, 3, 0 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1393[kb]cleanchord-A.wav.mp3"
            },
            new Chord
            {
                Name = "Dm",
                Difficulty = "easy",
                Description = "D Minor - a sad, emotional chord. Only strum the top 4 strings.",
                Frets = new[] { -1, -1, 0, 2, 3, 1 },
                Fingers = new[] { 0, 0, 0, 2, 3, 1 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1317[kb]cleanchord-Dm.wav.mp3"
            },
            new Chord
            {
                Name = "D",
                Difficulty = "easy",
                Description = "D Major - a bright chord often used in pop and country. Strum only the top 4 strings.",
                Frets = new[] { -1, -1, 0, 2, 3, 2 },
                Fingers = new[] { 0, 0, 0, 1, 3, 2 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1269[kb]cleanchord-D.wav.mp3"
            },

            // ── Medium Chords ──
            new Chord
            {
                Name = "C",
                Difficulty = "medium",
                Description = "C Major - one of the most common chords. Requires a good stretch from the index finger.",
                Frets = new[] { -1, 3, 2, 0, 1, 0 },
                Fingers = new[] { 0, 3, 2, 0, 1, 0 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1132[kb]cleanchord-C.wav.mp3"
            },
            new Chord
            {
                Name = "G",
                Difficulty = "medium",
                Description = "G Major - a big, full sounding chord. Uses all 6 strings.",
                Frets = new[] { 3, 2, 0, 0, 0, 3 },
                Fingers = new[] { 2, 1, 0, 0, 0, 3 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1232[kb]cleanchord-G.wav.mp3"
            },
            new Chord
            {
                Name = "A7",
                Difficulty = "medium",
                Description = "A Dominant 7th - adds a bluesy feel. Common in blues and jazz.",
                Frets = new[] { -1, 0, 2, 0, 2, 0 },
                Fingers = new[] { 0, 0, 2, 0, 3, 0 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1687[kb]cleanchord-A7.wav.mp3"
            },
            new Chord
            {
                Name = "D7",
                Difficulty = "medium",
                Description = "D Dominant 7th - a common blues and jazz chord.",
                Frets = new[] { -1, -1, 0, 2, 1, 2 },
                Fingers = new[] { 0, 0, 0, 2, 1, 3 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1841[kb]cleanchord-D7.wav.mp3"
            },
            new Chord
            {
                Name = "E7",
                Difficulty = "medium",
                Description = "E Dominant 7th - an easy blues chord based on the E major shape.",
                Frets = new[] { 0, 2, 0, 1, 0, 0 },
                Fingers = new[] { 0, 2, 0, 1, 0, 0 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1275[kb]cleanchord-E7.wav.mp3"
            },
            new Chord
            {
                Name = "B7",
                Difficulty = "medium",
                Description = "B Dominant 7th - essential for many chord progressions.",
                Frets = new[] { -1, 2, 1, 2, 0, 2 },
                Fingers = new[] { 0, 2, 1, 3, 0, 4 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1482[kb]cleanchord-B7.wav.mp3"
            },

            // ── Hard Chords ──
            new Chord
            {
                Name = "F",
                Difficulty = "hard",
                Description = "F Major barre chord - the most challenging beginner chord. Requires barring all strings at the 1st fret.",
                Frets = new[] { 1, 3, 3, 2, 1, 1 },
                Fingers = new[] { 1, 3, 4, 2, 1, 1 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1777[kb]cleanchord-F.wav.mp3"
            },
            new Chord
            {
                Name = "Bm",
                Difficulty = "hard",
                Description = "B Minor barre chord - a barre chord at the 2nd fret using the Am shape.",
                Frets = new[] { -1, 2, 4, 4, 3, 2 },
                Fingers = new[] { 0, 1, 3, 4, 2, 1 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1626[kb]cleanchord-Bm.wav.mp3"
            },
            new Chord
            {
                Name = "F#m",
                Difficulty = "hard",
                Description = "F# Minor - a barre chord at the 2nd fret. Common in keys of A and D.",
                Frets = new[] { 2, 4, 4, 2, 2, 2 },
                Fingers = new[] { 1, 3, 4, 1, 1, 1 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1634[kb]cleanchord-Fm.wav.mp3"
            },
            new Chord
            {
                Name = "Bb",
                Difficulty = "hard",
                Description = "B-flat Major - a barre chord at the 1st fret using the A shape.",
                Frets = new[] { -1, 1, 3, 3, 3, 1 },
                Fingers = new[] { 0, 1, 2, 3, 4, 1 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1362[kb]cleanchord-B.wav.mp3"
            },
            new Chord
            {
                Name = "C#m",
                Difficulty = "hard",
                Description = "C# Minor - a barre chord at the 4th fret. Used in many popular songs.",
                Frets = new[] { -1, 4, 6, 6, 5, 4 },
                Fingers = new[] { 0, 1, 3, 4, 2, 1 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/980[kb]cleanchord-Cm.wav.mp3"
            },
            new Chord
            {
                Name = "G#m",
                Difficulty = "hard",
                Description = "G# Minor - a barre chord. An advanced chord that requires good barre technique.",
                Frets = new[] { 4, 6, 6, 4, 4, 4 },
                Fingers = new[] { 1, 3, 4, 1, 1, 1 },
                SoundLink = "https://sampleswap.org/samples-ghost/INSTRUMENTS%20(MULTISAMPLED)/GUITAR/clean%20electric%20guitar%20chords/1231[kb]cleanchord-Gm.wav.mp3"
            }
        };

        public Task<List<Chord>> GetChordsByDifficultyAsync(string difficulty)
        {
            if (difficulty.Equals("all", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(new List<Chord>(AllChords));

            var filtered = AllChords
                .Where(c => c.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult(filtered);
        }

        public Task<List<Chord>> GetAllChordsAsync()
        {
            return Task.FromResult(new List<Chord>(AllChords));
        }

        public Task<List<Chord>> GetRandomChordsAsync(string difficulty, int count)
        {
            var filtered = AllChords
                .Where(c => c.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var random = new Random();
            var selected = filtered.OrderBy(_ => random.Next()).Take(count).ToList();
            return Task.FromResult(selected);
        }
    }
}
