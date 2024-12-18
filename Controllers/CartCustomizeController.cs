

using InquiryManagementApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace PetalOrSomething
{
    public class CartCustomize : Controller
    {
        public readonly FileUploadService _fileUploadService;
        public CartCustomize(FileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        public async Task<string> UploadFile(IFormFile file)
        {
            var fileUrl = await _fileUploadService.UploadFileToCloudinaryAsync(file);
            return fileUrl;
        }

    }
}