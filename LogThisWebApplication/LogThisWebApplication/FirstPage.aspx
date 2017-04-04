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
	    <input class="qwerty" onclick="testMethod(this)" onfocus="alert(1)"/>
	    <div>
	    <input class="qwerty bah" onclick="testMethod(this)"/>
	    </div>
	    <div>
	    <input class="qwerty" onclick="testMethod(this)"/>
	    <input class="qwerty" onclick="testMethod(this)"/>
	    </div>
    </div>
    
    <script type="text/javascript">
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

        function getJQueryEvents($elm, $curr, eventType /*e.g. 'focus'*/) {
            if ($curr.size() == 0) //e.g. $(document).parent()
                return [];

            //AND: a=$('[myc="blue"][myid="1"][myid="3"]');
            //OR: a=$('[myc="blue"],[myid="1"],[myid="3"]');

            var result = [];

            var events = ($._data || $.data)($curr[0], 'events');
            if (events) {
                var typeEvents = events[eventType];
                if (typeEvents) {
                    result = $.map(typeEvents, function (v) {
                        return v.handler;
                    });
                }
                if (eventType == 'focus') {
                    var typeEvents = events[eventType + "in"];
                    if (typeEvents) {

                        //vil gerne for hvert event teste om den typeEvent[n].selector matcher den originale $elm

                        result.push.apply(result, $.map(typeEvents, function (v) {
                            return v.handler;
                        }));                        
                    }
                    //Burde overskrive event.stopPropagation() og event.stopImmediatePropagation()
                }
            }
            //alert($elm.prop("nodeName"));

            result.push.apply(result, getJQueryEvents($elm, $curr.parent(), eventType));

            return result;
        }

        function testMethod(that) {
            var s = logRecorderAndPlayer.getElementPath(that);
            var $o = logRecorderAndPlayer.getJQueryElementByElementPath(s);

            var b1 = validateSelectorAgainstElement(".qwerty", $o);
            var b2 = validateSelectorAgainstElement(".qwerty2", $o);
            var b3 = validateSelectorAgainstElement(".qwerty,.woohoo", $o);
            var b4 = validateSelectorAgainstElement(".qwerty .woohoo", $o);
            //var $events = getJQueryEvents($o, $o, 'focus');

            return;
            logRecorderAndPlayer.playEventFor(logRecorderAndPlayer.LogType.OnFocus, s);

            var events = $._data($o[0], 'events');
            var docEvents = $._data(document, 'events');

            //alert(o.value);
        }

        $("#ost").on('focus', '.qwerty', function () {
            //alert('event hosted by #ost');
        });

        $(".qwerty").on("focus", function (e) {
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

        Event.prototype.stopImmediatePropagation = function () { alert('abc'); }
        Event.prototype.stopPropagation = function () { alert('123'); }
        //$('input').on('click', function (e) {
        //    e.stopPropagation();
        //})

    </script>

</asp:Content>