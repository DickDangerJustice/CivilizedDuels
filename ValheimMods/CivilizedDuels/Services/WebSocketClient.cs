using LitJson;
using UnityEngine;
using WebSocketSharp;

namespace CivilizedDuels.Services {
    public class Message
    {
        public string type { get; set; }
        public bool isWhite { get; set; }
        public string gameId { get; set; }
        public string state { get; set; }
    }

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
                var message = JsonMapper.ToObject<Message>(e.Data);
                switch (message.type)
                {
                    case "gameOver":
                        switch (message.state)
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
                        break;
                }
            };

            ws.OnError += (sender, e) =>
            {
                Debug.Log($"Error: {e.Message}");
                if (!ws.IsAlive)
                {
                    ws.Connect();
                }
            };

            ws.OnClose += (sender, e) =>
                Debug.Log("WS disconnected!");

            ws.Connect();
        }

        void OnDestroy()
        {
            ws.Close();
        }

        private void Update()
        {
            //if (ws == null || !ws.IsAlive)
            //{
            //    return;
            //}

            // keep heroku websocket alive
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                var message = new Message
                {
                    type = "ping"
                };
                ws.Send(JsonMapper.ToJson(message));
                timer = pingInterval;
            }
        }

        public void Send(string message)
        {
            ws.Send(message);
        }
    }
}