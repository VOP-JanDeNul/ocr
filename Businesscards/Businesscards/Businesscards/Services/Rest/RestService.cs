using Businesscards.Models;
using System;
//using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Businesscards.Services.Rest
{
    public class RestService : IRestService
    {
        private HttpClient client;
        private JsonSerializerOptions serializerOptions;

        // Initializing the service
        public RestService()
        {
            client = new HttpClient();
            serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        // Sends the card to the endpoint and returns a boolean indicating if this was succesfull or not
        public async Task SendCardsAsync(Businesscard card)
        {
            try
            {
                string json = JsonSerializer.Serialize<Businesscard>(card, serializerOptions);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                // Constants.RestUrl references a constants class
                response = await client.PostAsync(Constants.RestUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("card was sent succesfully");
                }
                else
                {
                    Console.WriteLine("card could be sent to the endpoint!");
                    throw new RestException("card wasn't sent succesfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("card could be sent to the endpoint!");
                throw new RestException(ex.ToString());
            }
        }
    }
}
