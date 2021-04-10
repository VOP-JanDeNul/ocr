using Businesscards.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Businesscards.Services.OCR
{
    public abstract class AOcrService
    {
        //undefined fields
        private ArrayList unkown;

        //List of possibilities with number of occurence
        protected Dictionary<string, int> companyDic;
        protected Dictionary<string, int> nameDic;
        protected Dictionary<string, int> natureDic;
        protected Dictionary<string, int> jobTitleDic;
        protected Dictionary<string, int> phoneDic;
        protected Dictionary<string, int> mobileDic;
        protected Dictionary<string, int> emailDic;
        protected Dictionary<string, int> faxDic;
        protected Dictionary<string, int> addressDic;
        private string extraField = "";

        private readonly Dictionary<string, int>[] dictionariesArray;


        //regex
        private Regex regexPhone;
        protected Regex regexPhoneFilter;
        protected Regex regexPhoneFilterInternational;
        private Regex regexNameFilter;
        private Regex regexAddressFilter;
        private Regex regexCompanyFilter;
        private Regex regexAddress;
        private Regex regexWebsite;

        //host email adresses that are not a company
        static private readonly string[] emailadresses = { "gmail", "skynet", "telenet", "icloud", "outlook", "yahoo", "fastmail", "protonmail" };

        public AOcrService()
        {
            unkown = new ArrayList();

            companyDic = new Dictionary<string, int>();
            nameDic = new Dictionary<string, int>();
            natureDic = new Dictionary<string, int>();
            jobTitleDic = new Dictionary<string, int>();
            phoneDic = new Dictionary<string, int>();
            mobileDic = new Dictionary<string, int>();
            emailDic = new Dictionary<string, int>();
            faxDic = new Dictionary<string, int>();
            addressDic = new Dictionary<string, int>();
            extraField = "";

            //regex
            regexPhone = new Regex(@"([+.\/0-9]{8,})");
            regexPhoneFilter = new Regex(@"[;,\t\-\(\)\{\}\[\]\.\* a-z]");
            regexPhoneFilterInternational = new Regex(@"\([^()]*\)");
            regexNameFilter = new Regex(@"[;,\-\+\(\)\{\}\[\]\*0-9]");
            regexAddressFilter = new Regex(@"[;,\+\(\)\{\}\[\]\.\*]");
            regexCompanyFilter = new Regex(@"[;,\+\(\)\{\}\[\]\.\* ]");
            regexAddress = new Regex(@"[ a-zA-Z-]*([0-9]+)[ a-zA-Z-]*");
            regexWebsite = new Regex(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})");
            dictionariesArray = new Dictionary<string, int>[] { companyDic, nameDic, jobTitleDic, phoneDic, mobileDic, emailDic, faxDic, addressDic, natureDic };
        }

        public void resetOCR()
        {
            Console.WriteLine("AOCRService - reseted OCR");
            //reset all dictionaries
            unkown.Clear();

            companyDic.Clear();
            nameDic.Clear();
            natureDic.Clear();
            jobTitleDic.Clear();
            phoneDic.Clear();
            mobileDic.Clear();
            emailDic.Clear();
            faxDic.Clear();
            addressDic.Clear();
            extraField = "";
        }

        public abstract Task<Businesscard> getCard(string imagePath);

        protected Businesscard analyzeText(string[] lines)
        {
            try
            {
                Console.WriteLine("AOCRService - started analyzing the text");
                string previous = "";
                foreach (string line in lines)
                {
                    //first general clean of data, we lower all the data for string comparison and capitalize later were required
                    string trimmed = line.Trim().ToLower();
                    if (trimmed.Length == 0)
                    {
                        continue;
                    }

                    //filter the data on everything, if nothing is assigned we add it to unkown so we do not have to iterate everything again
                    if (!filterOnAll(trimmed, previous))
                    {
                        unkown.Add(trimmed);

                    }
                    // For address
                    previous = trimmed;
                }

                previous = "";
                //we keep iterating the unkown list until no changes are made
                int unkownCount = unkown.Count + 1;

                while (unkownCount > unkown.Count)
                {
                    ArrayList unkownDel = new ArrayList();
                    unkownCount = unkown.Count;
                    foreach (string line in unkown)
                    {
                        if (filterOnAll(line, previous))
                        {
                            //if the line is filtered we will remove it from unkown
                            unkownDel.Add(line);
                        }
                        else
                        {
                            previous = line;
                        }
                    }
                    foreach (string delete in unkownDel)
                    {
                        unkown.Remove(delete);
                    }
                }

                //assigning to random dictionaries
                foreach (string line in unkown)
                {
                    bool breaked = false;
                    foreach (Dictionary<string, int> dic in dictionariesArray)
                    {
                        if (dic.Count() == 0)
                        {
                            Console.WriteLine("random: " + line);
                            addValue(dic, line);
                            breaked = true;
                            break;
                        }
                    }
                    //extra field only when all dictionaries are used. Always a chance to guess correctly
                    if (!breaked && extraField.Equals(""))
                    {
                        Console.WriteLine("extra: " + line);
                        extraField = line;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error" + e);
            }
            return makeCard();
        }

        private Businesscard makeCard()
        {

            Businesscard card = new Businesscard();
            card.Company = getHighest(companyDic, true);
            card.Name = getHighest(nameDic, true);
            card.Jobtitle = getHighest(jobTitleDic, true);
            card.Phone = getHighest(phoneDic, false);
            card.Mobile = getHighest(mobileDic, false);
            card.Email = getHighest(emailDic, false);
            card.Fax = getHighest(faxDic, false);
            string[] address = getHighest(addressDic, true).Split(',');
            if (address?.Length > 0) card.Street = address[0].Trim();
            if (address?.Length > 1)
            {
                string city = "";
            
            for (int i = 1; i < address.Length; i++)
                {
                    city += " "+address[i];
                }
                card.City = city.Trim();
            }
            card.Nature = getHighest(natureDic, true);
            card.Extra = extraField;

            return card;
        }

        private bool filterOnAll(string line, string previous = "")
        {
            bool assigned = false;
            Console.WriteLine("AOCRService filter:" + line);
            //first the filters independently from others
            assigned |= filterOnEmail(line);
            assigned |= filterOnPhone(line);
            assigned |= filterOnMobile(line);
            assigned |= filterOnWebsite(line);
            assigned |= filterOnCompany(line);
            assigned |= filterOnName(line);
            assigned |= filterOnAddress(line, previous);

            return assigned;
        }

        private bool filterOnAddress(string current, string previous)
        {
            previous = regexAddressFilter.Replace(previous, string.Empty);
            current = regexAddressFilter.Replace(current, string.Empty);

            //when name is already in list and occurs once secluded, high chance
            //TODO: filter straat street
            bool previousIsAddress = false;
            bool currentIsAddress = false;
            bool matched = false;

            //check if previous is an address or a part
            if (!regexPhone.Match(regexPhoneFilter.Replace(previous, string.Empty)).Success && !previous.Equals("") && regexAddress.Match(previous).Success)
            {
                previousIsAddress = true;
                matched = true;
                addValue(addressDic, previous);
            }

            //check if current is an address or a part
            if (!regexPhone.Match(regexPhoneFilter.Replace(current, string.Empty)).Success && !current.Equals("") && regexAddress.Match(current).Success)
            {
                currentIsAddress = true;
                matched = true;
                addValue(addressDic, current);
            }

            //if both are an address -> together they form the full address
            if (previousIsAddress && currentIsAddress)
            {
                addValue(addressDic, previous + ", " + current, 5);
            }
            return matched;
        }

        private bool filterOnName(string name)
        {
            name = regexNameFilter.Replace(name, string.Empty);
            //when name is already in list and occurs once secluded, high chance
            if (nameDic.ContainsKey(name))
            {
                addValue(nameDic, name, 5);
                return true;
            }

            //when name a part of name is in nameDic -> the name is not full so we add the full name
            //example J Doe is in nameDic but John Doe appears
            string[] names = name.Split(' ');
            if (names.Length > 1)
            {
                foreach (string n in names)
                {
                    if (nameDic.ContainsKey(n))
                    {
                        addValue(nameDic, name, 10);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool filterOnCompany(string company)
        {
            //when company is already in list and occurs once secluded, high chance
            //addressFilter because company can contain numbers
            string companyFiltered = regexCompanyFilter.Replace(company, string.Empty);
            if (companyDic.ContainsKey(companyFiltered) || companyDic.ContainsKey(companyFiltered.Replace(" ",string.Empty)))
            {
                addValue(companyDic, company, 10);
                return true;
            }
            return false;
        }

        private bool filterOnWebsite(string website)
        {
            //check if string satisfies website regex
            if (regexWebsite.Match(website).Success)
            {
                //website most likely contains company name
                string[] names = website.Split('.');
                for (int i = 1; i < names.Length - 1; i++)
                {
                    if (companyDic.ContainsKey(names[i]))
                    {
                        addValue(companyDic, names[i], 2);
                    }
                    else
                    {
                        addValue(companyDic, names[i]);
                    }
                }

                //if website is like facebook/JanDeNul then is facebook not the company but JanDeNul
                string[] possibleCompany = names[names.Length - 1].Split('/');
                if (possibleCompany.Length > 1 && possibleCompany[1].Length > 0)
                {
                    addValue(companyDic, possibleCompany[1], 2);
                }

                return true;
            }
            return false;
        }

        private bool filterOnPhone(string phone)
        {
            phone = regexPhoneFilterInternational.Replace(phone, string.Empty);
            phone = regexPhoneFilter.Replace(phone, string.Empty);
            //check if string satisfies phone regex
            //TODO: difference mobile and phone
            //TODO: check if mob: or phone: occurs

            if (regexPhone.Match(phone).Success && !mobileDic.ContainsKey(phone))
            {
                if (phoneDic.Count > 0 && mobileDic.Count == 0)
                {
                    addValue(mobileDic, phone);
                }
                addValue(phoneDic, phone);
                return true;
            }
            return false;
        }

        private bool filterOnMobile(string mobile)
        {
            mobile = regexPhoneFilterInternational.Replace(mobile, string.Empty);
            mobile = regexPhoneFilter.Replace(mobile, string.Empty);
            //check if string satisfies phone regex
            //TODO: difference mobile and phone
            //TODO: check if mob: or phone: occurs
            if (regexPhone.Match(mobile).Success && !phoneDic.ContainsKey(mobile))
            {
                if (phoneDic.Count == 0 && mobileDic.Count > 0)
                {
                    addValue(phoneDic, mobile);
                }
                addValue(mobileDic, mobile);
                return true;
            }
            return false;
        }

        private bool filterOnEmail(string email)
        {
            //check if string we can cast string to MailAddress clas (beter than using regex, due to large email formats)
            try
            {
                string addr = new MailAddress(email).Address.Trim();
                addValue(emailDic, addr);
                string[] splitted = addr.Split('@');

                //first section most likely contains name
                string name = splitted[0].Replace('.', ' ');
                addValue(nameDic, name, 3);

                //add parts of the name also in nameDic, if name is J Doe we correct this in nameFilter
                string[] names = name.Split(' ');
                if (names.Length > 1)
                {
                    foreach (string n in names)
                    {
                        addValue(nameDic, n);
                    }
                }

                //seconde section most likely contains company name if not like gmail.com
                string possibleCompany = splitted[1].Split('.')[0];
                if (!emailadresses.Contains(possibleCompany))
                {
                    addValue(companyDic, possibleCompany, 3);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }



        //get string with highest score/ most likely to contain to right value
        private string getHighest(Dictionary<string, int> dic, bool capitalize)
        {
            //default value
            string highest = "";
            int count = 0;
            foreach (KeyValuePair<string, int> kvp in dic)
            {
                //if there are keyValuePairs and highest is still default, first value is highest
                if (count == 0)
                {
                    highest = kvp.Key;
                    count = kvp.Value;
                }
                // if there are more than 1 keyValuePairs, we check occurence. If equal we take current one
                else if (count < kvp.Value)
                {
                    highest = kvp.Key;
                    count = kvp.Value;
                }

            }
            // capitalize the first character of every word
            return capitalize ? Regex.Replace(highest, @"(^\w)|(\s\w)", m => m.Value.ToUpper()) : highest;
        }

        //add score in dictionary for a certain string
        protected void addValue(Dictionary<string, int> dic, string key, int score = 1)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, score);
            }
            else
            {
                dic[key] += score;
            }
        }

        // Easy function used to print the card
        public void PrintCard(Businesscard card)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Company: " + card.Company);
            sb.AppendLine("Name: " + card.Name);
            sb.AppendLine("Nature: " + card.Nature);
            sb.AppendLine("JobTitle: " + card.Jobtitle);
            sb.AppendLine("Phone: " + card.Phone);
            sb.AppendLine("Mobile: " + card.Mobile);
            sb.AppendLine("Email:" + card.Email);
            sb.AppendLine("Fax: " + card.Fax);
            sb.AppendLine("Street: " + card.Street);
            sb.AppendLine("City: " + card.City);
            sb.AppendLine("Date: " + card.Date);
            sb.AppendLine("Origin: " + card.Origin);
            sb.AppendLine("Extra: " + card.Extra);
            Console.WriteLine(sb.ToString());
        }

        // Make an empty card
        public Businesscard MakeEmptyCard()
        {
            Businesscard card = new Businesscard();
            card.Company = "";
            card.Name = "";
            card.Nature = "";
            card.Jobtitle = "";
            card.Phone = "";
            card.Mobile = "";
            card.Email = "";
            card.Fax = "";
            card.Street = "";
            card.City = "";
            card.Date = new DateTime();
            card.Origin = "";
            card.Extra = "";
            return card;
        }
    }
}
