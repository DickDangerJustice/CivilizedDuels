<template>
  <div>
    <!-- <input type="text" v-model="gameId" placeholder="Game Id" />
    <input type="checkbox" v-model="isWhite" />
    <button @click="startGame">Start Game</button> -->
    <multi-board
      :gameId="gameId"
      :isWhite="isWhite"
      :orientation="isWhite ? 'white' : 'black'"
    ></multi-board>
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
    };
  },
  methods: {
    startGame() {
      this.gameStarted = true;
    },
  },
  mounted() {
    console.log(`Game id: ${this.gameId}`);
    console.log(`Player color: ${this.isWhite ? "white" : "black"}`);
  },
};
</script>

<style></style>
