<%@ Page Title="" Language="C#" MasterPageFile="~/Page.Master" AutoEventWireup="true" CodeBehind="FirstPage.aspx.cs" Inherits="LogThisWebApplication.FirstPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label runat="server" id="someLabel"></asp:Label>
    Server textbox: <asp:TextBox runat="server" id="serverTextbox"></asp:TextBox><br/>
    Server textbox2: <asp:TextBox runat="server" id="serverTextbox2"></asp:TextBox><br/>
    <asp:Button runat="server" id="serverButton" Text="Fetch current time" OnClick="serverButton_OnClick" /><br/>
    <asp:Button runat="server" id="serverButton2" Text="Fetch current time2" OnClick="serverButton2_OnClick" /><br/>
    <asp:Button runat="server" id="redirectButton" Text="Redirect to other page" OnClick="redirectButton_OnClick" /><br/>
    <br/>
    <br/>
    Client textbox with id and no class: <input id="clientTextboxWithID"/><br/>
    Client textbox with class and no id: <input class="clientTextboxWithNoID"/><br/>
    Client textbox with no id and no class, and no type: <input /><br/>
    Client textbox with no id and no class, but with type: <input type="text"/><br/>
    <br/>
    <br/>
    <input type="button" value="Call generic handler" id="btncallgenerichandler"/><br/><input type="button" value="Call generic handler2" id="btncallgenerichandler2" onclick="console.log('1111')"/><br/><input type="button" value="Call generic handler2" id="btncallgenerichandler3"/><br/>
    <input type="button" value="Call generic handler4 no click event" id="btncallgenerichandler4noclickevent"/><br/>
    Input for generic handler: <input id="inputValueForGenericHandler" style="width:300px"/><br/>
    Output for generic handler: <input id="outputValueForGenericHandler" style="width:300px"/><br/>
    <br/>
    <br/>
    <a href="http://www.google.dk">Google default</a><br/>
    <a href="http://www.google.dk" target="_blank">Google _blank</a><br/>
    <a href="http://www.google.dk" target="_self">Google _self</a><br/>
    <a href="http://www.google.dk" target="_parent">Google _parent</a><br/>
    <a href="http://www.google.dk" target="_top">Google _top</a><br/>
    
    <div id="ost">
	    <input id="firstinput" class="qwerty" value="wtf" onchange="testChange()" onfocus="return someFocusFunc()" onfocus2="someFocusFunc(1,2,3,4, funcResult())" onkeydown="testKeyDown()" onmousedown="testMouseDown()"/>
	    <div>
	        <input class="qwerty bah" onclick="testMethod()"/>
	    </div>
	    <div>
	        <input class="qwerty" onclick="testMethod()"/>
	        <input class="qwerty" onclick="testMethod()"/>
	    </div>
    </div>
    <input type="button" value="Tryk på mig click" onclick="callFocusMethod('#whatwhat')"/>
    <input type="button" value="Tryk på mig focus" onclick="callFocusMethod('#ost,1!.qwerty')"/>
    <div class="parentparentStopPropagation">
        <div class="parentStopPropagation">
            <input type="button" value="test stopPropagation" class="testStopPropagation" id="whatwhat"/>
        </div>
    </div>
    
    <br/><br/>Different input elements:<br/>
    <input id="i0" value="default"/>
