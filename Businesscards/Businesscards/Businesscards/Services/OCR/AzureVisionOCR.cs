using Businesscards.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Businesscards.Services.OCR
{
    public class AzureVisionOCR : AOcrService
    {
        private ComputerVisionClient cv;
        public AzureVisionOCR()
        {
            cv = new ComputerVisionClient(new ApiKeyServiceClientCredentials(Constants.AVSubscriptionKey)) { Endpoint = Constants.AVEndPoint };
        }

        public override async Task<Businesscard> getCard(string imagePath)
        {
            try
            {
                using (var image = File.OpenRead(imagePath))
                {
                    Businesscard card = MakeEmptyCard();
                    Console.WriteLine("AZUREVISION - Received image");
                    var textHeaders = await cv.ReadInStreamAsync(image);
                    string operationLocation = textHeaders.OperationLocation;
                    string operationId = operationLocation.Substring(operationLocation.Length - 36);
                    // Extract the text
                    ReadOperationResult results = null;
                    Console.WriteLine("AZUREVISION - sending image to Azure Service");
                    do
                    {
                        results = await cv.GetReadResultAsync(Guid.Parse(operationId));
                    }
                    while ((results.Status == OperationStatusCodes.Running ||
                        results.Status == OperationStatusCodes.NotStarted));

                    Console.WriteLine("AZUREVISION - received information from Azure Service");
                    card = analyzeText(formatResults(results));
                    PrintCard(card);
                    return card;
                }
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.ToString());
            }
        }


        //format the results in lines so that AOcrService can analyse them
        private string[] formatResults(ReadOperationResult results)
        {
            List<string> lines = new List<string>();
            foreach (ReadResult page in results.AnalyzeResult.ReadResults)
            {
                foreach (Line line in page.Lines)
                {
                    lines.Add(line.Text);
                }
            }
            return lines.ToArray();
        }
    }
}
