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


        public static async Task<byte[]> ProcessedFormFile<T>(FileData data, IFormFile formFile, ModelStateDictionary modelState,
            string[] permittedExtensions, int minValue, int maxValue, string targetFilePath, string trustedFileNameForFileStorage)

        {
            long numberOfPict = 0, numberOfTxtFiles = 0, numberOfOthrFiles = 0, numberOfPicAnswPairs = 0;

            var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                formFile.FileName);
            if (CheckForValidation(data, formFile, modelState, permittedExtensions, targetFilePath, trustedFileNameForFileStorage))
            {
                try
                {

                    using var memoryStream = new MemoryStream();
                    await formFile.CopyToAsync(memoryStream);


                    if (memoryStream.Length == 0)
                    {
                        modelState.AddModelError(trustedFileNameForDisplay, "Загружаемый файл пуст");
                    }


                    using var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read);
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

                                using var fileStream = entry.Open();
                                using var file = new System.IO.StreamReader(fileStream);
                                while (file.ReadLine() != null)
                                {
                                    numberOfPicAnswPairs++;
                                }

                            }
                            else
                            {
                                numberOfOthrFiles++;
                            }

                        }
                    }

                    long[] numbers = new long[] { numberOfPict, numberOfTxtFiles, numberOfOthrFiles, numberOfPicAnswPairs };

                    if (CheckForRequirements(data, numbers,
                        minValue, maxValue, modelState) && modelState.IsValid)
                    {

                        return memoryStream.ToArray();
                    }
                    else
                    {

                        return Array.Empty<byte>();
                    }


                }
                catch (Exception ex)
                {
                    modelState.AddModelError(trustedFileNameForDisplay, "Не удалось загрузить файл. " +
                        $"Пожалуйста, свяжитесь с поддержкой. Ошибка: " + ex);

                }
            }

            return Array.Empty<byte>();
        }


        private static bool CheckForRequirements(FileData data, long[] counts, int minValue,
            int maxValue, ModelStateDictionary modelState)

        {

            if (data.ContainsCyrillic)
            {
                minValue += +3000;
                maxValue += +3000;
            }

            if (data.ContainsLatin)
            {
                minValue += +3000;
                maxValue += +3000;
            }

            if (data.ContainsNumbers)
            {
                minValue += +3000;
                maxValue += +3000;
            }

            if (data.ContainsSpChar)
            {
                minValue += 3000;
                maxValue += 3000;
            }

            if (data.HasRegistrSensitivity)
            {
                minValue += 3000;
                maxValue += 3000;
            }

            if (counts[0] == 0 && counts[2] != 0)
            {

                modelState.AddModelError("File", "Неправильный формат входных данных");
                return false;

            }


            if (!(counts[0] > minValue && counts[0] < maxValue))
            {
                modelState.AddModelError("File", "Количество картинок должно находится в диапазоне от " + minValue +
                  " до " + maxValue +
                    ". Количество изображений в вашем архиве: " + counts[0].ToString());
                return false;
            }

            if (data.SelectedAnswerId == 2)
            {
                if (counts[1] == 0)
                {
                    modelState.AddModelError("File", "Указано, что ответы в файле, но файл в архиве отстутствует");
                    return false;
                }
                else if (counts[0] != counts[3])
                {
                    modelState.AddModelError("File", "Количество ответов не совпадает с количеством картинок. " +
                       "Количество картинок в архиве: " + counts[0].ToString() + "; Количество ответов: " + counts[3].ToString());
                    return false;
                }
            }

            return true;
        }

        private static bool CheckForValidation(FileData data, IFormFile postedFile, ModelStateDictionary modelState,
            string[] permittedExtensions, string targetFilePath, string trustedFileNameForFileStorage)
        {

            if (data.Name.Contains("captcha"))
            {
                modelState.AddModelError("File", " Имя не может содержать слово “captcha”");

                return false;
            }

            if (data.Name.Length > 8)
            {
                modelState.AddModelError("File", " Максимальная длина имени - 8 символов!");

                return false;
            }

            if (data.ContainsCyrillic == false && data.ContainsLatin == false && data.ContainsNumbers == false)
            {
                modelState.AddModelError("File", "Выберите как минимум один из параметров: “Содержит кириллицу”, “Содержит латиницу”,“Содержит цифры”");

                return false;
            }

            if (string.IsNullOrEmpty(data.Name) || postedFile == null || postedFile.Length == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(postedFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                modelState.AddModelError(data.Name, "Неправильный формат заргружаемых данных: " +
                    "данные должны подаваться в систему в виде архива");
                return false;

            }

            if (System.IO.File.Exists(Path.Combine(targetFilePath, trustedFileNameForFileStorage)))
            {
                modelState.AddModelError(data.Name, "Файл с таким именем уже существует.");
                return false;
            }

            return true;

        }
    }
}
