function mapInit(e){if(null==e)var e="39.995299, 32.699498";e=e.split(",");var o=[{featureType:"water",stylers:[{visibility:"on"},{color:"#b5cbe4"}]},{featureType:"landscape",stylers:[{color:"#efefef"}]},{featureType:"road.highway",elementType:"geometry",stylers:[{color:"#83a5b0"}]},{featureType:"road.arterial",elementType:"geometry",stylers:[{color:"#bdcdd3"}]},{featureType:"road.local",elementType:"geometry",stylers:[{color:"#ffffff"}]},{featureType:"poi.park",elementType:"geometry",stylers:[{color:"#e3eed3"}]},{featureType:"administrative",stylers:[{visibility:"on"},{lightness:33}]},{featureType:"road"},{featureType:"poi.park",elementType:"labels",stylers:[{visibility:"on"},{lightness:20}]},{},{featureType:"road",stylers:[{lightness:20}]},{featureType:"road.arterial",elementType:"labels",stylers:[{visibility:"off"}]},{featureType:"poi",elementType:"geometry",stylers:[{visibility:"off"}]},{featureType:"poi.park",elementType:"geometry",stylers:[{visibility:"on"}]}],t={zoom:13,center:new google.maps.LatLng(e[0],e[1]),mapTypeId:google.maps.MapTypeId.ROADMAP,styles:o,scrollwheel:!1,navigationControl:!1,scaleControl:!1,draggable:!1,mapTypeControl:!0,mapTypeControlOptions:{style:google.maps.MapTypeControlStyle.HORIZONTAL_BAR,position:google.maps.ControlPosition.BOTTOM_RIGHT},zoomControl:!0,zoomControlOptions:{style:google.maps.ZoomControlStyle.LARGE,position:google.maps.ControlPosition.RIGHT_CENTER}},l=new google.maps.Map(document.getElementById("map"),t),a="../img/mappin.png",r=new google.maps.Marker({position:new google.maps.LatLng(e[0],e[1]),map:l,draggable:!1,animation:google.maps.Animation.DROP,icon:a});google.maps.event.addListener(r,"click",toggleBounce)}function toggleBounce(){marker.setAnimation(null!=marker.getAnimation()?null:google.maps.Animation.BOUNCE)}$(function(){mapInit()});