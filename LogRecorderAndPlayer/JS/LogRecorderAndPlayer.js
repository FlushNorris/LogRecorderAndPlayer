var logRecorderAndPlayer = (function () {

    var handlerLRAPUrl = "/logrecorderandplayerhandler.lrap";

    var pageGUIDCache = null;
    var sessionGUIDCache = null;

    function setPageGUID(_pageGUID) {
        var v = getPageGUID();
        if (v == null) {
            pageGUIDCache = _pageGUID;
            var $frm = getPrimaryForm();
            $("<input type='hidden' />")
                .attr("class", "LRAP-PAGEGUID")
                .attr("name", "LRAP-PAGEGUID")
                .val(_pageGUID)
                .appendTo($frm);
        }
    }

    function getPageGUID() {
        if (pageGUIDCache != null)
            return pageGUIDCache;

        var $frm = getPrimaryForm();
        var $pageGUID = $frm.find(".LRAP-PAGEGUID");
        if ($pageGUID.size() != 0) 
            pageGUIDCache = $pageGUID.val();       
        return pageGUIDCache;
    }

    function setSessionGUID(_sessionGUID) {
        var v = getSessionGUID();
        if (v == null) {
            sessionGUIDCache = _sessionGUID;
            var $frm = getPrimaryForm();
            $("<input type='hidden' />")
                .attr("class", "LRAP-SESSIONGUID")
                .attr("name", "LRAP-SESSIONGUID")
                .val(_sessionGUID)
                .appendTo($frm);
        }
    }

    function getSessionGUID() {
        if (sessionGUIDCache != null)
            return sessionGUIDCache;

        var $frm = getPrimaryForm();
        var $sessionGUID = $frm.find(".LRAP-SESSIONGUID");
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
        OnAjaxRequestSend: 0,
        OnAjaxRequestReceived: 1,
        OnAjaxResponseSend: 2,
        OnAjaxResponseReceived: 3,
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
        OnWCFServiceRequest: 27,
        OnWCFServiceResponse: 28    
    };

    function init(sessionGUID, pageGUID) {
        setSessionGUID(sessionGUID);
        setPageGUID(pageGUID); 

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
                new Date(),
                LogType.OnAjaxResponseReceived,
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

        setupLogger();
    }

    function getSelectionText(elm) {
        var selectedText = null;

        if (elm.selectionStart != undefined) {
            var startPos = elm.selectionStart;
            var endPos = elm.selectionEnd;
            selectedText = elm.value.substring(startPos, endPos);
        }
        else
            // IE older versions
            if (document.selection != undefined) {
                var $focused = $(document.activeElement);
                elm.focus(); //TODO Ignore this logevent for focus and blur
                var sel = document.selection.createRange();
                selectedText = sel.text;
                $focused.focus(); //TODO Ignore this logevent for focus and blur
            }

        return selectedText;
    }

    //regionstart: Path to the element from the closest self or parent with an ID
    function trimString(x) {
        return x.replace(/^\s+|\s+$/gm, '');
    }

    function getElementIdxAndParentPath(elm, selector) {
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
            if (idxPath == -1)
                return "[ERROR]";
            parentPath = getElementPath($parent[0]);
        }
        return (parentPath ? parentPath + "," : "") + (idxPath + 1) + selector;
    }

    function getElementPath(elm) {
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
        return getElementIdxAndParentPath(elm, nodeName);
    }
    //regionend: Path to the element from the closest self or parent with an ID

    function setupBasicClientsideControlEvents($input, inputSelector) {
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

            //Avoid "bubbling" to other logevents.. but preserve the normal bubbling for other events
            var elmP = document.elementFromPoint(event.pageX, event.pageY);
            if (this != elmP)
                return;

            logElementEx(LogType.OnMouseDown, getElementPath(this), JSON.stringify(v));

            //console.log("mousedown: " + getElementPath(this) + " button=" + event.button + " shiftKey=" + event.shiftKey + " altKey=" + event.altKey + " ctrlKey=" + event.ctrlKey);
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

            //Avoid "bubbling" to other logevents.. but preserve the normal bubbling for other events
            var elmP = document.elementFromPoint(event.pageX, event.pageY);
            if (this != elmP)
                return;

            logElementEx(LogType.OnMouseUp, getElementPath(this), JSON.stringify(v));
            //            console.log("mouseup: " + getElementPath(this) + " button=" + event.button + " shiftKey=" + event.shiftKey + " altKey=" + event.altKey + " ctrlKey=" + event.ctrlKey);
        });

        $document.on('click', inputSelector, function (event) {
            if (!event)
                event = window.event;

            if (event.pageX != 0 && event.pageY != 0) {
                var elmP = document.elementFromPoint(event.pageX, event.pageY);
                if (this != elmP)
                    return;
            }

            logElementEx(LogType.OnClick, getElementPath(this), "");
            //console.log("click: " + getElementPath(this));
        });

        $document.on('dblclick', inputSelector, function (event) {
            if (!event)
                event = window.event;

            if (event.pageX != 0 && event.pageY != 0) {
                var elmP = document.elementFromPoint(event.pageX, event.pageY);
                if (this != elmP)
                    return;
            }

            logElementEx(LogType.OnDblClick, getElementPath(this), "");
            //console.log("dblclick: " + getElementPath(this));
        });

        $document.on('dragstart', inputSelector, function (event) {            
            var target = event.target ? event.target : event.srcElement;

            logElementEx(LogType.OnDragStart, getElementPath(target), "");
        });

        $document.on('dragend', inputSelector, function (event) {
            var target = event.target ? event.target : event.srcElement;

            logElementEx(LogType.OnDragEnd, getElementPath(target), "");
        });

        $document.on('dragover', inputSelector, function (event) { //To tell if it is allowed
            var target = event.target ? event.target : event.srcElement;

            logElementEx(LogType.OnDragOver, getElementPath(target), "");
        });

        $document.on('drop', inputSelector, function (event) {
            var target = event.target ? event.target : event.srcElement;

            logElementEx(LogType.OnDrop, getElementPath(target), "");
        });

        $document.on('scroll', inputSelector, function (event) {
            var $this = $(this);

            var v = {
                top: $this.scrollTop(),
                left: $this.scrollLeft()
            };

            logElementEx(LogType.OnScroll, getElementPath(this), JSON.stringify(v), compareLogElementsNoValue);
        });

        //$.each($input,
        //    function (idx, obj) {
        //        //moveLastEventToFirst(obj, 'mouseover');
        //        //moveLastEventToFirst(obj, 'mouseout');
        //        moveLastEventToFirst(obj, 'mousedown');
        //        moveLastEventToFirst(obj, 'mouseup');
        //        moveLastEventToFirst(obj, 'click');
        //        moveLastEventToFirst(obj, 'dblclick');
        //    });
    }

    function setupInputClientsideControlEvents($input, inputSelector) {

        var $document = $(document);

        $document.on('blur', inputSelector, function () {
            logElementEx(LogType.OnBlur, getElementPath(this), "");
        });

        $document.on('focus', inputSelector, function () {
            logElementEx(LogType.OnFocus, getElementPath(this), "");
        });

        $document.on('change', inputSelector, function () {
            logElementEx(LogType.OnChange, getElementPath(this), $(this).val());
        });

        $document.on('select', inputSelector, function () { //select text.. apparently no way of getting the selected text? or is there... check caret showSelectionInsideTextarea also works on inputs
            logElementEx(LogType.OnSelect, getElementPath(this), getSelectionText(this));
        });

        $document.on('copy', inputSelector, function () {
            logElementEx(LogType.OnCopy, getElementPath(this), getSelectionText(this));
        });

        $document.on('cut', inputSelector, function () {
            logElementEx(LogType.OnCut, getElementPath(this), getSelectionText(this));
        });

        $document.on('paste', inputSelector, function () {
            logElementEx(LogType.OnPaste, getElementPath(this), $(this).val());
        });

        $document.on('keydown', inputSelector,
            function (event) {
                if (!event)
                    event = window.event;
                var charCode = event.which || event.keyCode;
                var ch = String.fromCharCode(charCode);

                var v = {
                    charCode: charCode,
                    ch: ch,
                    shiftKey: event.shiftKey,
                    altKey: event.altKey,
                    ctrlKey: event.ctrlKey
                };

                logElementEx(LogType.OnKeyDown, getElementPath(this), JSON.stringify(v));
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
                ctrlKey: event.ctrlKey
            };

            logElementEx(LogType.OnKeyUp, getElementPath(this), JSON.stringify(v), compareLogElementsKeyup, combineLogElementsKeyup);
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
                ctrlKey: event.ctrlKey
            };
            
            logElementEx(LogType.OnKeyPress, getElementPath(this), JSON.stringify(v), compareLogElementsKeypress, combineLogElementsKeypress);
        });        

        //$.each($input,
        //    function (idx, obj) {
        //        moveLastEventToFirst(obj, 'blur');
        //        moveLastEventToFirst(obj, 'focus');
        //        moveLastEventToFirst(obj, 'change');
        //        moveLastEventToFirst(obj, 'select');
        //        moveLastEventToFirst(obj, 'copy');
        //        moveLastEventToFirst(obj, 'cut');
        //        moveLastEventToFirst(obj, 'paste');
        //        //moveLastEventToFirst(obj, 'keydown');
        //        moveLastEventToFirst(obj, 'keyup');
        //        moveLastEventToFirst(obj, 'keypress');
        //    });
    }

    function logWindowScroll()
    {
        var $this = $(window);

        var v = {
            top: $this.scrollTop(),
            left: $this.scrollLeft()
        };

        logElementEx(LogType.OnScroll, "window", JSON.stringify(v), compareLogElementsNoValue);
    }

    function logWindowSize() {
        var v = {
            width: window.outerWidth,
            height: window.outerHeight
        };        
        logElementEx(LogType.OnResize, "window", JSON.stringify(v), compareLogElementsNoValue);
    }

    function setupAllClientsideControlEvents() {
        var $document = $(document);
//        var $form = $("form");
        $document.on('submit', 'form', function () {
            //logElementEx(LogType.OnSubmit, getElementPath(this), "");
        });

        $document.on('reset', 'form', function () {
            //logElementEx(LogType.OnReset, getElementPath(this), "");
        });

        setupBasicClientsideControlEvents($("p"));
        setupBasicClientsideControlEvents($("div"));
        setupBasicClientsideControlEvents($("span"));
        setupBasicClientsideControlEvents($("textarea"));
        setupBasicClientsideControlEvents($("input"));
        setupInputClientsideControlEvents($("textarea"), "textarea");
        setupInputClientsideControlEvents($("input"), "input"); //last one goes on top

//        var $inputSearch = $("input[type=search]");
        $document.on('search', "input[type=search]", function () {
            logElementEx(LogType.OnSearch, getElementPath(this), $(this).val());
        });

        //var $body = $("body");

        //console.log($body.resize);
        //$body.resize(function () { //log load body size ... no, resize event is also called on load
        //    console.log('body size??');
        //    console.log("body size = " + window.outerWidth + " : " + window.outerHeight);
        //    //var w = window.outerWidth;
        //    //var h = window.outerHeight;
        //    //var txt = "Window size: width=" + w + ", height=" + h;
        //    //document.getElementById("demo").innerHTML = txt;
        //});

        var $window = $(window);

        $window.resize(logWindowSize);
        logWindowSize();

        $window.scroll(logWindowScroll);        
        logWindowScroll();

        //https://www.w3schools.com/jsref/dom_obj_event.asp
        //scroll?
        //pageshow?
        //pagehide
        //hashchange
        //error
        //search          
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

    function handleLogElements(async) {
        async = typeof (async) == "undefined" || async;

        //console.log("handleLogElements called");
        var logElementsForHandler = logElements;
        logElements = [];

        //console.log("logElementsForHandler.length = " + logElementsForHandler.length);
        //compressItAll(logElementsForHandler);
        //console.log("logElementsForHandler.length = " + logElementsForHandler.length);

        //console.log("logElementsForHandler.length = " + logElementsForHandler.length);

        if (logElementsForHandler.length > 0) {
            $.ajax({
                async: async,
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
                    console.log(logElements);
                    alert('Failed to log: Contact administrator');
                }
            });
        }
    }

    function logElementEx(logType, element, value, compareFn, combineFn) {
        logElement(getSessionGUID(), getPageGUID(), null, null, new Date(), logType, element, value, compareFn, combineFn);
    }

    function logElement(sessionGUID, pageGUID, bundleGUID, progressGUID, timestamp, logType, element, value, compareFn, combineFn) {
        if (element == null)
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
            Timestamp: getDataMemberDate(timestamp),
            LogType: logType,
            Element: htmlEncode(element), //denne burde html encodes (eller faktisk burde den kun html encodes når det ikke er status=200... hmmm... er jo heller ikke holdbart
            Element2: null,
            Value: value,
            Times: 1,
            TimestampEnd: null
        };
        logElements.push(request);

        compactLogElementList(compareFn, combineFn);
    }

    //function getCompareFnByLogType(logType) {
    //    switch (logType) {
    //        case LogType.OnKeyPress:
    //            return compareLogElementsKeypress;
    //        case LogType.OnKeyUp:
    //            return compareLogElementsKeyup;
    //        case LogType.OnScroll:
    //            return compareLogElementsNoValue;
    //        case LogType.OnResize:
    //            return compareLogElementsNoValue;
    //        default:
    //            return compareLogElements;
    //    }
    //}

    //function getCombineFnByLogType(logType) {
    //    switch(logType) {
    //        case LogType.OnKeyPress:
    //            return combineLogElementsKeypress;
    //        case LogType.OnKeyUp:
    //            return combineLogElementsKeyup;
    //        default:
    //            return combineLogElements;
    //    }
    //}   

    //function compressItAll(logElements) {
    //    for(var i = logElements.length - 1; i > 0; i--) {
    //        var le = logElements[i];

    //        var compareFn = getCompareFnByLogType(le.LogType);
    //        var combineFn = getCombineFnByLogType(le.LogType);

    //        var removedElements = compactLogElementList(logElements, i, compareFn, combineFn);
    //        //removedElements=0 træk 0 fra
    //        //removedElements=1 træk 0 fra, da den nye jo også kan collapse videre
    //        //removedElements=2 træk 1 fra da vi har mistet en
    //        //removedElements=3 træk 2 fra...

    //        //i -= (removedElements == 0 ? 0 : removedElements - 1);
    //    }
    //}

    ////Returns how many logElements were removed
    //function compactLogElementList(logElements, fromIdx, compareFn, combineFn) {
    //    var removed = 0;
    //    while (fromIdx > 0) {
    //        var secondLastRequest = logElements[fromIdx - 1];
    //        var lastRequest = logElements[fromIdx];
    //        var compareResult = compareFn(secondLastRequest, lastRequest, logElements, fromIdx);
    //        if (compareResult) {
    //            combineFn(secondLastRequest, lastRequest, compareResult);

    //            //logElements.pop(); //remove lastRequest
    //            logElements.splice(fromIdx, 1);
    //            removed++;
    //            fromIdx--;
    //        } else
    //            return;
    //    }
    //}

    function compactLogElementList(compareFn, combineFn) {
        var requestIdx = logElements.length - 1;

        while (requestIdx > 0) {
            var secondLastRequest = logElements[requestIdx - 1];
            var lastRequest = logElements[requestIdx];
            var compareResult = compareFn(secondLastRequest, lastRequest);
            if (compareResult) {
                combineFn(secondLastRequest, lastRequest, compareResult);

                logElements.pop(); //remove lastRequest
                requestIdx--;
            } else
                return;
        }
    }

    function combineLogElementsKeypress(le1, le2, compareResult) { // le1 = le1 + le2 (non pure)
        le1.TimestampEnd = le2.Timestamp;
        le1.Value = le2.Value;
        le1.LogType = le2.LogType;
        if (compareResult == 2) {
            le1.Times += le2.Times;
        }
    }

    function combineLogElementsKeyup(le1, le2, compareResult) { // le1 = le1 + le2 (non pure)
        le1.TimestampEnd = le2.Timestamp;
        if (compareResult == 2) {
            le1.Times += le2.Times;
        }
    }

    function getKeyUpDownKeypadChar(charCode) {
        if (charCode >= 96 && charCode <= 105)
            return String.fromCharCode(charCode - 48);
        return "";
    }

    function compareLogElementsKeypress(le1, le2) {
        //return 0;
        if (le1.SessionGUID == le2.SessionGUID &&
            le1.PageGUID == le2.PageGUID &&
            le1.BundleGUID == le2.BundleGUID &&
            le1.ProgressGUID == le2.ProgressGUID &&
            le1.Times == le2.Times &&
            le1.LogType == LogType.OnKeyDown &&
            le2.LogType == LogType.OnKeyPress) {

            var v1 = JSON.parse(htmlDecode(le1.Value));
            var v2 = JSON.parse(htmlDecode(le2.Value));

            if ((
                    getKeyUpDownKeypadChar(v1.charCode) == v2.ch ||
                    v1.ch.toUpperCase() == v2.ch.toUpperCase()
                ) &&
                v1.shiftKey == v2.shiftKey &&
                v1.altKey == v2.altKey &&
                v1.ctrlKey == v2.ctrlKey) {
                return 1;
            }
        }                       

        if (compareLogElements(le1, le2))
            return 2;

        return 0;
    }

    function compareLogElementsKeyup(le1, le2/*, logElements, le2Idx*/) {
        if (le1.SessionGUID == le2.SessionGUID &&
            le1.PageGUID == le2.PageGUID &&
            le1.BundleGUID == le2.BundleGUID &&
            le1.ProgressGUID == le2.ProgressGUID &&
            le1.Times == le2.Times &&
            le1.LogType == LogType.OnKeyPress &&
            le2.LogType == LogType.OnKeyUp) {

            var v1 = JSON.parse(htmlDecode(le1.Value));
            var v2 = JSON.parse(htmlDecode(le2.Value));

            if ((
                    v1.ch == getKeyUpDownKeypadChar(v2.charCode) ||
                    v1.ch.toUpperCase() == v2.ch.toUpperCase()
                ) &&
                v1.shiftKey == v2.shiftKey &&
                v1.altKey == v2.altKey &&
                v1.ctrlKey == v2.ctrlKey) {

                return 1;
                ////Extended test
                //if (le2Idx - 3 >= 0) {
                //    var leM0 = logElements[le2Idx - 2];
                //    var leM1 = logElements[le2Idx - 3];

                //    if (leM0.LogType == LogType.OnKeyDown &&
                //        leM1.LogType != LogType.OnKeyPress) {
                //        return 1;
                //    }
                //}                
            }
        }

        if (le1.Times == le2.Times && compareLogElements(le1, le2))
            return 2;

        return 0;
    }

    function combineLogElements(le1, le2) { // le1 = le1 + le2 (non pure)
        le1.Times += le2.Times;
        le1.TimestampEnd = le2.Timestamp;
        le1.Value = le2.Value;
        le1.LogType = le2.LogType;
    }

    function compareLogElementsNoValue(le1, le2) { //NoValue compare would be used with e.g. resize events
        return le1.SessionGUID == le2.SessionGUID &&
            le1.PageGUID == le2.PageGUID &&
            le1.BundleGUID == le2.BundleGUID &&
            le1.ProgressGUID == le2.ProgressGUID &&
            le1.LogType == le2.LogType &&
            le1.Element == le2.Element;
    }

    function compareLogElements(le1, le2) { //Without Timestamp compares atm... should build a delay check on the compare        
        return compareLogElementsNoValue(le1, le2) &&
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
        var guidTag = "lrap-guid";
        var sessionGUIDTag = "lrap-sessionguid";
        var pageGUIDTag = "lrap-pageguid";
        var bundleGUIDTag = "lrap-bundleguid";        

        url = addQryStrElement(removeQryStrElement(url, guidTag), guidTag, generateGUID());
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

    function bindAjaxCompleteFirst(fn) {
        $(document).ajaxComplete(fn);
        moveLastEventToFirst(document, "ajaxComplete");
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

    ///#endregion

    var publicMethods = {};

    publicMethods.init = init;
    publicMethods.getPageGUID = getPageGUID;
    publicMethods.getSessionGUID = getSessionGUID;
    
    return publicMethods;
}());

