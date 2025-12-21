using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PromptEngine.Desktop;

public partial class MainWindow : Window
{
    private const string CustomTag = "__custom__";
    private const string DefaultNegativePrompt = "blurry, low quality, watermark, text, signature, bad anatomy, deformed";

    private readonly Random _random = new();
    private readonly Dictionary<string, PromptContext> _contexts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["scifi"] = new PromptContext
        {
            Medium = ["Digital Painting", "3D Render", "Unreal Engine 5", "Concept Art", "Blueprint", "Matte Painting"],
            Style = ["Cyberpunk", "Biomechanical", "Retro-Futurism", "Hard Surface", "Post-Apocalyptic", "Solarpunk"],
            Lighting = ["Neon Lights", "Bioluminescence", "Holographic Glow", "Cold Sterile Light", "Volumetric Fog"],
            Camera = ["Wide Angle", "Isometric", "Drone View", "Macro Detail", "Cinematic Shot"],
            Material = ["Brushed Steel", "Carbon Fiber", "Translucent Polymer", "Rusty Metal", "Chrome"],
            Artist = ["Syd Mead", "H.R. Giger", "Simon Stålenhag", "Beeple"],
            Color = ["Cyan and Magenta", "Orange and Teal", "Monochrome Green", "High Contrast Black & White"]
        },
        ["fantasy"] = new PromptContext
        {
            Medium = ["Oil Painting", "Watercolor", "Ink Drawing", "Fantasy Illustration", "Tapestry Style"],
            Style = ["High Fantasy", "Dark Souls Style", "Ethereal", "Grimdark", "Studio Ghibli", "D&D Art"],
            Lighting = ["God Rays", "Candlelight", "Magical Aura", "Moonlight", "Firelight"],
            Camera = ["Low Angle (Heroic)", "Portrait", "Landscape Panorama", "Dutch Angle"],
            Material = ["Worn Leather", "Damascus Steel", "Velvet", "Stone and Moss", "Crystal"],
            Artist = ["Greg Rutkowski", "Frank Frazetta", "Yoshitaka Amano", "Alan Lee"],
            Color = ["Earth Tones", "Gold and Purple", "Blood Red", "Pastel Dream"]
        },
        ["photo"] = new PromptContext
        {
            Medium = ["Photography", "Polaroid", "Editorial Shot", "Candid Shot"],
            Style = ["Photorealistic", "Cinematic", "Noir", "Vintage 1980s", "Minimalist"],
            Lighting = ["Golden Hour", "Studio Softbox", "Rembrandt Lighting", "Natural Light", "Flash Photography"],
            Camera = ["85mm Lens", "35mm Lens", "Bokeh Depth of Field", "Fisheye", "Macro Lens"],
            Material = ["Skin Texture", "Fabric Detail", "Realistic Water", "Dust Particles"],
            Artist = ["Annie Leibovitz", "Steve McCurry", "Ansel Adams"],
            Color = ["Kodak Portra 400", "Black and White", "Desaturated", "Warm Tones"]
        },
        ["arch"] = new PromptContext
        {
            Medium = ["Architectural Photography", "ArchViz Render", "Floor Plan"],
            Style = ["Brutalism", "Mid-Century Modern", "Bauhaus", "Industrial", "Scandinavian"],
            Lighting = ["Natural Window Light", "Warm Interior Lights", "Sunset", "Overcast Soft"],
            Camera = ["2-Point Perspective", "Wide Angle Interior", "Aerial View"],
            Material = ["Concrete", "Mahogany Wood", "Marble", "Exposed Brick", "Glass"],
            Artist = ["Zaha Hadid Style", "Frank Lloyd Wright Style"],
            Color = ["Neutral Beiges", "White on White", "Dark Moody"]
        },
        ["horror"] = new PromptContext
        {
            Medium = ["Found Footage", "Oil Painting", "Grainy Photo", "Charcoal Sketch"],
            Style = ["Lovecraftian", "Body Horror", "Liminal Space", "Gothic"],
            Lighting = ["Single Light Source", "Flickering Light", "Pitch Black shadows", "Red Emergency Light"],
            Camera = ["Dutch Angle", "Security Camera", "Blurry Motion"],
            Material = ["Viscera", "Rusted Metal", "Rotting Wood", "Fog"],
            Artist = ["Zdzisław Beksiński", "Junji Ito"],
            Color = ["Desaturated Green", "Blood Red", "Sepia", "Void Black"]
        }
    };

    private readonly Dictionary<string, List<string>> _randomSubjects = new(StringComparer.OrdinalIgnoreCase)
    {
        ["scifi"] = ["A rogue AI android", "A massive space station", "A cybernetic tiger", "A futuristic racer"],
        ["fantasy"] = ["An ancient dragon", "A hidden elven temple", "A warrior with a glowing sword", "A magical potion shop"],
        ["photo"] = ["A portrait of an old sailor", "A rainy city street", "A woman in a red dress", "A vintage car"],
        ["arch"] = ["A modern glass mansion", "An abandoned factory", "A cozy wooden cabin", "A futuristic library"],
        ["horror"] = ["A haunted asylum", "A monster in the mist", "A creepy doll", "A dark ritual"]
    };

    private readonly List<string> _qualityOptions = ["4k", "8k", "High Detail", "Masterpiece", "Trending on ArtStation", "Raw Photo"];

    private readonly Dictionary<string, FieldControls> _fields;
    private readonly string[] _orderedFields = ["medium", "style", "lighting", "camera", "material", "artist", "color"];

    public MainWindow()
    {
        InitializeComponent();

        _fields = new Dictionary<string, FieldControls>(StringComparer.OrdinalIgnoreCase)
        {
            ["medium"] = new FieldControls(MediumCombo, MediumCustom),
            ["style"] = new FieldControls(StyleCombo, StyleCustom),
            ["lighting"] = new FieldControls(LightingCombo, LightingCustom),
            ["camera"] = new FieldControls(CameraCombo, CameraCustom),
            ["material"] = new FieldControls(MaterialCombo, MaterialCustom),
            ["artist"] = new FieldControls(ArtistCombo, ArtistCustom),
            ["color"] = new FieldControls(ColorCombo, ColorCustom),
            ["quality"] = new FieldControls(QualityCombo, QualityCustom)
        };

        ContextCombo.ItemsSource = _contexts.Keys;
        ContextCombo.SelectedIndex = 0;
        LoadContext(CurrentContextKey);

        NegativeOutputBox.Text = DefaultNegativePrompt;
    }

    private string CurrentContextKey => ContextCombo.SelectedItem as string ?? "scifi";

    private void ContextCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadContext(CurrentContextKey);
    }

    private void FieldCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo)
        {
            var key = FindFieldKey(combo);
            if (key is not null)
            {
                UpdateCustomVisibility(_fields[key]);
            }
        }
    }

    private void RandomizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        RandomizeAll();
    }

    private void GenerateButton_OnClick(object sender, RoutedEventArgs e)
    {
        GeneratePrompt();
    }

    private void CopyPositive_OnClick(object sender, RoutedEventArgs e)
    {
        CopyToClipboard(OutputBox, sender as Button);
    }

    private void CopyNegative_OnClick(object sender, RoutedEventArgs e)
    {
        CopyToClipboard(NegativeOutputBox, sender as Button);
    }

    private void LoadContext(string contextKey)
    {
        if (!_contexts.TryGetValue(contextKey, out var context))
        {
            return;
        }

        SetOptions("medium", context.Medium);
        SetOptions("style", context.Style);
        SetOptions("lighting", context.Lighting);
        SetOptions("camera", context.Camera);
        SetOptions("material", context.Material);
        SetOptions("artist", context.Artist);
        SetOptions("color", context.Color);
        SetOptions("quality", _qualityOptions);
    }

    private void SetOptions(string fieldKey, IEnumerable<string> options)
    {
        var controls = _fields[fieldKey];

        controls.Combo.Items.Clear();
        controls.Combo.Items.Add(new ComboBoxItem { Content = "None", Tag = string.Empty });

        foreach (var option in options)
        {
            controls.Combo.Items.Add(new ComboBoxItem { Content = option, Tag = option });
        }

        controls.Combo.Items.Add(new ComboBoxItem { Content = "Custom...", Tag = CustomTag });
        controls.Combo.SelectedIndex = 0;
        controls.CustomBox.Visibility = Visibility.Collapsed;
        controls.CustomBox.Text = string.Empty;
    }

    private void RandomizeAll()
    {
        var selectedContext = _contexts.Keys.ElementAt(_random.Next(_contexts.Count));
        ContextCombo.SelectedItem = selectedContext;
        LoadContext(selectedContext);

        if (_randomSubjects.TryGetValue(selectedContext, out var subjects) && subjects.Count > 0)
        {
            CoreSubjectBox.Text = ChooseRandom(subjects);
        }

        foreach (var controls in _fields.Values)
        {
            if (controls.Combo.Items.Count <= 2)
            {
                continue;
            }

            int randomIndex = _random.Next(1, controls.Combo.Items.Count - 1);
            controls.Combo.SelectedIndex = randomIndex;
        }

        GeneratePrompt();
    }

    private void GeneratePrompt()
    {
        string core = CoreSubjectBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(core))
        {
            MessageBox.Show("Please enter a core subject.", "Missing Subject", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string format = (FormatCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "standard";
        List<string> parts = new() { core };

        foreach (var field in _orderedFields)
        {
            string? value = GetFieldValue(field);
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            parts.Add(format == "natural"
                ? ToNaturalFragment(field, value)
                : value);
        }

        string? quality = GetFieldValue("quality");
        if (!string.IsNullOrWhiteSpace(quality))
        {
            parts.Add(quality);
        }

        OutputBox.Text = string.Join(", ", parts);
        NegativeOutputBox.Text = DefaultNegativePrompt;
    }

    private string? GetFieldValue(string fieldKey)
    {
        if (!_fields.TryGetValue(fieldKey, out var controls))
        {
            return null;
        }

        if (controls.Combo.SelectedItem is not ComboBoxItem selectedItem)
        {
            return null;
        }

        var tag = selectedItem.Tag as string;
        if (string.IsNullOrWhiteSpace(tag))
        {
            return null;
        }

        if (tag == CustomTag)
        {
            string custom = controls.CustomBox.Text.Trim();
            return string.IsNullOrWhiteSpace(custom) ? null : custom;
        }

        return tag;
    }

    private void UpdateCustomVisibility(FieldControls controls)
    {
        if (controls.Combo.SelectedItem is ComboBoxItem { Tag: string tag } && tag == CustomTag)
        {
            controls.CustomBox.Visibility = Visibility.Visible;
            controls.CustomBox.Focus();
        }
        else
        {
            controls.CustomBox.Visibility = Visibility.Collapsed;
        }
    }

    private static string ToNaturalFragment(string field, string value) =>
        field switch
        {
            "lighting" => $"with {value} lighting",
            "style" => $"in {value} style",
            "artist" => $"by {value}",
            "camera" => $"viewed from {value}",
            _ => value
        };

    private string ChooseRandom(IReadOnlyList<string> values) => values[_random.Next(values.Count)];

    private string? FindFieldKey(ComboBox combo) =>
        _fields.FirstOrDefault(pair => ReferenceEquals(pair.Value.Combo, combo)).Key;

    private static void CopyToClipboard(TextBox textBox, Button? button)
    {
        Clipboard.SetText(textBox.Text);

        if (button is null)
        {
            return;
        }

        string originalContent = button.Content?.ToString() ?? string.Empty;
        button.Content = "Copied!";

        _ = Task.Delay(1500).ContinueWith(_ =>
        {
            button.Dispatcher.Invoke(() => button.Content = originalContent);
        });
    }

    private record PromptContext
    {
        public List<string> Medium { get; init; } = [];
        public List<string> Style { get; init; } = [];
        public List<string> Lighting { get; init; } = [];
        public List<string> Camera { get; init; } = [];
        public List<string> Material { get; init; } = [];
        public List<string> Artist { get; init; } = [];
        public List<string> Color { get; init; } = [];
    }

    private record FieldControls(ComboBox Combo, TextBox CustomBox);
}
