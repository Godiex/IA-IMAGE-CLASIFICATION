using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Application.Commands
{
    public record ModelCreateCommand(
    ) : IRequest<ModelCreateDto>;

}
