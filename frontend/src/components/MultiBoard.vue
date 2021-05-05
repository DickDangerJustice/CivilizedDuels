// newboard.vue
<script>
import { chessboard } from "vue-chessboard";
import "vue-chessboard/dist/vue-chessboard.css";

export default {
  name: "newboard",
  extends: chessboard,
  props: ["gameId", "isWhite", "resigned", "startingTime"],
  watch: {
    resigned: function (val) {
      if (val) {
        this.resign();
      }
    },
    time: function (val) {
      this.$emit("timeChanged", val);
    },
    opponentTime: function (val) {
      this.$emit("opponentTimeChanged", val);
    },
  },
  data: function () {
    return {
      connection: null,
      gameState: null,
      timer: null,
      opponentTimer: null,
      ping: null,
      time: 0,
      opponentTime: 0,
    };
  },
  methods: {
    userPlay() {
      return (orig, dest) => {
        if (this.isPromotion(orig, dest)) {
          this.promoteTo = this.onPromotion();
        }
        let move = { from: orig, to: dest, promotion: this.promoteTo };
        this.game.move(move); // promote to queen for simplicity
        this.board.set({
          fen: this.game.fen(),
        });
        this.calculatePromotions();
        if (this.game.game_over()) {
          if (this.game.in_checkmate()) {
            console.log("You win!");
            this.sendGameOver("win");
          } else {
            console.log("Draw!");
            this.sendGameOver("draw");
          }
          this.gameOver();
        }
        this.sendUpdate(move);
      };
    },
    sendUpdate(move) {
      console.log(this.connection);
      clearInterval(this.timer);
      this.opponentTimer = setInterval(() => (this.opponentTime -= 1), 1000);
      this.connection.send(
        JSON.stringify({
          type: "move",
          gameId: this.gameId,
          isWhite: this.isWhite,
          move: move,
          time: this.time,
        })
      );
    },
    sendGameOver(state) {
      this.connection.send(
        JSON.stringify({
          type: "gameOver",
          state: state,
          gameId: this.gameId,
          isWhite: this.isWhite,
        })
      );
    },
    resign() {
      this.connection.send(
        JSON.stringify({
          type: "resign",
          gameId: this.gameId,
          isWhite: this.isWhite,
        })
      );
      console.log("You lose!");
      this.gameOver();
    },
    handleTimer() {
      this.time -= 1;
      if (this.time <= 0) {
        this.resign();
        clearInterval(this.timer);
      }
    },
    gameOver() {
      clearInterval(this.ping);
      this.$emit("gameOver");
    },
  },
  mounted() {
    this.board.set({
      movable: { events: { after: this.userPlay() } },
      viewOnly: true,
    });
  },
  created() {
    this.time = this.startingTime;
    this.opponentTime = this.startingTime;
    console.log("Starting connection to WebSocket Server");
    this.connection = new WebSocket("wss://civilized-duels.herokuapp.com");

    this.connection.onmessage = (event) => {
      let message = JSON.parse(event.data);
      console.log(message);

      switch (message.type) {
        case "startGame":
          this.board.set({
            viewOnly: false,
          });
          if (this.isWhite) {
            this.timer = setInterval(() => this.handleTimer(), 1000);
          } else {
            this.opponentTimer = setInterval(
              () => (this.opponentTime -= 1),
              1000
            );
          }
          break;
        case "updateBoard":
          console.log(message.move);
          this.game.move(message.move);

          this.board.set({
            fen: this.game.fen(),
            turnColor: this.toColor(),
            movable: {
              color: this.toColor(),
              dests: this.possibleMoves(),
              events: { after: this.userPlay() },
            },
          });

          if (this.game.game_over()) {
            if (this.game.in_checkmate()) {
              console.log("You lose!");
              this.sendGameOver("lose");
              this.gameOver();
            } else {
              console.log("Draw!");
              this.sendGameOver("draw");
              this.gameOver();
            }
          } else {
            this.opponentTime = message.time;
            clearInterval(this.opponentTimer);
            this.timer = setInterval(() => this.handleTimer(), 1000);
          }
          break;
        case "opponentResigned":
          console.log("You win!");
          this.gameOver();
      }
    };

    this.connection.onopen = (event) => {
      console.log(event);
      this.connection.send(
        JSON.stringify({
          type: "joinGame",
          gameId: this.gameId,
          isWhite: this.isWhite,
        })
      );

      this.ping = setInterval(() => {
        this.connection.send(JSON.stringify({ type: "ping" }));
      }, 30000);

      console.log("Successfully connected to the echo websocket server...");
    };
  },
};
</script>