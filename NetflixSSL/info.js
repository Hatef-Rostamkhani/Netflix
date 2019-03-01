Array.prototype.last = function () { return this[this.length - 1]; }
var format = "https://Netflix.com/watch/";
var currentCategory = window.location.href.split('/').last().toUpperCase();
var textfile = $(".infopop").map(function (e, v) { return format + $(v).attr("href").split("/").last() }).get().join("\r\n");
if (textfile.length > 0) {
    var anchor = document.querySelector('a');
    anchor.onclick = function () {
        anchor.href = 'data:text/plain;charset=utf-8,' + encodeURIComponent(textfile);
        anchor.download = 'Netflix_' + currentCategory + '.txt';
    };
    anchor.click();
}
window.setTimeout(function (e) { close(); }, 5000);


var nextURL = $(".datemenu a:contains('" + currentCategory + "')").next().attr("href")
if (nextURL !== undefined)
    window.location = nextURL;



// open all tabs not worked get Internal Server Error
//if(localStorage.getItem("callAll") == null){
//localStorage.setItem("callAll", "true");
//$(".datemenu a:not(:contains('"+currentCategory+"'))").each(function(i,v)
//{
//    window.open($(v).attr("href"),"_blank")
//});
//}