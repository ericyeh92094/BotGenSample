using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace BotGenSample
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                //Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                Activity reply = await HandleBotMessage(activity);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private async Task<Activity> HandleBotMessage(Activity message)
        {
            LuisHelp cognitive = new LuisHelp();
            string CarCaringString;
            CarCaringLUIS carLUIS = await LuisHelp.GetEntityFromLUIS(message.Text);
            if (carLUIS.intents.Count() > 0)
            {
                Intent maxscore_intent = carLUIS.intents.Max();

                switch (maxscore_intent.intent)
                {
                    case "StoreLocation":
                        string storeURL = "";
                        CarCaringString = cognitive.GetStoreLocation(carLUIS, ref storeURL);
                        break;
                    case "CheckItem":
                        CarCaringString = cognitive.GetItem(carLUIS);
                        break;
                    case "News":
                        CarCaringString = cognitive.GetNews(carLUIS);
                        break;
                    case "Tips":
                        CarCaringString = await cognitive.GetTips(carLUIS, "FromUser", "ToUser");
                        break;
                    default:
                        CarCaringString = await cognitive.GetBingSearch(message.Text); // "您可以到网站去查询我门的最新讯息。";


                        break;
                }
            }
            else
            {
                CarCaringString = "您可以到网站去查询我门的最新讯息。";
            }
            // return our reply to the user
            return message.CreateReply(CarCaringString);
        }
    }
    public class ReplyType
    {
        /// <summary>
        /// 普通文本消息
        /// </summary>
        public static string Message_Text
        {
            get { return @"<xml>
                        <ToUserName><![CDATA[{0}]]></ToUserName>
                        <FromUserName><![CDATA[{1}]]></FromUserName>
                        <CreateTime>{2}</CreateTime>
                        <MsgType><![CDATA[text]]></MsgType>
                        <Content><![CDATA[{3}]]></Content>
                        </xml>"; }
        }
        /// <summary>
        /// 图文消息主体
        /// </summary>
        public static string Message_News_Main
        {
            get
            {
                return @"<xml>
                        <ToUserName><![CDATA[{0}]]></ToUserName>
                        <FromUserName><![CDATA[{1}]]></FromUserName>
                        <CreateTime>{2}</CreateTime>
                        <MsgType><![CDATA[news]]></MsgType>
                        <ArticleCount>{3}</ArticleCount>
                        <Articles>
                        {4}
                        </Articles>
                        </xml> ";
            }
        }
        /// <summary>
        /// 图文消息项
        /// </summary>
        public static string Message_News_Item
        {
            get
            {
                return @"<item>
                        <Title><![CDATA[{0}]]></Title> 
                        <Description><![CDATA[{1}]]></Description>
                        <PicUrl><![CDATA[{2}]]></PicUrl>
                        <Url><![CDATA[{3}]]></Url>
                        </item>";
            }
        }
    }
}