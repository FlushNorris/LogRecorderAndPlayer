<%@ Page Title="" Language="C#" MasterPageFile="~/Page.Master" AutoEventWireup="true" CodeBehind="PageWithASCX.aspx.cs" Inherits="LogThisWebApplication.PageWithASCX" %>

<%@ Register Src="~/TestUserControl.ascx" TagPrefix="uc1" TagName="TestUserControl" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <uc1:TestUserControl runat="server" id="TestUserControl" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
</asp:Content>
