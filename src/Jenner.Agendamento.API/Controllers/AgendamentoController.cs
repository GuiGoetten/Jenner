using AutoMapper;
using Jenner.Agendamento.API.Services;
using Jenner.Agendamento.API.ViewModels;
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
        private readonly IMapper _mapper;

        private CancellationToken Token => HttpContext?.RequestAborted ?? default;

        public AgendamentoController(ISender sender, IMapper mapper)
        {
            this.sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
            _mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] AgendamentoCreate novoAgendamento)
        {

            //var criarAplicacao = _mapper.Map<Aplicacao>(novoAgendamento);

            //Aplicacao result = await sender.Send(new AgendamentoCreate(criarAplicacao));
            Aplicacao result = await sender.Send(novoAgendamento);

            return Ok(result);
            
            //TODO: Arrumar o retorno, para retornar um 201 quando criado com sucesso + o objeto criado com o ID correto
            //return CreatedAtAction(nameof(Aplicacao), result);
        }
    }
}
