window.IntentumECharts = {
  instances: {},
  init: function (elementId) {
    if (this.instances[elementId]) return true;
    var el = document.getElementById(elementId);
    if (!el) return false;
    if (typeof echarts === 'undefined') {
      console.warn('ECharts not loaded. Add script src="https://cdn.jsdelivr.net/npm/echarts@5/dist/echarts.min.js"');
      return false;
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
    if (chart && option) {
      if (option.tooltip && typeof option.tooltip === 'object' && !option.tooltip.confine) {
        option.tooltip.confine = true;
      }
      chart.setOption(option);
    } else if (chart) chart.setOption(option || {});
  },
  setHeatmapOption: function (elementId, option) {
    var chart = this.instances[elementId];
    if (!chart) {
      this.init(elementId);
      chart = this.instances[elementId];
    }
    if (!chart || !option) return;
    if (option.series && option.series[0] && option.series[0].type === 'heatmap') {
      var yData = (option.yAxis && option.yAxis.data) || [];
      var xData = (option.xAxis && option.xAxis.data) || [];
      option.series[0].label = option.series[0].label || {};
      option.series[0].label.formatter = function (params) {
        var v = params.value;
        var num = Array.isArray(v) ? v[2] : v;
        return (Math.round((num || 0) * 100)) + '%';
      };
      if (!option.tooltip) option.tooltip = {};
      option.tooltip.confine = true;
      option.tooltip.formatter = function (params) {
        var p = params && params[0];
        if (!p || !p.data) return '';
        var v = p.data;
        var arr = Array.isArray(v) ? v : [v];
        var xi = arr[0], yi = arr[1], score = arr[2];
        var yLabel = yData[yi] != null ? yData[yi] : yi;
        var xLabel = xData[xi] != null ? xData[xi] : xi;
        var pct = (Math.round((score || 0) * 100));
        return 'Niyet: ' + yLabel + '<br/>Zaman: ' + xLabel + '<br/>Güven: ' + pct + '%';
      };
    }
    chart.setOption(option);
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
