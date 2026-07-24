namespace IpGroups.Models.ViewModels
{
    public class DashboardViewModel
    {
        // KPI Kartları
        public int TotalPersonCount { get; set; }
        public int TotalBinaCount { get; set; }
        public int TotalBirimCount { get; set; }
        public int TotalIpCount { get; set; }

        // IP Durum İstatistikleri
        public int AssignedIpCount { get; set; }
        public int UnassignedIpCount { get; set; }

        // Birima Göre Personel Dağılımı (Bar/Donut Chart)
        public List<BirimPersonelStat> PersonelByBirim { get; set; } = new();

        // Aylara Göre Personel Dağılımı (Line Chart)
        public List<MonthPersonelStat> PersonelByMonth { get; set; } = new();

        // Son eklenen personeller (Özet tablo)
        public List<RecentPersonDto> RecentPersons { get; set; } = new();

        // IP Gruplarının Doluluk Oranı (Bar Chart)
        public List<IpGroupOccupancyStat> IpGroupOccupancies { get; set; } = new();
    }

    public class BirimPersonelStat
    {
        public string BirimAdi { get; set; } = string.Empty;
        public int PersonelSayisi { get; set; }
    }

    public class MonthPersonelStat
    {
        public string Ay { get; set; } = string.Empty;   // "Oca", "Şub" vb.
        public int PersonelSayisi { get; set; }
    }

    public class RecentPersonDto
    {
        public string AdSoyad { get; set; } = string.Empty;
        public string BirimAdi { get; set; } = string.Empty;
        public string BinaAdi { get; set; } = string.Empty;
        public string IpAdresi { get; set; } = string.Empty;
        public bool HasIp { get; set; }
    }

    public class IpGroupOccupancyStat
    {
        public string IpNo { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }
        public int AssignedCount { get; set; }
    }
}
