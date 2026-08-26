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
        base + '/data/world.json',
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
};window.setIntentumGeoMapData = async function (elementId, normalLng, normalLat, normalLabel, loginLng, loginLat, loginLabel) {
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

// ============================================================
// CLIMATE RISK MAP — Drill-Down System
// ============================================================
window.ClimateMap = {
  instance: null,
  elementId: null,
  currentLevel: 'world',  // 'world' | 'country'
  currentCountry: null,
  worldRiskData: [],
  factoryMarker: null,

  init: async function(elementId) {
    this.elementId = elementId;
    var el = document.getElementById(elementId);
    if (!el || typeof echarts === 'undefined') return false;

    // Load world.json
    var worldJson = null;
    var urls = ['/data/world.json'];
    for (var i = 0; i < urls.length; i++) {
      try { var r = await fetch(urls[i]); if (r.ok) { worldJson = await r.json(); break; } } catch(e) {}
    }
    if (!worldJson) { console.warn('world.json failed'); return false; }
    echarts.registerMap('world', worldJson);

    // Dispose existing
    if (this.instance) { this.instance.dispose(); }
    this.instance = echarts.init(el);
    this.currentLevel = 'world';

    var self = this;
    this.instance.on('click', function(params) {
      if (self.currentLevel === 'world' && params.componentType === 'series') {
        var countryName = params.name;
        var iso3 = self._getIso3FromName(countryName);
        if (iso3) {
          self.drillDown(iso3, countryName);
        }
      } else if (self.currentLevel === 'country' && params.componentType === 'series') {
        if (window.DotNetClimateMap) {
          window.DotNetClimateMap.invokeMethodAsync('OnProvinceClicked', params.name);
        }
      }
    });

    this.renderWorldView();
    return true;
  },

  renderWorldView: function() {
    if (!this.instance) return;
    this.currentLevel = 'world';
    var riskData = this.worldRiskData || [];

    this.instance.setOption({
      tooltip: {
        trigger: 'item',
        formatter: function(params) {
          var d = riskData.find(function(r) { return r.name === params.name; });
          var score = d ? d.value : 'N/A';
          var level = score > 4 ? 'Çok Yüksek' : score > 3 ? 'Yüksek' : score > 2 ? 'Orta' : score > 1 ? 'Düşük' : 'Veri Yok';
          return '<b>' + params.name + '</b><br/>Risk: ' + score + '/5<br/>Seviye: ' + level + '<br/><i>Tıklamak için tıklayın</i>';
        }
      },
      geo: {
        map: 'world',
        roam: true,
        zoom: 1.3,
        center: [30, 40],
        itemStyle: { areaColor: '#1a1f36', borderColor: '#334155', borderWidth: 0.5 },
        emphasis: {
          itemStyle: { areaColor: '#334155', borderColor: '#60a5fa', borderWidth: 1.5 },
          label: { show: true, color: '#fff', fontSize: 11 }
        },
        regions: []
      },
      visualMap: {
        min: 0, max: 5,
        left: 'left', bottom: 20,
        text: ['Yüksek', 'Düşük'],
        textStyle: { color: '#94a3b8', fontSize: 10 },
        inRange: { color: ['#22c55e', '#84cc16', '#f59e0b', '#ef4444', '#991b1b'] },
        calculable: true,
        realtime: true
      },
      series: [{
        name: 'Risk Haritası',
        type: 'map',
        map: 'world',
        roam: true,
        zoom: 1.3,
        center: [30, 40],
        emphasis: {
          label: { show: true, color: '#fff', fontSize: 11 },
          itemStyle: { areaColor: '#3b82f6' }
        },
        label: { show: false },
        itemStyle: { areaColor: '#1a1f36', borderColor: '#334155', borderWidth: 0.5 },
        data: riskData
      }]
    }, true);

    if (this.factoryMarker) this.showFactory();
  },

  updateRiskData: function(riskData) {
    this.worldRiskData = riskData || [];
    if (this.currentLevel === 'world') this.renderWorldView();
  },

  // Name → ISO3 mapping for drill-down (20 mismatched countries)
  _nameToIso3: {
    'South Korea': 'KOR', 'North Korea': 'PRK', 'Czech Republic': 'CZE',
    'Dominican Republic': 'DOM', 'South Sudan': 'SSD', 'Laos': 'LAO',
    'Bosnia and Herz.': 'BIH', 'Bosnia and Herzegovina': 'BIH',
    'Republic of the Congo': 'COG', 'Republic of Congo': 'COG',
    'Dem. Rep. Congo': 'COD', 'Democratic Republic of the Congo': 'COD',
    'Central African Rep.': 'CAF', 'Central African Republic': 'CAF',
    "Côte d'Ivoire": 'CIV', "Ivory Coast": 'CIV',
    'Eq. Guinea': 'GNQ', 'Equatorial Guinea': 'GNQ',
    'Antigua and Barb.': 'ATG', 'Antigua and Barbuda': 'ATG',
    'Saint Kitts and Nevis': 'KNA', 'St. Kitts and Nevis': 'KNA',
    'San Marino': 'SMR'
  },

  _getIso3FromName: function(name) {
    if (this._nameToIso3[name]) return this._nameToIso3[name];
    // Try common patterns
    var n = name.toLowerCase();
    if (n === 'turkey') return 'TUR';
    if (n === 'united states of america' || n === 'united states') return 'USA';
    if (n === 'united kingdom') return 'GBR';
    if (n === 'germany') return 'DEU';
    if (n === 'france') return 'FRA';
    if (n === 'italy') return 'ITA';
    if (n === 'china') return 'CHN';
    if (n === 'india') return 'IND';
    if (n === 'japan') return 'JPN';
    if (n === 'brazil') return 'BRA';
    if (n === 'russia') return 'RUS';
    if (n === 'canada') return 'CAN';
    if (n === 'australia') return 'AUS';
    if (n === 'south africa') return 'ZAF';
    if (n === 'saudi arabia') return 'SAU';
    return '';
  },

  drillDown: async function(iso3, countryName) {
    if (!this.instance) return;
    this.currentLevel = 'country';
    this.currentCountry = iso3;

    // Fetch GADM level 1 data
    var geoJson = null;
    try {
      var resp = await fetch('/data/gadm/gadm41_' + iso3 + '_1.json');
      if (resp.ok) geoJson = await resp.json();
    } catch(e) {}

    if (!geoJson) {
      console.warn('GADM level 1 not found for ' + iso3);
      this.currentLevel = 'world';
      return;
    }

    var mapName = 'gadm_' + iso3 + '_1';
    try { echarts.registerMap(mapName, geoJson); } catch(e) {}

    // Extract province names and assign random risk for demo
    var provinces = geoJson.features.map(function(f) {
      return { name: f.properties.NAME_1, value: (Math.random() * 4 + 1).toFixed(1) };
    });

    var self = this;
    this.instance.setOption({
      tooltip: {
        trigger: 'item',
        formatter: function(params) {
          return '<b>' + params.name + '</b><br/>Risk: ' + (params.value || 'N/A') + '/5';
        }
      },
      geo: {
        map: mapName,
        roam: true,
        itemStyle: { areaColor: '#1a1f36', borderColor: '#475569', borderWidth: 1 },
        emphasis: {
          itemStyle: { areaColor: '#1e3a5f', borderColor: '#60a5fa', borderWidth: 2 },
          label: { show: true, color: '#fff' }
        }
      },
      series: [{
        name: countryName + ' Bölgeleri',
        type: 'map',
        map: mapName,
        roam: true,
        emphasis: {
          label: { show: true, color: '#fff' },
          itemStyle: { areaColor: '#3b82f6' }
        },
        label: { show: true, fontSize: 9, color: '#94a3b8' },
        itemStyle: { areaColor: '#1a1f36', borderColor: '#475569', borderWidth: 1 },
        data: provinces
      }]
    }, true);

    if (this.factoryMarker) this.showFactory();

    // Notify Blazor of drill-down
    if (window.DotNetClimateMap) {
      window.DotNetClimateMap.invokeMethodAsync('OnCountryDrillDown', iso3, countryName);
    }
  },

  goBack: function() {
    if (this.currentLevel === 'country') {
      this.renderWorldView();
      if (window.DotNetClimateMap) {
        window.DotNetClimateMap.invokeMethodAsync('OnMapBackToWorld');
      }
    }
  },

  showFactory: function() {
    if (!this.instance || !this.factoryMarker) return;
    var f = this.factoryMarker;
    var radiusDeg = (f.radiusKm || 10) / 111.32;
    var circlePoints = [];
    for (var i = 0; i <= 60; i++) {
      var angle = (i / 60) * 2 * Math.PI;
      var lat = f.lat + radiusDeg * Math.cos(angle);
      var lng = f.lng + (radiusDeg * Math.cos(f.lat * Math.PI / 180)) * Math.sin(angle);
      circlePoints.push([lng, lat]);
    }

    var existingSeries = this.instance.getOption().series || [];
    var filtered = existingSeries.filter(function(s) { return s.name !== 'factory'; });

    filtered.push({
      name: 'factory',
      type: 'effectScatter',
      coordinateSystem: 'geo',
      data: [{ name: f.label || 'Fabrika', value: [f.lng, f.lat, 50], itemStyle: { color: f.color || '#ef4444' } }],
      symbolSize: 14,
      rippleEffect: { brushType: 'stroke', scale: 4 },
      label: { show: true, formatter: f.label || 'Fabrika', position: 'right', color: '#f1f5f9', fontSize: 11, fontWeight: 'bold' }
    });
    filtered.push({
      name: 'factory',
      type: 'scatter',
      coordinateSystem: 'geo',
      data: circlePoints,
      symbolSize: 1,
      itemStyle: { color: f.color || 'rgba(239,68,68,0.6)' },
      silent: true
    });

    this.instance.setOption({ series: filtered }, true);
  },

  setFactory: function(lat, lng, radiusKm, label, color) {
    this.factoryMarker = { lat: lat, lng: lng, radiusKm: radiusKm, label: label, color: color };
    if (this.instance) this.showFactory();
  },

  zoomIn: function() {
    if (!this.instance) return;
    var opt = this.instance.getOption();
    var geo = opt.geo && opt.geo[0];
    var series = opt.series && opt.series[0];
    var curZoom = (geo && geo.zoom) || (series && series.zoom) || 1.3;
    this.instance.setOption({ geo: { zoom: curZoom * 1.3 }, series: [{ zoom: curZoom * 1.3 }] });
  },

  zoomOut: function() {
    if (!this.instance) return;
    var opt = this.instance.getOption();
    var geo = opt.geo && opt.geo[0];
    var series = opt.series && opt.series[0];
    var curZoom = (geo && geo.zoom) || (series && series.zoom) || 1.3;
    this.instance.setOption({ geo: { zoom: curZoom / 1.3 }, series: [{ zoom: curZoom / 1.3 }] });
  },

  resetZoom: function() {
    if (this.currentLevel === 'world') this.renderWorldView();
    else if (this.currentCountry) this.drillDown(this.currentCountry, this.currentCountry);
  }
};

// Blazor callable functions
window.registerDotNetClimateMap = function(dotNetRef) {
  window.DotNetClimateMap = dotNetRef;
};

window.unregisterDotNetClimateMap = function() {
  window.DotNetClimateMap = null;
};

window.initClimateGeoMap = async function(elementId) {
  return await ClimateMap.init(elementId);
};

window.updateClimateWorldMap = function(elementId, riskData) {
  ClimateMap.updateRiskData(riskData);
  return true;
};

window.setFactoryMarkerOnMap = function(lat, lng, radiusKm, label, color) {
  ClimateMap.setFactory(lat, lng, radiusKm, label, color);
  return true;
};

window.drillDownCountry = function(iso3, name) {
  ClimateMap.drillDown(iso3, name);
  return true;
};

window.goBackToWorld = function() {
  ClimateMap.goBack();
  return true;
};

window.zoomClimateMapIn = function() { ClimateMap.zoomIn(); return true; };
window.zoomClimateMapOut = function() { ClimateMap.zoomOut(); return true; };
window.resetClimateMapZoom = function() { ClimateMap.resetZoom(); return true; };
