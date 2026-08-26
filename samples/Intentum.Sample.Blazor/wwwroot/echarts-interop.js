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
        } catch (e) { /* try next URL */ }
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

// ============================================================
// CLIMATE RISK MAP — Drill-Down: Ülke > Bölge/İl + Fabrika
// ============================================================
window.ClimateMap = {
  instance: null,
  elementId: null,
  currentLevel: 'world',
  currentCountry: null,
  currentCountryName: null,
  worldRiskData: [],
  factoryMarker: null,
  _initialized: false,

  init: async function(elementId) {
    this.elementId = elementId;
    var el = document.getElementById(elementId);
    if (!el || typeof echarts === 'undefined') {
      console.warn('[ClimateMap] ECharts or element missing');
      return false;
    }
    // Load world.json from local wwwroot
    var worldJson = null;
    var urls = ['/data/world.json', '/world.json'];
    for (var i = 0; i < urls.length; i++) {
      try {
        var r = await fetch(urls[i]);
        if (r.ok) { worldJson = await r.json(); console.log('[ClimateMap] world.json loaded from ' + urls[i]); break; }
        else console.warn('[ClimateMap] world.json fetch ' + urls[i] + ' -> ' + r.status);
      } catch(e) { console.warn('[ClimateMap] world.json fetch error ' + urls[i], e); }
    }
    if (!worldJson) {
      el.innerHTML = '<div style="color:#ef4444;padding:20px;text-align:center;">Harita yüklenemedi (world.json bulunamadı)</div>';
      return false;
    }
    try { echarts.registerMap('world', worldJson); } catch(e) { console.warn('[ClimateMap] registerMap world error', e); }

    if (this.instance) { try { this.instance.dispose(); } catch(e){} }
    this.instance = echarts.init(el);
    this.currentLevel = 'world';
    this._initialized = true;
    this._bindWorldClick();
    this.renderWorldView();
    console.log('[ClimateMap] init done');
    return true;
  },

  _bindWorldClick: function() {
    var self = this;
    if (!this.instance) return;
    this.instance.off('click');
    this.instance.on('click', function(params) {
      if (self.currentLevel === 'world' && params.componentType === 'series' && params.seriesType === 'map') {
        var countryName = params.name;
        var iso3 = self._getIso3FromName(countryName);
        console.log('[ClimateMap] world click:', countryName, '->', iso3);
        if (iso3) {
          self.drillDown(iso3, countryName);
        } else {
          console.warn('[ClimateMap] ISO3 not found for', countryName);
        }
      } else if (self.currentLevel === 'country' && params.componentType === 'series') {
        if (window.DotNetClimateMap) {
          window.DotNetClimateMap.invokeMethodAsync('OnProvinceClicked', params.name);
        }
      }
    });
  },

  renderWorldView: function() {
    if (!this.instance) return;
    this.currentLevel = 'world';
    var riskData = this.worldRiskData || [];
    console.log('[ClimateMap] renderWorldView with', riskData.length, 'countries');

    this.instance.setOption({
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'item',
        confine: true,
        formatter: function(params) {
          if (params.seriesType !== 'map') return params.name;
          var d = null;
          for (var i = 0; i < riskData.length; i++) { if (riskData[i].name === params.name) { d = riskData[i]; break; } }
          var score = d ? d.value : null;
          if (score == null) return '<b>' + params.name + '</b><br/>Veri yok';
          var level = score > 4 ? 'Çok Yüksek' : score > 3 ? 'Yüksek' : score > 2 ? 'Orta' : 'Düşük';
          return '<b>' + params.name + '</b><br/>Su Stresi: ' + score + '/5<br/>Seviye: ' + level + '<br/><i style="color:#60a5fa;">Detay için tıklayın</i>';
        }
      },
      visualMap: {
        min: 0, max: 5,
        left: 'left', bottom: 15,
        text: ['Yüksek', 'Düşük'],
        textStyle: { color: '#94a3b8', fontSize: 10 },
        inRange: { color: ['#22c55e', '#84cc16', '#f59e0b', '#ef4444', '#991b1b'] },
        calculable: true,
        show: true
      },
      geo: {
        map: 'world',
        roam: true,
        zoom: 1.1,
        center: [25, 30],
        selectedMode: false,
        itemStyle: { areaColor: '#1e293b', borderColor: '#334155', borderWidth: 0.5 },
        emphasis: {
          itemStyle: { areaColor: '#334155', borderColor: '#60a5fa', borderWidth: 1.2 },
          label: { show: true, color: '#fff', fontSize: 11 }
        }
      },
      series: [{
        name: 'Risk Haritası',
        type: 'map',
        map: 'world',
        roam: false,
        zoom: 1.1,
        center: [25, 30],
        selectedMode: false,
        label: { show: false },
        itemStyle: { areaColor: '#1e293b', borderColor: '#334155', borderWidth: 0.5 },
        emphasis: {
          label: { show: false },
          itemStyle: { areaColor: '#334155' }
        },
        data: riskData
      }]
    }, true);

    this._bindWorldClick();
    if (this.factoryMarker) {
      var self = this;
      setTimeout(function(){ self.showFactory(); }, 150);
    }
  },

  updateRiskData: function(riskData) {
    console.log('[ClimateMap] updateRiskData', (riskData||[]).length);
    this.worldRiskData = riskData || [];
    if (this.currentLevel === 'world' && this._initialized) this.renderWorldView();
  },

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
    var n = name.toLowerCase();
    var map = {
      'turkey': 'TUR', 'united states of america': 'USA', 'united states': 'USA',
      'united kingdom': 'GBR', 'germany': 'DEU', 'france': 'FRA', 'italy': 'ITA',
      'china': 'CHN', 'india': 'IND', 'japan': 'JPN', 'brazil': 'BRA',
      'russia': 'RUS', 'canada': 'CAN', 'australia': 'AUS', 'south africa': 'ZAF',
      'saudi arabia': 'SAU', 'spain': 'ESP', 'mexico': 'MEX', 'indonesia': 'IDN'
    };
    return map[n] || '';
  },

  drillDown: async function(iso3, countryName) {
    console.log('[ClimateMap] drillDown', iso3, countryName);
    if (!this.instance) return;
    this.currentLevel = 'country';
    this.currentCountry = iso3;
    this.currentCountryName = countryName;

    var geoJson = null;
    try {
      var resp = await fetch('/data/gadm/gadm41_' + iso3 + '_1.json');
      console.log('[ClimateMap] GADM fetch /data/gadm/gadm41_' + iso3 + '_1.json ->', resp.status);
      if (resp.ok) geoJson = await resp.json();
    } catch(e) { console.warn('[ClimateMap] GADM fetch error:', e); }

    if (!geoJson || !geoJson.features || geoJson.features.length === 0) {
      console.warn('[ClimateMap] GADM level 1 not found for ' + iso3 + ', staying on world view');
      this.currentLevel = 'world';
      this.currentCountry = null;
      // Fallback: just zoom to country area on world map if GADM missing, and show factory
      if (this.factoryMarker) {
        this.instance.setOption({
          geo: { center: [this.factoryMarker.lng, this.factoryMarker.lat], zoom: 4 },
          series: [{ center: [this.factoryMarker.lng, this.factoryMarker.lat], zoom: 4 }]
        });
        this.showFactory();
      }
      return;
    }

    var mapName = 'gadm_' + iso3 + '_1';
    try { echarts.registerMap(mapName, geoJson); console.log('[ClimateMap] registered', mapName); } catch(e) { console.warn('[ClimateMap] registerMap error:', e); }

    // Calculate bounds for centering
    var minLng = Infinity, maxLng = -Infinity, minLat = Infinity, maxLat = -Infinity;
    geoJson.features.forEach(function(f) {
      var coords = f.geometry.coordinates;
      function scan(arr) {
        if (typeof arr[0] === 'number') {
          if (arr[0] < minLng) minLng = arr[0];
          if (arr[0] > maxLng) maxLng = arr[0];
          if (arr[1] < minLat) minLat = arr[1];
          if (arr[1] > maxLat) maxLat = arr[1];
        } else { arr.forEach(scan); }
      }
      scan(coords);
    });
    var centerLng = (minLng + maxLng) / 2;
    var centerLat = (minLat + maxLat) / 2;
    var span = Math.max(maxLng - minLng, maxLat - minLat);
    var zoom = span > 30 ? 1.2 : span > 15 ? 2 : span > 8 ? 3 : span > 4 ? 4.5 : 6;
    console.log('[ClimateMap] bounds', minLng, maxLng, minLat, maxLat, 'center', centerLng, centerLat, 'zoom', zoom);

    var provinces = geoJson.features.map(function(f, idx) {
      return { name: f.properties.NAME_1, value: (1 + (idx % 5) * 0.8).toFixed(1) };
    });

    this.instance.setOption({
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'item',
        confine: true,
        formatter: function(params) {
          if (params.seriesType !== 'map') return params.name;
          return '<b>' + params.name + '</b><br/>Risk: ' + (params.value || 'N/A') + '/5<br/><i>' + countryName + '</i>';
        }
      },
      visualMap: {
        min: 0, max: 5,
        left: 'left', bottom: 15,
        text: ['Yüksek', 'Düşük'],
        textStyle: { color: '#94a3b8', fontSize: 10 },
        inRange: { color: ['#22c55e', '#84cc16', '#f59e0b', '#ef4444', '#991b1b'] },
        calculable: true,
        show: true
      },
      geo: {
        map: mapName,
        roam: true,
        center: [centerLng, centerLat],
        zoom: zoom,
        selectedMode: false,
        itemStyle: { areaColor: '#1e293b', borderColor: '#475569', borderWidth: 0.8 },
        emphasis: {
          itemStyle: { areaColor: '#1e3a5f', borderColor: '#60a5fa', borderWidth: 1.5 },
          label: { show: true, color: '#fff' }
        }
      },
      series: [{
        name: countryName + ' Bölgeleri',
        type: 'map',
        map: mapName,
        roam: false,
        center: [centerLng, centerLat],
        zoom: zoom,
        label: { show: true, fontSize: 8, color: '#94a3b8' },
        itemStyle: { areaColor: '#1e293b', borderColor: '#475569', borderWidth: 0.8 },
        emphasis: {
          label: { show: true, color: '#fff', fontSize: 10 },
          itemStyle: { areaColor: '#3b82f6' }
        },
        data: provinces
      }]
    }, true);

    var self = this;
    this.instance.off('click');
    this.instance.on('click', function(params) {
      if (params.componentType === 'series' && params.seriesType === 'map') {
        console.log('[ClimateMap] province click', params.name);
        if (window.DotNetClimateMap) {
          window.DotNetClimateMap.invokeMethodAsync('OnProvinceClicked', params.name);
        }
      }
    });

    if (this.factoryMarker) {
      setTimeout(function(){ self.showFactory(); }, 150);
    }

    if (window.DotNetClimateMap) {
      window.DotNetClimateMap.invokeMethodAsync('OnCountryDrillDown', iso3, countryName);
    }
  },

  goBack: function() {
    if (this.currentLevel === 'country') {
      console.log('[ClimateMap] goBack to world');
      this.currentCountry = null;
      this.currentCountryName = null;
      this.renderWorldView();
      if (window.DotNetClimateMap) {
        window.DotNetClimateMap.invokeMethodAsync('OnMapBackToWorld');
      }
    }
  },

  // Factory marker: effectScatter + dairesel polygon + doğrudan zoom
  showFactory: function() {
    if (!this.instance || !this.factoryMarker) {
      console.log('[ClimateMap] showFactory: no instance or marker');
      return;
    }
    var f = this.factoryMarker;
    console.log('[ClimateMap] showFactory', f);
    var radiusDeg = (f.radiusKm || 10) / 111.0;
    var circlePoints = [];
    for (var i = 0; i <= 64; i++) {
      var angle = (i / 64) * 2 * Math.PI;
      var dLat = radiusDeg * Math.cos(angle);
      var dLng = radiusDeg * Math.sin(angle) / Math.cos(f.lat * Math.PI / 180);
      circlePoints.push([f.lng + dLng, f.lat + dLat]);
    }
    // polygon line for radius
    var lineData = [];
    for (var j = 0; j < circlePoints.length - 1; j++) {
      lineData.push({ coords: [circlePoints[j], circlePoints[j+1]] });
    }

    var geoName = this.currentLevel === 'country' ? ('gadm_' + this.currentCountry + '_1') : 'world';
    console.log('[ClimateMap] showFactory geo', geoName);

    // Use setOption with merge to keep map, add overlay series
    this.instance.setOption({
      series: [
        // keep existing map series is handled by notMerge=false; we append factory series via extra setOption
      ]
    });

    // Add factory series as overlay (merge)
    var opt = this.instance.getOption();
    // ECharts getOption returns series array; we re-set with factory appended
    // For simplicity, append two series for factory
    var currentSeries = opt.series || [];
    // Remove old factory series
    var baseSeries = [];
    for (var k = 0; k < currentSeries.length; k++) {
      if (currentSeries[k].name !== 'factory' && currentSeries[k].name !== 'factory-radius') baseSeries.push(currentSeries[k]);
    }
    baseSeries.push({
      name: 'factory',
      type: 'effectScatter',
      coordinateSystem: 'geo',
      geoIndex: 0,
      data: [{ name: f.label || 'Fabrika', value: [f.lng, f.lat, 1], itemStyle: { color: f.color || '#ef4444' } }],
      symbolSize: 16,
      rippleEffect: { brushType: 'stroke', scale: 4, period: 3 },
      label: { show: true, formatter: f.label || 'Fabrika', position: 'right', color: '#f1f5f9', fontSize: 12, fontWeight: 'bold', backgroundColor: 'rgba(15,23,42,0.8)', padding: [3,6], borderRadius: 4 },
      zlevel: 5
    });
    baseSeries.push({
      name: 'factory-radius',
      type: 'lines',
      coordinateSystem: 'geo',
      geoIndex: 0,
      polyline: false,
      lineStyle: { color: f.color || '#ef4444', width: 1.5, opacity: 0.6, type: 'dashed' },
      effect: { show: false },
      data: lineData,
      zlevel: 4,
      silent: true
    });
    this.instance.setOption({ series: baseSeries });

    // Center map on factory if world level, keep factory in view
    if (this.currentLevel === 'world') {
      this.instance.setOption({
        geo: { center: [f.lng, f.lat], zoom: 5 },
        series: [{ center: [f.lng, f.lat], zoom: 5 }]
      });
    }
  },

  setFactory: function(lat, lng, radiusKm, label, color) {
    console.log('[ClimateMap] setFactory', lat, lng, radiusKm, label, color);
    this.factoryMarker = { lat: lat, lng: lng, radiusKm: radiusKm, label: label, color: color };
    if (this.instance) this.showFactory();
  },

  zoomIn: function() {
    if (!this.instance) return;
    var opt = this.instance.getOption();
    var geo = opt.geo && opt.geo[0];
    var curZoom = (geo && geo.zoom) || 1.1;
    this.instance.setOption({ geo: { zoom: curZoom * 1.4 } });
  },

  zoomOut: function() {
    if (!this.instance) return;
    var opt = this.instance.getOption();
    var geo = opt.geo && opt.geo[0];
    var curZoom = (geo && geo.zoom) || 1.1;
    this.instance.setOption({ geo: { zoom: Math.max(0.5, curZoom / 1.4) } });
  },

  resetZoom: function() {
    if (!this.instance) return;
    if (this.currentLevel === 'world') {
      this.instance.setOption({ geo: { center: [25, 30], zoom: 1.1 }, series: [{ center: [25, 30], zoom: 1.1 }] });
    } else if (this.currentCountry) {
      this.drillDown(this.currentCountry, this.currentCountryName || this.currentCountry);
    }
  },

  focusFactory: function() {
    if (!this.instance || !this.factoryMarker) return;
    var f = this.factoryMarker;
    this.instance.setOption({
      geo: { center: [f.lng, f.lat], zoom: 6 }
    });
    this.showFactory();
  }
};

// Blazor callable functions
window.registerDotNetClimateMap = function(dotNetRef) {
  window.DotNetClimateMap = dotNetRef;
  console.log('[ClimateMap] DotNet registered');
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

window.focusFactoryOnMap = function() {
  ClimateMap.focusFactory();
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
