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
  if (!(message.gameId in valheimConnections)) valheimConnections[message.gameId] = {}
  valheimConnections[message.gameId][message.isWhite] = ws
  console.log(valheimConnections[message.gameId])
  websocketGameMap.set(ws, [message.gameId, message.isWhite])
}

function joinGame(message, ws) {
  // create entry for game if it doesn't exist
  if (!(message.gameId in webConnections)) webConnections[message.gameId] = {}

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
  if (message.gameId in valheimConnections) {
    if (valheimConnections[message.gameId][message.isWhite]) {
      valheimConnections[message.gameId][message.isWhite].send(JSON.stringify({
        type: "gameOver",
        state: "lose"
      }))
    }
    if (valheimConnections[message.gameId][!message.isWhite]) {
      valheimConnections[message.gameId][!message.isWhite].send(JSON.stringify({
        type: "gameOver",
        state: "win"
      }))
    }
  }
  
  webConnections[message.gameId][!message.isWhite].send(JSON.stringify({
    type: "opponentResigned"
  }))
}

function gameOver(message) {
  console.log(`Game over: ${message.gameId}, ${message.isWhite}`)

  if (message.gameId in valheimConnections) {
    if (valheimConnections[message.gameId][message.isWhite]) {
      valheimConnections[message.gameId][message.isWhite].send(JSON.stringify({
        type: "gameOver",
        state: message.state
      }))
      delete valheimConnections[message.gameId][message.isWhite]
    }
  }
  
  webConnections[message.gameId][message.isWhite].close()
  delete webConnections[message.gameId][message.isWhite]
}

function clientDisconnect(ws) {
  console.log("Client disconnected")
  if (websocketGameMap.has(ws)) {
    const connection = websocketGameMap.get(ws)
    if (connection.gameId in webConnections) {
      for (const webConnection in webConnections) {
        webConnections[connection.gameId][webConnection].close()
        delete webConnections[connection.gameId][webConnection]
      }
    }
    if (connection.gameId in valheimConnections) {
      for (const valheimConnection in valheimConnections[connection.gameId]) {
        valheimConnections[connection.gameId][valheimConnection].send(JSON.stringify({
          type: "gameOver",
          state: "draw"
        }))
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
