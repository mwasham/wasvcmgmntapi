<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" Async="true" AutoEventWireup="true" CodeBehind="CreateVM.aspx.cs" Inherits="MyVMPortal.CreateVM" %>
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
                Subscscription Quotas
                <br />
                <table style="border: 1px dashed white; color: white; font-weight: bold; font-size: medium">
                    <tr>
                        <td>
                            Cores Used
                        </td>
                        <td>
                            Cores Availabile
                        </td>
                        <td>
                            Cloud Services Used
                        </td>
                        <td>
                            Cloud Services Available
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lblCoreCountUsed" runat="server"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblCoreCountAvailable" runat="server"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblHostedServicesUsed" runat="server"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblHostedServicesAvailable" runat="server"></asp:Label>
                        </td>
                    </tr>
                </table>
                
                <br />
                
            </h2>
        </div>
    </section>
    <table border="1" style="background-color: white; width: 100%">
        <tr>
            <td style="padding-left: 10px">
                <table>
                    <tr>
                        <td>
                            Service Name 
                        </td>
                        <td>
                            <asp:TextBox ID="txtServiceName" runat="server" Text=""/>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            VM Name
                        </td>
                        <td>
                            <asp:TextBox ID="txtVMName" runat="server" Text="" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Admin Password
                        </td>
                        <td>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" Text="" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            VM Size
                        </td>
                        <td>
                            <asp:DropDownList runat="server" id="ddlVMSize">
                                <asp:ListItem>Small</asp:ListItem>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Storage Account
                        </td>
                        <td>
                            <asp:Label ID="lblStorageAccount" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Source Image
                        </td>
                        <td>
                            <asp:DropDownList runat="server" id="ddlImages">
                
                            </asp:DropDownList>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblStatus" Font-Bold="true" ForeColor="Red" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td align="left" valign="top">
                <div id="divXML" style="background-color: white; border: 1px dashed gray" runat="server" visible="false">
                    <pre>
                        <asp:Label ID="lblVMXML" runat="server"></asp:Label>
                    </pre>
                </div>
            </td>
        </tr>
    </table>

    <div style="font-size: medium; width: 100%; white-space: nowrap; background-color: white">
        <asp:Button ID="cmdCreateVMConfig" runat="server" Text="Create VM Configuration" OnClick="cmdCreateVMConfig_Click" />

        <table border="1" id="tblConfig" runat="server" visible="false">
            <tr>
                 <td style="width: 5px"><asp:CheckBox ID="chkAddDataDisk" runat="server" TextAlign="Left"  OnCheckedChanged="chkAddDataDisk_Click" AutoPostBack="true" /></td>
                 <td>Add Data Disk</td>
            </tr>
            <tr>
                <td><asp:CheckBox ID="chkAddRDPEndpoint" runat="server" TextAlign="Left" OnCheckedChanged="chkAddRDPEndpoint_Click" AutoPostBack="true"/></td>
                <td>Add RDP Endpoint</td>
            </tr>
            <tr>
                <td><asp:CheckBox ID="chkAddHTTPEndpoint" runat="server" TextAlign="Left" OnCheckedChanged="chkAddHTTPEndpoint_Click" AutoPostBack="true"/></td>
                <td>Add HTTP Endpoint</td>
            </tr>
        </table>
        <br />
        <asp:Button ID="cmdCreateVM" runat="server" Text="Create VM" OnClick="cmdCreateVM_Click" Visible="false" />
        <br />
        <asp:HyperLink ID="linkCheckStatus" runat="server" Target="_blank"></asp:HyperLink>
    </div>
</asp:Content>
