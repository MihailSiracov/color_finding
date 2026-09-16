using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ColorWordsVisualizer
{
    /// <summary>
    /// Отдельное окно: сетка цветных квадратиков (в порядке появления
    /// слов в тексте) + боковая панель со списком самих цветовых слов.
    /// </summary>
    public sealed class ColorMosaicForm : Form
    {
        private const int CellSize = 22;   // размер квадратика в мозаике
        private const int Columns  = 20;   // столбцов в мозаике
        private const int Gap      = 1;    // зазор между квадратиками

        private const int SwatchSize = 14; // размер цветного образца в списке
        private const int ItemHeight = 22; // высота строки в списке

        private readonly IReadOnlyList<(string Word, Color Color)> _items;
        private readonly Panel _canvas;
        private readonly ListBox _wordList;

        public ColorMosaicForm(IReadOnlyList<(string Word, Color Color)> items)
        {
            _items = items ?? Array.Empty<(string, Color)>();

            Text = "Мозаика цветов";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(780, 620);
            MinimumSize = new Size(420, 260);
            Font = new Font("Segoe UI", 10f);

            // ---------- Мозаика ----------
            int rows = Math.Max(1, (_items.Count + Columns - 1) / Columns);
            int contentWidth  = Columns * (CellSize + Gap);
            int contentHeight = rows    * (CellSize + Gap);

            _canvas = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(contentWidth + 2, contentHeight + 2),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            _canvas.Paint += OnCanvasPaint;

            var mosaicScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0),
            };
            mosaicScroll.Controls.Add(_canvas);

            // ---------- Список слов справа ----------
            _wordList = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = ItemHeight,
                IntegralHeight = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(248, 248, 248),
                Font = new Font("Segoe UI", 10f),
            };
            _wordList.DrawItem += OnWordListDrawItem;

            var listPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 240,
                Padding = new Padding(0),
                BackColor = Color.FromArgb(248, 248, 248),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var listTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "  Цветовые слова",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(232, 232, 232),
            };

            listPanel.Controls.Add(_wordList);
            listPanel.Controls.Add(listTitle);

            // ---------- Нижняя строка со статистикой ----------
            var info = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(245, 245, 245),
                Text = $"Найдено цветовых терминов: {_items.Count}   " +
                       $"(столбцов: {Columns}, строк: {rows})",
            };

            // ---------- Сборка формы ----------
            Controls.Add(mosaicScroll);
            Controls.Add(listPanel);
            Controls.Add(info);

            // Заполняем список слов.
            PopulateWordList();
        }

        // ================== Мозаика ==================

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            using var border = new Pen(Color.FromArgb(90, 90, 90));

            for (int i = 0; i < _items.Count; i++)
            {
                int col = i % Columns;
                int row = i / Columns;
                int x = 1 + col * (CellSize + Gap);
                int y = 1 + row * (CellSize + Gap);

                var rect = new Rectangle(x, y, CellSize, CellSize);

                using (var brush = new SolidBrush(_items[i].Color))
                    e.Graphics.FillRectangle(brush, rect);

                e.Graphics.DrawRectangle(border, rect);
            }
        }

        // ================== Список слов ==================

        private void PopulateWordList()
        {
            _wordList.BeginUpdate();
            _wordList.Items.Clear();

            if (_items.Count == 0)
            {
                _wordList.Items.Add("(ничего не найдено)");
            }
            else
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    // Формат: "1. красное"
                    _wordList.Items.Add($"{i + 1}. {_items[i].Word}");
                }
            }

            _wordList.EndUpdate();
        }

        private void OnWordListDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Фон строки.
            bool hasColor = e.Index < _items.Count && _items.Count > 0;
            Color background = (e.State & DrawItemState.Selected) != 0
                ? Color.FromArgb(200, 220, 245)
                : _wordList.BackColor;

            using (var bg = new SolidBrush(background))
                e.Graphics.FillRectangle(bg, e.Bounds);

            if (!hasColor)
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    _wordList.Items[e.Index]?.ToString() ?? "",
                    e.Font,
                    new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 6, e.Bounds.Height),
                    Color.Gray,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                return;
            }

            var item = _items[e.Index];

            // Цветной образец слева.
            int swatchY = e.Bounds.Y + (e.Bounds.Height - SwatchSize) / 2;
            var swatchRect = new Rectangle(e.Bounds.X + 6, swatchY, SwatchSize, SwatchSize);
            using (var brush = new SolidBrush(item.Color))
                e.Graphics.FillRectangle(brush, swatchRect);
            using (var pen = new Pen(Color.FromArgb(120, 120, 120)))
                e.Graphics.DrawRectangle(pen, swatchRect);

            // Текст слова.
            var textRect = new Rectangle(
                swatchRect.Right + 8,
                e.Bounds.Y,
                e.Bounds.Width - swatchRect.Right - 8 - 4,
                e.Bounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                _wordList.Items[e.Index]?.ToString() ?? "",
                e.Font,
                textRect,
                Color.Black,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }
}