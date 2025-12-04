using System.Linq;
using System.Xml.Linq;
namespace FiscalApi.Infrastructure.Helpers;

public static class XmlHelper
{
    public static string GetValue(this XContainer doc, string tagName)
    {
        var element = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == tagName);
        return element?.Value ?? string.Empty;
    }
}