using Application.Commands;
using Domain.Entities;
using Domain.Services;
using MediatR;

namespace Application.Person.Commands
{

    public class ModelCreateHandler : IRequestHandler<ModelCreateCommand, ModelCreateDto>
    {

        private readonly ImageSorterService _ImageSorterService;

        public ModelCreateHandler(ImageSorterService imageSorterService)
        {
            _ImageSorterService = imageSorterService ?? throw new ArgumentNullException(nameof(imageSorterService));
        }


        async Task<ModelCreateDto> IRequestHandler<ModelCreateCommand, ModelCreateDto>.Handle(ModelCreateCommand request, CancellationToken cancellationToken)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request), "request object needed to handle this task");

            _ImageSorterService.TrainModel();

            return new ModelCreateDto();
        }
    }
}
