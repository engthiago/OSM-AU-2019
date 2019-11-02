document.addEventListener("DOMContentLoaded", function (event) {

    var currentBoundLayer;
    var mapBounds = {
        left: -73.99154663085939,
        bottom: 40.69638796878103,
        right: -73.9369583129883,
        top: 40.73763256552795
    };

    if (localStorage.getItem('bounds')) {
        mapBounds = JSON.parse(localStorage.getItem('bounds'));
    }

    var centerX = (mapBounds.left + mapBounds.right) / 2;
    var centerY = (mapBounds.bottom + mapBounds.top) / 2;

    var map = L.map('mapid').setView([ centerY , centerX ], 13);

    var streets = L.tileLayer('https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoibWFwYm94IiwiYSI6ImNpejY4NXVycTA2emYycXBndHRqcmZ3N3gifQ.rJcFIG214AriISLbB6B5aw', {   
        attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, ' +
            '<a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' +
            'Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
        id: 'mapbox.streets'
    });

    var satellite = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
	    attribution: 'Tiles &copy; Esri &mdash; Source: Esri, i-cubed, USDA, USGS, AEX, GeoEye, Getmapping, Aerogrid, IGN, IGP, UPR-EGP, and the GIS User Community'
    });

    satellite.addTo(map);

    var baseLayers = {
        "Satellite": satellite,
        "Streets": streets
    };

    L.control.layers(baseLayers).addTo(map);

    L.drawLocal.draw.handlers.rectangle.tooltip.start = 'Draw the map bounding box';
    L.drawLocal.draw.toolbar.buttons.rectangle = 'Create boundings';

    // Initialise the FeatureGroup to store editable layers
    var editableLayers = new L.FeatureGroup();
    map.addLayer(editableLayers);

    var drawControl = new L.Control.Draw({
        position: 'topright',
        draw: {
            polyline: false,
            polygon: false,
            circle: false,
            marker: false,
            rectangle: {
                shapeOptions: {
                    clickable: true,
                    color: '#0077ff',
                    weight: 1
                }
            }
        },
        edit: {
            featureGroup: editableLayers,
            remove: false,
            edit: false,
        }
    });

    map.addControl(drawControl);

    map.on('draw:created', function (e) {
        var type = e.layerType,
            layer = e.layer;

        removeLayer();

        if (type === 'rectangle') {

            var bounds = layer.getBounds();
            var northEast = bounds.getNorthEast();
            var southWest = bounds.getSouthWest();

            mapBounds.left = southWest.lng;
            mapBounds.bottom = southWest.lat;
            mapBounds.right = northEast.lng;
            mapBounds.top = northEast.lat;

            localStorage.setItem('bounds', JSON.stringify(mapBounds));

            var popUp = '<h3>Revit Bounding Box</h3>'
            popUp += mapBounds.left + ', ' + mapBounds.bottom + '</br>' + mapBounds.right + ', ' + mapBounds.top + '</br></br>';
            popUp += '<button id="rem">Remove Boundings</button><button id="sendrevit">Send to Revit</button>'

            layer.bindPopup(popUp);
        }

        editableLayers.addLayer(layer);
        currentBoundLayer = layer;
    });

    function removeLayer() {
        if (currentBoundLayer) {
            editableLayers.removeLayer(currentBoundLayer);
            currentBoundLayer = null;
        }
    }

    function sendToRevit() {
        console.log(mapBounds);
    }

    var popclick = function (e) {
        if (e.popup) {
            document.getElementById('rem').addEventListener('click', removeLayer);
            document.getElementById('sendrevit').addEventListener('click', sendToRevit);
        }
    };

    var popclose = function (e) {
        if (e.popup) {
            document.getElementById('rem').removeEventListener('click', removeLayer);
            document.getElementById('sendrevit').removeEventListener('click', sendToRevit);
        }
    }

    map.on('popupopen', popclick);
    map.on('popupclose', popclose);

});