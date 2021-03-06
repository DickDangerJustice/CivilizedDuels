"use strict";

const express = require("express");
const { Server } = require("ws");

const PORT = process.env.PORT || 3000;
const INDEX = "/index.html";

const server = express()
  .use((req, res) => res.sendFile(INDEX, { root: __dirname }))
  .listen(PORT, () => console.log(`Listening on ${PORT}`));

const wss = new Server({ server });

wss.on("connection", (ws) => {
  console.log("Client connected");

  ws.on("message", (data) => {
    // console.log(data)
    console.log("data received \n %o", data);

    try {
      const message = JSON.parse(data);
      switch (message.type) {
        case "connectValheim":
          connectValheim(message, ws);
          break;
        case "joinGame":
          joinGame(message, ws);
          break;
        case "move":
          move(message);
          break;
        case "resign":
          resign(message);
          break;
        case "forceQuit":
          forceQuit(ws);
          break;
        case "gameOver":
          gameOver(message);
          break;
      }
    } catch {
      console.log("Failed to handle message!");
    }
  });

  ws.on("close", () => {
    clientDisconnect(ws);
  });
});

function connectValheim(message, ws) {
  if (!(message.gameId in valheimConnections))
    valheimConnections[message.gameId] = {};
  valheimConnections[message.gameId][message.isWhite] = ws;
  console.log(valheimConnections[message.gameId]);
  webSocketGameMap.set(ws, {
    gameId: message.gameId,
    isWhite: message.isWhite,
  });
}

function joinGame(message, ws) {
  // create entry for game if it doesn't exist
  if (!(message.gameId in webConnections)) webConnections[message.gameId] = {};

  // log connection for player
  webConnections[message.gameId][message.isWhite] = ws;
  webSocketGameMap.set(ws, {
    gameId: message.gameId,
    isWhite: message.isWhite,
  });

  // if both players have joined, start the game for each
  if (webConnections[message.gameId][!message.isWhite]) {
    ws.send(JSON.stringify({ type: "startGame" }));
    webConnections[message.gameId][!message.isWhite].send(
      JSON.stringify({ type: "startGame" })
    );
  }
}

function move(message) {
  webConnections[message.gameId][!message.isWhite].send(
    JSON.stringify({
      type: "updateBoard",
      move: message.move,
      time: message.time,
    })
  );
}

function forceQuit(ws) {
  if (webSocketGameMap.has(ws)) {
    const connection = webSocketGameMap.get(ws);

    if (connection.gameId in webConnections) {
      if (webConnections[connection.gameId][!connection.isWhite]) {
        webConnections[connection.gameId][!connection.isWhite].send(
          JSON.stringify({
            type: "opponentResigned",
          })
        );
      }
      if (webConnections[connection.gameId][connection.isWhite]) {
        webConnections[connection.gameId][connection.isWhite].send(
          JSON.stringify({
            type: "opponentResigned",
          })
        );
      }
    }
    deleteWebConnections(connection.gameId);

    if (connection.gameId in valheimConnections) {
      for (const valheimConnection in valheimConnections[connection.gameId]) {
        valheimConnections[connection.gameId][valheimConnection].send(
          JSON.stringify({
            type: "gameOver",
            state: "draw",
          })
        );
        webSocketGameMap.delete(valheimConnection);
      }
    }
  }
}

function resign(message) {
  if (message.gameId in valheimConnections) {
    if (valheimConnections[message.gameId][message.isWhite]) {
      valheimConnections[message.gameId][message.isWhite].send(
        JSON.stringify({
          type: "gameOver",
          state: "lose",
        })
      );
    }
    if (valheimConnections[message.gameId][!message.isWhite]) {
      valheimConnections[message.gameId][!message.isWhite].send(
        JSON.stringify({
          type: "gameOver",
          state: "win",
        })
      );
    }
  }

  webConnections[message.gameId][!message.isWhite].send(
    JSON.stringify({
      type: "opponentResigned",
    })
  );

  deleteWebConnections(message.gameId);
}

function deleteWebConnections(gameId) {
  if (gameId in webConnections) {
    for (const webConnection in webConnections[gameId]) {
      webConnections[gameId][webConnection].close();
      delete webConnections[gameId][webConnection];
      webSocketGameMap.delete(webConnection);
    }
  }
}

function gameOver(message) {
  console.log(`Game over: ${message.gameId}, ${message.isWhite}`);

  if (message.gameId in valheimConnections) {
    if (valheimConnections[message.gameId][message.isWhite]) {
      valheimConnections[message.gameId][message.isWhite].send(
        JSON.stringify({
          type: "gameOver",
          state: message.state,
        })
      );
      delete valheimConnections[message.gameId][message.isWhite];
    }
  }

  webConnections[message.gameId][message.isWhite].close();
  delete webConnections[message.gameId][message.isWhite];
}

function clientDisconnect(ws) {
  console.log("Client disconnected");
  if (webSocketGameMap.has(ws)) {
    console.log("Handling disconnect");
    const connection = webSocketGameMap.get(ws);

    if (connection.gameId in webConnections) {
      if (webConnections[connection.gameId][!connection.isWhite]) {
        webConnections[connection.gameId][!connection.isWhite].send(
          JSON.stringify({
            type: "opponentResigned",
          })
        );
      }
    }
    deleteWebConnections(connection.gameId);

    if (connection.gameId in valheimConnections) {
      for (const valheimConnection in valheimConnections[connection.gameId]) {
        valheimConnections[connection.gameId][valheimConnection].send(
          JSON.stringify({
            type: "gameOver",
            state: "draw",
          })
        );
        webSocketGameMap.delete(valheimConnection);
      }
    }
  }
}

const webConnections = {};
const valheimConnections = {};
const webSocketGameMap = new Map();

// setInterval(() => {
//   wss.clients.forEach((client) => {
//     client.send(new Date().toTimeString());
//   });
// }, 1000);
