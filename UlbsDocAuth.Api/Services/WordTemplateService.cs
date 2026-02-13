using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UlbsDocAuth.Api.DTOs; // <--- Importăm DTO-ul tău

namespace UlbsDocAuth.Api.Services;

public interface IWordTemplateService
{
    // Primim datele exact sub forma DTO-ului tău
    string GenerateDocx(CertificateResponse studentData, string reason);
}

public class WordTemplateService : IWordTemplateService
{
    private readonly string _templatePath;

    public WordTemplateService(IWebHostEnvironment env)
    {
        // Calea către template: folderul "Templates" din rădăcina proiectului
        _templatePath = Path.Combine(env.ContentRootPath, "Templates", "Adeverinta_2_Completat.docx"); 
        // ATENȚIE: Verifică extensia! Dacă e .doc vechi, OpenXml nu merge. Trebuie .docx
        // Recomand să salvezi template-ul ca .docx în Word.
    }

    public string GenerateDocx(CertificateResponse studentData, string reason)
    {
        // Verificăm dacă fișierul template există
        // Notă: Dacă ai extensia .doc, schimbă mai jos în .docx după ce convertești fișierul
        var templateExtension = Path.GetExtension(_templatePath);
        

        if (!File.Exists(_templatePath))
        {
            throw new FileNotFoundException($"Template-ul nu a fost găsit la: {_templatePath}");
        }

        // Creăm un fișier temporar unic
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"Adeverinta_{Guid.NewGuid()}.docx");
        File.Copy(_templatePath, tempFilePath, true);

        // Mapăm datele din CertificateResponse către textul din Word
        var replacements = new Dictionary<string, string>
        {
            { "{{NumeComplet}}", studentData.FullName ?? "" },
            { "{{AnStudiu}}",    studentData.StudyYear.ToString() },
            { "{{Program}}",     studentData.Program ?? "" },
            { "{{Motiv}}",       reason ?? "" },
            { "{{Facultate}}",   studentData.Faculty ?? "" },
            { "{{Grupa}}",       studentData.Group ?? "" }
        };

        // Deschidem și modificăm
        using (var wordDoc = WordprocessingDocument.Open(tempFilePath, true))
        {
            var body = wordDoc.MainDocumentPart.Document.Body;

            foreach (var text in body.Descendants<Text>())
            {
                foreach (var item in replacements)
                {
                    if (text.Text.Contains(item.Key))
                    {
                        text.Text = text.Text.Replace(item.Key, item.Value);
                    }
                }
            }
            wordDoc.MainDocumentPart.Document.Save();
        }

        return tempFilePath;
    }
}