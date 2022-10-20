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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] AplicacaoCreate novaAplicacao)
        {
            Comum.Models.Aplicacao result;
            try
            {
               result = await sender.Send(novaAplicacao);
            }
            catch (System.Exception e)
            {
                logger.LogError(e, "Um erro aconteceu");
                return BadRequest();
            }

            return Ok(result);

            
            //TODO: Arrumar o retorno, para retornar um 201 quando criado com sucesso + o objeto criado com o ID correto
            //return CreatedAtAction(nameof(Aplicacao), result);
        }
    }
}
