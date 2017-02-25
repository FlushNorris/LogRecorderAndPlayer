var logRecorderAndPlayer = (function () {

    var handlerLRAPUrl = "/logrecorderandplayerhandler.lrap";

    function setPageGUID(_pageGUID) {
        var v = getPageGUID();
        if (v == null) {
            var $frm = getPrimaryForm();
            $("<input type='hidden' />")
                .attr("class", "LRAP-PAGEGUID")
                .attr("name", "LRAP-PAGEGUID")
                .val(_pageGUID)
                .appendTo($frm);
        }
    }

    function getPageGUID() {
        var $frm = getPrimaryForm();
        var $pageGUID = $frm.find(".LRAP-PAGEGUID");
        if ($pageGUID.size() != 0)
            return $pageGUID.val();
        return null;
    }

    function setSessionGUID(_sessionGUID) {
        var v = getSessionGUID();
        if (v == null) {
            var $frm = getPrimaryForm();
            $("<input type='hidden' />")
                .attr("class", "LRAP-SESSIONGUID")
                .attr("name", "LRAP-SESSIONGUID")
                .val(_sessionGUID)
                .appendTo($frm);
        }
    }

    function getSessionGUID() {
        var $frm = getPrimaryForm();
        var $sessionGUID = $frm.find(".LRAP-SESSIONGUID");
        if ($sessionGUID.size() != 0)
            return $sessionGUID.val();
        return null;
    }    

    function getPrimaryForm() {
        var __EVENTTARGET = $("#__EVENTTARGET");
        return __EVENTTARGET.closest("form");
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
            if (options.LRAPCall) { //To avoid recurrency when logging via LRAP
                return;
            }
            options.lrapSessionGUID = getSessionGUID();
            options.lrapPageGUID = getPageGUID();
            options.lrapBundleGUID = generateGUID();
            options.lrapOrigURL = options.url;

            logElement(options.lrapSessionGUID, options.lrapPageGUID, options.lrapBundleGUID, null, new Date(), LogType.OnAjaxRequestSend, options.lrapOrigURL, JSON.stringify(options.data));
            options.url = getHandlerUrlForLogging(options.url, options.lrapSessionGUID, options.lrapPageGUID, options.lrapBundleGUID);

            //Ja, det er faktisk et ret stort tab... vi kan ikke simulere tid og dato som det var "dengang" det fejlede. Det skal beskrives i rapporten
        });

        bindAjaxCompleteLast(function (event, xhr, options) {
            if (options.LRAPCall) { //To avoid recurrency when logging via LRAP
                return;
            }
            var sessionGUID = options.lrapSessionGUID;
            var pageGUID = options.lrapPageGUID;
            var bundleGUID = options.lrapBundleGUID;

            logElement(sessionGUID,
                pageGUID,
                bundleGUID,
                null,
                new Date(),
                LogType.OnAjaxResponseReceived,
                options.lrapOrigURL,
                JSON.stringify(xhr));

            //alert(options.complete);

            //xhr.responseText = "???";
            //alert(JSON.stringify(xhr));

            //xhr...
            //error:
            //readyState = 4
            //responseText = some error text... embedded in a lot of html
            //status = 500
            //statusText = "Internal Server Error"


            //alert(event ? JSON.stringify(event) : "false???");

            //alert('ajax complete... log this '+bundleGUID);
        });        

        bindWindowUnloadFirst(function () {
            finalizeLogger();                        
        });

        bindWindowBeforeClose(function() {
            finalizeLogger();
        });
       
        setupLogger();
    }

    var logElements = [];
    var logElementsTimer = null;

    function setupLogger() {
        //logElementsTimer = setTimeout(function () {
        //    logElementsTimer = null;
        //    handleLogElements();
        //    setupLogger();
        //}, 10000);
    }

    function finalizeLogger() {
//        if (logElementsTimer != null) {
//            clearTimeout(logElementsTimer);
            logElementsTimer = null;
            handleLogElements();
//        }
    }

    function handleLogElements() {
        var logElementsForHandler = logElements;
        logElements = [];

        if (logElementsForHandler.length > 0) {
            $.ajax({
                LRAPCall: true,
                type: "POST",
                url: handlerLRAPUrl,
                data: {
                    'request': JSON.stringify(logElementsForHandler)
                },
                //complete: function (event, xhr, options) {
                //    alert('log complete : '+JSON.stringify(xhr));
                //},
                success: function (data) {
                    //OKIDOKI
                },
                error: function(data) {
                    logElements = logElementsForHandler.concat(logElements); //Put elements back in queue
                    alert('Failed to log: Contact administrator');
                }
            });
        }
    }

    function logElement(sessionGUID, pageGUID, bundleGUID, progressGUID, timestamp, logType, element, value) {
        var request = {
            GUID: generateGUID(),
            SessionGUID: sessionGUID,
            PageGUID: pageGUID,
            BundleGUID: bundleGUID,
            ProgressGUID: progressGUID,
            Timestamp: getDataMemberDate(timestamp),
            LogType: logType,
            Element: htmlEncode(element), //denne burde html encodes (eller faktisk burde den kun html encodes når det ikke er status=200... hmmm... er jo heller ikke holdbart
            Value: value
        };
        logElements.push(request);        
    }

    //from http://www.freeformatter.com/epoch-timestamp-to-date-converter.html
    //convertToEpoch: function() {

    //    var d = new Date();
    //    if ($('#timeZone').val() == 'gmt') {
    //        d.setUTCFullYear(converter.getValue('year'));
    //        d.setUTCMonth(converter.getValue('month') -1);
    //        d.setUTCDate(converter.getValue('date'));
    //        d.setUTCHours(converter.getValue('hours'));
    //        d.setUTCMinutes(converter.getValue('minutes'));
    //        d.setUTCSeconds(converter.getValue('seconds'));					
    //    } else {
    //        d.setFullYear(converter.getValue('year'));
    //        d.setMonth(converter.getValue('month') -1);
    //        d.setDate(converter.getValue('date'));
    //        d.setHours(converter.getValue('hours'));
    //        d.setMinutes(converter.getValue('minutes'));
    //        d.setSeconds(converter.getValue('seconds'));
    //    }
				
    //    $('#resultEpoch').text(parseInt(d.getTime() / 1000));
    //    $('#result').show();
			
    //}

    function convertStringToDateObject(dateStr) {
        var dateObj = null;
        if (this.isDate(dateStr)) {
            var rxDatePattern = /^(\d{1,2})(\/|-)(\d{1,2})(\/|-)(\d{4})$/;
            var dtArray = dateStr.match(rxDatePattern); // is format OK?

            dateObj = new Date(+dtArray[5], +dtArray[3] - 1, +dtArray[1]);
        }
        return dateObj;
    }

    function getDataMemberDate(dateStr) {
        var dateObj = undefined;
        if (typeof dateStr === 'string' || dateStr instanceof String)
            dateObj = this.convertStringToDateObject(dateStr);
        else
            dateObj = dateStr;
        return dateObj ? '/Date(' + Date.UTC(dateObj.getFullYear(), dateObj.getMonth(), dateObj.getDate(), dateObj.getHours(), dateObj.getMinutes(), dateObj.getSeconds(), dateObj.getMilliseconds()) + ')/' : null;
    }

    function htmlEncode(value) {
        if (!!value) 
            return $('<div></div>').text(value).html();
        return "";        
    }

    function htmlDecode(value) {
        return $('<div></div>').html(value).text();
    }

    function generateGUID() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
    }

    function getHandlerUrlForLogging(url, sessionGUID, pageGUID, bundleGUID) {
        var sessionGUIDTag = "lrap-sessionguid";
        var pageGUIDTag = "lrap-pageguid";
        var bundleGUIDTag = "lrap-bundleguid";

        url = addQryStrElement(removeQryStrElement(url, sessionGUIDTag), sessionGUIDTag, sessionGUID);
        url = addQryStrElement(removeQryStrElement(url, pageGUIDTag), pageGUIDTag, pageGUID);
        url = addQryStrElement(removeQryStrElement(url, bundleGUIDTag), bundleGUIDTag, bundleGUID);
        return url;
    }

    function setupAjaxEvents(type) {
        var events = $._data(document, 'events')[type];
        if (typeof (events) == "undefined")
            return;

        $.each(events, function(idx, event) {
            var f = event.handler;
            event.handler = function (event, xhr, options) {
                //alert(type + ' : ' + JSON.stringify(options));
                if (!options.LRAPCall) { //only call existing ajax events when we are not doing log via LRAP
                    f(event, xhr, options);
                }
            };
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

    //function bindWindowBeforeClose(fn) {
    //    window.onbeforeclose = fn;
    //    //$(window).beforeclose(fn);
    //    //moveLastEventToFirst(window, "unload");
    //}

    function bindWindowBeforeClose(fn) {
        window.addEventListener("beforeunload", function (e) {
            fn();

            (e || window.event).returnValue = null;
            return null;
        });
    }


    function moveLastEventToFirst(elm, action) {
        var handlers = $._data(elm, 'events')[action];
        var handler = handlers.pop();
        handlers.splice(0, 0, handler);
    }

    function getQryStrElement(url, tag) {
        var regEx = new RegExp("[?&]" + tag + "=([^\\Z$?&]*)[\\Z$?&]*");
        var m = regEx.exec(url);
        if (m != null) return m[1];       
        return null;
    }

    function removeQryStrElement(url, tag) {
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

    ///#endregion

    var publicMethods = {};

    publicMethods.init = init;
    publicMethods.setPageGUID = setPageGUID;
    publicMethods.getPageGUID = getPageGUID;
    publicMethods.setSessionGUID = setSessionGUID;
    publicMethods.getSessionGUID = getSessionGUID;
    
    return publicMethods;
}());

logRecorderAndPlayer.init();
