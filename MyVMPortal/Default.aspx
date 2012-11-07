<%@ Page Title="Home Page" Language="C#" Async="true" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MyVMPortal._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1>IaaS Portal</h1>
                
            </hgroup>
            <h2>
                Subscscription Quotas
                <br />
                <table style="border: 1px dashed white; color: white; font-weight: bold;font-size: medium">
                    <tr>
                        <td>
                            Cores Used
                        </td>
                        <td>
                            Cores Available
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
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
        
        <asp:Button ID="cmdListVMs" runat="server" Text="List Virtual Machines" OnClick="cmdListVMs_Click" />
        <asp:Button ID="cmdCreateVM" runat="server" Text="Create a Virtual Machine" OnClick="cmdCreateVM_Click" />
        <br />
        <asp:HyperLink ID="linkCheckStatus" runat="server" Target="_blank"></asp:HyperLink>
        <br />
        <asp:Label ID="lblLastStatus" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>
        <br />
        <asp:GridView ID="gridVMs" runat="server" CellPadding="4" ForeColor="#333333" GridLines="None" OnRowCommand="gridVMs_RowCommand">
            <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
            <Columns>
                <asp:ButtonField CommandName="LoginRDP" Text="Login (RDP)" />
                <asp:ButtonField CommandName="Reboot" Text="Reboot" />
                <asp:ButtonField CommandName="Shutdown" Text="Shutdown" />
            </Columns>
            <EditRowStyle BackColor="#999999" />
            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#E9E7E2" />
            <SortedAscendingHeaderStyle BackColor="#506C8C" />
            <SortedDescendingCellStyle BackColor="#FFFDF8" />
            <SortedDescendingHeaderStyle BackColor="#6F8DAE" />

        </asp:GridView>

</asp:Content>
