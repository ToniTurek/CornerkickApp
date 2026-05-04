using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CornerkickApp.Controllers.Member;

namespace WebsocketChat.Websocket
{
    public class WebsocketHandler : IWebsocketHandler
  {
    private readonly IConfiguration _configuration;
    public static List<SocketConnection> websocketConnections = new List<SocketConnection>();
    Random rnd = new Random();

    public class OpenAISettings
    {
      public string ApiKey { get; set; }
      public string Organization { get; set; }
    }

    public class ChatMessage
    {
      public DateTime date { get; set; }
      public string user { get; set; }
      public string message { get; set; }
      public string color { get; set; }
    }
    public static List<ChatMessage> ltChatMessages = new List<ChatMessage>();

    public WebsocketHandler(IConfiguration configuration)
    {
      _configuration = configuration;

      SetupCleanUpTask();
    }

    public async Task Handle(CornerkickManager.User usr, WebSocket webSocket, bool bForceLogin = false)
    {
      if (usr == null) return;

      Guid guid = new Guid(usr.id);

      SocketConnection sc_old = websocketConnections.Find(w => w.Id == guid);
      lock (websocketConnections) {
        if (bForceLogin) {
          websocketConnections.Remove(sc_old);
          sc_old = null;
        }

        if (sc_old == null) {
          websocketConnections.Add(new SocketConnection {
            Id = guid,
            WebSocket = webSocket,
            dtLastActive = DateTime.Now
          });
        } else {
          sc_old.WebSocket = webSocket;
          sc_old.dtLastActive = DateTime.Now;
        }
      }

      // Store date now (last active) in user club start
      usr.dtClubStart = DateTime.Now;

      if (sc_old == null) {
        ChatMessage chatMsg = new ChatMessage() { date = DateTime.Now, user = usr.sFirstname + " " + usr.sSurname, color = getUserColor(usr.club), message = bForceLogin ? "hat den Chat betreten" : "hat sich eingeloggt" };
        string jsonString = JsonConvert.SerializeObject(chatMsg);
        await SendMessageToSockets(jsonString);
      } else if (sc_old.bInactive) {
        await sc_old.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by user", CancellationToken.None);
        return;
      }

      while (webSocket.State == WebSocketState.Open) {
        var message = await ReceiveMessage(webSocket/*, usr*/);
        if (message != null) {
          await SendMessageToSockets(message);
        }
      }

      if (webSocket.State == WebSocketState.Closed && webSocket.CloseStatus == WebSocketCloseStatus.NormalClosure) {
        SocketConnection sc_inactive = websocketConnections.Find(w => w.Id == guid);
        if (sc_inactive != null) {
          lock (websocketConnections) {
            sc_inactive.bInactive = true;
          }
        }
        ChatMessage chatMsgClose = new ChatMessage() { date = DateTime.Now, user = usr.sFirstname + " " + usr.sSurname, color = getUserColor(usr.club), message = "hat den Chat verlassen ..." };
        string jsonString = JsonConvert.SerializeObject(chatMsgClose);
        await SendMessageToSockets(jsonString, bChatGPT: false);
      }
    }

    public static string getUserColor(CornerkickManager.Club clb)
    {
      string sColor = "0,0,0";
      if (clb == null) return sColor;

      if      (!CornerkickApp.Controllers.Tool.checkColorsSimilar(clb.cl1[0], System.Drawing.Color.White)) sColor = CornerkickApp.Controllers.Tool.getColorRgbString(clb.cl1[0]);
      else if (!CornerkickApp.Controllers.Tool.checkColorsSimilar(clb.cl2[0], System.Drawing.Color.White)) sColor = CornerkickApp.Controllers.Tool.getColorRgbString(clb.cl2[0]);
      else if (!CornerkickApp.Controllers.Tool.checkColorsSimilar(clb.cl1[1], System.Drawing.Color.White)) sColor = CornerkickApp.Controllers.Tool.getColorRgbString(clb.cl1[1]);

      return sColor;
    }

