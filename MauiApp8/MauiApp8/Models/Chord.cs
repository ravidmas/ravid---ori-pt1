namespace MauiApp8.Models
{
    public class Chord
    {
        public string ImgLink { get; set; } = "";
        public string SoundLink { get; set; } = "";
        public string Difficulty { get; set; } = "";
        public string Name { get; set; } = "";
        public ChordPoint[]? ChordPoints { get; set; }

        /// <summary>
        /// Fret positions for each string (low E to high E).
        /// -1 = muted (X), 0 = open (O), 1+ = fret number.
        /// </summary>
        public int[] Frets { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Finger numbers for each string (0 = none, 1-4 = index to pinky).
        /// </summary>
        public int[] Fingers { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Description of the chord for learning.
        /// </summary>
        public string Description { get; set; } = "";
    }

    public class ChordPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}