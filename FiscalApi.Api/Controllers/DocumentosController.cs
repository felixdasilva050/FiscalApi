using FiscalApi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System;
using System.Threading.Tasks;
using FiscalApi.Application.DTOs;

namespace FiscalApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DocumentosController : ControllerBase
{
    private readonly IFiscalDocumentService _service;

    public DocumentosController(IFiscalDocumentService service)
    {
        _service = service;
    }

    // POST api/v1/documentos
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile arquivo) 
    {
        try
        {
            if (arquivo == null || arquivo.Length == 0) 
                return BadRequest("Nenhum arquivo enviado.");
            
            string xmlContent;
            using (var reader = new StreamReader(arquivo.OpenReadStream(), Encoding.UTF8))
            {
                xmlContent = await reader.ReadToEndAsync();
            }
            
            await _service.ProcessarXmlAsync(xmlContent);
            
            return Ok(new { Message = "Documento recebido e processado." });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Erro no processamento", Detail = ex.Message });
        }
    }

    // GET api/v1/documentos?pagina=1&cnpjEmitente=...
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1, 
        [FromQuery] int tamanho = 10,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] string? cnpjEmitente = null)
    {
        var resultado = await _service.ListarAsync(pagina, tamanho, dataInicio, dataFim, cnpjEmitente);
        return Ok(resultado);
    }
    
    // GET api/v1/documentos/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(string id)
    {
        var doc = await _service.ObterPorIdAsync(id);
        if (doc == null) return NotFound(new { Message = "Documento não encontrado." });
    
        return Ok(doc);
    }
    
    // PUT api/v1/documentos/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(string id, IFormFile arquivo)
    {
        try
        {
            if (arquivo == null || arquivo.Length == 0) return BadRequest("Arquivo obrigatório.");

            string xmlContent;
            using (var reader = new StreamReader(arquivo.OpenReadStream(), Encoding.UTF8))
            {
                xmlContent = await reader.ReadToEndAsync();
            }

            await _service.AtualizarAsync(id, xmlContent);
            return Ok(new { Message = "Documento atualizado com sucesso." });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { Error = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { Error = ex.Message }); }
    }
    
    // DELETE api/v1/documentos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(string id)
    {
        try
        {
            await _service.ExcluirAsync(id);
            return NoContent(); // 204 No Content
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}