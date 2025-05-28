#nullable disable

using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JSONViewer
{
    /// <summary>
    /// Interaction logic for JsonViewer.xaml
    /// </summary>
    public partial class JsonViewer : Window
    {
        public JsonViewer()
        {
            InitializeComponent();
        }

        private void LoadJson(string json)
        {
            try
            {
                JsonDocument doc = JsonDocument.Parse(json);
                tvJson.Items.Clear();
                tvJson.Items.Add(BuildTree(doc.RootElement));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error parsing JSON: " + ex.Message);
            }
        }

        private TreeViewItem BuildTree(JsonElement element, string key = null, int level = 0)
        {
            TreeViewItem item = new TreeViewItem
            {
                IsExpanded = true
            };

            string jsonKey = key != null ? $"\"{key}\": " : "";

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    item.Header = jsonKey + " {}";
                    item.Foreground = Brushes.DarkCyan;
                    List<JsonProperty> props = element.EnumerateObject().ToList();
                    for (int idx = 0; idx < props.Count; idx++)
                    {
                        JsonProperty prop = props[idx];
                        TreeViewItem tvi = BuildTree(prop.Value, prop.Name, level + 1);
                        if (idx < props.Count - 1)
                            tvi.Header += ",";
                        item.Items.Add(tvi);
                    }
                    //item.Items.Add(new TreeViewItem { Header = "}" });
                    break;

                case JsonValueKind.Array:
                    item.Header = jsonKey + " []";
                    item.Foreground = Brushes.DarkGreen;
                    List<JsonElement> elements = element.EnumerateArray().ToList();
                    for (int idx = 0; idx < elements.Count; idx++)
                    {
                        JsonElement elem = elements[idx];
                        TreeViewItem tvi = BuildTree(elem, null, level + 1);
                        if (idx < elements.Count - 1)
                            tvi.Header += ",";
                        item.Items.Add(tvi);
                    }

                    // item.Items.Add(new TreeViewItem { Header = "]" });
                    break;

                case JsonValueKind.String:
                    item.Header = jsonKey + $"\"{element.GetString()}\"";
                    item.Foreground = Brushes.Brown;
                    break;

                case JsonValueKind.Number:
                    item.Header = jsonKey + element.ToString().ToLower();
                    item.Foreground = Brushes.Blue;
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    item.Header = jsonKey + element.ToString().ToLower();
                    item.Foreground = Brushes.Purple;
                    break;

                case JsonValueKind.Null:
                    item.Header = jsonKey + "null";
                    item.Foreground = Brushes.Gray;
                    break;
            }

            return item;
        }


        private void btnPasteJson_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                LoadJson(Clipboard.GetText());
            }
        }

        private void btnLoadJson_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    LoadJson(File.ReadAllText(dlg.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load JSON: " + ex.Message);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                if (clipboardText.StartsWith("[") || clipboardText.StartsWith("{"))
                {
                    try
                    {
                        LoadJson(clipboardText);
                    }
                    catch { }
                }   
            }
        }
    }
}
