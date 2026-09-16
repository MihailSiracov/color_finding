using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ColorWordsVisualizer
{
    /// <summary>
    /// Ядро программы: словарь цветовых терминов, регулярное выражение
    /// для выделения слов и логика поиска цвета для слова.
    /// </summary>
    public static class ColorWordAnalyzer
    {
        /// <summary>
        /// Словарь соответствия «корень цветового термина → цвет».
        /// Сравнение регистронезависимое (OrdinalIgnoreCase).
        /// </summary>
        public static readonly Dictionary<string, Color> ColorMap =
            new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
            {
                { "красн",       Color.Red },
                { "ал",          Color.Crimson },
                { "бар",         Color.DarkRed },
                { "зелен",       Color.Green },
                { "изумруд",     Color.MediumSeaGreen },
                { "малахит",     Color.MediumSeaGreen },
                { "син",         Color.Blue },
                { "голуб",       Color.LightBlue },
                { "лазур",       Color.LightSkyBlue },
                { "ультрамарин", Color.Blue },
                { "желт",        Color.Yellow },
                { "золот",       Color.Gold },
                { "лимон",       Color.LemonChiffon },
                { "бел",         Color.White },
                { "черн",        Color.Black },
                { "сер",         Color.Gray },
                { "фиолетов",    Color.Purple },
                { "лилов",       Color.Purple },
                { "оранжев",     Color.Orange },
                { "коричнев",    Color.Brown },
                { "розов",       Color.Pink },
                { "бирюз",       Color.Turquoise },
            };

        /// <summary>
        /// Гласные буквы русского алфавита (в обоих регистрах).
        /// После корня цветового слова должна идти именно такая буква.
        /// </summary>
        private static readonly HashSet<char> Vowels = new HashSet<char>
        {
            'а', 'е', 'ё', 'и', 'о', 'у', 'ы', 'э', 'ю', 'я',
            'А', 'Е', 'Ё', 'И', 'О', 'У', 'Ы', 'Э', 'Ю', 'Я',
        };

        private static readonly Regex WordRegex =
            new Regex(@"\b[А-Яа-яЁё]+\b", RegexOptions.Compiled);

        /// <summary>
        /// Возвращает цвет для слова или <see cref="Color.Empty"/>,
        /// если слово не является цветовым термином.
        ///
        /// Правило:
        ///   1) слово должно начинаться с одного из корней ColorMap;
        ///   2) сразу после корня обязана стоять гласная буква;
        ///   3) если подходит несколько корней, берётся самый длинный.
        ///
        /// Примеры: «красное» → Red, «синий» → Blue, «белый» → White,
        ///          но «белка», «аллея», «серьёзный», «синька» — не цвета.
        /// </summary>
        public static Color FindColor(string word)
        {
            if (string.IsNullOrEmpty(word))
                return Color.Empty;

            Color best = Color.Empty;
            int bestLength = 0;

            foreach (var pair in ColorMap)
            {
                string root = pair.Key;

                if (word.Length <= root.Length)
                    continue;

                if (!word.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Проверяем, что сразу после корня идёт гласная.
                char after = word[root.Length];
                if (!Vowels.Contains(after))
                    continue;

                if (root.Length > bestLength)
                {
                    best = pair.Value;
                    bestLength = root.Length;
                }
            }

            return best;
        }

 
        public static IEnumerable<(string Text, Color Color)> Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            int lastIndex = 0;
            foreach (Match match in WordRegex.Matches(text))
            {
                if (match.Index > lastIndex)
                    yield return (text.Substring(lastIndex, match.Index - lastIndex), Color.Black);

                string word = match.Value;
                Color color = FindColor(word);
                yield return (word, color == Color.Empty ? Color.Black : color);

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < text.Length)
                yield return (text.Substring(lastIndex), Color.Black);
        }


        public static IEnumerable<(string Word, Color Color)> FindColorWords(string text)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            foreach (Match match in WordRegex.Matches(text))
            {
                Color color = FindColor(match.Value);
                if (color != Color.Empty)
                    yield return (match.Value, color);
            }
        }
    }
}