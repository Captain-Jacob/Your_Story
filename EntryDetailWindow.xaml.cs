using System.Windows;
using Your_Story.Models;

namespace Your_Story
{
    public partial class EntryDetailWindow : Window
    {
        public EntryDetailWindow()   // <-- parametresiz
        {
            InitializeComponent();
        }
        public EntryDetailWindow(Entry e) : this() // <-- mevcut akış
        {
            LblTime.Text      = $"{e.Day:yyyy-MM-dd}  {e.TimeNote}";
            TxtLongRead.Text  = e.LongNote;
            TxtFinalRead.Text = e.FinalComment;
            LblCreated.Text   = $"Kaydedildi: {e.CreatedAt:yyyy-MM-dd HH:mm}";
        }
    }
}
