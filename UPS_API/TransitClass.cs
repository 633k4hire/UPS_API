using System;
using System.Net;

using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ShippingAPI.WebReference2;

namespace ShippingAPI
{
    public class program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Address shipFrom = new Address();
                shipFrom.AddressLine = new string[] { "3300 cherry tree lane" };
                shipFrom.Name = "M-Tech";
                shipFrom.AttentionName = "Matthew";
                shipFrom.City = "Prospect";
                shipFrom.State = "KY";
                shipFrom.Postal = "40059";
                shipFrom.Country = "US";
                shipFrom.Phone = "5022973852";

                Address shipTo = new Address();
                shipTo.AddressLine = new string[] { "13457 Inglewood Ave", "Apt A" };
                shipTo.Name = "Eclectic Elegance";
                shipTo.AttentionName = "Jami Williams";
                shipTo.City = "hawthorne";
                shipTo.State = "CA";
                shipTo.Postal = "90250";
                shipTo.Country = "US";
                shipTo.Phone = "5022973852";

                //Package package = new Package();
                //package.PackType = UPS_PackagingType.CustomerSupplied;
                //package.Weight = "15";
                //Rate r = new Rate("RATE", "6D1E455E9EE5C60E", "jnoble21", "SUS12qaz", "9A14T7");
                //Console.ReadKey();
                //var reply = r.SubmitRateRequest(shipFrom, shipFrom, shipTo, UPScode.NextDayEarlyAM, package);
                //Console.ReadKey();
                //var aa = reply;
                try
                {
                    Transit t = new Transit();
                    t.Initialize("6D1E455E9EE5C60E", "jnoble21", "SUS12qaz");
                    t.AddShipFrom(shipFrom);
                    t.AddShipTo(shipTo);
                    t.AddPickup(DateTime.Now.AddMonths(1));
                    t.SetUnits();
                    t.SetInvoice();
                    t.SetPackage("10", "1");
                    var result = t.Estimate();
                    Transit.RESPONSE r = new Transit.RESPONSE(result);
                    Console.ReadKey();
                }
                catch
                {

                }
            }
        }
        public void test()
        {
            TimeInTransitService tntService = new TimeInTransitService();
            TimeInTransitRequest tntRequest = new TimeInTransitRequest();
            RequestType request = new RequestType();
            String[] requestOption = { "TNT" };
            request.RequestOption = requestOption;
            tntRequest.Request = request;

            RequestShipFromType shipFrom = new RequestShipFromType();
            RequestShipFromAddressType addressFrom = new RequestShipFromAddressType();
            addressFrom.City = "ShipFrom city";
            addressFrom.CountryCode = "ShipFrom country";
            addressFrom.PostalCode = "ShipFrom postal code";
            addressFrom.StateProvinceCode = "ShipFrom state province code";
            shipFrom.Address = addressFrom;
            tntRequest.ShipFrom = shipFrom;

            RequestShipToType shipTo = new RequestShipToType();
            RequestShipToAddressType addressTo = new RequestShipToAddressType();
            addressTo.City = "ShipTo city";
            addressTo.CountryCode = "ShipTo country code";
            addressTo.PostalCode = "ShipTo postal code";
            addressTo.StateProvinceCode = "ShipTo state province code";
            shipTo.Address = addressTo;
            tntRequest.ShipTo = shipTo;

            PickupType pickup = new PickupType();
            pickup.Date = "Your pickup date";
            tntRequest.Pickup = pickup;

            //Below code uses dummy data for reference. Please update as required.
            ShipmentWeightType shipmentWeight = new ShipmentWeightType();
            shipmentWeight.Weight = "10";
            CodeDescriptionType unitOfMeasurement = new CodeDescriptionType();
            unitOfMeasurement.Code = "KGS";
            unitOfMeasurement.Description = "Kilograms";
            shipmentWeight.UnitOfMeasurement = unitOfMeasurement;
            tntRequest.ShipmentWeight = shipmentWeight;

            tntRequest.TotalPackagesInShipment = "1";
            InvoiceLineTotalType invoiceLineTotal = new InvoiceLineTotalType();
            invoiceLineTotal.CurrencyCode = "CAD";
            invoiceLineTotal.MonetaryValue = "10";
            tntRequest.InvoiceLineTotal = invoiceLineTotal;
            tntRequest.MaximumListSize = "1";

            UPSSecurity upss = new UPSSecurity();
            UPSSecurityServiceAccessToken upsSvcToken = new UPSSecurityServiceAccessToken();
            upsSvcToken.AccessLicenseNumber = "Your access license number";
            upss.ServiceAccessToken = upsSvcToken;
            UPSSecurityUsernameToken upsSecUsrnameToken = new UPSSecurityUsernameToken();
            upsSecUsrnameToken.Username = "Your username";
            upsSecUsrnameToken.Password = "Your password";
            upss.UsernameToken = upsSecUsrnameToken;
            tntService.UPSSecurityValue = upss;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = Transit.CheckValidationResult;
            //Console.WriteLine(tntRequest);
            TimeInTransitResponse tntResponse = tntService.ProcessTimeInTransit(tntRequest);
            //Console.WriteLine("Response code: " + tntResponse.Response.ResponseStatus.Code);
            //Console.WriteLine("Response description: " + tntResponse.Response.ResponseStatus.Description);
        }
    }

    public class Transit:ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        [Serializable]
        public class RESPONSE
        {
            public RESPONSE()
            {
            }
            public RESPONSE(TimeInTransitResponse response, List<Exception>exceptions=null)
            {
                Response = response;
                Exceptions = exceptions;
            }
            private TimeInTransitResponse m_response;
            public TimeInTransitResponse Response
            {
                get { return m_response; }
                set
                {
                    m_response = value;
                    try
                    {
                        Summary = ((TransitResponseType)value.Item).ServiceSummary;
                    }
                    catch { }
                }
            }
            public ServiceSummaryType[] Summary;
            public List<Exception> Exceptions = new List<Exception>();
        }
        public class ExceptionOccured : EventArgs
        {
            public ExceptionOccured(Exception ex = null)
            {
                if (ex != null)
                {
                    Exception = ex;
                }
            }
            public Exception Exception { get; set; }
        }
        public class ReturnEvent : EventArgs
        {
            public ReturnEvent(Transit.RESPONSE obj = null)
            {
                if (obj != null)
                {
                    Response = obj;
                }
            }
            public Transit.RESPONSE Response { get; set; }
        }
        public static List<Exception> Exceptions = new List<Exception>();
        protected virtual void newException(ExceptionOccured e)
        {
            Exceptions.Add(e.Exception);
            EventHandler<ExceptionOccured> handler = ExceptionListener;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<ExceptionOccured> ExceptionListener;
        protected virtual void newSoapException(SoapExceptionOccured e)
        {
            Exceptions.Add(e.Exception);
            EventHandler<SoapExceptionOccured> handler = SoapExceptionListener;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<SoapExceptionOccured> SoapExceptionListener;
        protected virtual void ReturnReady(ReturnEvent obj)
        {
            EventHandler<ReturnEvent> handler = ReturnListener;
            if (handler != null)
            {
                handler(this, obj);
            }
        }
        public event EventHandler<ReturnEvent> ReturnListener;
        public Transit()
        {
            Initialize();
        }
        public TimeInTransitService tntService;
        public TimeInTransitRequest tntRequest;
        public RequestType request;
        public String[] requestOption;
        public RequestShipFromType shipFrom ;
        public RequestShipFromAddressType addressFrom;
        public RequestShipToType shipTo;
        public RequestShipToAddressType addressTo ;
        public PickupType pickup;
        public ShipmentWeightType shipmentWeight;
        public CodeDescriptionType unitOfMeasurement;
        public InvoiceLineTotalType invoiceLineTotal ;
        public UPSSecurity upss;
        public UPSSecurityServiceAccessToken upsSvcToken ;
        public UPSSecurityUsernameToken upsSecUsrnameToken ;
        //result
        public TimeInTransitResponse tntResponse;
        public Transit Initialize(string access = "", string userid = "", string password = "")
        {
            tntService = new TimeInTransitService();
            tntService.Url = "https://wwwcie.ups.com/webservices/TimeInTransit"; //maybe not needed
            tntRequest = new TimeInTransitRequest();
            request = new RequestType();
            requestOption =new String[] { "TNT" };
            request.RequestOption = requestOption;
            tntRequest.Request = request;
            shipFrom = new RequestShipFromType();
            addressFrom = new RequestShipFromAddressType();
            shipTo = new RequestShipToType();
            addressTo = new RequestShipToAddressType();
            pickup = new PickupType();
            shipmentWeight = new ShipmentWeightType();
            unitOfMeasurement = new CodeDescriptionType();
            invoiceLineTotal = new InvoiceLineTotalType();
            upss = new UPSSecurity();
            upsSvcToken = new UPSSecurityServiceAccessToken();
            upsSecUsrnameToken = new UPSSecurityUsernameToken();
            //account
            upsSvcToken.AccessLicenseNumber = access;
            upss.ServiceAccessToken = upsSvcToken;

            upsSecUsrnameToken.Username = userid;
            upsSecUsrnameToken.Password = password;
            upss.UsernameToken = upsSecUsrnameToken;
            tntService.UPSSecurityValue = upss;
            SetUnits();
            SetInvoice();
            return this;
        }
        public Transit AddShipFrom(Address address)
        {
            try
            {
                addressFrom.City = address.City;
                addressFrom.PostalCode = address.Postal;
                addressFrom.StateProvinceCode = address.State;
                addressFrom.CountryCode = address.Country;
                shipFrom.Address = this.addressFrom;
                tntRequest.ShipFrom = shipFrom;

            }
            
            catch (Exception ex)
            {
                //Console.WriteLine("");
                //Console.WriteLine("-------------------------");
                //Console.WriteLine(" General Exception= " + ex.Message);
                //Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                //Console.WriteLine("-------------------------");

            }
            return this;
        }
        public Transit AddShipTo(Address address)
        {
            try
            {

                this.addressTo.City = address.City;
                this.addressTo.PostalCode = address.Postal;
                this.addressTo.CountryCode = address.Country;
                this.addressTo.StateProvinceCode = address.State;
                shipTo.Address = this.addressTo;
                tntRequest.ShipTo = shipTo;
            }
            catch (Exception ex)
            {
                newException(new ExceptionOccured(ex));
                //Console.WriteLine("");
                //Console.WriteLine("-------------------------");
                //Console.WriteLine(" General Exception= " + ex.Message);
                //Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                //Console.WriteLine("-------------------------");

            }
            return this;
        }
        public Transit AddPickup(DateTime date)
        {
            //YYYYMMDD
            pickup.Date = date.ToString("yyyyMMdd");
            tntRequest.Pickup = pickup;
            return this;
        }
        public Transit SetUnits(string unit="LBS", string unitdesc="Pounds")
        {
            unitOfMeasurement.Code = unit;
            unitOfMeasurement.Description = unitdesc;
            shipmentWeight.UnitOfMeasurement = unitOfMeasurement;
            return this;
        }
        public Transit SetPackage(string weight="0",string packageCount="1")
        {
            shipmentWeight.Weight = weight;

            tntRequest.ShipmentWeight = shipmentWeight;

            tntRequest.TotalPackagesInShipment = packageCount;
            return this;
        }
        public Transit SetInvoice(string CurrencyCode = "USD", string packageValue = "1", string listSize="1")
        {
            invoiceLineTotal.CurrencyCode = CurrencyCode;
            invoiceLineTotal.MonetaryValue = packageValue;
            tntRequest.InvoiceLineTotal = invoiceLineTotal;
            tntRequest.MaximumListSize = listSize;
            return this;
        }
        public TimeInTransitResponse Estimate()
        {
            try
            {
                if (NetInfo.CheckForInternetConnection())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;

                    RESPONSE responce = new RESPONSE();
                    responce.Exceptions = Exceptions;
                    responce.Response = tntResponse = tntService.ProcessTimeInTransit(tntRequest);
                    ReturnReady(new ReturnEvent(responce));
                    //Console.WriteLine("");
                    //Console.WriteLine("Response Code: " + tntResponse.Response.ResponseStatus.Code + " :: " + (tntResponse.Response.ResponseStatus.Description));
                    //Console.WriteLine("");
                    //Console.WriteLine(tntResponse.Item);

                    return tntResponse;
                }return null;

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                newSoapException(new SoapExceptionOccured(ex));
                //Console.WriteLine("");
                //Console.WriteLine("---------Time In Transit Web Service returns error----------------");
                //Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                //Console.WriteLine("SoapException Message= " + ex.Message);
                //Console.WriteLine("");
                //Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                //Console.WriteLine("");
                //Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                //Console.WriteLine("");
                //Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                //Console.WriteLine("-------------------------");
                //Console.WriteLine("");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                newException(new ExceptionOccured(ex));

                //Console.WriteLine("");
                //Console.WriteLine("--------------------");
                //Console.WriteLine("CommunicationException= " + ex.Message);
                //Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                //Console.WriteLine("-------------------------");
                //Console.WriteLine("");

            }
            catch (Exception ex)
            {
                newException(new ExceptionOccured(ex));
                //Console.WriteLine("");
                //Console.WriteLine("-------------------------");
                //Console.WriteLine(" Generaal Exception= " + ex.Message);
                //Console.WriteLine(" Generaal Exception-StackTrace= " + ex.StackTrace);
                //Console.WriteLine("-------------------------");

            }
            
            return null;
        }
        public bool EstimateAsync(ProcessTimeInTransitCompletedEventHandler handler = null)
        {
            try
            {
               
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;

                    if (handler == null)
                        tntService.ProcessTimeInTransitCompleted += TntService_ProcessTimeInTransitCompleted;
                    else
                        tntService.ProcessTimeInTransitCompleted += handler;

                    tntService.ProcessTimeInTransitAsync(tntRequest);
                    return true;
                
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                newSoapException(new SoapExceptionOccured(ex));
                //Console.WriteLine("");
                //Console.WriteLine("---------Time In Transit Web Service returns error----------------");
                //Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                //Console.WriteLine("SoapException Message= " + ex.Message);
                //Console.WriteLine("");
                //Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                //Console.WriteLine("");
                //Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                //Console.WriteLine("");
                //Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                //Console.WriteLine("-------------------------");
                //Console.WriteLine("");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                newException(new ExceptionOccured(ex));

                //Console.WriteLine("");
                //Console.WriteLine("--------------------");
                //Console.WriteLine("CommunicationException= " + ex.Message);
                //Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                //Console.WriteLine("-------------------------");
                //Console.WriteLine("");

            }
            catch (Exception ex)
            {
                newException(new ExceptionOccured(ex));
                //Console.WriteLine("");
                //Console.WriteLine("-------------------------");
                //Console.WriteLine(" Generaal Exception= " + ex.Message);
                //Console.WriteLine(" Generaal Exception-StackTrace= " + ex.StackTrace);
                //Console.WriteLine("-------------------------");

            }
            return false;
        }

        private void TntService_ProcessTimeInTransitCompleted(object sender, ProcessTimeInTransitCompletedEventArgs e)
        {
            RESPONSE responce = new RESPONSE();
            Exceptions.Add(e.Error);
            responce.Exceptions = Exceptions;
            responce.Response = tntResponse = e.Result;
            ReturnReady(new ReturnEvent(responce));
            //Console.WriteLine("");
            //Console.WriteLine("Response Code: " + tntResponse.Response.ResponseStatus.Code + " :: " + (tntResponse.Response.ResponseStatus.Description));
            //Console.WriteLine("");
            //Console.WriteLine(tntResponse.Item);
        }

        public static bool CheckValidationResult(object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        {

            return true;
        }
       
}
}
