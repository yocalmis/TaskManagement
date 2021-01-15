var test;
var takvims;
var myPlacemark;
var myMap;
$(function () {
    $(".content").scroll(function () {
        $(".arrow").css("display", "none");
    })
    /*$("input[name=bast]").val("");
    $("input[name=bit]").val("");*/
    /*mainpage e taşıdım
    ymaps.ready(init);   

     function init() {
        myMap = new ymaps.Map("map", {
            center: [38.727562, 35.480195],
            zoom: 7
        });
    };
    setTimeout(function () {
        haritayaPinEkle();
    }, 3000)*/
    bildirimGetir(0);
    /*$('.settings .notification').click(function () {
        $('.settings .notification ul').css("display", "block");
        $(".devamini-yukle").addClass("active");
    });*/
    $(window).click(function (e) {
        if (e.target != $('.settings .notification a i')[0]) {
            if (e.target != $(".devamini-yukle")[0]) {
                $('.settings .notification ul').css("display", "none");
                $(".devamini-yukle").removeClass("active");
            }
        }
        else {
            $('.settings .notification ul').css("display", "block");
            $(".devamini-yukle").addClass("active");
        }
    });
    $('[data-toggle="tooltip"]').tooltip();   
});
/*mainpage e taşıdım function haritayaPinEkle() {
    $.post("harita", "", function (data) {
        $.each(data.Message, function () {
            console.log(this);
            myPlacemark = new ymaps.Placemark([this["latitude"], this["longitude"]], { hintContent: this["date"], balloonContent: this["ad"] });
            myMap.geoObjects.add(myPlacemark);
        });
    })
}*/
function bildirimGetir(sayi) {
    $.post("bildirimlerim", { from: sayi }, function (data) {
        var text = "";
        var okunduCount = 0;

        if (data.IsSuccess == true) {
            $.each(data.Message, function () {
                //this["okundu"] okunmadı
                if (this["okundu"] == 1) {
                    text += '<li><a href="' + this["ilgili_url"] + '" class="unread" data-vid="' + this["vid"] + '">' + this["mesaj"] + '</a></li>';
                    okunduCount++;
                } else {
                    text += '<li><a href="' + this["ilgili_url"] + '" data-vid="' + this["vid"] + '">' + this["mesaj"] + '</a></li>';
                }
            })
        } else {

        }
        $(".settings .notification ul").append(text);
        if(okunduCount!=0){
            $(".settings .notification .bell span").html(okunduCount);
            $(".settings .notification .bell span").css("display", "block");
        }
        else {
            $(".settings .notification .bell span").css("display","none");
        }
        hoverKapat();
    });
}
$(document).on("click", ".devamini-yukle", function (e) {
    var sayi = $(".settings .notification ul li").length;
    bildirimGetir(sayi);
});
$(document).on("click", "#mesajList a", function (e) {
    e.preventDefault();
});
function hoverKapat() {
    $(".settings .notification ul a.unread").hover(function (e) {
        e.preventDefault();
        $(this).removeClass("unread");
        $.post("okundu", { vid: $(this).data("vid") }, function (data) {
            console.log(data);
        });
    });
}


