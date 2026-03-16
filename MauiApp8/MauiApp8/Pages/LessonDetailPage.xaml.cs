namespace MauiApp8.Pages;

public partial class LessonDetailPage : ContentPage
{
    public LessonDetailPage(string title, string description, string[] steps)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        DescriptionLabel.Text = description;

        for (int i = 0; i < steps.Length; i++)
        {
            var stepFrame = new Frame
            {
                BackgroundColor = (Color)Application.Current!.Resources["AppCream"],
                CornerRadius = 12,
                Padding = new Thickness(15),
                HasShadow = true
            };

            var stepGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                ColumnSpacing = 12
            };

            var numberLabel = new Label
            {
                Text = $"{i + 1}",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current!.Resources["AppBrown"],
                VerticalOptions = LayoutOptions.Start,
                WidthRequest = 30,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var textLabel = new Label
            {
                Text = steps[i],
                FontSize = 15,
                TextColor = (Color)Application.Current!.Resources["AppDarkBrown"],
                LineHeight = 1.3
            };

            Grid.SetColumn(numberLabel, 0);
            Grid.SetColumn(textLabel, 1);

            stepGrid.Children.Add(numberLabel);
            stepGrid.Children.Add(textLabel);

            stepFrame.Content = stepGrid;
            StepsContainer.Children.Add(stepFrame);
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
