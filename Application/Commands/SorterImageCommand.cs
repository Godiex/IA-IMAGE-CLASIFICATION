using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public record SorterImageCommand(
        [Required] IFormFile Image
    ) : IRequest<SorterImageDto>;

}
