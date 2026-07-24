using IpGroups.Models;
using IpGroups.Models.ViewModels;
using IpGroups.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IpGroups.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPersonService _personService;
        private readonly IBinaService _binaService;
        private readonly IBirimService _birimService;
        private readonly IIpService _ipService;

        public HomeController(
            IPersonService personService,
            IBinaService binaService,
            IBirimService birimService,
            IIpService ipService)
        {
            _personService = personService;
            _binaService = binaService;
            _birimService = birimService;
            _ipService = ipService;
        }

        public async Task<IActionResult> Index()
        {
            var persons = await _personService.GetAllWithDetailsAsync();
            var binalar = await _binaService.GetAllAsync();
            var birimler = await _birimService.GetAllAsync();
            var ipler = await _ipService.GetAllWithAddressesAsync();
            var ipAdresleriAtanmis = ipler.SelectMany(i => i.IpAddresses).Where(a => a.IsAssigned).Count();
            var ipAdresleriAtanmamis = ipler.SelectMany(i => i.IpAddresses).Where(a => !a.IsAssigned).Count();

            // Birime göre personel sayısı
            var personelByBirim = persons
                .Where(p => p.Birim != null)
                .GroupBy(p => p.Birim!.Name)
                .Select(g => new BirimPersonelStat
                {
                    BirimAdi = g.Key,
                    PersonelSayisi = g.Count()
                })
                .OrderByDescending(x => x.PersonelSayisi)
                .ToList();

            // Birimi olmayan personel varsa "Birim Atanmamış" grubuna ekle
            var birimAtanmamisCount = persons.Count(p => p.Birim == null);
            if (birimAtanmamisCount > 0)
            {
                personelByBirim.Add(new BirimPersonelStat
                {
                    BirimAdi = "Birim Atanmamış",
                    PersonelSayisi = birimAtanmamisCount
                });
            }

            // Son 5 eklenen personel (Id'ye göre azalan sırada)
            var recentPersons = persons
                .OrderByDescending(p => p.Id)
                .Take(5)
                .Select(p => new RecentPersonDto
                {
                    AdSoyad = $"{p.Ad} {p.Soyad}",
                    BirimAdi = p.Birim?.Name ?? "—",
                    BinaAdi = p.Bina?.Name ?? "—",
                    IpAdresi = p.IpAddress?.FullIpAddress ?? "—",
                    HasIp = p.IpAddress != null
                })
                .ToList();

            // Aylara göre personel dağılımı — yalnızca geçmiş + mevcut ay
            var trCulture = new System.Globalization.CultureInfo("tr-TR");
            var now = DateTime.Now;
            var personelByMonth = Enumerable.Range(1, now.Month)
                .Select(m => new MonthPersonelStat
                {
                    Ay = trCulture.DateTimeFormat.GetAbbreviatedMonthName(m),
                    PersonelSayisi = persons.Count(p => {
                        var effectiveDate = p.CreatedDate.Year < 2000 ? p.CreatedDate = now : p.CreatedDate;
                        // Sadece bu yıla ait veya geçmiş yıllardaki kayıtları aya göre grupla (veya test verisi ise bu aya ekle)
                        return (effectiveDate.Year == now.Year || effectiveDate.Year < now.Year) && effectiveDate.Month == m;
                    })
                })
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TotalPersonCount = persons.Count(),
                TotalBinaCount = binalar.Count(),
                TotalBirimCount = birimler.Count(),
                TotalIpCount = ipler.Count(),
                AssignedIpCount = persons.Count(p => p.IpAddressId.HasValue),
                UnassignedIpCount = persons.Count(p => !p.IpAddressId.HasValue),
                PersonelByBirim = personelByBirim,
                PersonelByMonth = personelByMonth,
                RecentPersons = recentPersons,
                IpGroupOccupancies = ipler.Select(ipGroup => new IpGroupOccupancyStat
                {
                    IpNo = ipGroup.IpNo,
                    TotalCapacity = ipGroup.IpAddresses.Count,
                    AssignedCount = ipGroup.IpAddresses.Count(a => a.IsAssigned)
                }).ToList()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
