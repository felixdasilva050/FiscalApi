using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text;
using Xunit;
using FiscalApi.Application.DTOs;
using System.Globalization;

namespace FiscalApi.Tests.Integration;

[Collection("Sequencial")] 
public class DocumentosIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly Random _random = new Random(); 

    public DocumentosIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_XmlRecife_DeveRetornarSucesso()
    {
        var response = await RealizarUploadFake(GerarNumeroUnico(), GerarValorAleatorio());
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Upload_XmlInvalido_DeveRetornarErro()
    {
        var xmlLixo = "<lixo>Isso nao é uma nota fiscal</lixo>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlLixo));
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "arquivo", "teste_invalido.xml");
        
        var response = await _client.PostAsync("/api/v1/documentos", content);
        
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Listar_Deve_Retornar_Documentos_Paginados()
    {
        await RealizarUploadFake(GerarNumeroUnico(), GerarValorAleatorio());

        var response = await _client.GetAsync("/api/v1/documentos?pagina=1&tamanho=10");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var resultado = await response.Content.ReadFromJsonAsync<PagedResult<FiscalDocumentResumoDTO>>();
        
        resultado.Should().NotBeNull();
        resultado.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObterPorId_Deve_Retornar_Detalhes_Do_Documento()
    {
        string numero = GerarNumeroUnico();
        decimal valorUnico = GerarValorDecimalAleatorio(); 
        string valorString = valorUnico.ToString("F2", CultureInfo.InvariantCulture);

        await RealizarUploadFake(numero, valorString);
        
        var id = await ObterIdPorValor(valorUnico);
        
        var response = await _client.GetAsync($"/api/v1/documentos/{id}");
        
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<FiscalDocumentDetalheDTO>();
        dto.Id.Should().Be(id);
        dto.ValorTotal.Should().Be(valorUnico);
    }

    [Fact]
    public async Task Atualizar_Deve_Modificar_Valor_Do_Documento()
    {
        string numero = GerarNumeroUnico();
        decimal valorOriginal = GerarValorDecimalAleatorio();
        string valorOriginalStr = valorOriginal.ToString("F2", CultureInfo.InvariantCulture);
        
        await RealizarUploadFake(numero, valorOriginalStr);
        var id = await ObterIdPorValor(valorOriginal);

        decimal valorNovo = valorOriginal + 10.50m;
        string valorNovoStr = valorNovo.ToString("F2", CultureInfo.InvariantCulture);

        var xmlAtualizado = GerarXmlRecife(numero, valorNovoStr);
        
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlAtualizado));
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "arquivo", "nota_atualizada.xml");
        
        var response = await _client.PutAsync($"/api/v1/documentos/{id}", content);
        
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var responseGet = await _client.GetAsync($"/api/v1/documentos/{id}");
        var dto = await responseGet.Content.ReadFromJsonAsync<FiscalDocumentDetalheDTO>();
        dto.ValorTotal.Should().Be(valorNovo);
    }

    [Fact]
    public async Task Excluir_Deve_Remover_Documento()
    {
        string numero = GerarNumeroUnico();
        decimal valorUnico = GerarValorDecimalAleatorio(); 
        string valorUnicoStr = valorUnico.ToString("F2", CultureInfo.InvariantCulture);
        
        await RealizarUploadFake(numero, valorUnicoStr);
        var id = await ObterIdPorValor(valorUnico);
        
        var responseDelete = await _client.DeleteAsync($"/api/v1/documentos/{id}");
        
        responseDelete.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        
        var responseGet = await _client.GetAsync($"/api/v1/documentos/{id}");
        responseGet.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
    
    private string GerarNumeroUnico()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 9);
    }

    private string GerarValorAleatorio()
    {
        return _random.Next(10, 9000).ToString() + ".00";
    }

    private decimal GerarValorDecimalAleatorio()
    {
        return Math.Round((decimal)_random.NextDouble() * 1000m + 10m, 2);
    }

    private string GerarXmlRecife(string numero, string valor)
    {
        return $@"<Nfse xmlns=""http://nfse.recife.pe.gov.br"">
                    <Numero>{numero}</Numero>
                    <Cnpj>12345678000199</Cnpj>
                    <ValorServico>{valor}</ValorServico>
                    <DataEmissao>2023-10-25</DataEmissao>
                  </Nfse>";
    }

    private async Task<HttpResponseMessage> RealizarUploadFake(string numeroNota, string valor)
    {
        var xml = GerarXmlRecife(numeroNota, valor);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "arquivo", "nota_teste.xml");
        
        return await _client.PostAsync("/api/v1/documentos", content);
    }

    private async Task<string> ObterIdPorValor(decimal valorAlvo)
    {
        var response = await _client.GetFromJsonAsync<PagedResult<FiscalDocumentResumoDTO>>("/api/v1/documentos?tamanho=100");
        var doc = response.Items.FirstOrDefault(x => x.ValorTotal == valorAlvo);
        
        if (doc == null) 
            throw new Exception($"TESTE FALHOU: Documento com valor {valorAlvo} não encontrado.");

        return doc.Id;
    }
}