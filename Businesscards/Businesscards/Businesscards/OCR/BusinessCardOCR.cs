using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Tesseract;
using Xamarin.Essentials;

namespace Businesscards.OCR
{
    class BusinessCardOCR
    {
        public static Dictionary<string, List<string>> getText(string imagePath)
        {
            try
            {
                Console.WriteLine("OCR - Entered the OCR program with path: "+imagePath+"\n");                
                using (var engine = new TesseractEngine(Path.Combine(FileSystem.CacheDirectory, "tessdata"), "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(imagePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            Console.WriteLine("OCR - Values found!");
                            var values = new Dictionary<string, List<string>>();

                            string text = page.GetText();
                            string[] splitText = text.Split(' ');
                            // business name - for now first recognized word
                            values["business"].Add(splitText[0]);

                            // person name - for now the next two words concatenated (first and last name)
                            values["name"].Add(splitText[1] + splitText[2]);

                            // jobtitle - random word - don't know how to do best;
                            values["title"].Add(splitText[4]);

                            // phone number
                            MatchCollection matches = Regex.Matches(text, @"[ a-zA-Z ]* ([ +.\/0-9]{8,}) [ a-zA-Z ]*$");
                            if (matches.Count > 0)
                            {
                                // take the first value of all phonenumbers found, first one most likely the most important one
                                foreach (Match match in matches)
                                {
                                    values["phone"].Add(match.ToString());
                                }
                            }

                            // email adresses
                            matches = Regex.Matches(text, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                            if (matches.Count > 0)
                            {
                                // take the first value of all emailadresses found, first one most likely the most important one
                                foreach (Match match in matches)
                                {
                                    values["email"].Add(match.ToString());
                                }
                            }
                            Console.WriteLine("OCR - Returning values!");
                            return values;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Error: " + e.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(e.ToString());
                return new Dictionary<string, List<string>>();
            }
        }
    }
}
