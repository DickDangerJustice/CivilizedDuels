<template>
  <div>
    <input type="text" v-model="gameId" placeholder="Game Id" />
    <input type="checkbox" v-model="isWhite" />
    <button @click="startGame">Start Game</button>
    <multi-board
      v-if="gameStarted"
      :gameId="gameId"
      :isWhite="isWhite"
      :orientation="isWhite ? 'white' : 'black'"
    ></multi-board>
  </div>
</template>

<script>
import MultiBoard from "./MultiBoard.vue";

export default {
  components: {
    MultiBoard,
  },
  data() {
    return {
      gameId: 1,
      isWhite: true,
      gameStarted: false,
    };
  },
  methods: {
    startGame() {
      this.gameStarted = true;
    },
  },
};
</script>

<style></style>
