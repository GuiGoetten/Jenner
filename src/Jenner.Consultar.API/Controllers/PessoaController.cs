using Jenner.Consultar.API.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Consultar.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class PessoaController : ControllerBase
    {
        private readonly ISender sender;

        private CancellationToken Token => HttpContext?.RequestAborted ?? default;

        public PessoaController(ISender sender)
        {
            this.sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
        }

        // POST: PessoaController/Create
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] PessoaCreate pessoa)
        {
            string result = await sender.Send(pessoa);
            return Ok(result);
        }
    }
}
