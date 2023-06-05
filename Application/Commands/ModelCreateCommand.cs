using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public record ModelCreateCommand(
        [Required] string Category,
        [Required] List<IFormFile> Images
    ) : IRequest<ModelCreateDto>;

}
