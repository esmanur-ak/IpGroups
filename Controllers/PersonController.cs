using IpGroups.Constants;
using IpGroups.Models.ViewModels;
using IpGroups.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IpGroups.Controllers
{
    [Authorize(Policy = Permissions.Person.View)]
    public class PersonController : Controller
    {
        private readonly IPersonService _personService;
        private readonly IBinaService _binaService;
        private readonly IBirimService _birimService;
        private readonly IIpAddressService _ipAddressService;

        public PersonController(
            IPersonService personService,
            IBinaService binaService,
            IBirimService birimService,
            IIpAddressService ipAddressService)
        {
            _personService = personService;
            _binaService = binaService;
            _birimService = birimService;
            _ipAddressService = ipAddressService;
        }

        public async Task<IActionResult> Index()
        {
            var personeller = await _personService.GetAllWithDetailsAsync();
            var model = personeller.Select(PersonViewModel.FromEntity).ToList();
            await PopulateDropdownsAsync();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var person = await _personService.GetByIdWithDetailsAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(PersonViewModel.FromEntity(person));
        }

        // ---- CREATE ----
        [HttpPost]
        [Authorize(Policy = Permissions.Person.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            try
            {
                await _personService.AddAsync(model.ToEntity());
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ---- EDIT GET (AJAX) ----
        [HttpGet]
        [Authorize(Policy = Permissions.Person.Edit)]
        public async Task<IActionResult> GetEdit(int id)
        {
            var person = await _personService.GetByIdWithDetailsAsync(id);
            if (person == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            var vm = PersonViewModel.FromEntity(person);
            string currentIpText = person.IpAddress != null ? person.IpAddress.FullIpAddress : "";

            return Json(new { 
                success = true, 
                id = vm.Id, 
                ad = vm.Ad,
                soyad = vm.Soyad,
                domain = vm.Domain,
                binaId = vm.BinaId,
                birimId = vm.BirimId,
                ipAddressId = vm.IpAddressId,
                currentIpText = currentIpText
            });
        }

        // ---- EDIT POST ----
        [HttpPost]
        [Authorize(Policy = Permissions.Person.Edit)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonViewModel model)
        {
            if (id != model.Id)
                return Json(new { success = false, errors = new[] { "Geçersiz istek." } });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            try
            {
                await _personService.UpdateAsync(model.ToEntity());
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ---- DELETE GET (AJAX) ----
        [HttpGet]
        [Authorize(Policy = Permissions.Person.Delete)]
        public async Task<IActionResult> GetDelete(int id)
        {
            var person = await _personService.GetByIdWithDetailsAsync(id);
            if (person == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            var vm = PersonViewModel.FromEntity(person);
            return Json(new
            {
                success = true,
                recordId = vm.Id,
                adSoyad = $"{vm.Ad} {vm.Soyad}",
                domain = vm.Domain,
                createdDate = vm.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss")
            });
        }

        // ---- DELETE POST ----
        [HttpPost]
        [Authorize(Policy = Permissions.Person.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _personService.DeleteAsync(id);
            if (!result)
                return Json(new { success = false, message = "Kayıt bulunamadı veya silinemedi." });

            return Json(new { success = true });
        }

        private async Task PopulateDropdownsAsync(int? currentIpAddressId = null)
        {
            var binalar = await _binaService.GetAllAsync();
            var birimler = await _birimService.GetAllAsync();
            var allIpAddresses = await _ipAddressService.GetAllAsync();

            // Atanmamış IP adreslerini filtrele
            var unassignedIpAddresses = allIpAddresses
                .Where(ia => !ia.IsAssigned)
                .ToList();

            // Eğer düzenlemede kişinin mevcut bir IP'si varsa ve boştakiler listesinde değilse, onu da listeye ekliyoruz
            if (currentIpAddressId.HasValue)
            {
                var currentIp = await _ipAddressService.GetByIdAsync(currentIpAddressId.Value);
                if (currentIp != null && !unassignedIpAddresses.Any(ip => ip.Id == currentIpAddressId.Value))
                {
                    unassignedIpAddresses.Insert(0, currentIp);
                }
            }

            ViewData["BinaId"] = new SelectList(binalar, "Id", "Name");
            ViewData["BirimId"] = new SelectList(birimler, "Id", "Name");
            ViewData["IpAddressId"] = new SelectList(unassignedIpAddresses, "Id", "FullIpAddress");
        }
    }
}

