using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMLibrary;
using System.Xml.Linq;
using System.Configuration;
namespace MyVMPortal
{
    public partial class _Default : Page
    {


        private VMManager GetVMM()
        {
            return new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                GetQuota();
            }
            catch (Exception exc)
            {
                Response.Write(exc.ToString());
            }
        }

        async protected void GetQuota()
        {
            try
            {
                VMManager vmm = GetVMM();

                XDocument subscriptionXML = await vmm.GetAzureSubscription();
                var quota = (from vm in subscriptionXML.Descendants(vmm.ns + "Subscription")
                             select new
                             {
                                 CurrentCoreCount = (string)vm.Element(vmm.ns + "CurrentCoreCount"),
                                 MaxCoreCount = (string)vm.Element(vmm.ns + "MaxCoreCount"),
                                 CurrentHostedServices = (string)vm.Element(vmm.ns + "CurrentHostedServices"),
                                 MaxHostedServices = (string)vm.Element(vmm.ns + "MaxHostedServices")
                             }).FirstOrDefault();

                lblCoreCountUsed.Text = quota.CurrentCoreCount;
                lblCoreCountAvailable.Text = quota.MaxCoreCount;
                lblHostedServicesUsed.Text = quota.CurrentHostedServices;
                lblHostedServicesAvailable.Text = quota.MaxHostedServices;
            }
            catch (Exception exc)
            {
                String buffer = "";

                buffer = exc.ToString();
                if (exc.InnerException != null)
                {
                    buffer += "<br />";
                    buffer += exc.InnerException.ToString();
                }

                Response.Write(buffer);

            }
        }

        async protected void cmdListVMs_Click(object sender, EventArgs e)
        {

            VMManager vmm = GetVMM();

            XDocument vmsXML = await vmm.GetAzureVMs();

            var vms = from vm in vmsXML.Descendants(vmm.ns + "RoleInstance")
                           select new
                           {
                               RoleName = (string)vm.Element(vmm.ns + "RoleName"),
                               InstanceStatus = (string)vm.Element(vmm.ns + "InstanceStatus"),
                               InstanceSize = (string)vm.Element(vmm.ns + "InstanceSize"),
                               IpAddress = (string)vm.Element(vmm.ns + "IpAddress"),
                               PowerState = (string)vm.Element(vmm.ns + "PowerState"),
                               ServiceName = (string)vm.Element(vmm.ns + "ServiceName")
                           };

            gridVMs.DataSource = vms.ToList();
            gridVMs.DataBind();

            
        }

        async protected void gridVMs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            VMManager vmm = GetVMM();
            int row = Convert.ToInt32(e.CommandArgument);
            String roleName = gridVMs.Rows[row].Cells[3].Text;
            String serviceName = gridVMs.Rows[row].Cells[8].Text;
            if (e.CommandName == "LoginRDP")
            {
                try
                {
                    byte[] rdpFile = await vmm.GetRDPFile(serviceName, roleName);
                    Response.AppendHeader("Content-Disposition", String.Format("filename={0}.rdp", roleName));
                    Response.AppendHeader("Content-Length", rdpFile.Length.ToString());
                    Response.ContentType = "application/octet-stream";
                    Response.BinaryWrite(rdpFile);
                }
                catch (Exception exc)
                {
                    lblLastStatus.Text = "Virtual Machine is Missing RDP Endpoint";
                }
            }
            if (e.CommandName == "Reboot")
            {
                String requestID = await vmm.RebootVM(serviceName, roleName);
                linkCheckStatus.NavigateUrl = "/GetOperationStatus.aspx?requestid=" + requestID;
                linkCheckStatus.Text = "Check Request Status";
            }
            else if (e.CommandName == "Shutdown")
            {
                String requestID = await vmm.ShutDownVM(serviceName, roleName);
                linkCheckStatus.NavigateUrl = "/GetOperationStatus.aspx?requestid=" + requestID;
                linkCheckStatus.Text = "Check Request Status";
            }
        }

        protected void cmdCreateVM_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/CreateVM.aspx", false);
        }
    }
}