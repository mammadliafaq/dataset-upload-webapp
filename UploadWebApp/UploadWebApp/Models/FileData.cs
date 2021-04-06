using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace UploadWebApp.Models
{
    public class FileData
    {

        public int Id { get; set; }

        [Display(Name = "Имя")]
        [Required(ErrorMessage = "Необходимо дать имя загружаемому датасету")]
        //[StringLength(8, MinimumLength = 4, ErrorMessage = "Максимальная длина имени - от 4 до 8 символов")]
        //[MaxLength(8, ErrorMessage = "Максимальная длина имени - 8 символов!")]
        [MinLength(4, ErrorMessage = "Минимальная длина имени - от 4 символов!")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Имя должно содержать только латинские буквы")]
        public string Name { get; set; }

        [Display(Name = "Дата создания")]
        [DataType(DataType.Date)]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Содержит кириллицу")]
        public Boolean ContainsCyrillic { get; set; }

        [Display(Name = "Содержит латиницу")]
        public Boolean ContainsLatin { get; set; }

        [Display(Name = "Содержит цифры")]
        public Boolean ContainsNumbers { get; set; }

        [Display(Name = "Содержит специальные символы")]
        public Boolean ContainsSpChar { get; set; }

        [Display(Name = "Чувствительность к регистру")]
        public Boolean HasRegistrSensitivity { get; set; }


        public int SelectedAnswerId { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> AnswerList { get; set; }

        [NotMapped]
        public IFormFile postedFile { get; set; }

    }
}
