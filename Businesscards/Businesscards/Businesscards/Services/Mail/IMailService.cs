using Businesscards.Models;
using System.Threading.Tasks;

namespace Businesscards.Services.Mail
{
    interface IMailService
    {
        // Simple interface for cleanliness of the code, this defines the task of the
        // mail service, which is only one at the moment: composing an email
        // Makes it easy to later define extra functionality
        Task ComposeEmailAsync(Businesscard card);
    }
}
