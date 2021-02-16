var items = [[39.06568, 41.1000464640], [38.06568, 40.1000464640], [39.1000464640, 36.06568]];
function initialize() {
    var e = {
        zoom: zoomLevel, center: new google.maps.LatLng("39.519331", "34.828671"), scrollwheel: false,
        navigationControl: false, mapTypeControlOptions: {
            mapTypeIds: [mapName]
        }
, disableDefaultUI: disableDefaultUI, panControl: !1, zoomControl: !1, scaleControl: !1, styles: [{
    featureType: "water", elementType: "geometry", stylers: [{
        color: "#585858"
    }
, {
    lightness: 17
}
    ]
}
, {
    featureType: "landscape", elementType: "geometry", stylers: [{
        color: "#f5f5f5"
    }
, {
    lightness: 20
}
    ]
}
, {
    featureType: "road.highway", elementType: "geometry.fill", stylers: [{
        color: "#00bec1"
    }
, {
    lightness: 17
}
    ]
}
, {
    featureType: "road.highway", elementType: "geometry.stroke", stylers: [{
        color: "#00bec1"
    }
, {
    lightness: 29
}
, {
    weight: .2
}
    ]
}
, {
    featureType: "road.arterial", elementType: "geometry", stylers: [{
        color: "#00bec1"
    }
, {
    lightness: 18
}
    ]
}
, {
    featureType: "road.local", elementType: "geometry", stylers: [{
        color: "#00bec1"
    }
, {
    lightness: 16
}
    ]
}
, {
    featureType: "poi", elementType: "geometry", stylers: [{
        color: "#00bec1"
    }
, {
    lightness: 21
}
    ]
}
, {
    featureType: "poi.park", elementType: "geometry", stylers: [{
        color: "#dedede"
    }
, {
    lightness: 21
}
    ]
}
, {
    elementType: "labels.text.stroke", stylers: [{
        visibility: "on"
    }
, {
    color: "#ffffff"
}
, {
    lightness: 16
}
    ]
}
, {
    elementType: "labels.text.fill", stylers: [{
        saturation: 36
    }
, {
    color: "#333333"
}
, {
    lightness: 40
}
    ]
}
, {
    elementType: "labels.icon", stylers: [{
        visibility: "off"
    }
    ]
}
, {
    featureType: "transit", elementType: "geometry", stylers: [{
        color: "#00bec1"
    }
, {
    lightness: 19
}
    ]
}
, {
    featureType: "administrative", elementType: "geometry.fill", stylers: [{
        color: "#00bec1"
    }
, {
    lightness: 20
}
    ]
}
, {
    featureType: "administrative", elementType: "geometry.stroke", stylers: [{
        color: "#00bec1"
    }
, {
    lightness: 17
}
, {
    weight: 1.2
}
    ]
}
], draggable: !0
    }
, o = document.getElementById(mapId), t = new google.maps.Map(o, e);
    for (var i = 0; i < 3 ; i++) {
        marker = new google.maps.Marker({
            map: t, draggable: draggableMarker, icon: iconUrl, animation: google.maps.Animation.DROP, position: new google.maps.LatLng(items[i][0],items[i][1])
        }), google.maps.event.addListener(marker, "click", function () {
            console.log("aaaa");
        })
    }
}
function toggleBounce() {
    marker.setAnimation(null != marker.getAnimation() ? null : google.maps.Animation.BOUNCE)
}
var mapId = "googlemap",
mapName = "",
zoomLevel = 7,
iconUrl = new google.maps.MarkerImage("../public/img/map_icon.png", new google.maps.Size(50, 60), new google.maps.Point(0, 0), new google.maps.Point(0, 0), new google.maps.Size(37, 50)),
scrollable = !0,
disableDefaultUI = !1,

draggableMarker = !1, marker, map;
$(document).ready(function () {
    $("#googlemap").css("height", "700px"), $("#googlemap").css("width", "100%");
}), google.maps.event.addDomListener(window, "load", initialize);
