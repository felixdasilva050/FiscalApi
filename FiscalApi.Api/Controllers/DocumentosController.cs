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

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] string? cnpj = null)
    {
        var docs = await _service.ListarAsync(pagina, 10, null, cnpj);
        return Ok(docs);
    }
}