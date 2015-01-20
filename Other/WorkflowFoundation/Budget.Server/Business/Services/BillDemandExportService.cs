using System;
using System.Globalization;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using Budget2.Server.Business.Interface;
using Budget2.Server.Business.Interface.DataContracts;
using Budget2.Server.Business.Interface.Services;
using Common;
using ExternalServices;
using System.Linq;

namespace Budget2.Server.Business.Services
{
    public class BillDemandExportService : IBillDemandExportService
    {
        public const int FirmId = 3;
        public const string TypeDoc = "11020";

        public void ExportBillDemand(BillDemandForExport billDemandForExport)
        {
           
            using (var client = new IccBossNewDocExport_IccBossNewDocHttpService())
            {
                createDoc doc = new createDoc()
                                    {
                                        request = new BossNewDocRequest()
                                                      {
                                                          idPaydoc = billDemandForExport.Id.ToString("N"),
                                                          basePayDocIds =
                                                              billDemandForExport.PaymentPlanItemIds.Select(
                                                                  p => p.ToString(CultureInfo.InvariantCulture)).ToArray
                                                              (),
                                                          currCode = billDemandForExport.CurrencyCode,
                                                          currCourseDate = billDemandForExport.CurrencyRevisionDate,
                                                          currCourseDateSpecified = true,
                                                          custIdFrom =
                                                              billDemandForExport.CustomerCounteragentId.ToString(
                                                                  CultureInfo.InvariantCulture),
                                                          custIdTo =
                                                              billDemandForExport.CounteragentId.ToString(
                                                                  CultureInfo.InvariantCulture),
                                                          docDate =
                                                              billDemandForExport.DocumentDate,
                                                          docDateSpecified = true,
                                                          rDocDate = string.Format("{0:yyyy-MM-dd}",billDemandForExport.DocumentDate),
                                                          sDocStringNum = billDemandForExport.Number,
                                                          idFirm = billDemandForExport.FirmCode,
                                                          docStringNum = billDemandForExport.IdNumber.ToString(CultureInfo.InvariantCulture),
                                                          rDocStringNum = billDemandForExport.IdNumber.ToString(CultureInfo.InvariantCulture),
                                                          rootDocId =
                                                              billDemandForExport.ContractId.ToString(
                                                                  CultureInfo.InvariantCulture),
                                                          typeDoc = TypeDoc,
                                                          taxString =
                                                              billDemandForExport.NDSTaxValue.ToString("0.00",
                                                                                                       CultureInfo.
                                                                                                           CreateSpecificCulture
                                                                                                           ("en-US")),
                                                          docSum =
                                                              billDemandForExport.SumWIthNDS.ToString("0.00",
                                                                                                      CultureInfo.
                                                                                                          CreateSpecificCulture
                                                                                                          ("en-US")),
                                                          rUserSmeta = billDemandForExport.BudgetPartId.ToString(CultureInfo.InvariantCulture),
                                                          rUserExpenses = billDemandForExport.SmetaCode,
                                                          rUserCfuExpenses = billDemandForExport.OPCode,
                                                          rUserCfuPotreb = billDemandForExport.PPCode,
                                                          rUserProject = billDemandForExport.ProjectCode

                                                      }
                                    };

                if (billDemandForExport.AccountDate.HasValue)
                {
                    doc.request.sDocDate = billDemandForExport.AccountDate.Value;
                    doc.request.sDocDateSpecified = true;
                }
                else
                {
                    doc.request.sDocDateSpecified = false;
                }

#if DEBUG
                return;
               // throw new InvalidOperationException("sad");
#else
                try
                {
                    var response = client.createDoc(doc);
                    if (response.response.respCode != 0)
                        throw new InvalidOperationException(
                            string.Format("Не удалось выгрузить документ BillDemandId = {0} Описание ошибки = {1}",
                                          billDemandForExport.Id, response.response.errDesc));
                }
                catch (SoapException ex)
                {
                    Logger.Log.Error(string.Format("Не удалось выгрузить документ BillDemandId = {0} Описание ошибки = {1}", billDemandForExport.Id,ex.Detail.InnerText));
                    Logger.Log.Error(string.Format("Не удалось выгрузить документ BillDemandId = {0} Описание ошибки = {1}", billDemandForExport.Id, ex.Message));
                    string[] errorMessages = ex.Detail.InnerText.Split(']');
                    string errorMessage = errorMessages[errorMessages.Length - 1];

                    throw new InvalidOperationException(
                        string.Format("Не удалось выгрузить документ BillDemandId = {0} Описание ошибки = {1}", billDemandForExport.Id, string.IsNullOrEmpty(errorMessage) ? "Не определено" : errorMessage
                                      ));
                }
                  

#endif
            }
        }

        public BillDemandExternalState GetBillDemandExternalStaus(Guid billDemandUid)
        {
            using (var client = new IccCftGetDocStatusService())
            {
                var request = new getDocStatus()
                                  {
                                      getDocStatusRequest = new DocStatusRequest()
                                                                {
                                                                    docNum = billDemandUid.ToString("N")
                                                                }
                                  };

                DocStatusResponse response = null;
#if DEBUG
                response = new DocStatusResponse()
                               {
                                   status = "4",
                                   payDocDate = DateTime.Now,
                                   payDocNum = "A123",
                                   descr = "Все плохо"

                               };
#else
                try
                {
                    response = client.getDocStatus(request).getDocStatusResponse1;
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("Ошибка вызова сервиса проверки статуса РД",ex);
                    throw;
                }
#endif

                var status =
                    BillDemandExternalStatus.All.FirstOrDefault(
                        p => p.Code == int.Parse(response.status, CultureInfo.InvariantCulture));

                if (status == null)
                    status = BillDemandExternalStatus.Unknown;

                return new BillDemandExternalState
                           {
                               DocumentNumber = response.payDocNum,
                               PaymentDate = response.payDocDate,
                               Status = status,
                               Description = response.descr

                           };
            }
        }
    }
}
