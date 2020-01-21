using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProAgil.Domain;
using ProAgil.Repository;
using ProAgil.WebAPI.Dtos;

namespace ProAgil.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventoController : ControllerBase
    {
        //Aqui estou injetando por meio de uma interface o meu repositório
        public readonly IProAgilRepository _repo;

        private readonly IMapper _mapper;

        public EventoController(IProAgilRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
            
        }


        // GET all
        [HttpGet]
        public async  Task<IActionResult> Get()
        {

            try
            {
                //lista os dados do banco
                var eventos = await _repo.GetAllEventoAsync(true);
                // Adicionando o mapper no meu retorno
                var results = _mapper.Map<EventoDto[]>(eventos);
                
                return Ok(results);
            }
            catch (System.Exception ex)
            {
                
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Banco de dados Falhou {ex.Message}");
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {

            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources","Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if(file.Length > 0){
                    var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName;
                    var fullPath = Path.Combine(pathToSave, filename.Replace("\"", " ").Trim());

                    using(var stream = new FileStream(fullPath, FileMode.Create)){
                        file.CopyTo(stream);
                    }
                }

                return Ok();
            }
            catch (System.Exception ex)
            {
                
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Banco de dados Falhou {ex.Message}");
            }

            return BadRequest("Erro ao tentar realizar upload");
        }

        // GET por Id
        [HttpGet("{EventoId}")]
        public async  Task<IActionResult> Get(int EventoId)
        {

            try
            {
                //lista os dados do banco
                var evento = await _repo.GetEventoAsyncById(EventoId, true);

                var results = _mapper.Map<EventoDto>(evento);
                
                return Ok(results);
            }
            catch (System.Exception)
            {
                
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Banco de dados Falhou");
            }
        }

        // GET por Tema
        [HttpGet("GetByTema/{tema}")]
        public async  Task<IActionResult> Get(string tema)
        {

            try
            {
                //lista os dados do banco
                var eventos = await _repo.GetAllEventoAsyncByTema(tema, true);

                var results = _mapper.Map<EventoDto[]>(eventos);
                
                return Ok(results);
            }
            catch (System.Exception)
            {
                
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Banco de dados Falhou");
            }
        }


        // POST
        [HttpPost]
        public async  Task<IActionResult> Post(EventoDto model)
        {

            try
            {
                var evento = _mapper.Map<Evento>(model);


                //lista os dados do banco
                _repo.Add(evento);

                if(await _repo.SaveChangesAsync()){
                    //aqui eeu chamo a roda de evento id criada lá em cima
                    return Created($"/api/evento/{model.Id}", _mapper.Map<EventoDto>(evento));
                }

            }
            catch (System.Exception ex)
            {
                
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Banco de dados Falhou {ex.Message}");
            }

            return BadRequest();
        }

        //PUT
        [HttpPut("{EventoId}")]
        public async  Task<IActionResult> Put(int EventoId, EventoDto model)
        {

            try
            {   
                //Aqui eu localizo o registro
                var evento = await _repo.GetEventoAsyncById(EventoId, false);
                //aqui verifico se o registro possui algo de fato no banco, se não achar retorno que não existe
                if(evento == null) return NotFound();

                /* traduzindo a linha debaixo. A var _mapper está mapeando através do Map o evento (que já está na base de dados)
                e substituindo pelo que foi alterado no model (parâmetro que recebi) Model- dados novos, evento- dados antigos */
                _mapper.Map(model, evento);

                //atualiza os dados do banco
                _repo.Update(evento);

                if(await _repo.SaveChangesAsync()){
                    //aqui eeu chamo a roda de evento id criada lá em cima
                    return Created($"/api/evento/{model.Id}", _mapper.Map<EventoDto>(evento));
                }

            }
            catch (System.Exception)
            {
                
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Banco de dados Falhou");
            }

            return BadRequest();
        }


        //DELETE
        [HttpDelete("{EventoId}")]
        public async  Task<IActionResult> Delete(int EventoId)
        {

            try
            {
                //Aqui eu localizo o registro
                var evento = await _repo.GetEventoAsyncById(EventoId, false);
                //aqui verifico se o registro possui algo de fato no banco,se não achar retorno que não existe
                if(evento == null) return NotFound();

                //atualiza os dados do banco
                _repo.Delete(evento);

                if(await _repo.SaveChangesAsync()){
                    //aqui eeu chamo a roda de evento id criada lá em cima
                    return Ok();
                }

            }
            catch (System.Exception)
            {
                
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Banco de dados Falhou");
            }

            return BadRequest();
        }




    }
}