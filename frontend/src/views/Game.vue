<template>
  <div>
    <!-- <input type="text" v-model="gameId" placeholder="Game Id" />
    <input type="checkbox" v-model="isWhite" />
    <button @click="startGame">Start Game</button> -->
    <multi-board
      v-if="!isGameOver"
      class="game"
      :gameId="gameId"
      :isWhite="isWhite"
      :orientation="isWhite ? 'white' : 'black'"
      :resigned="resigned"
      @gameOver="handleGameOver"
    ></multi-board>
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
      gameStarted: false,
      resigned: false,
      isGameOver: false,
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
  },
  mounted() {
    console.log(`Game id: ${this.gameId}`);
    console.log(`Player color: ${this.isWhite ? "white" : "black"}`);
  },
};
</script>

<style>
.game {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 5vh 0;
}
</style>
