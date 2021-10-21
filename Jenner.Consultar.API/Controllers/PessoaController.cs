using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jenner.Consultar.API.Controllers
{
    public class PessoaController : ControllerBase
    {
        private readonly ISender sender;

        public PessoaController(ISender sender)
        {
            this.sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
        }

        // POST: PessoaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PessoaCreate command)
        {
            string result = await sender.Send(command);
            return Ok(result);
        }
    }
}
