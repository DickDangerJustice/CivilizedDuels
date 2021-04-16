using UnityEngine;
using WebSocketSharp;

namespace CivilizedDuels.Services {
    public class WebSocketClient : MonoBehaviour
    {
        WebSocket ws;

        public void Start()
        {
            ws = new WebSocket("ws://localhost:3000/");

            ws.OnOpen += (sender, e) =>
                Debug.Log("WS connected!");

            ws.OnMessage += (sender, e) =>
            {
                Chat.instance.SendText(Talker.Type.Shout, e.Data);
            };
            
            ws.OnError += (sender, e) =>
                Debug.Log($"Error: {e.Message}");

            ws.OnClose += (sender, e) =>
                Debug.Log("WS disconnected!");

            ws.Connect();
        }

        private void Update()
        {
            if (ws == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ws.Send("Hello");
            }
        }
    }
}