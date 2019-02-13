Array.prototype.last = function () { return this[this.length - 1]; }


var CollectOtherVideo = [];
function PushUrls(urls) {
    for (var i = 0; i < urls.length; i++) {
        if (CollectOtherVideo.findIndex(x => x.VideoId == urls[i].VideoId) == -1)
            CollectOtherVideo.push(urls[i]);
    }
}

function GotoButton() {
    //$("html, body").animate({ scrollTop: $(document).height() + $(window).height() });
    //if ($(document).height() - $(window).height() > $(window).scrollTop()) {
    //    $("html, body").animate({ scrollTop: $(document).height() + $(window).height() });
    //    window.setTimeout(function (e) { GotoButton(); }, 3000);
    //}
    //else {
    CheckUrl();
    SaveData();
    //}
}

function LoadVideoUrl() {
    GotoButton();
}


function SaveData() {
    var root = window.location.href.split('/').last().toUpperCase();
    var name = $(".title img").attr("alt") || $(".title .text").text();
    var postData = {
        Urls: CollectOtherVideo,
        Season: null,
        MasterRootVideo: root,
        Name: name
    };
    // CORS policy: No 'Access-Control-Allow-Origin' header is present on the 
    // when CORS policy activated on domain , we can just sent get method to other domain , post method blocked
    $.ajax({
        "async": true,
        "crossDomain": true,
        url: "https://localhost:44373/API/Values/CollectVideo", method: 'get', "headers": {
            "Content-Type": "application/x-www-form-urlencoded",
            "cache-control": "no-cache"
        }, data: { dataText: encodeURIComponent(JSON.stringify(postData)) }
        // success event did not worked when  CORS policy activated
        //, success: function (e) {
        //    //console.log(e);
        //    //LoadOtherSeason();
        //    console.log(0);
        //},
        , complete: function (e) {
            CollectOtherVideo = [];
            LoadNextPage(false);
        }
    });
}
function CheckUrl() {
    var format = "https://www.Netflix.com";
    var urls = $("a[href*='/watch/']").map(function (e, v) {
        return $(v)[0].pathname.split('/').last();
    }).get();

    PushUrls(urls);
}



function LoadNextPage(notfound) {
    var currentId = localStorage.getItem("currentVideo");
    var url = "";
    if (currentId == null)
        url = "https://localhost:44373/api/values/GetNextVideo?notfound=" + notfound;
    else
        url = "https://localhost:44373/api/values/GetNextVideo?id=" + currentId + "&notfound=" + notfound;
    var settings = {
        "async": true,
        "crossDomain": true,
        "url": url,
        "method": "GET",
        "headers": {
            "Content-Type": "application/x-www-form-urlencoded",
            "cache-control": "no-cache"
        }
    }

    $.ajax(settings).done(function (response) {
        localStorage.setItem("currentVideo", response);
        console.log("redirecting to " + "https://www.Netflix.com/title/" + response);
        window.location = "https://www.Netflix.com/title/" + response;

    });
}



var DisableAllLoadPage = false;

function InitialPage() {


    var enablePaging = localStorage.getItem("EnablePagin");
    if (enablePaging != undefined) {
        console.log("video pageing dsabled try again after 10 second");
        window.setTimeout(function (e) { InitialPage(); }, 10000);
        return;
    }

    if ($(".Episodes").length == 0 && window.location.href.indexOf("/title/") != -1) {

        var currentId = window.location.href.split('/').last();
        localStorage.setItem("currentVideo", currentId);
        LoadVideoUrl();
    }
    else {

        var pageNotfound = window.location.pathname.toLowerCase() === "/browse";
        if (pageNotfound)
            console.log("page not found after 10 second new page loaded");
        else console.log("video dosenot have episode loading new page loaded");
        LoadNextPage(pageNotfound);

    }
}
InitialPage();
window.setInterval(function (e) {
    location.reload();
}, 300000);
