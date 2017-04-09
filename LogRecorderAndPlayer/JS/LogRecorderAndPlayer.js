var logRecorderAndPlayer = (function () {

    var handlerLRAPUrl = "/logrecorderandplayerhandler.lrap";

    var pageGUIDCache = null;
    var sessionGUIDCache = null;
    var serverGUIDCache = null;

    var GUIDTag = "lrap-guid";
    var SessionGUIDTag = "lrap-sessionguid";
    var PageGUIDTag = "lrap-pageguid";
    var BundleGUIDTag = "lrap-bundleguid";
    var ServerGUIDTag = "lrap-serverguid"; //For the namedpipe-connection to the LogPlayer
    var LogElementGUID = null;

    function pushLogElementGUID(logElementGUID) {
        alert('pushing ' + logElementGUID);
        LogElementGUID = logElementGUID;
    }

    function popLogElementGUID() {
        var r = LogElementGUID;
        LogElementGUID = null;
        return r;
    }

    function unixTimestamp(dt) { //seconds since 1970/1/1
        if (typeof (dt) == "undefined" || dt == null)
            dt = new Date();
        return dt.getTime() / 1000.0;
    }    

    var clientTimeOffset = 0;

    //function unitTimeStampByOffset() {
    //    return unixTimestamp() + clientTimeOffset;
    //}

    function setPageGUID(_pageGUID) {
        var v = getPageGUID();
        if (v == null) {
            pageGUIDCache = _pageGUID;
            var $frm = getPrimaryForm();
            $("<input type='hidden' />")
                .attr("class", PageGUIDTag)
                .attr("name", PageGUIDTag)
                .val(_pageGUID)
                .appendTo($frm);
        }
    }

    function getPageGUID() {
        if (pageGUIDCache != null)
            return pageGUIDCache;

        var $frm = getPrimaryForm();
        var $pageGUID = $frm.find("." + PageGUIDTag);
        if ($pageGUID.size() != 0)
            pageGUIDCache = $pageGUID.val();
        return pageGUIDCache;
    }

    function setServerGUID(_serverGUID) {
        var v = getServerGUID();
        if (v == null) {
            serverGUIDCache = _serverGUID;
            var $frm = getPrimaryForm();
            $("<input type='hidden' />")
                .attr("class", ServerGUIDTag)
                .attr("name", ServerGUIDTag)
                .val(_serverGUID)
                .appendTo($frm);
        }
    }

    function getServerGUID() {
        if (serverGUIDCache != null)
            return serverGUIDCache;

        var $frm = getPrimaryForm();
        var $serverGUID = $frm.find("." + ServerGUIDTag);
        if ($serverGUID.size() != 0)
            serverGUIDCache = $serverGUID.val();
        return serverGUIDCache;
    }

    function isPlaying() {
        var r = getServerGUID();
        return (r != null && r != "");
    }

    function setSessionGUID(_sessionGUID) {
        var v = getSessionGUID();
        if (v == null) {
            sessionGUIDCache = _sessionGUID;
            var $frm = getPrimaryForm();
            $("<input type='hidden' />")
                .attr("class", SessionGUIDTag)
                .attr("name", SessionGUIDTag)
                .val(_sessionGUID)
                .appendTo($frm);
        }
    }

    function getSessionGUID() {
        if (sessionGUIDCache != null)
            return sessionGUIDCache;

        var $frm = getPrimaryForm();
        var $sessionGUID = $frm.find("." + SessionGUIDTag);
        if ($sessionGUID.size() != 0) 
            sessionGUIDCache = $sessionGUID.val();        
        return sessionGUIDCache;
    }    

    function getPrimaryForm() {
        var __EVENTTARGET = $("#__EVENTTARGET");
        return __EVENTTARGET.closest("form");
    }

    var LogType =
    {
        OnHandlerRequestSend: 0,
        OnHandlerRequestReceived: 1,
        OnHandlerResponseSend: 2,
        OnHandlerResponseReceived:3,
        OnBlur: 4,
        OnFocus: 5,
        OnChange: 6,
        OnSelect: 7,
        OnCopy: 8,
        OnCut: 9,
        OnPaste: 10,
        OnKeyDown: 11,
        OnKeyUp: 12,
        OnKeyPress: 13,
        OnMouseDown: 14,
        OnMouseUp: 15,
        OnClick: 16,
        OnDblClick: 17,
        OnSearch: 18,
        OnResize: 19,
        OnDragStart: 20,
        OnDragEnd: 21,
        OnDragOver: 22,
        OnDrop: 23,
        OnScroll: 24,
        OnPageRequest: 25,
        OnPageResponse: 26,
        OnPageSessionBefore: 27,
        OnPageSessionAfter: 28,
        OnPageViewStateBefore: 29,
        OnPageViewStateAfter: 30,
        OnWCFServiceRequest: 31,
        OnWCFServiceResponse: 32,
        OnDatabaseRequest: 33,
        OnDatabaseResponse: 34,
        OnHandlerSessionBefore: 35,
        OnHandlerSessionAfter: 36
    }; //Hover etc results in too many event, describe this...!

    function init(sessionGUID, pageGUID, serverGUID/*for playing*/) {
        if (!window.jQuery) {
            return;
        }

        setSessionGUID(sessionGUID);
        setPageGUID(pageGUID);
        setServerGUID(serverGUID);

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

            logElement(options.lrapSessionGUID, options.lrapPageGUID, options.lrapBundleGUID, null, unixTimestamp(), LogType.OnHandlerRequestSend, options.lrapOrigURL, JSON.stringify(options.data));
            options.url = getHandlerUrlForLogging(options.url, options.lrapSessionGUID, options.lrapPageGUID, options.lrapBundleGUID);

            //Ja, det er faktisk et ret stort tab... vi kan ikke simulere tid og dato som det var "dengang" det fejlede. Det skal beskrives i rapporten
        });

        bindAjaxCompleteFirst(function (event, xhr, options) { //has to be first, if one of the other events is doing a redirect we may not get to do this event...
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
                unixTimestamp(),
                LogType.OnHandlerResponseReceived,
                options.lrapOrigURL,
                JSON.stringify(xhr));

            //handleLogElements();
            //Because this global ajaxComplete is called after the $.ajax-instance's success/complete/error which could do a redirect of the page, we are called.. but so late that the logelement-timer is no longer active, therefore
            //do a handleLogElements to clean up always after an ajax-call
        });        

        bindWindowUnloadFirst(function () {
            finalizeLogger();                        
        });

        bindWindowBeforeClose(function() {
            finalizeLogger();
        });

        setupAllClientsideControlEvents();

        //To sync server and client time
        callLogHandler(false/*async*/, undefined, function() {
            setupLogger();
        });
    }    

    function getSelectionInfo(elm) {
        var selectedInfo = null;

        if (elm.selectionStart != undefined) { //IE10, Chrome and FF
            var startPos = elm.selectionStart;
            var endPos = elm.selectionEnd;
            selectedInfo = {};
            selectedInfo.text = elm.value.substring(startPos, endPos);
            selectedInfo.startPos = startPos;
            selectedInfo.endPos = endPos;
        }
        else
            // IE older versions
            if (document.selection != undefined) {
                var $focused = $(document.activeElement);
                elm.focus(); //TODO Ignore this logevent for focus and blur
                var sel = document.selection.createRange();
                selectedInfo = {};
                selectedInfo.text = sel.text;
                selectedInfo.startPos = 0;
                selectedInfo.endPos = sel.text.length;
                $focused.focus(); //TODO Ignore this logevent for focus and blur
            }

        return selectedInfo;
    }

    function setSelectionText(elm, startPos, endPos) {
        elm.focus();
        if (typeof elm.selectionStart != "undefined") { //IE10, Chrome and FF
            elm.selectionStart = startPos;
            elm.selectionEnd = endPos;
        }
        else if (document.selection && document.selection.createRange) {
            // IE branch
            elm.select();
            var range = document.selection.createRange();
            range.collapse(true);
            range.moveEnd("character", endPos);
            range.moveStart("character", startPos);
            range.select();
        }
    }

    //regionstart: Path to the element from the closest self or parent with an ID
    function trimString(x) {
        return x.replace(/^\s+|\s+$/gm, '');
    }

    function getElementIdxAndParentPath(elm, selector) { //must match getJQueryElementByElementPath countwise
        var $elm = $(elm);
        var $parent = $elm.parent();
        var parentPath = null;
        var idxPath = 0;
        if ($parent != null && $parent.size() > 0) {
            idxPath = -1;
            var $siblings = $parent.find(selector);

            $.each($siblings, function (idx, obj) {
                if (elm === obj) {
                    idxPath = idx;
                }
            });
            if (idxPath == -1) {
                console.log("ERROR: selector = " + selector);
                return "[ERROR]";
            }
            parentPath = getElementPath($parent[0]);
        }
        return (parentPath ? parentPath + "," : "") + (idxPath + 1) + '!' + selector;
    }

    function getElementPath(elm) { //e.g. "#someId,2!DIV,3!INPUT[type=text]"
        var $elm = $(elm);

        var elmID = $elm.attr("id");
        if (elmID)
            return "#" + elmID;
        var classNames = $elm.attr("class");
        if (classNames) {
            var classNameSelector = "." + trimString(classNames.replace(/\s+/g, ' ')).replace(/ /g, '.');
            return getElementIdxAndParentPath(elm, classNameSelector);
        }

        var nodeName = $elm.prop('nodeName');
        if (nodeName.length > 0 && nodeName.charAt(0) == '#') //#document or any other #<nodeName>, the very last element
            return null;
        
        if (nodeName.toUpperCase() === "INPUT") {
            var nodeType = $elm.attr("type");
            if (typeof (nodeType) != "undefined")
                nodeName = "INPUT[type=" + nodeType + "]";
            else
                nodeName = "INPUT:not([type])";
        }

        return getElementIdxAndParentPath(elm, nodeName);
    }

    function getJQueryElementByElementPath(elementPath) {
        if (elementPath == null || elementPath == "")
            return null;

        var elementPathUpperCase = elementPath.toUpperCase();

        if (elementPathUpperCase == 'WINDOW')
            return $(window);
        if (elementPathUpperCase == 'DOCUMENT')
            return $(document);

        var arr = elementPath.split(',');
        var $elm = null;
        $.each(arr, function(i, v) {
            v = $.trim(v);
            if (v == "")
                return;
            if (v.charAt(0) == "#") {
                if (v.toUpperCase() == "#DOCUMENT")
                    $elm = $(document);
                else
                    $elm = $(v);
            } else {
                if ($elm == null || $elm.size() == 0)
                    return;

                //Get int prefix, 
                var vArr = v.split('!', 2);
                if (vArr.length != 2)
                    return;

                var childIndex = +vArr[0]; //1..n
                var childSelector = vArr[1];

                var $child = null;

                $.each($elm.children(childSelector), function(ci, cv) {
                    if (ci + 1 == childIndex) {
                        $child = $(cv);
                    }
                });

                $elm = $child;

                //Hvilket element starter jeg fra? I tilfælde af jeg kun har klassenavne her i pathen
                //#form1,3!DIV,1!.clientTextboxWithNoID
            }
        });

        return $elm;

        //#ost, 2!<div>, 1!.qwerty.lord.helmet”
    }
    //regionend: Path to the element from the closest self or parent with an ID

    function getEvents(elm, type) {
        var events = $._data(elm, 'events');
        if (events) {
            var typeEvents = events[type];
            if (typeEvents)
                return typeEvents;
        }
        return null;
    }

    function getAttributes(that) {
        var result = [];
        $.each(that.attributes, function (i, v) {
            result.push({ Key: v.name, Value: v.value });
        });
        return result;
    }

    function logEvent(that, value, eventType, logType, event, compareFn) {
        var v = {
            event: JSONEvent(event),
            attributes: getAttributes(that),
            events: [],
            value: value
        };

        var events = getEvents(that, eventType);
        if (events) {
            v.events.push("jQueryEvent:" + events.length);
        }

        var $that = $(that);
        var attr = $that.attr('on' + eventType);
        if (attr) {
            v.events.push(attr);
        }

        if (v.events.length > 0) {
            logElementEx(logType, getElementPath(that), JSON.stringify(v));
            return;
        }

        if (typeof (event.pageX) != "undefined")
        {
            if (event.pageX != 0 && event.pageY != 0) {                
                var elmP = document.elementFromPoint(Math.ceil(event.pageX - window.pageXOffset), Math.ceil(event.pageY - window.pageYOffset));
                if (that == elmP) {                
                    logElementEx(logType, getElementPath(that), JSON.stringify(v), compareFn);
                }
            }
        }
        else
        {
            logElementEx(logType, getElementPath(that), JSON.stringify(v), compareFn);
        }
    }

    function setupBasicClientsideControlEvents(inputSelector) {
        var $document = $(document);
        $document.on('mousedown', inputSelector, function (event) { //left click vs right click?
            if (!event)
                event = window.event;

            var v = {
                button: event.button, //0=left 1=middle 2=right //The best supported
                shiftKey: event.shiftKey,
                altKey: event.altKey,
                ctrlKey: event.ctrlKey                
            };

            logEvent(this, v, 'mousedown', LogType.OnMouseDown, event);
        });

        $document.on('mouseup', inputSelector, function (event) { //left click vs right click?
            if (!event)
                event = window.event;

            var v = {
                button: event.button,
                shiftKey: event.shiftKey,
                altKey: event.altKey,
                ctrlKey: event.ctrlKey                
            };

            logEvent(this, v, 'mouseup', LogType.OnMouseUp, event);
        });        

        $document.on('click', inputSelector, function(event) { //Only elements with onclick/click event attached will be logged
            if (!event)
                event = window.event;

            var v = {};

            logEvent(this, v, 'click', LogType.OnClick, event);
        });

        $document.on('dblclick', inputSelector, function (event) {
            if (!event)
                event = window.event;

            var v = {};

            logEvent(this, v, 'dblclick', LogType.OnDblClick, event);
        });

        $document.on('dragstart', inputSelector, function (event) {            
            var target = event.target ? event.target : event.srcElement;

            var v = {
                event: JSONEvent(event)
            };

            logEvent(target, v, 'dragstart', LogType.OnDragStart, event);
        });

        $document.on('dragend', inputSelector, function (event) {
            var target = event.target ? event.target : event.srcElement;

            var v = {};

            logEvent(target, v, 'dragend', LogType.OnDragEnd, event);
        });

        $document.on('dragover', inputSelector, function (event) { //To tell if it is allowed
            var target = event.target ? event.target : event.srcElement;

            var v = {};

            logEvent(target, v, 'dragover', LogType.OnDragOver, event);
        });

        $document.on('drop', inputSelector, function (event) {
            var target = event.target ? event.target : event.srcElement;

            var v = {};

            logEvent(target, v, 'drop', LogType.OnDrop, event);
        });

        $document.on('scroll', inputSelector, function (event) {
            var $this = $(this);

            var v = {
                top: $this.scrollTop(),
                left: $this.scrollLeft()                
            };

            logEvent(this, v, 'scroll', LogType.OnScroll, event, compareLogElementsNoValue);
        });
    }

    function logInputClientsideControlEvent(that, logType, value, event, compareFn, combineFn) {
        var v = {
            event: event,
            attributes: getAttributes(that),
            value: value
        };         

        logElementEx(logType, getElementPath(that), JSON.stringify(v), compareFn, combineFn);
    }

    function JSONEvent(event) {
        var r = {};
        for (var key in event) {
            var v = event[key];
            var type = (typeof (v) + "").toLowerCase();
            if (type !== 'function' && type !== 'object' && key != 'timeStamp' && key.indexOf("jQuery") != 0) {
                r[key] = v;
            }
        };
        return r;
    }

    function setupInputClientsideControlEvents(inputSelector) {

        var $document = $(document);

        $document.on('blur', inputSelector, function (event) {
            logInputClientsideControlEvent(this, LogType.OnBlur, null, JSONEvent(event));
        });

        $document.on('focus', inputSelector, function (event) {
            logInputClientsideControlEvent(this, LogType.OnFocus, null, JSONEvent(event));
        });

        $document.on('change', inputSelector, function (event) {
            var $this = $(this);
            var type = $this.prop('type').toUpperCase();
            var v;
            if (type === 'RADIO' || type === 'CHECKBOX')
                v = $this.prop("checked");
            else
                v = $this.val();

            logInputClientsideControlEvent(this, LogType.OnChange, v, JSONEvent(event));
        });

        $document.on('select', inputSelector, function (event) { //select text.. apparently no way of getting the selected text? or is there... check caret showSelectionInsideTextarea also works on inputs
            logInputClientsideControlEvent(this, LogType.OnSelect, getSelectionInfo(this), JSONEvent(event));
        });

        $document.on('copy', inputSelector, function (event) {
            logInputClientsideControlEvent(this, LogType.OnCopy, getSelectionInfo(this), JSONEvent(event));
        });

        $document.on('cut', inputSelector, function (event) {
            logInputClientsideControlEvent(this, LogType.OnCut, getSelectionInfo(this), JSONEvent(event));
        });

        $document.on('paste', inputSelector, function (event) {
            logInputClientsideControlEvent(this, LogType.OnPaste, $(this).val(), JSONEvent(event));
        });

        $document.on('keydown', inputSelector, function (event) {
            if (!event)
                event = window.event;
            var charCode = event.which || event.keyCode;
            var ch = String.fromCharCode(charCode);

            var v = {
                charCode: charCode,
                ch: ch,
                shiftKey: event.shiftKey,
                altKey: event.altKey,
                ctrlKey: event.ctrlKey,
                caretPos: getCaretPositionRelativeToEnd(this)
            };

            logInputClientsideControlEvent(this, LogType.OnKeyDown, v, JSONEvent(event));
        });

        $document.on('keyup', inputSelector, function (event) { //keyCode is incase-sensative
            if (!event)
                event = window.event;
            var charCode = event.which || event.keyCode;
            var ch = String.fromCharCode(charCode);

            var v = {
                charCode: charCode,
                ch: ch,
                shiftKey: event.shiftKey,
                altKey: event.altKey,
                ctrlKey: event.ctrlKey,
                caretPos: getCaretPositionRelativeToEnd(this)
            };

            logInputClientsideControlEvent(this, LogType.OnKeyUp, v, JSONEvent(event), compareLogElementsKeyup, combineLogElementsKeyup);
        });

        $document.on('keypress', inputSelector, function (event) { //keyCode is case-sensative
            if (!event)
                event = window.event;
            var charCode = event.which || event.keyCode;
            var ch = String.fromCharCode(charCode);

            var v = {
                charCode: charCode,
                ch: ch,
                shiftKey: event.shiftKey,
                altKey: event.altKey,
                ctrlKey: event.ctrlKey,
                caretPos: getCaretPositionRelativeToEnd(this)                
            };
            
            logInputClientsideControlEvent(this, LogType.OnKeyPress, v, JSONEvent(event), compareLogElementsKeypress, combineLogElementsKeypress);
        });        
    }

    function logWindowScroll() {
        //if (isPlaying()) {
        //    var logElementGUID = popLogElementGUID();
        //    if (logElementGUID != null) {
        //        window.external.SetLogElementAsDone(logElementGUID);
        //    }
        //    return;
        //}

        var $this = $(window);

        var v = {
            top: $this.scrollTop(),
            left: $this.scrollLeft()
        };

        logElementEx(LogType.OnScroll, "window", JSON.stringify(v), compareLogElementsNoValue);
    }

    function logWindowSize() {
        var $window = $(window);
        var v = {
            width: $window.width(),
            height: $window.height()
        };        
        logElementEx(LogType.OnResize, "window", JSON.stringify(v), compareLogElementsNoValue);
    }

    function setupAllClientsideControlEvents() {
        var $document = $(document);
        $document.on('submit', 'form', function () {
            logElementEx(LogType.OnSubmit, getElementPath(this), "");
        });

        $document.on('reset', 'form', function () {
            logElementEx(LogType.OnReset, getElementPath(this), "");
        });

        setupBasicClientsideControlEvents("a");
        setupBasicClientsideControlEvents("p");
        setupBasicClientsideControlEvents("div");
        setupBasicClientsideControlEvents("span");
        setupBasicClientsideControlEvents("textarea");
        setupBasicClientsideControlEvents("input");
        setupBasicClientsideControlEvents("select");
        setupBasicClientsideControlEvents("img");
        setupBasicClientsideControlEvents("area");
        setupInputClientsideControlEvents("textarea");
        setupInputClientsideControlEvents("input"); 
        setupInputClientsideControlEvents("select"); 

        $document.on('search', "input[type=search]", function () {
            logElementEx(LogType.OnSearch, getElementPath(this), $(this).val());
        });

        var $window = $(window);

        $window.resize(logWindowSize);
        logWindowSize();

        $window.scroll(logWindowScroll);        
        logWindowScroll();
    }

    var logElements = [];
    var logElementsTimer = null;

    function setupLogger() {
        logElementsTimer = setTimeout(function () {
            logElementsTimer = null;
            handleLogElements();
            setupLogger();
        }, 5000);
    }

    function finalizeLogger() {
        if (logElementsTimer != null) {
            clearTimeout(logElementsTimer);
            logElementsTimer = null;            
        }
        handleLogElements(false);
    }

    // the NTP algorithm
    // t0 is the client's timestamp of the request packet transmission,
    // t1 is the server's timestamp of the request packet reception,
    // t2 is the server's timestamp of the response packet transmission and
    // t3 is the client's timestamp of the response packet reception.
    function ntp(t0, t1, t2, t3) {
        return {
            roundtripdelay: (t3 - t0) - (t2 - t1),
            offset: ((t1 - t0) + (t2 - t3)) / 2
        };
    }

    function handleLogElements(async) {
        async = typeof (async) == "undefined" || async;

        var logElementsForHandler = logElements;
        logElements = [];

        if (logElementsForHandler.length > 0) {
            var logHandlerRequest = {            
                LogElements: logElementsForHandler
            };
            callLogHandler(async, logHandlerRequest);
        }
    }

    function callLogHandler(async, logHandlerRequest, fSuccess) {
        async = typeof (async) == "undefined" || async;

        if (typeof (logHandlerRequest) == "undefined") {
            logHandlerRequest = {
                LogElements: []
            };
        }
                
        $.each(logHandlerRequest.LogElements,function(idx, v) {
            v.UnixTimestamp = v.UnixClientTimestamp + clientTimeOffset;
            v.CompareFn = undefined;
            v.CombineFn = undefined;
            v.CombinedElements = undefined;
        });

        var clientTimeStart = unixTimestamp();        
        $.ajax({
            async: async,
            LRAPCall: true,
            type: "POST",
            url: handlerLRAPUrl,
            data: {
                'request': JSON.stringify(logHandlerRequest)
            },
            success: function (data) {
                var clientTimeEnd = unixTimestamp();
                var ntpR = ntp(clientTimeStart, data.ServerTimeStart, data.ServerTimeEnd, clientTimeEnd);
                clientTimeOffset = ntpR.offset;

                if (fSuccess)
                    fSuccess();
            },
            error: function (data) {
                logElements = logElementsForHandler.concat(logElements); //Put elements back in queue
                console.log(logElements);
                alert('Failed to log: Contact administrator');
            }
        });
    }

    function parseJsonDate(jsonDateString) {
        return new Date(parseInt(jsonDateString.replace('/Date(', '')));
    }

    function logElementEx(logType, element, value, compareFn, combineFn) {
        logElement(getSessionGUID(), getPageGUID(), null, null, unixTimestamp(), logType, element, value, compareFn, combineFn);
    }

    function logElement(sessionGUID, pageGUID, bundleGUID, progressGUID, unixTimestamp, logType, element, value, compareFn, combineFn) {
        if (element == null || isPlaying())
            return;

        if (typeof (compareFn) == "undefined")
            compareFn = compareLogElements;

        if (typeof (combineFn) == "undefined")
            combineFn = combineLogElements;

        var request = {
            GUID: generateGUID(),
            SessionGUID: sessionGUID,
            PageGUID: pageGUID,
            BundleGUID: bundleGUID,
            ProgressGUID: progressGUID,
            UnixClientTimestamp: unixTimestamp, //getDataMemberDate(timestamp),
            LogType: logType,
            Element: htmlEncode(element), //denne burde html encodes (eller faktisk burde den kun html encodes når det ikke er status=200... hmmm... er jo heller ikke holdbart
            Element2: null,
            Value: value != null ? htmlEncode(value) : null,
            Times: 1,
            UnixTimestampEnd: null,
            CombinedRequestsWithDifferentLogType: [],
            CompareFn: compareFn,
            CombineFn: combineFn
        };
        logElements.push(request);

        compactLogElementList();
    }

    function compactLogElementList() {
        var requestIdx = logElements.length - 1;

        while (requestIdx > 0) {
            var secondLastRequest = logElements[requestIdx - 1];
            var lastRequest = logElements[requestIdx];

            var compareFn = lastRequest.CompareFn;
            var combineFn = lastRequest.CombineFn;

            if (typeof (compareFn) == "undefined")
                compareFn = compareLogElements;

            if (typeof (combineFn) == "undefined")
                combineFn = combineLogElements;

            var compareResult = compareFn(secondLastRequest, lastRequest);
            if (compareResult) {
                var newLogElement = combineFn(secondLastRequest, lastRequest, compareResult);

                logElements.pop(); //remove lastRequest
                logElements.pop(); //remove secondLastRequest
                logElements.push(newLogElement);

                requestIdx--;
            } else {
                return;
            }
        }
    }

    function getKeyUpDownKeypadChar(charCode) {
        if (charCode >= 96 && charCode <= 105)
            return String.fromCharCode(charCode - 48);
        return "";
    }

    function compareLogElementsKeypress(le1, le2) {
        if (le1.SessionGUID == le2.SessionGUID &&
            le1.PageGUID == le2.PageGUID &&
            le1.BundleGUID == le2.BundleGUID &&
            le1.ProgressGUID == le2.ProgressGUID &&
            le1.Times == le2.Times &&
            le1.LogType == LogType.OnKeyDown &&
            le2.LogType == LogType.OnKeyPress) {

            var v1 = JSON.parse(htmlDecode(le1.Value)).value;
            var v2 = JSON.parse(htmlDecode(le2.Value)).value;

            if ((
                    getKeyUpDownKeypadChar(v1.charCode) == v2.ch ||
                    v1.ch.toUpperCase() == v2.ch.toUpperCase()
                ) &&
                v1.shiftKey == v2.shiftKey &&
                v1.altKey == v2.altKey &&
                v1.ctrlKey == v2.ctrlKey &&
                v1.caretPos == v2.caretPos) {
                return 1;
            }
        }

        if (le1.LogType == LogType.OnKeyPress && le2.LogType == LogType.OnKeyPress)
            return compareLogElementsKeypress2(le1, le2); //Do not call default compareMethod for KeyPress/KeyPress
        
        if (compareLogElements(le1, le2)) 
            return 2;       

        return 0;
    }

    //le1/le2 == KeyPress
    function compareLogElementsKeypress2(le1, le2) {
        var v1 = JSON.parse(htmlDecode(le1.Value)).value;
        var v2 = JSON.parse(htmlDecode(le2.Value)).value;

        if (v1.ch != v2.ch ||
            v1.shiftKey != v2.shiftKey ||
            v1.altKey != v2.altKey ||
            v1.ctrlKey != v2.ctrlKey ||
            v1.caretPos != v2.caretPos) {
            return 0;
        }

        if (GetCombinedElement(le1, 'KeyDown') == GetCombinedElement(le1, 'KeyUp') &&
            GetCombinedElement(le2, 'KeyDown') == GetCombinedElement(le2, 'KeyUp'))
            return 2;

        return 0;
    }

    //le1=OnKeyDown/OnKeyPress le2=OnKeyPress
    function combineLogElementsKeypress(le1, le2, compareResult) { // le2 = le1 + le2 (non pure)
        if (le1.LogType != le2.LogType) {
            le2.CombinedRequestsWithDifferentLogType.push(le1); 
        }
        //le1.UnixTimestampEnd = le2.UnixTimestamp;
        //le1.Value = le2.Value;
        //le1.LogType = le2.LogType;
        if (compareResult == 2) {
            le2.Times += le1.Times;
        }

        if (le1.LogType == LogType.OnKeyDown && le2.LogType == LogType.OnKeyPress) {
            SetCombinedElement(le2,  'KeyDown', GetCombinedElement(le2, 'KeyDown') + 1);
        } else if (le1.LogType == LogType.OnKeyPress && le2.LogType == LogType.OnKeyPress) { //Only able to combine if the KeyDown/KeyUp ratio is 1 on both le1 and le2
            SetCombinedElement(le2,  'KeyDown', GetCombinedElement(le2, 'KeyDown') + GetCombinedElement(le1, 'KeyDown'));
            SetCombinedElement(le2,  'KeyUp', GetCombinedElement(le2, 'KeyUp') + GetCombinedElement(le1, 'KeyUp'));
        }

        return le2;
    }

    function SetCombinedElement(le, type, value) {
        if (!le.CombinedElements)
            le.CombinedElements = {};
        le.CombinedElements[type] = value;
    }

    function GetCombinedElement(le, type) {
        if (!le || !le.CombinedElements)
            return 0;
        var c = le.CombinedElements[type];
        if (!c)
            return 0;
        return c;
    }

    function compareLogElementsKeyup(le1, le2) {
        if (le1.SessionGUID == le2.SessionGUID &&
            le1.PageGUID == le2.PageGUID &&
            le1.BundleGUID == le2.BundleGUID &&
            le1.ProgressGUID == le2.ProgressGUID &&
            //le1.Times == le2.Times &&
            le1.LogType == LogType.OnKeyPress &&
            le2.LogType == LogType.OnKeyUp) {

            var le1Value = JSON.parse(htmlDecode(le1.Value));
            var le2Value = JSON.parse(htmlDecode(le2.Value));

            var v1 = le1Value.value;
            var v2 = le2Value.value;
            
            if ((
                    v1.ch == getKeyUpDownKeypadChar(v2.charCode) ||
                    v1.ch.toUpperCase() == v2.ch.toUpperCase()
                ) &&
                v1.shiftKey == v2.shiftKey &&
                v1.altKey == v2.altKey &&
                v1.ctrlKey == v2.ctrlKey) {

                var keyPressTimes = le1.Times;
                var keyUpTimes = le1.CombinedElements ? le1.CombinedElements["KeyUp"] : 0;
                if (!keyUpTimes) //0 or undefined
                    keyUpTimes = 0;

                if (keyPressTimes - 1 == keyUpTimes) { //dvs der mangler en keyup til den pågældende KeyPress
                    return 1;
                }
            }
        }

        //Kan ikke opstå en situation hvor der er 2 stk keyup af den samme tast
        //if (le1.Times == le2.Times && compareLogElements(le1, le2)) {
        //    return 2;
        //}

        return 0;
    }

    //le1=KeyPress
    //le2=KeyUp
    function combineLogElementsKeyup(le1, le2, compareResult) { // le1 = le1 + le2 (non pure)
        if (le1.LogType != le2.LogType) {
            le1.CombinedRequestsWithDifferentLogType.push(le2);
        }
        le1.UnixTimestampEnd = le2.UnixTimestamp;
        if (compareResult == 1) {
            if (!le1.CombinedElements) 
                le1.CombinedElements = {};

            SetCombinedElement(le1, 'KeyUp', GetCombinedElement(le1, 'KeyUp') + 1);
        }
        return le1;
    }

    function combineLogElements(le1, le2) { // le1 = le1 + le2 (non pure)
        if (le1.LogType != le2.LogType) {
            le1.CombinedRequestsWithDifferentLogType.push(le2);
        }
        le1.Times += le2.Times;
        le1.UnixTimestampEnd = le2.UnixTimestamp;
        le1.Value = le2.Value;
        le1.LogType = le2.LogType;
        return le1;
    }

    function compareLogElementsNoValue(le1, le2) { //NoValue compare would be used with e.g. resize events
        return le1.SessionGUID == le2.SessionGUID &&
            le1.PageGUID == le2.PageGUID &&
            le1.BundleGUID == le2.BundleGUID &&
            le1.ProgressGUID == le2.ProgressGUID &&
            le1.LogType == le2.LogType &&
            le1.Element == le2.Element;
    }

    function compareLogElements(le1, le2) { //Without UnixTimestamp compares atm... should build a delay check on the compare        
        return  compareLogElementsNoValue(le1, le2) &&
                le1.Value == le2.Value; 
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
            dateObj = convertStringToDateObject(dateStr);
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
        url = addQryStrElement(removeQryStrElement(url, GUIDTag), GUIDTag, generateGUID());
        url = addQryStrElement(removeQryStrElement(url, SessionGUIDTag), SessionGUIDTag, sessionGUID);
        url = addQryStrElement(removeQryStrElement(url, PageGUIDTag), PageGUIDTag, pageGUID);
        url = addQryStrElement(removeQryStrElement(url, BundleGUIDTag), BundleGUIDTag, bundleGUID);
        return url;
    }

    function setupAjaxEvents(type) {
        var documentEvents = $._data(document, 'events');
        if (typeof (documentEvents) == "undefined") {
            return;
        }
        var events = documentEvents[type];
        if (typeof (events) == "undefined") {
            return;
        }

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

    function bindAjaxCompleteFirst(fn) {
        $(document).ajaxComplete(fn);
        moveLastEventToFirst(document, "ajaxComplete");
    }

    function bindAjaxCompleteLast(fn) {
        $(document).ajaxComplete(fn);
    }

    function bindWindowUnloadFirst(fn) {
        var $window = $(window);
        if (!$window.unload)
            return;
        $window.unload(fn);
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

            //(e || window.event).returnValue = null;
            //return null;
        });
    }

    function moveLastEventToFirst(elm, action) {
        var handlers = $._data(elm, 'events')[action];
        if (!handlers)
            return;
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

    //Replacement for "new Date()" in order to get the current time/date, but for the LogPlayer we need to simulate the same "current time/date" as when we recorded useractivity.
    function now() {
        return new Date();
    }

    // Return value relative to end of value (0-oField.value.length)
    function getCaretPositionRelativeToEnd(elem) {

        var pos = 0;

        // IE Support
        if (document.selection) {
            elem.focus();

            // To get cursor position, get empty selection range
            var oSel = document.selection.createRange();

            // Move selection start to 0 position
            oSel.moveStart('character', -elem.value.length);

            // The caret position is selection length
            pos = oSel.text.length;
        }            
        else // Firefox support
            if (elem.selectionStart || elem.selectionStart == '0')
                pos = elem.selectionStart;
        
        return elem.value.length - pos;
    }

    function setCaretPositionRelativeToEnd(elem, caretPos) {
        caretPos = elem.value.length - caretPos;

        if (elem.createTextRange) {
            var range = elem.createTextRange();
            range.move('character', caretPos);
            range.select();
        }
        else {
            if (elem.selectionStart) {
                elem.focus();
                elem.setSelectionRange(caretPos, caretPos);
            }
            else
                elem.focus();
        }
    }

    function validateSelectorAgainstElement(selector, $elm) {
        var id = $elm.prop("id");
        var classes = $elm.prop("class");

        var $child = $('<div></div>');
        if (typeof (id) != "undefined")
            $child.prop("id", id);
        if (typeof (classes) != "undefined")
            $child.prop("class", classes);

        var $parent = $('<div></div>');
        $parent.append($child);

        var f = $parent.find(selector);
        if (f.size() > 0)
            return f[0] == $child[0];
        return false;
    }

    function getJQueryEvents($elm, $curr, eventType /*e.g. 'focus'*/, level) {
        if ($curr.size() == 0) //e.g. $(document).parent() / Top Level
            return [];

        level = typeof (level) == "undefined" ? 0 : level;

        //AND: a=$('[myc="blue"][myid="1"][myid="3"]');
        //OR: a=$('[myc="blue"],[myid="1"],[myid="3"]');

        var result = [];

        var events = ($._data || $.data)($curr[0], 'events');
        if (events) {
            var typeEvents = events[eventType];
            if (typeEvents && ((eventType != 'focus' && eventType != 'blur') || level == 0)) { //focus and blur event should only be used if they are directly attached the the $elm
                result = typeEvents;
            }
            if (eventType == 'focus' && level > 0) {
                var typeEvents = events["focusin"];
                if (typeEvents) {
                    result.push.apply(result, typeEvents);
                }
            }
            if (eventType == 'blur' && level > 0) {
                var typeEvents = events["focusout"];
                if (typeEvents) {
                    result.push.apply(result, typeEvents);
                }
            }
        }

        if ($elm !== $curr) {
            result = $.grep(result, function (vTE) {
                return validateSelectorAgainstElement(vTE.selector, $elm);
            });
        }

        result = $.map(result, function (v) {
            return {
                level: level,
                handler: v.handler
            };
        });

        result.push.apply(result, getJQueryEvents($elm, $curr.parent(), eventType, level + 1));

        return result;
    }

    function callElementsEventMethods($elm, eventType/*e.g. click or focus*/, event) {
        var $events = getJQueryEvents($elm, $elm, eventType);
        var cancelAllHandlers = false;
        var lastLevel = null;
        $.each($events, function (i, v) {
            if (cancelAllHandlers)
                return;
            if (lastLevel != null && v.level > lastLevel)
                return;

            if (i == 0) {
                logRecorderAndPlayer.stopImmediatePropagationCalled = false;
                logRecorderAndPlayer.stopPropagationCalled = false;
            }

            v.handler.call($elm[0], event); //document.createEvent('Event'));

            if (logRecorderAndPlayer.stopImmediatePropagationCalled) {
                if (v.level == 0)
                    cancelAllHandlers = true;
                lastLevel = v.level;
            }
            if (logRecorderAndPlayer.stopPropagationCalled) {
                lastLevel = v.level;
            }
        });
        return;
    }

    function createKeyboardEvent(elm, event) {
        var modifiers = [];
        if (event.altKey)
            modifiers.push("Alt");
        if (event.shiftKey)
            modifiers.push("Shift");
        if (event.ctrlKey)
            modifiers.push("Control");

        var keyboardEvent = document.createEvent("KeyboardEvent");

        keyboardEvent.initKeyboardEvent(
            event.type,
            event.bubbles,
            event.cancelable,
            null, //viewArg,
            event.key, // keyArg,
            null, //locationArg,        
            modifiers.join(" "), //modifiersListArg,
            false,//repeat
            null //locale
            );

        return keyboardEvent;
    }

    function createMouseEvent(elm, event) {
        var mouseEvent = document.createEvent("MouseEvent");

        mouseEvent.initMouseEvent(
            event.type, //typeArg: string, 
            event.bubbles, //canBubbleArg: boolean, 
            event.cancelable, //cancelableArg: boolean, 
            null, //viewArg: Window, 
            event.detail, //detailArg: number, 
            event.screenX, //screenXArg: number, 
            event.screenY, //screenYArg: number, 
            event.clientX, //clientXArg: number, 
            event.clientX, //clientYArg: number, 
            event.ctrlKey, //ctrlKeyArg: boolean, 
            event.altKey, //altKeyArg: boolean, 
            event.shiftKey, //shiftKeyArg: boolean, 
            event.metaKey, //metaKeyArg: boolean, 
            event.button, //buttonArg: number, 
            elm); //relatedTargetArg: EventTarget);

        return mouseEvent;
    }

    function createFocusEvent(elm, event) {
        var focusEvent = document.createEvent("FocusEvent");
        focusEvent.initFocusEvent(
            event.type, //typeArg,  (focus, blur, focusin, focusout)
            event.bubbles, //canBubbleArg, 
            event.cancelable, //cancelableArg, 
            null, //viewArg, 
            null, //detailArg, 
            elm); //relatedTargetArg

        return focusEvent;
    }
    
    function createDragEvent(elm, event) {
        var dragEvent = document.createEvent("DragEvent");
        dragEvent.initDragEvent(
            event.type, //typeArg: string, 
            event.bubbles, //canBubbleArg: boolean, 
            event.cancelable, //cancelableArg: boolean, 
            null, //viewArg: Window, 
            event.detail, //detailArg: number, 
            event.screenX, //screenXArg: number, 
            event.screenY, //screenYArg: number, 
            event.clientX, //clientXArg: number, 
            event.clientY, //clientYArg: number, 
            event.ctrlKey, //ctrlKeyArg: boolean, 
            event.altKey, //altKeyArg: boolean, 
            event.shiftKey, //shiftKeyArg: boolean, 
            event.metaKey, //metaKeyArg: boolean, 
            event.button, //buttonArg: number, 
            elm, //relatedTargetArg: EventTarget, 
            null); //dataTransferArg: DataTransfer;
        return dragEvent;
    }

    function createDefaultEvent(elm, event) {
        var defaultEvent = document.createEvent('Event');
        defaultEvent.initEvent(
            event.type, //typeArg: string
            event.bubbles, //canBubbleArg: boolean
            event.cancelable); //cancelableArg: boolean
        return defaultEvent;
    }

    function createEventForElm(elm, elementValue) {
        var event = elementValue.event;
        if (typeof (event) == "undefined") {
            return document.createEvent('Event');
        }

        var type = (event.type+"").toLowerCase();

        if (type == 'keydown' || type == 'keypress' || type == 'keyup')
            return createKeyboardEvent(elm, event);
        if (type == 'focus' || type == 'blur' || type == 'focusin' || type == 'focusout')
            return createFocusEvent(elm, event);
        if (type == 'click' || type == 'mousedown' || type == 'mouseup' || type == 'mouseover' || type == 'mousemove' || type == 'mouseout')
            return createMouseEvent(elm, event);        
        if (type == 'drag' || type == 'dragend' || type == 'dragenter' || type == 'dragexit' || type == 'dragleave' || type == 'dragover' || type == 'dragstart' || type == 'drop')
            return createDragEvent(elm, event);
        return createDefaultEvent(elm, event);
    }

    function isControlRequiredToBeVisible($elm, logElement) {
        if ($elm[0] === document) //nodeName = #document, but still a "non-visual" element
            return false;

        var nodeName = $elm.prop("nodeName"); //e.g. window does not have a nodeName (which is used for scroll and resize events)
        if (!nodeName)
            return false;

        return true;

        //Following code would be correct, if the control somehow was possible to enter by e.g. the tab-key.. but for most cases i would say that it's more normal to have a overlay/model-div on top

        //var logType = logElement.LogType;

        //return logType == LogType.OnMouseDown ||
        //    logType == LogType.OnMouseUp ||
        //    logType == LogType.OnClick ||
        //    logType == LogType.OnDblClick ||
        //    logType == LogType.OnDragStart ||
        //    logType == LogType.OnDragEnd ||
        //    logType == LogType.OnDragOver ||
        //    logType == LogType.OnDrop;
    }

    function isControlVisible($elm) {
        var position = $elm.position();
        return $elm[0] == document.elementFromPoint(Math.ceil(position.left), Math.ceil(position.top)); 
    }

    function playEventFor(elementPath, logElement /*json*/) {
        //setTimeout(function() {
        //    window.external.SetLogElementAsDone(0, false, 'woohooo timed!');
        //}, 3000);
        //return;

        var $elm = getJQueryElementByElementPath(elementPath);
        if ($elm == null || $elm.size() == 0) {
            alert("Error occured while playing event for " + elementPath + ", could not be located");
            return;
        }

        var loopTotal = elementValue.Times ? elementValue.Times : 1;
        var timeoutInSec = 10;
        //var loopStart = unixTimestamp();

        playLoop(loopTotal, 
            function () { //condition
                return isControlRequiredToBeVisible($elm, logElement) ? isControlVisible($elm) : true;
            },
            function () { //execute
                doPlayEventFor($elm, logElement);
            },
            function (loopCounter/*n-1..0*/) { //progress                             
            },
            function (timedout) { //done
                if (timedout) {
                    alert('Error occured while playing events (Timeout exception)');
                    window.external.SetLogElementAsDone(logElement.GUID);
                    return;
                }
                alert('the end');
                window.external.SetLogElementAsDone(logElement.GUID);
            },
            timeoutInSec
        );
    }

    function playLoop(loopCounter, conditionFunc, executeFunc, progressFunc, doneFunc, timeoutInSec, roundStart/*internal/optional*/) {
        if (loopCounter <= 0) {
            doneFunc(false);
            return;
        }

        if (!roundStart)
            roundStart = unixTimestamp();

        if (timeoutInSec && unixTimestamp() - roundStart > timeoutInSec) {
            doneFunc(true);
            return;
        }

        setTimeout(function () {
            if (conditionFunc(loopCounter)) {
                executeFunc(loopCounter);
                if (progressFunc)
                    progressFunc(loopCounter);
                playLoop(loopCounter - 1, conditionFunc, executeFunc, progressFunc, doneFunc, timeoutInSec, undefined);
            } else {
                playLoop(loopCounter, conditionFunc, executeFunc, progressFunc, doneFunc, timeoutInSec, roundStart);
            }            
        }, 100);
    }

    function scrollToElement($elm) {
        var elm = $elm[0];
        if (elm === window || elm === document)
            return;

        var $window = $(window);
        var windowTop = $window.scrollTop();
        var windowLeft = $window.scrollLeft();
        var windowWidth = $window.width();
        var windowHeight = $window.height();
        var elmPosition = $elm.position();
        var elmLeft = elmPosition.left;
        var elmTop = elmPosition.top;

        if (windowTop <= elmTop &&
            windowLeft <= elmLeft &&
            windowTop + windowHeight > elmTop &&
            windowLeft + windowWidth > elmLeft)
            return; //elm is visible, do nothing

        window.scrollBy(elmLeft - windowLeft, elmTop - windowTop);
    }

    function doPlayEventFor($elm, logElement) {
        scrollToElement($elm);

        var logType = logElement.LogType;
        var elementValue = JSON.parse(htmlDecode(logElement.Value));

        var eventName = null;

        //button
        //checkbox
        //color
        //date
        //datetime - local
        //email
        //file
        //hidden
        //image
        //month
        //number
        //password
        //radio
        //range
        //reset
        //search
        //submit
        //tel
        //text
        //time
        //url
        //week

        var preCombinedLogTypes = [];
        var postCombinedLogTypes = [];

        switch(logType) {
            case LogType.OnFocus:
                $elm.focus();
                eventName = 'focus';
                break;
            case LogType.OnBlur:
                $elm.blur();
                eventName = 'blur';
                break;
            case LogType.OnChange:
                var inputType = $elm.prop("type").toUpperCase();
                if (inputType === 'RADIO' || inputType == 'CHECKBOX') {
                    $elm.prop("checked", elementValue.value);
                } else {
                    $elm.val(elementValue.value);
                }
                eventName = 'change';
                //nye html5 elementer:
                //Datalist https://www.w3schools.com/html/html_form_elements.asp
                //Keygen
                //Output
                break;
            case LogType.OnSelect: //selected text
                //selectedInfo.text = elm.value.substring(startPos, endPos);
                //selectedInfo.startPos = startPos;
                //selectedInfo.endPos = endPos;
                setSelectionText($elm[0], elementValue.startPos, elementValue.endPos);

                var validateText = getSelectionInfo($elm[0]);
                if (validateText != elementValue.text) {
                    alert('ERROR: Selection was not played successfully');
                    return;
                }

                eventName = 'select';
                break;
            case LogType.OnCopy:
                //clipboard copy, wouldn't make any sense to simulate this
                eventName = 'copy';
                break;
            case LogType.OnCut:
                //change event will be call afterwards
                eventName = 'cut';
                break;
            case LogType.OnPaste:
                //change event will be call afterwards
                eventName = 'paste';
                break;
            case LogType.OnKeyDown:
                //The value is not changed yet
                eventName = 'keydown';                
                break;
            case LogType.OnKeyPress: 
                eventName = 'keypress';
                
                preCombinedLogTypes = $.grep(logElement.CombinedRequestsWithDifferentLogType, function (c) {
                    return c.LogType == LogType.OnKeyDown;
                });

                postCombinedLogTypes = $.grep(logElement.CombinedRequestsWithDifferentLogType, function (c) {
                    return c.LogType == LogType.OnKeyUp;
                });

                break;
            case LogType.OnKeyUp:
                //The value is changed
                eventName = 'keyup';
                break;
            case LogType.OnMouseDown:
                //Only call event
                eventName = 'mousedown';
                break;
            case LogType.OnMouseUp:
                //Only call event
                eventName = 'mouseup';
                break;
            case LogType.OnClick:
                //Only call event
                eventName = 'click';
                break;
            case LogType.OnDblClick:
                //Only call event
                eventName = 'dblclick';
                break;
            case LogType.OnSearch:
                //Only call event
                eventName = 'search';
                break;
            case LogType.OnResize:
                //Only call event
                eventName = 'resize';
                break;
            case LogType.OnDragStart:
                //Only call event
                eventName = 'dragstart';
                break;
            case LogType.OnDragEnd:
                //Only call event
                eventName = 'dragend';
                break;
            case LogType.OnDragOver:
                //Only call event
                eventName = 'dragover';
                break;
            case LogType.OnDrop:
                //Only call event
                eventName = 'drop';
                break;
            case LogType.OnScroll:
                //Perform.scroll
                eventName = 'scroll';
                alert('scroll top=' + elementValue.top);
                $elm[0].scrollTo(elementValue.left, elementValue.top);                
                break;
            default:
                alert("LogType (" + logType + ") is not supported");
                return;
        }

        $.each(preCombinedLogTypes, function (preIdx, preCombinedLogType) {
            doPlayEventFor(preCombinedLogType.LogType, $elm, preCombinedLogType);
        });

        callEventMethods($elm, elementValue, eventName);

        //Post-section
        switch (logType) {
            case LogType.OnKeyPress:

                var selectionInfo = getSelectionInfo($elm[0]);
                if (selectionInfo != null) { //startPos + endPos
                    var value = $elm.val();
                    var relatedToStart;
                    var newValue;
                    var startPos = selectionInfo.startPos;
                    var endPos = selectionInfo.endPos;
                    if (startPos >= endPos) {
                        //replace text within startPos and endPos
                    } else {
                        //use caretPos to insert value                        
                        relatedToStart = value.length - elementValue.value.caretPos;
                        startPos = relatedToStart;
                        endPos = relatedToStart;
                    }
                    newValue = value.substring(0, startPos) + elementValue.value.ch + value.substring(endPos);
                    $elm.val(newValue);
                }

                //Jeg er nået til dette event.. som jo skal rette på $elm.val, burde nok gøre det via caretPos og selvfølgelig den charcode
                //var v = {
                //charCode: charCode,
                //ch: ch,
                //shiftKey: event.shiftKey,
                //altKey: event.altKey,
                //ctrlKey: event.ctrlKey,
                //caretPos: getCaretPositionRelativeToEnd(this)                

                break;
        }

        $.each(postCombinedLogTypes, function (preIdx, postCombinedLogType) {
            doPlayEventFor(postCombinedLogType.LogType, $elm, postCombinedLogType);
        });
    }

    function callEventMethods($elm, elementValue, eventName) {
        var propResult = true;

        var event = createEventForElm($elm[0], elementValue);

        if (eventName != null) {
            var p = $elm.prop("on" + eventName);
            if (p) {
                propResult = p.call($elm[0], event);
                propResult = typeof (propResult) == "undefined" || propResult;
            }
        }

        if (propResult) {
            callElementsEventMethods($elm, eventName, event);
        }
    }

    ///#endregion

    var publicMethods = {};

    publicMethods.init = init;
    publicMethods.getPageGUID = getPageGUID;
    publicMethods.getSessionGUID = getSessionGUID;
    publicMethods.now = now;
    publicMethods.pushLogElementGUID = pushLogElementGUID;
    publicMethods.testmethod = function() {
        alert('weeehoooo');
        return 1337;
    }
    publicMethods.getSelectionInfo = getSelectionInfo;
    publicMethods.playEventFor = playEventFor; //function playEventFor(logType, elementPath, logElement /*json*/) 
    //publicMethods.playEventFor2 = playEventFor2;
    publicMethods.LogType = LogType;
    publicMethods.getJQueryElementByElementPath = getJQueryElementByElementPath;
    publicMethods.getElementPath = getElementPath;
    publicMethods.stopImmediatePropagationCalled = false;
    publicMethods.stopPropagationCalled = false;
    
    return publicMethods;
}());

Event.prototype.origStopImmediatePropagation = Event.prototype.stopImmediatePropagation;
Event.prototype.origStopPropagation = Event.prototype.stopPropagation;
Event.prototype.stopImmediatePropagation = function () {
    logRecorderAndPlayer.stopImmediatePropagationCalled = true;
    this.origStopImmediatePropagation();
} //stop all other handlers from being called

Event.prototype.stopPropagation = function () {
    logRecorderAndPlayer.stopPropagationCalled = true;
    this.origStopPropagation();
} //stop parent handlers from being called

$.fn.origSize = $.fn.size;
$.fn.size = function () {
    if ($.fn.origSize)
        return $.fn.origSize();
    return this.length;
};

