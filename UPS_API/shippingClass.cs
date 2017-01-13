using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShippingAPI.WebReference1;
using System.Drawing;

namespace ShippingAPI
{
   
    public class Ship
        {
        
        [Serializable]
        public class RESPONSE
        {
            public ShipmentResponse Response;
            public string TrackingNumber;
            public Image Label;
            public string Price;
            public object Tag;
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
            public ReturnEvent(Ship.RESPONSE obj = null)
            {
                if (obj != null)
                {
                    Response = obj;
                }
            }
            public Ship.RESPONSE Response { get; set; }
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
        protected virtual void ReturnReady(ReturnEvent obj)
        {
            EventHandler<ReturnEvent> handler = ReturnListener;
            if (handler != null)
            {
                handler(this, obj);
            }
        }
        public event EventHandler<ReturnEvent> ReturnListener;
        public static Ship CreateLabel(Package package, Address shipper, Address shipFrom, Address shipTo, UPScode code, string labelType = "GIF")
            {
                Ship SL = new Ship();
            SL.Initialize();
            SL.AddShipper( shipper);
            SL.AddShipFrom(shipFrom);
            SL.AddShipTo(shipTo);
            SL.SelectServiceCode( code);
            SL.AddPackage(package);
            SL.ConfigureLabel(labelType);
            SL.ProcessShipment();
                return SL;
            }
            public Ship()
            {
                System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
                Initialize();
            }
            public Ship(bool custom, string access = "", string userid = "", string password = "", string acctNumber = "")
            {
                System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
                Initialize(access, userid, password, acctNumber);
            }
            public Ship Initialize(string access = "", string userid = "", string password = "", string acctNumber = "")
            {
                try
                {
                    this._ShipService = new ShipService();
                    this._ShipmentRequest = new ShipmentRequest();
                    this.UPSSecurity = new UPSSecurity();
                    this.UPSSecurityServiceAccessToken = new UPSSecurityServiceAccessToken();
                    this.UPSSecurityServiceAccessToken.AccessLicenseNumber = access;
                    this.UPSSecurity.ServiceAccessToken = this.UPSSecurityServiceAccessToken;
                    this.UPSSecurityUsernameToken = new UPSSecurityUsernameToken();
                    this.UPSSecurityUsernameToken.Username = userid;
                    this.UPSSecurityUsernameToken.Password = password;
                    this.UPSSecurity.UsernameToken = this.UPSSecurityUsernameToken;
                    this._ShipService.UPSSecurityValue = this.UPSSecurity;

                    this.RequestType = new RequestType();
                    String[] requestOption = { "nonvalidate" }; //non validate is does not validate address
                    this.RequestType.RequestOption = requestOption;
                    this._ShipmentRequest.Request = this.RequestType;

                    this._Shipment = new ShipmentType();
                    this._Shipment.Description = "Starrag US Service Department";
                    this._Shipper = new ShipperType();
                    this._Shipper.ShipperNumber = acctNumber;
                    this.PaymentInfoType = new PaymentInfoType();
                    this.ShipmentChargeType = new ShipmentChargeType();
                    this.BillShipperType = new BillShipperType();
                    this.BillShipperType.AccountNumber = acctNumber;
                    this.ShipmentChargeType.BillShipper = this.BillShipperType;
                    this.ShipmentChargeType.Type = "01"; //always 01, 02 is duty free
                    ShipmentChargeType[] shpmentChargeArray = { this.ShipmentChargeType };
                    this.PaymentInfoType.ShipmentCharge = shpmentChargeArray;
                    this._Shipment.PaymentInformation = this.PaymentInfoType;
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("---------Ship Web Service returns error----------------");
                    Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                    Console.WriteLine("SoapException Message= " + ex.Message);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("--------------------");
                    Console.WriteLine("CommunicationException= " + ex.Message);
                    Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(" General Exception= " + ex.Message);
                    Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");

                }
                return this;
            }
            public Ship AddShipper( Address address)
            {
                try
                {

                    this._ShipperAddress = new ShipAddressType();

                    this._ShipperAddress.AddressLine = address.AddressLine;
                    this._ShipperAddress.City = address.City;
                    this._ShipperAddress.PostalCode = address.Postal;
                    this._ShipperAddress.StateProvinceCode = address.State;
                    this._ShipperAddress.CountryCode = address.Country;
                    this._ShipperAddress.AddressLine = address.AddressLine;
                    this._Shipper.Address = this._ShipperAddress;
                    this._Shipper.Name = address.Name;
                    this._Shipper.AttentionName = address.AttentionName;
                    ShipPhoneType shipperPhone = new ShipPhoneType();
                    shipperPhone.Number = address.Phone;
                    this._Shipper.Phone = shipperPhone;

                    this._Shipment.Shipper = this._Shipper;
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("---------Ship Web Service returns error----------------");
                    Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                    Console.WriteLine("SoapException Message= " + ex.Message);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("--------------------");
                    Console.WriteLine("CommunicationException= " + ex.Message);
                    Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(" General Exception= " + ex.Message);
                    Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");

                }
                return this;
            }
            public Ship AddShipFrom( Address address)
            {
                try
                {

                    this._ShipFrom = new ShipFromType();
                    ShipAddressType shipFromAddress = new ShipAddressType();
                    String[] shipFromAddressLine = address.AddressLine;
                    shipFromAddress.AddressLine = address.AddressLine;
                    shipFromAddress.City = address.City;
                    shipFromAddress.PostalCode = address.Postal;
                    shipFromAddress.StateProvinceCode = address.State;
                    shipFromAddress.CountryCode = address.Country;
                    this._ShipFrom.Address = shipFromAddress;
                    this._ShipFrom.AttentionName = address.AttentionName;
                    this._ShipFrom.Name = address.Name;
                    this._Shipment.ShipFrom = this._ShipFrom;
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("---------Ship Web Service returns error----------------");
                    Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                    Console.WriteLine("SoapException Message= " + ex.Message);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("--------------------");
                    Console.WriteLine("CommunicationException= " + ex.Message);
                    Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(" General Exception= " + ex.Message);
                    Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");

                }
                return this;
            }
            public Ship AddShipTo( Address address)
            {
                try
                {

                    this._ShipTo = new ShipToType();
                    ShipToAddressType shipToAddress = new ShipToAddressType();
                    String[] addressLine1 = address.AddressLine;
                    shipToAddress.AddressLine = addressLine1;
                    shipToAddress.City = address.City;
                    shipToAddress.PostalCode = address.Postal;
                    shipToAddress.CountryCode = address.Country;
                shipToAddress.StateProvinceCode = "KY";
                    this._ShipTo.Address = shipToAddress;
                    this._ShipTo.AttentionName = address.AttentionName;
                    this._ShipTo.Name = address.Name;
                    ShipPhoneType shipToPhone = new ShipPhoneType();
                    shipToPhone.Number = address.Phone;
                    this._ShipTo.Phone = shipToPhone;
                    this._Shipment.ShipTo = this._ShipTo;
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                newException(new ExceptionOccured(ex));
                Console.WriteLine("");
                    Console.WriteLine("---------Ship Web Service returns error----------------");
                    Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                    Console.WriteLine("SoapException Message= " + ex.Message);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                newException(new ExceptionOccured(ex));
                Console.WriteLine("");
                    Console.WriteLine("--------------------");
                    Console.WriteLine("CommunicationException= " + ex.Message);
                    Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");

                }
                catch (Exception ex)
                {
                newException(new ExceptionOccured(ex));
                Console.WriteLine("");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(" General Exception= " + ex.Message);
                    Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");

                }
                return this;
            }
            public Ship SelectServiceCode( UPScode code)
            {
                try
                {
                    this._ServiceCode = new ServiceType();
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
                this._ServiceCode.Code = actual;
                    this._Shipment.Service = this._ServiceCode;
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("---------Ship Web Service returns error----------------");
                    Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                    Console.WriteLine("SoapException Message= " + ex.Message);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("--------------------");
                    Console.WriteLine("CommunicationException= " + ex.Message);
                    Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(" General Exception= " + ex.Message);
                    Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");

                }
                return this;
            }
            public Ship AddITNLandInvoice( Address soldTo, string formtype = "01")
            {
                try
                {
                    this.ShipmentTypeShipmentServiceOptions = new ShipmentTypeShipmentServiceOptions();

                    /** **** International Forms ***** */
                    this.InternationalFormType = new InternationalFormType();

                    /** **** Commercial Invoice ***** */
                    String[] formTypeList = { formtype };
                    this.InternationalFormType.FormType = formTypeList;

                    /** **** Contacts and Sold To ***** */
                    this.Contacts = new ContactType();

                    this.SoldTo = new SoldToType();
                    SoldTo.Option = "1";
                    SoldTo.AttentionName = soldTo.AttentionName;
                    SoldTo.Name = soldTo.Name;
                    PhoneType soldToPhone = new PhoneType();
                    soldToPhone.Number = soldTo.Phone;
                    soldToPhone.Extension = "";
                    SoldTo.Phone = soldToPhone;
                    AddressType soldToAddress = new AddressType();
                    String[] soldToAddressLine = soldTo.AddressLine;
                    soldToAddress.AddressLine = soldToAddressLine;
                    soldToAddress.City = soldTo.City;
                    soldToAddress.PostalCode = soldTo.Postal;
                    soldToAddress.CountryCode = soldTo.Country;
                    SoldTo.Address = soldToAddress;
                    Contacts.SoldTo = SoldTo;

                    this.InternationalFormType.Contacts = Contacts;

                    /** **** Product ***** */
                    this.Product = new ProductType();
                    String[] description = { "Product 1" };
                    Product.Description = description;
                    Product.CommodityCode = "111222AA";
                    Product.OriginCountryCode = "US";
                    UnitType unit = new UnitType();
                    unit.Number = "147";
                    unit.Value = "478";
                    UnitOfMeasurementType uomProduct = new UnitOfMeasurementType();
                    uomProduct.Code = "BOX";
                    uomProduct.Description = "BOX";
                    unit.UnitOfMeasurement = uomProduct;
                    Product.Unit = unit;
                    ProductWeightType productWeight = new ProductWeightType();
                    productWeight.Weight = "10";
                    UnitOfMeasurementType uomForWeight = new UnitOfMeasurementType();
                    uomForWeight.Code = "LBS";
                    uomForWeight.Description = "LBS";
                    productWeight.UnitOfMeasurement = uomForWeight;
                    Product.ProductWeight = productWeight;
                    ProductType[] productList = { Product };
                    this.InternationalFormType.Product = productList;

                    /** **** InvoiceNumber, InvoiceDate, PurchaseOrderNumber, TermsOfShipment, ReasonForExport, Comments and DeclarationStatement  ***** */
                    this.InternationalFormType.InvoiceNumber = "asdf123";
                    this.InternationalFormType.InvoiceDate = "20151225";
                    this.InternationalFormType.PurchaseOrderNumber = "999jjj777";
                    this.InternationalFormType.TermsOfShipment = "CFR";
                    this.InternationalFormType.ReasonForExport = "Sale";
                    this.InternationalFormType.Comments = "Your Comments";
                    this.InternationalFormType.DeclarationStatement = "Your Declaration Statement";

                    /** **** Discount, FreightCharges, InsuranceCharges, OtherCharges and CurrencyCode  ***** */
                    IFChargesType discount = new IFChargesType();
                    discount.MonetaryValue = "100";
                    this.InternationalFormType.Discount = discount;
                    IFChargesType freight = new IFChargesType();
                    freight.MonetaryValue = "50";
                    this.InternationalFormType.FreightCharges = freight;
                    IFChargesType insurance = new IFChargesType();
                    insurance.MonetaryValue = "200";
                    this.InternationalFormType.InsuranceCharges = insurance;
                    OtherChargesType otherCharges = new OtherChargesType();
                    otherCharges.MonetaryValue = "50";
                    otherCharges.Description = "Misc";
                    this.InternationalFormType.OtherCharges = otherCharges;
                    this.InternationalFormType.CurrencyCode = "USD";


                    this.ShipmentTypeShipmentServiceOptions.InternationalForms = this.InternationalFormType;
                    this._Shipment.ShipmentServiceOptions = this.ShipmentTypeShipmentServiceOptions;
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("---------Ship Web Service returns error----------------");
                    Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                    Console.WriteLine("SoapException Message= " + ex.Message);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("--------------------");
                    Console.WriteLine("CommunicationException= " + ex.Message);
                    Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(" General Exception= " + ex.Message);
                    Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");

                }
                return this;
            }
            public Ship AddPackage( Package package)
            {
                try
                {
                    /**package**/
                    this._Package = new PackageType();
                    this.PackageWeight = new PackageWeightType();
                    PackageWeight.Weight = package.Weight;
                    ShipUnitOfMeasurementType uom = new ShipUnitOfMeasurementType();
                    uom.Code = "LBS";
                    PackageWeight.UnitOfMeasurement = uom;
                    _Package.PackageWeight = PackageWeight;
                    this.PackageType = new PackagingType();
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
                PackageType.Code = actual;
                    _Package.Packaging = PackageType;
                    PackageType[] pkgArray = { _Package };
                    this._Shipment.Package = pkgArray;
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("---------Ship Web Service returns error----------------");
                    Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                    Console.WriteLine("SoapException Message= " + ex.Message);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("--------------------");
                    Console.WriteLine("CommunicationException= " + ex.Message);
                    Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(" General Exception= " + ex.Message);
                    Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");

                }
                return this;
            }
            public LabelSpecificationType LabelSpecification;
            public LabelStockSizeType LabelStockSize; 
            public LabelImageFormatType LabelImageFormat;
            public Ship ConfigureLabel( string LabelType = "GIF")
            {
                //Label
                try
                {
                    this.LabelSpecification = new LabelSpecificationType();
                    this.LabelStockSize = new LabelStockSizeType();
                    LabelStockSize.Height = "6";
                    LabelStockSize.Width = "4";
                    LabelSpecification.LabelStockSize = LabelStockSize;
                    this.LabelImageFormat = new LabelImageFormatType();
                    LabelImageFormat.Code = LabelType;
                    LabelSpecification.LabelImageFormat = LabelImageFormat;
                    this._ShipmentRequest.LabelSpecification = LabelSpecification;
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("---------Ship Web Service returns error----------------");
                    Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                    Console.WriteLine("SoapException Message= " + ex.Message);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                    Console.WriteLine("");
                    Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");
                }
                catch (System.ServiceModel.CommunicationException ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("--------------------");
                    Console.WriteLine("CommunicationException= " + ex.Message);
                    Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");
                    Console.WriteLine("");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(" General Exception= " + ex.Message);
                    Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                    Console.WriteLine("-------------------------");

                }
                return this;
            }
            public string _TrackingNumber = null;
        public string _ShippingCharges = "";
        public Ship ProcessShipment()
            {
            try
            {
                this._ShipmentRequest.Shipment = this._Shipment;

                System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
                this.Response = this._ShipService.ProcessShipment(this._ShipmentRequest);
                this._TrackingNumber = this.Response.ShipmentResults.ShipmentIdentificationNumber;
                var aa = Convert.FromBase64String(this.Response.ShipmentResults.PackageResults[0].ShippingLabel.GraphicImage);
                Image x = LabelImage = (Bitmap)((new ImageConverter()).ConvertFrom(aa));
                Console.WriteLine("Serice was a " + Response.Response.ResponseStatus.Description);
                Console.WriteLine("Tracking number " + Response.ShipmentResults.ShipmentIdentificationNumber);
                RESPONSE responce = new RESPONSE();
                responce.Response = Response;
                responce.TrackingNumber = _TrackingNumber;
                responce.Label = x;
                responce.Exceptions = Exceptions;
                responce.Price = _ShippingCharges= Response.ShipmentResults.ShipmentCharges.TotalCharges.MonetaryValue+ Response.ShipmentResults.ShipmentCharges.TotalCharges.CurrencyCode;
                ReturnReady(new ReturnEvent(responce));
            }
           
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                newException(new ExceptionOccured(ex));
                Console.WriteLine("");
                Console.WriteLine("---------Ship Web Service returns error----------------");
                Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                Console.WriteLine("SoapException Message= " + ex.Message);
                Console.WriteLine("");
                Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                Console.WriteLine("");
                Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                Console.WriteLine("");
                Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");
                Console.WriteLine("");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                newException(new ExceptionOccured(ex));
                Console.WriteLine("");
                Console.WriteLine("--------------------");
                Console.WriteLine("CommunicationException= " + ex.Message);
                Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");
                Console.WriteLine("");

            }
            catch (Exception ex)
            {
                newException(new ExceptionOccured(ex));
                Console.WriteLine("");
                Console.WriteLine("-------------------------");
                Console.WriteLine(" General Exception= " + ex.Message);
                Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");

            }
            return this;
        }
        public Ship ProcessShipmentAsync()
        {
            try
            {
                if (NetInfo.CheckForInternetConnection())
                {
                    this._ShipmentRequest.Shipment = this._Shipment;
                    System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
                    _ShipService.ProcessShipmentCompleted += _ShipService_ProcessShipmentCompleted;
                    this._ShipService.ProcessShipmentAsync(this._ShipmentRequest);
                }
            }

            catch (System.Web.Services.Protocols.SoapException ex)
            {
                newException(new ExceptionOccured(ex));
                Console.WriteLine("");
                Console.WriteLine("---------Ship Web Service returns error----------------");
                Console.WriteLine("---------\"Hard\" is user error \"Transient\" is system error----------------");
                Console.WriteLine("SoapException Message= " + ex.Message);
                Console.WriteLine("");
                Console.WriteLine("SoapException Category:Code:Message= " + ex.Detail.LastChild.InnerText);
                Console.WriteLine("");
                Console.WriteLine("SoapException XML String for all= " + ex.Detail.LastChild.OuterXml);
                Console.WriteLine("");
                Console.WriteLine("SoapException StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");
                Console.WriteLine("");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                newException(new ExceptionOccured(ex));
                Console.WriteLine("");
                Console.WriteLine("--------------------");
                Console.WriteLine("CommunicationException= " + ex.Message);
                Console.WriteLine("CommunicationException-StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");
                Console.WriteLine("");

            }
            catch (Exception ex)
            {
                newException(new ExceptionOccured(ex));
                Console.WriteLine("");
                Console.WriteLine("-------------------------");
                Console.WriteLine(" General Exception= " + ex.Message);
                Console.WriteLine(" General Exception-StackTrace= " + ex.StackTrace);
                Console.WriteLine("-------------------------");

            }
            return this;
        }

