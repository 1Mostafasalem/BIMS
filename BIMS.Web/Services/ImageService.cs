using BIMS.Web.Core.Models;

namespace BIMS.Web.Services
{
	public class ImageService : IImageService
	{
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ImageService(IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
		}

		private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
		private int _maxAllowedSize = 2097152;
		public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail)
		{
			var extension = Path.GetExtension(image.FileName);

			if (!_allowedExtensions.Contains(extension))
				return (isUploaded: false, errorMessage: Errors.NotAllowedExtensions);


			if (image.Length > _maxAllowedSize)
				return (isUploaded: false, errorMessage: Errors.MaxSize);

			var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);
			var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}/thumb", imageName);

			var stream = File.Create(path);
			await image.CopyToAsync(stream);
			stream.Dispose();

			if (hasThumbnail)
			{
				var loadedImage = Image.Load(image.OpenReadStream());
				var ratio = (float)loadedImage.Width / 200;
				var height = loadedImage.Height / ratio;
				loadedImage.Mutate(i => i.Resize(width: 200, height: (int)height));
				loadedImage.Save(thumbPath);
			}

			return (isUploaded: true, errorMessage: null);
		}

		public void Delete(string imagePath, string ThumbnailPath = null)
		{
			var oldImagePath = $"{_webHostEnvironment.WebRootPath}{imagePath}";
			var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{ThumbnailPath}";

			if (File.Exists(oldImagePath))
				File.Delete(oldImagePath);

			if (!string.IsNullOrEmpty(ThumbnailPath))
			{
				if (File.Exists(oldThumbPath))
					File.Delete(oldThumbPath);
			}
		}
	}
}
