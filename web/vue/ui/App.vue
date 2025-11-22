<template>
    <el-tabs v-model="activeName" @tab-click="handleClick">
      <el-tab-pane label="Layers" name="1st">
        <opt-layers />
      </el-tab-pane>
      <el-tab-pane label="Style" name="2nd">Style options</el-tab-pane>
      <el-tab-pane label="Options" name="3rd">
        <opt-options />
      </el-tab-pane>
      <el-tab-pane label="Tools" name="4th">Visual tools</el-tab-pane>
      <el-tab-pane label="About" name="5ve">Coming soon</el-tab-pane>
    </el-tabs>
</template>

<script>
import { Tabs, TabPane } from "element-ui";
import OptLayers from "./components/opt-layers";
import OptOptions from "./components/opt-options";

const components = {
  "el-tabs": Tabs,
  "el-tab-pane": TabPane,
  OptLayers,
  OptOptions,
};

export default {
  components,
  data() {
    return {
      activeName: "3rd",
    };
  },
  methods: {
    handleClick(tab, event) {
      // console.log(tab, event);
    },
  },
};
</script>

<style scoped>
</style>