checkbox:<input id="i1" type="checkbox" value="1" /><br/>
radio:<input id="i12" type="radio" value="1" /><br/>
file:<input id="i6" type="file" value="1" /><br/> <!-- Due to security it is forbidden to set this file-input's value. But might be able to simulate the request.. unless javascript check if its empty before submitting. -->
range:<input id="i13" type="range" value="50" /><br/> <!-- default min/max is 0/100 -->
<br/>
color:<input id="i2" type="color" name="favcolor" value="#ff0000" /><br/> <!-- Not supported in IE, just shown like a normal input -->
date:<input id="i3" type="date" value="12-12-2012" /><br/>  <!-- Not supported in IE, just shown like a normal input -->
datetime-local:<input id="i4" type="datetime-local" value="12-12-2012" /><br/>  <!-- Not supported in IE, just shown like a normal input -->
email:<input id="i5" type="email" value="asd@asd.dk" /><br/> <!-- Not supported in IE -->
hidden:<input id="i7" type="hidden" value="1" /><br/> <!-- input -->
image:<input id="i8" type="image" value="1" /><br/> <!-- image, cannot change -->
month:<input id="i9" type="month" value="1" /><br/> <!-- Not supported in IE, just shown like a normal input -->
number:<input id="i10" type="number" value="1" /><br/> <!-- Not supported in IE, just shown like a normal input -->
password:<input id="i11" type="password" value="1" /><br/> <!-- input -->
reset:<input id="i14" type="reset" value="1" /><br/> <!-- Button -->
search:<input id="i15" type="search" value="1" /><br/> <!-- shown like a normal input -->
submit:<input id="i16" type="submit" value="1" /><br/> <!-- Button -->
tel:<input id="i17" type="tel" value="1" /><br/> <!-- shown like a normal input -->
text:<input id="i18" type="text" value="1" /><br/> <!-- shown like a normal input, because it is.. text is default -->
time:<input id="i19" type="time" value="1" /><br/> <!-- Not supported in IE, just shown like a normal input -->
url:<input id="i20" type="url" value="http://www.itu.dk" /><br/> <!-- Not supported in IE, just shown like a normal input -->
week:<input id="i21" type="week" value="1" /><br/> <!-- Not supported in IE, just shown like a normal input -->
    
<input type="button" value="test input elements" onclick="testInputElements()"/>   

