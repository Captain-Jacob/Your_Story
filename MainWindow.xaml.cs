using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; // CalendarDayButton
using System.Windows.Media;
using Your_Story.Models;
using Your_Story.Services;

namespace Your_Story
{
    public partial class MainWindow : Window
    {
        // --- Dil sözlükleri ---
        private Dictionary<string, string> _lang = new();
        private readonly Dictionary<string, Dictionary<string, string>> _langs =
            new()
            {
                ["tr"] = new()
                {
                    ["Header"] = "Bugün ne yaptın?",
                    ["Calendar"] = "Takvim",
                    ["Date"] = "Tarih:",
                    ["Long"] = "Uzunca Not:",
                    ["Final"] = "Final Yorum:",
                    ["Saved"] = "(Kaydedildi)",
                    ["WarnFill"] = "(Uyarı) Lütfen en az bir alan doldurun.",
                    ["ConfirmSave"] = "Kaydetmek istiyor musun?"
                },
                ["en"] = new()
                {
                    ["Header"] = "What did you do today?",
                    ["Calendar"] = "Calendar",
                    ["Date"] = "Date:",
                    ["Long"] = "Long Note:",
                    ["Final"] = "Final Comment:",
                    ["Saved"] = "(Saved)",
                    ["WarnFill"] = "(Warning) Please fill at least one field.",
                    ["ConfirmSave"] = "Do you want to save?"
                },
                ["ja"] = new()
                {
                    ["Header"] = "今日は何をした？",
                    ["Calendar"] = "カレンダー",
                    ["Date"] = "日付：",
                    ["Long"] = "長いメモ：",
                    ["Final"] = "最終コメント：",
                    ["Saved"] = "（保存しました）",
                    ["WarnFill"] = "（注意）少なくとも1つ入力してください。",
                    ["ConfirmSave"] = "保存しますか？"
                },
                ["de"] = new()
                {
                    ["Header"] = "Was hast du heute gemacht?",
                    ["Calendar"] = "Kalender",
                    ["Date"] = "Datum:",
                    ["Long"] = "Lange Notiz:",
                    ["Final"] = "Abschließender Kommentar:",
                    ["Saved"] = "(Gespeichert)",
                    ["WarnFill"] = "(Hinweis) Bitte fülle mindestens ein Feld aus.",
                    ["ConfirmSave"] = "Möchtest du speichern?"
                }
            };

        private HashSet<DateTime> _daysWithEntries = new();

        public MainWindow()
        {
            InitializeComponent();
            ApplyLanguage("tr");          // (varsayılan dil)
            CmbLang.SelectedIndex = 0;    // (seçimi koddan yap)

            Database.Ensure();               // DB tabloyu oluştur (yoksa)

            var today = DateTime.Today;
            Cal.SelectedDate = today;
            Dp.SelectedDate  = today;

            RefreshMonthColors(today);
            LoadDay(today);
        }

        // ========== Dil ==========
        private void ApplyLanguage(string code)
        {
            _lang = _langs.ContainsKey(code) ? _langs[code] : _langs["tr"];
            Title             = "Your Story";
            LblHeader.Text    = _lang["Header"];
            LblCalendar.Text  = _lang["Calendar"];
            LblDate.Text      = _lang["Date"];
            LblLong.Text      = _lang["Long"];
            LblFinal.Text     = _lang["Final"];
        }

        private void CmbLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return; // (erken çağrıyı es geç)
            if (CmbLang.SelectedItem is ComboBoxItem item && item.Tag is string code)
                ApplyLanguage(code);
        }

        // ========== Kaydet ==========
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                _lang["ConfirmSave"], "Your Story",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var day          = Dp.SelectedDate?.Date ?? DateTime.Today;
            var timeNote     = DateTime.Now.ToString("HH:mm"); // saat otomatik
            var longNote     = TxtLong.Text?.Trim()  ?? string.Empty;
            var finalComment = TxtFinal.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(longNote) && string.IsNullOrEmpty(finalComment))
            {
                LblStatus.Text = _lang["WarnFill"];
                return;
            }

            var entry = new Entry
            {
                Day          = day,
                TimeNote     = timeNote,
                LongNote     = longNote,
                FinalComment = finalComment,
                CreatedAt    = DateTime.Now
            };
            Database.Insert(entry);

            TxtLong.Clear();
            TxtFinal.Clear();
            LblStatus.Text = _lang["Saved"];

            LoadDay(day);
            RefreshMonthColors(day);
        }

        // ========== Gün yükleme ==========
        private void LoadDay(DateTime day)
        {
            var items = Database.GetByDate(day);
            Lb.ItemsSource = items;
        }

        private void RefreshMonthColors(DateTime reference)
        {
            _daysWithEntries = Database.GetDaysWithEntriesInMonth(reference);
            ApplyDayColors();
        }

        // ========== Eventler ==========
        private void Cal_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Cal.SelectedDate is DateTime d)
            {
                Dp.SelectedDate = d.Date;
                LoadDay(d.Date);
            }
        }

        private void Dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Dp.SelectedDate is DateTime d)
            {
                Cal.SelectedDate = d.Date;
                LoadDay(d.Date);
            }
        }

        private void Cal_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyDayColors();
        }

        private void Cal_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            var month = Cal.DisplayDate;
            RefreshMonthColors(month);
        }

        // ========== Gün renklendirme ==========
        private void ApplyDayColors()
        {
            if (!IsLoaded) return;

            Cal.Dispatcher.InvokeAsync(() =>
            {
                var dayButtons = FindVisualChildren<CalendarDayButton>(Cal).ToList();
                foreach (var b in dayButtons)
                {
                    if (b.DataContext is DateTime day)
                    {
                        b.ClearValue(BackgroundProperty);
                        b.ClearValue(BorderBrushProperty);

                        if (_daysWithEntries.Contains(day.Date))
                        {
                            var color = DayOfWeekColor(day.DayOfWeek);
                            b.Background = new SolidColorBrush(color);
                            b.BorderBrush = Brushes.Black;
                        }
                    }
                }
            });
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t) yield return t;
                foreach (var sub in FindVisualChildren<T>(child))
                    yield return sub;
            }
        }

        private static Color DayOfWeekColor(DayOfWeek d) =>
            d switch
            {
                DayOfWeek.Monday    => Colors.RoyalBlue,
                DayOfWeek.Tuesday   => Colors.Gold,
                DayOfWeek.Wednesday => Colors.SeaGreen,
                DayOfWeek.Thursday  => Colors.MediumPurple,
                DayOfWeek.Friday    => Colors.Orange,
                DayOfWeek.Saturday  => Colors.HotPink,
                DayOfWeek.Sunday    => Colors.IndianRed,
                _ => Colors.LightGray
            };

        // ========== Kayıt detayı (çift tık) ==========
        private void Lb_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Lb.SelectedItem is Entry entry)
            {
                var w = new EntryDetailWindow(entry) { Owner = this };
                w.ShowDialog();
            }
        }
    }
}
