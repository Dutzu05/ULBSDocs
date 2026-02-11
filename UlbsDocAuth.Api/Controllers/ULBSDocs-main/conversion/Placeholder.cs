using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.IO;
using System.Linq; // Necesar pentru operatii pe liste

namespace UlbsDocAuth.Api
{
    public class Placeholder
    {
        public void ReplacePlaceholder()
        {
            // --- CONFIGURARE ---
            string folderPath = @"C:\Users\oboro\Desktop\ULBSDocs\UlbsDocAuth.Api\Controllers\ULBSDocs-main\conversion\Templates";
            string sourceFile = System.IO.Path.Combine(folderPath, "Adeverinta_2.docx");
            string destFile = System.IO.Path.Combine(folderPath, "Adeverinta_2_Completat.docx");

            // Verificare fișier
            if (!File.Exists(sourceFile))
            {
                Console.WriteLine($"[EROARE] Nu găsesc fișierul: {sourceFile}");
                return;
            }

            // Copiere
            File.Copy(sourceFile, destFile, true);

            // --- PROCESARE DOCUMENT ---
            using (var wordDoc = WordprocessingDocument.Open(destFile, true))
            {
                var mainPart = wordDoc.MainDocumentPart;
                if (mainPart == null || mainPart.Document.Body == null) return;

                var body = mainPart.Document.Body;

                // SCHIMBAREA MAJORA:
                // Nu mai căutăm în 'Text', ci în 'Paragraph' (paragrafe întregi).
                // Asta rezolvă problema textului spart de Word.
                foreach (var paragraph in body.Descendants<Paragraph>())
                {
                    // Luăm tot textul din paragraf legat (ex: "Subsemnatul {{NUME}}...")
                    string textParagraf = paragraph.InnerText;

                    // Verificăm dacă există placeholder-ul în textul brut
                    if (textParagraf.Contains("{{NUME}}") || textParagraf.Contains("{{PRENUME}}"))
                    {
                        // Facem înlocuirea în memoria locală
                        string textNou = textParagraf.Replace("{{NUME}}", "Ion")
                                                     .Replace("{{PRENUME}}", "Popescu");

                        // Ștergem conținutul vechi al paragrafului (cel spart în bucăți)
                        paragraph.RemoveAllChildren<Run>();

                        // Adăugăm un singur element nou cu textul completat
                        Run newRun = new Run();
                        Text newText = new Text(textNou);
                        newRun.Append(newText);
                        paragraph.Append(newRun);

                        Console.WriteLine($" -> Am modificat un paragraf: {textNou}");
                    }
                }

                // Salvare
                mainPart.Document.Save();
            }

            Console.WriteLine("Gata! Verifică fișierul Adeverinta_2_Completat.docx");
        }
    }
}