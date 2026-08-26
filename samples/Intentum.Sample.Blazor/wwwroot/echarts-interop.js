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
// CLIMATE RISK MAP — Redesigned: Clean hierarchy, consistent markers
// ============================================================
window.ClimateMap = {
  instance: null,
  elementId: null,
  level: 'world',        // 'world' | 'country'
  countryIso3: null,
  countryName: null,
  riskData: [],          // world risk data [{name, value, iso3}]
  location: null,        // {lat, lng, label, color, radiusKm}
  _initialized: false,

  init: async function(elementId) {
    this.elementId = elementId;
    var el = document.getElementById(elementId);
    if (!el || typeof echarts === 'undefined') return false;

    // Load world.json
    var worldJson = null;
    var urls = ['/data/world.json', '/world.json'];
    for (var i = 0; i < urls.length; i++) {
      try {
        var r = await fetch(urls[i]);
        if (r.ok) { worldJson = await r.json(); break; }
      } catch(e) {}
    }
    if (!worldJson) {
      el.innerHTML = '<div style="color:#ef4444;padding:20px;text-align:center;">Harita yüklenemedi</div>';
      return false;
    }
    try { echarts.registerMap('world', worldJson); } catch(e) {}

    if (this.instance) { try { this.instance.dispose(); } catch(e){} }
    this.instance = echarts.init(el);
    this.level = 'world';
    this._initialized = true;
    this._bindClicks();
    this._renderWorld();
    return true;
  },

  _bindClicks: function() {
    var self = this;
    if (!this.instance) return;
    this.instance.off('click');
    this.instance.on('click', function(params) {
      if (params.componentType !== 'series' || params.seriesType !== 'map') return;

      if (self.level === 'world') {
        // Click on country → drill down
        var iso3 = self._nameToIso3(params.name);
        if (iso3) {
          self._drillToCountry(iso3, params.name);
        }
      } else if (self.level === 'country') {
        // Click on province → notify Blazor
        if (window.DotNetClimateMap) {
          window.DotNetClimateMap.invokeMethodAsync('OnProvinceClicked', params.name);
        }
      }
    });
  },

  // ── WORLD VIEW ──────────────────────────────────────────────
  _renderWorld: function() {
    if (!this.instance) return;
    this.level = 'world';

    var data = (this.riskData || []).map(function(d) {
      return { name: d.name, value: d.value, iso3: d.iso3 };
    });

    this.instance.setOption({
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'item',
        confine: true,
        formatter: function(params) {
          if (params.seriesType !== 'map') return params.name;
          var d = data.find(function(x) { return x.name === params.name; });
          var score = d ? d.value : null;
          if (score == null) return '<b>' + params.name + '</b><br/>Veri yok';
          var level = score > 4 ? 'Cok Yuksek' : score > 3 ? 'Yuksek' : score > 2 ? 'Orta' : 'Dusuk';
          return '<b>' + params.name + '</b><br/>Su Stresi: ' + score + '/5 (' + level + ')';
        }
      },
      visualMap: {
        min: 0, max: 5,
        left: 'left', bottom: 10,
        text: ['Yuksek', 'Dusuk'],
        textStyle: { color: '#94a3b8', fontSize: 10 },
        inRange: { color: ['#22c55e', '#84cc16', '#f59e0b', '#ef4444', '#991b1b'] },
        calculable: true
      },
      geo: {
        map: 'world',
        roam: true,
        zoom: 1.1,
        center: [25, 30],
        itemStyle: { areaColor: '#1e293b', borderColor: '#334155', borderWidth: 0.5 },
        emphasis: {
          itemStyle: { areaColor: '#334155', borderColor: '#60a5fa', borderWidth: 1.2 },
          label: { show: true, color: '#fff', fontSize: 11 }
        }
      },
      series: [{
        name: 'Risk Haritasi',
        type: 'map',
        map: 'world',
        roam: false,
        zoom: 1.1,
        center: [25, 30],
        label: { show: false },
        itemStyle: { areaColor: '#1e293b', borderColor: '#334155', borderWidth: 0.5 },
        emphasis: {
          label: { show: false },
          itemStyle: { areaColor: '#334155' }
        },
        data: data
      }]
    }, true);

    this._bindClicks();
    this._showLocationMarker();
  },

  updateRiskData: function(riskData) {
    this.riskData = riskData || [];
    if (this.level === 'world' && this._initialized) this._renderWorld();
  },

  // ── COUNTRY DRILL-DOWN ──────────────────────────────────────
  _drillToCountry: async function(iso3, countryName) {
    if (!this.instance) return;
    this.level = 'country';
    this.countryIso3 = iso3;
    this.countryName = countryName;

    // Fetch GADM level 1
    var geoJson = null;
    try {
      var resp = await fetch('/data/gadm/gadm41_' + iso3 + '_1.json');
      if (resp.ok) geoJson = await resp.json();
    } catch(e) {}

    if (!geoJson || !geoJson.features || geoJson.features.length === 0) {
      // Fallback: zoom to country on world map
      this.level = 'world';
      this.countryIso3 = null;
      this.countryName = null;
      if (this.location) {
        this.instance.setOption({
          geo: { center: [this.location.lng, this.location.lat], zoom: 5 },
          series: [{ center: [this.location.lng, this.location.lat], zoom: 5 }]
        });
        this._showLocationMarker();
      }
      return;
    }

    var mapName = 'gadm_' + iso3 + '_1';
    try { echarts.registerMap(mapName, geoJson); } catch(e) {}

    // Calculate bounds
    var minLng = Infinity, maxLng = -Infinity, minLat = Infinity, maxLat = -Infinity;
    geoJson.features.forEach(function(f) {
      function scan(arr) {
        if (typeof arr[0] === 'number') {
          if (arr[0] < minLng) minLng = arr[0];
          if (arr[0] > maxLng) maxLng = arr[0];
          if (arr[1] < minLat) minLat = arr[1];
          if (arr[1] > maxLat) maxLat = arr[1];
        } else { arr.forEach(scan); }
      }
      scan(f.geometry.coordinates);
    });
    var centerLng = (minLng + maxLng) / 2;
    var centerLat = (minLat + maxLat) / 2;
    var span = Math.max(maxLng - minLng, maxLat - minLat);
    var zoom = span > 30 ? 1.2 : span > 15 ? 2 : span > 8 ? 3 : span > 4 ? 4.5 : 6;

    // Country risk baseline
    var countryRisk = 2.5;
    for (var r = 0; r < this.riskData.length; r++) {
      if (this.riskData[r].iso3 === iso3 || this.riskData[r].name === countryName) {
        countryRisk = this.riskData[r].value || 2.5;
        break;
      }
    }

    // Province risk with variation
    var provinces = geoJson.features.map(function(f, idx) {
      var name = f.properties.NAME_1 || ('Province ' + idx);
      var hash = 0;
      for (var c = 0; c < name.length; c++) {
        hash = ((hash << 5) - hash) + name.charCodeAt(c);
        hash = hash & hash;
      }
      var variation = ((Math.abs(hash % 1000) / 1000) - 0.5) * 2.0;
      var risk = Math.max(0.3, Math.min(5.0, countryRisk + variation));
      return { name: name, value: parseFloat(risk.toFixed(1)) };
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
        left: 'left', bottom: 10,
        text: ['Yuksek', 'Dusuk'],
        textStyle: { color: '#94a3b8', fontSize: 10 },
        inRange: { color: ['#22c55e', '#84cc16', '#f59e0b', '#ef4444', '#991b1b'] },
        calculable: true
      },
      geo: {
        map: mapName,
        roam: true,
        center: [centerLng, centerLat],
        zoom: zoom,
        itemStyle: { areaColor: '#1e293b', borderColor: '#475569', borderWidth: 0.8 },
        emphasis: {
          itemStyle: { areaColor: '#1e3a5f', borderColor: '#60a5fa', borderWidth: 1.5 },
          label: { show: true, color: '#fff' }
        }
      },
      series: [{
        name: countryName,
        type: 'map',
        map: mapName,
        roam: false,
        center: [centerLng, centerLat],
        zoom: zoom,
        label: { show: true, fontSize: 9, color: '#94a3b8' },
        itemStyle: { areaColor: '#1e293b', borderColor: '#475569', borderWidth: 0.8 },
        emphasis: {
          label: { show: true, color: '#fff', fontSize: 11 },
          itemStyle: { areaColor: '#3b82f6' }
        },
        data: provinces
      }]
    }, true);

    this._bindClicks();
    this._showLocationMarker();

    if (window.DotNetClimateMap) {
      window.DotNetClimateMap.invokeMethodAsync('OnCountryDrillDown', iso3, countryName);
    }
  },

  // ── BACK TO WORLD ───────────────────────────────────────────
  goBack: function() {
    if (this.level === 'country') {
      this.countryIso3 = null;
      this.countryName = null;
      this._renderWorld();
      if (window.DotNetClimateMap) {
        window.DotNetClimateMap.invokeMethodAsync('OnMapBackToWorld');
      }
    }
  },

  // ── LOCATION MARKER ─────────────────────────────────────────
  _showLocationMarker: function() {
    if (!this.instance || !this.location) return;
    var loc = this.location;
    var color = loc.color || '#ef4444';
    var label = loc.label || 'Konum';
    var radiusDeg = (loc.radiusKm || 10) / 111.0;

    // Circle points
    var circlePoints = [];
    for (var i = 0; i <= 64; i++) {
      var angle = (i / 64) * 2 * Math.PI;
      var dLat = radiusDeg * Math.cos(angle);
      var dLng = radiusDeg * Math.sin(angle) / Math.cos(loc.lat * Math.PI / 180);
      circlePoints.push([loc.lng + dLng, loc.lat + dLat]);
    }

    // Circle lines
    var lineData = [];
    for (var j = 0; j < circlePoints.length - 1; j++) {
      lineData.push({ coords: [circlePoints[j], circlePoints[j+1]] });
    }

    // Get existing non-factory series
    var opt = this.instance.getOption();
    var currentSeries = opt.series || [];
    var baseSeries = [];
    for (var k = 0; k < currentSeries.length; k++) {
      if (currentSeries[k].name !== 'location' && currentSeries[k].name !== 'location-radius') {
        baseSeries.push(currentSeries[k]);
      }
    }

    // Add location marker
    baseSeries.push({
      name: 'location',
      type: 'effectScatter',
      coordinateSystem: 'geo',
      geoIndex: 0,
      data: [{ name: label, value: [loc.lng, loc.lat, 1], itemStyle: { color: color } }],
      symbolSize: 14,
      rippleEffect: { brushType: 'stroke', scale: 4, period: 3 },
      label: {
        show: true,
        formatter: label,
        position: 'right',
        color: '#f1f5f9',
        fontSize: 11,
        fontWeight: 'bold',
        backgroundColor: 'rgba(15,23,42,0.85)',
        padding: [4, 8],
        borderRadius: 4,
        borderColor: color,
        borderWidth: 1
      },
      zlevel: 5
    });

    // Add radius circle
    baseSeries.push({
      name: 'location-radius',
      type: 'lines',
      coordinateSystem: 'geo',
      geoIndex: 0,
      polyline: false,
      lineStyle: { color: color, width: 1.5, opacity: 0.5, type: 'dashed' },
      effect: { show: false },
      data: lineData,
      zlevel: 4,
      silent: true
    });

    this.instance.setOption({ series: baseSeries });
  },

  setLocation: function(lat, lng, radiusKm, label, color) {
    this.location = { lat: lat, lng: lng, radiusKm: radiusKm, label: label, color: color };
    if (this.instance) this._showLocationMarker();
  },

  // ── NAVIGATION ──────────────────────────────────────────────
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
    if (this.level === 'world') {
      this.instance.setOption({ geo: { center: [25, 30], zoom: 1.1 }, series: [{ center: [25, 30], zoom: 1.1 }] });
    } else if (this.countryIso3) {
      this._drillToCountry(this.countryIso3, this.countryName || this.countryIso3);
    }
  },

  focusLocation: function() {
    if (!this.instance || !this.location) return;
    var loc = this.location;
    this.instance.setOption({
      geo: { center: [loc.lng, loc.lat], zoom: 6 }
    });
    this._showLocationMarker();
  },

  // ── ISO3 MAPPING ────────────────────────────────────────────
  _nameToIso3: function(name) {
    var n = name.toLowerCase();
    var map = {
      'turkey': 'TUR', 'united states of america': 'USA', 'united states': 'USA',
      'united kingdom': 'GBR', 'germany': 'DEU', 'france': 'FRA', 'italy': 'ITA',
      'china': 'CHN', 'india': 'IND', 'japan': 'JPN', 'brazil': 'BRA',
      'russia': 'RUS', 'canada': 'CAN', 'australia': 'AUS', 'south africa': 'ZAF',
      'saudi arabia': 'SAU', 'spain': 'ESP', 'mexico': 'MEX', 'indonesia': 'IDN',
      'south korea': 'KOR', 'north korea': 'PRK', 'czech republic': 'CZE',
      'dominican republic': 'DOM', 'south sudan': 'SSD', 'laos': 'LAO',
      'bosnia and Herz.': 'BIH', 'bosnia and herzegovina': 'BIH',
      'republic of the congo': 'COG', 'republic of congo': 'COG',
      'dem. rep. congo': 'COD', 'democratic republic of the congo': 'COD',
      'central african rep.': 'CAF', 'central african republic': 'CAF',
      "cote d'ivoire": 'CIV', "ivory coast": 'CIV',
      'eq. guinea': 'GNQ', 'equatorial guinea': 'GNQ',
      'antigua and barb.': 'ATG', 'antigua and baruda': 'ATG',
      'saint kitts and nevis': 'KNA', 'st. kitts and nevis': 'KNA',
      'san marino': 'SMR'
    };
    return map[n] || '';
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
  ClimateMap.setLocation(lat, lng, radiusKm, label, color);
  return true;
};

window.focusFactoryOnMap = function() {
  ClimateMap.focusLocation();
  return true;
};

window.drillDownCountry = function(iso3, name) {
  ClimateMap._drillToCountry(iso3, name);
  return true;
};

window.goBackToWorld = function() {
  ClimateMap.goBack();
  return true;
};

window.zoomClimateMapIn = function() { ClimateMap.zoomIn(); return true; };
window.zoomClimateMapOut = function() { ClimateMap.zoomOut(); return true; };
window.resetClimateMapZoom = function() { ClimateMap.resetZoom(); return true; };