        private void _ShipService_ProcessShipmentCompleted(object sender, ProcessShipmentCompletedEventArgs e)
        {
            this.Response = e.Result;
            this._TrackingNumber = this.Response.ShipmentResults.ShipmentIdentificationNumber;
            var aa = Convert.FromBase64String(this.Response.ShipmentResults.PackageResults[0].ShippingLabel.GraphicImage);
            Image x = LabelImage = (Bitmap)((new ImageConverter()).ConvertFrom(aa));
            Console.WriteLine("Serice was a " + Response.Response.ResponseStatus.Description);
            Console.WriteLine("Tracking number " + Response.ShipmentResults.ShipmentIdentificationNumber);
            RESPONSE responce = new RESPONSE();
            responce.Response = Response;
            responce.TrackingNumber = _TrackingNumber;
            responce.Label = x;
            responce.Exceptions = Exceptions;
            responce.Price = _ShippingCharges = Response.ShipmentResults.ShipmentCharges.TotalCharges.MonetaryValue + Response.ShipmentResults.ShipmentCharges.TotalCharges.CurrencyCode;
            ReturnReady(new ReturnEvent(responce));
        }

        public Image GetProcessedLabel()
            {
                try
                {
                    var aa = Convert.FromBase64String(this.Response.ShipmentResults.PackageResults[0].ShippingLabel.GraphicImage);
                    LabelImage = (Bitmap)((new ImageConverter()).ConvertFrom(aa));
                    return LabelImage;
                }
                catch { }
                return null;
            }


