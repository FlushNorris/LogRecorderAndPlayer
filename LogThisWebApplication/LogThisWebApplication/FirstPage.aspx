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
	    <input class="qwerty" onclick="testMethod(this)"/>
	    <div>
	    <input class="qwerty" onclick="testMethod(this)"/>
	    </div>
	    <div>
	    <input class="qwerty" onclick="testMethod(this)"/>
	    <input class="qwerty" onclick="testMethod(this)"/>
	    </div>
    </div>
    
    <script type="text/javascript">
        function testMethod(that) {
            var s = logRecorderAndPlayer.getElementPath(that);
            var o = logRecorderAndPlayer.getElementByElementPath(s);
            alert(o.value);
        }
    </script>

</asp:Content>