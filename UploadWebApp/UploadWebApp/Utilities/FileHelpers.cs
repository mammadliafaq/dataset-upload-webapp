using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using UploadWebApp.Models;

namespace UploadWebApp.Utilities
{
    public static class FileHelpers
    {

        private static readonly byte[] _allowedChars = { };

        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
        };


        public static async Task<byte[]> ProcessedFormFile<T>(FileData data, IFormFile formFile, ModelStateDictionary modelState,
            string[] permittedExtensions, int selectedAnswerValue, int minValue,
            int maxValue)

        {
            long numberOfPict = 0, numberOfTxtFiles = 0, numberOfPicAnswPairs = 0;

            var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                formFile.FileName);
            if (CheckForValidation(data, formFile, modelState, permittedExtensions))
            {
                try
                {

                    using (var memoryStream = new MemoryStream())
                    {
                        await formFile.CopyToAsync(memoryStream);

                        // Check the content length in case the file's only
                        // content was a BOM and the content is actually
                        // empty after removing the BOM.
                        if (memoryStream.Length == 0)
                        {
                            modelState.AddModelError(formFile.Name, "File is empty.");
                        }


                        using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                        {
                            foreach (var entry in zip.Entries)
                            {
                                if (!entry.FullName.StartsWith("_"))
                                {
                                    if (entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                                           || entry.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                                           || entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))

                                    {
                                        numberOfPict++;
                                    }
                                    else if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                                    {
                                        numberOfTxtFiles++;

                                        using (var fileStream = entry.Open())
                                        {
                                            using (var file = new System.IO.StreamReader(fileStream))
                                            {
                                                while (file.ReadLine() != null)
                                                {
                                                    numberOfPicAnswPairs++;
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        modelState.AddModelError(formFile.Name, "bu fayil bize leazim deyil ");
                                        break;
                                    }
                                }
                            }

                            if (CheckForRequirements(data, numberOfPict, numberOfTxtFiles, numberOfPicAnswPairs, selectedAnswerValue,
                                minValue, maxValue, modelState) && modelState.IsValid)
                            {

                                return memoryStream.ToArray();
                            }
                            else
                            {

                                return new byte[0];
                            }


                        }

                    }


                }
                catch (Exception ex)
                {
                    modelState.AddModelError(formFile.Name, "File upload failed. " +
                        $"Please contact the Help Desk for support. Error: {ex.HResult}");

                }
            }

            return new byte[0];
        }


        private static bool CheckForRequirements(FileData data, long numbOfPictures, long numbOfTetxFiles, long numberOfTxtLines, int selectedAnswerValue, int minValue,
            int maxValue, ModelStateDictionary modelState)

        {

            if (data.ContainsCyrillic)
            {
                minValue = minValue + 3000;
                maxValue = maxValue + 3000;
            }

            if (data.ContainsLatin)
            {
                minValue = minValue + 3000;
                maxValue = maxValue + 3000;
            }

            if (data.ContainsNumbers)
            {
                minValue = minValue + 3000;
                maxValue = maxValue + 3000;
            }

            if (data.ContainsSpChar)
            {
                minValue = minValue + 3000;
                maxValue = maxValue + 3000;
            }

            //if (!(numbOfPictures > minValue && numbOfPictures < maxValue))
            //{
            //    modelState.AddModelError("File", "Количество картинок, находящихся в архиве, начинается c диапазон");
            //    return false;
            //}

            if (!(selectedAnswerValue == 2 && numbOfPictures == numberOfTxtLines))
            {
                modelState.AddModelError("File", "Количество ответов совпадает с количеством картинок, если указано расположение ответов “в файле”");
                return false;
            }

            if (selectedAnswerValue == 2 && numbOfTetxFiles == 0)
            {
                modelState.AddModelError("File", "Присутствует файл с ответами, если указано, что ответы в файле");
                return false;
            }

            return true;
        }

        private static bool CheckForValidation(FileData data, IFormFile postedFile, ModelStateDictionary modelState, string[] permittedExtensions)
        {

            if (data.Name.Contains("captcha"))
            {
                modelState.AddModelError("File", " Имя не может содержать слово “captcha”");

                return false;
            }
            if (data.ContainsCyrillic == false && data.ContainsLatin == false && data.ContainsNumbers == false)
            {
                modelState.AddModelError("File", "Выбрано как минимум одно из: “Содержит кириллицу”, “Содержит латиницу”,“Содержит цифры”");

                return false;
            }

            if (string.IsNullOrEmpty(data.Name) || postedFile == null || postedFile.Length == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(postedFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                modelState.AddModelError(data.Name, " file " +
                    "type isn't permitted or the file's signature " +
                    "doesn't match the file's extension.");
                return false;

            }

            if (postedFile.Length == 0)
            {
                modelState.AddModelError(data.Name, "File is empty.");

                return false;
            }

            return true;

        }
    }
}
