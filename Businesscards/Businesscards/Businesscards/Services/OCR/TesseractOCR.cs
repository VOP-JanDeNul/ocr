using Businesscards.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Tesseract;
using Xamarin.Essentials;

namespace Businesscards.Services.OCR
{
    public class TesseractOCR : AOcrService
    {
        public TesseractOCR()
        {
            // Tesseract needs training files so load these into device memory
            PrepareFiles();
        }

        async void PrepareFiles()
        {
            // create the tessdata directory 
            string dataPath = Path.Combine(FileSystem.AppDataDirectory, "tessdata");
            Directory.CreateDirectory(dataPath);
            using (var stream = await FileSystem.OpenAppPackageFileAsync("eng.traineddata"))
            {
                using (var reader = new StreamReader(stream))
                {
                    string fileContents = await reader.ReadToEndAsync();
                    string localPath = Path.Combine(FileSystem.AppDataDirectory, "tessdata", "eng.traineddata");
                    File.WriteAllText(localPath, fileContents);
                }
            }
            using (var stream = await FileSystem.OpenAppPackageFileAsync("pdf.ttf"))
            {
                using (var reader = new StreamReader(stream))
                {
                    string fileContents = await reader.ReadToEndAsync();
                    string localPath = Path.Combine(FileSystem.AppDataDirectory, "tessdata", "pdf.ttf");
                    File.WriteAllText(localPath, fileContents);
                }
            }
        }

        public override async Task<Businesscard> getCard(string imagePath)
        {
            try
            {
                Debug.WriteLine("OCR - Entered the OCR program with path: " + imagePath + "\n");
                using (var engine = new TesseractEngine(Path.Combine(FileSystem.CacheDirectory, "tessdata"), "eng", EngineMode.Default))
                {
                    // We think this Pix function is why this OCR framework doesn't work (libtonica library)
                    using (var img = Pix.LoadFromFile(imagePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            // function analyzeText expects an array of strings (the lines)
                            // but Tesseract returns only one page, so we convert this into a single line
                            string[] lines = new string[] { };
                            lines[0] = page.GetText();
                            return await analyzeText(lines);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.ToString());
            }
        }
    }
}
