namespace MauiApp8.Models
{
    public class Chord
    {
        public string ImgLink { get; set; } = "";
        public string SoundLink { get; set; } = "";
        public string Difficulty { get; set; } = "";
        public string Name { get; set; } = "";
        public ChordPoint[]? ChordPoints { get; set; }
    }

    public class ChordPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}