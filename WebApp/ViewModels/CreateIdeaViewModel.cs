using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Data;

namespace WebApp.ViewModels
{
    public class CreateIdeaViewModel
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; } = null!;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ValidateNever]
        [NotMapped]
        public List<SelectListItem>? Categories { get; set; }

        [BooleanMustBeTrue(ErrorMessage = "You must Agree for Term and Conditions")]
        public bool IsCheckTerm { get; set; }

        [Display(Name = "Make my idea incognito")]
        [Required]
        public bool IsIncognito { get; set; }

        [MaxFileSize]
        [AllowedDocument]
        public List<IFormFile>? Documents { get; set; }
    }

    public class BooleanMustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is bool boolean && boolean;
        }
    }

    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private string? errorMessage;

        public int MaxFileSize { get; set; } = UploadFileHelper.MaxFileSize;

        public new string ErrorMessage
        {
            get
            {
                return errorMessage ?? $"Maximum allowed file size is {MaxFileSize / 1024.0 / 1024} MB.";
            }
            set => errorMessage = value;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is List<IFormFile> files)
            {
                foreach (var file in files)
                    if (file != null)
                    {
                        if (!IsFileSizeValid(file))
                            return new ValidationResult(ErrorMessage);

                    }
                return ValidationResult.Success;
            }
            else if (value is IFormFile file)
            {
                if (IsFileSizeValid(file))
                    return ValidationResult.Success;
                else
                    return new ValidationResult(ErrorMessage);
            }
            else
                throw new ArgumentException("Invalid type");
        }

        private bool IsFileSizeValid(IFormFile file)
        {
            return file.Length < MaxFileSize;
        }
    }

    public class AllowedDocumentAttribute : ValidationAttribute
    {
        public List<string> DocumentExtensions { get; } = UploadFileHelper.DocumentExtensions;

        public new string ErrorMessage
        {
            get => $"Only document is allowed ({string.Join(", ", DocumentExtensions)})";
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is ICollection<IFormFile> files)
            {
                foreach (var file in files)
                    if (file != null)
                    {
                        if (!IsFileExtensionValid(file))
                            return new ValidationResult(ErrorMessage);
                    }
                return ValidationResult.Success;
            }
            else if (value is IFormFile file)
            {
                if (IsFileExtensionValid(file))
                    return ValidationResult.Success;
                else
                    return new ValidationResult(ErrorMessage);
            }
            else
                throw new ArgumentException("Invalid value type");
        }

        private bool IsFileExtensionValid(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            return DocumentExtensions.Any(ext => ext == extension);
        }
    }
}
