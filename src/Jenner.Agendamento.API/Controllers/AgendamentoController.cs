using Jenner.Agendamento.API.Services;
using Jenner.Comum.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Agendamento.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class AgendamentoController : ControllerBase
    {
        private readonly ISender sender;

        private CancellationToken Token => HttpContext?.RequestAborted ?? default;

        public AgendamentoController(ISender sender)
        {
            this.sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] AgendamentoCreate novoAgendamento)
        {
            Aplicacao result;
            try
            {
                result = await sender.Send(novoAgendamento);
            }
            catch (System.Exception e)
            {
                return BadRequest(e.Message);
            }
            
            return Ok(result);

        }
    }
}
