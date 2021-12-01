using Domain.Entities;
using Domain.Ports;
using Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{

    public class SorterImageHandler : IRequestHandler<SorterImageCommand, SorterImageDto>
    {

        private readonly ImageSorterService _ImageSorterService;
        private readonly IImageSorterRepository _imageSorterRepository;

        public SorterImageHandler(ImageSorterService imageSorterService, IImageSorterRepository imageSorterRepository)
        {
            _ImageSorterService = imageSorterService ?? throw new ArgumentNullException(nameof(imageSorterService));
            _imageSorterRepository = imageSorterRepository ?? throw new ArgumentNullException(nameof(imageSorterRepository));
        }


        async Task<SorterImageDto> IRequestHandler<SorterImageCommand, SorterImageDto>.Handle(SorterImageCommand request, CancellationToken cancellationToken)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request), "request object needed to handle this task");

            string imagePath = await SaveFileToClasify(_imageSorterRepository.GetImageClasificationFolder(), request.Image);

            var prediction = _ImageSorterService.Predict(new ImageData { ImagePath = imagePath });

            return new SorterImageDto { PredictedLabelValue = prediction.PredictedLabelValue, Score = prediction.Score };
        }

        public async Task<string> SaveFileToClasify(string basePath, IFormFile file)
        {
            string pathToCombine = GetPathCombine(file);
            var filePath = Path.Combine(basePath, pathToCombine);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return pathToCombine;
        }

        public string GetPathCombine(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            return $"{Guid.NewGuid()}{extension}";
        }

    }
}