            public ShipmentResponse Response;
            public Image LabelImage = null;

            //pavkage
            public PackageType _Package;
            public PackageWeightType PackageWeight;
            public ShipUnitOfMeasurementType ShipUnitOfMeasurementType;
            public PackagingType PackageType;

            //security
            public ShipService _ShipService;
            public ShipmentRequest _ShipmentRequest;
            public UPSSecurity UPSSecurity;
            public UPSSecurityServiceAccessToken UPSSecurityServiceAccessToken;
            public UPSSecurityUsernameToken UPSSecurityUsernameToken;
            //request
            public RequestType RequestType;
            public String[] requestOption;
            public ShipmentType _Shipment;
            public ShipperType _Shipper;
            public PaymentInfoType PaymentInfoType;
            public ShipmentChargeType ShipmentChargeType;
            public BillShipperType BillShipperType;
            public ShipmentChargeType[] ShipmentChargeTypeArray;
            public ShipAddressType _ShipperAddress;
            public String[] addressLineo;
            public ShipPhoneType ShipPhoneType;

            public ShipFromType _ShipFrom;
            public ShipAddressType ShipAddressType2;
            public String[] shipFromAddressLine;
            public ShipToType _ShipTo;
            public ShipToAddressType ShipToAddressType;
            public String[] addressLine1;
            public ShipPhoneType ShipPhoneType2;
            public ServiceType _ServiceCode;

