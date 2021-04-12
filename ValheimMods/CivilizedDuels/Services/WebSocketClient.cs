using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSocketSharp;

// Use plugin namespace
//using HybridWebSocket;

namespace CivilizedDuels.Services {
    public class WebSocketClient
    {
        public static void Test()
        {
            using (var ws = new WebSocket("ws://civilized-duels.herokuapp.com/"))
            {
                ws.OnOpen += (sender, e) => 
                    Debug.Log("WS connected!");

                ws.OnMessage += (sender, e) =>
                    Debug.Log("ECHO: " + e.Data);

                ws.Connect();
                //ws.Send("BALUS");
            }
        }
    }

    //public class WebSocketDemo : MonoBehaviour
    //{

    //    // Use this for initialization
    //    void Start()
    //    {

    //        // Create WebSocket instance
    //        WebSocket ws = WebSocketFactory.CreateInstance("ws://localhost:9000/echo");

    //        // Add OnOpen event listener
    //        ws.OnOpen += () =>
    //        {
    //            Debug.Log("WS connected!");
    //            Debug.Log("WS state: " + ws.GetState().ToString());

    //            ws.Send(Encoding.UTF8.GetBytes("Hello from Unity 3D!"));
    //        };

    //        // Add OnMessage event listener
    //        ws.OnMessage += (byte[] msg) =>
    //        {
    //            Debug.Log("WS received message: " + Encoding.UTF8.GetString(msg));

    //            ws.Close();
    //        };

    //        // Add OnError event listener
    //        ws.OnError += (string errMsg) =>
    //        {
    //            Debug.Log("WS error: " + errMsg);
    //        };

    //        // Add OnClose event listener
    //        ws.OnClose += (WebSocketCloseCode code) =>
    //        {
    //            Debug.Log("WS closed with code: " + code.ToString());
    //        };

    //        // Connect to the server
    //        ws.Connect();

    //    }

    //    // Update is called once per frame
    //    void Update()
    //    {

    //    }
    //}
}