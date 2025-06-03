#nullable disable

using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            tvJson.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(ItemExpanded));
            tvJson.AddHandler(TreeViewItem.CollapsedEvent, new RoutedEventHandler(ItemCollapsed));
        }

        JsonDocument doc = null;

        private void LoadJson(string json, bool showerror = true)
        {
            try
            {
                tvJson.Items.Clear();
                doc = JsonDocument.Parse(json);
                tvJson.Visibility = Visibility.Hidden;
                TreeViewItem rootnode = BuildTree(doc.RootElement);
                tvJson.Items.Add(rootnode);
                tvJson.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                if (showerror)
                    MessageBox.Show("Error parsing JSON: " + ex.Message);
            }
        }

        private TreeViewItem BuildTree(JsonElement element, string key = null, int level = 0)
        {
            TreeViewItem item = new()
            {
                IsExpanded = (level < 2),
                Tag = element
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

        private bool isexpanding = false;

        private void ItemExpanded(object sender, RoutedEventArgs e)
        {
            if (isexpanding) return;
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item?.IsExpanded == true && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                isexpanding = true;
                ExpandAllChildren(item);
                isexpanding = false;
            }
        }

        private void ExpandAllChildren(TreeViewItem item, bool recursive = false)
        {
            item.IsExpanded = true;
            foreach (TreeViewItem child in item.Items)
            {
                if (child.Items.Count == 0)
                    continue;
                if (recursive)
                    ExpandAllChildren(child, recursive);
                else 
                    child.IsExpanded = true;
            }
        }

        private void ItemCollapsed(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem item)
            {
                item.Foreground = Brushes.Black;
            }
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
            OpenFileDialog dlg = new OpenFileDialog { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*" };
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
            if (!Clipboard.ContainsText())
                return;

            string cliptext = Clipboard.GetText();
            if (cliptext.StartsWith('[') || cliptext.StartsWith('{'))
            {
                LoadJson(cliptext, false);
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("JSON Viewer\n\nA simple tool to view and explore JSON data.\n\n© 2025 Doug Johnson\nhttps://github.com/doubledeej/JSONViewer");
        }
    }
}