            public ShipmentTypeShipmentServiceOptions ShipmentTypeShipmentServiceOptions;

            public InternationalFormType InternationalFormType;

            public String[] formTypeList;

            public ContactType Contacts;
            public SoldToType SoldTo;
            public PhoneType PhoneType;
            public AddressType AddressType;
            public String[] soldToAddressLine;

            public ProductType Product;
            public String[] description;
            public UnitType UnitType;
            public UnitOfMeasurementType UnitOfMeasurementType;
            public ProductWeightType ProductWeightType;
            public UnitOfMeasurementType UnitOfMeasurementType2;
            public ProductType[] productList;
        }
        


    public enum UPScode
    {
        NextDayAir = 02, SecondDayAir = 02, Ground = 03, Express = 07, Expidited = 08,
        Standard = 11, ThreeDaySelect = 12, NextDayAirSaver = 13, NextDayEarlyAM = 14, ExpressPlus = 54,
        SecondDayAirAM = 59, UPSsaver = 65, UPSAccessPointEconomy = 70, UPSTodayStudent = 82,
        UPSTodayDedicatedCourier = 83
    }
    public enum UPS_PackagingType
    {
        UPSletter = 01, CustomerSupplied = 02, Tube = 03, PAK = 04, ExpressBox = 21, Express25kgBox = 24, Express10kgBox = 25,
        Pallet = 30, Flats = 56, Parcels = 57, BPM = 58, FirstClass = 59, Priority = 60, Machinables = 61, Irregulars = 62, ParcelPost = 63,
        BPMparcel = 64, MediaMail = 65, BPMflat = 66, StandardFlat = 67
    }
    public class Address
    {
        public Address()
        {

        }
        public Address(string[] addressLine, string city, string postal, string state, string country, string name, string attentionName, string phone)
        {
            AddressLine = addressLine;
            City = city;
            Postal = postal;
            State = state;
            Country = country;
            Name = name;
            AttentionName = attentionName;
            Phone = phone;
        }
        public string[] AddressLine;
        public string City;
        public string Postal;
        public string State;
        public string Country;
        public string Name;
        public string AttentionName;
        public string Phone;
    }
    public class Package
    {
        public string Weight;
        public UPS_PackagingType PackType;
    }
    public class ShippingForms
    {
        public static Ship _ShippingLabel = new Ship();
        public class ShippingForm : System.Windows.Forms.Form// shippingform
        {
            public ShippingForm()
            {

            }
        }
        public class Shipper : System.Windows.Forms.Form// shipperform
        {
            public Shipper()
            {

            }
        }
        public class ShipFrom : System.Windows.Forms.Form// shipfromform
        {
            public ShipFrom()
            {

            }
        }
        public class ShipTo : System.Windows.Forms.Form// shiptoform
        {
            public ShipTo()
            {

            }
        }
        public class Confirmation : System.Windows.Forms.Form// confirmationform
        {
            public Confirmation()
            {

            }
        }
    }
}
