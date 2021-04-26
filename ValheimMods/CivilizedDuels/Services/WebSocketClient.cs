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
                        //Player.m_localPlayer.m_seman.RemoveStatusEffect(Mod.StatusEffects["Challenged"]);
                        //Player.m_localPlayer.m_teleporting = false;
                        switch (message.state)
                        {
                            case "win":
                                break;
                            case "lose":
                                var hitData = new HitData();
                                hitData.m_damage.m_damage = 99999f;
                                Player.m_localPlayer.Damage(hitData);
                                break;
                            case "draw":
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