//bilgehanın kodlar
function getTamamlanmaDurumText(durum)
{
    var durumStr = "";
    if (durum == 1)
    {
        durumStr = "Bekliyor";
    }
    else if (durum == 2)
    {
        durumStr = "Devam Ediyor";
    }
    else if (durum == 3)
    {
        durumStr = "Tamamlandı";
    }
    else if (durum == 4)
    {
        durumStr = "Pasif";
    }
    else if (durum == 5) {
        durumStr = "Önceki Görev Bekleniyor";
    }
    return durumStr;
}
function jsonDateToDate(jsonDate)
{
    var tarih = "";
    var date = new Date(parseInt(jsonDate.substr(6)));
    var d = date.getDate();
    if (d < 10) {
        d = "0" + d;
    }
    var m = date.getMonth();
    m += 1;  // JavaScript months are 0-11
    if (m < 10) {
        m = "0" + m;
    }
    var y = date.getFullYear();
    tarih = d + "." + m + "." + y;
    return tarih;
}
function jsonDateToDateForInputDate(jsonDate) {
    var tarih = "";
    var date = new Date(parseInt(jsonDate.substr(6)));
    var d = date.getDate();
    if (d < 10) {
        d = "0" + d;
    }
    var m = date.getMonth();
    m += 1;  // JavaScript months are 0-11
    if (m < 10) {
        m = "0" + m;
    }
    var y = date.getFullYear();
    tarih = y + "-" + m + "-" + d;
    return tarih;
}
function getProjeSurecString(tur){
    var turStr = "";
    if (tur == 1){
        turStr = "Proje";
    }
    else if (tur == 2){
        turStr = "Süreç";
    }
    else if (tur == 3) {
        turStr = "Görev";
    }
    return turStr;
}
function getProjeSurecClass(tur){
    var turStr = "";
    if (tur == null){
        return "file-text";
    }
    else if (tur == 1){
        turStr = "briefcase";
    }
    else if (tur == 2){
        turStr = "refresh";
    }
    return turStr;
}
function gorevIsmiEki(projeSurec, tarih) {
    var turStr = "";
    if (projeSurec == 2) {
        turStr = "(" + jsonDateToDate(tarih) + ")";
    }
    return turStr;
}
function ifNullReturnEmpty(object) {
    if (object == null) {
        return "";
    }
    return object;
}
$(document).on("click", ".dashboard-tabs .tab-list a.item", function (e) {
    e.preventDefault();
    $(".dashboard-tabs .tab-list a.item").removeClass("active");
    $(this).addClass("active");
    var tabName = $(this).data("tab");
    console.log($("#" + tabName).addClass("active"));
    $(".dashboard-tabs .list .tab-cont .ic-div").removeClass("active");
    $("#" + tabName).addClass("active");
    $(".dashboard-tabs .list .proje-list").html("");
});
$(document).on("click", ".arrow", function (e) {
    e.preventDefault();
    var body = $(".content");
    body.stop().animate({ scrollTop: $(".dashboard-tabs").offset().top - 40 }, 500, 'swing');
    $(".arrow").css("display", "none");
});
function loadingAc()
{
    $("#loading").css({ 'display': "block" });
}
function loadingKapat()
{
    $("#loading").css({ 'display': "none" });
}
$(document).on("click", ".ust-filtre .btn-second", function (e) {
    e.preventDefault();
    if ($(".ust-filtre .btn-second").html() == "Tarihi Kapat") { 
        $(".ust-filtre .btn-second").html("Tarih Filtrele");
        $("input[name=bast]").val("");
        $("input[name=bit]").val("");
    } else {
        //tarih burda yazılacak
        /*
        var dt=new Date();
        dt.yyyymmdd();
        */
        var dt = new Date();
        $("input[name=bast]").val(dt.yyyymmdd());
        dt.setMonth(dt.getMonth()+1);
        $("input[name=bit]").val(dt.yyyymmdd());
        $(".ust-filtre .btn-second").html("Tarihi Kapat");
    }
    $(".tarih-listesi").toggleClass("active");
    $(".ust-filtre").toggleClass("active");
});
var orderType = "bitis_tarihi";
var desc = "";
function setOrderType(ord) {
    if (desc == "") {
        desc = "desc";
    }
    else {
        desc = "";
    }
    orderType = ord;
    filtrele();
}
function exportTable(tableName, baslangicTarihi, bitisTarihi, durum, order, descc, tur, rootUrl)
{
    $.ajax({
        url: rootUrl + "exportTable",
        data: { tableName: tableName, baslangicTarihi: baslangicTarihi, bitisTarihi: bitisTarihi, durum: durum, order: order, descc: descc, tur:tur },
    dataType: "json",
    type: "POST",
    success: function (data) {
        console.log(data);
    }
});
}
Date.prototype.yyyymmdd = function () {
    var mm = this.getMonth() + 1; // getMonth() is zero-based
    var dd = this.getDate();

    return [this.getFullYear(),
            (mm > 9 ? '' : '0')+"-" + mm,
            (dd > 9 ? '' : '0') + "-" + dd
    ].join('');
};





$.fn.extend({
    animateCss: function (animationName, callback) {
        var animationEnd = (function (el) {
            var animations = {
                animation: 'animationend',
                OAnimation: 'oAnimationEnd',
                MozAnimation: 'mozAnimationEnd',
                WebkitAnimation: 'webkitAnimationEnd',
            };

            for (var t in animations) {
                if (el.style[t] !== undefined) {
                    return animations[t];
                }
            }
        })(document.createElement('div'));

        this.addClass('animated ' + animationName).one(animationEnd, function () {
            $(this).removeClass('animated ' + animationName);

            if (typeof callback === 'function') callback();
        });

        return this;
    },
});
$(document).on("click", "#burger-menu", function (e) {
    e.preventDefault();
    $("#burger-menu").toggleClass("active");
    $(".sidebar").toggleClass("active");
})