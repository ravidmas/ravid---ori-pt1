namespace MauiApp8.Pages;

public partial class LessonsPage : ContentPage
{
    // Curated lesson content - can be replaced with API content later
    private static readonly Dictionary<int, LessonInfo> _lessons = new()
    {
        [1] = new("How to Hold a Guitar",
            "Learn the proper way to hold your guitar for comfortable playing.",
            new[]
            {
                "Sit up straight with the guitar body resting on your right leg (for right-handed players).",
                "Keep your left thumb behind the neck for support.",
                "Curve your fingers so you press strings with your fingertips.",
                "Keep your wrist relaxed - tension is the enemy of good guitar playing!",
                "The neck should be slightly angled upward, not parallel to the floor."
            }),
        [2] = new("Your First Chord: Em",
            "The E minor chord is the easiest chord to learn on guitar.",
            new[]
            {
                "Place your middle finger on the 2nd fret of the A string (5th string).",
                "Place your ring finger on the 2nd fret of the D string (4th string).",
                "Strum all 6 strings from top to bottom.",
                "Make sure each string rings clearly - adjust your finger pressure if needed.",
                "Practice pressing down and releasing the chord shape repeatedly."
            }),
        [3] = new("Basic Strumming Patterns",
            "Master the fundamental down-up strumming pattern.",
            new[]
            {
                "Start with simple downstrokes on each beat: 1-2-3-4.",
                "Add upstrokes between beats: 1-and-2-and-3-and-4-and.",
                "Keep your wrist loose and relaxed while strumming.",
                "Use a metronome or tap your foot to keep time.",
                "Try this pattern: Down-Down-Up-Up-Down-Up."
            }),
        [4] = new("Smooth Chord Transitions",
            "Learn techniques to switch between chords quickly and smoothly.",
            new[]
            {
                "Practice switching between just 2 chords at first (e.g., Em to G).",
                "Keep your fingers close to the strings when lifting.",
                "Look for 'anchor fingers' - fingers that stay in the same position between chords.",
                "Start slow and gradually increase speed.",
                "Practice the 'one-minute change' drill: count how many clean switches you can make in 60 seconds."
            }),
        [5] = new("Introduction to Barre Chords",
            "Take your playing to the next level with barre chords.",
            new[]
            {
                "A barre chord uses one finger to press down multiple strings at once.",
                "Start with the F major chord: barre all strings at the 1st fret with your index finger.",
                "Place your other fingers in the E major shape, shifted up one fret.",
                "Use the side of your index finger for the barre, not the flat pad.",
                "Build strength gradually - barre chords take weeks of practice to master!"
            })
    };

    public LessonsPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnLesson1Tapped(object sender, EventArgs e) => ShowLesson(1);
    private void OnLesson2Tapped(object sender, EventArgs e) => ShowLesson(2);
    private void OnLesson3Tapped(object sender, EventArgs e) => ShowLesson(3);
    private void OnLesson4Tapped(object sender, EventArgs e) => ShowLesson(4);
    private void OnLesson5Tapped(object sender, EventArgs e) => ShowLesson(5);

    private async void ShowLesson(int lessonNumber)
    {
        if (!_lessons.TryGetValue(lessonNumber, out var lesson))
            return;

        var lessonPage = new LessonDetailPage(lesson.Title, lesson.Description, lesson.Steps);
        await Navigation.PushAsync(lessonPage);
    }

    public record LessonInfo(string Title, string Description, string[] Steps);
}
