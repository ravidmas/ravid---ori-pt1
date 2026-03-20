namespace MauiApp8.Pages;

public partial class LessonDetailPage : ContentPage
{
    private CancellationTokenSource? _cssInjectionCts;

    public LessonDetailPage(string title, string description, string[] steps, string videoUrl = "")
    {
        InitializeComponent();

        TitleLabel.Text = title;
        DescriptionLabel.Text = description;

        // Load video if URL is provided
        if (!string.IsNullOrEmpty(videoUrl))
        {
            VideoFrame.IsVisible = true;

            var videoId = videoUrl.Replace("https://www.youtube.com/embed/", "");

            // Load mobile YouTube - simple and reliable
            VideoWebView.Source = new UrlWebViewSource
            {
                Url = $"https://m.youtube.com/watch?v={videoId}"
            };

            _cssInjectionCts = new CancellationTokenSource();
            VideoWebView.Navigated += OnVideoNavigated;
        }

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

    private async void OnVideoNavigated(object? sender, WebNavigatedEventArgs e)
    {
        var ct = _cssInjectionCts?.Token ?? CancellationToken.None;

        var css = "html, body { background: #000 !important; }"
            + " .watch-below-the-player,"
            + " ytm-item-section-renderer,"
            + " ytm-comment-section-renderer,"
            + " ytm-rich-section-renderer,"
            + " .related-chips-slot-wrapper,"
            + " ytm-engagement-panel-section-list-renderer,"
            + " .single-column-watch-next-modern-panels,"
            + " #below, #related, #comments, #secondary,"
            + " ytd-watch-metadata, #meta, #info, #actions,"
            + " ytd-compact-video-renderer,"
            + " ytd-merch-shelf-renderer,"
            + " .ytp-unmute-text,"
            + " .ytp-mute-button-container,"
            + " .ytp-unmute,"
            + " .playerMuteButton,"
            + " .player-controls-content,"
            + " .ytp-chrome-bottom { display: none !important; }";

        var js = "(function() {"
            + " window.scrollTo(0, 0);"
            + " if (!document.getElementById('rd-hide')) {"
            + "   var s = document.createElement('style');"
            + "   s.id = 'rd-hide';"
            + "   s.textContent = '" + css.Replace("'", "\\'") + "';"
            + "   document.head.appendChild(s);"
            + " }"
            + " window.scrollTo(0, 0);"
            + " var v = document.querySelector('video');"
            + " if (v && v.paused) {"
            + "   v.muted = true;"
            + "   v.play().then(function(){ v.muted = false; }).catch(function(){});"
            + "   v.dispatchEvent(new MouseEvent('click', {bubbles:true}));"
            + " }"
            + " var selectors = ['.ytp-large-play-button','.ytp-play-button','.icon-button.player-controls-play-pause-button','.player-controls-play-pause-button','button.ytp-large-play-button'];"
            + " selectors.forEach(function(sel) {"
            + "   var el = document.querySelector(sel);"
            + "   if (el) { el.click(); el.dispatchEvent(new MouseEvent('click', {bubbles:true})); }"
            + " });"
            + " var btns = document.querySelectorAll('.ytp-unmute, .playerMuteButton');"
            + " btns.forEach(function(b) { b.click(); });"
            + "})();";

        for (int i = 0; i < 10; i++)
        {
            if (ct.IsCancellationRequested) break;
            try
            {
                await VideoWebView.EvaluateJavaScriptAsync(js);
                await Task.Delay(500, ct);
            }
            catch { break; }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cssInjectionCts?.Cancel();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
