
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShippingAPI.WebReferenceFreight;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;

namespace ShippingAPI
{

    public class Freight
    {

        [Serializable]
        public class RESPONSE
        {
            public FreightShipResponse Response;
            public string TrackingNumber;
            public Image Label;
            public string Price;
            public string Negotiated;
            public object Tag;
            public List<Exception> Exceptions = new List<Exception>();

            public string Confirmation { get; internal set; }
            public string DeliveryDate { get; internal set; }
            public string NextPickup { get; internal set; }
            public string TimeInTransit { get; internal set; }
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
            public ReturnEvent(Freight.RESPONSE obj = null)
            {
                if (obj != null)
                {
                    Response = obj;
                }
            }
            public Freight.RESPONSE Response { get; set; }
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
            try
            {
                EventHandler<ReturnEvent> handler = ReturnListener;
                if (handler != null)
                {
                    handler(this, obj);
                }
            }
            catch (Exception ex) { newException(new ExceptionOccured(ex)); }
        }
        public event EventHandler<ReturnEvent> ReturnListener;
        public static bool CheckValidationResult(object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        {

            return true;
        }
        private bool _testmode = true;
        public bool Testmode
        {
            get { return _testmode; }
            set { _testmode = value; }
        }
        public Freight()
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;

            Initialize();
        }
        public Freight(string access = "", string userid = "", string password = "", string acctNumber = "")
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
            Initialize(access, userid, password, acctNumber);
        }
        public Freight Initialize(string access = "", string userid = "", string password = "", string acctNumber = "")
        {
            try
            {
                this.upss = new UPSSecurity();
                this.upssSvcAccessToken = new UPSSecurityServiceAccessToken();
                this.upssSvcAccessToken.AccessLicenseNumber = access;
                this.upss.ServiceAccessToken = this.upssSvcAccessToken;
                this.upssUsrNameToken = new UPSSecurityUsernameToken();
                this.upssUsrNameToken.Username = userid;
                this.upssUsrNameToken.Password = password;
                this.upss.UsernameToken = this.upssUsrNameToken;

                this.freightShipService.UPSSecurityValue = upss;

                shipment.ShipperNumber = acctNumber;

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                new SoapExceptionOccured(ex);
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                newException(new ExceptionOccured(ex));

            }
            catch (Exception ex)
            {
                newException(new ExceptionOccured(ex));
            }
            return this;
        }
        public static readonly string TestUrl = "https://wwwcie.ups.com/webservices/Rate";
        public static readonly string ProductionUrl = "https://onlinetools.ups.com/webservices/Ship";
        public Freight ProcessFreight(Address Payer, Address From, Address To, FreightPackage package, ServiceCodes serviceCode, BillingCode Billing)
        {
            try
            {

                request.RequestOption = requestOption;
                freightShipRequest.Request = request;

                //Ship From
                shipFromAddress.AddressLine = From.AddressLine;
                shipFromAddress.City = From.City;
                shipFromAddress.StateProvinceCode = From.State;
                shipFromAddress.PostalCode = From.Postal;
                shipFromAddress.CountryCode = From.Country;
                shipFrom.Address = shipFromAddress;
                shipFrom.AttentionName = From.AttentionName;
                shipFrom.Name = From.Name;
                shipFromPhone.Number = From.Phone;
                shipFrom.Phone = shipFromPhone;
                shipFrom.EMailAddress = From.Email;
                shipment.ShipFrom = shipFrom;

               
                //Ship To
                shipToAddress.AddressLine = To.AddressLine;
                shipToAddress.City = To.City;
                shipToAddress.StateProvinceCode = To.State;
                shipToAddress.PostalCode = To.Postal;
                shipToAddress.CountryCode = To.Country;
                shipTo.Address = shipFromAddress;
                shipTo.AttentionName = To.AttentionName;
                shipTo.Name = To.Name;
                shipToPhone.Number = To.Phone;
                shipTo.Phone = shipToPhone;
                shipTo.EMailAddress = To.Email;
                shipment.ShipTo = shipTo;

                //PAYER
                payer.AttentionName = Payer.AttentionName;
                payer.Name = Payer.Name;
                payerPhone.Number = Payer.Phone;
                payer.Phone = payerPhone;
                payer.ShipperNumber = shipment.ShipperNumber;
                payer.EMailAddress = Payer.Email;
                payerAddress.AddressLine = payerAddressLines;
                payerAddress.City = Payer.City;
                payerAddress.StateProvinceCode = Payer.State;
                payerAddress.PostalCode = Payer.Postal;
                payerAddress.CountryCode = Payer.Country;
                payer.Address = payerAddress;

                //PAYMENT
                paymentInfo.Payer = payer;
                shipBillOption.Code =  Billing.ToString();
                shipBillOption.Description = Billing.ToString();
                paymentInfo.ShipmentBillingOption = shipBillOption;
                shipment.PaymentInformation = paymentInfo;

                //Freight Package
                service.Code = package.serviceCode;
                service.Description = package.serviceDescription;
                shipment.Service = service;
                commodity.NumberOfPieces = package.NumberOfPieces;
                nmfcCommodity.PrimeCode = package.PrimeCode;
                nmfcCommodity.SubCode = package.SubCode;
                commodity.NMFCCommodity = nmfcCommodity;
                commodity.FreightClass = package.FreightClass;
                packagingType.Code = package.packagingTypeCode;
                packagingType.Description = package.packagingTypeDescription;
                commodity.PackagingType = packagingType;
                weight.Value = package.Weight;
                unitOfMeasurement.Code = "lbs";
                unitOfMeasurement.Description = "pounds";
                weight.UnitOfMeasurement = unitOfMeasurement;
                commodity.Weight = weight;
                commodityValue.CurrencyCode = package.CurrencyCode;
                commodityValue.MonetaryValue = package.MonetaryValue;
                commodity.CommodityValue = commodityValue;
                commodity.Description = package.Description;
                CommodityType[] commodityArray = { commodity };
                shipment.Commodity = commodityArray;
                handlingUnit.Quantity = package.Quantity;
                handlingUnitType.Code = package.handlingUnitTypeCode;
                handlingUnitType.Description = package.handlingUnitTypeDescription;
                handlingUnit.Type = handlingUnitType;
                shipment.HandlingUnitOne = handlingUnit;

               
                freightShipRequest.Shipment = shipment;


                //process
                RESPONSE response = new RESPONSE();               

                freightShipResponse = freightShipService.ProcessShipment(freightShipRequest);
                response.Response = freightShipResponse;
                response.TrackingNumber = freightShipResponse.ShipmentResults.ShipmentNumber;
                response.Price = freightShipResponse.ShipmentResults.TotalShipmentCharge.MonetaryValue;
                response.Confirmation = freightShipResponse.ShipmentResults.PickupRequestConfirmationNumber;
                response.Negotiated = response.Price;
                response.DeliveryDate = freightShipResponse.ShipmentResults.DeliveryDate;
                response.NextPickup = freightShipResponse.ShipmentResults.NextAvailablePickupDate;
                response.TimeInTransit = freightShipResponse.ShipmentResults.TimeInTransit.DaysInTransit;
                ReturnReady(new ReturnEvent(response));
                Console.WriteLine("The transaction was a " + freightShipResponse.Response.ResponseStatus.Description);
                Console.WriteLine("The BOLID of the shipment is: " + freightShipResponse.ShipmentResults.BOLID);
                Console.WriteLine("The Shipment number of the shipment is " + freightShipResponse.ShipmentResults.ShipmentNumber);

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                newSoapException(new SoapExceptionOccured(ex));
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                newException(new ExceptionOccured(ex));
            }
            catch (Exception ex)
            {
                newException(new ExceptionOccured(ex));
            }

            return this;
        }
        public void ProcessFreightAsync(Address Payer, Address From, Address To, FreightPackage package, ServiceCodes serviceCode)
        {
            try
            {

                request.RequestOption = requestOption;
                freightShipRequest.Request = request;

                //Ship From
                shipFromAddress.AddressLine = From.AddressLine;
                shipFromAddress.City = From.City;
                shipFromAddress.StateProvinceCode = From.State;
                shipFromAddress.PostalCode = From.Postal;
                shipFromAddress.CountryCode = From.Country;
                shipFrom.Address = shipFromAddress;
                shipFrom.AttentionName = From.AttentionName;
                shipFrom.Name = From.Name;
                shipFromPhone.Number = From.Phone;
                shipFrom.Phone = shipFromPhone;
                shipFrom.EMailAddress = From.Email;
                shipment.ShipFrom = shipFrom;


                //Ship To
                shipToAddress.AddressLine = To.AddressLine;
                shipToAddress.City = To.City;
                shipToAddress.StateProvinceCode = To.State;
                shipToAddress.PostalCode = To.Postal;
                shipToAddress.CountryCode = To.Country;
                shipTo.Address = shipFromAddress;
                shipTo.AttentionName = To.AttentionName;
                shipTo.Name = To.Name;
                shipToPhone.Number = To.Phone;
                shipTo.Phone = shipToPhone;
                shipTo.EMailAddress = To.Email;
                shipment.ShipTo = shipTo;

                //PAYER
                payer.AttentionName = Payer.AttentionName;
                payer.Name = Payer.Name;
                payerPhone.Number = Payer.Phone;
                payer.Phone = payerPhone;
                payer.ShipperNumber = shipment.ShipperNumber;
                payer.EMailAddress = Payer.Email;
                payerAddress.AddressLine = payerAddressLines;
                payerAddress.City = Payer.City;
                payerAddress.StateProvinceCode = Payer.State;
                payerAddress.PostalCode = Payer.Postal;
                payerAddress.CountryCode = Payer.Country;
                payer.Address = payerAddress;

                //PAYMENT
                paymentInfo.Payer = payer;
                shipBillOption.Code = BillingCode.PrePaid.ToString();
                shipBillOption.Description = "PREPAID";
                paymentInfo.ShipmentBillingOption = shipBillOption;
                shipment.PaymentInformation = paymentInfo;

                //Freight Package
                service.Code = package.serviceCode;
                service.Description = package.serviceDescription;
                shipment.Service = service;
                commodity.NumberOfPieces = package.NumberOfPieces;
                nmfcCommodity.PrimeCode = package.PrimeCode;
                nmfcCommodity.SubCode = package.SubCode;
                commodity.NMFCCommodity = nmfcCommodity;
                commodity.FreightClass = package.FreightClass;
                packagingType.Code = package.packagingTypeCode;
                packagingType.Description = package.packagingTypeDescription;
                commodity.PackagingType = packagingType;
                weight.Value = package.Weight;
                unitOfMeasurement.Code = "lbs";
                unitOfMeasurement.Description = "pounds";
                weight.UnitOfMeasurement = unitOfMeasurement;
                commodity.Weight = weight;
                commodityValue.CurrencyCode = package.CurrencyCode;
                commodityValue.MonetaryValue = package.MonetaryValue;
                commodity.CommodityValue = commodityValue;
                commodity.Description = package.Description;
                CommodityType[] commodityArray = { commodity };
                shipment.Commodity = commodityArray;
                handlingUnit.Quantity = package.Quantity;
                handlingUnitType.Code = package.handlingUnitTypeCode;
                handlingUnitType.Description = package.handlingUnitTypeDescription;
                handlingUnit.Type = handlingUnitType;
                shipment.HandlingUnitOne = handlingUnit;



                freightShipRequest.Shipment = shipment;


                //process
                freightShipService.ProcessShipmentCompleted += FreightShipService_ProcessShipmentCompleted;
                freightShipService.ProcessShipmentAsync(freightShipRequest);
                Console.WriteLine("The transaction was a " + freightShipResponse.Response.ResponseStatus.Description);
                Console.WriteLine("The BOLID of the shipment is: " + freightShipResponse.ShipmentResults.BOLID);
                Console.WriteLine("The Shipment number of the shipment is " + freightShipResponse.ShipmentResults.ShipmentNumber);

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                newSoapException(new SoapExceptionOccured(ex));
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                newException(new ExceptionOccured(ex));
            }
            catch (Exception ex)
            {
                newException(new ExceptionOccured(ex));
            }
            
        }

