var logRecorderAndPlayer = (function () {

    function init() {
        bindAjaxSendFirst(function (event, xhr, options) {
            alert('first ajax');

            options.url = addLogRecorderIdToQueryString(options.url, 1337);
        });
        bindWindowUnloadFirst(function () {
            alert('first unload');
        });
    }

    ///#region Private methods   
    function bindAjaxSendFirst(fn) {
        $(document).ajaxSend(fn);
        moveLastEventToFirst(document, "ajaxSend");
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
    return publicMethods;
}());

logRecorderAndPlayer.init();
