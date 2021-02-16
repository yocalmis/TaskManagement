$.fn.isOnSc = function() {
    var $elem = this;
    var $window = $(window);

    var docViewTop = $window.scrollTop();
    var docViewBottom = docViewTop + $window.height();

    var elemTop = $elem.offset().top;
    var elemBottom = elemTop + $elem.height();
	
    return ((elemBottom <= docViewBottom) && (elemTop >= docViewTop));
  }