        private void FreightShipService_ProcessShipmentCompleted(object sender, ProcessShipmentCompletedEventArgs e)
        {
            try
            {
                RESPONSE response = new RESPONSE();
                freightShipResponse = e.Result;
                response.Response = freightShipResponse;
                response.TrackingNumber = freightShipResponse.ShipmentResults.ShipmentNumber;
                response.Price = freightShipResponse.ShipmentResults.TotalShipmentCharge.MonetaryValue;
                response.Confirmation = freightShipResponse.ShipmentResults.PickupRequestConfirmationNumber;
                response.Negotiated = response.Price;
                response.DeliveryDate = freightShipResponse.ShipmentResults.DeliveryDate;
                response.NextPickup = freightShipResponse.ShipmentResults.NextAvailablePickupDate;
                response.TimeInTransit = freightShipResponse.ShipmentResults.TimeInTransit.DaysInTransit;
                ReturnReady(new ReturnEvent(response));
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                newSoapException(new SoapExceptionOccured(ex));
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                newException(new ExceptionOccured(ex));
            }
            catch (Exception ex)
            {
                newException(new ExceptionOccured(ex));
            }
        }

        FreightShipService freightShipService = new FreightShipService();
        FreightShipRequest freightShipRequest = new FreightShipRequest();
        RequestType request = new RequestType();
        String[] requestOption = { "1" };
        ShipmentType shipment = new ShipmentType();
        ShipFromType shipFrom = new ShipFromType();
        FreightShipAddressType shipFromAddress = new FreightShipAddressType();
        String[] shipFromAddressLines = { "ShipFrom address line" };
        FreightShipPhoneType shipFromPhone = new FreightShipPhoneType();
        ShipToType shipTo = new ShipToType();
        FreightShipAddressType shipToAddress = new FreightShipAddressType();
        String[] shipToAddressLines = { "ShipTo address line" };
        FreightShipPhoneType shipToPhone = new FreightShipPhoneType();
        PaymentInformationType paymentInfo = new PaymentInformationType();
        PayerType payer = new PayerType();
        FreightShipPhoneType payerPhone = new FreightShipPhoneType();
        FreightShipAddressType payerAddress = new FreightShipAddressType();
        String[] payerAddressLines = { "Payer address line" };
        ShipCodeDescriptionType shipBillOption = new ShipCodeDescriptionType();
        ShipCodeDescriptionType service = new ShipCodeDescriptionType();
        CommodityType commodity = new CommodityType();
        NMFCCommodityType nmfcCommodity = new NMFCCommodityType();
        ShipCodeDescriptionType packagingType = new ShipCodeDescriptionType();
        WeightType weight = new WeightType();
        FreightShipUnitOfMeasurementType unitOfMeasurement = new FreightShipUnitOfMeasurementType();
        CommodityValueType commodityValue = new CommodityValueType();
        HandlingUnitType handlingUnit = new HandlingUnitType();
        ShipCodeDescriptionType handlingUnitType = new ShipCodeDescriptionType();
        UPSSecurity upss = new UPSSecurity();
        UPSSecurityServiceAccessToken upssSvcAccessToken = new UPSSecurityServiceAccessToken();
        UPSSecurityUsernameToken upssUsrNameToken = new UPSSecurityUsernameToken();
        FreightShipResponse freightShipResponse = new FreightShipResponse();
    }

    public class FreightPackage
    {
        public FreightPackage()
        { }
        public FreightPackage(string numberOfPieces, string weight,PackagingType packageType, HandlingUnit handlingUnit, ServiceCodes ServiceCode)
        {
            NumberOfPieces = numberOfPieces;
            Weight = weight;
            packagingTypeCode = packageType.Value;
            packagingTypeDescription = packageType.Value;
            handlingUnitTypeCode = handlingUnit.Value;
            handlingUnitTypeDescription = handlingUnit.Value;
            serviceCode = ServiceCode.ToString();
        }

        public string serviceCode = ServiceCodes.LTL_Guaranteed.ToString();
        public string serviceDescription = "UPS Ground Freight";
        public string NumberOfPieces = "1";
        public string PrimeCode = "160300";
        public string SubCode = "01";
        public string FreightClass = "92.5"; // ??
        public string packagingTypeCode = PackagingType.Box.Value;
        public string packagingTypeDescription = PackagingType.Box.Value;
        public string Weight = "155";
        public string unitOfMeasurementCode = "lbs";
        public string unitOfMeasurementDescription = "pounds";
        public string CurrencyCode = "USD";
        public string MonetaryValue = "100";
        public string Description = "Package";
        public string Quantity = "1";
        public string handlingUnitTypeCode = HandlingUnit.Skid.Value;
        public string handlingUnitTypeDescription = HandlingUnit.Skid.Value;
    }
    public enum BillingCode
    {
        PrePaid=10,ThirdParty=30,FreightCollect=40
    }
    public enum ServiceCodes
    {
        LTL=308,LTL_Guaranteed=309, LTL_Guaranteed_AM=334, Standard_LTL=349
    }
    public class PackagingType
    {
        private PackagingType(string value) { Value = value; }

        public string Value { get; set; }

        public static PackagingType Bag { get { return new PackagingType("BAG"); } }
        public static PackagingType Bale { get { return new PackagingType("BAL"); } }
        public static PackagingType Barrel { get { return new PackagingType("BAR"); } }
        public static PackagingType Bundle { get { return new PackagingType("BDL"); } }
        public static PackagingType Bin { get { return new PackagingType("BIN"); } }
        public static PackagingType Box { get { return new PackagingType("Box"); } }
        public static PackagingType Basket { get { return new PackagingType("BSK"); } }
        public static PackagingType Container { get { return new PackagingType("CON"); } }
        public static PackagingType Loose { get { return new PackagingType("LOO"); } }
        public static PackagingType Other { get { return new PackagingType("OTH"); } }
        public static PackagingType Pieces { get { return new PackagingType("PCS"); } }
        public static PackagingType Package { get { return new PackagingType("PKG"); } }
        public static PackagingType Pallet { get { return new PackagingType("PLT"); } }
        public static PackagingType Rack { get { return new PackagingType("RCK"); } }
        public static PackagingType Roll { get { return new PackagingType("ROL"); } }
        public static PackagingType Skid { get { return new PackagingType("SKD"); } }
        public static PackagingType Tube { get { return new PackagingType("TBE"); } }
        public static PackagingType Tank { get { return new PackagingType("TNK"); } }
        public static PackagingType Unit { get { return new PackagingType("UNT"); } }
        public static PackagingType VanPack { get { return new PackagingType("VPK"); } }
        public static PackagingType Wrapped { get { return new PackagingType("WRP"); } }

    }
    public class HandlingUnit
    {
        private HandlingUnit(string value) { Value = value; }

        public string Value { get; set; }

        public static HandlingUnit Skid { get { return new HandlingUnit("SKD"); } }
        public static HandlingUnit CarBoy { get { return new HandlingUnit("CBY"); } }
        public static HandlingUnit Pallet { get { return new HandlingUnit("PLT"); } }
        public static HandlingUnit Totes { get { return new HandlingUnit("TOT"); } }
        public static HandlingUnit Loose { get { return new HandlingUnit("LOO"); } }
        public static HandlingUnit Other { get { return new HandlingUnit("OTH"); } }


    }

}
