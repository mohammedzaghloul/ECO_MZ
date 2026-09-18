using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace ECO.BLL.Services.Upload
{
    public class ImageManagementService: IImageManagementService
    {
        private readonly IFileProvider fileProvider;

        public ImageManagementService(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider;
        }
        public async Task<List<string>> AddImageAsync(IFormFileCollection files, string src)
        {
            var images = new List<string>();
            var ImageDictionaries = Path.Combine("wwwroot", "Images", src);
            if (!Directory.Exists(ImageDictionaries))
            {
                Directory.CreateDirectory(ImageDictionaries);
            }
            foreach (var image in files)
            {
                if (image.Length > 0)
                {
                    {
                        var filePath = Path.Combine(ImageDictionaries, image.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }
                        images.Add(Path.Combine("Images", src, image.FileName));

                    }
                }
            }
                return images;
        }

        public Task DeleteImageAsync(string src)
        {
            var info= fileProvider.GetFileInfo(src);
            string? filePath = info.PhysicalPath;

            if (filePath != null && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }

       
    }
}
