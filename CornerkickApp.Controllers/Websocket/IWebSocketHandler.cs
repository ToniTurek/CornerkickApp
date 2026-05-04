using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebsocketChat.Websocket
{
  public interface IWebsocketHandler
  {
    Task Handle(CornerkickManager.User usr, WebSocket websocket, bool bForceLogin = false);
  }
}
