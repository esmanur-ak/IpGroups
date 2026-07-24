using IpGroups.Constants;
using IpGroups.Models.ViewModels;
using IpGroups.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IpGroups.Controllers
{
    [Authorize(Policy = Permissions.Birim.View)]
    public class BirimController : Controller
    {
        private readonly IBirimService _birimService;

        public BirimController(IBirimService birimService)
        {
            _birimService = birimService;
        }

        public async Task<IActionResult> Index()
        {
            var birimler = await _birimService.GetAllAsync();
            var model = birimler.Select(BirimViewModel.FromEntity).ToList();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var birim = await _birimService.GetByIdAsync(id);
            if (birim == null)
            {
                return NotFound();
            }
            return View(BirimViewModel.FromEntity(birim));
        }

        // ---- CREATE ----
        [HttpPost]
        [Authorize(Policy = Permissions.Birim.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BirimViewModel model)
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
                await _birimService.AddAsync(model.ToEntity());
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ---- EDIT GET (AJAX) ----
        [HttpGet]
        [Authorize(Policy = Permissions.Birim.Edit)]
        public async Task<IActionResult> GetEdit(int id)
        {
            var birim = await _birimService.GetByIdAsync(id);
            if (birim == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            var vm = BirimViewModel.FromEntity(birim);
            return Json(new { success = true, id = vm.Id, name = vm.Name });
        }

        // ---- EDIT POST ----
        [HttpPost]
        [Authorize(Policy = Permissions.Birim.Edit)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BirimViewModel model)
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
                await _birimService.UpdateAsync(model.ToEntity());
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ---- DELETE GET (AJAX) ----
        [HttpGet]
        [Authorize(Policy = Permissions.Birim.Delete)]
        public async Task<IActionResult> GetDelete(int id)
        {
            var birim = await _birimService.GetByIdAsync(id);
            if (birim == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            var vm = BirimViewModel.FromEntity(birim);
            return Json(new
            {
                success = true,
                recordId = vm.Id,
                name = vm.Name,
                createdDate = vm.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss")
            });
        }

        // ---- DELETE POST ----
        [HttpPost]
        [Authorize(Policy = Permissions.Birim.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _birimService.DeleteAsync(id);
            if (!result)
                return Json(new { success = false, message = "Kayıt bulunamadı veya silinemedi." });

            return Json(new { success = true });
        }
    }
}
