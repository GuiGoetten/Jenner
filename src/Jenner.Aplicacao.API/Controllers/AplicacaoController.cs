using Jenner.Aplicacao.API.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Aplicacao.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class AplicacaoController : ControllerBase
    {
        private readonly ISender sender;
        private readonly ILogger<AplicacaoController> logger;

        private CancellationToken Token => HttpContext?.RequestAborted ?? default;

        public AplicacaoController(ISender sender, ILogger<AplicacaoController> logger)
        {
            this.sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync([FromBody] AplicacaoCreate novaAplicacao, CancellationToken cancellationToken)
        {
            try
            {
                var aplicacao = await sender.Send(novaAplicacao, cancellationToken);
                return Ok();
            }
            catch (System.Exception e)
            {
                logger.LogError(e, "Um erro aconteceu");
                return BadRequest(e);
            }
        }
    }
}
