using IpGroups.Constants;
using IpGroups.Models.ViewModels;
using IpGroups.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IpGroups.Controllers
{
    [Authorize(Policy = Permissions.Bina.View)]
    public class BinaController : Controller
    {
        private readonly IBinaService _binaService;

        public BinaController(IBinaService binaService)
        {
            _binaService = binaService;
        }

        public async Task<IActionResult> Index()
        {
            var binalar = await _binaService.GetAllAsync();
            var model = binalar.Select(BinaViewModel.FromEntity).ToList();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var bina = await _binaService.GetByIdAsync(id);
            if (bina == null)
            {
                return NotFound();
            }
            return View(BinaViewModel.FromEntity(bina));
        }

        // ---- CREATE ----
        [HttpPost]
        [Authorize(Policy = Permissions.Bina.Create)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BinaViewModel model)
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
                await _binaService.AddAsync(model.ToEntity());
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ---- EDIT GET (AJAX) ----
        [HttpGet]
        [Authorize(Policy = Permissions.Bina.Edit)]
        public async Task<IActionResult> GetEdit(int id)
        {
            var bina = await _binaService.GetByIdAsync(id);
            if (bina == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            var vm = BinaViewModel.FromEntity(bina);
            return Json(new { success = true, id = vm.Id, name = vm.Name });
        }

        // ---- EDIT POST ----
        [HttpPost]
        [Authorize(Policy = Permissions.Bina.Edit)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BinaViewModel model)
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
                await _binaService.UpdateAsync(model.ToEntity());
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ---- DELETE GET (AJAX) ----
        [HttpGet]
        [Authorize(Policy = Permissions.Bina.Delete)]
        public async Task<IActionResult> GetDelete(int id)
        {
            var bina = await _binaService.GetByIdAsync(id);
            if (bina == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            var vm = BinaViewModel.FromEntity(bina);
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
        [Authorize(Policy = Permissions.Bina.Delete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _binaService.DeleteAsync(id);
            if (!result)
                return Json(new { success = false, message = "Kayıt bulunamadı veya silinemedi." });

            return Json(new { success = true });
        }
    }
}
