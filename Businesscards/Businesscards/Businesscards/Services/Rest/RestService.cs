using Businesscards.Models;
using System;
using System.Diagnostics;
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
                // Serializing the businesscard object into a json string
                string json = JsonSerializer.Serialize<Businesscard>(card, serializerOptions);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                // Send the card asynchronously to the url and wait for the response
                // Constants.RestUrl references a constants class
                response = await client.PostAsync(Constants.RestUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    // If the response was succesful (200 statuscode) then the card was sent succesfully
                    Debug.WriteLine("RestService: card was sent succesfully");
                }
                else
                {
                    // If not then something went wrong, let the calling part of the application know with an exception
                    Debug.WriteLine("RestService: card could not be sent to the endpoint!");
                    throw new RestException("RestService: card wasn't sent succesfully");
                }
            }
            catch (Exception ex)
            {
                throw new RestException("RestService: Card wasn't sent succesfully - Probably due to lack of internet connection");
            }
        }
    }
}
