using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using AdaptiveCards;
using APIXULib;
using System.Configuration;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json;
using trialweatherbot;

namespace trialweatherbot.Dialogs
{

    [LuisModel("dbb42685-9826-4a0a-ab1a-3c8e958fc34f", "ac3bc87867f148e982c6c9105fc35446")]
    [Serializable]
    public class SimpleLuisDialog : LuisDialog<object>
    {

        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"I am a bot, which helps you to find weather and news at different locations");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Weather")]
        public async Task Weather(IDialogContext context, LuisResult result)
        {
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            EntityRecommendation entitylocation;
            List<string> entitiespresent = new List<string>();

            foreach (var entityList in result.Entities)
            {
                entitiespresent.Add(entityList.Entity);
            }


            if (entitiespresent.Count != 0)
            {
                foreach (var epl in entitiespresent)
                {
                    string Loc = epl;
                    if (result.TryFindEntity("Location", out entitylocation))
                    {
                        AdaptiveCard acard = Card.GetCard(Loc);
                        var attachment = new Attachment()
                        {
                            Content = acard,
                            ContentType = "application/vnd.microsoft.card.adaptive",
                        };
                        reply.Attachments.Add(attachment);

                    }

                }
                await context.PostAsync(reply);
            }
            else
            {
                await context.PostAsync("Enter valid location");

            }



            context.Wait(MessageReceived);
        }


        private async Task WeatherDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            string Loc = await result;
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();


            AdaptiveCard a1card = Card.GetCard(Loc);
            var attachment = new Attachment()
            {
                Content = a1card,
                ContentType = "application/vnd.microsoft.card.adaptive",
            };
            reply.Attachments.Add(attachment);
            await context.PostAsync(reply);


            context.Wait(this.MessageReceived);
        }





        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I have no idea what you are talking about.");
            context.Wait(MessageReceived);
        }




        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();
            message.Attachments = new List<Attachment>();
            var thumbnailCard = new ThumbnailCard
            {
                Title = "Weather & News  Help",
                
                Text = "We do help you to find the weather and news of a partciular location",
                Images = new List<CardImage> {
                    new CardImage("https://www.consumer.ftc.gov/sites/default/files/rotator-images/mini-rotator-weather-emergencies.png") },
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.OpenUrl, "View More", value: "https://www.google.co.in/search?q=google+weather") }
            };
            Attachment plAttachment = thumbnailCard.ToAttachment();
            message.Attachments.Add(plAttachment);
            await context.PostAsync(message);
            context.Wait(this.MessageReceived);

        }
        public async Task<BingNews> getBingNews(string query)
        {
            BingNews bingNews;
            String bingUri = "https://api.cognitive.microsoft.com/bing/v5.0/news/search/?count=50&q=" + query;
            string rawResponse;
            HttpClient httpClient = new HttpClient()
            {
                DefaultRequestHeaders = {
 {"Ocp-Apim-Subscription-Key", "57efd08127c24467a65f2a5067986897"},
 {"Accept", "application/json"}
 }
            };
            try
            {
                rawResponse = await httpClient.GetStringAsync(bingUri);
                bingNews = JsonConvert.DeserializeObject<BingNews>(rawResponse);
            }
            catch (Exception e)
            {
                return null;
            }
            return bingNews;
        }


        [LuisIntent("News")]
        public async Task News(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            var reply = context.MakeMessage();
            EntityRecommendation locEntity;
            if (result.TryFindEntity("Location", out locEntity))
            {
                BingNews bingNews = await getBingNews(locEntity.Entity);


                if (bingNews == null || bingNews.totalEstimatedMatches == 0)
                {
                    reply.Text = "Sorry, couldn't find any news about '" + locEntity.Entity ;
                }
                else
                {
                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments = new List<Attachment>();
                    for (int i = 0; i < 10 && i < (bingNews?.totalEstimatedMatches ?? 0);i++)
                    {
                        var article = bingNews.value[i];
                        HeroCard attachment = new HeroCard()
                        {
                            Title = article.name.Length > 60 ?
                        article.name.Substring(0, 57) + "..." : article.name,
                            Text = article.provider[0].name + ", " +
                        article.datePublished.ToString("d") + " - " +
                        article.description,
                            Images = new List<CardImage>() { new
                                                            CardImage(article.image?.thumbnail?.contentUrl +"&w=400&h=400") },
                            Buttons = new List<CardAction>() { new CardAction(
                                                                    ActionTypes.OpenUrl,
                                                                    title: "View on Web",
                                                            value: article.url)}
                        };
                        reply.Attachments.Add(attachment.ToAttachment());
                    }
                }
            }

            else
            {
                reply.Text = $"I couldn't understand what you're looking for. ";
            }
            await context.PostAsync(reply);
            context.Wait(this.MessageReceived);

        }
    }
}
        



        
    