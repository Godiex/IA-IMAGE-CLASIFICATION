using Domain.Services;
using MediatR;

namespace Application.Person.Commands {

    public class ModelCreateHandler : IRequestHandler<ModelCreateCommand, ModelCreateDto>
    {

        private readonly VehicleEntryRegistrationService _VehicleEntryRegistrationService;

        public ModelCreateHandler(VehicleEntryRegistrationService vehicleEntryRegistrationService)
        {
            _VehicleEntryRegistrationService = vehicleEntryRegistrationService ?? throw new ArgumentNullException(nameof(vehicleEntryRegistrationService));
        }


        async Task<ModelCreateDto> IRequestHandler<ModelCreateCommand, ModelCreateDto>.Handle(ModelCreateCommand request, CancellationToken cancellationToken)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request), "request object needed to handle this task");

            var cellVehicle = await _VehicleEntryRegistrationService.RegisterEntranceToParkingAsync(
                 new Domain.Entities.Vehicle(request.Type, request.Plaque, request.Cylinders)
             );

            return new ModelCreateDto {CellId = cellVehicle.CellId };
        }
    }
}
