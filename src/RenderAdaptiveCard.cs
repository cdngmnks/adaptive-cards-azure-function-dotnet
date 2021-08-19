using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using AdaptiveCards;
using AdaptiveCards.Rendering;
using AdaptiveCards.Rendering.Html;
using AdaptiveCards.Templating;
using RenderAdaptiveCard.Resources;
using Microsoft.Extensions.Primitives;

namespace RenderAdaptiveCard
{
    public static class RenderAdaptiveCard
    {
        [FunctionName("RenderAdaptiveCard")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log) {
            log.LogInformation("got request to render card");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            CardPayloadInput cardPayloadInput = JsonConvert.DeserializeObject<CardPayloadInput>(requestBody);

            AdaptiveCardRenderer renderer = new AdaptiveCardRenderer();
            
            StringValues svQueryHostConfig;
            if (req.Query.TryGetValue("hostConfig", out svQueryHostConfig))
            {
                renderer.HostConfig = AdaptiveHostConfig.FromJson(
                    EmbeddedResourceJsonBase.GetJsonFromEmbeddedResource(string.Format("RenderAdaptiveCard.Resources.HostConfigs.{0}.json", svQueryHostConfig.ToString()))
                );
            }

            ICard cardCreator = CardFactory.Create(cardPayloadInput);
            AdaptiveCard adaptiveCard = cardCreator.Generate();
            
            RenderedAdaptiveCard renderedCard = renderer.RenderCard(adaptiveCard);
            HtmlTag htmlTag = renderedCard.Html;

            return new OkObjectResult(htmlTag.ToString());
        }
    }
}
