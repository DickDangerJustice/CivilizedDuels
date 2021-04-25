'use strict';

const express = require('express');
const { Server } = require('ws');

const PORT = process.env.PORT || 3000;
const INDEX = '/index.html';

const server = express()
  .use((req, res) => res.sendFile(INDEX, { root: __dirname }))
  .listen(PORT, () => console.log(`Listening on ${PORT}`));

const wss = new Server({ server });

wss.on('connection', (ws) => {
  console.log('Client connected');

  ws.on('message', (data) => {
    // console.log(data)
    console.log('data received \n %o',data)

    const message = JSON.parse(data)
    switch(message.type) {
      case "connectValheim":
        connectValheim(message, ws)
        break;
      case "joinGame":
        joinGame(message, ws)
        break;
      case "move":
        move(message)
        break;
      case "resign":
        resign(message)
        break;
      case "gameOver":
        gameOver(message)
        break;
    }
 })

  ws.on('close', () => { clientDisconnect(ws) });
});

function connectValheim(message, ws) {
  if (!valheimConnections[message.gameId]) valheimConnections[message.gameId] = {}
  valheimConnections[message.gameId][message.isWhite] = ws
  websocketGameMap.set(ws, [message.gameId, message.isWhite])
}

function joinGame(message, ws) {
  // create entry for game if it doesn't exist
  if (!webConnections[message.gameId]) webConnections[message.gameId] = {}

  // log connection for player
  webConnections[message.gameId][message.isWhite] = ws

  // if both players have joined, start the game for each
  if (webConnections[message.gameId][!message.isWhite]) {
    ws.send(JSON.stringify({type: "startGame"}))
    webConnections[message.gameId][!message.isWhite].send(JSON.stringify({type: "startGame"}))
  }
}

function move(message) {
  webConnections[message.gameId][!message.isWhite].send(JSON.stringify({
    type: "updateBoard",
    move: message.move
  }))
}

function resign(message) {
  console.log("test")
  valheimConnections[message.gameId]?.[message.isWhite]?.send(JSON.stringify({
    type: "gameOver",
    state: "lose"
  }))
  valheimConnections[message.gameId]?.[!message.isWhite]?.send(JSON.stringify({
    type: "gameOver",
    state: "win"
  }))
  webConnections[message.gameId][!message.isWhite].send(JSON.stringify({
    type: "opponentResigned"
  }))
}

function gameOver(message) {
  console.log(`Game over: ${message.gameId}, ${message.isWhite}`)
  
  let sendState;
  switch (message.state) {
    case "win": 
      sendState = "lose"
      break;
    case "lose":
      sendState = "win"
      break;
    case "draw":
      sendState = "draw"
      break;
  }

  if (valheimConnections[message.gameId]?.[message.isWhite] != null) {
    valheimConnections[message.gameId][message.isWhite].send(JSON.stringify({
      type: "gameOver",
      state: sendState
    }))
    valheimConnections[message.gameId][message.isWhite].close()
    valheimConnections[message.gameId][message.isWhite] = null
  }
  
  webConnections[message.gameId][message.isWhite].close()
  webConnections[message.gameId][message.isWhite] = null
}

function clientDisconnect(ws) {
  console.log("Client disconnected")
  if (websocketGameMap.has(ws)) {
    const connection = websocketGameMap.get(ws)
    if (connection.gameId in webConnections) {
      if (webConnections[connection.gameId][connection.isWhite]) {
        webConnections[connection.gameId][connection.isWhite].close()
        webConnections[connection.gameId][connection.isWhite] = null
      }
    }
  }
}

const webConnections = {}
const valheimConnections = {}
const websocketGameMap = new Map()


// setInterval(() => {
//   wss.clients.forEach((client) => {
//     client.send(new Date().toTimeString());
//   });
// }, 1000);
