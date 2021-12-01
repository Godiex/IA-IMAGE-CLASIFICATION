using Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers 
{

    [ApiController]
    [Route("api/[controller]")]
    public class ImageSorterController
    {

        readonly IMediator _mediator;

        public ImageSorterController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<ModelCreateDto> PostAsync(ModelCreateCommand request) => await _mediator.Send(request);

    }
}

