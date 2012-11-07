using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SMLibrary
{
    public class VMManager
    {
        private String _subscriptionid { get; set; }
        private String _certthumbprint { get; set; }
        public XNamespace ns = "http://schemas.microsoft.com/windowsazure";
        XNamespace ns1 = "http://www.w3.org/2001/XMLSchema-instance";
        
        public VMManager(String SubscriptionID, String CertThumbPrint)
        {
            _subscriptionid = SubscriptionID;
            _certthumbprint = CertThumbPrint;
        }

        public HttpClient GetHttpClient()
        {
            WebRequestHandler handler = new WebRequestHandler();
            String CertThumbprint = _certthumbprint;
            X509Certificate2 managementCert = FindX509Certificate(CertThumbprint);
            if (managementCert != null)
            {
                handler.ClientCertificates.Add(managementCert);
                HttpClient httpClient = new HttpClient(handler);
                httpClient.DefaultRequestHeaders.Add("x-ms-version", "2012-03-01");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                return httpClient;
            }
            return null;
        }
        
        async public Task<XDocument> GetAzureSubscription()
        {
            String uri = String.Format("https://management.core.windows.net/{0}", _subscriptionid);
            XDocument subXML = new XDocument();
            HttpClient http = GetHttpClient();
            Stream responseStream = await http.GetStreamAsync(uri);
            if (responseStream != null)
            {
                subXML = XDocument.Load(responseStream);
            }
            return subXML;
        }

        async public Task<List<String>> GetAzureVMImages()
        {
            List<String> imageList = new List<String>();
            String uri = String.Format("https://management.core.windows.net/{0}/services/images", _subscriptionid);

            HttpClient http = GetHttpClient();
            Stream responseStream = await http.GetStreamAsync(uri);

            if (responseStream != null)
            {
                XDocument xml = XDocument.Load(responseStream);
                var images = xml.Root.Descendants(ns + "OSImage").Where(i => i.Element(ns + "OS").Value == "Windows");
                foreach (var image in images)
                {
                    string img = image.Element(ns + "Name").Value;
                    imageList.Add(img);
                }
            }


            return imageList;
        }

        async public Task<String> RebootVM(String ServiceName, String RoleName)
        {
            String requestID = String.Empty;

            String deployment = await GetAzureDeploymentName(ServiceName);
            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}/roleInstances/{3}/Operations", _subscriptionid, ServiceName, deployment, RoleName);

            HttpClient http = GetHttpClient();

            XElement srcTree = new XElement("RestartRoleOperation",
                        new XAttribute(XNamespace.Xmlns + "i", ns1),
                        new XElement("OperationType", "RestartRoleOperation")
                    );
            ApplyNamespace(srcTree, ns);

            XDocument CSXML = new XDocument(srcTree);
            HttpContent content = new StringContent(CSXML.ToString());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

            HttpResponseMessage responseMsg = await http.PostAsync(uri, content);
            if (responseMsg != null)
            {
                requestID = responseMsg.Headers.GetValues("x-ms-request-id").FirstOrDefault();
            }
            return requestID;
        }

        async public Task<String> ShutDownVM(String ServiceName, String RoleName)
        {
            String requestID = String.Empty;

            String deployment = await GetAzureDeploymentName(ServiceName);
            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}/roleInstances/{3}/Operations", _subscriptionid, ServiceName, deployment, RoleName);

            HttpClient http = GetHttpClient();

            XElement srcTree = new XElement("ShutdownRoleOperation",
                        new XAttribute(XNamespace.Xmlns + "i", ns1),
                        new XElement("OperationType", "ShutdownRoleOperation")
                    );
            ApplyNamespace(srcTree, ns);

            XDocument CSXML = new XDocument(srcTree);
            HttpContent content = new StringContent(CSXML.ToString());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

            HttpResponseMessage responseMsg = await http.PostAsync(uri, content);
            if (responseMsg != null)
            {
                requestID = responseMsg.Headers.GetValues("x-ms-request-id").FirstOrDefault();
            }

            return requestID;
        }

        async public Task<XDocument> GetAzureVMs()
        {
            List<XDocument> services = await GetAzureServices();
            XDocument vms = new XDocument();
            vms.Add(new XElement("VirtualMachines"));
            ApplyNamespace(vms.Root, ns);
            foreach (var svc in services)
            {
                string ServiceName = svc.Root.Element(ns + "ServiceName").Value;
                
                String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}", _subscriptionid, ServiceName, "Production");

                try
                {
                    HttpClient http = GetHttpClient();
                    Stream responseStream = await http.GetStreamAsync(uri);

                    if (responseStream != null)
                    {
                        XDocument xml = XDocument.Load(responseStream);
                        var roles = xml.Root.Descendants(ns + "RoleInstance");
                        foreach (XElement r in roles)
                        {
                            XElement svcnameel = new XElement("ServiceName", ServiceName);
                            ApplyNamespace(svcnameel, ns);
                            r.Add(svcnameel); // not part of the roleinstance
                            vms.Root.Add(r);
                        }
                    }
                }
                catch (HttpRequestException http)
                {
                    // no vms with cloud service
                }
            }
            return vms;
        }

        async public Task<List<XDocument>> GetAzureVMs(String ServiceName)
        {
            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}", _subscriptionid, ServiceName, "Production");
            List<XDocument> vms = new List<XDocument>();
            
            HttpClient http = GetHttpClient();
            Stream responseStream = await http.GetStreamAsync(uri);

            if (responseStream != null)
            {
                XDocument xml = XDocument.Load(responseStream);
                var roles = xml.Root.Descendants(ns + "RoleInstance");
                foreach (XElement r in roles)
                {
                    XDocument vm = new XDocument(r);
                    vms.Add(vm);
                }
            }

            return vms;
        }

        async public Task<XDocument> GetAzureVM(String ServiceName, String VMName)
        {
            String deployment = await GetAzureDeploymentName(ServiceName);
            XDocument vmXML = new XDocument();

            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}/roles/{3}", _subscriptionid, ServiceName, deployment, VMName);

            HttpClient http = GetHttpClient();
            Stream responseStream = await http.GetStreamAsync(uri);
            if (responseStream != null)
            {
                vmXML = XDocument.Load(responseStream);
            }

            return vmXML;
        }

        async public Task<String> NewAzureCloudService(String ServiceName, String Location, String AffinityGroup)
        {
            String requestID = String.Empty;

            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices", _subscriptionid);
            HttpClient http = GetHttpClient();

            System.Text.ASCIIEncoding ae = new System.Text.ASCIIEncoding();
            byte[] svcNameBytes = ae.GetBytes(ServiceName);

            String locationEl = String.Empty;
            String locationVal = String.Empty;

            if (String.IsNullOrEmpty(Location) == false)
            {
                locationEl = "Location";
                locationVal = Location;
            }
            else
            {
                locationEl = "AffinityGroup";
                locationVal = AffinityGroup;
            }

            XElement srcTree = new XElement("CreateHostedService",
                        new XAttribute(XNamespace.Xmlns + "i", ns1),
                        new XElement("ServiceName", ServiceName),
                        new XElement("Label", Convert.ToBase64String(svcNameBytes)),
                        new XElement(locationEl, locationVal)
                    );
            ApplyNamespace(srcTree, ns);

            XDocument CSXML = new XDocument(srcTree);
            HttpContent content = new StringContent(CSXML.ToString());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

            HttpResponseMessage responseMsg = await http.PostAsync(uri, content);
            if (responseMsg != null)
            {
                requestID = responseMsg.Headers.GetValues("x-ms-request-id").FirstOrDefault();
            }
            return requestID;
        }

        async public Task<String> NewAzureVM(String ServiceName, String VMName, XDocument VMXML)
        {
            String requestID = String.Empty;

            String deployment = await GetAzureDeploymentName(ServiceName);

            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}/roles", _subscriptionid, ServiceName, deployment);

            HttpClient http = GetHttpClient();
            HttpContent content = new StringContent(VMXML.ToString());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            HttpResponseMessage responseMsg = await http.PostAsync(uri, content);
            if (responseMsg != null)
            {
                requestID = responseMsg.Headers.GetValues("x-ms-request-id").FirstOrDefault();
            }
            return requestID;
        }

        async public Task<String> NewAzureVMDeployment(String ServiceName, String VMName, String VNETName, XDocument VMXML, XDocument DNSXML)
        {
            String requestID = String.Empty;


            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments", _subscriptionid, ServiceName);
            HttpClient http = GetHttpClient();
            XElement srcTree = new XElement("Deployment",
                        new XAttribute(XNamespace.Xmlns + "i", ns1),
                        new XElement("Name", ServiceName),
                        new XElement("DeploymentSlot", "Production"),
                        new XElement("Label", ServiceName),
                        new XElement("RoleList", null)
                    );

            if (String.IsNullOrEmpty(VNETName) == false)
            {
                srcTree.Add(new XElement("VirtualNetworkName", VNETName));
            }
            
            if(DNSXML != null)
            {
                srcTree.Add(new XElement("DNS", new XElement("DNSServers", DNSXML)));
            }

            XDocument deploymentXML = new XDocument(srcTree);
            ApplyNamespace(srcTree, ns);

            deploymentXML.Descendants(ns + "RoleList").FirstOrDefault().Add(VMXML.Root);
                       

            String fixedXML = deploymentXML.ToString().Replace(" xmlns=\"\"", "");
            HttpContent content = new StringContent(fixedXML);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

            HttpResponseMessage responseMsg = await http.PostAsync(uri, content);
            if (responseMsg != null)
            {
                requestID = responseMsg.Headers.GetValues("x-ms-request-id").FirstOrDefault();
            }

            return requestID;
        }

        async public Task<String> UpdateAzureVM(String ServiceName, String VMName, XDocument VMXML)
        {
            String requestID = String.Empty;

            String deployment = await GetAzureDeploymentName(ServiceName);

            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}/roles/{3}", _subscriptionid, ServiceName, deployment, VMName);

            HttpClient http = GetHttpClient();
            String fixedXML = VMXML.ToString().Replace(" xmlns=\"\"", "");
            HttpContent content = new StringContent(fixedXML);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
                
            HttpResponseMessage responseMsg = await http.PutAsync(uri, content);
            if (responseMsg != null)
            {
                requestID = responseMsg.Headers.GetValues("x-ms-request-id").FirstOrDefault();
            }

            return requestID;
        }

        async public Task<byte[]> GetRDPFile(String ServiceName, String vmName)
        {
            String deploymentName = await GetAzureDeploymentName(ServiceName);
            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}/roleinstances/{3}/ModelFile?FileType=RDP", _subscriptionid, ServiceName, deploymentName, vmName);
            byte[] RDPFile = null;

            HttpClient http = GetHttpClient();
            Stream responseStream = await http.GetStreamAsync(uri);
            if (responseStream != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    responseStream.CopyTo(ms);
                    RDPFile = ms.ToArray();
                }
            }
            return RDPFile;
        }

        async private Task<List<XDocument>> GetAzureServices()
        {
            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices ", _subscriptionid);
            List<XDocument> services = new List<XDocument>();

            HttpClient http = GetHttpClient();

            Stream responseStream = await http.GetStreamAsync(uri);

            if (responseStream != null)
            {
                XDocument xml = XDocument.Load(responseStream);
                var svcs = xml.Root.Descendants(ns + "HostedService");
                foreach (XElement r in svcs)
                {
                    XDocument vm = new XDocument(r);
                    services.Add(vm);
                }
           }

            return services;
        }

        async public Task<bool> IsServiceNameAvailable(String ServiceName)
        {
            bool available = false;
            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/operations/isavailable/{1}", _subscriptionid, ServiceName);
            HttpClient http = GetHttpClient();
            Stream responseStream = await http.GetStreamAsync(uri);
            if (responseStream != null)
            {
                XDocument xml = XDocument.Load(responseStream);
                var response = xml.Descendants(ns + "AvailabilityResponse").FirstOrDefault();
                var strAvailable = response.Element(ns + "Result").Value;
                if (strAvailable == "true")
                    available = true;
            }
            return available;
        }

        async private Task<String> GetAzureDeploymentName(String ServiceName)
        {
            String uri = String.Format("https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}", _subscriptionid, ServiceName, "Production");
            String DeploymentName = String.Empty;

            HttpClient http = GetHttpClient();

            Stream responseStream = await http.GetStreamAsync(uri);

            if (responseStream != null)
            {
                XDocument xml = XDocument.Load(responseStream);
                var name = xml.Root.Element(ns + "Name");
                DeploymentName = name.Value;
            }

            return DeploymentName;
        }

        public XDocument NewAzureVMConfig(String RoleName, String VMSize, String ImageName, String MediaLocation, bool InitialDeployment = false)
        {
            System.Text.ASCIIEncoding ae = new System.Text.ASCIIEncoding();
            byte[] roleNameBytes = ae.GetBytes(RoleName);
            XElement srcTree = null;

            if (InitialDeployment == false)
            {
                srcTree = new XElement("PersistentVMRole",
                                        new XAttribute(XNamespace.Xmlns + "i", ns1),
                                        new XElement("RoleName", RoleName),
                                        new XElement("OsVersion", new XAttribute(ns1 + "nil", true)),
                                        new XElement("RoleType", "PersistentVMRole"),
                                        new XElement("ConfigurationSets", null),
                                        new XElement("DataVirtualHardDisks", null),
                                        new XElement("Label", Convert.ToBase64String(roleNameBytes)),
                                        new XElement("OSVirtualHardDisk",
                                            new XElement("MediaLink", MediaLocation),
                                            new XElement("SourceImageName", ImageName)),
                                        new XElement("RoleSize", VMSize)
                                    );

                ApplyNamespace(srcTree, ns);
            }
            else
            {
                srcTree = new XElement("Role",
                        new XAttribute(ns1 + "type", "PersistentVMRole"),
                        new XElement("RoleName", RoleName),
                        new XElement("OsVersion", new XAttribute(ns1 + "nil", true)),
                        new XElement("RoleType", "PersistentVMRole"),
                        new XElement("ConfigurationSets", null),
                        new XElement("DataVirtualHardDisks", null),
                        new XElement("Label", Convert.ToBase64String(roleNameBytes)),
                        new XElement("OSVirtualHardDisk",
                            new XElement("MediaLink", MediaLocation),
                            new XElement("SourceImageName", ImageName)),
                        new XElement("RoleSize", VMSize)
                    );

                ApplyNamespace(srcTree, ns);
            }

            XDocument VMXML = new XDocument();
            VMXML.Add(srcTree);
            return VMXML;
        }

        public XDocument NewWindowsProvisioningConfig(XDocument VMXML, String ComputerName, String Password, bool EnableAutomaticUpdate = false, bool ResetPasswordOnFirstLogon = false)
        {

            XElement srcTree = new XElement("ConfigurationSet",
                        new XAttribute(ns1 + "type", "WindowsProvisioningConfigurationSet"),
                        new XElement("ConfigurationSetType", "WindowsProvisioningConfiguration"),
                        new XElement("ComputerName", ComputerName),
                        new XElement("AdminPassword", Password),
                        new XElement("EnableAutomaticUpdates", EnableAutomaticUpdate),
                        new XElement("ResetPasswordOnFirstLogon", ResetPasswordOnFirstLogon)
                    );


            VMXML.Root.Element(ns + "ConfigurationSets").Add(srcTree);
            return VMXML;
        }

        public XDocument NewNetworkConfigurationSet(XDocument VMXML)
        {
            XElement srcTree = new XElement("ConfigurationSet",
                new XAttribute(ns1 + "type", "NetworkConfigurationSet"),
                new XElement("ConfigurationSetType", "NetworkConfiguration"),
                new XElement("InputEndpoints", null),
                new XElement("SubnetNames", null)
            );


            VMXML.Root.Element(ns + "ConfigurationSets").Add(srcTree);
            return VMXML;
        }

        public XDocument AddNewDataDisk(XDocument VMXML, int DiskSizeInGB, int Lun, String DiskLabel, String MediaLocation)
        {
            XElement srcTree = new XElement("DataVirtualHardDisk",
                                    new XElement("DiskLabel", DiskLabel),
                                    new XElement("Lun", Lun),
                                    new XElement("LogicalDiskSizeInGB", DiskSizeInGB),
                                    new XElement("MediaLink", MediaLocation)
                                );
            VMXML.Root.Element(ns + "DataVirtualHardDisks").Add(srcTree);
            return VMXML;
        }

        public XDocument AddNewInputEndpoint(XDocument VMXML, String Name, String Protocol, int LocalPort, int PublicPort)
        {
            
            XElement srcTree = new XElement("InputEndpoint",
                                    new XElement("LocalPort", LocalPort),
                                    new XElement("Name", Name),
                                    new XElement("Port", PublicPort),
                                    new XElement("Protocol", Protocol)
                                );

           VMXML.Descendants("InputEndpoints").FirstOrDefault().Add(srcTree);
           return VMXML;
        }

        public XDocument AddNewSubnet(XDocument VMXML, String Name)
        {
            XElement srcTree = new XElement("SubnetName", Name);
            VMXML.Descendants("SubnetNames").FirstOrDefault().Add(srcTree);
            return VMXML;
        }



        async public Task<XElement> GetOperationStatus(string requestId)
        {
            string uriFormat = "https://management.core.windows.net/{0}/operations/{1}";

            Uri uri = new Uri(String.Format(uriFormat, _subscriptionid, requestId));
            
            HttpClient http = GetHttpClient();
            
            Stream responseStream = await http.GetStreamAsync(uri);
            if (responseStream != null)
            {
                StreamReader response = new StreamReader(responseStream);

                if (responseStream != null)
                {
                    XDocument xml = XDocument.Load(responseStream);
                    return xml.Element(ns + "Operation");
                }
            }
            return null;
        }

        async public Task<OperationResult> PollGetOperationStatus(string requestId, int pollIntervalSeconds, int timeoutSeconds)
        {
            OperationResult result = new OperationResult();
            DateTime beginPollTime = DateTime.UtcNow;
            TimeSpan pollInterval = new TimeSpan(0, 0, pollIntervalSeconds);
            DateTime endPollTime = beginPollTime + new TimeSpan(0, 0, timeoutSeconds);
            bool done = false;
            while (!done)
            {
                XElement operation = await GetOperationStatus(requestId);
                result.RunningTime = DateTime.UtcNow - beginPollTime;
                try
                {
                    // Turn the Status string into an OperationStatus value
                    result.Status = (OperationStatus)Enum.Parse(
                        typeof(OperationStatus),
                        operation.Element(ns + "Status").Value);
                }
                catch (Exception)
                {
                    throw new ApplicationException(string.Format(
                        "Get Operation Status {0} returned unexpected status: {1}{2}",
                        requestId,
                        Environment.NewLine,
                        operation.ToString(SaveOptions.OmitDuplicateNamespaces)));
                }

                switch (result.Status)
                {
                    case OperationStatus.InProgress:
                        Thread.Sleep((int)pollInterval.TotalMilliseconds);
                        break;

                    case OperationStatus.Failed:
                        result.StatusCode = (HttpStatusCode)Convert.ToInt32(
                            operation.Element(ns + "HttpStatusCode").Value);
                        XElement error = operation.Element(ns + "Error");
                        result.Code = error.Element(ns + "Code").Value;
                        result.Message = error.Element(ns + "Message").Value;
                        done = true;
                        break;

                    case OperationStatus.Succeeded:
                        result.StatusCode = (HttpStatusCode)Convert.ToInt32(
                            operation.Element(ns + "HttpStatusCode").Value);
                        done = true;
                        break;
                }

                if (!done && DateTime.UtcNow > endPollTime)
                {
                    result.Status = OperationStatus.TimedOut;
                    done = true;
                }
            }

            return result;
        }

        private static X509Certificate2 FindX509Certificate(string thumbprint)
        {
            X509Store certificateStore = null;
            X509Certificate2 certificate = null;

            try
            {
                certificateStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);

                certificateStore.Open(OpenFlags.ReadOnly);

                var certificates = certificateStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (certificates.Count > 0)
                {
                    certificate = certificates[0];
                }
            }
            finally
            {
                if (certificateStore != null) certificateStore.Close();
            }

            return certificate;
        }
        

        private static void ApplyNamespace(XElement parent, XNamespace nameSpace)
        {
            parent.Name = nameSpace + parent.Name.LocalName;
            foreach (XElement child in parent.Elements())
            {
                ApplyNamespace(child, nameSpace);
            }
        }

    }
}
