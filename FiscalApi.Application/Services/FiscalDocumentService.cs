using FiscalApi.Domain.Entities;
using FiscalApi.Domain.Interfaces;
using System.Text;
using FiscalApi.Application.DTOs;

namespace FiscalApi.Application.Services;

public interface IFiscalDocumentService
{
    Task ProcessarXmlAsync(string xmlContent);
    Task<PagedResult<FiscalDocumentResumoDTO>> ListarAsync(int page, int size, DateTime? inicio, DateTime? fim, string? cnpj);
    Task<FiscalDocumentDetalheDTO?> ObterPorIdAsync(string id);
    Task AtualizarAsync(string id, string novoXml);
    Task ExcluirAsync(string id);
}

public class FiscalDocumentService : IFiscalDocumentService
{
    private readonly IFiscalDocumentRepository _repository;
    private readonly IParserFactory _parserFactory;

    public FiscalDocumentService(IFiscalDocumentRepository repository, IParserFactory parserFactory)
    {
        _repository = repository;
        _parserFactory = parserFactory;
    }

    public async Task ProcessarXmlAsync(string xmlContent)
    {
        // 1. Escolhe estratégia (OCP)
        var strategy = _parserFactory.GetStrategy(xmlContent);
            
        // 2. Faz o parse
        var dados = strategy.Parse(xmlContent);

        // 3. Encode de segurança (Base64 como solicitado)
        var emitenteEnc = Convert.ToBase64String(Encoding.UTF8.GetBytes(dados.EmitenteRaw ?? ""));
        var destEnc = Convert.ToBase64String(Encoding.UTF8.GetBytes(dados.DestinatarioRaw ?? ""));

        // 4. Cria Entidade
        var doc = new FiscalDocument(
            dados.UniqueId, 
            xmlContent, 
            dados.TipoDocumento, 
            dados.DataEmissao, 
            emitenteEnc, 
            destEnc, 
            dados.ValorTotal
        );

        // 5. Salva (Idempotente)
        await _repository.UpsertAsync(doc);
    }

    public async Task<PagedResult<FiscalDocumentResumoDTO>> ListarAsync(int page, int size, DateTime? inicio, DateTime? fim, string? cnpj)
    {
        var (data, total) = await _repository.GetAllAsync(page, size, inicio, fim, cnpj);

        var dtos = data.Select(d => new FiscalDocumentResumoDTO()
        {
            Id = d.Id,
            TipoDocumento = d.TipoDocumento,
            DataEmissao = d.DataEmissao,
            ValorTotal = d.ValorTotal,
            Emitente = DecodeBase64(d.EmitenteDocEncodado)
        });

        return new PagedResult<FiscalDocumentResumoDTO>
        {
            Items = dtos,
            TotalCount = total,
            Page = page,
            PageSize = size
        };
    }
    
    public async Task<FiscalDocumentDetalheDTO?> ObterPorIdAsync(string id)
    {
        var doc = await _repository.GetByIdAsync(id);
        if (doc == null) return null;

        return new FiscalDocumentDetalheDTO()
        {
            Id = doc.Id,
            TipoDocumento = doc.TipoDocumento,
            DataEmissao = doc.DataEmissao,
            ValorTotal = doc.ValorTotal,
            XmlOriginal = doc.XmlOriginal,
            Emitente = DecodeBase64(doc.EmitenteDocEncodado),
            Destinatario = DecodeBase64(doc.DestinatarioDocEncodado)
        };
    }
    
    public async Task AtualizarAsync(string id, string novoXml)
    {
        var docExistente = await _repository.GetByIdAsync(id);
        if (docExistente == null) 
            throw new KeyNotFoundException("Documento não encontrado.");

        var strategy = _parserFactory.GetStrategy(novoXml);
        var dadosNovos = strategy.Parse(novoXml);
        
        if (dadosNovos.UniqueId != id)
            throw new InvalidOperationException("O XML fornecido pertence a outro documento (ID diferente).");

        var emitenteEnc = Convert.ToBase64String(Encoding.UTF8.GetBytes(dadosNovos.EmitenteRaw));
        var destEnc = Convert.ToBase64String(Encoding.UTF8.GetBytes(dadosNovos.DestinatarioRaw));
        
        docExistente.AtualizarDados(novoXml, dadosNovos.DataEmissao, dadosNovos.ValorTotal, emitenteEnc, destEnc);
        
        await _repository.UpdateAsync(docExistente);
    }
    
    public async Task ExcluirAsync(string id)
    {
        var exists = await _repository.GetByIdAsync(id);
        if (exists == null) throw new KeyNotFoundException("Documento não encontrado.");
    
        await _repository.DeleteAsync(id);
    }
    private string DecodeBase64(string encoded)
    {
        if (string.IsNullOrEmpty(encoded)) return "";
        try { return Encoding.UTF8.GetString(Convert.FromBase64String(encoded)); }
        catch { return encoded; }
    }
}