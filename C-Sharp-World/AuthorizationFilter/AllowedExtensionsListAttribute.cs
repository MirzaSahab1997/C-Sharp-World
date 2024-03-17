using System.ComponentModel.DataAnnotations;

namespace UserManagement.Application.Filters
{
    public class AllowedExtensionsListAttribute : ValidationAttribute
    {

        private readonly string[] _extensions;
        public AllowedExtensionsListAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
        {
            if (value is List<IFormFile> file)
            {
                foreach (var fileItem in file.Select(x => x.FileName))
                {
                    var extension = Path.GetExtension(fileItem);
                    if (!_extensions.Contains(extension.ToLower()))
                    {
                        return new ValidationResult(GetErrorMessage(fileItem));
                    }
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage(string FileName)
        {
            return $"You can't attach executable files!-" + FileName;
        }

    }
}
