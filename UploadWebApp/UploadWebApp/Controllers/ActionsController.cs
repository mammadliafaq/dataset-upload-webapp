using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using UploadWebApp.Data;
using UploadWebApp.Models;
using UploadWebApp.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace UploadWebApp.Controllers
{
    public class ActionsController : Controller
    {
        private readonly UploadingContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly long _fileSizeLimit;
        private readonly ILogger<ActionsController> _logger;
        private readonly string[] _permittedExtensions = { ".rar", ".zip" };
        private readonly string _targetFilePath;
        private readonly int _minValue = 2000;
        private readonly int _maxValue = 3000;


        // Get the default form options so that we can use them to set the default 
        // limits for request body data.
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public ActionsController(ILogger<ActionsController> logger,
            UploadingContext context, IWebHostEnvironment appEnvironment, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _appEnvironment = appEnvironment;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

            // To save physical files to a path provided by configuration:
            _targetFilePath = config.GetValue<string>("StoredFilesPath") + "/ASPUploads";
        }

        // GET: Actions
        public async Task<IActionResult> Index()
        {
            CommonViewModel model = new CommonViewModel();

            model.FileData = new FileData();

            model.DataSet = await _context.FileData.ToListAsync();

            return View(model);
            // return View(await _context.FileData.ToListAsync());
        }

        // GET: Actions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fileData = await _context.FileData
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fileData == null)
            {
                return NotFound();
            }

            return View(fileData);
        }

        // GET: Actions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actions/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483647)]
        public async Task<IActionResult> Create(CommonViewModel model,
           IFormFile postedFile)
        {
            ModelState.Clear();

            FileData fileData = model.FileData;

            CommonViewModel commonView = new CommonViewModel();

            commonView.FileData = fileData;

            commonView.DataSet = await _context.FileData.ToListAsync();

            if (postedFile == null)
            {
                ModelState.AddModelError(fileData.Name, "Выберите файл для загрузки");
                return View("Index", commonView);
            }

            try
            {
                var streamedFileContent = new byte[0];
                var ext = Path.GetExtension(postedFile.FileName).ToLowerInvariant();
                var trustedFileNameForFileStorage = Path.Combine(_targetFilePath, fileData.Name + ext);

                if (!Directory.Exists(_targetFilePath)) Directory.CreateDirectory(_targetFilePath);

                streamedFileContent = await FileHelpers.ProcessedFormFile<ActionsController>(fileData, postedFile, ModelState,
                    _permittedExtensions, _minValue, _maxValue, _targetFilePath, trustedFileNameForFileStorage);


                if (ModelState.IsValid)
                {

                    using (var targetStream = System.IO.File.Create(
                    Path.Combine(_targetFilePath, trustedFileNameForFileStorage)))
                    {
                        await targetStream.WriteAsync(streamedFileContent);

                    }

                    _context.Add(fileData);
                    await _context.SaveChangesAsync();

                    ViewBag.Message = "Файл заргужен в систему успешно.";

                    return RedirectToAction(nameof(Index));
                }
                else
                {

                    return View("Index", commonView);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(fileData.Name, "Не удалось загрузить файл. " +
                        $"Пожалуйста, свяжитесь с поддержкой. Ошибка: " + ex);
                return View("Index", commonView);

            }


        }



        // GET: Actions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fileData = await _context.FileData.FindAsync(id);
            if (fileData == null)
            {
                return NotFound();
            }
            return View(fileData);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreationDate,ContainsCyrillic,ContainsLatin,ContainsNumbers,ContainsSpChar")] FileData fileData)
        {
            if (id != fileData.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fileData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FileDataExists(fileData.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fileData);
        }

        // GET: Actions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fileData = await _context.FileData
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fileData == null)
            {
                return NotFound();
            }

            return View(fileData);
        }

        // POST: Actions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fileData = await _context.FileData.FindAsync(id);
            _context.FileData.Remove(fileData);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FileDataExists(int id)
        {
            return _context.FileData.Any(e => e.Id == id);
        }
    }
}
