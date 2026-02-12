using Microsoft.AspNetCore.Mvc;
using Spire.Doc;

namespace UlbsDocAuth.Api.Controllers;

[ApiController]
[Route("api/doc-docx")]
public class DocDocxConv : ControllerBase
{
    [HttpGet("convert")]
    public IActionResult Convert()
    {
        // calea catre fisierul template
        string templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "Templates",
            "Adeverinta.doc"
        );

        // incarcam documentul .doc
        Document document = new Document();
        document.LoadFromFile(templatePath);

        // il salvam in memorie ca .docx
        MemoryStream ms = new MemoryStream();
        document.SaveToStream(ms, FileFormat.Docx);

        // returnam fisierul rezultat
        return File(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "Adeverinta.docx"
        );
    }
}
