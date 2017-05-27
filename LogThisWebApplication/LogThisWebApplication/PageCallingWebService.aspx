<%@ Page Title="" Language="C#" MasterPageFile="~/Page.Master" AutoEventWireup="true" CodeBehind="PageCallingWebService.aspx.cs" Inherits="LogThisWebApplication.PageCallingWebService" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script src="JavaScript.js"></script>
    <asp:Button runat="server" ID="btnCallWebService" OnClick="btnCallWebService_OnClick" Text="Call WebService"/><br/>
    Input: <asp:TextBox runat="server" ID="txtSomeInput" Width="400" Text="Et eller andet input"></asp:TextBox><br/>
    Result: <asp:TextBox runat="server" ID="txtSomeResult" Width="1000"></asp:TextBox><br/>
   
    <script>
        //$(window).hashchange(function() {
        //    alert('1234');
        //});
        //$(window).on('hashchange', function() {
        //    alert('hello');
        //});
    </script> 
</asp:Content>


