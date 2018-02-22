using System;
using System.Net;

using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ShippingAPI.WebReference1;

namespace ShippingAPI
{
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
    public class SoapExceptionOccured : EventArgs
    {
        public SoapExceptionOccured(System.Web.Services.Protocols.SoapException ex = null)
        {
            if (ex != null)
            {
                Exception = ex;
            }
        }
        public System.Web.Services.Protocols.SoapException Exception { get; set; }
    }

    public class Rate : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public class ReturnEvent : EventArgs
        {
            public ReturnEvent(RESPONSE obj = null)
            {
                if (obj != null)
                {
                    Response = obj;
                }
            }
            public RESPONSE Response { get; set; }
        }
        [Serializable]
        public class RESPONSE
        {
            public RateResponse Response;
            public string TrackingNumber;
            public string ResponseStatus;
            public Image Label;
            public string Price;
            public string Negotiated;
            public object Tag;
            public List<Exception> Exceptions = new List<Exception>();
        }
        public static List<Exception> Exceptions = new List<Exception>();
        public event EventHandler<ExceptionOccured> ExceptionListener;
        public event EventHandler<SoapExceptionOccured> SoapExceptionListener;
        protected virtual void newSoapException(SoapExceptionOccured e)
        {
            try
            {
                Exceptions.Add(e.Exception);
                EventHandler<SoapExceptionOccured> handler = SoapExceptionListener;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            catch (Exception ex) { newException(new ExceptionOccured(ex)); }
        }
        protected virtual void newException(ExceptionOccured e)
        {
            try
            {
                Exceptions.Add(e.Exception);
                EventHandler<ExceptionOccured> handler = ExceptionListener;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            catch (Exception ex) { newException(new ExceptionOccured(ex)); }
        }
        protected virtual void ReturnReady(ReturnEvent obj)
        {
            try { 
            EventHandler<ReturnEvent> handler = ReturnListener;
            if (handler != null)
            {
                handler(this, obj);
            }
            }
            catch (Exception ex) { newException(new ExceptionOccured(ex)); }
        }
        public event EventHandler<ReturnEvent> ReturnListener;
        public Rate()
        {
            Initialize();
        }
        public Rate(string option, string access = "", string userid = "", string password = "", string acctNumber = "")
        {
            Initialize(option,access,userid,password,acctNumber);
        }

        public RateService _Rate;
        public RateRequest _RateRequest;
        public UPSSecurity _UPSSecurity;
        public UPSSecurityServiceAccessToken upssSvcAccessToken;
        public UPSSecurityUsernameToken upssUsrNameToken;
        public RequestType _Request;
        public ShipmentType _Shipment;
        public ShipperType _Shipper;
        public Rate Initialize(string option = "Rate", string access = "", string userid = "", string password = "", string acctNumber = "") // or "Shop"
        {
            this._Rate = new RateService();
            this._RateRequest = new RateRequest();
            this._UPSSecurity = new UPSSecurity();
            this.upssSvcAccessToken = new UPSSecurityServiceAccessToken();
            upssSvcAccessToken.AccessLicenseNumber = access;
            _UPSSecurity.ServiceAccessToken = upssSvcAccessToken;
            this.upssUsrNameToken = new UPSSecurityUsernameToken();
            upssUsrNameToken.Username = userid;
            upssUsrNameToken.Password = password;
            _UPSSecurity.UsernameToken = upssUsrNameToken;
            this._Rate.UPSSecurityValue = _UPSSecurity;

            this._Request = new RequestType();
            string[] requestOption = { option };

            _Request.RequestOption = requestOption;
            _RateRequest.Request = _Request;
            this._Shipment = new ShipmentType();
            this._Shipper = new ShipperType();
            _Shipper.ShipperNumber = acctNumber;

            return this;
        }
        public AddressType ShipperAddress;
        public Rate SetOption(string Option)
        {
            this._Request = new RequestType();
            string[] requestOption = { Option };
            _Request.RequestOption = requestOption;
            _RateRequest.Request = _Request;
            return this;
        }
        public Rate AddShipper(Address address)
        {
            //shipper
            ShipperAddress = new AddressType();
            String[] addressLine = address.AddressLine;
            ShipperAddress.AddressLine = addressLine;
            ShipperAddress.City = address.City;
            ShipperAddress.PostalCode = address.Postal;
            ShipperAddress.StateProvinceCode = address.State;
            ShipperAddress.CountryCode = address.Country;
            ShipperAddress.AddressLine = addressLine;
            this._Shipper.Address = ShipperAddress;
            this._Shipment.Shipper = this._Shipper;
            return this;
        }
        public ShipFromType _ShipFrom;
        public Rate AddShipFrom(Address address)
        {
            this._ShipFrom = new ShipFromType();
            ShipAddressType shipFromAddress = new ShipAddressType();
            shipFromAddress.AddressLine = address.AddressLine;
            shipFromAddress.City = address.City;
            shipFromAddress.PostalCode = address.Postal;
            shipFromAddress.StateProvinceCode = address.State;
            shipFromAddress.CountryCode = address.Country;
            _ShipFrom.Address = shipFromAddress;
            this._Shipment.ShipFrom = _ShipFrom;
            return this;
        }
        public ShipToType _ShipTo;
        public ShipToAddressType ShipToAddress;
        public Rate AddShipTo(Address address)
        {
            this._ShipTo = new ShipToType();
            this.ShipToAddress = new ShipToAddressType();
            String[] addressLine1 = address.AddressLine;
            ShipToAddress.AddressLine = addressLine1;
            ShipToAddress.City = address.City;
            ShipToAddress.PostalCode = address.Postal;
            ShipToAddress.StateProvinceCode = address.State;
            ShipToAddress.CountryCode = address.Country;
            _ShipTo.Address = ShipToAddress;
            this._Shipment.ShipTo = _ShipTo;
            return this;
        }
        public CodeDescriptionType _Service;
        public Rate SelectServiceCode(UPScode code)
        {
            this._Service = new CodeDescriptionType();


            var values = Enum.GetValues(typeof(UPScode));
            int value = 03;
            foreach (int val in values)
            {
                string check = Enum.GetName(typeof(UPScode), val);
                if (check == code.ToString())
                {
                    value = val;
                }
            }
            string actual = "0";
            if (value < 10)
            {
                actual += value.ToString();
            }
            else
            {
                actual = value.ToString();
            }
            _Service.Code = actual;
            this._Shipment.Service = _Service;
            return this;
        }
        public PackageType _Package;
        public Rate AddPackage(Package package)
        {
            this._Package = new PackageType();
            PackageWeightType packageWeight = new PackageWeightType();
            packageWeight.Weight = package.Weight;
            CodeDescriptionType uom = new CodeDescriptionType();
            uom.Code = "LBS";
            uom.Description = "pounds";
            packageWeight.UnitOfMeasurement = uom;
            _Package.PackageWeight = packageWeight;
            CodeDescriptionType packType = new CodeDescriptionType();

            var values = Enum.GetValues(typeof(UPS_PackagingType));
            int value = 02;
            foreach (int val in values)
            {
                string check = Enum.GetName(typeof(UPScode), val);
                if (check == package.PackType.ToString())
                {
                    value = val;
                }
            }
            string actual = "0";
            if (value < 10)
            {
                actual += value.ToString();
            }
            else
            {
                actual = value.ToString();
            }

            packType.Code = actual;
            _Package.PackagingType = packType;
            PackageType[] pkgArray = { _Package };
            _Shipment.Package = pkgArray;
            _RateRequest.Shipment = _Shipment;
            return this;
        }
        public Rate FlagNegotiatedRate(bool negotiated = false)
        {
            try
            {
                if (negotiated)
                {
                    ShipmentRatingOptionsType rt = new ShipmentRatingOptionsType();
                    rt.NegotiatedRatesIndicator = "";
                    _RateRequest.Shipment.ShipmentRatingOptions = rt;
                    this._Shipment.ShipmentRatingOptions = rt;
                }
            }
            catch(Exception ex) {
                newException(new ExceptionOccured(ex));
            }
            return this;
        }

        public RateResponse RateResponse;
        public string _ShippingCharges;
        public RESPONSE SubmitRateRequest()
        {
            try
            {
                if (NetInfo.CheckForInternetConnection())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                    ////Console.WriteLine(_RateRequest);
                    this.RateResponse = _Rate.ProcessRate(_RateRequest);
                    _ShippingCharges = this.RateResponse.RatedShipment[0].TotalCharges.MonetaryValue + this.RateResponse.RatedShipment[0].TotalCharges.CurrencyCode;
                    //Console.WriteLine("The transaction was a " + this.RateResponse.Response.ResponseStatus.Description);
                    //Console.WriteLine("Total Shipment Charges " + this.RateResponse.RatedShipment[0].TotalCharges.MonetaryValue + this.RateResponse.RatedShipment[0].TotalCharges.CurrencyCode);
                    //Console.ReadKey();
                    RESPONSE responce = new RESPONSE();
                    responce.Response = RateResponse;
                    responce.Exceptions = Exceptions;
                    try
                    {
                        responce.Negotiated = RateResponse.RatedShipment[0].NegotiatedRateCharges.TotalCharge.MonetaryValue + RateResponse.RatedShipment[0].NegotiatedRateCharges.TotalCharge.CurrencyCode;
                    }
                    catch { }
                    responce.ResponseStatus = this.RateResponse.Response.ResponseStatus.Description;
                    responce.Price = _ShippingCharges = this.RateResponse.RatedShipment[0].TotalCharges.MonetaryValue + this.RateResponse.RatedShipment[0].TotalCharges.CurrencyCode;
                    ReturnReady(new ReturnEvent(responce));
                    return responce;
                }
                
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                newException(new ExceptionOccured(ex));
                //Console.WriteLine("");
                //Console.WriteLine("---------Rate Web Service returns error----------------");
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
        public void SubmitRateRequestAsync(ProcessRateCompletedEventHandler handler=null)
        {

            try
            {
                if (NetInfo.CheckForInternetConnection())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                    ////Console.WriteLine(_RateRequest);\
                    if (handler != null)
                    {
                        _Rate.ProcessRateCompleted += handler;
                        _Rate.ProcessRateAsync(_RateRequest);
                    }
                    else
                    {
                        _Rate.ProcessRateCompleted += _Rate_ProcessRateCompleted;
                        _Rate.ProcessRateAsync(_RateRequest);
                    }

                    return;
                }

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                //Console.WriteLine("");
                //Console.WriteLine("---------Ship Web Service returns error----------------");
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
                //Console.WriteLine("");
                //Console.WriteLine("--------------------");
                //Console.WriteLine("CommunicationException= " + ex.Message);
                //Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                //Console.WriteLine("-------------------------");
                //Console.WriteLine("");

            }
            catch (Exception ex) { newException(new ExceptionOccured(ex)); }

        }
        public Rate SubmitRateRequest(Address shipper, Address shipFrom, Address shipTo, UPScode code, Package package)
        {
            try
            {
                if (NetInfo.CheckForInternetConnection())
                {
                    AddShipper(shipper);
                    AddShipFrom(shipFrom);
                    AddShipTo(shipTo);
                    SelectServiceCode(code);
                    AddPackage(package);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                    ////Console.WriteLine(_RateRequest);
                    _RateRequest.Shipment.Service = _Service;
                    this.RateResponse = _Rate.ProcessRate(_RateRequest);
                    _ShippingCharges = this.RateResponse.RatedShipment[0].TotalCharges.MonetaryValue + this.RateResponse.RatedShipment[0].TotalCharges.CurrencyCode;
                    
                    //Console.WriteLine("The transaction was a " + this.RateResponse.Response.ResponseStatus.Description);
                    //Console.WriteLine("Total Shipment Charges " + this.RateResponse.RatedShipment[0].TotalCharges.MonetaryValue + this.RateResponse.RatedShipment[0].TotalCharges.CurrencyCode);
                    //Console.ReadKey();                    
                    _RESPONSE = new RESPONSE();
                    try
                    {
                        _RESPONSE.Negotiated = RateResponse.RatedShipment[0].NegotiatedRateCharges.TotalCharge.MonetaryValue + RateResponse.RatedShipment[0].NegotiatedRateCharges.TotalCharge.CurrencyCode;
                    }
                    catch { }
                    _RESPONSE.Response = RateResponse;
                    _RESPONSE.Exceptions = Exceptions;
                    _RESPONSE.Exceptions = Exceptions;
                    try
                    {
                        _RESPONSE.Negotiated = RateResponse.RatedShipment[0].NegotiatedRateCharges.TotalCharge.MonetaryValue + RateResponse.RatedShipment[0].NegotiatedRateCharges.TotalCharge.CurrencyCode;
                    }
                    catch { }
                    _RESPONSE.ResponseStatus = this.RateResponse.Response.ResponseStatus.Description;
                    _RESPONSE.Price = _ShippingCharges = this.RateResponse.RatedShipment[0].TotalCharges.MonetaryValue + this.RateResponse.RatedShipment[0].TotalCharges.CurrencyCode;
                    ReturnReady(new ReturnEvent(_RESPONSE));
                }

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                newSoapException(new SoapExceptionOccured(ex));
                //Console.WriteLine("");
                //Console.WriteLine("---------Rate Web Service returns error----------------");
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
            return this;
        }
        public void SubmitRateRequestAsync(Address shipper, Address shipFrom, Address shipTo, UPScode code, Package package, ProcessRateCompletedEventHandler handler=null)
        {
            try
            {
                AddShipper(shipper);
                AddShipFrom(shipFrom);
                AddShipTo(shipTo);
                SelectServiceCode(code);
                AddPackage(package);
                if (NetInfo.CheckForInternetConnection())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                    ////Console.WriteLine(_RateRequest);\
                    if (handler != null)
                    {
                        _Rate.ProcessRateCompleted += handler;
                        
                        _Rate.ProcessRateAsync(_RateRequest);
                    }
                    else
                    {
                        _Rate.ProcessRateCompleted += _Rate_ProcessRateCompleted;
                        
                        _Rate.ProcessRateAsync(_RateRequest);
                    }

                    return;
                }
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                //Console.WriteLine("");
                //Console.WriteLine("---------Ship Web Service returns error----------------");
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
            }
        }
        private void _Rate_ProcessRateCompleted(object sender, ProcessRateCompletedEventArgs e)
        {
            try
            {
                this.RateResponse = e.Result;
                _ShippingCharges = this.RateResponse.RatedShipment[0].TotalCharges.MonetaryValue + this.RateResponse.RatedShipment[0].TotalCharges.CurrencyCode;
                //Console.WriteLine("The transaction was a " + this.RateResponse.Response.ResponseStatus.Description);
                //Console.WriteLine("Total Shipment Charges " + this.RateResponse.RatedShipment[0].TotalCharges.MonetaryValue + this.RateResponse.RatedShipment[0].TotalCharges.CurrencyCode);
                //Console.ReadKey();
                RESPONSE responce = new RESPONSE();
                responce.Response = RateResponse;

                responce.Exceptions = Exceptions;
                try
                {
                    responce.Negotiated = RateResponse.RatedShipment[0].NegotiatedRateCharges.TotalCharge.MonetaryValue + RateResponse.RatedShipment[0].NegotiatedRateCharges.TotalCharge.CurrencyCode;
                }
                catch (Exception exx) { }// newException(new ExceptionOccured(exx));
                responce.ResponseStatus = this.RateResponse.Response.ResponseStatus.Description;
                responce.Price = _ShippingCharges = this.RateResponse.RatedShipment[0].TotalCharges.MonetaryValue + this.RateResponse.RatedShipment[0].TotalCharges.CurrencyCode;
                ReturnReady(new ReturnEvent(responce));
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                //Console.WriteLine("");
                //Console.WriteLine("---------Ship Web Service returns error----------------");
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
            }
          
        }
        public RESPONSE _RESPONSE;
        public static bool CheckValidationResult(object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        {

            return true;
        }
    }
    public class NetInfo
    {
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }

    
  

}

