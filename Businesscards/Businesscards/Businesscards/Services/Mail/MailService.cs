using Businesscards.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Businesscards.Services.Mail
{
    // class for sending an automated email using native email app
    // source: https://docs.microsoft.com/en-us/xamarin/essentials/email?context=xamarin%2Fandroid&tabs=android
    public class MailService : IMailService
    {
        private const string DestinationEmail = "jan.denul.vop@gmail.com";
        private const string Subject = "New Business Card";
        private static string EmailContent = "This is an automated email from the Jan De Nul Business Card App.\n\nVisualization of the business card:\n" +
            "Name:\t\t{0}\n" +
            "Job title:\t\t{1}\n" +
            "Organization:\t\t{2}\n" +
            "Nature:\t\t{3}\n\n" +
            "Mobile number:\t{4}\n" +
            "Phone number:\t{5}\n" +
            "Email address:\t{6}\n" +
            "Fax:\t{7}\n" +
            "Address:\t{8}\n" +
            "Extra:\t{9}\n\n" +
            "Creation date:\t{10}\n" +
            "Origin:\t{11}\n" +
            "The business card is also represented in {12}.vcf";

        public MailService()
        {

        }

        public async Task ComposeEmailAsync(Businesscard card)
        {
            string Name = card.Name;
            string Company = card.Company;
            string JobTitle = card.Jobtitle;
            string Nature = card.Nature;
            string MobileNumber = card.Mobile;
            string PhoneNumber = card.Phone;
            string EmailAdress = card.Email;
            string Fax = card.Fax;
            string Address = card.Street + ", " + card.City;
            string Extra = card.Extra;
            string CreationDate = card.Date.ToString();
            string Origin = card.Origin;
            string FileName = Name.Trim().Replace(" ", "_");
            string data = card.Base64; // this is base64 of imageiVBORw0KGgoAAAANSUhEUgAAAcIAAAENC...................


            string Result = string.Format(EmailContent, Name, JobTitle, Company, Nature, MobileNumber, PhoneNumber, EmailAdress, Fax, Address, Extra, CreationDate, Origin, FileName);
            var vcf = new StringBuilder();

            vcf.AppendLine("BEGIN:VCARD");
            vcf.AppendLine("VERSION:4.0");
            vcf.AppendLine($"FN:{Name}");
            vcf.AppendLine($"ORG:{Company}");
            vcf.AppendLine($"TITLE:{JobTitle}");
            vcf.AppendLine($"TEL;TYPE=work,voice;VALUE=uri:tel:{PhoneNumber}");
            vcf.AppendLine($"TEL;TYPE=work,mobile;VALUE=uri:tel:{MobileNumber}");
            vcf.AppendLine($"TEL;TYPE=work,fax;VALUE=uri:fax:{Fax}");
            vcf.AppendLine($"ADR; TYPE = WORK; PREF = 1; LABEL = {Address} :;; {Address} ");
            vcf.AppendLine($"EMAIL:{EmailAdress}");
            vcf.AppendLine($"REV:{CreationDate}");
            vcf.AppendLine($"NOTE:{Extra}");
            vcf.AppendLine($"PHOTO;;TYPE=PNG:{data}");
            vcf.AppendLine("END:VCARD");

            EmailMessage message = new EmailMessage(Subject, Result, DestinationEmail);

            var fn = FileName + ".vcf";     // filename 
            try
            {
                var file = Path.Combine(FileSystem.CacheDirectory, fn);
                File.WriteAllText(file, vcf.ToString());

                message.Attachments.Add(new EmailAttachment(file));

                await Email.ComposeAsync(message);
            }
            catch(Exception ex)
            {
                throw new MailException(ex.ToString());
            }
            
        }


    }
}
