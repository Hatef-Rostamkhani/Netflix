Array.prototype.last = function () { return this[this.length - 1]; }


var SeasonUrls = [];
function PushUrls(urls) {
    for (var i = 0; i < urls.length; i++) {
        if (SeasonUrls.findIndex(x => x.VideoId == urls[i].VideoId) == -1)
            SeasonUrls.push(urls[i]);
    }
}

function LoadVideoUrl() {
    CheckUrl();

    if (SeasonUrls.length == 0)
        window.setTimeout(function (e) { LoadVideoUrl() }, 500);
    var currentSeason = $(".episodesContainer .nfDropDown .label").text() || $(".episodesContainer .single-season-label").text();
    var root = window.location.href.split('/').last().toUpperCase();
    var name = $(".title img").attr("alt") || $(".title .text").text();
    var postData = {
        Urls: SeasonUrls,
        Season: currentSeason,
        MasterRootVideo: root,
        Name: name
    };
    console.log(postData);
    console.log(JSON.stringify(postData));
    // CORS policy: No 'Access-Control-Allow-Origin' header is present on the 
    // when CORS policy activated on domain , we can just sent get method to other domain , post method blocked
    $.ajax({
        "async": true,
        "crossDomain": true,
        url: "https://localhost:44373/API/Values/ProcessVideo", method: 'get', "headers": {
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
            SeasonUrls = [];
            LoadOtherSeason();
        }
    });
}

function CheckUrl() {
    var format = "https://www.netflix.com";
    var urls = $(".episodeWrapper a[href*='/watch/']").map(function (e, v) {
        return {
            VideoId: $(v)[0].pathname.split('/').last(),
            FullUrl: format + $(v).attr("href")
        }
    }).get();

    PushUrls(urls);
}

function loadAllVideoFromSeason() {
    window.setTimeout(function (e) {
        var length = $(".episodeWrapper [class='handle handleNext active']").length;
        if (length > 0) {
            CheckUrl();
            $(".episodeWrapper [class='handle handleNext active']").click();
            window.setTimeout(function (e) { loadAllVideoFromSeason() }, 500);
        }
        else {
            CheckUrl();
            window.setTimeout(function (e) { LoadVideoUrl() }, 500);
        }
    }, 1000);
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
        console.log("redirecting to " + "https://www.netflix.com/title/" + response);
        window.location = "https://www.netflix.com/title/" + response;
        //window.setTimeout(function (e) { window.location = "https://www.netflix.com/title/" + response; }, 10000);

    });
}

// load other season if exits
function LoadOtherSeason() {
    if ($(".episodesContainer .nfDropDown div").length > 0) {
        $(".episodesContainer .nfDropDown div").trigger("click"); // open combobox
        var currentSeason = $(".episodesContainer .nfDropDown .label").text();
        var findedA = $(".episodesContainer .nfDropDown div .sub-menu-list .sub-menu-item:has(a:contains('" + currentSeason + "'))").next().find("a");
        if (findedA[0] != undefined) {
            findedA[0].click();
            window.setTimeout(function (e) { loadAllVideoFromSeason() }, 500);
        }
        else {
            LoadNextPage(false);
        }
    }
    else {
        LoadNextPage(false);
    }
}

var DisableAllLoadPage = false;

function InitialPage() {


    var enablePaging = localStorage.getItem("EnablePagin");
    if (enablePaging != undefined) {
        console.log("video pageing dsabled try again after 10 second");
        window.setTimeout(function (e) { InitialPage(); }, 10000);
        return;
    }

    if ($(".Episodes").length > 0) {
        $(".Episodes").trigger("click");
        loadAllVideoFromSeason();
        var currentId = window.location.href.split('/').last();
        localStorage.setItem("currentVideo", currentId);
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
