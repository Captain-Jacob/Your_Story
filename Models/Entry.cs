using System;

namespace Your_Story.Models
{
    public class Entry
    {
        public long Id { get; set; }
        public DateTime Day { get; set; }            // sadece tarih (yyyy-MM-dd)
        public string TimeNote { get; set; } = "";   // saat/gün notu
        public string LongNote { get; set; } = "";   // uzun not
        public string FinalComment { get; set; } = "";// final yorum
        public DateTime CreatedAt { get; set; }      // kayıt zamanı
    }
}