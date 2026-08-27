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
      chart.setOption(option, true);
    } else if (chart) chart.setOption(option || {}, true);
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
      if (!option.series[0].label.formatter) {
        option.series[0].label.formatter = function (params) {
          var v = params.value;
          var num = Array.isArray(v) ? v[2] : v;
          return (Math.round((num || 0) * 100)) + '%';
        };
      }
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
        var color = score > 0.75 ? '#991b1b' : score > 0.5 ? '#ef4444' : score > 0.25 ? '#f59e0b' : '#22c55e';
        return '<b>' + yLabel + '</b><br/>' + xLabel + ' riski: ' +
          '<span style="color:' + color + ';font-weight:700;">' + (Math.round((score || 0) * 100)) + '%</span>';
      };
    }
    chart.setOption(option, true);
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
  countryProvinces: null, // province risk data [{name, value}]
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
    this._render();
    return true;
  },

  // One unified option builder — always produces a complete, consistent chart.
  // mapName: the registered map to show ('world' or 'gadm_XXX_1')
  // data: choropleth data [{name, value}]
  // tooltipSuffix: extra tooltip line (e.g. country name on province view)
  _buildOption: function(mapName, data, tooltipSuffix) {
    var self = this;
    var loc = this.location;
    var series = [{
      name: 'risk',
      type: 'map',
      geoIndex: 0,
      data: data || [],
      label: { show: this.level === 'country', fontSize: 9, color: '#94a3b8' },
      itemStyle: { areaColor: '#1e293b', borderColor: '#475569', borderWidth: 0.8 },
      emphasis: {
        label: { show: true, color: '#fff', fontSize: 11 },
        itemStyle: { areaColor: '#3b82f6' }
      }
    }];

    // Location marker + radius as overlaid series (share the geo coordinate system)
    if (loc) {
      var color = loc.color || '#ef4444';
      var label = loc.label || 'Konum';
      var radiusDeg = (loc.radiusKm || 10) / 111.0;
      var circlePoints = [];
      for (var i = 0; i <= 64; i++) {
        var angle = (i / 64) * 2 * Math.PI;
        var dLat = radiusDeg * Math.cos(angle);
        var dLng = radiusDeg * Math.sin(angle) / Math.cos(loc.lat * Math.PI / 180);
        circlePoints.push([loc.lng + dLng, loc.lat + dLat]);
      }
      var lineData = [];
      for (var j = 0; j < circlePoints.length - 1; j++) {
        lineData.push({ coords: [circlePoints[j], circlePoints[j + 1]] });
      }
      series.push({
        name: 'location',
        type: 'effectScatter',
        coordinateSystem: 'geo',
        geoIndex: 0,
        data: [{ value: [loc.lng, loc.lat], itemStyle: { color: color } }],
        symbolSize: 13,
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
      series.push({
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
    }

    return {
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'item',
        confine: true,
        formatter: function(params) {
          if (params.seriesType !== 'map') return params.name;
          var v = params.value;
          if (v == null) return '<b>' + params.name + '</b><br/>Veri yok';
          var score = +v;
          var level = score > 4 ? 'Çok Yüksek' : score > 3 ? 'Yüksek' : score > 2 ? 'Orta' : 'Düşük';
          var color = score > 4 ? '#991b1b' : score > 3 ? '#ef4444' : score > 2 ? '#f59e0b' : '#22c55e';
          var extra = tooltipSuffix ? '<br/><i style="color:#64748b;">' + tooltipSuffix + '</i>' : '';
          return '<div style="min-width:160px;">' +
            '<div style="font-weight:700;font-size:13px;color:#e2e8f0;margin-bottom:4px;">' + params.name + '</div>' +
            '<div style="font-size:12px;margin-bottom:3px;">Su Stresi Riski: ' +
              '<span style="font-weight:700;color:' + color + ';">' + score + '/5 (' + level + ')</span>' +
            '</div>' +
            '<div style="font-size:11px;color:#94a3b8;">İl detayı için tıklayın</div>' +
            '</div>';
        }
      },
      visualMap: {
        min: 0, max: 5,
        left: 'left', bottom: 10,
        text: ['Yüksek', 'Düşük'],
        textStyle: { color: '#94a3b8', fontSize: 10 },
        inRange: { color: ['#22c55e', '#84cc16', '#f59e0b', '#ef4444', '#991b1b'] },
        calculable: true
      },
      geo: {
        map: mapName,
        roam: true,
        layoutCenter: ['50%', '50%'],
        layoutSize: '92%',
        itemStyle: { areaColor: '#1e293b', borderColor: '#475569', borderWidth: 0.8 },
        emphasis: {
          itemStyle: { areaColor: '#1e3a5f', borderColor: '#60a5fa', borderWidth: 1.5 },
          label: { show: true, color: '#fff' }
        }
      },
      series: series
    };
  },

  _render: function() {
    if (!this.instance) return;
    var opt;
    if (this.level === 'country') {
      opt = this._buildOption('gadm_' + this.countryIso3 + '_1', this.countryProvinces || [], this.countryName);
    } else {
      opt = this._buildOption('world', (this.riskData || []).map(function(d) {
        return { name: d.name, value: d.value };
      }), null);
    }
    this.instance.setOption(opt, true);
    this._bindClicks();
  },

  _bindClicks: function() {
    var self = this;
    if (!this.instance) return;
    this.instance.off('click');
    this.instance.on('click', function(params) {
      if (params.componentType !== 'series' || params.seriesType !== 'map') return;

      if (self.level === 'world') {
        var iso3 = self._nameToIso3(params.name);
        if (iso3) {
          self._drillToCountry(iso3, params.name);
        }
      } else if (self.level === 'country') {
        if (window.DotNetClimateMap) {
          var riskVal = (params.value != null) ? (+params.value).toFixed(1) : '';
          window.DotNetClimateMap.invokeMethodAsync('OnProvinceClicked', params.name, riskVal, self.countryName || '');
        }
      }
    });
  },

  updateRiskData: function(riskData) {
    var self = this;
    this.riskData = (riskData || []).map(function(d) {
      return {
        name: self._normName(d.name),
        value: d.value,
        iso3: d.iso3
      };
    });
    if (this.level === 'world' && this._initialized) this._render();
  },

  // Normalize WRI country names to world.json geo feature names
  _normName: function(name) {
    var map = {
      'United States of America': 'United States',
      'Dominican Republic': 'Dominican Rep.',
      'Democratic Republic of the Congo': 'Dem. Rep. Congo',
      'Congo (Democratic Republic of the)': 'Dem. Rep. Congo',
      'Congo, Dem. Rep.': 'Dem. Rep. Congo',
      'Republic of the Congo': 'Congo',
      'Central African Republic': 'Central African Rep.',
      'Equatorial Guinea': 'Eq. Guinea',
      'Cote d\'Ivoire': 'Côte d\'Ivoire',
      'Ivory Coast': 'Côte d\'Ivoire',
      'South Korea': 'Korea',
      'Republic of Korea': 'Korea',
      'North Korea': 'Dem. Rep. Korea',
      "Democratic People's Republic of Korea": 'Dem. Rep. Korea',
      'Czech Republic': 'Czech Rep.',
      'Bosnia and Herzegovina': 'Bosnia and Herz.',
      'Falkland Islands': 'Falkland Is.',
      'United States Virgin Islands': 'U.S. Virgin Is.',
      'Virgin Islands (U.S.)': 'U.S. Virgin Is.',
      'Western Sahara': 'W. Sahara',
      'North Macedonia': 'Macedonia',
      'Macedonia (the former Yugoslav Republic of)': 'Macedonia',
      'South Sudan': 'Somalia',
      'Swaziland': 'Swaziland',
      'Eswatini': 'Swaziland',
      'Serbia': 'Serbia',
      'Kosovo': 'Kosovo',
      'Republic of Serbia': 'Serbia',
      'United Republic of Tanzania': 'Tanzania',
      'Tanzania, United Republic of': 'Tanzania',
      'Cape Verde': 'Cabo Verde',
      'Cabo Verde': 'Cabo Verde',
      'The Bahamas': 'Bahamas',
      'Gambia': 'Gambia',
      'The Gambia': 'Gambia',
      'Myanmar': 'Myanmar',
      'Burma': 'Myanmar',
      'Viet Nam': 'Vietnam',
      'Vietnam': 'Vietnam',
      'Lao PDR': 'Laos',
      "Lao People's Democratic Republic": 'Laos',
      'Laos': 'Laos',
      'Dem. Rep. Korea': 'Dem. Rep. Korea',
      'United States': 'United States',
      'United Kingdom': 'United Kingdom',
      'Turkey': 'Turkey',
      'Türkiye': 'Turkey'
    };
    return map[name] || name;
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
      // No GADM data — go back to world view
      this.level = 'world';
      this.countryIso3 = null;
      this.countryName = null;
      this.countryProvinces = null;
      this._render();
      if (window.DotNetClimateMap) {
        window.DotNetClimateMap.invokeMethodAsync('OnMapBackToWorld');
      }
      return;
    }

    var mapName = 'gadm_' + iso3 + '_1';
    // ECharts map series matches data.name to feature.properties.name.
    // GADM uses NAME_1 (and VARNAME_1), so inject a 'name' prop for matching.
    geoJson.features.forEach(function(f) {
      if (!f.properties) f.properties = {};
      f.properties.name = f.properties.NAME_1 || f.properties.VARNAME_1 || f.properties.NAME || 'Unknown';
      if (!f.properties.NAME) f.properties.NAME = f.properties.name;
    });
    try { echarts.registerMap(mapName, geoJson); } catch(e) {}

    // Country risk baseline
    var countryRisk = 2.5;
    for (var r = 0; r < this.riskData.length; r++) {
      if (this.riskData[r].iso3 === iso3 || this.riskData[r].name === countryName) {
        countryRisk = this.riskData[r].value || 2.5;
        break;
      }
    }

    // Province risk with deterministic variation for visual distinction
    var provinces = geoJson.features.map(function(f, idx) {
      var name = f.properties.NAME_1 || ('Province ' + idx);
      var hash = 0;
      for (var c = 0; c < name.length; c++) {
        hash = ((hash << 5) - hash) + name.charCodeAt(c);
        hash = hash & hash;
      }
      var variation = ((Math.abs(hash % 1000) / 1000) - 0.5) * 2.4;
      var risk = Math.max(0.5, Math.min(5.0, countryRisk + variation));
      return { name: name, value: parseFloat(risk.toFixed(1)) };
    });
    this.countryProvinces = provinces;

    this._render();

    if (window.DotNetClimateMap) {
      window.DotNetClimateMap.invokeMethodAsync('OnCountryDrillDown', iso3, countryName);
    }
  },

  // ── BACK TO WORLD ───────────────────────────────────────────
  goBack: function() {
    if (this.level === 'country') {
      this.countryIso3 = null;
      this.countryName = null;
      this.countryProvinces = null;
      this._render();
      if (window.DotNetClimateMap) {
        window.DotNetClimateMap.invokeMethodAsync('OnMapBackToWorld');
      }
    }
  },

  // ── LOCATION MARKER ─────────────────────────────────────────
  // Marker is built into _buildOption; just re-render to show/update it.
  setLocation: function(lat, lng, radiusKm, label, color) {
    this.location = { lat: lat, lng: lng, radiusKm: radiusKm, label: label, color: color };
    if (this.instance && this._initialized) this._render();
  },

  // ── NAVIGATION ──────────────────────────────────────────────
  zoomIn: function() {
    if (!this.instance) return;
    var opt = this.instance.getOption();
    var geo = opt.geo && opt.geo[0];
    var curZoom = (geo && geo.zoom) || 1;
    this.instance.setOption({ geo: { zoom: curZoom * 1.4 } });
  },

  zoomOut: function() {
    if (!this.instance) return;
    var opt = this.instance.getOption();
    var geo = opt.geo && opt.geo[0];
    var curZoom = (geo && geo.zoom) || 1;
    this.instance.setOption({ geo: { zoom: Math.max(0.4, curZoom / 1.4) } });
  },

  resetZoom: function() {
    if (!this.instance) return;
    this._render();
  },

  focusLocation: function() {
    if (!this.instance || !this.location) return;
    var loc = this.location;
    this.instance.dispatchAction({
      type: 'geoRoam',
      geoIndex: 0,
      zoom: 5,
      center: [loc.lng, loc.lat]
    });
  },

  // ── ISO3 MAPPING ────────────────────────────────────────────
  _nameToIso3: function(name) {
    var n = name.toLowerCase();
    var map = {
      'turkey': 'TUR', 'united states': 'USA', 'united states of america': 'USA',
      'united kingdom': 'GBR', 'germany': 'DEU', 'france': 'FRA', 'italy': 'ITA',
      'china': 'CHN', 'india': 'IND', 'japan': 'JPN', 'brazil': 'BRA',
      'russia': 'RUS', 'canada': 'CAN', 'australia': 'AUS', 'south africa': 'ZAF',
      'saudi arabia': 'SAU', 'spain': 'ESP', 'mexico': 'MEX', 'indonesia': 'IDN',
      'korea': 'KOR', 'south korea': 'KOR', 'republic of korea': 'KOR',
      'dem. rep. korea': 'PRK', 'north korea': 'PRK', 'czech rep.': 'CZE',
      'czech republic': 'CZE', 'dominican rep.': 'DOM', 'dominican republic': 'DOM',
      'south sudan': 'SSD', 'laos': 'LAO', 'bosnia and herz.': 'BIH',
      'bosnia and herzegovina': 'BIH', 'congo': 'COG', 'republic of the congo': 'COG',
      'republic of congo': 'COG', 'dem. rep. congo': 'COD',
      'democratic republic of the congo': 'COD', 'central african rep.': 'CAF',
      'central african republic': 'CAF', "côte d'ivoire": 'CIV', "cote d'ivoire": 'CIV',
      "ivory coast": 'CIV', 'eq. guinea': 'GNQ', 'equatorial guinea': 'GNQ',
      'antigua and barb.': 'ATG', 'antigua and barbuda': 'ATG',
      'saint kitts and nevis': 'KNA', 'st. kitts and nevis': 'KNA',
      'san marino': 'SMR', 'north macedonia': 'MKD', 'macedonia': 'MKD',
      'serbia': 'SRB', 'kosovo': 'XKX', 'cabo verde': 'CPV', 'cape verde': 'CPV',
      'the bahamas': 'BHS', 'bahamas': 'BHS', 'swaziland': 'SWZ', 'eswatini': 'SWZ',
      'romania': 'ROU', 'poland': 'POL', 'portugal': 'PRT', 'netherlands': 'NLD',
      'belgium': 'BEL', 'switzerland': 'CHE', 'austria': 'AUT', 'sweden': 'SWE',
      'norway': 'NOR', 'finland': 'FIN', 'denmark': 'DNK', 'greece': 'GRC',
      'ireland': 'IRL', 'ukraine': 'UKR', 'egypt': 'EGY', 'israel': 'ISR',
      'iraq': 'IRQ', 'iran': 'IRN', 'afghanistan': 'AFG', 'pakistan': 'PAK',
      'bangladesh': 'BGD', 'thailand': 'THA', 'vietnam': 'VNM', 'philippines': 'PHL',
      'malaysia': 'MYS', 'singapore': 'SGP', 'argentina': 'ARG', 'chile': 'CHL',
      'colombia': 'COL', 'peru': 'PER', 'venezuela': 'VEN', 'ecuador': 'ECU',
      'morocco': 'MAR', 'tunisia': 'TUN', 'algeria': 'DZA', 'libya': 'LBY',
      'nigeria': 'NGA', 'ethiopia': 'ETH', 'kenya': 'KEN', 'ghana': 'GHA',
      'tanzania': 'TZA', 'moldova': 'MDA', 'viet nam': 'VNM', 'türkiye': 'TUR'
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
