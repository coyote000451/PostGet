using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Xml;
using System.Collections.Specialized;
using System.Web;
using System.Xml.Linq;
//using VMware.Vim;

namespace TempPostGet
{
    class Program
    {
        
        static void Main(string[] args)
            
        {
            
            var request1 = (HttpWebRequest)WebRequest.Create("http://skyboxlabs.cerner.com/api/versions");
            var response1 = (HttpWebResponse)request1.GetResponse();
            var responseString1 = new StreamReader(response1.GetResponseStream()).ReadToEnd();

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(responseString1);  //Load the XML output

            XmlNodeList loginUrl = xmlDoc.GetElementsByTagName("LoginUrl");
            Console.WriteLine("Login Url:" + loginUrl[0].InnerText);  //Get the inner text between </LoginUrl>
            ///Console.ReadLine();
            
                        //var request = (HttpWebRequest)WebRequest.Create("https://skyboxlabs.cerner.com/api/sessions"); //<LoginURL>
                        var request = (HttpWebRequest)WebRequest.Create(loginUrl[0].InnerText); //<LoginURL>
                        request.Accept = "application/*+xml;version=5.5";
                        
                        //var postData = "<CernerUserId>@labs:<password>";
                        var postData = "Administrator@labs:CloudLabs!";  //Define up above
                        // System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(postData));
                        var data = request.Headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(postData));
                        //var data = request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(postData));
/*
                        int arraySize = data.Length;

                        for (int i = 0; i < arraySize; i++ )
                        {
                            Console.Write("{0}", data[i]);
                        }

                        Console.ReadLine();
 */
                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = data.Length;                        
            
                        using (var stream = request.GetRequestStream())//system.net.connectstream
                        {
                            var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                            stream.Write(bytes, 0, data.Length);                      
                        }
            
                        var response = (HttpWebResponse)request.GetResponse();           
                        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // Here is where the x-vcloud-authorization comes back

                        Console.WriteLine(" RESPONSE STRING: {0}", responseString);
                        ///Console.ReadLine();
                        
            string AuthToken = string.Empty;
            string SetCookie = string.Empty;

                        for (int i = 0; i < response.Headers.Count; ++i)
                        {
                            
                            Console.WriteLine("\nHeader Name:{0}, Value :{1}", response.Headers.Keys[i], response.Headers[i]);
                            
                            if (response.Headers.Keys[i] == "x-vcloud-authorization") //Define this above per Jeff
                            {
                                //Console.WriteLine("\n x-vcloud-authorization: {0}", response.Headers[i]);
                                AuthToken = response.Headers.Keys[i] + ": " + response.Headers[i];
                                //break;
                                //AuthToken = response.Headers[i];
                            }

                            if (response.Headers.Keys[i] == "Set-Cookie") //Define this above per Jeff
                            {
                                //Console.WriteLine("\n x-vcloud-authorization: {0}", response.Headers[i]);
                                SetCookie = response.Headers.Keys[i] + ": " + response.Headers[i];
                                //break;
                                //AuthToken = response.Headers[i];
                            }
                             
                        }


                        Console.WriteLine("HEADERS:  {0}", response.Headers);
                        // Releases the resources of the response.
                        response.Close();
                        Console.ReadLine();
                        Console.WriteLine("\n x-vcloud-authorization: {0}", AuthToken);
            //
                        var AuthHeader = response.Headers.ToString();
            //Convert the string XML to true XML
                        XmlDocument xmlResponse = new XmlDocument();
                        xmlResponse.LoadXml(responseString);
                        XmlNodeList elemlist = xmlResponse.GetElementsByTagName("Link");  //find the <Link> elements
                        String AttrValToken = null;

            //userId
                        for (int i = 0; i < elemlist.Count; i++)
                        {
                            string attrVal = elemlist[i].Attributes["href"].Value;  //look for the href strings in the <Link> elements
                            
                            if (attrVal.Length > 50 )
                            //if (attrVal.Length > 0)
                            {
                                //Console.WriteLine("{0}", attrVal);
                                AttrValToken = attrVal;
                                break;
                                //Console.WriteLine("HREF: {0}", AttrValToken); 
                            }
                            //Console.WriteLine("{0}", AttrValToken); 
                        }
                        Console.WriteLine("ATTRVALTOKEN: {0}", AttrValToken); //Write the HREF AttvValToken and print to console
                        //Console.ReadLine();
            // Obtaining the Session element.  Should contain one or more Link elements with URL that allows further exploration of objects
                        XmlNodeList elemSessionlist = xmlResponse.GetElementsByTagName("Session");


                        String AttrValSessionToken = null;

                        //userId
                        for (int i = 0; i < elemSessionlist.Count; i++)
                        {
                            
                            string attrSessionVal = elemSessionlist[i].Attributes["userId"].Value; //null reference pointer

                            if (attrSessionVal.Length > 50)
                            {
                                //Console.WriteLine("{0}", attrVal);
                                AttrValSessionToken = attrSessionVal;
                                //Console.WriteLine("{0}", AttrValToken); 
                            }
                            Console.WriteLine("ATTRVALTOKEN: {0}", AttrValToken); 
                        }
                        Console.WriteLine("ATTRVALSESSIONTOKEN: {0}", AttrValSessionToken);   // Write the encrypted UserId
                        //Console.ReadLine();
            
            /// Find a Catalog and a VDC
                        //https://skyboxlabs.cerner.com/api/org/
                        var requestOrg = (HttpWebRequest)WebRequest.Create(AttrValToken); //

                        //var requestOrg = (HttpWebRequest)WebRequest.Create("https://skyboxlabs.cerner.com/api/org/5");
                        var orgaccept = requestOrg.Accept = "application/*+xml;version=5.5";
                        //var orgaccept = requestOrg.Accept = "text/html";
                        //var orgaccept = requestOrg.Accept = "application/vnd.vmware.vcloud.orgList+xml;version=5.5";
                        //var orgdata = requestOrg.Headers["Authorization"] = "x-vcloud-authorization: " + AuthToken;
                        var orgdata = requestOrg.Headers["Authorization"] = AuthToken + ";" + SetCookie;
                        //var orgdata = requestOrg.Headers["Authorization"] = AuthToken;
           
                        //requestOrg.Headers["Set-Cookie"] = "vcloud-token=" + AuthToken + "; " + "Secure; Path=/";

                        //Set-Cookie: vcloud-token=hAXRtjaKLripYL/AIN5jroNMTPxE3bTceIKLb6iCnm4=; Secure; Path=/
            /*
                        requestOrg.CookieContainer = new CookieContainer();
                        requestOrg.CookieContainer.Add(new Uri("http://api.search.live.net"),
                        new Cookie("id", "1234"));
            */
                        //Console.WriteLine("CONTENT:  {0}", orgaccept);
                        Console.WriteLine("HEADER:   {0}", orgdata);
                        //Console.ReadLine();

                        //requestOrg.ContentLength = orgdata.Length; 
                        var responseOrg = (HttpWebResponse)requestOrg.GetResponse();     //403 forbidden implies the authentication header is missing page 26    
                        
                        var responseOrgString = new StreamReader(responseOrg.GetResponseStream()).ReadToEnd();
                        
                        Console.WriteLine("RESPONSE ORG: {0}", responseOrgString); // This is blank
                        Console.ReadLine();

        }
    }
}
