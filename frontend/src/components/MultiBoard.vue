// newboard.vue
<script>
import { chessboard } from "vue-chessboard";
import "vue-chessboard/dist/vue-chessboard.css";

export default {
  name: "newboard",
  extends: chessboard,
  props: ["gameId"],
  data: function () {
    return {
      isTurn: false,
      connection: null,
    };
  },
  methods: {
    userPlay() {
      return (orig, dest) => {
        if (this.isPromotion(orig, dest)) {
          this.promoteTo = this.onPromotion();
        }
        this.game.move({ from: orig, to: dest, promotion: this.promoteTo }); // promote to queen for simplicity
        this.board.set({
          fen: this.game.fen(),
        });
        this.calculatePromotions();
        this.sendUpdate(this.game.fen());
        this.aiNextMove();
      };
    },
    aiNextMove() {
      let moves = this.game.moves({ verbose: true });
      let randomMove = moves[Math.floor(Math.random() * moves.length)];
      this.game.move(randomMove);

      this.board.set({
        fen: this.game.fen(),
        turnColor: this.toColor(),
        movable: {
          color: this.toColor(),
          dests: this.possibleMoves(),
          events: { after: this.userPlay() },
        },
      });
    },
    sendUpdate(fen) {
      console.log(this.connection);
      this.connection.send(
        JSON.stringify({
          gameId: this.gameId,
          fen: fen,
        })
      );
    },
  },
  mounted() {
    this.board.set({
      movable: { events: { after: this.userPlay() } },
    });
  },
  created() {
    console.log("Starting connection to WebSocket Server");
    this.connection = new WebSocket("ws://localhost:3000");

    this.connection.onmessage = function (event) {
      console.log(event);
    };

    this.connection.onopen = function (event) {
      console.log(event);
      console.log("Successfully connected to the echo websocket server...");
    };
  },
};
</script>