using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Application.Person.Commands
{
    public record ModelCreateCommand(
        [Required] string Plaque,
        [Required] int Cylinders,
        [Required] [Range(1, 2)] int Type
    ) : IRequest<ModelCreateDto>;

} 