    private async Task<string> ReceiveMessage(WebSocket webSocket/*, CornerkickManager.User usr*/)
    {
      var arraySegment = new ArraySegment<byte>(new byte[4096]);
      var receivedMessage = await webSocket.ReceiveAsync(arraySegment, CancellationToken.None);
      if (receivedMessage.MessageType == WebSocketMessageType.Text) {
        var message = Encoding.Default.GetString(arraySegment).TrimEnd('\0');
        if (!string.IsNullOrWhiteSpace(message)) {
          //return $"<b>{id}</b>: {message}";
          return message;
        }
      } else if (receivedMessage.MessageType == WebSocketMessageType.Close) {
        //sendLogOutMessage(usr);

        await webSocket.CloseAsync(
            receivedMessage.CloseStatus.Value,
            receivedMessage.CloseStatusDescription,
            CancellationToken.None);
      }

      return null;
    }

    private async Task SendMessageToSockets(string message, bool bChatGPT = true, string sOpenAiToken = "")
    {
      IEnumerable<SocketConnection> toSentTo;

      lock (websocketConnections) {
        toSentTo = websocketConnections.ToList();
      }

      ChatMessage chatMsg = JsonConvert.DeserializeObject<ChatMessage>(message);
      if (string.IsNullOrEmpty(chatMsg.message)) return;

      ltChatMessages.Add(chatMsg);

      var tasks = toSentTo.Select(async websocketConnection => {
        if (websocketConnection.WebSocket.State == WebSocketState.Open) {
          var bytes = Encoding.Default.GetBytes(message);
          var arraySegment = new ArraySegment<byte>(bytes);
          await websocketConnection.WebSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
      });
      await Task.WhenAll(tasks);

      if (bChatGPT && !chatMsg.message.Contains("hat das Spiel verlassen") && (chatMsg.message.Contains("chatGPT", System.StringComparison.CurrentCultureIgnoreCase) || rnd.Next(10) == 0)) {
        // First, get token from environment
        /*
        string sOpenAiToken = Environment.GetEnvironmentVariable("ckOpenAiToken");
        if (string.IsNullOrEmpty(sOpenAiToken)) { // If empty, get it from appsettings.json
          OpenAISettings oaiSet = _configuration.GetSection("OpenAIServiceOptions").Get<OpenAISettings>();
          sOpenAiToken = oaiSet.ApiKey;
        }
        */

        if (!string.IsNullOrEmpty(sOpenAiToken)) {
          const string openApiEndpoint = "https://api.openai.com/v1/engines/gpt-3.5-turbo/completions";

          using (HttpClient httpClient = new HttpClient()) {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {sOpenAiToken}");

            //var chatGptRequestContent = new StringContent("{\"prompt\": \"" + chatMsg.message + "\", \"max_tokens\": 50}", Encoding.UTF8, "application/json");
            var chatGptRequestContent =
              new StringContent("{\"messages\": [{\"role\": \"system\", \"content\": \"You are a helpful assistant.\"}, {\"role\": \"user\", \"content\": \"" + chatMsg.message + "\"}]}",
                    Encoding.UTF8, "application/json");
            var chatGptResponse = await httpClient.PostAsync(openApiEndpoint, chatGptRequestContent);

            if (chatGptResponse.IsSuccessStatusCode) {
              string responseContent = await chatGptResponse.Content.ReadAsStringAsync();
              Console.WriteLine(responseContent);
              ChatMessage chatMsgChatGPT = new ChatMessage() { date = DateTime.Now, user = "ChatGPT", color = "0,0,0", message = responseContent };
              SendMessageToSockets(JsonConvert.SerializeObject(chatMsgChatGPT), bChatGPT: false);
            } else if (chatGptResponse.StatusCode == System.Net.HttpStatusCode.TooManyRequests) {
              // Handle rate limiting by waiting and retrying
              ChatMessage chatMsgChatGPT = new ChatMessage() { date = DateTime.Now, user = "ChatGPT", color = "0,0,0", message = "Error: " + chatGptResponse.StatusCode.ToString() + ". Please try again later..." };
              SendMessageToSockets(JsonConvert.SerializeObject(chatMsgChatGPT), bChatGPT: false);

              /*
              // Define your rate limiting parameters
              const int maxRequestsPerMinute = 60; // Adjust this based on OpenAI's rate limits
              const int delayBetweenRequests = 1000 / maxRequestsPerMinute; // Delay in milliseconds

              Thread.Sleep(delayBetweenRequests);
              SendMessageToSockets(message, bChatGPT: true);
              */
            } else {
              ChatMessage chatMsgChatGPT = new ChatMessage() { date = DateTime.Now, user = "ChatGPT", color = "0,0,0", message = "Error: " + chatGptResponse.StatusCode.ToString() };
              SendMessageToSockets(JsonConvert.SerializeObject(chatMsgChatGPT), bChatGPT: false);
            }
          }
          /*
          ChatGPTClient chatgptclient = new ChatGPTClient(sOpenAiToken, "gpt-3.5-turbo");
          //var msg = await chatgptclient.SendMessage("Hello, My name is Sin");
          var msgLast = await chatgptclient.SendMessage(chatMsg.message);
          ChatMessage chatMsgChatGPT = new ChatMessage() { date = DateTime.Now, user = "ChatGPT", color = "0,0,0", message = msgLast.Response };
          string jsonString = JsonConvert.SerializeObject(chatMsgChatGPT);
          */
          /*await *///SendMessageToSockets(jsonString, bChatGPT: false);

          /*
          var sysMsg = "You will review group messages as a group administrator, and I will inform you in the format of {[who][said what]} to reply with a number from 0 to 10 to indicate the severity of political content in their speech, such as \"0\". No need to reply with any other unnecessary content, such as no political content or inability to understand the defense. Please note that group members may be cunning and use pinyin, initials, homophones, abbreviations, etc., to describe things to avoid scrutiny.";
          var msg = await chatgptclient.SendMessage("{[MrWang][Can Trump be president again?]}", sendSystemType: SendSystemType.Custom, sendSystemMessage: sysMsg);
          ChatMessage chatMsg = new ChatMessage() { date = DateTime.Now, user = "ChatGPT", color = "0,0,0", message = msg.Response };
          string jsonString = JsonConvert.SerializeObject(chatMsg);
          await SendMessageToSockets(jsonString);
          */
        }
      }
    }

    private void SetupCleanUpTask()
    {
      Task.Run(async () => {
        while (true) {
          IEnumerable<SocketConnection> openSockets;
          IEnumerable<SocketConnection> closedSockets;

          lock (websocketConnections) {
            openSockets = websocketConnections.Where(x => x.WebSocket.State == WebSocketState.Open || x.WebSocket.State == WebSocketState.Connecting || (DateTime.Now - x.dtLastActive).TotalMinutes < 30);
            //closedSockets = websocketConnections.Where(x => x.WebSocket.State != WebSocketState.Open && x.WebSocket.State != WebSocketState.Connecting);
            closedSockets = websocketConnections.Except(openSockets);

            websocketConnections = openSockets.ToList();
          }

          /*
          foreach (var closedWebsocketConnection in closedSockets) {
            ChatMessage chatMsg = new ChatMessage() { date = DateTime.Now, user = closedWebsocketConnection.Id.ToString(), color = "0,0,0", message = "hat den Chat verlassen. State: " + closedWebsocketConnection.WebSocket.State };
            string jsonString = JsonConvert.SerializeObject(chatMsg);
            await SendMessageToSockets(jsonString);
          }
          */

          await Task.Delay(5000);
        }
      });
    }

    private void sendLogOutMessage(CornerkickManager.User usr)
    {
      Task.Run(async () => {
        string sColor = "0,0,0";
        if (usr.club != null) {
          if      (!CornerkickApp.Controllers.Tool.checkColorsSimilar(usr.club.cl1[0], System.Drawing.Color.White)) sColor = CornerkickApp.Controllers.Tool.getColorRgbString(usr.club.cl1[0]);
          else if (!CornerkickApp.Controllers.Tool.checkColorsSimilar(usr.club.cl2[0], System.Drawing.Color.White)) sColor = CornerkickApp.Controllers.Tool.getColorRgbString(usr.club.cl2[0]);
          else if (!CornerkickApp.Controllers.Tool.checkColorsSimilar(usr.club.cl1[2], System.Drawing.Color.White)) sColor = CornerkickApp.Controllers.Tool.getColorRgbString(usr.club.cl1[2]);
        }
        ChatMessage chatMsg = new ChatMessage() { date = DateTime.Now, user = usr.sFirstname + " " + usr.sSurname, color = sColor, message = "hat sich ausgeloggt..." };
        string jsonString = JsonConvert.SerializeObject(chatMsg);
        await SendMessageToSockets(jsonString);
      });
    }
  }

  public class SocketConnection
  {
    public Guid Id { get; set; }
    public WebSocket WebSocket { get; set; }
    public DateTime dtLastActive { get; set; }
    public bool bInactive { get; set; }
  }
}
