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
</asp:Content>