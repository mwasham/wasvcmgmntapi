using SMLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;

namespace MyVMPortal
{
    public partial class CreateVM : System.Web.UI.Page
    {
        private String _StorageAccount = String.Empty;

        private String GetStorageAccount()
        {
            string[] accounts = ConfigurationManager.AppSettings["StorageAccount"].Split(new char[] { ',' });
            Random r = new Random(DateTime.Now.Millisecond);
            return accounts[r.Next(0, accounts.Length)];
        }

        async protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                VMManager vmm = GetVMM();
                List<String> images = await vmm.GetAzureVMImages();
                ddlImages.DataSource = images;
                ddlImages.DataBind();

                lblStorageAccount.Text = GetStorageAccount();
                _StorageAccount = lblStorageAccount.Text;
                ViewState["StorageAccount"] = _StorageAccount; 
            }
            else
            {
                txtPassword.Attributes["value"] = txtPassword.Text;
                _StorageAccount = ViewState["StorageAccount"].ToString();
            }

            GetQuota();
        }

        private VMManager GetVMM()
        {
            return new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
        }

        private String GetOSDiskMediaLocation()
        {
            String osdiskmedialocation = String.Format("https://{0}.blob.core.windows.net/vhds/{1}-OS-{2}.vhd", _StorageAccount, txtVMName.Text, Guid.NewGuid().ToString());
            return osdiskmedialocation;
        }
        private String GetDataDiskMediaLocation()
        {
            String osdiskmedialocation = String.Format("https://{0}.blob.core.windows.net/vhds/{1}-Data-{2}.vhd", _StorageAccount, txtVMName.Text, Guid.NewGuid().ToString());
            return osdiskmedialocation;
        }

        protected void cmdCreateVMConfig_Click(object sender, EventArgs e)
        {
            GenerateVMConfig(false);
        }
        
        async public void GenerateVMConfig(bool Create)
        {
            if (String.IsNullOrEmpty(txtServiceName.Text))
            {
                lblStatus.Text = "Missing Service Name";
                return;
            }
            if (String.IsNullOrEmpty(txtVMName.Text))
            {
                lblStatus.Text = "Missing VM Name";
                return;
            }
            if (String.IsNullOrEmpty(txtPassword.Text))
            {
                lblStatus.Text = "Missing Pasword";
                return;
            }
            if (ValidationHelpers.IsWindowsComputerNameValid(txtVMName.Text) == false)
            {
                lblStatus.Text = "VM Name is either too long or has invalid characters";
                return;
            }
            if (ValidationHelpers.IsWindowsPasswordValid(txtPassword.Text) == false)
            {
                lblStatus.Text = "Windows Password does not meet complexity requirements.";
                return;
            }

            lblStatus.Text = "";

            VMManager vmm = GetVMM();

            if(await vmm.IsServiceNameAvailable(txtServiceName.Text) == false)
            {
                lblStatus.Text = "Service Name is not available. Must be unique";
                return;
            }

            XDocument vm = vmm.NewAzureVMConfig(txtVMName.Text, ddlVMSize.Text, ddlImages.Text, GetOSDiskMediaLocation(), true);

            vm = vmm.NewWindowsProvisioningConfig(vm, txtVMName.Text, txtPassword.Text);
            vm = vmm.NewNetworkConfigurationSet(vm);

            if (chkAddDataDisk.Checked == true)
            {
                vm = vmm.AddNewDataDisk(vm, 10, 0, "MyDataDisk", GetDataDiskMediaLocation());
            }
            if (chkAddHTTPEndpoint.Checked == true)
            {
                vm = vmm.AddNewInputEndpoint(vm, "web", "TCP", 80, 80);
            }
            if (chkAddRDPEndpoint.Checked == true)
            {
                vm = vmm.AddNewInputEndpoint(vm, "rdp", "TCP", 3389, 3389);
            }

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings()
            {
                Indent = true
            };
            using (var writer = XmlWriter.Create(builder, settings))
            {
                vm.WriteTo(writer);
            }


            divXML.Visible = true;
            lblVMXML.Text = Server.HtmlEncode(builder.ToString());

            tblConfig.Visible = true;
            cmdCreateVM.Visible = true;
            cmdCreateVMConfig.Visible = false;

            if (Create == true)
            {
                String requestID_cloudService = await vmm.NewAzureCloudService(txtServiceName.Text, "West US", String.Empty);

                OperationResult result = await vmm.PollGetOperationStatus(requestID_cloudService, 5, 120);

                if (result.Status == OperationStatus.Succeeded)
                {
                    // VM creation takes too long so we'll check it later
                    String requestID_createDeployment = await vmm.NewAzureVMDeployment(txtServiceName.Text, txtVMName.Text, String.Empty, vm, null);

                    Response.Redirect("/GetOperationStatus.aspx?requestid=" + requestID_createDeployment);
                }
                else
                {
                    lblStatus.Text = String.Format("Creating Cloud Service Failed. Message: {0}", result.Message);
                }
            }
        }

        protected void chkAddDataDisk_Click(object sender, EventArgs e)
        {
            GenerateVMConfig(false);
        }

        protected void chkAddRDPEndpoint_Click(object sender, EventArgs e)
        {
            GenerateVMConfig(false);
        }

        protected void chkAddHTTPEndpoint_Click(object sender, EventArgs e)
        {
            GenerateVMConfig(false);
        }

        protected void cmdCreateVM_Click(object sender, EventArgs e)
        {
            GenerateVMConfig(true);
        }

        async protected void GetQuota()
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
    }
}