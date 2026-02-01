window.IntentumECharts = {
  instances: {},
  _worldRegistered: false,
  initGeoMap: async function (elementId) {
    if (typeof echarts === 'undefined') {
      console.warn('ECharts not loaded');
      return false;
    }
    if (!this._worldRegistered) {
      var base = (typeof window !== 'undefined' && window.location && window.location.origin) ? window.location.origin : '';
      var urls = [
        base + '/world.json',
        'https://echarts.apache.org/examples/data/asset/geo/world.json',
        'https://fastly.jsdelivr.net/npm/echarts@5/map/json/world.json',
        'https://cdn.jsdelivr.net/npm/echarts@5.4.3/map/json/world.json'
      ];
      var loaded = false;
      for (var i = 0; i < urls.length; i++) {
        try {
          var res = await fetch(urls[i]);
          if (!res.ok) continue;
          var world = await res.json();
          echarts.registerMap('world', world);
          this._worldRegistered = true;
          loaded = true;
          break;
        } catch (e) {
          /* try next URL */
        }
      }
      if (!loaded) {
        console.warn('World map load failed for all URLs');
        return false;
      }
    }
    var el = document.getElementById(elementId);
    if (!el) return false;
    var existing = this.instances[elementId];
    if (existing) {
      try {
        var chartDom = existing.getDom ? existing.getDom() : (existing.getZr && existing.getZr() ? existing.getZr().dom : null);
        var orphan = !chartDom || !document.body.contains(chartDom);
        if (orphan || chartDom !== el) {
          existing.dispose();
          delete this.instances[elementId];
        }
      } catch (e) {
        existing.dispose();
        delete this.instances[elementId];
      }
    }
    if (!this.instances[elementId]) {
      this.instances[elementId] = echarts.init(el);
    }
    var chart = this.instances[elementId];
    if (chart) {
      setTimeout(function () { chart.resize(); }, 50);
    }
    return true;
  },
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
window.initIntentumGeoMap = function (elementId) {
  return window.IntentumECharts && window.IntentumECharts.initGeoMap(elementId);
};

window.setIntentumGeoMapData = async function (elementId, normalLng, normalLat, normalLabel, loginLng, loginLat, loginLabel) {
  if (!window.IntentumECharts) return false;
  var ok = await window.IntentumECharts.initGeoMap(elementId);
  if (!ok) return false;
  var chart = window.IntentumECharts.instances[elementId];
  if (!chart) return false;
  var option = {
    tooltip: { trigger: 'item', confine: true },
    geo: {
      map: 'world',
      roam: true,
      itemStyle: { areaColor: '#f3f3f3', borderColor: '#999' },
      emphasis: { itemStyle: { areaColor: '#eee' } }
    },
    series: [
      {
        name: 'Normal',
        type: 'scatter',
        coordinateSystem: 'geo',
        data: [[normalLng, normalLat, normalLabel]],
        symbolSize: 16,
        itemStyle: { color: '#4caf50' },
        label: { show: true, formatter: normalLabel }
      },
      {
        name: 'Login IP',
        type: 'scatter',
        coordinateSystem: 'geo',
        data: [[loginLng, loginLat, loginLabel]],
        symbolSize: 16,
        itemStyle: { color: '#d32f2f' },
        label: { show: true, formatter: loginLabel }
      }
    ]
  };
  chart.setOption(option, true);
  setTimeout(function () { chart.resize(); }, 80);
  return true;
};