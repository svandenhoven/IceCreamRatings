using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IceCreamRatings
{
    public static class GetRatings
    {
        [FunctionName("GetRatings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetRatings/{userId}")]
                HttpRequest req,
            [CosmosDB(
                databaseName: "Icecream",
                collectionName: "Ratings",
                ConnectionStringSetting = "CosmosDbConnectionString",
                SqlQuery = "SELECT * FROM c Where c.userId = {userId}")]
                IEnumerable<Rating> ratings,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var r = (List<Rating>)ratings;
            if (r.Count == 0)
            {
                return new NotFoundObjectResult("The rating is not found");
            }
            else
            {
                return new OkObjectResult(ratings);
            }

        }
    }
}
