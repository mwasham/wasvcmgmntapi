using SMLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace MyVMPortal
{
    public partial class GetOperationStatus : System.Web.UI.Page
    {
        async protected void Page_Load(object sender, EventArgs e)
        {

            if (Request.QueryString["requestid"] != null)
            {
                VMManager vmm = GetVMM();
                String requestID = Request.QueryString["requestid"];
                XElement status = await vmm.GetOperationStatus(requestID);
                String strStatus = status.Element(vmm.ns + "Status").Value;
                String strMessage = String.Empty;
                if (status.Descendants(vmm.ns + "Message").FirstOrDefault() != null)
                    strMessage = status.Descendants(vmm.ns + "Message").FirstOrDefault().Value;

                String osStatus = String.Format("Status: {0}, Message: {1}",strStatus, strMessage );
                lblStatus.Text = osStatus;
            }
        }


        private VMManager GetVMM()
        {
            return new VMManager(ConfigurationManager.AppSettings["SubcriptionID"], ConfigurationManager.AppSettings["CertificateThumbprint"]);
        }

    }
}