<br><br>
<div id="div1" ondrop="drop(event)" ondragover="allowDrop(event)"></div>
<br>
<img id="drag1" src="img_logo.gif" draggable="true" ondragstart="drag(event)" width="336" height="69">
<br><br>
<input id="btnTestPropEvent" type="button" value="test prop event" onclick="testPropEvent()"/>   
<br><br>
<input type="button" value="test playloop" onclick="playEventFor()"/>   
<br><br>
<input id="btnTestGetselectioninfo" type="button" value="test getselectioninfo" onclick="testGetSelectionInfo()"/>   
<br><br>
<input type="button" value="test location change" onclick="window.location='firstpage.aspx'"/>   

    
    <style>
    #div1 {
        width: 350px;
        height: 70px;
        padding: 10px;
        border: 1px solid #aaaaaa;
    }
    </style>
    <script type="text/javascript">
        

        $("#clientTextboxWithID").on('select', function (event) {
            console.log('select!');
        });

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
                windowLeft + windowWidth > elmLeft) {
                alert("its visible");
                return; //elm is visible, do nothing
            }

            alert('okay, will scroll to it!');
            window.scrollBy(elmLeft - windowLeft, elmTop - windowTop);
        }


        $("#btnTestGetselectioninfo").on('click', function (event) {
            setTimeout(function () {

                scrollToElement($("#i21"));

                //var $this = $("#clientTextboxWithID");
                //$this.val($this.val() + 'x');
            }, 5000);
        });

        function testGetSelectionInfo() {


            //var $elm = logRecorderAndPlayer.getJQueryElementByElementPath("#clientTextboxWithID");
            //var v = logRecorderAndPlayer.getSelectionInfo($elm[0]);
            //alert(JSON.stringify(v));
        }

        function testInvokeScript(someValue) {
            var $elm = logRecorderAndPlayer.getJQueryElementByElementPath("#firstinput");
            $elm.val("tjuhej");
            return 1337;
        }

        function unixTimestamp(dt) { //seconds since 1970/1/1
            if (typeof (dt) == "undefined" || dt == null)
                dt = new Date();
            return dt.getTime() / 1000.0;
        }

        function playEventFor() {

            var loopTotal = 10;
            var timeoutInSec = 10;

            playLoop(loopTotal,
                function () { //condition
                    return false;
                },
                function (loopCounter) { //execute
                    alert('execute ' + loopCounter);
                },
                function (loopCounter/*n-1..0*/) { //progress             
                    console.log('progress ' + loopCounter);
                },
                function (timedout) { //done
                    if (timedout) {
                        alert('Error occured while playing events (Timeout exception)');
                        return;
                    }
                    alert('the end');
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
                if (conditionFunc()) {
                    executeFunc(loopCounter);
                    if (progressFunc)
                        progressFunc(loopCounter);
                    playLoop(loopCounter - 1, conditionFunc, executeFunc, progressFunc, doneFunc, timeoutInSec, undefined);
                } else {
                    playLoop(loopCounter, conditionFunc, executeFunc, progressFunc, doneFunc, timeoutInSec, roundStart);
                }
            }, 100);
        }        

        function testPropEvent() {
            var $btnTestPropEvent = $("#btnTestPropEvent");
            var f = $btnTestPropEvent.prop("onclick");
            alert(f);
        }

        function allowDrop(ev) {
            ev.preventDefault();
        }

        function drag(ev) {
            ev.dataTransfer.setData("text", ev.target.id);
            //ev.dataTransfer.setData("text", "woohooo"); //ev.target.id);
            //ev.dataTransfer.setData("text/plain", "woohooo");
            //ev.dataTransfer.clearData();
            //ev.dataTransfer.setData("url", "http://localhost:61027/img_logo.gif");
        }

        function drop(ev) {
            var dEvent = document.createEvent("DragEvent");
            dEvent.initDragEvent(
                'drag',
                true,
                true,
                null,
                1,
                2,
                3,
                4,
                5,
                true,
                true,
                true,
                true,
                1,
                document.getElementById('div1'), // null, //relatedTargetArg: EventTarget, 
                null); //dataTransferArg: DataTransfer): void;
            dEvent.hest = "woohooo";
            dEvent.detail = 2;

            window.event = dEvent;

            ev.preventDefault();
            var data = ev.dataTransfer.getData("text");
            ev.target.appendChild(document.getElementById(data));
        }

        function JSONEvent(event) {
            var r = {};
            for (var key in event) {
                //s += key + ' : ' + typeof (key) + '\n';
                var v = event[key];
                var type = (typeof (v) + "").toLowerCase();
                if (type !== 'function' && type !== 'object') {
                    console.log(type);
                    r[key] = v;
                }
            };
            return r;
        }

        function testChange() {
            console.log("change");
            console.log(JSONEvent(event));
        }

        $("#firstinput").on('change', function (event) {
            console.log("jchange");
            console.log(JSONEvent(event));
        });




        function testMouseDown() {
            var xxx = document.createEvent('Event');

            ////Creates a Event object.
            ////document.createEvent("Event").initEvent

            ////Creates a FocusEvent object.
            //var fEvent = document.createEvent("FocusEvent");
            //fEvent.initFocusEvent(
            //    'blur', //typeArg, 
            //    true, //canBubbleArg, 
            //    true, //cancelableArg, 
            //    null, //viewArg, 
            //    null, //detailArg, 
            //    null); //relatedTargetArg

            ////Creates a DragEvent object.
            //var dEvent = document.createEvent("DragEvent");
            //dEvent.initDragEvent()
            //initDragEvent(
            //    typeArg: string, 
            //    canBubbleArg: boolean, 
            //    cancelableArg: boolean, 
            //    viewArg: Window, 
            //    detailArg: number, 
            //    screenXArg: number, 
            //    screenYArg: number, 
            //    clientXArg: number, 
            //    clientYArg: number, 
            //    ctrlKeyArg: boolean, 
            //    altKeyArg: boolean, 
            //    shiftKeyArg: boolean, 
            //    metaKeyArg: boolean, 
            //    buttonArg: number, 
            //    relatedTargetArg: EventTarget, 
            //    dataTransferArg: DataTransfer): void;

            ////Creates a MouseEvent object.
            //document.createEvent("MouseEvent").initMouseEvent
            //var mEvent = document.createEvent("MouseEvent");
            //mEvent.initMouseEvent(
            //    typeArg: string, 
            //    canBubbleArg: boolean, 
            //    cancelableArg: boolean, 
            //    viewArg: Window, 
            //    detailArg: number, 
            //    screenXArg: number, 
            //    screenYArg: number, 
            //    clientXArg: number, 
            //    clientYArg: number, 
            //    ctrlKeyArg: boolean, 
            //    altKeyArg: boolean, 
            //    shiftKeyArg: boolean, 
            //    metaKeyArg: boolean, 
            //    buttonArg: number, 
            //    relatedTargetArg: EventTarget);

            //var elementValue = {
            //    value: {
            //        event: {
            //            type: 'keypress',
            //            bubbles: false,
            //            cancelable: false,
            //            key: "h"
            //        }
            //    }
            //};

            //var keyboardEvent = document.createEvent("KeyboardEvent");
            //var hmm = keyboardEvent.initKeyboardEvent(
            //    elementValue.value.event.type,
            //    elementValue.value.event.bubbles,
            //    elementValue.value.event.cancelable,
            //    null, //viewArg,
            //    //elementValue.value.event.char, // charArg,
            //    elementValue.value.event.key, // keyArg,
            //    null, //locationArg,
            //    "Control Shift Alt", //modifiersListArg,
            //    false,//repeat
            //    null //locale
            //);

            //var keyevent = document.createEvent("KeyboardEvent");
            //keyevent.initKeyboardEvent("keypress",       // typeArg,                                                           
            //    true,             // canBubbleArg,                                                        
            //    true,             // cancelableArg,                                                       
            //    null,             // viewArg,  Specifies UIEvent.view. This value may be null.     
            //    false,            // ctrlKeyArg,                                                               
            //    false,            // altKeyArg,                                                        
            //    false,            // shiftKeyArg,                                                      
            //    false,            // metaKeyArg,                                                       
            //    9,               // keyCodeArg,                                                      
            //    0);              // charCodeArg);

            //var mevent = document.createEvent("MouseEvent");
            ////mevent.initMouseEvent(type,
            ////    canBubble,
            ////    cancelable,
            ////    view,
            ////    detail,
            ////    screenX,
            ////    screenY,
            ////    clientX,
            ////    clientY,
            ////    ctrlKey,
            ////    altKey,
            ////    shiftKey,
            ////    metaKey,
            ////    button,
            ////    relatedTarget);

            //console.log("mouseDown");
            ////console.log(JSONEvent(event));
        }

        $("#firstinput").on('mousedown', function(event) {
            console.log("jmousedown");
            //console.log(JSONEvent(event));
        });

        function testKeyDown() {
            console.log("keydown");
            //console.log(JSONEvent(event));
        }

        $("#firstinput").on('keydown', function (event) {
            console.log("jkeydown");
            //console.log(JSONEvent(event));
        });

        $("#i11").on('keypress', function (event) { //password is now tested to ensure keypress contains the charcode
            if (!event)
                event = window.event;
            var charCode = event.which || event.keyCode;
            var ch = String.fromCharCode(charCode);
            console.log('keypress: ' + $("#i11").val());
        });

        $("#i11").on('keyup', function (event) { //password is now tested to ensure keypress contains the charcode
            console.log('keyup:    ' + $("#i11").val());
        });

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

        function setSelectionText(elm, startPos, endPos) {
            elm.focus();
            if (typeof elm.selectionStart != "undefined") {
                elm.selectionStart = startPos;
                elm.selectionEnd = endPos;
            } else if (document.selection && document.selection.createRange) {
                // IE branch
                elm.select();
                var range = document.selection.createRange();
                range.collapse(true);
                range.moveEnd("character", endPos);
                range.moveStart("character", startPos);
                range.select();
            }
        }        

        function testInputElements() { //input.type burde nok indgå i elementpath, da dette er lige så vigtigt som nodeName
            setTimeout(function() {
                var elm = document.getElementById("clientTextboxWithID");
                var range = document.selection.createRange();
                alert(range);
            },5000);
            

            //setSelectionText(document.getElementById("clientTextboxWithID"), 2, 3);

            //alert("i1.val = " + $("#i1").val()+" : i1.checked="+$("#i1").prop("checked")); //checkbox
            ////$("#i1").prop("checked", true);

            //alert("i12.val = " + $("#i12").val() + " : i12.checked=" + $("#i12").prop("checked")); //radio

            //alert("i6.val = " + $("#i6").val()); //file

            //$("#i13").val(75); //range

            //alert("i0.type = " + $("#i0").prop("type"));
            //alert("i1.type = " + $("#i1").prop("type"));
            //alert("i18.type = " + $("#i18").prop("type"));
        }

        $(".parentparentStopPropagation").on('click', '.testStopPropagation', function () {
            //event.stopImmediatePropagation();
            console.log(".parentparentStopPropagation.testStopPropagation.click1");
        });

        $(".parentparentStopPropagation").on('click', '.testStopPropagation', function () {
            console.log(".parentparentStopPropagation.testStopPropagation.click2");
        });

        $(".parentStopPropagation").on('click', '.testStopPropagation', function () {
            //event.stopImmediatePropagation(); 
            console.log(".parentStopPropagation.testStopPropagation.click1");
        });

        $(".parentStopPropagation").on('click', '.testStopPropagation', function () {
            console.log(".parentStopPropagation.testStopPropagation.click2");
        });

        $(".testStopPropagation").on('click', function (event) {
            var $elm = logRecorderAndPlayer.getJQueryElementByElementPath("#firstinput");
            var x = $elm.prop("onfocus");
            var r = x.call($elm[0]);

            alert(typeof(r) != "undefined" && !r);

            //event.stopPropagation();
            //event.stopImmediatePropagation(); //this will only work when doing it on events directly binded to the element, otherwise it works just like stopPropagation
            console.log(".testStopPropagation.click1");
        });

        $(".testStopPropagation").on('click', function () {
            console.log(".testStopPropagation.click2");
        });

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
            if ($curr.size() == 0) //e.g. $(document).parent()
                return [];

            level = typeof (level) == "undefined" ? 0 : level;

            //AND: a=$('[myc="blue"][myid="1"][myid="3"]');
            //OR: a=$('[myc="blue"],[myid="1"],[myid="3"]');

            var result = [];

            var events = ($._data || $.data)($curr[0], 'events');
            if (events) {
                var typeEvents = events[eventType];
                if (typeEvents) {
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

        //var globalBandit = null;
        //var globalBandit2 = null;

        //Event.prototype.origStopImmediatePropagation = Event.prototype.stopImmediatePropagation;
        //Event.prototype.origStopPropagation = Event.prototype.stopPropagation;
        //Event.prototype.stopImmediatePropagation = function () {
        //    logRecorderAndPlayer.stopImmediatePropagationCalled = true;
        //    this.origStopImmediatePropagation();
        //} //stop all other handlers from being called

        //Event.prototype.stopPropagation = function () {
        //    logRecorderAndPlayer.stopPropagationCalled = true;
        //    this.origStopPropagation();
        //} //stop parent handlers from being called

        function callFocusMethod(elementPath) {
            var $elm = logRecorderAndPlayer.getJQueryElementByElementPath(elementPath);
            var $events = getJQueryEvents($elm, $elm, 'click');
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

                v.handler.call($elm[0], document.createEvent('Event'));

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

            //var f = myfunc; // globalBandit.prop("onfocus"); //f is a function, not a string as first expected
            //var f2 = $elm.prop("onfocus");
            //if (typeof (f) != "undefined") {
            //    //alert(typeof (f));
            //    //alert(f);
            //    elm = $elm[0];
            //    alert(f);
            //    alert(typeof(f));
            //    alert(f2);
            //    alert(typeof (f2));
            //    alert('calling3 ' + $(elm).prop("class"));
            //    f.call(elm, null);
            //    f2.call(elm, null);
            //}


        }

        function funcResult() {
            return 1337;
        }

        var myfunc = function () {
            //alert(typeof(this)); //.name);
            //            alert(this.value);
            alert('calling6 ' + $(this).prop("class"));
            //alert('calling5 ' + $(elm).prop("class"));
        };

        var obj_a = {
            name: "FOO"
        };

        var obj_b = {
            name: "BAR!!"
        };

        function someFocusFunc(a, b, c, that, funcResult) { //Ahh... cannot parse this-context for attribute-events like normal jQuery-events, because they are embedded methods
            return false;
            var event2 = document.createEvent('Event');

            alert('someFocusFunc ' + $(this).prop("class")); //Har behov for at oprette event
            return;

            //var $this = $(this);
            //alert($this.prop("nodeName"));
            //alert("received " + $this.prop("class"));
            //return; //WHAT this is not set by doing onfocus="someFocusFunc" or "someFocusFunc()"?!?

            //myfunc.call(obj_a);
            //myfunc.call(obj_b);
            var firstinput = document.getElementById("firstinput");
            //alert(typeof (this));
            alert(firstinput.value);
            myfunc.call(firstinput);

        }

        function testMethod() {
            var s = logRecorderAndPlayer.getElementPath(this);
            var $o = logRecorderAndPlayer.getJQueryElementByElementPath(s);

            var b1 = validateSelectorAgainstElement(".qwerty", $o);
            var b2 = validateSelectorAgainstElement(".qwerty2", $o);
            var b3 = validateSelectorAgainstElement(".qwerty,.woohoo", $o);
            var b4 = validateSelectorAgainstElement(".qwerty .woohoo", $o);
            var $events = getJQueryEvents($o, $o, 'focus');

            return;
            logRecorderAndPlayer.playEventFor(logRecorderAndPlayer.LogType.OnFocus, s);

            var events = $._data($o[0], 'events');
            var docEvents = $._data(document, 'events');

            //alert(o.value);
        }

        $("#ost").on('focus', '.qwerty', function () {
            //alert('event hosted by #ost');
            console.log('event hosted by #ost');
        });

        $(".qwerty").on("focus", function (e) {
            console.log('event hosted by .qwerty itself');
            //alert('event hosted by .qwerty itself');
            e.stopPropagation();
            e.stopImmediatePropagation();
        });
        $(".qwerty").on("blur", function () {
            //alert('event hosted by .qwerty itself');
        });

        var counter = 0;
        var $document = $(document);
        $document.on('focus', '.qwerty', function () {
            var $this = $(this);

            //alert('focus?');

            $this.val(counter);

            counter++;
        });

        $document.on('focus', '.bah', function () {
            var $this = $(this);

            //alert('focus?');

            $this.val(counter);

            counter++;
        });

        $("#btncallgenerichandler").on('click', function () {
            var request = {
                SomeValue: $("#inputValueForGenericHandler").val()
            };
            //alert('firstpage ajax send first');
            $.ajax({
                url: "/TestHandler.ashx",
                data: {
                    'request': JSON.stringify(request)
                },
                success: function (data) {
                    $("#outputValueForGenericHandler").val(data.SomeValue);
                },
                error: function (data) {
                    $.alert("Error occured while calling TestHandler.ashx", "Error");
                }
            });
        });

        function staticFunc() {
            console.log('staticFunc');
        }

        var funcCompareThingy = null;

        function getLRAPEvent(that, eventType, lrapFn) {
            var events = $._data(that, 'events');
            //console.log('getLRAPEvent 1');
            if (events) {
                //console.log('getLRAPEvent 2');
                var typeEvents = events[eventType];
                //console.log(events);
                if (typeEvents) {
                    //console.log('getLRAPEvent 3');

                    var handlerInfo = $.findFirst(typeEvents,
                        function (v, i) {
                            //console.log('getLRAPEvent loop ' + i);
                            return v.handler === lrapFn;
                        });
                    if (handlerInfo)
                        return handlerInfo;
                }
            }

            return null;
        }

        function removeEventByIndex(that, eventType, index) {
            var events = $._data(that, 'events');
            if (events) {
                var typeEvents = events[eventType];
                if (typeEvents) {
                    typeEvents.splice(index, 1);
                }
            }
        }

        function removeLRAPEvent(that, eventType, lrapFn) {
            var handlerInfo = getLRAPEvent(that, eventType, lrapFn);
            if (handlerInfo) {
                removeEventByIndex(that, eventType, handlerInfo.index);
            }
        }

        function moveLastEventToFirst(elm, action) {
            var handlers = $._data(elm, 'events')[action];
            if (!handlers)
                return;
            var handler = handlers.pop();
            handlers.splice(0, 0, handler);
        }

        var testClass = (function () {
            function someClickFunction() {
                console.log('first one to rule em all!!!444');
            }

            function setupLRAPEvent(that, eventType/*e.g. 'click'*/, eventMethod) {
                var $that = $(that);
                var onEventProp = $that.prop('on' + eventType);                
                if (onEventProp) {
                    var f = function (event) {
                        eventMethod(that, eventType);
                        return onEventProp(event);
                    };

                    if ($that.data('lrapOn' + eventType) !== onEventProp) {
                        console.log('is not the same method');
                        eval("that.on" + eventType + "=f");
                        //this.onclick = f; // function () { console.log('1337') }; //Wont work, because the order of the function is behind everything else (even jQuery-bound-events). Det virker dog hvis man ikke kalder $this.prop('onclick', null) først!!!
                        $that.data('lrapOn' + eventType, f); //Kan ikke anvende prop til functions... åbenbart... men derimod data kan jeg anvende!
                    } else {
                        console.log('is the same method');
                    }
                    removeLRAPEvent(that, eventType, eventMethod);
                } else {
                    if (!getLRAPEvent(that, eventType, eventMethod)) {
                        console.log('setup ' + eventType);
                        $that.on(eventType, eventMethod);
                        moveLastEventToFirst(this, eventType);

                        //console.log(getLRAPEvent(that, eventType, eventMethod));

                    } else {
                        console.log('found lrap method!!!');
                    }
                }
            }            

            function setupInputEvents() {
                function embeddedClickFunction() {
                    console.log('embedded click function also works!');
                }

                //$(document).on('mousedown', 'input', function () {
                //    var $this = $(this);
                //    console.log('orig mousedown ' + $this.prop("id"));
                //});

                //$(document).on('mouseenter mousedown', 'input', function () {
                //    var $this = $(this);
                //    console.log('orig mouseenter mousedown' + $this.prop("id"));
                //});

                //$(document).on('keydown', 'input', function () {
                //    var $this = $(this);
                //    console.log('orig keydown '+$this.prop("id"));
                //});

                //$(document).on('mouseenter', 'input', function () {
                //    alert('mouseenter');
                //    console.log('mouseenter');
                //});

                $(document).on('beforeactivate', 'input', function () {
                    var $this = $(this);
                    console.log('orig beforeactivate ' + $this.prop("id"));
                    //var $this = $(this);
                    if (funcCompareThingy === embeddedClickFunction) {
                        console.log('TRUE-TRUE-TRUE-TRUE-TRUE-TRUE-TRUE-TRUE-TRUE-TRUE');
                    } else {
                        console.log('FALSE-FALSE-FALSE-FALSE-FALSE-FALSE-FALSE-FALSE');
                    }
                    funcCompareThingy = embeddedClickFunction;

                    setupLRAPEvent(this, 'click', embeddedClickFunction);
                    //setupLRAPEvent(this, 'click', function () {
                    //    console.log('anonymous function also works?!');
                    //    //console.log('lrap click event');
                    //    //console.log(event);
                    //});
                    
                    //var onClickProp = $this.prop('onclick');
                    //if (onClickProp) {
                    //    var f = function (event) {
                    //        someClickFunction();
                    //        return onClickProp(event);
                    //    };

                    //    if ($this.data('lrapOnClick') != onClickProp) {
                    //        this.onclick = f; // function () { console.log('1337') }; //Wont work, because the order of the function is behind everything else (even jQuery-bound-events). Det virker dog hvis man ikke kalder $this.prop('onclick', null) først!!!
                    //        $this.data('lrapOnClick', f); //Kan ikke anvende prop til functions... åbenbart... men derimod data kan jeg anvende!
                    //    }
                    //    removeLRAPEvent(this, 'click', someClickFunction);
                    //} else {
                    //    if (!getLRAPEvent(this, 'click', someClickFunction)) {
                    //        $this.on('click', someClickFunction);
                    //        moveLastEventToFirst(this, 'click');
                    //    }
                    //}
                });
            }

            var api = {};
            api.setupInputEvents = setupInputEvents;

            return api;
        }());

        testClass.setupInputEvents();

        //var oldJQueryEventTrigger = jQuery.event.trigger;
        //jQuery.event.trigger = function (event, data, elem, onlyHandlers) {
        //    console.log(event); //, data, elem, onlyHandlers);
        //    oldJQueryEventTrigger(event, data, elem, onlyHandlers);
        //}

        ////////////////////////////////////////////////////////////////////////

        //var Wiretap = (function () {
        //    "use strict";

        //    function defaults(settings) {
        //        var d = Wiretap.defaultSettings,
        //            i;
        //        for (i in d) if (d.hasOwnProperty(i)) {
        //            settings[i] = settings[i] !== undefined ? settings[i] : d[i];
        //        }

        //        return settings;
        //    }

        //    function Wiretap(settings) {
        //        this.settings = settings = defaults(settings);

        //        if (settings.add !== null) {
        //            var aEL = HTMLElement.prototype.addEventListener,
        //                add = settings.add;

        //            HTMLElement.prototype.addEventListener = function (eventName, callback) {
        //                add.apply(this, arguments);

        //                aEL.apply(this, [eventName, function () {
        //                    if (settings.before) {
        //                        settings.before.apply(this, arguments);
        //                    }

        //                    callback.apply(this, arguments);

        //                    if (settings.after) {
        //                        settings.after.apply(this, arguments);
        //                    }
        //                }]);
        //            };
        //        }
        //    }

        //    Wiretap.prototype = {};

        //    Wiretap.defaultSettings = {
        //        add: null,
        //        before: null,
        //        after: null
        //    };

        //    return Wiretap;
        //})();

        //new Wiretap({
        //    add: function () {
        //        console.log('wiretap.add');
        //        //fire when an event is bound to element
        //    },
        //    before: function () {
        //        console.log('wiretap.before');
        //        //fire just before an event executes, arguments are automatic
        //    },
        //    after: function () {
        //        //fire just after an event executes, arguments are automatic
        //        console.log('wiretap.after');
        //    }
        //});

        ////////////////////////////////////////////////////////////////////////

        //function setJQueryEventHandlersDebugHooks(bMonTrigger, bMonOn, bMonOff) {
        //    jQuery.fn.___getHookName___ = function () {
        //        // First, get object name
        //        var sName = new String(this[0].constructor),
        //        i = sName.indexOf(' ');
        //        sName = sName.substr(i, sName.indexOf('(') - i);

        //        // Classname can be more than one, add class points to all
        //        if (typeof this[0].className === 'string') {
        //            var sClasses = this[0].className.split(' ');
        //            sClasses[0] = '.' + sClasses[0];
        //            sClasses = sClasses.join('.');
        //            sName += sClasses;
        //        }
        //        // Get id if there is one
        //        sName += (this[0].id) ? ('#' + this[0].id) : '';
        //        return sName;
        //    };

        //    var bTrigger = (typeof bMonTrigger !== "undefined") ? bMonTrigger : true,
        //        bOn = (typeof bMonOn !== "undefined") ? bMonOn : true,
        //        bOff = (typeof bMonOff !== "undefined") ? bMonOff : true,
        //        fTriggerInherited = jQuery.fn.trigger,
        //        fOnInherited = jQuery.fn.on,
        //        fOffInherited = jQuery.fn.off;

        //    if (bTrigger) {
        //        jQuery.fn.trigger = function () {
        //            console.log(this.___getHookName___() + ':trigger(' + arguments[0] + ')');
        //            return fTriggerInherited.apply(this, arguments);
        //        };
        //    }

        //    if (bOn) {
        //        jQuery.fn.on = function () {
        //            if (!this[0].__hooked__) {
        //                this[0].__hooked__ = true; // avoids infinite loop!
        //                console.log(this.___getHookName___() + ':on(' + arguments[0] + ') - binded');
        //                $(this).on(arguments[0], function (e) {
        //                    console.log($(this).___getHookName___() + ':' + e.type);
        //                });
        //            }
        //            var uResult = fOnInherited.apply(this, arguments);
        //            this[0].__hooked__ = false; // reset for another event
        //            return uResult;
        //        };
        //    }

        //    if (bOff) {
        //        jQuery.fn.off = function () {
        //            if (!this[0].__unhooked__) {
        //                this[0].__unhooked__ = true; // avoids infinite loop!
        //                console.log(this.___getHookName___() + ':off(' + arguments[0] + ') - unbinded');
        //                $(this).off(arguments[0]);
        //            }

        //            var uResult = fOffInherited.apply(this, arguments);
        //            this[0].__unhooked__ = false; // reset for another event
        //            return uResult;
        //        };
        //    }
        //}
        //setJQueryEventHandlersDebugHooks(); //http://stackoverflow.com/questions/7439570/how-do-you-log-all-events-fired-by-an-element-in-jquery
        ////////////////////////////////////////////////////////////////////////

        $("#btncallgenerichandler3").on('click', function() {
            //var $two = $("#btncallgenerichandler2");
            //var two = $two[0];

            ////$two.prop('onclick', null);
            //two.onclick = function () { console.log('noget nyt og spændende!!'); };

            console.log('new method!!!!!!!!!!!!!!!!!!!!!!!!!1111');

            $(document).on('mousedown', 'input', function () {
                console.log('orig mousedown 2');
            });
            console.log('new method!!!!!!!!!!!!!!!!!!!!!!!!!2222');

        });

        

        $("#btncallgenerichandler2").on('click', function() {
            console.log(2222);
        });

        $.extend({
            findFirst: function (elems, validateCb) {
                var i;
                for (i = 0 ; i < elems.length ; ++i) {
                    if (validateCb(elems[i], i))
                        return { index: i, value: elems[i] };
                }
                return undefined;
            }
        });


        //Event.prototype.stopImmediatePropagation = function () { alert('abc'); }
        //Event.prototype.stopPropagation = function () { alert('123'); }
        //$('input').on('click', function (e) {
        //    e.stopPropagation();
        //})

        $.extend({
            alert: function (message, title, closeFn) {
                $("<div></div>").dialog({
                    buttons: { "Ok": function () { $(this).dialog("close"); } },
                    close: function (event, ui) {
                        $(this).remove();
                        if (closeFn) closeFn();
                    },
                    resizable: false,
                    title: title,
                    modal: true
                }).text(message);
            }
        });

        //$.ajaxSetup({
        //    beforeSend: function () {
        //        alert('test before send');
        //    },
        //    success: function () {
        //        //$(".button").button("enable");
        //    }
        //});


    </script>

</asp:Content>