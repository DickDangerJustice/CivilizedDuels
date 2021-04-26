using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CivilizedDuels.Services {
    public class WebSocketClient : MonoBehaviour
    {
        WebSocket ws;
        private float timer;
        private float pingInterval = 30;


        public void Start()
        {
            ws = new WebSocket("wss://civilized-duels.herokuapp.com//");

            ws.OnOpen += (sender, e) =>
                Debug.Log("WS connected!");

            //ws.OnMessage += (sender, e) =>
            //{
            //    Chat.instance.SendText(Talker.Type.Shout, e.Data);
            //};

            ws.OnMessage += (sender, e) =>
            {
                dynamic d = JObject.Parse(e.Data);
                switch (d.type)
                {
                    case "gameOver":
                        switch (d.state)
                        {
                            case "win":
                                Chat.instance.SendText(Talker.Type.Shout, "I WIN");
                                break;
                            case "lose":
                                Chat.instance.SendText(Talker.Type.Shout, "I LOSE");
                                break;
                            case "draw":
                                Chat.instance.SendText(Talker.Type.Shout, "DRAW");
                                break;
                        }
                        ws.Close();
                        ws = null;
                        break;
                }
            };

            ws.OnError += (sender, e) =>
                Debug.Log($"Error: {e.Message}");

            ws.OnClose += (sender, e) =>
                Debug.Log("WS disconnected!");
        }

        void OnDestroy()
        {
            ws.Close();
        }

        private void Update()
        {
            if (ws == null || !ws.IsAlive)
            {
                return;
            }

            // keep heroku websocket alive
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Mod.WebSocketObject.GetComponent<WebSocketClient>().Send(ZDOMan.instance != null ? "Ping from " + ZDOMan.instance.GetMyID() : "Ping");
                timer = pingInterval;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ws.Send("Hello");
            }
        }

        public void Send(string message)
        {
            ws.Send(message);
        }
    }
}