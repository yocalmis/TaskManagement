var test;

$(function (){
    
});
$(document).on("click", ".resim-onizleme a", function (e) {
    e.preventDefault();
    $("#sayfa_form #imageUpload").val("");
    $(".resim-onizleme").removeClass("active");
});
$(document).on("change", "#sayfa_form #imageUpload", function () {
    $(".resim-onizleme").addClass("active");
})
$(document).on("change", "#files", function () {
    $("#temp_form").submit();
})



//bilgehanýn kodlar
$(document).on("change", "#xFilePath", function () {
    $('#imgSelected').attr('src', $("#xFilePath").val());
    $("#imgSelected").css("display", "block");
})
function BrowseServer(inputId) {
    var finder = new CKFinder();
    finder.BasePath = '/ckfinder/';
    finder.SelectFunction = SetFileField;
    finder.SelectFunctionData = inputId;
    finder.Popup();
}
function SetFileField(fileUrl, data) {
    $('#xFilePath').val(fileUrl);
    $('#imgSelected').attr('src', fileUrl);
    $("#imgSelected").css("display", "block");
}

$(document).on("change", "#xFilePath2", function () {
})
function BrowseServer2(inputId) {
    var finder = new CKFinder();
    finder.BasePath = '/ckfinder/';
    finder.SelectFunction = SetFileField2;
    finder.SelectFunctionData = inputId;
    finder.Popup();
}
function SetFileField2(fileUrl, data) {
    $('#xFilePath2').val(fileUrl);
}
