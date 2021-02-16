var test;
var int = 0;
$(function () {
    $("input[type=range]").change(function () {
        console.log($(this).val());
        if ($(this).val() < 40) {
            $(".yorum-faces").css("background-position","-300px 0");
        }
        else if ($(this).val() > 39 && $(this).val() < 59) {
            $(".yorum-faces").css("background-position","-150px 0");
        }
        else if ($(this).val() > 60) {
            $(".yorum-faces").css("background-position","0px 0");
        }
    });

    //input show :)
    $('input').focus(function () {
        selectedInput = $(this).attr('placeholder');
        $(this).attr('placeholder', '')
    });
    $('input').focusout(function () {
        $(this).attr('placeholder', selectedInput);
    });
    $('textarea').focus(function () {
        selectedInput = $(this).attr('placeholder');
        $(this).attr('placeholder', '')
    });
    $('textarea').focusout(function () {
        $(this).attr('placeholder', selectedInput);
    });
    //input showover
});
function hatali() {
    $(".loginform").addClass("wrong");
    $(".loginform span").html("Yanlış kullanıcı adı veya şifre girdiniz lütfen tekrar deneyiniz.");
    $(".loginform span").addClass("active");
}
function dogru() {
    $(".loginform").removeClass("wrong");
    $(".loginform").addClass("correct");
    $(".loginform span").html("Tebrikler! Sisteme giriliyor lütfen bekleyiniz.");
    $(".loginform span").addClass("active");
    setTimeout(function () { window.location.href = "../Admin/AHome/Index"; }, 1000);
}



$(document).on("change", "#nereden,#nereye", function (e) {
    e.preventDefault();
    //console.log($(this).val());
    var ilgiliId = $(this).data('gidecek');
    $.post('/Home/ilceler/', { "id": $(this).val() }, function (data) {
        $(ilgiliId).html('');
        $.each(data, function () {
            $(ilgiliId).append('<option value="' + this.id + '">' + this.ilceAdi + '</option>');
        })
        //<option value="1"></option>
    });
})