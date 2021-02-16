var test;
var takvims;
var myPlacemark;
var myMap;
$(function () {
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
function getProjeSurecString(tur){
    var turStr = "";
    if (tur == 1){
        turStr = "Proje";
    }
    else if (tur == 2){
        turStr = "Süreç";
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