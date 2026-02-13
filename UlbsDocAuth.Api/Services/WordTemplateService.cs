using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UlbsDocAuth.Api.DTOs; 

namespace UlbsDocAuth.Api.Services;

public interface IWordTemplateService
{
    
    string GenerateDocx(CertificateResponse studentData, string reason);
}

public class WordTemplateService : IWordTemplateService
{
    private readonly string _templatePath;

    public WordTemplateService(IWebHostEnvironment env)
    {
       
        _templatePath = Path.Combine(env.ContentRootPath, "Templates", "Adeverinta_2_Completat.docx"); 
       
    }

    public string GenerateDocx(CertificateResponse studentData, string reason)
    {
        
        var templateExtension = Path.GetExtension(_templatePath);
        

        if (!File.Exists(_templatePath))
        {
            throw new FileNotFoundException($"Template-ul nu a fost găsit la: {_templatePath}");
        }

       
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"Adeverinta_{Guid.NewGuid()}.docx");
        File.Copy(_templatePath, tempFilePath, true);

       
        var replacements = new Dictionary<string, string>
        {
            { "{{NumeComplet}}", studentData.FullName ?? "" },
            { "{{AnStudiu}}",    studentData.StudyYear.ToString() },
            { "{{Program}}",     studentData.Program ?? "" },
            { "{{Motiv}}",       reason ?? "" },
            { "{{Facultate}}",   studentData.Faculty ?? "" },
            { "{{Grupa}}",       studentData.Group ?? "" }
        };

       
       using (var wordDoc = WordprocessingDocument.Open(tempFilePath, true))
    {
       
        var mainPart = wordDoc.MainDocumentPart;
        if (mainPart?.Document?.Body == null)
        {
            throw new InvalidOperationException("Documentul Word este invalid sau lipsește corpul (body).");
        }

        var body = mainPart.Document.Body;

        foreach (var text in body.Descendants<Text>())
        {
           
            if (string.IsNullOrEmpty(text.Text)) continue;

            foreach (var item in replacements)
            {
                if (text.Text.Contains(item.Key))
                {
                    text.Text = text.Text.Replace(item.Key, item.Value);
                }
            }
        }
        
        mainPart.Document.Save();
    }

    return tempFilePath;
    }
}