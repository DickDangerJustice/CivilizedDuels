<template>
  <div>
    <!-- <input type="text" v-model="gameId" placeholder="Game Id" />
    <input type="checkbox" v-model="isWhite" />
    <button @click="startGame">Start Game</button> -->
    <div v-if="!isGameOver">
      <multi-board
        class="game"
        :gameId="gameId"
        :isWhite="isWhite"
        :orientation="isWhite ? 'white' : 'black'"
        :resigned="resigned"
        :startingTime="time"
        @gameOver="handleGameOver"
        @timeChanged="updateTimer"
        @opponentTimeChanged="updateOpponentTimer"
      ></multi-board>
      <h5>Time remaining: {{ time | timecode }}</h5>
      <h5>Opponent Time remaining: {{ opponentTime | timecode }}</h5>
    </div>

    <h1 v-else>Game over!</h1>
    <button v-if="!isGameOver" @click="resign">Resign</button>
  </div>
</template>

<script>
import MultiBoard from "@/components/MultiBoard.vue";

export default {
  props: ["gameId", "isWhite"],
  components: {
    MultiBoard,
  },
  data() {
    return {
      resigned: false,
      isGameOver: false,
      time: 180,
      opponentTime: 180,
    };
  },
  methods: {
    startGame() {
      this.gameStarted = true;
    },
    resign() {
      this.resigned = true;
    },
    handleGameOver() {
      this.isGameOver = true;
    },
    updateTimer(time) {
      this.time = time;
    },
    updateOpponentTimer(time) {
      this.opponentTime = time;
    },
  },
  mounted() {
    console.log(`Game id: ${this.gameId}`);
    console.log(`Player color: ${this.isWhite ? "white" : "black"}`);
  },
  filters: {
    timecode(value) {
      var seconds = Math.floor(value % 60).toString();
      var minutes = Math.floor(value / 60).toString();
      if (seconds.length === 1) {
        seconds = "0" + seconds;
      }
      return minutes + ":" + seconds;
    },
  },
};
</script>

<style>
.game {
  display: flex;
  align-items: center;
  justify-content: center;
  padding-top: 5vh;
}
</style>
