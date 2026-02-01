window.IntentumECharts = {
  instances: {},
  init: function (elementId) {
    if (this.instances[elementId]) return;
    var el = document.getElementById(elementId);
    if (!el) return;
    if (typeof echarts === 'undefined') {
      console.warn('ECharts not loaded. Add script src="https://cdn.jsdelivr.net/npm/echarts@5/dist/echarts.min.js"');
      return null;
    }
    var chart = echarts.init(el);
    this.instances[elementId] = chart;
    return true;
  },
  setOption: function (elementId, option) {
    var chart = this.instances[elementId];
    if (!chart) {
      this.init(elementId);
      chart = this.instances[elementId];
    }
    if (chart) chart.setOption(option || {});
  },
  resize: function (elementId) {
    var chart = this.instances[elementId];
    if (chart) chart.resize();
  },
  dispose: function (elementId) {
    var chart = this.instances[elementId];
    if (chart) {
      chart.dispose();
      delete this.instances[elementId];
    }
  }
};
