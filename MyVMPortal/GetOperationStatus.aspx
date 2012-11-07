<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" Async="true" AutoEventWireup="true" CodeBehind="GetOperationStatus.aspx.cs" Inherits="MyVMPortal.GetOperationStatus" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
            <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1>IaaS Manager</h1>
                
            </hgroup>
            <h2>
                <a href="/">Back to Virtual Machine List</a>
                <br /><br />
                Request Status: <asp:Label ID="lblStatus" runat="server"></asp:Label>
                <br /><br />
                Hit F5 to Update
            </h2>
        </div>
    </section>


</asp:Content>
