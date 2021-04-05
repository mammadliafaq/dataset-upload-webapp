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
using UploadWebApp.Filters;
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
            return View(await _context.FileData.ToListAsync());
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
        [RequestFormLimits(MultipartBodyLengthLimit = 268435456)]
        public async Task<IActionResult> Create([Bind("Id,Name,CreationDate,ContainsCyrillic,ContainsLatin,ContainsNumbers,ContainsSpChar")] FileData fileData,
           IFormFile postedFile, int SelectedAnswerId)
        {
            var selectedValue = SelectedAnswerId;



            try
            {
                long size = postedFile.Length;



                int numberOfPict = 0;

                int counter = 0;



                var streamedFileContent = new byte[0];



                string folderName = fileData.Name + "/";
                var ext = Path.GetExtension(postedFile.FileName).ToLowerInvariant();
                var trustedFileNameForFileStorage = Path.Combine(_targetFilePath, folderName + fileData.Name + ext);

                streamedFileContent = await FileHelpers.ProcessedFormFile<ActionsController>(fileData, postedFile, ModelState, _permittedExtensions,
                    SelectedAnswerId, _minValue, _maxValue);

                if (!ModelState.IsValid)
                {
                    ViewBag.Message = "File upload failed!!" + ModelState;
                    return View(fileData);
                }

                if (!Directory.Exists(Path.Combine(_targetFilePath, folderName)))
                    Directory.CreateDirectory(Path.Combine(_targetFilePath, folderName));



                ViewBag.Message = "File uploaded successfully.";

                if (ModelState.IsValid)
                {
                    using (var targetStream = System.IO.File.Create(
                    Path.Combine(_targetFilePath, trustedFileNameForFileStorage)))
                    {
                        await targetStream.WriteAsync(streamedFileContent);

                    }

                    _context.Add(fileData);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return View(fileData);
                }


                //using (var stream = System.IO.File.Create(trustedFileNameForFileStorage))
                //{
                //    await postedFile.CopyToAsync(stream);
                //}


                //using ZipArchive archive = ZipFile.OpenRead(trustedFileNameForFileStorage);

                //foreach (ZipArchiveEntry entry in archive.Entries)
                //{

                //    var insideFileExtension = Path.GetExtension(entry.FullName);
                //    if (!entry.FullName.StartsWith("_"))
                //    {
                //        if (entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                //                                       || entry.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                //                                       || entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))

                //        {
                //            numberOfPict++;
                //        }
                //        else
                //        {
                //            if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                //            {

                //                string destinationPath = Path.GetFullPath(Path.Combine(_targetFilePath, folderName + "/answers.txt"));


                //                if (destinationPath.StartsWith(_targetFilePath, StringComparison.Ordinal))
                //                    entry.ExtractToFile(destinationPath);

                //                string line;

                //                System.IO.StreamReader file = new System.IO.StreamReader(destinationPath);

                //                while ((line = file.ReadLine()) != null)
                //                {
                //                    System.Console.WriteLine(line);
                //                    counter++;
                //                }

                //                file.Close();

                //            }

                //        }
                //    }

                //}

            }
            catch (Exception message)
            {
                ViewBag.Message = "File upload failed!!" + message;

            }

            return View(fileData);
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

        // POST: Actions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
