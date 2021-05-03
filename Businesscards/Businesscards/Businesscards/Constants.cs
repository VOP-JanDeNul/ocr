namespace Businesscards
{
    public static class Constants
    {
        // Endpoint URL
        public static readonly string RestUrl = "https://businesscardendpoint20210409142733.azurewebsites.net/api/cards";

        // AzureVisionOCR.cs
        public static readonly string AVSubscriptionKey = "6f34ceacacf24c228450d156a8ac809f";
        public static readonly string AVEndPoint = "https://bcocr.cognitiveservices.azure.com/";

        // AzurFormRecognizer.cs
        public static readonly string AFRendpoint = " https://frocr.cognitiveservices.azure.com/";
        public static readonly string AFRapiKey = "8e110e64c1d84a58912eeb978c04135a";
    }
}
