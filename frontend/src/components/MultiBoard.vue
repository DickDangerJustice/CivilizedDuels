// newboard.vue
<script>
import { chessboard } from "vue-chessboard";
import "vue-chessboard/dist/vue-chessboard.css";

export default {
  name: "newboard",
  extends: chessboard,
  props: ["gameId", "isWhite", "resigned"],
  watch: {
    resigned: function (val) {
      if (val) {
        this.connection.send(
          JSON.stringify({
            type: "resign",
            gameId: this.gameId,
            isWhite: this.isWhite,
          })
        );
        console.log("You lose!");
        this.sendGameOver("lose");
        this.$emit("gameOver");
      }
    },
  },
  data: function () {
    return {
      connection: null,
      gameState: null,
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
            this.$emit("gameOver");
          } else {
            console.log("Draw!");
            this.sendGameOver("draw");
          }
        }
        this.sendUpdate(move);
      };
    },
    sendUpdate(move) {
      console.log(this.connection);
      this.connection.send(
        JSON.stringify({
          type: "move",
          gameId: this.gameId,
          isWhite: this.isWhite,
          move: move,
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
  },
  mounted() {
    this.board.set({
      movable: { events: { after: this.userPlay() } },
      viewOnly: true,
    });
  },
  created() {
    console.log("Starting connection to WebSocket Server");
    this.connection = new WebSocket("wss://civilized-duels.herokuapp.com/");

    this.connection.onmessage = (event) => {
      let message = JSON.parse(event.data);
      console.log(message);

      switch (message.type) {
        case "startGame":
          this.board.set({
            viewOnly: false,
          });
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
              this.$emit("gameOver");
            } else {
              console.log("Draw!");
              this.sendGameOver("draw");
              this.$emit("gameOver");
            }
          }
          break;
        case "opponentResigned":
          console.log("You win!");
          this.$emit("gameOver");
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

      setInterval(() => {
        this.connection.send(JSON.stringify({ type: "ping" }));
      }, 30000);

      console.log("Successfully connected to the echo websocket server...");
    };
  },
};
</script>