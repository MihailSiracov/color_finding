using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ColorWordsVisualizer
{
    public sealed class MainForm : Form
    {
        private readonly RichTextBox _textBox;
        private readonly Button _openButton;
        private readonly Button _mosaicButton;
        private readonly Label _statusLabel;
        private readonly OpenFileDialog _openFileDialog;

        private List<(string Word, Color Color)> _colorWords = new();

        public MainForm()
        {
            Text = "Визуализация цветовых терминов";
            ClientSize = new Size(1000, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10f);

            // ---------- Текст без подсветки ----------
            _textBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 13f),
                ReadOnly = true,
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                WordWrap = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = false,
            };

            // ---------- Верхняя панель управления ----------
            _openButton = new Button
            {
                Text = "Открыть файл...",
                AutoSize = true,
                Padding = new Padding(8, 3, 8, 3),
            };
            _openButton.Click += OnOpenButtonClick;

            _mosaicButton = new Button
            {
                Text = "Показать мозаику",
                AutoSize = true,
                Padding = new Padding(8, 3, 8, 3),
                Enabled = false,
            };
            _mosaicButton.Click += OnMosaicButtonClick;

            _statusLabel = new Label
            {
                AutoSize = true,
                Text = "Файл не загружен",
                Margin = new Padding(14, 14, 0, 0),
            };

            var topPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(8),
            };
            topPanel.Controls.Add(_openButton);
            topPanel.Controls.Add(_mosaicButton);
            topPanel.Controls.Add(_statusLabel);

            Controls.Add(_textBox);
            Controls.Add(topPanel);

            _openFileDialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                Title = "Выберите текстовый файл",
            };
        }

        // ================== Загрузка файла ==================

        private void OnOpenButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_openFileDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                string path = _openFileDialog.FileName;

                _statusLabel.Text = "Чтение файла...";
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                string text = File.ReadAllText(path);

                // Показываем текст как есть — без подсветки.
                _textBox.Clear();
                _textBox.ForeColor = Color.Black;
                _textBox.Text = text;

                // Собираем список цветовых слов только для мозаики.
                _colorWords = ColorWordAnalyzer.FindColorWords(text).ToList();
                _mosaicButton.Enabled = _colorWords.Count > 0;

                _statusLabel.Text = $"Файл: {Path.GetFileName(path)}  " +
                                    $"(цветовых терминов: {_colorWords.Count})";
                Text = $"Визуализация — {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Произошла ошибка:\n\n{ex}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ================== Открытие окна мозаики ==================

        private void OnMosaicButtonClick(object sender, EventArgs e)
        {
            if (_colorWords.Count == 0)
            {
                MessageBox.Show(this,
                    "В тексте не найдено цветовых терминов.",
                    "Мозаика",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var mosaic = new ColorMosaicForm(_colorWords);
            mosaic.ShowDialog(this);
        }
    }
}