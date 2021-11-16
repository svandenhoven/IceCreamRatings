using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Flurl.Http;

namespace IceCreamRatings
{
    public static class CreateRating
    {
        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "Icecream",
                collectionName: "Ratings",
                ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut,

            ILogger log)
        {
            log.LogInformation("A new rating has been created");

            ///
//            {
//                "userId": "cc20a6fb-a91f-4192-874d-132493685376",
                //  "productId": "4c25613a-a3c2-4ef3-8e02-9c335eb23204",
                //  "locationName": "Sample ice cream shop",
                //  "rating": 5,
                //  "userNotes": "I love the subtle notes of orange in this ice cream!"
                //}


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if(!await GetUserAsync(data.userId.Value))
            {
                return new BadRequestResult();
            }

            if (!await GetProductAsync(data.productId.Value))
            {
                return new BadRequestResult();
            }

            if (data.rating.Value < 0 || data.rating.Value > 5)
            {
                return new BadRequestResult();
            }

            var rating = new Rating();
                rating.id = Guid.NewGuid().ToString();
                rating.timestamp = DateTime.Now.ToString();
                rating.locationName = data.locationName.Value;
                rating.userId = data.userId.Value;
                rating.rating = data.rating;
                rating.userNotes = data.userNotes.Value;

            await documentsOut.AddAsync(rating);

            return new OkObjectResult(rating);

        }

        private static async Task<bool> GetUserAsync(string id)
        {
            try
            {
                var response = await $"https://serverlessohapi.azurewebsites.net/api/GetUser?userId={id}".GetAsync().ReceiveString();
                return response.ToString().Contains(id) ? true : false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private static async Task<bool> GetProductAsync(string productId)
        {
            try
            {
                var response = await $"https://serverlessohapi.azurewebsites.net/api/GetProduct?productId={productId}".GetAsync().ReceiveString();
                return response.ToString().Contains(productId) ? true : false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }

    public class Rating
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string productId { get; set; }
        public string timestamp { get; set; }
        public string locationName { get; set; }
        public int rating { get; set; }
        public string userNotes { get; set; }
    }

}
