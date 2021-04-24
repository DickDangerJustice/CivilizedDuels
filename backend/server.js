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
    console.log(data)
    console.log('data received \n %o',data)

    const message = JSON.parse(data)
    switch(message.type) {
      case "joinGame":
        joinGame(message, ws)
        break;
      case "move":
        move(message)
        break;
    }
 })

  ws.on('close', () => {console.log('Client disconnected')});
});

function joinGame(message, ws) {
  // create entry for game if it doesn't exist
  if (!connections[message.gameId]) connections[message.gameId] = {}

  // log connection for player
  connections[message.gameId][message.isWhite] = ws

  // if both players have joined, start the game for each
  if (connections[message.gameId][!message.isWhite]) {
    ws.send(JSON.stringify({type: "startGame"}))
    connections[message.gameId][!message.isWhite].send(JSON.stringify({type: "startGame"}))
  }
}

function move(message) {
  connections[message.gameId][!message.isWhite].send(JSON.stringify({
    type: "updateBoard",
    move: message.move
  }))
}

const connections = {}


// setInterval(() => {
//   wss.clients.forEach((client) => {
//     client.send(new Date().toTimeString());
//   });
// }, 1000);
