using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Businesscards.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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

        //start analysing data
        public override async Task<Businesscard> getCard(string path)
        {
            Debug.WriteLine(">> start: " + DateTime.Now.ToString("h:mm:ss tt"));
            imagePath = path;

            try
            {
                //Call the Azure api 2 times async: for analyze the business card (sends filtered data) and regonize content (send unfiltered data so we can filter it)
                //the filtered data does not always contains everything correctly
                await Task.WhenAll(AnalyzeBusinessCardPath(), RecognizeContent());
                Debug.WriteLine(">> OCR end: " + DateTime.Now.ToString("h:mm:ss tt"));

                //filtering double data
                foreach (string value in assignedValues)
                {
                    //if length is small, we dont delete to preserve important data
                    if (value.Length <= 3) continue;
                    //no spaces better to compare
                    string trimmedValue = value.Replace(" ", string.Empty);
                    for (int i = 0; i < results.Length; i++)
                    {
                        if (results[i].Contains("@") || (Math.Round(value.Length*1.5)<=results[i].Length && value.Length<=10))
                        {
                            continue;
                        }
                        string trimmedResult = results[i].Replace(" ", string.Empty);
                        if (trimmedValue.Equals(trimmedResult))
                        {
                            Debug.WriteLine("--< verwijderen dubbele waarde: " + results[i] + " gevonden: "+trimmedValue+" = "+trimmedResult);
                            results[i] = "";
                        }
                        else if (trimmedResult.Contains(trimmedValue))
                        {
                            Debug.WriteLine("--< verwijderen dubbele waarde: " + results[i] + " gevonden: " + trimmedValue + " = " + trimmedResult);
                            results[i] = "";
                        }
                    }
                }
                return await analyzeText(results);
            }
            catch (Exception ex)
            {
                throw new FormRecognizerException(ex.ToString());
            }
        }

        //analyze the business card (sends filtered data) value + tag, example: "Emails" "example@example.com"
        private async Task AnalyzeBusinessCardPath()
        {
            using (var stream = new FileStream(imagePath, FileMode.Open))
            {
                try
                {
                    var options = new RecognizeBusinessCardsOptions() { Locale = "en-US" };

                    RecognizeBusinessCardsOperation operation = await recognizerClient.StartRecognizeBusinessCardsAsync(stream, options);
                    Response<RecognizedFormCollection> operationResponse = await operation.WaitForCompletionAsync();
                    RecognizedFormCollection businessCards = operationResponse.Value;

                    //only 1 business Card so we take the first page
                    addValues(businessCards[0]);
                }
                catch (Exception ex)
                {
                    throw new FormRecognizerException(ex.ToString());
                }
            }
        }

        //sends content from the card in lines, example "example@example.com". Data is used to improve AnalyzeBusinessCardPath() 
        private async Task RecognizeContent()
        {
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    Response<FormPageCollection> response = await recognizerClient.StartRecognizeContentAsync(stream).WaitForCompletionAsync();
                    FormPageCollection formPages = response.Value;

                    //only 1 business Card so we take the first
                    results = formatResults(formPages[0]);
                }
                catch (Exception ex)
                {
                    throw new FormRecognizerException(ex.ToString());
                }
            }
        }

        //format the data from RecognizeContent into lines of strings 
        private string[] formatResults(FormPage formPages)
        {
            List<string> lines = new List<string>();
            foreach (FormLine line in formPages.Lines)
            {
                string lineAsString = line.Text.Trim().ToLower();
                //not higher than 3 is seen as garbarge and will lower the quality of the result + easy to type for the user and most names start from 4+
                if (lineAsString.Length <= 3) continue;
                if (lineAsString.Contains("@")) lineAsString = correctEmail(lineAsString);
                string[] splitted = splitBigLine(lineAsString);
                foreach(string split in splitted)
                {
                    if (split.Length <= 3) continue;
                    lines.Add(split);
                    Debug.WriteLine("--< tekst voor algoritme: " + split);
                }
                if (splitted.Length > 1)
                {
                    lines.Add(lineAsString);
                    Debug.WriteLine("--< tekst voor algoritme: " + lineAsString);
                }
            }
            return lines.ToArray();
        }

        //assign the field in the right dictionary from the AnalyzeBusinessCardPath() function
        private void checkField(RecognizedForm businessCard, string key, Dictionary<string, int> dic)
        {

            FormField multipleField;
            if (businessCard.Fields.TryGetValue(key, out multipleField) && multipleField.Value.ValueType == FieldValueType.List)
            {
                foreach (FormField singleField in multipleField.Value.AsList())
                {
                    if (singleField.Confidence >= confidenceTreshold && !singleField.ValueData.Text.Equals(""))
                    {
                        string value = singleField.ValueData.Text.Trim().ToLower();
                        value = replaceBigLine(value);
                        int score = (int) Math.Round(singleField.Confidence*10.0);
                        if (key.Equals("Emails") && !new EmailAddressAttribute().IsValid(value))
                        {
                            //if Email is not valid, we dont add it
                            Debug.WriteLine("We did not add Email: "+ value);
                            continue;
                        }
                        else if (Array.Exists(phones, element => element.Equals(key)))
                        {
                            string filtered = regexPhoneFilter.Replace(regexPhoneFilterInternational.Replace(value, string.Empty), string.Empty);
                            assignedValues.Add(filtered);
                            assignedValues.Add(value);
                            if (dic.Count > 0)
                            {
                                dic = getEmptyPhoneDic();
                                if (dic is null)
                                {
                                    Debug.WriteLine("added: "+filtered + "to extraField");
                                    extraField = filtered;
                                    continue;
                                }
                            }
                            addValue(dic, filtered, 10);

                        }
                        else
                        {
                            
                            //if there is already a value with a higher score
                            if (dic.Count > 0 && Int32.Parse(getHighest(dic,false,true))>=score)
                            {
                                if (!extraField.Equals(""))
                                {
                                    addValue(dic, value, score);
                                    continue;
                                }
                                Debug.WriteLine("--<Dic:" + key +" input: "+value + " score: " + score +"highest: "+ Int32.Parse(getHighest(dic, false, true)));
                                extraField = value;
                            }
                            addValue(dic, value, score);
                            if (!key.Equals("Emails"))
                            {
                                assignedValues.Add(value);
                            }
                        }
                        
                        Debug.WriteLine("--<Dic:" + key + " OCR: " +value + " treshold: " + singleField.Confidence + " score: "+score);
                    }
                }
            }
        }

        //check which phone dictionary should be used
        private Dictionary<string, int> getEmptyPhoneDic()
        {
            if (phoneDic.Count == 0)
            {
                return phoneDic;
            }
            if (mobileDic.Count == 0)
            {
                return mobileDic;
            }
            if (faxDic.Count == 0)
            {
                return faxDic;
            }
            return null;
        }

        //assigning the right key with the right dictionary with for loop so we dont use to many if-else
        private void addValues(RecognizedForm businessCard)
        {
            /* keys:
             * "CompanyNames", "ContactNames", "JobTitles", "WorkPhones", "MobilePhones","Faxes", "OtherPhones", "Emails", "Addresses", "Departments"
             * dictionaries:
             * companyDic, nameDic, jobTitleDic, phoneDic, mobileDic, faxDic, phoneDic, emailDic,  addressDic, natureDic
            */
            for (int i = 0; i < dictionaries.Length; i++)
            {
                checkField(businessCard, keys[i], dictionaries[i]);
            }
        }
    }
}

