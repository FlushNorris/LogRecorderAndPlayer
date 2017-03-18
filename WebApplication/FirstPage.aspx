<%@ Page Title="" Language="C#" MasterPageFile="~/Page.Master" AutoEventWireup="true" CodeBehind="FirstPage.aspx.cs" Inherits="WebApplication.FirstPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Button runat="server" ID="ServerButton" OnClick="btnButton_OnClick" Text="Do postback and set session value to Datetime.Now" /><br/>
    ServerTextBox: <asp:TextBox runat="server" id="ServerTextBox"></asp:TextBox><br/>
    ClientTextBox: <input id="clientTextbox" value="<%=Session["SomeSessionKey"] ?? "null" %>"/><br/>
    <br/>
    SomeText for GenericHandler:<input id="someText" value=""/><br/>
    <input type="button" value="Call GenericHandler with value 5" onclick="callGenericHandler(5)"/><br/>
    <input type="button" value="Call GenericHandler with value 10" onclick="callGenericHandler(10)"/><br/>
    Handler result:
    <div id="handlerResult"></div>
    Dynamic content:
    <div id="dynamicContent"></div>
    
    <script type="text/javascript">
        function parseJsonDate(jsonDateString) {
            return new Date(parseInt(jsonDateString.replace('/Date(', '')));
        }

        function htmlEncode(value) {
            if (!!value) return $('<div></div>').text(value).html();
            return "";
        }

        function callGenericHandlerByDynamic(idx) {
            var $dynamicValue = $("#dynamicValue" + idx);
            callGenericHandler(+$dynamicValue.val());
        }

        function addStatus(txt) {
            var $handlerResult = $("#handlerResult");
            $handlerResult.html($handlerResult.html() + htmlEncode(txt) + "<br/>");
        }

        function callGenericHandler(value) {
            var request = {
                SomeText: $("#someText").val(),
                SomeValue: value
            };

            //addStatus("Calling handler with value=" + value);

            //if (confirm('someAlert')) {
            //    addStatus("yeees!");
            //} else {
            //    addStatus("nooo!");
            //}
            //addStatus("Skulle først ske efter confirm-valget");

            $.ajax({
                type: "POST",
                url: "handlers/testGenericHandler.ashx",
                data: {
                    request: JSON.stringify(request)
                },
                success: function (obj, textStatus) {
                    addStatus("Got " + obj.SomeRandomInt + " at " + parseJsonDate(obj.SomeTimestamp) + " from handler");

                    var $dynamicContent = $("#dynamicContent");
                    $dynamicContent.html('');

                    for (var i = 0; i < obj.SomeRandomInt; i++) {
                        $dynamicContent.append($("<input>",
                        {
                            type: "button",
                            value: "Dynamic button: Call GenericHandler with following Value " + i * 5,
                            onclick: "callGenericHandlerByDynamic(" + i + ")"
                        }));
                        $dynamicContent.append($("<input>", { id:"dynamicValue"+i, value: i * 5 }));
                        $dynamicContent.append("<br/>");
                    }
                },
                error: function (obj) {
                    addStatus("Failed to call handler");
                }
            });
        }
        (function (proxied) {
            window.alert = function () {
                addStatus("Alert: " + arguments[0]);
                $("<div>", { title: "Alert" }).text(arguments[0]).dialog({ modal: true });
                // do something here
                return false; //proxied.apply(this, arguments);
            };
        })(window.alert);

        function fakeConfirm(callback, txt) {
            $("<div>", { title: "Confirm" }).text(arguments[0]).dialog(
                {
                    modal: true,
                    buttons: {
                        OK: function () {
                            callback(true);
                            $( this ).dialog( "close" );
                        },
                        Cancel: function () {
                            callback(false);
                            $( this ).dialog( "close" );
                        }
                    }
                });
        }

        (function (proxied) {
            window.confirm = async(this, function(T) {
                addStatus('før');
                var result = await(this, fakeConfirm, arguments[0]);
                addStatus('efter '+result);
            });

            //window.confirm = function () {
            //    addStatus("Confirm: " + arguments[0]);

            //    window.confirmChoice = null;
            //    function callBackFunc(confirmChoice) {
            //        window.confirmChoice = confirmChoice;
            //    }

            //    $("<div>", { title: "Confirm" }).text(arguments[0]).dialog({
            //        modal: true,
            //        buttons: {
            //            OK: function () {
            //                callBackFunc(true);
            //                $( this ).dialog( "close" );
            //            },
            //            Cancel: function () {
            //                callBackFunc(false);
            //                $( this ).dialog( "close" );
            //            }
            //        }
            //    });

            //    while (window.confirmChoice == null) {
                    
            //    }

            //    return window.confirmChoice;
            //    // do something here
            //    return false; //proxied.apply(this, arguments);
            //};
        })(window.confirm);
        (function (proxied) {
            window.prompt = function () {
                addStatus("Prompt: " + arguments[0] + " : " + arguments[1]);
                // do something here
                return false; //proxied.apply(this, arguments);
            };
        })(window.prompt);

        //<button onclick="Login.Try()">Login.Try()</button>

        window.onerror = function (message, file, lineNumber) {
            return true;
        }

        var Login = {
            Url: "http://code.jquery.com/jquery-1.9.1.min.js#fake-url",
            Try: async(this, function (T) {

                console.log('before login');

                //var success = call(this, Login.Proceed); // normal call
                var success = await(this, Login.Proceed);  // that we want!

                console.log('after login');
                console.log('success ' + success);

            }),

            Proceed: function (callback) {
                console.log('before ajax');
                $.ajax({
                    url: this.Url,
                    context: document.body
                }).done(function () {
                    console.log('after ajax');
                    callback("role=admin");
                });
            }
        }

        function async(T, method) {
            console.log('before async create');
            return function () { return method.apply(T); };
            console.log('after async create');
        };

        function await(T, method, args) {
            var fn = arguments.callee.caller.toString();
            var pos = fn.indexOf('await(');
            var allBeforeAwait = fn.substring(0, pos);

            var pos1 = fn.indexOf('await(');
            pos1 = fn.indexOf(',', pos1) + 1;
            var pos2 = fn.indexOf(')', pos1);
            var cc = fn.substring(pos1, pos2);


            pos = allBeforeAwait.lastIndexOf(';');
            var allBeforeCall = allBeforeAwait.substring(0, pos + 1) + "}";
            var callResult = allBeforeAwait.substring(pos + 1);

            var result = 10;
            var allAfterCall = "(" + fn.substring(0, fn.indexOf(")")) + ",V){" + callResult + "V;";
            pos = fn.indexOf(')', pos) + 2;
            allAfterCall = allAfterCall + fn.substring(pos) + ")";

            //uncomment to see function's parts after split
            //console.debug(allBeforeCall);
            //console.debug(cc);
            //console.debug(allAfterCall);

            method.apply(T, [function (value) {
                console.log('ajax response ' + value);
                eval(allAfterCall).apply(T, [T, value]);
            }].concat(args));

            throw new Error('This is not an error. This is just to abort javascript');
        };
    </script>
</asp:Content>
