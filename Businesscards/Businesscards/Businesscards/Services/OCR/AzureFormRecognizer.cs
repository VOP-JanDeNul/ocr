using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Businesscards.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


// source: https://docs.microsoft.com/en-us/azure/cognitive-services/form-recognizer/quickstarts/client-library?tabs=preview%2Cv2-1&pivots=programming-language-csharp
// source: https://docs.microsoft.com/en-us/azure/cognitive-services/form-recognizer/concept-business-cards
namespace Businesscards.Services.OCR
{
    class AzureFormRecognizer : AOcrService
    {

        //the treshold can be 
        private static readonly float confidenceTreshold = 0.1f;
        private readonly string[] keys = { "CompanyNames", "ContactNames", "JobTitles", "WorkPhones", "MobilePhones", "OtherPhones", "Emails", "Faxes", "Addresses", "Departments" };
        private readonly string[] phones = { "WorkPhones", "MobilePhones", "OtherPhones", "Faxes" };
        private readonly Dictionary<string, int>[] dictionaries;

        private FormRecognizerClient recognizerClient;
        private string imagePath;
        private string[] results;
        private ArrayList assignedValues;
        public AzureFormRecognizer()
        {
            dictionaries = new Dictionary<string, int>[] { companyDic, nameDic, jobTitleDic, phoneDic, mobileDic, phoneDic, emailDic, faxDic, addressDic, natureDic };
            assignedValues = new ArrayList();
            //check key and endpoint
            recognizerClient = new FormRecognizerClient(new Uri(Constants.AFRendpoint), new AzureKeyCredential(Constants.AFRapiKey));

        }

        public override async Task<Businesscard> getCard(string path)
        {
            Console.WriteLine("--------------------------->>>>>>>>>>> start: " + DateTime.Now.ToString("h:mm:ss tt"));
            imagePath = path;

            //Call the Azure api for analyze the business card (sends filtered data) and regonize content (send unfiltered data so we can filter it)
            //the filtered data does not always contains everything correctly
            try
            {
                await Task.WhenAll(AnalyzeBusinessCardPath(), RecognizeContent());
                Console.WriteLine("--------------------------->>>>>>>>>>> OCR end: " + DateTime.Now.ToString("h:mm:ss tt"));
                foreach(string value in assignedValues)
                {
                    for(int i = 0; i < results.Length; i++)
                    {
                        if (value.Equals(results[i])) {
                            Console.WriteLine("---------------------------< verwijderen dubbele waarde: " + results[i]);
                            results[i] = "";
                        }
                    }
                }
                return analyzeText(results);
            }
            catch(Exception ex)
            {
                throw new BadRequestException(ex.ToString());
            }
        }

        private async Task AnalyzeBusinessCardPath()
        {
            using (var stream = new FileStream(imagePath, FileMode.Open))
            {
                var options = new RecognizeBusinessCardsOptions() { Locale = "en-US" };

                RecognizeBusinessCardsOperation operation = await recognizerClient.StartRecognizeBusinessCardsAsync(stream, options);
                Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
                RecognizedFormCollection businessCards = operationResponse.Value;

                //only 1 business Card so we take the first page
                addValues(businessCards[0]);
            }


        }

        private async Task RecognizeContent()
        {
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                Response<FormPageCollection> response = await recognizerClient.StartRecognizeContentAsync(stream).WaitForCompletionAsync();
                FormPageCollection formPages = response.Value;

                //only 1 business Card so we take the first
                results = formatResults(formPages[0]);
            }

        }
        private string[] formatResults(FormPage formPages)
        {
            List<string> lines = new List<string>();
            foreach (FormLine line in formPages.Lines)
            {
                lines.Add(line.Text);
                Console.WriteLine("---------------------------< tekst voor algoritme: " + line.Text);
            }

            return lines.ToArray();
        }

        private void checkField(RecognizedForm businessCard, string key, Dictionary<string, int> dic)
        {

            FormField multipleField;
            if (businessCard.Fields.TryGetValue(key, out multipleField) && multipleField.Value.ValueType == FieldValueType.List)
            {
                foreach (FormField singleField in multipleField.Value.AsList())
                {
                    if (singleField.Confidence >= confidenceTreshold && !singleField.ValueData.Text.Equals(""))
                    {
                        if (Array.Exists(phones, element => element.Equals(key)))
                        {
                            string filtered = regexPhoneFilter.Replace(regexPhoneFilterInternational.Replace(singleField.ValueData.Text, string.Empty), string.Empty);
                            addValue(dic,filtered, 10);
                            assignedValues.Add(filtered);
                        }
                        else
                        {
                            addValue(dic, singleField.ValueData.Text, 10);
                        }
                        assignedValues.Add(singleField.ValueData.Text);
                        Console.WriteLine("---------------------------<Dic:"+key+" OCR: " + singleField.ValueData.Text+ " treshold: "+ singleField.Confidence);
                    }

                }
            }

        }


        private void addValues(RecognizedForm businessCard)
        {
            //companyDic, nameDic, jobTitleDic, phoneDic, mobileDic,phoneDic, emailDic, faxDic, addressDic, natureDic
            for (int i = 0; i < dictionaries.Length; i++)
            {
                checkField(businessCard, keys[i], dictionaries[i]);
            }
        }
    }
}

