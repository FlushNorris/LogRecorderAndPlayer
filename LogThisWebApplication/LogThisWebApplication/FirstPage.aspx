<%@ Page Title="" Language="C#" MasterPageFile="~/Page.Master" AutoEventWireup="true" CodeBehind="FirstPage.aspx.cs" Inherits="LogThisWebApplication.FirstPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    Server textbox: <asp:TextBox runat="server" id="serverTextbox"></asp:TextBox><br/>
    <asp:Button runat="server" id="serverButton" Text="Fetch current time" OnClick="serverButton_OnClick" /><br/>
    <br/>
    <br/>
    Client textbox with no id: <input class="clientTextboxWithNoID"/><br/>
    Client textbox with id: <input id="clientTextboxWithID"/><br/>
    <br/>
    <br/>
    <a href="http://www.google.dk">Google default</a><br/>
    <a href="http://www.google.dk" target="_blank">Google _blank</a><br/>
    <a href="http://www.google.dk" target="_self">Google _self</a><br/>
    <a href="http://www.google.dk" target="_parent">Google _parent</a><br/>
    <a href="http://www.google.dk" target="_top">Google _top</a><br/>
    
    <div id="ost">
	    <input id="firstinput" class="qwerty" value="wtf" onfocus="someFocusFunc()" onfocus2="someFocusFunc(1,2,3,4, funcResult())"/>
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
       
    <script type="text/javascript">
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
                if (eventType == 'focus') {
                    var typeEvents = events[eventType + "in"];
                    if (typeEvents) {

                        //vil gerne for hvert event teste om den typeEvent[n].selector matcher den originale $elm

                        result.push.apply(result, typeEvents);                        
                    }
                    //Burde overskrive event.stopPropagation() og event.stopImmediatePropagation()
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

        var globalBandit = null;
        var globalBandit2 = null;

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

        //Event.prototype.stopImmediatePropagation = function () { alert('abc'); }
        //Event.prototype.stopPropagation = function () { alert('123'); }
        //$('input').on('click', function (e) {
        //    e.stopPropagation();
        //})

    </script>

</asp:Content>