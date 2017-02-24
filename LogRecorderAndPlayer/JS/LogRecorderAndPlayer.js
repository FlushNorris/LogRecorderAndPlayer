var logRecorderAndPlayer = (function () {

    var handlerLRAPUrl = "/logrecorderandplayerhandler.lrap";
    var pageGUID = null;

    function setPageGUID(_pageGUID) {
        pageGUID = _pageGUID;
    }

    var LogType =
    {
        OnAjaxRequestSend: 0,
        OnAjaxRequestReceived: 1,
        OnAjaxResponseSend: 2,
        OnAjaxResponseReceived: 3,
        OnBlur: 4,
        OnFocus: 5
    };

    function init() {
        setupAjaxEvents('ajaxSend');
        setupAjaxEvents('ajaxComplete');

        bindAjaxSendFirst(function (event, xhr, options) {
            if (options.LRAPCall) { //To avoid recurrency
                return;
            }
            alert('ajax send... log this');
            options.url = addLogRecorderIdToQueryString(options.url, 1337); //1337-id'et kommer fra sidens qry-string ..... eller dvs, hvis den ikke er i qry-stringen, så skal den overføres via JS
            doStuff(1337, options.url); //timestamp nytter jo ikke noget at det er clientside tid, men hvordan skal jeg så få den rigtige frem? En løsningen ville jo være at overføre serverens tid 

            //Ja, det er faktisk et ret stort tab... vi kan ikke simulere tid og dato som det var "dengang" det fejlede. Det skal beskrives i rapporten
        });
        bindAjaxCompleteLast(function(event, xhr, options) {
            alert('ajax complete... log this');
        });        

        bindWindowUnloadFirst(function () {
            alert('first unload');
        });
    }

    function setupAjaxEvents(type) {
        var events = $._data(document, 'events')[type];
        if (typeof (events) == "undefined")
            return;

        $.each(events, function(idx, event) {
            var f = event.handler;
            event.handler = function(event, xhr, options) {
                if (!options.LRAPCall) {
                    f(event, xhr, options);
                }
            };
        });
    
        //$._data(document, 'events')['ajaxSend'] == undefined
    }

    function logElement(sessionGUID, pageGUID, bundleGUID, progressGUID, timestamp, logType, element) {
        var request = {
            SessionGUID: sessionGUID,
            PageGUID: pageGUID,
            BundleGUID: bundleGUID,
            ProgressGUID: progressGUID,
            Timestamp: timestamp,
            LogType: logType,
            Element: element
        };

        $.ajax({
            LRAPCall: true,
            type: "POST",
            url: handlerLRAPUrl,
            data: {
                'request': JSON.stringify(request)
            },
            complete: function () {
                alert('log complete');
            },
            success: function (data) {
                alert('log success');
            },
            error: function(data) {
                alert('log error');
            }
        });
    }

    ///#region Private methods   
    function bindAjaxSendFirst(fn) {
        $(document).ajaxSend(fn);
        moveLastEventToFirst(document, "ajaxSend");
    }

    function bindAjaxCompleteLast(fn) {
        $(document).ajaxComplete(fn);
    }

    function bindWindowUnloadFirst(fn) {
        $(window).unload(fn);
        moveLastEventToFirst(window, "unload");
    }

    function moveLastEventToFirst(elm, action) {
        var handlers = $._data(elm, 'events')[action];
        var handler = handlers.pop();
        handlers.splice(0, 0, handler);
    }

    function removeQryStrElement(url, tag) {
        //    var regEx = new RegExp("[?&]" + tag + "=\\d+[\\Z$?&]*");
        //var regEx = new RegExp("[?&]" + tag + "=.+[\\Z$?&]+");
        var regEx = new RegExp("[?&]" + tag + "=[^\\Z$?&]*[\\Z$?&]*");
        var m = regEx.exec(url);
        if (m != null) {
            var s = m[0];
            var sep = s[s.length - 1];
            return url.slice(0, m.index) + (sep == "&" ? s[0] + url.slice(m.index + s.length) : "");
        }
        return url;
    }

    function addQryStrElement(url, tag, value) {
        return url + (url.indexOf("?") == -1 ? "?" : "&") + tag + "=" + value;
    }

    function addLogRecorderIdToQueryString(url, lrId) {
        var tag = "logrecorderid";
        return addQryStrElement(removeQryStrElement(url, tag), tag, lrId);
    }

    ///#endregion

    var publicMethods = {};
    publicMethods.init = init;
    publicMethods.setPageGUID = setPageGUID;
    return publicMethods;
}());

logRecorderAndPlayer.init();
