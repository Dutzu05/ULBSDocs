using System;
using System.IO;
using Spire.Doc;

namespace DocxToPdfConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = "input.docx";
            string outputFile = "output.pdf";

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("Fisierul input.docx nu a fost gasit.");
                return;
            }

            ConvertDocxToPdf(inputFile, outputFile);

            Console.WriteLine("Conversia DOCX -> PDF s-a finalizat cu succes.");
        }

        static void ConvertDocxToPdf(string inputPath, string outputPath)
        {
            Document document = new Document();
            document.LoadFromFile(inputPath);
            document.SaveToFile(outputPath, FileFormat.PDF);
        }
    }
}
