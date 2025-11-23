using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Janphe.Fantasy.Map;
using SkiaSharp;

namespace FantasyMap.WinForms
{
    public class MapPreviewForm : Form
    {
        private readonly PictureBox _previewBox;
        private readonly Button _generateButton;
        private readonly Button _saveButton;
        private readonly NumericUpDown _seedInput;
        private readonly Label _statusLabel;

        public MapPreviewForm()
        {
            Text = "Fantasy Map DLL Preview";
            MinimumSize = new Size(900, 700);

            var controls = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(10),
                AutoSize = true,
            };

            _generateButton = new Button { Text = "Generate Map", AutoSize = true };
            _saveButton = new Button { Text = "Save PNG", AutoSize = true, Enabled = false };
            _seedInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 999999,
                Value = 1,
                Width = 100,
            };

            controls.Controls.Add(new Label { Text = "Seed:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft });
            controls.Controls.Add(_seedInput);
            controls.Controls.Add(_generateButton);
            controls.Controls.Add(_saveButton);

            _statusLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                Padding = new Padding(10, 0, 0, 5),
                Text = "Ready to render",
                AutoSize = false,
            };

            _previewBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom,
            };

            Controls.Add(_previewBox);
            Controls.Add(_statusLabel);
            Controls.Add(controls);

            Load += async (_, _) => await GenerateMapAsync();
            _generateButton.Click += async (_, _) => await GenerateMapAsync();
            _saveButton.Click += (_, _) => SavePreview();
        }

        private async Task GenerateMapAsync()
        {
            ToggleUi(false);
            _statusLabel.Text = "Generating map...";
            var seed = (int)_seedInput.Value;

            try
            {
                var bitmap = await Task.Run(() => RenderBitmap(seed));
                _previewBox.Image?.Dispose();
                _previewBox.Image = bitmap;
                _statusLabel.Text = $"Map rendered with seed {seed} at {DateTime.Now:T}";
                _saveButton.Enabled = true;
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Render failed: {ex.Message}";
                MessageBox.Show(this, ex.Message, "Render error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUi(true);
            }
        }

        private Bitmap RenderBitmap(int seed)
        {
            var width = Math.Max(1, _previewBox.Width);
            var height = Math.Max(1, _previewBox.Height);

            using var renderer = new MapPreview(width, height, seed);
            using var skBitmap = renderer.Render();
            return ConvertToBitmap(skBitmap);
        }

        private static Bitmap ConvertToBitmap(SKBitmap skBitmap)
        {
            using var image = SKImage.FromBitmap(skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new MemoryStream(data.ToArray());
            return new Bitmap(stream);
        }

        private void SavePreview()
        {
            if (_previewBox.Image == null)
            {
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                FileName = "fantasy-map.png",
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _previewBox.Image.Save(dialog.FileName);
                _statusLabel.Text = $"Saved to {dialog.FileName}";
            }
        }

        private void ToggleUi(bool enabled)
        {
            _generateButton.Enabled = enabled;
            _seedInput.Enabled = enabled;
        }
    }
}
