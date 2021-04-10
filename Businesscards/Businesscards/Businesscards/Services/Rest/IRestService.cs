using Businesscards.Models;
using System.Threading.Tasks;

namespace Businesscards.Services.Rest
{
    public interface IRestService
    {
        // Simple interface for cleanliness of the code, this defines the task of the
        // rest service, which is only one at the moment: sending cards to endpoint
        // Makes it easy to later define extra functionality such as changing already existing cards and retrieving them
        Task SendCardsAsync(Businesscard card);
    }
}
