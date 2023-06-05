using Domain.Ports;
using Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{

    public class ModelCreateHandler : IRequestHandler<ModelCreateCommand, ModelCreateDto>
    {

        private readonly ImageSorterService _imageSorterService;
        private readonly IImageSorterRepository _imageSorterRepository;

        public ModelCreateHandler(ImageSorterService imageSorterService, IImageSorterRepository imageSorterRepository)
        {
            _imageSorterService = imageSorterService ?? throw new ArgumentNullException(nameof(imageSorterService));
            _imageSorterRepository = imageSorterRepository ?? throw new ArgumentNullException(nameof(imageSorterRepository));
        }


        async Task<ModelCreateDto> IRequestHandler<ModelCreateCommand, ModelCreateDto>.Handle(ModelCreateCommand request, CancellationToken cancellationToken)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request), "request object needed to handle this task");

            foreach (var image in request.Images)
            {
                var nameOfImage = CreateNameForImage(image);
                await SaveFileToTrainModel(image, nameOfImage);
                await using var writer = new StreamWriter(_imageSorterRepository.GetDataTrainPath(), true);
                await writer.WriteLineAsync($"{nameOfImage}\t{request.Category}");
            }
            
            _imageSorterService.TrainModel();

            return new ModelCreateDto("Modelo entrenado con exito!!!");
        }

        private async Task SaveFileToTrainModel(IFormFile file, string fileName)
        {
            var filePath = Path.Combine(_imageSorterRepository.GetImageClasificationFolder(), fileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        private static string CreateNameForImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            return $"{Guid.NewGuid()}{extension}";
        }

    }
}
