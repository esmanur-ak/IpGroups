using IpGroups.Constants;
using IpGroups.Models.ViewModels;
using IpGroups.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IpGroups.Controllers
{
    [Authorize(Policy = Permissions.Ip.View)]
    public class IpController : Controller
    {
        private readonly IIpService _ipService;
        private readonly IIpAddressService _ipAddressService;

        public IpController(IIpService ipService, IIpAddressService ipAddressService)
        {
            _ipService = ipService;
            _ipAddressService = ipAddressService;
        }

        public async Task<IActionResult> Index()
        {
            // IP grupları yüklenirken veritabanını senkronize et (eski adresleri temizle, eksikleri ekle)
            await _ipAddressService.SyncAllIpAddressesAsync();

            var ipler = await _ipService.GetAllAsync();
            var model = ipler.Select(IpViewModel.FromEntity).ToList();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ip = await _ipService.GetByIdAsync(id);
            if (ip == null)
            {
                return NotFound();
            }

            var ipAddresses = await _ipAddressService.GetByIpGroupIdAsync(id);
            ViewBag.IpAddresses = ipAddresses.Select(IpAddressViewModel.FromEntity).ToList();

            return View(IpViewModel.FromEntity(ip));
        }

        // ---- CREATE ----
        [HttpPost]
        [Authorize(Policy = Permissions.Ip.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IpViewModel model)
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
                await _ipService.AddAsync(model.ToEntity());
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ---- EDIT GET (AJAX) ----
        [HttpGet]
        [Authorize(Policy = Permissions.Ip.Edit)]
        public async Task<IActionResult> GetEdit(int id)
        {
            var ip = await _ipService.GetByIdAsync(id);
            if (ip == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            var vm = IpViewModel.FromEntity(ip);
            return Json(new { success = true, id = vm.Id, ipNo = vm.IpNo, description = vm.Description });
        }

        // ---- EDIT POST ----
        [HttpPost]
        [Authorize(Policy = Permissions.Ip.Edit)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IpViewModel model)
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
                await _ipService.UpdateAsync(model.ToEntity());
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ---- DELETE GET (AJAX) ----
        [HttpGet]
        [Authorize(Policy = Permissions.Ip.Delete)]
        public async Task<IActionResult> GetDelete(int id)
        {
            var ip = await _ipService.GetByIdAsync(id);
            if (ip == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            var vm = IpViewModel.FromEntity(ip);
            return Json(new
            {
                success = true,
                recordId = vm.Id,
                ipNo = vm.IpNo,
                description = string.IsNullOrEmpty(vm.Description) ? "-" : vm.Description,
                createdDate = vm.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss")
            });
        }

        // ---- DELETE POST ----
        [HttpPost]
        [Authorize(Policy = Permissions.Ip.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _ipService.DeleteAsync(id);
            if (!result)
                return Json(new { success = false, message = "Kayıt bulunamadı veya silinemedi." });

            return Json(new { success = true });
        }
    }
}
