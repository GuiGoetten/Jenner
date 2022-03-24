using Jenner.Carteira.API.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jenner.Carteira.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class CarteiraController : ControllerBase
    {
        private readonly ISender sender;

        private CancellationToken Token => HttpContext?.RequestAborted ?? default;

        public CarteiraController(ISender sender)
        {
            this.sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CarteiraCreate novaCarteira)
        {
            Comum.Models.Carteira result;
            try
            {
                result = await sender.Send(novaCarteira);
            }
            catch (System.Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(result);


            //TODO: Arrumar o retorno, para retornar um 201 quando criado com sucesso + o objeto criado com o ID correto
            //return CreatedAtAction(nameof(Aplicacao), result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Comum.Models.Carteira>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAll()
        {
            IEnumerable<Comum.Models.Carteira> result = await sender.Send(new CarteiraList(), Token);
            return Ok(result);
        }

        [HttpGet("cpf/{cpf}/nome/{nome}", Name = "Lista Carteira Unica")]
        [ProducesResponseType(typeof(IEnumerable<Comum.Models.Carteira>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListOne(string cpf, string nome)
        {
            IEnumerable<Comum.Models.Carteira> result = await sender.Send(new CarteiraSingle{Cpf = cpf, NomePessoa = nome }, Token);
            return Ok(result);
        }


    }
}
