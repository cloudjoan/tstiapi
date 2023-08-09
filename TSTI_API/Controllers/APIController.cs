#region 更新歷程
/*
注意：若要更新正式區，請將搜尋「【測試】」，將它註解，並反註解「【正式】」，webconfig記得也要調整


*/
#endregion

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Data;
using SAP.Middleware.Connector;
using System.Web.Mvc;
using TSTI_API.Models;
using System.Data.Entity.Validation;

namespace TSTI_API.Controllers
{
    #region API Key，上【正式】再打開
    [ApiFilter] 
    #endregion
    public class APIController : Controller
    {
        TESTEntities testDB = new TESTEntities();
        TSTIONEEntities dbOne = new TSTIONEEntities();
        ERP_PROXY_DBEntities dbProxy = new ERP_PROXY_DBEntities();
        APP_DATAEntities appDB = new APP_DATAEntities();
        MCSWorkflowEntities dbEIP = new MCSWorkflowEntities();

        CommonFunction CMF = new CommonFunction();

        RfcDestination sapConnector;
        RfcDestination sapTatungConnector;

        /// <summary>
        /// 程式作業編號檔系統ID(ALL，固定的GUID)
        /// </summary>
        string pSysOperationID = "F8EFC55F-FA77-4731-BB45-2F2147244A2D";

        /// <summary>
        /// 程式作業編號檔系統ID(一般服務)
        /// </summary>
        string pOperationID_GenerallySR = "869FC989-1049-4266-ABDE-69A9B07BCD0A";

        /// <summary>
        /// 程式作業編號檔系統ID(裝機服務)
        /// </summary>
        string pOperationID_InstallSR = "3B6FF77B-DAF4-4C2D-957A-6C28CE054D75";

        /// <summary>
        /// 程式作業編號檔系統ID(定維服務)
        /// </summary>
        string pOperationID_MaintainSR = "5B80D6AB-9143-4916-9273-ADFAEA9A61ED";

        /// <summary>
        /// 程式作業編號檔系統ID(合約主數據查詢/維護)
        /// </summary>
        string pOperationID_Contract = "A9556C2C-E5DE-4745-A76B-5F2E1F69A3A9";

        static string API_KEY = "6xdTlREsMbFd0dBT28jhb5W3BNukgLOos";


        /// <summary>全域變數</summary>
        string pMsg = "";

        #region 範例API
        // GET: API
        [HttpGet]
        public ActionResult Index()
        {
            return Json("Hello World!", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetData()
        {
            return Json(testDB.TB_MVC_CUST, JsonRequestBehavior.AllowGet);
        }

        #region 測試call API範例程式
        //[HttpPost]
        //public ActionResult callTestAPI_GENERALSR_CREATE()
        //{
        //    return Json(callTestToGENERALSR_CREATE());
        //}
        #endregion

        [HttpPost]
        public ActionResult SaveData(TB_MVC_CUST bean)
        {

            testDB.TB_MVC_CUST.Add(bean);
            var result = testDB.SaveChanges();

            if (result == 1)
            {
                return Json("Finsih");
            }
            else
            {
                return Json("Fail");
            }

            //if (Request.Headers["X-MBX-APIKEY"] == API_KEY)
            //{
            //    testDB.TB_MVC_CUST.Add(bean);
            //    testDB.SaveChanges();

            //    return Json(bean);
            //}
            //else
            //{
            //    return Json("Fail!");
            //}

        }

        [HttpPost]
        public ActionResult SaveDatas(List<TB_MVC_CUST> beans)
        {

            //testDB.TB_MVC_CUST.Add(beans);
            var result = testDB.SaveChanges();

            if (result > 0)
            {
                return Json("Finsih");
            }
            else
            {
                return Json("Fail");
            }

        }

        [HttpPost]
        public ActionResult TestGetFormData(FormCollection data)
        {
            return Json(string.Format("{0} Email:{1}", data["userNames"] ?? "逢金", data["email"] ?? "沒有"));
        }
        #endregion

        #region -----↓↓↓↓↓一般服務案件建立 ↓↓↓↓↓-----       

        #region 建立ONE SERVICE報修SR（一般服務案件）接口
        /// <summary>
        /// 建立ONE SERVICE報修SR（一般服務案件）接口
        /// </summary>
        /// <param name="bean"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_GENERALSR_CREATE(SRMain_GENERALSR_INPUT beanIN)
        {
            #region Json範列格式，一筆(建立GENERALSR_CREATEByAPI)
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //     "IV_CUSTOMER": "D03251108",
            //     "IV_SRTEAM": "SRV.12100000",
            //     "IV_RKIND": "Z01",
            //     "IV_PATHWAY": "Z01",
            //     "IV_DESC": "Test20230106",
            //     "IV_LTXT": "Test20230106詳細說明",
            //     "IV_MKIND1": "ZA01",
            //     "IV_MKIND2": "ZB0101",
            //     "IV_MKIND3": "ZC010101",
            //     "IV_REPAIRNAME": "王炯凱",
            //     "IV_REPAIRTEL": "02-2506-2121#1239",
            //     "IV_REPAIRMOB": "0909000000",
            //     "IV_REPAIRADDR": "台北市中山區松江路121號13樓",
            //     "IV_REPAIREMAIL": "elvis.chang@etatung.com",            
            //     "IV_ASSIGN": "L2",
            //     "IV_EMPNO": "10001567",
            //     "IV_SQEMPID": "ZC103",
            //     "IV_SERIAL": "SGH33223R6",
            //     "IV_SNPID": "G-654081B21-057",
            //     "IV_PIDNAME": "DL360pG8 E5-2650/8G*2/146G15K*2/D/R/IC-E",
            //     "IV_PN": "654081-B21-057",
            //     "IV_WTY": "OM363636",
            //     "IV_REFIX": "N",
            //     "IV_INTERNALWORK" : "N",
            //     "IV_ATTACHFiles" : "檔案",
            //     "CREATECONTACT_LIST": [
            //        {
            //        "SRID": "612211250004",
            //            "CONTNAME": "賴淑瑛",
            //            "CONTADDR": "台北市信義區菸廠路88號12樓",
            //            "CONTTEL": "(02)6638-6888#13158",
            //            "CONTMOBILE": "",
            //            "CONTEMAIL": "elvis.chang@etatung.com"
            //        },
            //        {
            //        "SRID": "612211250004",
            //            "CONTNAME": "廖勇翔",
            //            "CONTADDR": "台北市信義區菸廠路88號12樓",
            //            "CONTTEL": "02-6638-6888#13124",
            //            "CONTMOBILE": "",
            //            "CONTEMAIL": "elvis.chang@etatung.com"
            //        }                 
            //    ]
            //}
            #endregion

            SRMain_GENERALSR_OUTPUT SROUT = new SRMain_GENERALSR_OUTPUT();

            SROUT = SaveGenerallySR(beanIN, "ADD"); //新增

            return Json(SROUT);
        }
        #endregion

        #region 儲存一般服務案件
        /// <summary>
        /// 儲存一般服務案件
        /// </summary>
        /// <param name="bean">一般服務案件主檔資訊</param>
        /// <param name="tType">ADD.新增 EDIT.修改</param>
        /// <returns></returns>
        private SRMain_GENERALSR_OUTPUT SaveGenerallySR(SRMain_GENERALSR_INPUT bean, string tType)
        {
            SRMain_GENERALSR_OUTPUT SROUT = new SRMain_GENERALSR_OUTPUT();

            bool tIsFormal = false;
            string pLoginName = string.Empty;
            string pBUKRS = string.Empty;
            string pSRID = string.Empty;
            string OldCStatus = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string tAttachPath = string.Empty;

            string IV_LOGINEMPNO = string.IsNullOrEmpty(bean.IV_LOGINEMPNO) ? "" : bean.IV_LOGINEMPNO.Trim();
            string IV_CUSTOMER = string.IsNullOrEmpty(bean.IV_CUSTOMER) ? "" : bean.IV_CUSTOMER.Trim();
            string IV_SRTEAM = string.IsNullOrEmpty(bean.IV_SRTEAM) ? "" : bean.IV_SRTEAM.Trim();
            string IV_RKIND = string.IsNullOrEmpty(bean.IV_RKIND) ? "" : bean.IV_RKIND.Trim();
            string IV_PATHWAY = string.IsNullOrEmpty(bean.IV_PATHWAY) ? "" : bean.IV_PATHWAY.Trim();
            string IV_DESC = string.IsNullOrEmpty(bean.IV_DESC) ? "" : bean.IV_DESC.Trim();
            string IV_LTXT = string.IsNullOrEmpty(bean.IV_LTXT) ? "" : bean.IV_LTXT.Trim();
            string IV_MKIND1 = string.IsNullOrEmpty(bean.IV_MKIND1) ? "" : bean.IV_MKIND1.Trim();
            string IV_MKIND2 = string.IsNullOrEmpty(bean.IV_MKIND2) ? "" : bean.IV_MKIND2.Trim();
            string IV_MKIND3 = string.IsNullOrEmpty(bean.IV_MKIND3) ? "" : bean.IV_MKIND3.Trim();
            string IV_REPAIRNAME = string.IsNullOrEmpty(bean.IV_REPAIRNAME) ? "" : bean.IV_REPAIRNAME.Trim();
            string IV_REPAIRTEL = string.IsNullOrEmpty(bean.IV_REPAIRTEL) ? "" : bean.IV_REPAIRTEL.Trim();
            string IV_REPAIRMOB = string.IsNullOrEmpty(bean.IV_REPAIRMOB) ? "" : bean.IV_REPAIRMOB.Trim();
            string IV_REPAIRADDR = string.IsNullOrEmpty(bean.IV_REPAIRADDR) ? "" : bean.IV_REPAIRADDR.Trim();
            string IV_REPAIREMAIL = string.IsNullOrEmpty(bean.IV_REPAIREMAIL) ? "" : bean.IV_REPAIREMAIL.Trim();
            string IV_ASSIGN = string.IsNullOrEmpty(bean.IV_ASSIGN) ? "" : bean.IV_ASSIGN.Trim();
            string IV_EMPNO = string.IsNullOrEmpty(bean.IV_EMPNO) ? "" : bean.IV_EMPNO.Trim();
            string IV_SQEMPID = string.IsNullOrEmpty(bean.IV_SQEMPID) ? "" : bean.IV_SQEMPID.Trim();
            string IV_SERIAL = string.IsNullOrEmpty(bean.IV_SERIAL) ? "" : bean.IV_SERIAL.Trim();
            string IV_SNPID = string.IsNullOrEmpty(bean.IV_SNPID) ? "" : bean.IV_SNPID.Trim();
            string IV_PIDNAME = string.IsNullOrEmpty(bean.IV_PIDNAME) ? "" : bean.IV_PIDNAME.Trim();
            string IV_PN = string.IsNullOrEmpty(bean.IV_PN) ? "" : bean.IV_PN.Trim();
            string IV_WTY = string.IsNullOrEmpty(bean.IV_WTY) ? "" : bean.IV_WTY.Trim();
            string IV_REFIX = string.IsNullOrEmpty(bean.IV_REFIX) ? "" : bean.IV_REFIX.Trim();
            string IV_INTERNALWORK = string.IsNullOrEmpty(bean.IV_INTERNALWORK) ? "N" : bean.IV_INTERNALWORK.Trim();
            HttpPostedFileBase[] AttachFiles = bean.IV_ATTACHFiles;

            string CCustomerName = CMF.findCustName(IV_CUSTOMER);
            string CSqpersonName = CMF.findSQPersonName(IV_SQEMPID);
            string CMainEngineerName = CMF.findEmployeeName(IV_EMPNO);

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
                pBUKRS = "T012";
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                pBUKRS = EmpBean.BUKRS;
            }

            #region 判斷必填欄位
            if (string.IsNullOrEmpty(IV_LOGINEMPNO))
            {
                pMsg += "【登入者員工編號】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_CUSTOMER))
            {
                pMsg += "【客戶ID】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_SRTEAM))
            {
                pMsg += "【服務團隊ID】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_RKIND))
            {
                pMsg += "【維護服務種類】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_PATHWAY))
            {
                pMsg += "【報修管道】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_DESC))
            {
                pMsg += "【服務說明】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_LTXT))
            {
                pMsg += "【詳細描述】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_REPAIRNAME))
            {
                pMsg += "【報修人姓名】不得為空！" + Environment.NewLine;
            }           
            #endregion

            if (pMsg != "")
            {
                pMsg = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + pMsg;
                CMF.writeToLog(pSRID, "SaveGenerallySR_API", pMsg, pLoginName);

                SROUT.EV_SRID = "";
                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = pMsg;
            }
            else
            {
                #region 執行寫入
                try
                {
                    #region 取得系統位址參數相關資訊
                    SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

                    tIsFormal = ParaBean.IsFormal;

                    tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
                    tONEURLName = ParaBean.ONEURLName;
                    tBPMURLName = ParaBean.BPMURLName;
                    tPSIPURLName = ParaBean.PSIPURLName;
                    tAttachURLName = ParaBean.AttachURLName;
                    tAttachPath = Server.MapPath("~/REPORT");
                    #endregion

                    if (tType == "ADD")
                    {
                        if (IV_ASSIGN == "L2")
                        {
                            IV_ASSIGN = "E0002";
                        }
                        else
                        {
                            IV_ASSIGN = "E0005";
                        }

                        #region 判斷是否要自動帶入報修人資訊
                        if (string.IsNullOrEmpty(IV_REPAIRADDR))
                        {
                            List<PCustomerContact> ConList = CMF.findCONTACTINFO(pBUKRS, IV_CUSTOMER, IV_REPAIRNAME, "", "");

                            if (ConList.Count > 0)
                            {
                                foreach (var beanCon in ConList)
                                {
                                    IV_REPAIRADDR = beanCon.City + beanCon.Address;
                                    IV_REPAIRTEL = beanCon.Phone;
                                    IV_REPAIRMOB = beanCon.Mobile;
                                    IV_REPAIREMAIL = beanCon.Email;

                                    break;
                                }
                            }
                            else
                            {
                                IV_REPAIRADDR = IV_REPAIRNAME;
                            }
                        }                      
                        #endregion

                        #region 新增主檔
                        TB_ONE_SRMain beanM = new TB_ONE_SRMain();

                        pSRID = GetSRID("61");

                        //主表資料
                        beanM.cSRID = pSRID;
                        beanM.cStatus = IV_EMPNO != "" ? IV_ASSIGN : "E0001";    //新增時若有主要工程師，則預設為(L2或L3.處理中)，反之則預設為新建
                        beanM.cCustomerName = CCustomerName;
                        beanM.cCustomerID = IV_CUSTOMER;
                        beanM.cDesc = IV_DESC;
                        beanM.cNotes = IV_LTXT;
                        beanM.cAttachement = "";
                        beanM.cAttachementStockNo = "";
                        beanM.cMAServiceType = IV_RKIND;
                        beanM.cSRTypeOne = IV_MKIND1;
                        beanM.cSRTypeSec = IV_MKIND2;
                        beanM.cSRTypeThr = IV_MKIND3;
                        beanM.cSRPathWay = IV_PATHWAY;
                        beanM.cSRProcessWay = "";
                        beanM.cDelayReason = "";
                        beanM.cIsSecondFix = IV_REFIX;
                        beanM.cRepairName = IV_REPAIRNAME;
                        beanM.cRepairAddress = IV_REPAIRADDR;
                        beanM.cRepairPhone = IV_REPAIRTEL;
                        beanM.cRepairMobile = IV_REPAIRMOB;
                        beanM.cRepairEmail = IV_REPAIREMAIL;
                        beanM.cTeamID = IV_SRTEAM;
                        beanM.cSQPersonID = IV_SQEMPID;
                        beanM.cSQPersonName = CSqpersonName;
                        beanM.cSalesName = "";
                        beanM.cSalesID = "";
                        beanM.cMainEngineerName = CMainEngineerName;
                        beanM.cMainEngineerID = IV_EMPNO;
                        beanM.cAssEngineerID = "";
                        beanM.cTechManagerID = "";
                        beanM.cSystemGUID = Guid.NewGuid();
                        beanM.cIsAPPClose = "";
                        beanM.cIsInternalWork = IV_INTERNALWORK;

                        if (AttachFiles != null)
                        {
                            #region 檢附文件
                            if (AttachFiles.Length > 0)
                            {
                                Guid fileGuid = Guid.NewGuid();

                                string cAttachementID = string.Empty;
                                string path = string.Empty;
                                string fileId = string.Empty;
                                string fileOrgName = string.Empty;
                                string fileName = string.Empty;
                                string fileALLName = string.Empty;

                                foreach (var Attach in AttachFiles)
                                {
                                    if (Attach != null)
                                    {
                                        #region 檔案部份
                                        fileGuid = Guid.NewGuid();

                                        cAttachementID += fileGuid.ToString() + ",";

                                        fileId = fileGuid.ToString();
                                        fileOrgName = Attach.FileName;
                                        fileName = fileId + Path.GetExtension(Attach.FileName);
                                        path = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                                        Attach.SaveAs(path);
                                        #endregion

                                        #region table部份                                        
                                        TB_ONE_DOCUMENT beanDoc = new TB_ONE_DOCUMENT();

                                        beanDoc.ID = fileGuid;
                                        beanDoc.FILE_ORG_NAME = fileOrgName;
                                        beanDoc.FILE_NAME = fileName;
                                        beanDoc.FILE_EXT = Path.GetExtension(Attach.FileName);
                                        beanDoc.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                        dbOne.TB_ONE_DOCUMENT.Add(beanDoc);
                                        dbOne.SaveChanges();

                                        fileALLName += fileName + ",";
                                        #endregion
                                    }
                                }

                                beanM.cAttachement = cAttachementID;
                            }
                            #endregion
                        }

                        beanM.CreatedDate = DateTime.Now;
                        beanM.CreatedUserName = pLoginName;

                        dbOne.TB_ONE_SRMain.Add(beanM);
                        #endregion

                        #region 新增【客戶聯絡人資訊】明細
                        if (bean.CREATECONTACT_LIST != null)
                        {
                            foreach (var beanCon in bean.CREATECONTACT_LIST)
                            {
                                string IV_CONTNAME = string.IsNullOrEmpty(beanCon.CONTNAME) ? "" : beanCon.CONTNAME.Trim();
                                string IV_CONTADDR = string.IsNullOrEmpty(beanCon.CONTADDR) ? "" : beanCon.CONTADDR.Trim();
                                string IV_CONTTEL = string.IsNullOrEmpty(beanCon.CONTTEL) ? "" : beanCon.CONTTEL.Trim();
                                string IV_CONTMOBILE = string.IsNullOrEmpty(beanCon.CONTMOBILE) ? "" : beanCon.CONTMOBILE.Trim();
                                string IV_CONTEMAIL = string.IsNullOrEmpty(beanCon.CONTEMAIL) ? "" : beanCon.CONTEMAIL.Trim();

                                TB_ONE_SRDetail_Contact beanD = new TB_ONE_SRDetail_Contact();

                                beanD.cSRID = pSRID;
                                beanD.cContactName = IV_CONTNAME;
                                beanD.cContactAddress = IV_CONTADDR;
                                beanD.cContactPhone = IV_CONTTEL;
                                beanD.cContactMobile = IV_CONTMOBILE;
                                beanD.cContactEmail = IV_CONTEMAIL;
                                beanD.Disabled = 0;

                                beanD.CreatedDate = DateTime.Now;
                                beanD.CreatedUserName = pLoginName;

                                dbOne.TB_ONE_SRDetail_Contact.Add(beanD);
                            }
                        }
                        else
                        {
                            #region 若沒有傳入就預設同報修人
                            TB_ONE_SRDetail_Contact beanD = new TB_ONE_SRDetail_Contact();

                            beanD.cSRID = pSRID;
                            beanD.cContactName = IV_REPAIRNAME;
                            beanD.cContactAddress = IV_REPAIRADDR;
                            beanD.cContactPhone = IV_REPAIRTEL;
                            beanD.cContactMobile = IV_REPAIRMOB;
                            beanD.cContactEmail = IV_REPAIREMAIL;
                            beanD.Disabled = 0;

                            beanD.CreatedDate = DateTime.Now;
                            beanD.CreatedUserName = pLoginName;

                            dbOne.TB_ONE_SRDetail_Contact.Add(beanD);
                            #endregion
                        }
                        #endregion

                        #region 新增【產品序號資訊】明細
                        string[] PRcSerialID = IV_SERIAL.Split(';');
                        string[] PRcMaterialID = IV_SNPID.Split(';');
                        string[] PRcMaterialName = IV_PIDNAME.Split(';');
                        string[] PRcProductNumber = IV_PN.Split(';');
                        string PRcInstallID = string.Empty;

                        int countPR = PRcSerialID.Length;

                        for (int i = 0; i < countPR; i++)
                        {
                            if (IV_SERIAL != "")
                            {
                                TB_ONE_SRDetail_Product beanD = new TB_ONE_SRDetail_Product();

                                string cMaterialID = string.Empty;
                                string cMaterialName = string.Empty;
                                string cProductNumber = string.Empty;
                                string cInstallID = string.Empty;

                                if (PRcMaterialName[i] != "")
                                {
                                    cMaterialID = PRcMaterialID[i];
                                    cMaterialName = PRcMaterialName[i];
                                    cProductNumber = PRcProductNumber[i];
                                    cInstallID = CMF.findInstallNumber(IV_SERIAL);
                                }
                                else
                                {
                                    var ProBean = CMF.findMaterialBySerial(PRcSerialID[i]);

                                    cMaterialID = string.IsNullOrEmpty(PRcMaterialID[i]) ? ProBean.ProdID : PRcMaterialID[i];
                                    cMaterialName = ProBean.Product;
                                    cProductNumber = ProBean.MFRPN;
                                    cInstallID = ProBean.InstallNo;
                                }

                                beanD.cSRID = pSRID;
                                beanD.cSerialID = PRcSerialID[i];
                                beanD.cMaterialID = cMaterialID;
                                beanD.cMaterialName = cMaterialName;
                                beanD.cProductNumber = cProductNumber;
                                beanD.cNewSerialID = "";
                                beanD.cInstallID = cInstallID;
                                beanD.Disabled = 0;

                                beanD.CreatedDate = DateTime.Now;
                                beanD.CreatedUserName = pLoginName;

                                dbOne.TB_ONE_SRDetail_Product.Add(beanD);
                            }
                        }
                        #endregion

                        #region 新增【保固SLA資訊】明細
                        List<SRWarranty> QueryToList = new List<SRWarranty>();    //查詢出來的清單    

                        #region 呼叫RFC並回傳保固SLA Table清單
                        if (countPR > 0)
                        {
                            if (IV_SERIAL != "")
                            {
                                QueryToList = CMF.ZFM_TICC_SERIAL_SEARCHWTYList(PRcSerialID, tBPMURLName, tONEURLName, tAPIURLName);

                                #region 保固，因RFC已經有回傳所有清單，這邊暫時先不用
                                //foreach (string IV_SERIAL in ArySERIAL)
                                //{
                                //    if (IV_SERIAL != null)
                                //    {
                                //        var beans = dbProxy.Stockwties.OrderByDescending(x => x.IvEdate).ThenByDescending(x => x.BpmNo).Where(x => x.IvSerial == IV_SERIAL.Trim());

                                //        foreach (var bean in beans)
                                //        {
                                //            NowCount++;

                                //            #region 組待查詢清單
                                //            SRWarranty QueryInfo = new SRWarranty();

                                //            //string[] tBPMList = CMF.findBPMWarrantyInfo(bean.BpmNo);

                                //            DNDATE = bean.IvDndate == null ? "" : Convert.ToDateTime(bean.IvDndate).ToString("yyyy-MM-dd");
                                //            SDATE = bean.IvSdate == null ? "" : Convert.ToDateTime(bean.IvSdate).ToString("yyyy-MM-dd");
                                //            EDATE = bean.IvEdate == null ? "" : Convert.ToDateTime(bean.IvEdate).ToString("yyyy-MM-dd");

                                //            #region 取得BPM Url
                                //            tURL = "";

                                //            if (bean.BpmNo != null)
                                //            {
                                //                if (bean.BpmNo.IndexOf("WTY") != -1)
                                //                {
                                //                    tURL = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/Warranty/WarrantyForm.aspx?FormNo=" + bean.BpmNo + " target=_blank";
                                //                }
                                //                else
                                //                {
                                //                    tURL = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Form/Guarantee/GuaranteeForm.aspx?FormNo=" + bean.BpmNo + " target=_blank";
                                //                }
                                //            }
                                //            #endregion

                                //            QueryInfo.cID = NowCount.ToString();                                        //系統ID
                                //            QueryInfo.cSerialID = bean.IvSerial;                                         //序號                        
                                //            QueryInfo.cWTYID = bean.IvWtyid;                                             //保固
                                //            QueryInfo.cWTYName = bean.IvWtydesc;                                         //保固說明
                                //            QueryInfo.cWTYSDATE = SDATE;                                                //保固開始日期
                                //            QueryInfo.cWTYEDATE = EDATE;                                                //保固結束日期                                                          
                                //            QueryInfo.cSLARESP = bean.IvSlaresp;                                         //回應條件
                                //            QueryInfo.cSLASRV = bean.IvSlasrv;                                          //服務條件
                                //            QueryInfo.cContractID = "";                                                 //合約編號                        
                                //            QueryInfo.cBPMFormNo = string.IsNullOrEmpty(bean.BpmNo) ? "" : bean.BpmNo;      //BPM表單編號                        
                                //            QueryInfo.cBPMFormNoUrl = tURL;                                             //BPM URL                    
                                //            QueryInfo.cUsed = "N";                                                     //本次使用

                                //            QueryToList.Add(QueryInfo);
                                //            #endregion
                                //        }
                                //    }
                                //}
                                #endregion

                                QueryToList = QueryToList.OrderBy(x => x.SERIALID).ThenByDescending(x => x.WTYEDATE).ToList();
                            }
                        }
                        #endregion

                        foreach (var beanSR in QueryToList)
                        {
                            TB_ONE_SRDetail_Warranty beanD = new TB_ONE_SRDetail_Warranty();

                            string tSerialID = string.Empty;
                            string tWTYID = string.Empty;

                            if (IV_WTY.Trim() != "")
                            {
                                #region 因Joradn說不會有二筆序號，先註解，改只抓一筆
                                //tSerialID = IV_WTY.Trim().Split(';')[0];
                                //tWTYID = IV_WTY.Trim().Split(';')[1];
                                #endregion

                                tSerialID = beanSR.SERIALID;
                                tWTYID = IV_WTY.Trim();
                            }

                            beanD.cSRID = pSRID;
                            beanD.cSerialID = beanSR.SERIALID;
                            beanD.cWTYID = beanSR.WTYID;
                            beanD.cWTYName = beanSR.WTYName;

                            if (beanSR.WTYSDATE != "")
                            {
                                beanD.cWTYSDATE = Convert.ToDateTime(beanSR.WTYSDATE);
                            }

                            if (beanSR.WTYEDATE != "")
                            {
                                beanD.cWTYEDATE = Convert.ToDateTime(beanSR.WTYEDATE);
                            }

                            beanD.cSLARESP = beanSR.SLARESP;
                            beanD.cSLASRV = beanSR.SLASRV;
                            beanD.cContractID = beanSR.CONTRACTID;
                            beanD.cSubContractID = beanSR.SUBCONTRACTID;
                            beanD.cBPMFormNo = beanSR.BPMFormNo;
                            beanD.cAdvice = beanSR.ADVICE;

                            #region 判斷是否有指定使用
                            if (tWTYID != "")
                            {
                                switch (tWTYID.Substring(0, 2))
                                {
                                    #region 有保固抓保固欄位
                                    case "OM":
                                    case "EX":
                                    case "TM":
                                        if (beanSR.SERIALID == tSerialID && beanSR.WTYID == tWTYID)
                                        {
                                            beanD.cUsed = "Y";
                                        }
                                        else
                                        {
                                            beanD.cUsed = beanSR.USED;
                                        }

                                        break;
                                    #endregion

                                    #region 剩下判斷合約編號欄位
                                    default:
                                        if (beanSR.SERIALID == tSerialID && beanSR.CONTRACTID == tWTYID)
                                        {
                                            beanD.cUsed = "Y";
                                        }
                                        else
                                        {
                                            beanD.cUsed = beanSR.USED;
                                        }

                                        break;
                                        #endregion
                                }
                            }
                            else
                            {
                                beanD.cUsed = beanSR.USED;
                            }
                            #endregion

                            beanD.CreatedDate = DateTime.Now;
                            beanD.CreatedUserName = pLoginName;

                            dbOne.TB_ONE_SRDetail_Warranty.Add(beanD);
                        }
                        #endregion

                        int result = dbOne.SaveChanges();

                        if (result <= 0)
                        {
                            pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新建失敗！" + Environment.NewLine;
                            CMF.writeToLog(pSRID, "SaveGenerallySR_API", pMsg, pLoginName);

                            SROUT.EV_SRID = pSRID;
                            SROUT.EV_MSGT = "E";
                            SROUT.EV_MSG = pMsg;
                        }
                        else
                        {
                            SROUT.EV_SRID = pSRID;
                            SROUT.EV_MSGT = "Y";
                            SROUT.EV_MSG = "";

                            #region 寄送Mail通知
                            CMF.SetSRMailContent(SRCondition.ADD, pOperationID_GenerallySR, pOperationID_InstallSR, pOperationID_MaintainSR, pBUKRS, pSRID, tONEURLName, tAttachURLName, tAttachPath, pLoginName, tIsFormal);
                            #endregion
                        }
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (DbEntityValidationResult e in ex.EntityValidationErrors)
                    {
                        foreach (DbValidationError ve in e.ValidationErrors)
                        {
                            sb.AppendLine($"欄位 {ve.PropertyName} 發生錯誤: {ve.ErrorMessage}");
                        }
                    }

                    pMsg = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + sb.ToString() + Environment.NewLine;
                    pMsg += " 失敗行數：" + ex.ToString();

                    CMF.writeToLog(pSRID, "SaveGenerallySR_API", pMsg, pLoginName);

                    SROUT.EV_SRID = pSRID;
                    SROUT.EV_MSGT = "E";
                    SROUT.EV_MSG = pMsg;
                }
                #endregion
            }            

            return SROUT;
        }
        #endregion

        #region 取號(SRID)
        /// <summary>
        /// 取號(SRID)
        /// </summary>
        /// <param name="cTilte">SRID開頭</param>        
        /// <returns></returns>
        public string GetSRID(string cTilte)
        {
            string strCNO = "";
            string tYear = DateTime.Now.Year.ToString().Substring(2, 2);
            string tMonth = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string tDay = DateTime.Now.Day.ToString().PadLeft(2, '0');

            #region 取號
            var bean = dbOne.TB_ONE_SRIDFormat.FirstOrDefault(x => x.cTitle == cTilte && x.cYear == tYear && x.cMonth == tMonth && x.cDay == tDay);

            if (bean == null) //若沒有資料，則新增一筆當月的資料
            {
                TB_ONE_SRIDFormat FormNoTable = new TB_ONE_SRIDFormat();

                FormNoTable.cTitle = cTilte;
                FormNoTable.cYear = tYear;
                FormNoTable.cMonth = tMonth;
                FormNoTable.cDay = tDay;
                FormNoTable.cNO = "0000";

                dbOne.TB_ONE_SRIDFormat.Add(FormNoTable);
                dbOne.SaveChanges();
            }

            bean = dbOne.TB_ONE_SRIDFormat.FirstOrDefault(x => x.cTitle == cTilte && x.cYear == tYear && x.cMonth == tMonth && x.cDay == tDay);

            if (bean != null)
            {
                strCNO = cTilte + (bean.cYear + bean.cMonth + bean.cDay) + (int.Parse(bean.cNO) + 1).ToString().PadLeft(4, '0');
                bean.cNO = (int.Parse(bean.cNO) + 1).ToString().PadLeft(4, '0');

                dbOne.SaveChanges();
            }
            #endregion

            return strCNO;
        }
        #endregion       

        #region 一般服務案件主檔INPUT資訊
        /// <summary>一般服務案件主檔INPUT資訊</summary>
        public struct SRMain_GENERALSR_INPUT
        {
            /// <summary>建立者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>客戶ID</summary>
            public string IV_CUSTOMER { get; set; }
            /// <summary>服務團隊ID</summary>
            public string IV_SRTEAM { get; set; }
            /// <summary>維護服務種類</summary>
            public string IV_RKIND { get; set; }
            /// <summary>報修管道</summary>
            public string IV_PATHWAY { get; set; }
            /// <summary>服務案件說明</summary>
            public string IV_DESC { get; set; }
            /// <summary>詳細描述</summary>
            public string IV_LTXT { get; set; }
            /// <summary>報修代碼(大類)</summary>
            public string IV_MKIND1 { get; set; }
            /// <summary>報修代碼(中類)</summary>
            public string IV_MKIND2 { get; set; }
            /// <summary>報修代碼(小類)</summary>
            public string IV_MKIND3 { get; set; }
            /// <summary>報修人姓名</summary>
            public string IV_REPAIRNAME { get; set; }
            /// <summary>報修人電話</summary>
            public string IV_REPAIRTEL { get; set; }
            /// <summary>報修人手機</summary>
            public string IV_REPAIRMOB { get; set; }
            /// <summary>報修人地址</summary>
            public string IV_REPAIRADDR { get; set; }
            /// <summary>報修人Email</summary>
            public string IV_REPAIREMAIL { get; set; }
            /// <summary>指派L2/L3工程師</summary>
            public string IV_ASSIGN { get; set; }
            /// <summary>主要工程師員工編號</summary>
            public string IV_EMPNO { get; set; }
            /// <summary>SQ人員ID</summary>
            public string IV_SQEMPID { get; set; }
            /// <summary>序號ID</summary>
            public string IV_SERIAL { get; set; }
            /// <summary>物料代號</summary>
            public string IV_SNPID { get; set; }
            /// <summary>機器型號</summary>
            public string IV_PIDNAME { get; set; }
            /// <summary>Product Number(P/N)</summary>
            public string IV_PN { get; set; }
            /// <summary>保固代號(若是合約則傳入合約編號)</summary>
            public string IV_WTY { get; set; }
            /// <summary>是否為二修(Y.是 N.否)</summary>
            public string IV_REFIX { get; set; }
            /// <summary>是否為內部作業(Y.是 N.否)</summary>
            public string IV_INTERNALWORK { get; set; }
            /// <summary>檢附文件</summary>
            public HttpPostedFileBase[] IV_ATTACHFiles { get; set; }

            /// <summary>服務案件客戶聯絡人資訊</summary>
            public List<CREATECONTACTINFO> CREATECONTACT_LIST { get; set; }
        }
        #endregion

        #region 一般服務案件主檔OUTPUT資訊
        /// <summary>一般服務案件主檔OUTPUT資訊</summary>
        public struct SRMain_GENERALSR_OUTPUT
        {
            /// <summary>SRID</summary>
            public string EV_SRID { get; set; }
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #region 測試call API範例程式

        #region 【測試用】建立ONE SERVICE報修SR（一般服務請求）接口
        /// <summary>
        /// 【測試用】建立ONE SERVICE報修SR（一般服務請求）接口
        /// </summary>        
        public bool callTestToGENERALSR_CREATE()
        {
            bool reValue = false;

            SRMain_GENERALSR_OUTPUT SROUT = new SRMain_GENERALSR_OUTPUT();

            //string tAPIURLName = @"https://api.etatung.com";    //正式區
            string tAPIURLName = @"http://localhost:32603";    //測試區
            string tURL = tAPIURLName + "/API/API_GENERALSR_CREATE";

            string tSubject = "test12345";
            string tBody = "測試Body";
            string tCustomerID = "D03251108";
            string cTeamID = "SRV.12100000";

            string CONTNAME = string.Empty;
            string CONTADDR = string.Empty;
            string CONTTEL = string.Empty;
            string CONTMOBILE = string.Empty;
            string CONTEMAIL = string.Empty;

            try
            {
                var client = new RestClient(tURL);  //測試用            

                var request = new RestRequest();
                request.Method = RestSharp.Method.Post;

                #region 取得報修人清單(只取一筆即可)
                List<CREATECONTACTINFO> CREATECONTACT_LIST = new List<CREATECONTACTINFO>();

                CREATECONTACTINFO Con = new CREATECONTACTINFO();

                CONTNAME = "王炯凱";
                CONTADDR = "台北市中山區松江路121號13樓";
                CONTTEL = "02-2506-2121#1239";
                CONTMOBILE = "0909000000";
                CONTEMAIL = "elvis.chang@etatung.com";

                Con.CONTNAME = CONTNAME;
                Con.CONTADDR = CONTADDR;
                Con.CONTTEL = CONTTEL;
                Con.CONTMOBILE = CONTMOBILE;
                Con.CONTEMAIL = CONTEMAIL;

                CREATECONTACT_LIST.Add(Con);
                #endregion

                Dictionary<Object, Object> parameters = new Dictionary<Object, Object>();
                parameters.Add("IV_LOGINEMPNO", "Tsti_alert");
                parameters.Add("IV_CUSTOMER", tCustomerID);
                parameters.Add("IV_SRTEAM", cTeamID);
                parameters.Add("IV_RKIND", "Z03");      //預設為合約
                parameters.Add("IV_PATHWAY", "Z04");    //預設為主動式告警
                parameters.Add("IV_DESC", tSubject);
                parameters.Add("IV_LTXT", tBody);
                parameters.Add("IV_REFIX", "N");
                parameters.Add("IV_REPAIRNAME", CONTNAME);
                parameters.Add("IV_REPAIRADDR", CONTADDR);
                parameters.Add("IV_REPAIRTEL", CONTTEL);
                parameters.Add("IV_REPAIRMOB", CONTMOBILE);
                parameters.Add("IV_REPAIREMAIL", CONTEMAIL);
                parameters.Add("CREATECONTACT_LIST", CREATECONTACT_LIST);

                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", parameters, ParameterType.RequestBody);

                RestResponse response = client.Execute(request);

                var data = (JObject)JsonConvert.DeserializeObject(response.Content);

                SROUT.EV_SRID = data["EV_SRID"].ToString().Trim();
                SROUT.EV_MSGT = data["EV_MSGT"].ToString().Trim();
                SROUT.EV_MSG = data["EV_MSG"].ToString().Trim();

                if (SROUT.EV_MSGT == "Y")
                {
                    reValue = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateSR Error: {ex}");
            }

            return reValue;
        }
        #endregion        

        #endregion

        #endregion -----↑↑↑↑↑一般服務案件建立 ↑↑↑↑↑-----    

        #region -----↓↓↓↓↓裝機服務案件建立 ↓↓↓↓↓-----       

        #region 建立ONE SERVICE報修SR（裝機服務案件）接口
        /// <summary>
        /// 建立ONE SERVICE報修SR（裝機服務案件）接口
        /// </summary>
        /// <param name="bean"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_INSTALLSR_CREATE(SRMain_INSTALLSR_INPUT beanIN)
        {
            #region Json範列格式，一筆(建立INSTALLSR_CREATEByAPI)
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //     "IV_CUSTOMER": "D03251108",
            //     "IV_SRTEAM": "SRV.12200006",
            //     "IV_SALESNO": "201234567",
            //     "IV_SHIPMENTNO": "251234567",
            //     "IV_DESC": "251234567SAP系統自動派單",
            //     "IV_LTXT": "251234567SAP系統自動派單",
            //     "IV_MKIND1": "",
            //     "IV_MKIND2": "",
            //     "IV_MKIND3": "",                 
            //     "IV_SALESEMPNO": "10012088",
            //     "IV_SECRETARYEMPNO": "10005805",
            //     "IV_EMPNO": "",
            //     "IV_ATTACHFiles" :"檔案",
            //     "CREATECONTACT_LIST": [
            //        {
            //        "CONTNAME": "賴淑瑛",
            //            "CONTADDR": "台北市信義區菸廠路88號12樓",
            //            "CONTTEL": "(02)6638-6888#13158",
            //            "CONTMOBILE": "",
            //            "CONTEMAIL": "elvis.chang@etatung.com"
            //        }               
            //    ],
            //    "CREATEMATERIAL_LIST": [
            //        {
            //        "MATERIALID": "G-FIE15MPCAPUSB",
            //            "QTY": "1"
            //        },
            //        {
            //        "MATERIALID": "G-507283-001---",
            //            "QTY": "1"
            //        }                 
            //    ]
            //}
            #endregion

            SRMain_INSTALLSR_OUTPUT SROUT = new SRMain_INSTALLSR_OUTPUT();

            SROUT = SaveInstallSR(beanIN, "ADD"); //新增

            return Json(SROUT);
        }
        #endregion

        #region 儲存裝機服務案件
        /// <summary>
        /// 儲存裝機服務案件
        /// </summary>
        /// <param name="bean">裝機服務案件主檔資訊</param>
        /// <param name="tType">ADD.新增 EDIT.修改</param>
        /// <returns></returns>
        private SRMain_INSTALLSR_OUTPUT SaveInstallSR(SRMain_INSTALLSR_INPUT bean, string tType)
        {
            SRMain_INSTALLSR_OUTPUT SROUT = new SRMain_INSTALLSR_OUTPUT();

            bool tIsFormal = false;

            int pTotalQuantity = 0; //總安裝數量

            string pLoginName = string.Empty;
            string pBUKRS = string.Empty;
            string pSRID = string.Empty;
            string OldCStatus = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string tAttachPath = string.Empty;

            string IV_LOGINEMPNO = string.IsNullOrEmpty(bean.IV_LOGINEMPNO) ? "" : bean.IV_LOGINEMPNO.Trim();
            string IV_CUSTOMER = string.IsNullOrEmpty(bean.IV_CUSTOMER) ? "" : bean.IV_CUSTOMER.Trim();
            string IV_SRTEAM = string.IsNullOrEmpty(bean.IV_SRTEAM) ? "" : bean.IV_SRTEAM.Trim();
            string IV_SALESNO = string.IsNullOrEmpty(bean.IV_SALESNO) ? "" : bean.IV_SALESNO.Trim();
            string IV_SHIPMENTNO = string.IsNullOrEmpty(bean.IV_SHIPMENTNO) ? "" : bean.IV_SHIPMENTNO.Trim();
            string IV_DESC = string.IsNullOrEmpty(bean.IV_DESC) ? "" : bean.IV_DESC.Trim();
            string IV_LTXT = string.IsNullOrEmpty(bean.IV_LTXT) ? "" : bean.IV_LTXT.Trim();
            string IV_MKIND1 = string.IsNullOrEmpty(bean.IV_MKIND1) ? "" : bean.IV_MKIND1.Trim();
            string IV_MKIND2 = string.IsNullOrEmpty(bean.IV_MKIND2) ? "" : bean.IV_MKIND2.Trim();
            string IV_MKIND3 = string.IsNullOrEmpty(bean.IV_MKIND3) ? "" : bean.IV_MKIND3.Trim();
            string IV_SALESEMPNO = string.IsNullOrEmpty(bean.IV_SALESEMPNO) ? "" : bean.IV_SALESEMPNO.Trim();
            string IV_SECRETARYEMPNO = string.IsNullOrEmpty(bean.IV_SECRETARYEMPNO) ? "" : bean.IV_SECRETARYEMPNO.Trim();
            string IV_EMPNO = string.IsNullOrEmpty(bean.IV_EMPNO) ? "" : bean.IV_EMPNO.Trim();
            string IV_PATHWAY = string.IsNullOrEmpty(bean.IV_PATHWAY) ? "" : bean.IV_PATHWAY.Trim();
            HttpPostedFileBase[] AttachFiles = bean.IV_ATTACHFiles;

            string CCustomerName = CMF.findCustName(IV_CUSTOMER);
            string CMainEngineerName = CMF.findEmployeeName(IV_EMPNO);
            string CSalesName = CMF.findEmployeeName(IV_SALESEMPNO);
            string CSecretaryName = CMF.findEmployeeName(IV_SECRETARYEMPNO);

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
                pBUKRS = "T012";
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                pBUKRS = EmpBean.BUKRS;
            }

            #region 判斷必填欄位
            if (string.IsNullOrEmpty(IV_LOGINEMPNO))
            {
                pMsg += "【登入者員工編號】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_CUSTOMER))
            {
                pMsg += "【客戶ID】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_SALESNO))
            {
                pMsg += "【銷售訂單號】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_SHIPMENTNO))
            {
                pMsg += "【出貨單號】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_SALESEMPNO))
            {
                pMsg += "【業務人員員工編號】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_SECRETARYEMPNO))
            {
                pMsg += "【業務祕書員工編號】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_DESC))
            {
                pMsg += "【服務說明】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_LTXT))
            {
                pMsg += "【詳細描述】不得為空！" + Environment.NewLine;
            }                      
            #endregion

            if (pMsg != "")
            {
                pMsg = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + pMsg;
                CMF.writeToLog(pSRID, "SaveInstallSR_API", pMsg, pLoginName);

                SROUT.EV_SRID = pSRID;
                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = pMsg;
            }
            else
            {
                #region 開始執行
                try
                {
                    #region 取得系統位址參數相關資訊
                    SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

                    tIsFormal = ParaBean.IsFormal;

                    tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
                    tONEURLName = ParaBean.ONEURLName;
                    tBPMURLName = ParaBean.BPMURLName;
                    tPSIPURLName = ParaBean.PSIPURLName;
                    tAttachURLName = ParaBean.AttachURLName;
                    tAttachPath = Server.MapPath("~/REPORT");
                    #endregion

                    if (tType == "ADD")
                    {
                        #region 新增主檔
                        TB_ONE_SRMain beanM = new TB_ONE_SRMain();

                        pSRID = GetSRID("63");

                        //主表資料
                        beanM.cSRID = pSRID;

                        if (IV_EMPNO != "")
                        {
                            beanM.cStatus = "E0008"; //新建但狀態是裝機中
                        }
                        else
                        {
                            beanM.cStatus = "E0001"; //新建
                        }

                        beanM.cCustomerName = CCustomerName;
                        beanM.cCustomerID = IV_CUSTOMER;
                        beanM.cDesc = IV_DESC;
                        beanM.cNotes = IV_LTXT;
                        beanM.cSRTypeOne = IV_MKIND1;
                        beanM.cSRTypeSec = IV_MKIND2;
                        beanM.cSRTypeThr = IV_MKIND3;
                        beanM.cSalesNo = IV_SALESNO;
                        beanM.cShipmentNo = IV_SHIPMENTNO;
                        beanM.cSRPathWay = IV_PATHWAY;

                        beanM.cTeamID = IV_SRTEAM;
                        beanM.cMainEngineerName = CMainEngineerName;
                        beanM.cMainEngineerID = IV_EMPNO;
                        beanM.cSalesName = CSalesName;
                        beanM.cSalesID = IV_SALESEMPNO;
                        beanM.cSecretaryName = CSecretaryName;
                        beanM.cSecretaryID = IV_SECRETARYEMPNO;

                        beanM.cSystemGUID = Guid.NewGuid();
                        beanM.CreatedDate = DateTime.Now;
                        beanM.CreatedUserName = pLoginName;

                        if (AttachFiles != null)
                        {
                            #region 檢附文件
                            if (AttachFiles.Length > 0)
                            {
                                Guid fileGuid = Guid.NewGuid();

                                string cAttachementID = string.Empty;
                                string path = string.Empty;
                                string fileId = string.Empty;
                                string fileOrgName = string.Empty;
                                string fileName = string.Empty;
                                string fileALLName = string.Empty;

                                foreach (var Attach in AttachFiles)
                                {
                                    if (Attach != null)
                                    {
                                        #region 檔案部份
                                        fileGuid = Guid.NewGuid();

                                        cAttachementID += fileGuid.ToString() + ",";

                                        fileId = fileGuid.ToString();
                                        fileOrgName = Attach.FileName;
                                        fileName = fileId + Path.GetExtension(Attach.FileName);
                                        path = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                                        Attach.SaveAs(path);
                                        #endregion

                                        #region table部份                                        
                                        TB_ONE_DOCUMENT beanDoc = new TB_ONE_DOCUMENT();

                                        beanDoc.ID = fileGuid;
                                        beanDoc.FILE_ORG_NAME = fileOrgName;
                                        beanDoc.FILE_NAME = fileName;
                                        beanDoc.FILE_EXT = Path.GetExtension(Attach.FileName);
                                        beanDoc.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                        dbOne.TB_ONE_DOCUMENT.Add(beanDoc);
                                        dbOne.SaveChanges();

                                        fileALLName += fileName + ",";
                                        #endregion
                                    }
                                }

                                beanM.cAttachement = cAttachementID;
                            }
                            #endregion
                        }

                        #region 未用到的欄位
                        beanM.cAttachement = "";
                        beanM.cAttachementStockNo = "";
                        beanM.cRepairName = "";
                        beanM.cRepairAddress = "";
                        beanM.cRepairPhone = "";
                        beanM.cRepairMobile = "";
                        beanM.cRepairEmail = "";
                        beanM.cDelayReason = "";
                        beanM.cMAServiceType = "";                        
                        beanM.cSRProcessWay = "";
                        beanM.cIsSecondFix = "";
                        beanM.cAssEngineerID = "";
                        beanM.cTechManagerID = "";
                        beanM.cSQPersonID = "";
                        beanM.cSQPersonName = "";
                        beanM.cIsAPPClose = "";
                        beanM.cIsInternalWork = "N";
                        #endregion

                        dbOne.TB_ONE_SRMain.Add(beanM);
                        #endregion

                        #region 新增【客戶聯絡人資訊】明細
                        if (bean.CREATECONTACT_LIST != null)
                        {
                            foreach (var beanCon in bean.CREATECONTACT_LIST)
                            {
                                string IV_CONTNAME = string.IsNullOrEmpty(beanCon.CONTNAME) ? "" : beanCon.CONTNAME.Trim();
                                string IV_CONTADDR = string.IsNullOrEmpty(beanCon.CONTADDR) ? "" : beanCon.CONTADDR.Trim();
                                string IV_CONTTEL = string.IsNullOrEmpty(beanCon.CONTTEL) ? "" : beanCon.CONTTEL.Trim();
                                string IV_CONTMOBILE = string.IsNullOrEmpty(beanCon.CONTMOBILE) ? "" : beanCon.CONTMOBILE.Trim();
                                string IV_CONTEMAIL = string.IsNullOrEmpty(beanCon.CONTEMAIL) ? "" : beanCon.CONTEMAIL.Trim();

                                TB_ONE_SRDetail_Contact beanD = new TB_ONE_SRDetail_Contact();

                                beanD.cSRID = pSRID;
                                beanD.cContactName = IV_CONTNAME;
                                beanD.cContactAddress = IV_CONTADDR;
                                beanD.cContactPhone = IV_CONTTEL;
                                beanD.cContactMobile = IV_CONTMOBILE;
                                beanD.cContactEmail = IV_CONTEMAIL;
                                beanD.Disabled = 0;

                                beanD.CreatedDate = DateTime.Now;
                                beanD.CreatedUserName = pLoginName;

                                dbOne.TB_ONE_SRDetail_Contact.Add(beanD);
                            }
                        }
                        #endregion

                        #region 新增【物料訊息資訊】明細
                        if (bean.CREATEMATERIAL_LIST != null)
                        {
                            foreach (var beanMA in bean.CREATEMATERIAL_LIST)
                            {
                                string MATERIALID = string.IsNullOrEmpty(beanMA.MATERIALID) ? "" : beanMA.MATERIALID.Trim();
                                string MATERIALNAME = string.IsNullOrEmpty(beanMA.MATERIALNAME) ? "" : beanMA.MATERIALNAME.Trim();
                                string QTY = string.IsNullOrEmpty(beanMA.QTY) ? "1" : beanMA.QTY.Trim();
                                var MaInfo = CMF.findMaterialInfo(MATERIALID);

                                TB_ONE_SRDetail_MaterialInfo beanD = new TB_ONE_SRDetail_MaterialInfo();

                                pTotalQuantity += int.Parse(QTY);

                                beanD.cSRID = pSRID;
                                beanD.cMaterialID = MaInfo.MaterialID;

                                if (MATERIALNAME != "")
                                {
                                    beanD.cMaterialName = MATERIALNAME;
                                }
                                else
                                {
                                    beanD.cMaterialName = MaInfo.MaterialName;
                                }

                                beanD.cQuantity = int.Parse(QTY);
                                beanD.cBasicContent = MaInfo.BasicContent;
                                beanD.cMFPNumber = MaInfo.MFPNumber;
                                beanD.cBrand = MaInfo.Brand;
                                beanD.cProductHierarchy = MaInfo.ProductHierarchy;
                                beanD.Disabled = 0;

                                beanD.CreatedDate = DateTime.Now;
                                beanD.CreatedUserName = pLoginName;

                                dbOne.TB_ONE_SRDetail_MaterialInfo.Add(beanD);
                            }
                        }
                        #endregion

                        #region 新增【序號回報資訊】明細
                        if (bean.CREATEFEEDBACK_LIST != null)
                        {
                            foreach (var beanFB in bean.CREATEFEEDBACK_LIST)
                            {
                                string SERIALID = string.IsNullOrEmpty(beanFB.SERIALID) ? "" : beanFB.SERIALID.Trim();
                                string cMaterialID = string.Empty;
                                string cMaterialName = string.Empty;

                                var ProBean = CMF.findMaterialBySerial(SERIALID);
                                if (ProBean.IV_SERIAL != null)
                                {
                                    cMaterialID = ProBean.ProdID;
                                    cMaterialName = ProBean.Product;
                                }

                                TB_ONE_SRDetail_SerialFeedback beanD = new TB_ONE_SRDetail_SerialFeedback();

                                beanD.cSRID = pSRID;
                                beanD.cSerialID = SERIALID;
                                beanD.cMaterialID = cMaterialID;
                                beanD.cMaterialName = cMaterialName;

                                beanD.Disabled = 0;
                                beanD.CreatedDate = DateTime.Now;
                                beanD.CreatedUserName = pLoginName;

                                dbOne.TB_ONE_SRDetail_SerialFeedback.Add(beanD);
                            }
                        }
                        #endregion

                        int result = dbOne.SaveChanges();

                        if (result <= 0)
                        {
                            pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新建失敗！" + Environment.NewLine;
                            CMF.writeToLog(pSRID, "SaveInstallSR_API", pMsg, pLoginName);

                            SROUT.EV_SRID = pSRID;
                            SROUT.EV_MSGT = "E";
                            SROUT.EV_MSG = pMsg;
                        }
                        else
                        {
                            #region 批次儲存APP_INSTALL檔                        
                            int cID = 0;
                            string TotalQuantity = pTotalQuantity.ToString("N0");
                            string tIsFormAPP = "N";
                            string InstallQuantity = "0";
                            string InstallDate = string.Empty;
                            string ExpectedDate = string.Empty;

                            string returnMsg = CMF.SaveTB_SERVICES_APP_INSTALL(EmpBean.EmployeeNO, pLoginName, EmpBean.EmployeeERPID, cID, pSRID, TotalQuantity, InstallQuantity, InstallDate, ExpectedDate, tIsFormAPP);

                            if (returnMsg != "SUCCESS")
                            {
                                SROUT.EV_SRID = pSRID;
                                SROUT.EV_MSGT = "E";
                                SROUT.EV_MSG = returnMsg;
                            }
                            else
                            {
                                SROUT.EV_SRID = pSRID;
                                SROUT.EV_MSGT = "Y";
                                SROUT.EV_MSG = "";

                                #region 寄送Mail通知
                                CMF.SetSRMailContent(SRCondition.ADD, pOperationID_GenerallySR, pOperationID_InstallSR, pOperationID_MaintainSR, pBUKRS, pSRID, tONEURLName, tAttachURLName, tAttachPath, pLoginName, tIsFormal);
                                #endregion
                            }
                            #endregion
                        }
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (DbEntityValidationResult e in ex.EntityValidationErrors)
                    {
                        foreach (DbValidationError ve in e.ValidationErrors)
                        {
                            sb.AppendLine($"欄位 {ve.PropertyName} 發生錯誤: {ve.ErrorMessage}");
                        }
                    }

                    pMsg = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + sb.ToString() + Environment.NewLine;
                    pMsg += " 失敗行數：" + ex.ToString();

                    CMF.writeToLog(pSRID, "SaveInstallSR_API", pMsg, pLoginName);

                    SROUT.EV_SRID = pSRID;
                    SROUT.EV_MSGT = "E";
                    SROUT.EV_MSG = pMsg;
                }
                #endregion
            }

            return SROUT;
        }
        #endregion      

        #region 裝機服務案件主檔INPUT資訊
        /// <summary>裝機服務案件主檔INPUT資訊</summary>
        public struct SRMain_INSTALLSR_INPUT
        {
            /// <summary>建立者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>客戶ID</summary>
            public string IV_CUSTOMER { get; set; }
            /// <summary>服務團隊ID</summary>
            public string IV_SRTEAM { get; set; }
            /// <summary>銷售訂單號</summary>
            public string IV_SALESNO { get; set; }
            /// <summary>出貨單號</summary>
            public string IV_SHIPMENTNO { get; set; }
            /// <summary>服務案件說明</summary>
            public string IV_DESC { get; set; }
            /// <summary>詳細描述</summary>
            public string IV_LTXT { get; set; }
            /// <summary>報修管道</summary>
            public string IV_PATHWAY { get; set; }
            /// <summary>報修代碼(大類)</summary>
            public string IV_MKIND1 { get; set; }
            /// <summary>報修代碼(中類)</summary>
            public string IV_MKIND2 { get; set; }
            /// <summary>報修代碼(小類)</summary>
            public string IV_MKIND3 { get; set; }
            /// <summary>主要工程師員工編號</summary>
            public string IV_EMPNO { get; set; }
            /// <summary>業務人員員工編號</summary>
            public string IV_SALESEMPNO { get; set; }
            /// <summary>業務祕書員工編號</summary>
            public string IV_SECRETARYEMPNO { get; set; }
            /// <summary>檢附文件</summary>
            public HttpPostedFileBase[] IV_ATTACHFiles { get; set; }

            /// <summary>服務案件客戶聯絡人資訊</summary>
            public List<CREATECONTACTINFO> CREATECONTACT_LIST { get; set; }
            /// <summary>服務案件物料訊息資訊</summary>
            public List<CREATEMATERIAL> CREATEMATERIAL_LIST { get; set; }
            /// <summary>服務案件序號回報資訊</summary>
            public List<CREATEFEEDBACK> CREATEFEEDBACK_LIST { get; set; }
        }
        #endregion

        #region 裝機服務案件主檔OUTPUT資訊
        /// <summary>裝機服務案件主檔OUTPUT資訊</summary>
        public struct SRMain_INSTALLSR_OUTPUT
        {
            /// <summary>SRID</summary>
            public string EV_SRID { get; set; }
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑裝機服務案件建立 ↑↑↑↑↑-----    

        #region -----↓↓↓↓↓定維服務案件建立 ↓↓↓↓↓-----       

        #region 建立ONE SERVICE報修SR（定維服務案件）接口
        /// <summary>
        /// 建立ONE SERVICE報修SR（定維服務案件）接口
        /// </summary>
        /// <param name="bean"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_MAINTAINSR_CREATE(SRMain_MAINTAINSR_INPUT beanIN)
        {
            #region Json範列格式，一筆(建立MAINTAINSR_CREATEByAPI)
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //     "IV_CUSTOMER": "D86517315",
            //     "IV_SRTEAM": "SRV.12200006",
            //     "IV_CONTRACTID": "11012083",            
            //     "IV_DESC": "【11012083】OneService系統批次定維派單",
            //     "IV_LTXT": "【11012083】OneService系統批次定維派單",
            //     "IV_MKIND1": "",
            //     "IV_MKIND2": "",
            //     "IV_MKIND3": "",                 
            //     "IV_SALESEMPNO": "10012088",            
            //     "IV_SECRETARYEMPNO": "10005805",
            //     "IV_EMPNO": "10001567",
            //     "IV_ASSEMPNO": "99120894",
            //     "IV_ATTACHFiles" :"檔案",
            //     "CREATECONTACT_LIST": [
            //        {
            //        "CONTNAME": "張維倫",
            //            "CONTADDR": "台北市大安區敦化南路二段38號",
            //            "CONTTEL": "02-27067777#8011606",
            //            "CONTMOBILE": "",
            //            "CONTEMAIL": ""
            //        }               
            //    ]           
            //}
            #endregion

            SRMain_MAINTAINSR_OUTPUT SROUT = new SRMain_MAINTAINSR_OUTPUT();

            SROUT = SaveMaintainSR(beanIN, "ADD"); //新增

            return Json(SROUT);
        }
        #endregion

        #region 儲存定維服務案件
        /// <summary>
        /// 儲存定維服務案件
        /// </summary>
        /// <param name="bean">定維服務案件主檔資訊</param>
        /// <param name="tType">ADD.新增 EDIT.修改</param>
        /// <returns></returns>
        private SRMain_MAINTAINSR_OUTPUT SaveMaintainSR(SRMain_MAINTAINSR_INPUT bean, string tType)
        {
            SRMain_MAINTAINSR_OUTPUT SROUT = new SRMain_MAINTAINSR_OUTPUT();

            bool tIsFormal = false;            

            string pLoginName = string.Empty;
            string pBUKRS = string.Empty;
            string pSRID = string.Empty;
            string OldCStatus = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string tAttachPath = string.Empty;

            string IV_LOGINEMPNO = string.IsNullOrEmpty(bean.IV_LOGINEMPNO) ? "" : bean.IV_LOGINEMPNO.Trim();
            string IV_CUSTOMER = string.IsNullOrEmpty(bean.IV_CUSTOMER) ? "" : bean.IV_CUSTOMER.Trim();
            string IV_SRTEAM = string.IsNullOrEmpty(bean.IV_SRTEAM) ? "" : bean.IV_SRTEAM.Trim();
            string IV_CONTRACTID = string.IsNullOrEmpty(bean.IV_CONTRACTID) ? "" : bean.IV_CONTRACTID.Trim();            
            string IV_DESC = string.IsNullOrEmpty(bean.IV_DESC) ? "" : bean.IV_DESC.Trim();
            string IV_LTXT = string.IsNullOrEmpty(bean.IV_LTXT) ? "" : bean.IV_LTXT.Trim();
            string IV_MKIND1 = string.IsNullOrEmpty(bean.IV_MKIND1) ? "" : bean.IV_MKIND1.Trim();
            string IV_MKIND2 = string.IsNullOrEmpty(bean.IV_MKIND2) ? "" : bean.IV_MKIND2.Trim();
            string IV_MKIND3 = string.IsNullOrEmpty(bean.IV_MKIND3) ? "" : bean.IV_MKIND3.Trim();
            string IV_SALESEMPNO = string.IsNullOrEmpty(bean.IV_SALESEMPNO) ? "" : bean.IV_SALESEMPNO.Trim();
            string IV_SECRETARYEMPNO = string.IsNullOrEmpty(bean.IV_SECRETARYEMPNO) ? "" : bean.IV_SECRETARYEMPNO.Trim();
            string IV_EMPNO = string.IsNullOrEmpty(bean.IV_EMPNO) ? "" : bean.IV_EMPNO.Trim();
            string IV_ASSEMPNO = string.IsNullOrEmpty(bean.IV_ASSEMPNO) ? "" : bean.IV_ASSEMPNO.Trim();
            HttpPostedFileBase[] AttachFiles = bean.IV_ATTACHFiles;

            string CCustomerName = CMF.findCustName(IV_CUSTOMER);
            string CMainEngineerName = CMF.findEmployeeName(IV_EMPNO);
            string CSalesName = CMF.findEmployeeName(IV_SALESEMPNO);
            string CSecretaryName = CMF.findEmployeeName(IV_SECRETARYEMPNO);

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
                pBUKRS = "T012";
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                pBUKRS = EmpBean.BUKRS;
            }

            #region 判斷必填欄位
            if (string.IsNullOrEmpty(IV_LOGINEMPNO))
            {
                pMsg += "【登入者員工編號】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_CUSTOMER))
            {
                pMsg += "【客戶ID】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_CONTRACTID))
            {
                pMsg += "【合約文件編號】不得為空！" + Environment.NewLine;
            }           

            if (string.IsNullOrEmpty(IV_SALESEMPNO))
            {
                pMsg += "【業務人員員工編號】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_SECRETARYEMPNO))
            {
                pMsg += "【業務祕書員工編號】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_DESC))
            {
                pMsg += "【服務說明】不得為空！" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(IV_LTXT))
            {
                pMsg += "【詳細描述】不得為空！" + Environment.NewLine;
            }
            #endregion

            if (pMsg != "")
            {
                pMsg = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + pMsg;
                CMF.writeToLog(pSRID, "SaveMaintainSR_API", pMsg, pLoginName);

                SROUT.EV_SRID = pSRID;
                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = pMsg;
            }
            else
            {
                #region 開始執行
                try
                {
                    #region 取得系統位址參數相關資訊
                    SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

                    tIsFormal = ParaBean.IsFormal;

                    tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
                    tONEURLName = ParaBean.ONEURLName;
                    tBPMURLName = ParaBean.BPMURLName;
                    tPSIPURLName = ParaBean.PSIPURLName;
                    tAttachURLName = ParaBean.AttachURLName;
                    tAttachPath = Server.MapPath("~/REPORT");
                    #endregion

                    if (tType == "ADD")
                    {
                        #region 新增主檔
                        TB_ONE_SRMain beanM = new TB_ONE_SRMain();

                        pSRID = GetSRID("65");

                        //主表資料
                        beanM.cSRID = pSRID;

                        if (IV_EMPNO != "")
                        {
                            beanM.cStatus = "E0016"; //新建但狀態是定保處理中
                        }
                        else
                        {
                            beanM.cStatus = "E0001"; //新建
                        }

                        beanM.cCustomerName = CCustomerName;
                        beanM.cCustomerID = IV_CUSTOMER;
                        beanM.cContractID = IV_CONTRACTID;
                        beanM.cDesc = IV_DESC;
                        beanM.cNotes = IV_LTXT;
                        beanM.cSRTypeOne = IV_MKIND1;
                        beanM.cSRTypeSec = IV_MKIND2;
                        beanM.cSRTypeThr = IV_MKIND3;

                        beanM.cTeamID = IV_SRTEAM;
                        beanM.cMainEngineerName = CMainEngineerName;
                        beanM.cMainEngineerID = IV_EMPNO;
                        beanM.cAssEngineerID = string.IsNullOrEmpty(IV_ASSEMPNO) ? "" : IV_ASSEMPNO;
                        beanM.cSalesName = CSalesName;
                        beanM.cSalesID = IV_SALESEMPNO;
                        beanM.cSecretaryName = CSecretaryName;
                        beanM.cSecretaryID = IV_SECRETARYEMPNO;

                        beanM.cSystemGUID = Guid.NewGuid();
                        beanM.CreatedDate = DateTime.Now;
                        beanM.CreatedUserName = pLoginName;

                        if (AttachFiles != null)
                        {
                            #region 檢附文件
                            if (AttachFiles.Length > 0)
                            {
                                Guid fileGuid = Guid.NewGuid();

                                string cAttachementID = string.Empty;
                                string path = string.Empty;
                                string fileId = string.Empty;
                                string fileOrgName = string.Empty;
                                string fileName = string.Empty;
                                string fileALLName = string.Empty;

                                foreach (var Attach in AttachFiles)
                                {
                                    if (Attach != null)
                                    {
                                        #region 檔案部份
                                        fileGuid = Guid.NewGuid();

                                        cAttachementID += fileGuid.ToString() + ",";

                                        fileId = fileGuid.ToString();
                                        fileOrgName = Attach.FileName;
                                        fileName = fileId + Path.GetExtension(Attach.FileName);
                                        path = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                                        Attach.SaveAs(path);
                                        #endregion

                                        #region table部份                                        
                                        TB_ONE_DOCUMENT beanDoc = new TB_ONE_DOCUMENT();

                                        beanDoc.ID = fileGuid;
                                        beanDoc.FILE_ORG_NAME = fileOrgName;
                                        beanDoc.FILE_NAME = fileName;
                                        beanDoc.FILE_EXT = Path.GetExtension(Attach.FileName);
                                        beanDoc.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                        dbOne.TB_ONE_DOCUMENT.Add(beanDoc);
                                        dbOne.SaveChanges();

                                        fileALLName += fileName + ",";
                                        #endregion
                                    }
                                }

                                beanM.cAttachement = cAttachementID;
                            }
                            #endregion
                        }

                        #region 未用到的欄位
                        beanM.cAttachement = "";
                        beanM.cAttachementStockNo = "";
                        beanM.cRepairName = "";
                        beanM.cRepairAddress = "";
                        beanM.cRepairPhone = "";
                        beanM.cRepairMobile = "";
                        beanM.cRepairEmail = "";
                        beanM.cDelayReason = "";
                        beanM.cMAServiceType = "";
                        beanM.cSRPathWay = "";
                        beanM.cSRProcessWay = "";
                        beanM.cIsSecondFix = "";
                        beanM.cTechManagerID = "";
                        beanM.cSalesNo = "";
                        beanM.cShipmentNo = "";
                        beanM.cSQPersonID = "";
                        beanM.cSQPersonName = "";
                        beanM.cIsAPPClose = "";
                        beanM.cIsInternalWork = "N";
                        #endregion

                        dbOne.TB_ONE_SRMain.Add(beanM);
                        #endregion

                        #region 新增【客戶聯絡人資訊】明細
                        if (bean.CREATECONTACT_LIST != null)
                        {
                            foreach (var beanCon in bean.CREATECONTACT_LIST)
                            {
                                string IV_CONTNAME = string.IsNullOrEmpty(beanCon.CONTNAME) ? "" : beanCon.CONTNAME.Trim();
                                string IV_CONTADDR = string.IsNullOrEmpty(beanCon.CONTADDR) ? "" : beanCon.CONTADDR.Trim();
                                string IV_CONTTEL = string.IsNullOrEmpty(beanCon.CONTTEL) ? "" : beanCon.CONTTEL.Trim();
                                string IV_CONTMOBILE = string.IsNullOrEmpty(beanCon.CONTMOBILE) ? "" : beanCon.CONTMOBILE.Trim();
                                string IV_CONTEMAIL = string.IsNullOrEmpty(beanCon.CONTEMAIL) ? "" : beanCon.CONTEMAIL.Trim();

                                TB_ONE_SRDetail_Contact beanD = new TB_ONE_SRDetail_Contact();

                                beanD.cSRID = pSRID;
                                beanD.cContactName = IV_CONTNAME;
                                beanD.cContactAddress = IV_CONTADDR;
                                beanD.cContactPhone = IV_CONTTEL;
                                beanD.cContactMobile = IV_CONTMOBILE;
                                beanD.cContactEmail = IV_CONTEMAIL;
                                beanD.Disabled = 0;

                                beanD.CreatedDate = DateTime.Now;
                                beanD.CreatedUserName = pLoginName;

                                dbOne.TB_ONE_SRDetail_Contact.Add(beanD);
                            }
                        }
                        #endregion

                        int result = dbOne.SaveChanges();

                        if (result <= 0)
                        {
                            pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新建失敗！" + Environment.NewLine;
                            CMF.writeToLog(pSRID, "SaveMaintainSR_API", pMsg, pLoginName);

                            SROUT.EV_SRID = pSRID;
                            SROUT.EV_MSGT = "E";
                            SROUT.EV_MSG = pMsg;
                        }
                        else
                        {
                            SROUT.EV_SRID = pSRID;
                            SROUT.EV_MSGT = "Y";
                            SROUT.EV_MSG = "";

                            #region 寄送Mail通知
                            CMF.SetSRMailContent(SRCondition.ADD, pOperationID_GenerallySR, pOperationID_InstallSR, pOperationID_MaintainSR, pBUKRS, pSRID, tONEURLName, tAttachURLName, tAttachPath, pLoginName, tIsFormal);
                            #endregion
                        }
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (DbEntityValidationResult e in ex.EntityValidationErrors)
                    {
                        foreach (DbValidationError ve in e.ValidationErrors)
                        {
                            sb.AppendLine($"欄位 {ve.PropertyName} 發生錯誤: {ve.ErrorMessage}");
                        }
                    }

                    pMsg = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + sb.ToString() + Environment.NewLine;
                    pMsg += " 失敗行數：" + ex.ToString();

                    CMF.writeToLog(pSRID, "SaveMaintainSR_API", pMsg, pLoginName);

                    SROUT.EV_SRID = pSRID;
                    SROUT.EV_MSGT = "E";
                    SROUT.EV_MSG = pMsg;
                }
                #endregion
            }

            return SROUT;
        }
        #endregion      

        #region 定維服務案件主檔INPUT資訊
        /// <summary>定維服務案件主檔INPUT資訊</summary>
        public struct SRMain_MAINTAINSR_INPUT
        {
            /// <summary>建立者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>客戶ID</summary>
            public string IV_CUSTOMER { get; set; }
            /// <summary>服務團隊ID</summary>
            public string IV_SRTEAM { get; set; }
            /// <summary>合約文件編號</summary>
            public string IV_CONTRACTID { get; set; }            
            /// <summary>服務案件說明</summary>
            public string IV_DESC { get; set; }
            /// <summary>詳細描述</summary>
            public string IV_LTXT { get; set; }
            /// <summary>報修代碼(大類)</summary>
            public string IV_MKIND1 { get; set; }
            /// <summary>報修代碼(中類)</summary>
            public string IV_MKIND2 { get; set; }
            /// <summary>報修代碼(小類)</summary>
            public string IV_MKIND3 { get; set; }
            /// <summary>主要工程師員工編號</summary>
            public string IV_EMPNO { get; set; }
            /// <summary>協助工程師員工編號(可准許多筆)</summary>
            public string IV_ASSEMPNO { get; set; }
            /// <summary>業務人員員工編號</summary>
            public string IV_SALESEMPNO { get; set; }
            /// <summary>業務祕書員工編號</summary>
            public string IV_SECRETARYEMPNO { get; set; }
            /// <summary>檢附文件</summary>
            public HttpPostedFileBase[] IV_ATTACHFiles { get; set; }

            /// <summary>服務案件客戶聯絡人資訊</summary>
            public List<CREATECONTACTINFO> CREATECONTACT_LIST { get; set; }            
        }
        #endregion

        #region 定維服務案件主檔OUTPUT資訊
        /// <summary>定維服務案件主檔OUTPUT資訊</summary>
        public struct SRMain_MAINTAINSR_OUTPUT
        {
            /// <summary>SRID</summary>
            public string EV_SRID { get; set; }
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion        

        #endregion -----↑↑↑↑↑定維服務案件建立 ↑↑↑↑↑----- 

        #region -----↓↓↓↓↓服務案件(一般/裝機/定維)狀態更新 ↓↓↓↓↓-----

        #region ONE SERVICE 服務案件(一般/裝機/定維)狀態更新接口
        /// <summary>
        /// ONE SERVICE 服務案件(一般/裝機/定維)狀態更新接口
        /// </summary>
        /// <param name="beanIN"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_SRSTATUS_UPDATE(SRMain_SRSTATUS_INPUT beanIN)
        {
            #region Json範列格式
            //{
            //     "IV_LOGINEMPNO": "99120894",
            //     "IV_SRID": "612212070001",
            //     "IV_STATUS": "E0005",
            //     "IV_PROCESSWAY" : "Z04",
            //     "IV_ISAPPCLOSE" : "N",
            //     "IV_SRTypeOne" : "ZA01",
            //     "IV_SRTypeSec" : "ZB0101",
            //     "IV_SRTypeThr" : "ZC010101"
            //}
            #endregion

            SRMain_SRSTATUS_OUTPUT SROUT = new SRMain_SRSTATUS_OUTPUT();

            SROUT = SRSTATUS_Update(beanIN);

            return Json(SROUT);
        }
        #endregion

        #region 更新服務案件(一般/裝機/定維)狀態
        private SRMain_SRSTATUS_OUTPUT SRSTATUS_Update(SRMain_SRSTATUS_INPUT bean)
        {
            SRMain_SRSTATUS_OUTPUT SROUT = new SRMain_SRSTATUS_OUTPUT();

            bool tIsFormal = false;
            string tExcType = string.Empty;
            string pLoginName = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string tAttachPath = string.Empty;
            string IV_LOGINEMPNO = string.IsNullOrEmpty(bean.IV_LOGINEMPNO) ? "" : bean.IV_LOGINEMPNO.Trim();
            string IV_SRID = string.IsNullOrEmpty(bean.IV_SRID) ? "" : bean.IV_SRID.Trim();
            string IV_STATUS = string.IsNullOrEmpty(bean.IV_STATUS) ? "" : bean.IV_STATUS.Trim();
            string IV_PROCESSWAY = string.IsNullOrEmpty(bean.IV_PROCESSWAY) ? "" : bean.IV_PROCESSWAY.Trim();
            string IV_ISAPPCLOSE = string.IsNullOrEmpty(bean.IV_ISAPPCLOSE) ? "" : bean.IV_ISAPPCLOSE.Trim();
            string IV_SRTypeOne = string.IsNullOrEmpty(bean.IV_SRTypeOne) ? "" : bean.IV_SRTypeOne.Trim();
            string IV_SRTypeSec = string.IsNullOrEmpty(bean.IV_SRTypeSec) ? "" : bean.IV_SRTypeSec.Trim();
            string IV_SRTypeThr = string.IsNullOrEmpty(bean.IV_SRTypeThr) ? "" : bean.IV_SRTypeThr.Trim();

            if (IV_STATUS.IndexOf("|") >= 0) //「轉單」或「新建」或「狀態不一致」
            {
                string[] tArySTATUS = IV_STATUS.Split('|');

                IV_STATUS = tArySTATUS[0]; //狀態
                tExcType = tArySTATUS[1];  //執行動作
            }

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
            }

            try
            {
                #region 取得系統位址參數相關資訊
                SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

                tIsFormal = ParaBean.IsFormal;

                tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
                tONEURLName = ParaBean.ONEURLName;
                tBPMURLName = ParaBean.BPMURLName;
                tPSIPURLName = ParaBean.PSIPURLName;
                tAttachURLName = ParaBean.AttachURLName;
                tAttachPath = Server.MapPath("~/REPORT");
                #endregion

                SRCondition tCondition = new SRCondition();

                var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == IV_SRID);

                if (beanM != null)
                {
                    #region 判斷寄送Mail的狀態
                    if (tExcType == "ADD") //新建
                    {
                        tCondition = SRCondition.ADD;
                    }
                    else if (tExcType == "TRANS") //轉單
                    {
                        tCondition = SRCondition.TRANS;
                    }
                    else
                    {
                        if (tExcType != "") //舊狀態
                        {
                            tCondition = findSRCondition(beanM.cStatus, tExcType); //從網頁更新                            
                        }
                        else
                        {
                            tCondition = findSRCondition(IV_STATUS, beanM.cStatus); //從APP更新                            
                        }
                    }
                    #endregion

                    beanM.cStatus = IV_STATUS;

                    if (IV_PROCESSWAY != "")
                    {
                        beanM.cSRProcessWay = IV_PROCESSWAY;
                    }

                    if (IV_ISAPPCLOSE != "")
                    {
                        beanM.cIsAPPClose = IV_ISAPPCLOSE;
                    }

                    if (IV_SRTypeOne != "")
                    {
                        beanM.cSRTypeOne = IV_SRTypeOne;
                    }

                    if (IV_SRTypeSec != "")
                    {
                        beanM.cSRTypeSec = IV_SRTypeSec;
                    }

                    if (IV_SRTypeThr != "")
                    {
                        beanM.cSRTypeThr = IV_SRTypeThr;
                    }

                    beanM.ModifiedDate = DateTime.Now;
                    beanM.ModifiedUserName = pLoginName;

                    int result = dbOne.SaveChanges();

                    if (result <= 0)
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "更新失敗！" + Environment.NewLine;
                        CMF.writeToLog(IV_SRID, "SRSTATUS_Update_API", pMsg, pLoginName);

                        SROUT.EV_MSGT = "E";
                        SROUT.EV_MSG = pMsg;
                    }
                    else
                    {
                        SROUT.EV_MSGT = "Y";
                        SROUT.EV_MSG = "";

                        #region 寄送Mail通知
                        CMF.SetSRMailContent(tCondition, pOperationID_GenerallySR, pOperationID_InstallSR, pOperationID_MaintainSR, EmpBean.BUKRS, IV_SRID, tONEURLName, tAttachURLName, tAttachPath, pLoginName, tIsFormal);
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_SRID, "SRSTATUS_Update_API", pMsg, pLoginName);

                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = ex.Message;
            }

            return SROUT;
        }
        #endregion

        #region 判斷目前要執行什麼狀態
        /// <summary>
        /// 判斷目前要執行什麼狀態
        /// </summary>
        /// <param name="cNewStatus">新狀態</param>
        /// <param name="cOldStatus">原狀態</param>
        /// <returns></returns>
        public SRCondition findSRCondition(string cNewStatus, string cOldStatus)
        {
            SRCondition tCondition = new SRCondition();

            if (cNewStatus != cOldStatus)
            {
                switch (cNewStatus)
                {
                    case "E0002": //L2處理中(一般)
                    case "E0003": //報價中(一般)
                    case "E0005": //L3處理中(一般)
                    case "E0008": //裝機中(裝機)
                    case "E0016": //定保處理中(定維)
                        tCondition = SRCondition.SAVE;
                        break;

                    case "E0004": //3rd Party處理中(一般)                            
                        tCondition = SRCondition.THRPARTY;
                        break;

                    case "E0006": //完修(一般)                   
                        tCondition = SRCondition.DONE;
                        break;

                    case "E0007": //技術支援升級(一般)           
                        tCondition = SRCondition.SUPPORT;
                        break;

                    case "E0009": //維修/DOA(裝機)           
                        tCondition = SRCondition.DOA;
                        break;

                    case "E0010": //裝機完成(裝機)           
                        tCondition = SRCondition.INSTALLDONE;
                        break;

                    case "E0012": //HPGCSN 申請(一般)           
                        tCondition = SRCondition.HPGCSN;
                        break;

                    case "E0013": //HPGCSN 完成(一般)
                        tCondition = SRCondition.HPGCSNDONE;
                        break;

                    case "E0014": //駁回(共用)       
                        tCondition = SRCondition.REJECT;
                        break;

                    case "E0015": //取消(共用)
                        tCondition = SRCondition.CANCEL;
                        break;

                    case "E0017": //定保完成(定維)                   
                        tCondition = SRCondition.MAINTAINDONE;
                        break;
                }
            }
            else
            {
                tCondition = SRCondition.SAVE; //保存
            }

            return tCondition;
        }
        #endregion

        #region 服務案件(一般/裝機/定維)狀態更新INPUT資訊
        /// <summary>服務案件(一般/裝機/定維)狀態更新INPUT資訊</summary>
        public struct SRMain_SRSTATUS_INPUT
        {
            /// <summary>修改者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>服務狀態ID</summary>
            public string IV_STATUS { get; set; }
            /// <summary>處理方式</summary>
            public string IV_PROCESSWAY { get; set; }
            /// <summary>是否為APP結案</summary>
            public string IV_ISAPPCLOSE { get; set; }
            /// <summary>報修大類代碼</summary>
            public string IV_SRTypeOne { get; set; }
            /// <summary>報修中類代碼</summary>
            public string IV_SRTypeSec { get; set; }
            /// <summary>報修小類代碼</summary>
            public string IV_SRTypeThr { get; set; }
        }
        #endregion

        #region 服務案件(一般/裝機/定維)狀態更新OUTPUT資訊
        /// <summary>服務案件(一般/裝機/定維)狀態更新OUTPUT資訊</summary>
        public struct SRMain_SRSTATUS_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑服務案件(一般/裝機/定維)狀態更新 ↑↑↑↑↑-----   
       
        #region -----↓↓↓↓↓主要工程師/技術主管/協助工程師異動 ↓↓↓↓↓-----

        #region ONE SERVICE主要工程師異動接口
        /// <summary>
        /// ONE SERVICE主要工程師異動接口
        /// </summary>
        /// <param name="beanIN"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_SRMAINENGINEER_CHANGE(SRMain_SREMPCHANGE_INPUT beanIN)
        {
            #region Json範列格式
            //{
            //     "IV_LOGINEMPNO": "99120894",
            //     "IV_TRANSEMPNO": "99120894",
            //     "IV_SRID": "612212070001"            
            //}
            #endregion

            SRMain_SREMPCHANGE_OUTPUT SROUT = new SRMain_SREMPCHANGE_OUTPUT();

            beanIN.IV_TRANSTYPE = "MAIN";

            SROUT = SREMPCHANGE_CHANGE(beanIN);

            return Json(SROUT);
        }
        #endregion

        #region ONE SERVICE 技術主管異動接口
        /// <summary>
        /// ONE SERVICE 技術主管異動接口
        /// </summary>
        /// <param name="beanIN"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_SRTECHMANAGER_CHANGE(SRMain_SREMPCHANGE_INPUT beanIN)
        {
            #region Json範列格式
            //{
            //     "IV_LOGINEMPNO": "99120894",
            //     "IV_TRANSEMPNO": "99120894;99120023",
            //     "IV_SRID": "612304130001"            
            //}
            #endregion

            SRMain_SREMPCHANGE_OUTPUT SROUT = new SRMain_SREMPCHANGE_OUTPUT();

            beanIN.IV_STATUS = "E0007"; //技術支援升級
            beanIN.IV_TRANSTYPE = "TECH";

            SROUT = SREMPCHANGE_CHANGE(beanIN);

            return Json(SROUT);
        }
        #endregion

        #region ONE SERVICE 協助工程師異動接口
        /// <summary>
        /// ONE SERVICE 協助工程師異動接口
        /// </summary>
        /// <param name="beanIN"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_SRASSENGINEER_CHANGE(SRMain_SREMPCHANGE_INPUT beanIN)
        {
            #region Json範列格式
            //{
            //     "IV_LOGINEMPNO": "99120894",
            //     "IV_TRANSEMPNO": "99120894;99120023",
            //     "IV_SRID": "612304130001"            
            //}
            #endregion

            SRMain_SREMPCHANGE_OUTPUT SROUT = new SRMain_SREMPCHANGE_OUTPUT();

            beanIN.IV_TRANSTYPE = "ASS";

            SROUT = SREMPCHANGE_CHANGE(beanIN);

            return Json(SROUT);
        }
        #endregion      

        #region 人員異動(一般/裝機/定維)狀態
        private SRMain_SREMPCHANGE_OUTPUT SREMPCHANGE_CHANGE(SRMain_SREMPCHANGE_INPUT bean)
        {
            SRMain_SREMPCHANGE_OUTPUT SROUT = new SRMain_SREMPCHANGE_OUTPUT();

            bool tIsFormal = false;
            string tExcType = string.Empty;
            string pLoginName = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string tAttachPath = string.Empty;
            string IV_LOGINEMPNO = string.IsNullOrEmpty(bean.IV_LOGINEMPNO) ? "" : bean.IV_LOGINEMPNO.Trim();
            string IV_TRANSTYPE = string.IsNullOrEmpty(bean.IV_TRANSTYPE) ? "" : bean.IV_TRANSTYPE.Trim();
            string IV_TRANSEMPNO = string.IsNullOrEmpty(bean.IV_TRANSEMPNO) ? "" : bean.IV_TRANSEMPNO.TrimEnd(';');
            string IV_SRID = string.IsNullOrEmpty(bean.IV_SRID) ? "" : bean.IV_SRID.Trim();
            string IV_STATUS = string.IsNullOrEmpty(bean.IV_STATUS) ? "" : bean.IV_STATUS.Trim();

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
            }

            try
            {
                #region 取得系統位址參數相關資訊
                SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

                tIsFormal = ParaBean.IsFormal;

                tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
                tONEURLName = ParaBean.ONEURLName;
                tBPMURLName = ParaBean.BPMURLName;
                tPSIPURLName = ParaBean.PSIPURLName;
                tAttachURLName = ParaBean.AttachURLName;
                tAttachPath = Server.MapPath("~/REPORT");
                #endregion

                SRCondition tCondition = new SRCondition();

                var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == IV_SRID);

                if (beanM != null)
                {
                    #region 判斷寄送Mail的狀態
                    switch (IV_STATUS)
                    {
                        case "E0002": //L2處理中(一般)
                        case "E0003": //報價中(一般)
                        case "E0005": //L3處理中(一般)
                        case "E0008": //裝機中(裝機)
                            tCondition = SRCondition.SAVE;
                            break;

                        case "E0004": //3rd Party處理中(一般)                            
                            tCondition = SRCondition.THRPARTY;
                            break;

                        case "E0006": //完修(一般)                   
                            tCondition = SRCondition.DONE;
                            break;

                        case "E0007": //技術支援升級(一般)           
                            tCondition = SRCondition.SUPPORT;
                            break;

                        case "E0009": //維修/DOA(裝機)           
                            tCondition = SRCondition.DOA;
                            break;

                        case "E0010": //裝機完成(裝機)           
                            tCondition = SRCondition.INSTALLDONE;
                            break;

                        case "E0012": //HPGCSN 申請(一般)           
                            tCondition = SRCondition.HPGCSN;
                            break;

                        case "E0013": //HPGCSN 完成(一般)
                            tCondition = SRCondition.HPGCSNDONE;
                            break;

                        case "E0014": //駁回(共用)       
                            tCondition = SRCondition.REJECT;
                            break;

                        case "E0015": //取消(共用)
                            tCondition = SRCondition.CANCEL;
                            break;

                        default:
                            tCondition = SRCondition.SAVE;
                            break;
                    }
                    #endregion

                    if (IV_TRANSEMPNO != "")
                    {
                        if (IV_TRANSTYPE == "TECH")
                        {
                            beanM.cTechManagerID = IV_TRANSEMPNO;
                        }
                        else if (IV_TRANSTYPE == "MAIN")
                        {
                            beanM.cMainEngineerID = IV_TRANSEMPNO;
                            beanM.cMainEngineerName = CMF.findEmployeeName(IV_TRANSEMPNO);
                        }
                        else if (IV_TRANSTYPE == "ASS")
                        {
                            beanM.cAssEngineerID = IV_TRANSEMPNO;
                        }
                    }

                    if (IV_STATUS != "")
                    {
                        beanM.cStatus = IV_STATUS;
                    }

                    beanM.ModifiedDate = DateTime.Now;
                    beanM.ModifiedUserName = pLoginName;

                    int result = dbOne.SaveChanges();

                    if (result <= 0)
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "更新失敗！" + Environment.NewLine;
                        CMF.writeToLog(IV_SRID, "SREMPCHANGE_CHANGE_API", pMsg, pLoginName);

                        SROUT.EV_MSGT = "E";
                        SROUT.EV_MSG = pMsg;
                    }
                    else
                    {
                        SROUT.EV_MSGT = "Y";
                        SROUT.EV_MSG = "";

                        #region 寄送Mail通知
                        CMF.SetSRMailContent(tCondition, pOperationID_GenerallySR, pOperationID_InstallSR, pOperationID_MaintainSR, EmpBean.BUKRS, IV_SRID, tONEURLName, tAttachURLName, tAttachPath, pLoginName, tIsFormal);
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_SRID, "SREMPCHANGE_CHANGE_API", pMsg, pLoginName);

                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = ex.Message;
            }

            return SROUT;
        }
        #endregion

        #region 人員異動INPUT資訊
        /// <summary>人員異動INPUT資訊</summary>
        public struct SRMain_SREMPCHANGE_INPUT
        {
            /// <summary>修改者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>類型(TECH.技術主管 MAIN.主要工程師 ASS.協助工程師)</summary>
            public string IV_TRANSTYPE { get; set; }
            /// <summary>(主要工程師/技術主管/協助工程師)員工編號ERPID(若有多人以分號隔開)</summary>
            public string IV_TRANSEMPNO { get; set; }
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>服務狀態ID</summary>
            public string IV_STATUS { get; set; }
        }
        #endregion

        #region 人員異動OUTPUT資訊
        /// <summary>人員異動OUTPUT資訊</summary>
        public struct SRMain_SREMPCHANGE_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑主要工程師/技術主管/協助工程師 ↑↑↑↑↑-----    

        #region -----↓↓↓↓↓法人客戶資料 ↓↓↓↓↓-----

        #region 測試取得法人客戶資料
        //[HttpPost]
        //public ActionResult callAPI_CUSTOMERINFO_GET(CUSTOMERINFO_INPUT beanIN)
        //{
        //    var beanList = GetAPI_CUSTOMERINFO_GET(beanIN);

        //    return Json(beanList);
        //}

        ///// <summary>
        ///// 測試取得法人客戶資料
        ///// </summary>
        ///// <param name="beanIN"></param>
        //public CUSTOMERINFO_OUTPUT GetAPI_CUSTOMERINFO_GET(CUSTOMERINFO_INPUT beanIN)
        //{
        //    CUSTOMERINFO_OUTPUT OUTBean = new CUSTOMERINFO_OUTPUT();

        //    try
        //    {
        //        var client = new RestClient("http://api-qas.etatung.com/API/API_CUSTOMERINFO_GET");  //測試用            

        //        var request = new RestRequest();
        //        request.Method = RestSharp.Method.Post;

        //        Dictionary<Object, Object> parameters = new Dictionary<Object, Object>();
        //        parameters.Add("IV_CUSTOME", beanIN.IV_CUSTOME);

        //        request.AddHeader("Content-Type", "application/json");
        //        request.AddParameter("application/json", parameters, ParameterType.RequestBody);

        //        RestResponse response = client.Execute(request);

        //        #region 取得回傳訊息(成功或失敗)
        //        var data = (JObject)JsonConvert.DeserializeObject(response.Content);

        //        OUTBean.EV_MSGT = data["EV_MSGT"].ToString().Trim();
        //        OUTBean.EV_MSG = data["EV_MSG"].ToString().Trim();
        //        #endregion

        //        #region 取得法人客戶資料List
        //        var tList = (JArray)JsonConvert.DeserializeObject(data["CUSTOMERINFO_LIST"].ToString().Trim());

        //        if (tList != null)
        //        {
        //            List<CUSTOMERINFO_LIST> tCustList = new List<CUSTOMERINFO_LIST>();

        //            foreach (JObject bean in tList)
        //            {
        //                CUSTOMERINFO_LIST beanCust = new CUSTOMERINFO_LIST();

        //                beanCust.CUSTOMERID = bean["CUSTOMERID"].ToString().Trim();
        //                beanCust.CUSTOMERNAME = bean["CUSTOMERNAME"].ToString().Trim();

        //                tCustList.Add(beanCust);
        //            }

        //            OUTBean.CUSTOMERINFO_LIST = tCustList;
        //        }
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"callAPI_CUSTOMERINFO_GET Error: {ex}");
        //    }

        //    return OUTBean;
        //}
        #endregion

        #region 查詢法人客戶資料接口
        [HttpPost]
        public ActionResult API_CUSTOMERINFO_GET(CUSTOMERINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //   "IV_COMPID": "T012",
            //    "IV_CUSTOME": "元大證券股份有限公司"
            //}
            #endregion

            CUSTOMERINFO_OUTPUT ListOUT = new CUSTOMERINFO_OUTPUT();

            ListOUT = CUSTOMERINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得法人客戶資料
        private CUSTOMERINFO_OUTPUT CUSTOMERINFO_GET(CUSTOMERINFO_INPUT beanIN)
        {
            CUSTOMERINFO_OUTPUT OUTBean = new CUSTOMERINFO_OUTPUT();

            string IV_COMPID = string.IsNullOrEmpty(beanIN.IV_COMPID) ? "T012" : beanIN.IV_COMPID.Trim();
            string IV_CUSTOME = string.IsNullOrEmpty(beanIN.IV_CUSTOME) ? "" : beanIN.IV_CUSTOME.Trim();

            try
            {
                var tList = CMF.findCUSTOMERINFO(IV_COMPID, IV_CUSTOME);

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無法人客戶資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得法人客戶資料List
                    List<CUSTOMERINFO_LIST> tCustList = new List<CUSTOMERINFO_LIST>();

                    foreach (var bean in tList)
                    {
                        bool tIsExits = checkCustomerIDIsExits(bean.KNA1_KUNNR.Trim(), tCustList);

                        if (!tIsExits)
                        {
                            CUSTOMERINFO_LIST beanCust = new CUSTOMERINFO_LIST();

                            beanCust.COMPID = IV_COMPID;
                            beanCust.CUSTOMERID = bean.KNA1_KUNNR.Trim();
                            beanCust.CUSTOMERNAME = bean.KNA1_NAME1.Trim();
                            beanCust.CUSTOMERCITY = bean.CITY.Trim();
                            beanCust.CUSTOMERADDRESS = bean.STREET.Trim();
                            beanCust.CUSTOMERTEL = bean.TEL.Trim();

                            tCustList.Add(beanCust);
                        }
                    }

                    OUTBean.CUSTOMERINFO_LIST = tCustList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "CUSTOMERINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 檢查法人客戶ID是否重覆
        /// <summary>
        /// 檢查法人客戶ID是否重覆
        /// </summary>
        /// <param name="CUSTOMERID">客戶ID</param>
        /// <param name="tCustList">法人客戶資料OUTPUT資訊清單</param>
        /// <returns></returns>
        private bool checkCustomerIDIsExits(string CUSTOMERID, List<CUSTOMERINFO_LIST> tCustList)
        {
            bool reValue = false;

            foreach (var tList in tCustList)
            {
                if (tList.CUSTOMERID == CUSTOMERID)
                {
                    reValue = true;
                    break;
                }
            }

            return reValue;
        }
        #endregion

        #region 查詢法人客戶資料INPUT資訊
        /// <summary>查詢法人客戶資料資料INPUT資訊</summary>
        public struct CUSTOMERINFO_INPUT
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string IV_COMPID { get; set; }
            /// <summary>法人客戶(統一編號/客戶名稱)</summary>
            public string IV_CUSTOME { get; set; }
        }
        #endregion

        #region 查詢法人客戶資料OUTPUT資訊
        /// <summary>查詢法人客戶資料OUTPUT資訊</summary>
        public struct CUSTOMERINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>法人客戶資料清單</summary>
            public List<CUSTOMERINFO_LIST> CUSTOMERINFO_LIST { get; set; }
        }

        public struct CUSTOMERINFO_LIST
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string COMPID { get; set; }
            /// <summary>客戶代號</summary>
            public string CUSTOMERID { get; set; }
            /// <summary>客戶名稱</summary>
            public string CUSTOMERNAME { get; set; }
            /// <summary>客戶公司城市</summary>
            public string CUSTOMERCITY { get; set; }
            /// <summary>客戶公司地址</summary>
            public string CUSTOMERADDRESS { get; set; }
            /// <summary>客戶公司電話</summary>
            public string CUSTOMERTEL { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑法人客戶資料 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓查詢法人客戶聯絡人資料 ↓↓↓↓↓-----

        #region 查詢法人客戶聯絡人資料接口
        [HttpPost]
        public ActionResult API_CONTACTINFO_GET(CONTACTINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //   "IV_COMPID": "T012",
            //   "IV_CUSTOMEID": "D16151427",
            //   "IV_CONTACTNAME": "",
            //   "IV_CONTACTTEL": "",            
            //   "IV_CONTACTEMAIL": ""
            //}
            #endregion

            CONTACTINFO_OUTPUT ListOUT = new CONTACTINFO_OUTPUT();

            ListOUT = CONTACTINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得法人客戶聯絡人資料
        private CONTACTINFO_OUTPUT CONTACTINFO_GET(CONTACTINFO_INPUT beanIN)
        {
            CONTACTINFO_OUTPUT OUTBean = new CONTACTINFO_OUTPUT();

            try
            {
                string COMPID = string.IsNullOrEmpty(beanIN.IV_COMPID) ? "T012" : beanIN.IV_COMPID.Trim();
                string CUSTOMEID = string.IsNullOrEmpty(beanIN.IV_CUSTOMEID) ? "" : beanIN.IV_CUSTOMEID.Trim();
                string CONTACTNAME = string.IsNullOrEmpty(beanIN.IV_CONTACTNAME) ? "" : beanIN.IV_CONTACTNAME.Trim();
                string CONTACTTEL = string.IsNullOrEmpty(beanIN.IV_CONTACTTEL) ? "" : beanIN.IV_CONTACTTEL.Trim();                
                string CONTACTEMAIL = string.IsNullOrEmpty(beanIN.IV_CONTACTEMAIL) ? "" : beanIN.IV_CONTACTEMAIL.Trim();

                if (CUSTOMEID == "" && CONTACTNAME == "" && CONTACTTEL == "" && CONTACTEMAIL == "")
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "請至少輸入一項查詢條件！";
                }
                else
                {
                    var tList = CMF.findCONTACTINFO(COMPID, CUSTOMEID, CONTACTNAME, CONTACTTEL, CONTACTEMAIL);

                    if (tList.Count == 0)
                    {
                        OUTBean.EV_MSGT = "E";
                        OUTBean.EV_MSG = "查無法人客戶聯絡人資料，請重新查詢！";
                    }
                    else
                    {
                        OUTBean.EV_MSGT = "Y";
                        OUTBean.EV_MSG = "";

                        #region 取得法人客戶資料List
                        List<CONTACTINFO_LIST> tCustList = new List<CONTACTINFO_LIST>();

                        foreach (var bean in tList)
                        {
                            CONTACTINFO_LIST beanCust = new CONTACTINFO_LIST();

                            beanCust.COMPID = bean.BUKRS;
                            beanCust.CUSTOMERID = bean.CustomerID;
                            beanCust.CUSTOMERNAME = bean.CustomerName;
                            beanCust.CONTACTNAME = bean.Name;
                            beanCust.CONTACTCITY = bean.City;
                            beanCust.CONTACTADDRESS = bean.Address;
                            beanCust.CONTACTTEL = bean.Phone;
                            beanCust.CONTACTMOBILE = bean.Mobile;
                            beanCust.CONTACTEMAIL = bean.Email;
                            beanCust.CONTACTSTORE = bean.Store;
                            beanCust.CONTACTSTORENAME = bean.StoreName;

                            tCustList.Add(beanCust);
                        }

                        OUTBean.CONTACTINFO_LIST = tCustList;
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "CONTACTINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢法人客戶聯絡人資料INPUT資訊
        /// <summary>查詢法人客戶聯絡人資料資料INPUT資訊</summary>
        public struct CONTACTINFO_INPUT
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string IV_COMPID { get; set; }
            /// <summary>法人客戶代號</summary>
            public string IV_CUSTOMEID { get; set; }
            /// <summary>聯絡人姓名</summary>
            public string IV_CONTACTNAME { get; set; }
            /// <summary>聯絡人電話/手機</summary>
            public string IV_CONTACTTEL { get; set; }            
            /// <summary>聯絡人Email</summary>
            public string IV_CONTACTEMAIL { get; set; }
        }
        #endregion

        #region 查詢法人客戶聯絡人資料OUTPUT資訊
        /// <summary>查詢法人客戶聯絡人資料OUTPUT資訊</summary>
        public struct CONTACTINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>法人客戶聯絡人資料清單</summary>
            public List<CONTACTINFO_LIST> CONTACTINFO_LIST { get; set; }
        }

        public struct CONTACTINFO_LIST
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string COMPID { get; set; }
            /// <summary>客戶代號</summary>
            public string CUSTOMERID { get; set; }
            /// <summary>客戶名稱</summary>
            public string CUSTOMERNAME { get; set; }
            /// <summary>聯絡人姓名</summary>
            public string CONTACTNAME { get; set; }
            /// <summary>聯絡人城市</summary>
            public string CONTACTCITY { get; set; }
            /// <summary>聯絡人地址</summary>
            public string CONTACTADDRESS { get; set; }
            /// <summary>聯絡人電話</summary>
            public string CONTACTTEL { get; set; }
            /// <summary>聯絡人手機</summary>
            public string CONTACTMOBILE { get; set; }
            /// <summary>聯絡人Email</summary>
            public string CONTACTEMAIL { get; set; }
            /// <summary>聯絡人門市代號</summary>
            public string CONTACTSTORE { get; set; }
            /// <summary>聯絡人門市名稱</summary>
            public string CONTACTSTORENAME { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑查詢法人客戶聯絡人資料 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓法人客戶聯絡人資料建立/修改 ↓↓↓↓↓-----

        #region 法人客戶聯絡人資料新增接口
        [HttpPost]
        public ActionResult API_CONTACT_CREATE(CONTACTCREATE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //    "IV_COMPID": "T012",
            //    "IV_CUSTOMEID": "D16151427",
            //    "IV_CONTACTNAME": "張豐穎",
            //    "IV_CONTACTCITY": "台中市",
            //    "IV_CONTACTADDRESS": "南屯區五權西路二段236號6樓之1",
            //    "IV_CONTACTTEL": "04-24713300",
            //    "IV_CONTACTMOBILE": "0972",
            //    "IV_CONTACTEMAIL": "elvis.chang@etatung.com",
            //    "IV_CONTACTSTORE": ""            
            //}
            #endregion            

            var bean = SaveCONTACT(beanIN);

            return Json(bean);
        }
        #endregion

        #region 法人客戶聯絡人資料更新接口
        [HttpPost]
        public ActionResult API_CONTACT_UPDATE(CONTACTCREATE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //    "IV_COMPID": "T012",
            //    "IV_CUSTOMEID": "D16151427",
            //    "IV_CONTACTNAME": "張豐穎",
            //    "IV_CONTACTCITY": "台中市",
            //    "IV_CONTACTADDRESS": "南屯區五權西路二段236號6樓之1",
            //    "IV_CONTACTTEL": "04-24713300",
            //    "IV_CONTACTMOBILE": "0972",
            //    "IV_CONTACTEMAIL": "elvis.chang@etatung.com",
            //    "IV_CONTACTSTORE": "",            
            //    "IV_ISDELETE": "N"
            //}
            #endregion            

            var bean = SaveCONTACT(beanIN);

            return Json(bean);
        }
        #endregion

        #region 儲存法人客戶聯絡人資料
        /// <summary>
        /// 儲存法人客戶聯絡人資料
        /// </summary>
        /// <param name="beanIN">傳入資料</param>        
        /// <returns></returns>
        private CONTACTCREATE_OUTPUT SaveCONTACT(CONTACTCREATE_INPUT beanIN)
        {
            CONTACTCREATE_OUTPUT SROUT = new CONTACTCREATE_OUTPUT();

            string tBpmNo = "GenerallySR";            
            string pLoginName = string.Empty;

            string cBUKRS = string.IsNullOrEmpty(beanIN.IV_COMPID) ? "T012" : beanIN.IV_COMPID.Trim();
            string CCustomerName = CMF.findCustName(beanIN.IV_CUSTOMEID);
            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_CUSTOMEID = string.IsNullOrEmpty(beanIN.IV_CUSTOMEID) ? "" : beanIN.IV_CUSTOMEID.Trim();
            string IV_CONTACTNAME = string.IsNullOrEmpty(beanIN.IV_CONTACTNAME) ? "" : beanIN.IV_CONTACTNAME.Trim();
            string IV_CONTACTCITY = string.IsNullOrEmpty(beanIN.IV_CONTACTCITY) ? "" : beanIN.IV_CONTACTCITY.Trim();
            string IV_CONTACTADDRESS = string.IsNullOrEmpty(beanIN.IV_CONTACTADDRESS) ? "" : beanIN.IV_CONTACTADDRESS.Trim();
            string IV_CONTACTTEL = string.IsNullOrEmpty(beanIN.IV_CONTACTTEL) ? "" : beanIN.IV_CONTACTTEL.Trim();
            string IV_CONTACTMOBILE = string.IsNullOrEmpty(beanIN.IV_CONTACTMOBILE) ? "" : beanIN.IV_CONTACTMOBILE.Trim();
            string IV_CONTACTEMAIL = string.IsNullOrEmpty(beanIN.IV_CONTACTEMAIL) ? "" : beanIN.IV_CONTACTEMAIL.Trim();
            string IV_CONTACTSTORE = string.IsNullOrEmpty(beanIN.IV_CONTACTSTORE) ? "" : beanIN.IV_CONTACTSTORE.Trim();
            string IV_ISDELETE = string.IsNullOrEmpty(beanIN.IV_ISDELETE) ? "" : beanIN.IV_ISDELETE.Trim();

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                cBUKRS = string.IsNullOrEmpty(cBUKRS) ? EmpBean.BUKRS : cBUKRS;
            }

            try
            {
                #region 註解
                //var bean = dbProxy.CUSTOMER_Contact.FirstOrDefault(x => (x.Disabled == null || x.Disabled != 1) &&
                //                                                     x.ContactName != "" && x.ContactCity != "" && x.ContactAddress != "" &&
                //                                                    (x.ContactPhone != "" || (x.ContactMobile != "" && x.ContactMobile != null)) &&
                //                                                    x.BpmNo == tBpmNo && x.KNB1_BUKRS == cBUKRS &&
                //                                                    x.KNA1_KUNNR == beanIN.IV_CUSTOMEID.Trim() && x.ContactName == beanIN.IV_CONTACTNAME.Trim());
                #endregion

                var bean = dbProxy.CUSTOMER_Contact.FirstOrDefault(x => (x.Disabled == null || x.Disabled != 1) && 
                                                                     x.ContactName != "" && x.ContactCity != "" && x.ContactAddress != "" &&                                                                    
                                                                    x.BpmNo == tBpmNo && x.KNB1_BUKRS == cBUKRS &&
                                                                    x.KNA1_KUNNR == beanIN.IV_CUSTOMEID.Trim() && x.ContactName == beanIN.IV_CONTACTNAME.Trim());

                if (bean != null) //修改
                {
                    bean.ContactCity = IV_CONTACTCITY;
                    bean.ContactAddress = IV_CONTACTADDRESS;
                    bean.ContactPhone = IV_CONTACTTEL;
                    bean.ContactMobile = IV_CONTACTMOBILE;
                    bean.ContactEmail = IV_CONTACTEMAIL;

                    if (IV_CONTACTSTORE != "")
                    {
                        bean.ContactStore = Guid.Parse(IV_CONTACTSTORE);
                    }

                    if (IV_ISDELETE == "Y")
                    {
                        bean.Disabled = 1;
                    }

                    bean.ModifiedUserName = pLoginName;
                    bean.ModifiedDate = DateTime.Now;
                }
                else //新增
                {
                    CUSTOMER_Contact bean1 = new CUSTOMER_Contact();

                    bean1.ContactID = Guid.NewGuid();
                    bean1.KNA1_KUNNR = IV_CUSTOMEID;
                    bean1.KNA1_NAME1 = CCustomerName;
                    bean1.KNB1_BUKRS = cBUKRS;
                    bean1.ContactType = "5"; //One Service
                    bean1.ContactName = IV_CONTACTNAME;
                    bean1.ContactCity = IV_CONTACTCITY;
                    bean1.ContactAddress = IV_CONTACTADDRESS;
                    bean1.ContactPhone = IV_CONTACTTEL;
                    bean1.ContactMobile = IV_CONTACTMOBILE;
                    bean1.ContactEmail = IV_CONTACTEMAIL;

                    if (IV_CONTACTSTORE != "")
                    {
                        bean.ContactStore = Guid.Parse(IV_CONTACTSTORE);
                    }

                    bean1.BpmNo = tBpmNo;
                    bean1.Disabled = 0;

                    bean1.ModifiedUserName = pLoginName;
                    bean1.ModifiedDate = DateTime.Now;

                    dbProxy.CUSTOMER_Contact.Add(bean1);
                }

                var result = dbProxy.SaveChanges();

                if (result <= 0)
                {
                    if (IV_ISDELETE == "Y") //刪除
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "刪除失敗！請確認要刪除的聯絡人，是透過TICC或One Service建立的才可以刪除！" + Environment.NewLine;
                    }
                    else
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "儲存失敗！請重新確認傳入的法人客戶資料是否有誤！" + Environment.NewLine;
                    }

                    CMF.writeToLog("", "SaveCONTACT_API", pMsg, pLoginName);

                    SROUT.EV_MSGT = "E";
                    SROUT.EV_MSG = pMsg;
                }
                else
                {
                    SROUT.EV_MSGT = "Y";
                    SROUT.EV_MSG = "";
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "SaveCONTACT_API", pMsg, pLoginName);

                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = ex.Message;
            }

            return SROUT;
        }
        #endregion

        #region 法人客戶聯絡人資料新增INPUT資訊
        /// <summary>法人客戶聯絡人資料新增資料INPUT資訊</summary>
        public struct CONTACTCREATE_INPUT
        {
            /// <summary>建立者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string IV_COMPID { get; set; }
            /// <summary>法人客戶代號</summary>
            public string IV_CUSTOMEID { get; set; }
            /// <summary>聯絡人姓名</summary>
            public string IV_CONTACTNAME { get; set; }
            /// <summary>聯絡人城市</summary>
            public string IV_CONTACTCITY { get; set; }
            /// <summary>聯絡人地址</summary>
            public string IV_CONTACTADDRESS { get; set; }
            /// <summary>聯絡人電話</summary>
            public string IV_CONTACTTEL { get; set; }
            /// <summary>聯絡人手機</summary>
            public string IV_CONTACTMOBILE { get; set; }
            /// <summary>聯絡人Email</summary>
            public string IV_CONTACTEMAIL { get; set; }
            /// <summary>聯絡人門市代號</summary>
            public string IV_CONTACTSTORE { get; set; }
            /// <summary>聯絡人門市名稱</summary>
            public string IV_CONTACTSTORENAME { get; set; }
            /// <summary>是否要刪除</summary>
            public string IV_ISDELETE { get; set; }
        }
        #endregion

        #region 法人客戶聯絡人資料新增OUTPUT資訊
        /// <summary>法人客戶聯絡人資料新增OUTPUT資訊</summary>
        public struct CONTACTCREATE_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑法人客戶聯絡人資料/修改 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓個人客戶資料 ↓↓↓↓↓-----        

        #region 查詢個人客戶資料接口
        [HttpPost]
        public ActionResult API_PERSONALINFO_GET(PERSONALINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_COMPID": "T012",
            //    "IV_PERSONAL": "張豐穎"
            //}
            #endregion

            PERSONALINFO_OUTPUT ListOUT = new PERSONALINFO_OUTPUT();

            ListOUT = PERSONALINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得個人客戶資料
        private PERSONALINFO_OUTPUT PERSONALINFO_GET(PERSONALINFO_INPUT beanIN)
        {
            PERSONALINFO_OUTPUT OUTBean = new PERSONALINFO_OUTPUT();

            string IV_COMPID = string.IsNullOrEmpty(beanIN.IV_COMPID) ? "T012" : beanIN.IV_COMPID.Trim();
            string IV_PERSONAL = string.IsNullOrEmpty(beanIN.IV_PERSONAL) ? "" : beanIN.IV_PERSONAL.Trim();

            try
            {
                var tList = CMF.findPERSONALINFO(IV_COMPID, IV_PERSONAL);

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無個人客戶資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得個人客戶資料List
                    List<PERSONALINFO_LIST> tCustList = new List<PERSONALINFO_LIST>();

                    foreach (var bean in tList)
                    {
                        bool tIsExits = checkPersonalIDIsExits(bean.KNA1_KUNNR.Trim(), tCustList);

                        if (!tIsExits)
                        {
                            PERSONALINFO_LIST beanCust = new PERSONALINFO_LIST();

                            beanCust.COMPID = bean.KNB1_BUKRS;
                            beanCust.PERSONALID = bean.KNA1_KUNNR.Trim();
                            beanCust.PERSONALNAME = bean.KNA1_NAME1.Trim();
                            beanCust.PERSONALCITY = bean.ContactCity.Trim();
                            beanCust.PERSONALADDRESS = bean.ContactAddress.Trim();
                            beanCust.PERSONALTEL = bean.ContactPhone.Trim();
                            beanCust.PERSONALMOBILE = bean.ContactMobile.Trim();

                            tCustList.Add(beanCust);
                        }
                    }

                    OUTBean.PERSONALINFO_LIST = tCustList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "PERSONALINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 檢查個人客戶ID是否重覆
        /// <summary>
        /// 檢查個人客戶ID是否重覆
        /// </summary>
        /// <param name="PERSONALID">個人客戶ID</param>
        /// <param name="tCustList">個人客戶資料OUTPUT資訊清單</param>
        /// <returns></returns>
        private bool checkPersonalIDIsExits(string PERSONALID, List<PERSONALINFO_LIST> tCustList)
        {
            bool reValue = false;

            foreach (var tList in tCustList)
            {
                if (tList.PERSONALID == PERSONALID)
                {
                    reValue = true;
                    break;
                }
            }

            return reValue;
        }
        #endregion

        #region 查詢個人客戶資料INPUT資訊
        /// <summary>查詢個人客戶資料資料INPUT資訊</summary>
        public struct PERSONALINFO_INPUT
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string IV_COMPID { get; set; }
            /// <summary>個人客戶(個人編號/客戶名稱)</summary>
            public string IV_PERSONAL { get; set; }
        }
        #endregion

        #region 查詢個人客戶資料OUTPUT資訊
        /// <summary>查詢個人客戶資料OUTPUT資訊</summary>
        public struct PERSONALINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>個人客戶資料清單</summary>
            public List<PERSONALINFO_LIST> PERSONALINFO_LIST { get; set; }
        }

        public struct PERSONALINFO_LIST
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string COMPID { get; set; }
            /// <summary>個人客戶代號</summary>
            public string PERSONALID { get; set; }
            /// <summary>個人客戶名稱</summary>
            public string PERSONALNAME { get; set; }
            /// <summary>個人客戶城市</summary>
            public string PERSONALCITY { get; set; }
            /// <summary>個人客戶地址</summary>
            public string PERSONALADDRESS { get; set; }
            /// <summary>個人客戶電話</summary>
            public string PERSONALTEL { get; set; }
            /// <summary>個人客戶手機</summary>
            public string PERSONALMOBILE { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑個人客戶資料 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓查詢個人客戶聯絡人資料 ↓↓↓↓↓-----

        #region 查詢個人客戶聯絡人資料接口
        [HttpPost]
        public ActionResult API_PERSONALCONTACTINFO_GET(PERSONALCONTACTINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //   "IV_COMPID": "T012",
            //   "IV_PERSONALID": "P00000001",
            //   "IV_CONTACTNAME": "",
            //   "IV_CONTACTTEL": "",
            //   "IV_CONTACTMOBILE": "",
            //   "IV_CONTACTEMAIL": ""
            //}
            #endregion

            PERSONALCONTACTINFO_OUTPUT ListOUT = new PERSONALCONTACTINFO_OUTPUT();

            ListOUT = PERSONALCONTACTINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得個人客戶聯絡人資料
        private PERSONALCONTACTINFO_OUTPUT PERSONALCONTACTINFO_GET(PERSONALCONTACTINFO_INPUT beanIN)
        {
            PERSONALCONTACTINFO_OUTPUT OUTBean = new PERSONALCONTACTINFO_OUTPUT();

            try
            {
                string COMPID = string.IsNullOrEmpty(beanIN.IV_COMPID) ? "T012" : beanIN.IV_COMPID.Trim();
                string PERSONALID = string.IsNullOrEmpty(beanIN.IV_PERSONALID) ? "" : beanIN.IV_PERSONALID.Trim();
                string CONTACTNAME = string.IsNullOrEmpty(beanIN.IV_CONTACTNAME) ? "" : beanIN.IV_CONTACTNAME.Trim();
                string CONTACTTEL = string.IsNullOrEmpty(beanIN.IV_CONTACTTEL) ? "" : beanIN.IV_CONTACTTEL.Trim();
                string CONTACTMOBILE = string.IsNullOrEmpty(beanIN.IV_CONTACTMOBILE) ? "" : beanIN.IV_CONTACTMOBILE.Trim();
                string CONTACTEMAIL = string.IsNullOrEmpty(beanIN.IV_CONTACTEMAIL) ? "" : beanIN.IV_CONTACTEMAIL.Trim();

                if (PERSONALID == "" && CONTACTNAME == "" && CONTACTTEL == "" && CONTACTMOBILE == "" && CONTACTEMAIL == "")
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "請至少輸入一項查詢條件！";
                }
                else
                {
                    var tList = CMF.findPERSONALCONTACTINFO(COMPID, PERSONALID, CONTACTNAME, CONTACTTEL, CONTACTMOBILE, CONTACTEMAIL);

                    if (tList.Count == 0)
                    {
                        OUTBean.EV_MSGT = "E";
                        OUTBean.EV_MSG = "查無個人客戶聯絡人資料，請重新查詢！";
                    }
                    else
                    {
                        OUTBean.EV_MSGT = "Y";
                        OUTBean.EV_MSG = "";

                        #region 取得個人客戶資料List
                        List<PERSONALCONTACTINFO_LIST> tCustList = new List<PERSONALCONTACTINFO_LIST>();

                        foreach (var bean in tList)
                        {
                            PERSONALCONTACTINFO_LIST beanCust = new PERSONALCONTACTINFO_LIST();

                            beanCust.COMPID = bean.BUKRS;
                            beanCust.PERSONALID = bean.CustomerID;
                            beanCust.PERSONALNAME = bean.CustomerName;
                            beanCust.CONTACTNAME = bean.Name;
                            beanCust.CONTACTCITY = bean.City;
                            beanCust.CONTACTADDRESS = bean.Address;
                            beanCust.CONTACTTEL = bean.Phone;
                            beanCust.CONTACTMOBILE = bean.Mobile;
                            beanCust.CONTACTEMAIL = bean.Email;

                            tCustList.Add(beanCust);
                        }

                        OUTBean.PERSONALCONTACTINFO_LIST = tCustList;
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "PERSONALCONTACTINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢個人客戶聯絡人資料INPUT資訊
        /// <summary>查詢個人客戶聯絡人資料資料INPUT資訊</summary>
        public struct PERSONALCONTACTINFO_INPUT
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string IV_COMPID { get; set; }
            /// <summary>個人客戶代號</summary>
            public string IV_PERSONALID { get; set; }
            /// <summary>聯絡人姓名</summary>
            public string IV_CONTACTNAME { get; set; }
            /// <summary>聯絡人電話</summary>
            public string IV_CONTACTTEL { get; set; }
            /// <summary>聯絡人手機</summary>
            public string IV_CONTACTMOBILE { get; set; }
            /// <summary>聯絡人Email</summary>
            public string IV_CONTACTEMAIL { get; set; }
        }
        #endregion

        #region 查詢個人客戶聯絡人資料OUTPUT資訊
        /// <summary>查詢個人客戶聯絡人資料OUTPUT資訊</summary>
        public struct PERSONALCONTACTINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>個人客戶聯絡人資料清單</summary>
            public List<PERSONALCONTACTINFO_LIST> PERSONALCONTACTINFO_LIST { get; set; }
        }

        public struct PERSONALCONTACTINFO_LIST
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string COMPID { get; set; }
            /// <summary>個人客戶代號</summary>
            public string PERSONALID { get; set; }
            /// <summary>個人客戶名稱</summary>
            public string PERSONALNAME { get; set; }
            /// <summary>聯絡人姓名</summary>
            public string CONTACTNAME { get; set; }
            /// <summary>聯絡人城市</summary>
            public string CONTACTCITY { get; set; }
            /// <summary>聯絡人地址</summary>
            public string CONTACTADDRESS { get; set; }
            /// <summary>聯絡人電話</summary>
            public string CONTACTTEL { get; set; }
            /// <summary>聯絡人手機</summary>
            public string CONTACTMOBILE { get; set; }
            /// <summary>聯絡人Email</summary>
            public string CONTACTEMAIL { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑查詢個人客戶聯絡人資料 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓個人客戶聯絡人資料建立/修改 ↓↓↓↓↓-----

        #region 個人客戶聯絡人資料新增接口
        [HttpPost]
        public ActionResult API_PERSONALCONTACT_CREATE(PERSONALCONTACTCREATE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //    "IV_COMPID": "T012",
            //    "IV_PERSONALNAME": "個人客戶-田巧如",
            //    "IV_CONTACTNAME": "田巧如",
            //    "IV_CONTACTCITY": "台中市",
            //    "IV_CONTACTADDRESS": "南屯區五權西路二段236號6樓之2",
            //    "IV_CONTACTTEL": "04-24713300",
            //    "IV_CONTACTMOBILE": "0928",
            //    "IV_CONTACTEMAIL": "Cara.Tien@etatung.com"
            //}
            #endregion

            var bean = SavePERSONALCONTACT(beanIN);

            return Json(bean);
        }
        #endregion

        #region 個人客戶聯絡人資料更新接口
        [HttpPost]
        public ActionResult API_PERSONALCONTACT_UPDATE(PERSONALCONTACTCREATE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //    "IV_COMPID": "T012",
            //    "IV_PERSONALID": "P00000003",
            //    "IV_PERSONALNAME": "個人客戶-田巧如",
            //    "IV_CONTACTNAME": "田巧如",
            //    "IV_CONTACTCITY": "台中市",
            //    "IV_CONTACTADDRESS": "南屯區五權西路二段236號6樓之2",
            //    "IV_CONTACTTEL": "04-24713300#111",
            //    "IV_CONTACTMOBILE": "0928000000",
            //    "IV_CONTACTEMAIL": "Cara.Tien@etatung.com",
            //    "IV_ISDELETE": "N"
            //}
            #endregion            

            var bean = SavePERSONALCONTACT(beanIN);

            return Json(bean);
        }
        #endregion

        #region 儲存個人客戶聯絡人資料
        /// <summary>
        /// 儲存個人客戶聯絡人資料
        /// </summary>
        /// <param name="beanIN">傳入資料</param>        
        /// <returns></returns>
        private PERSONALCONTACTCREATE_OUTPUT SavePERSONALCONTACT(PERSONALCONTACTCREATE_INPUT beanIN)
        {
            PERSONALCONTACTCREATE_OUTPUT SROUT = new PERSONALCONTACTCREATE_OUTPUT();
            
            string pLoginName = string.Empty;

            string cBUKRS = string.IsNullOrEmpty(beanIN.IV_COMPID) ? "T012" : beanIN.IV_COMPID.Trim();
            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_PERSONALID = string.IsNullOrEmpty(beanIN.IV_PERSONALID) ? "" : beanIN.IV_PERSONALID.Trim();
            string IV_PERSONALNAME = string.IsNullOrEmpty(beanIN.IV_PERSONALNAME) ? "" : beanIN.IV_PERSONALNAME.Trim();
            string IV_CONTACTNAME = string.IsNullOrEmpty(beanIN.IV_CONTACTNAME) ? "" : beanIN.IV_CONTACTNAME.Trim();
            string IV_CONTACTCITY = string.IsNullOrEmpty(beanIN.IV_CONTACTCITY) ? "" : beanIN.IV_CONTACTCITY.Trim();
            string IV_CONTACTADDRESS = string.IsNullOrEmpty(beanIN.IV_CONTACTADDRESS) ? "" : beanIN.IV_CONTACTADDRESS.Trim();
            string IV_CONTACTTEL = string.IsNullOrEmpty(beanIN.IV_CONTACTTEL) ? "" : beanIN.IV_CONTACTTEL.Trim();
            string IV_CONTACTMOBILE = string.IsNullOrEmpty(beanIN.IV_CONTACTMOBILE) ? "" : beanIN.IV_CONTACTMOBILE.Trim();
            string IV_CONTACTEMAIL = string.IsNullOrEmpty(beanIN.IV_CONTACTEMAIL) ? "" : beanIN.IV_CONTACTEMAIL.Trim();

            string IV_ISDELETE = string.IsNullOrEmpty(beanIN.IV_ISDELETE) ? "" : beanIN.IV_ISDELETE.Trim();

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                cBUKRS = string.IsNullOrEmpty(cBUKRS) ? EmpBean.BUKRS : cBUKRS;
            }

            try
            {
                if (IV_PERSONALID != "") //修改
                {
                    var bean = dbProxy.PERSONAL_Contact.FirstOrDefault(x => x.Disabled == 0 && x.KNB1_BUKRS == cBUKRS &&
                                                                        x.KNA1_KUNNR == IV_PERSONALID && x.ContactName == beanIN.IV_CONTACTNAME.Trim());

                    if (bean != null)
                    {
                        bean.KNA1_NAME1 = IV_PERSONALNAME;
                        bean.ContactCity = IV_CONTACTCITY;
                        bean.ContactAddress = IV_CONTACTADDRESS;
                        bean.ContactPhone = IV_CONTACTTEL;
                        bean.ContactMobile = IV_CONTACTMOBILE;
                        bean.ContactEmail = IV_CONTACTEMAIL;

                        if (IV_ISDELETE == "Y")
                        {
                            bean.Disabled = 1;
                        }

                        bean.ModifiedUserName = pLoginName;
                        bean.ModifiedDate = DateTime.Now;
                    }
                }
                else //新增
                {
                    var bean = dbProxy.PERSONAL_Contact.FirstOrDefault(x => x.Disabled == 0 && x.KNB1_BUKRS == cBUKRS &&
                                                                        x.KNA1_NAME1 == IV_PERSONALNAME && x.ContactName == beanIN.IV_CONTACTNAME.Trim());

                    if (bean == null)
                    {
                        PERSONAL_Contact bean1 = new PERSONAL_Contact();

                        bean1.ContactID = Guid.NewGuid();
                        bean1.KNA1_KUNNR = CMF.findPERSONALISerialID();
                        bean1.KNA1_NAME1 = IV_PERSONALNAME;
                        bean1.KNB1_BUKRS = cBUKRS;
                        bean1.ContactName = IV_CONTACTNAME;
                        bean1.ContactCity = IV_CONTACTCITY;
                        bean1.ContactAddress = IV_CONTACTADDRESS;
                        bean1.ContactPhone = IV_CONTACTTEL;
                        bean1.ContactMobile = IV_CONTACTMOBILE;
                        bean1.ContactEmail = IV_CONTACTEMAIL;
                        bean1.Disabled = 0;

                        bean1.CreatedUserName = pLoginName;
                        bean1.CreatedDate = DateTime.Now;

                        dbProxy.PERSONAL_Contact.Add(bean1);
                    }
                }

                var result = dbProxy.SaveChanges();

                if (result <= 0)
                {
                    if (IV_ISDELETE == "Y") //刪除
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "刪除失敗！請確認要刪除的聯絡人，是透過TICC或One Service建立的才可以刪除！" + Environment.NewLine;
                    }
                    else
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "儲存失敗！請重新確認傳入的個人客戶資料是否有誤或已存在！" + Environment.NewLine;
                    }

                    CMF.writeToLog("", "SavePERSONALCONTACT_API", pMsg, pLoginName);

                    SROUT.EV_MSGT = "E";
                    SROUT.EV_MSG = pMsg;
                }
                else
                {
                    SROUT.EV_MSGT = "Y";
                    SROUT.EV_MSG = "";
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "SavePERSONALCONTACT_API", pMsg, pLoginName);

                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = ex.Message;
            }

            return SROUT;
        }
        #endregion

        #region 個人客戶聯絡人資料新增INPUT資訊
        /// <summary>個人客戶聯絡人資料新增資料INPUT資訊</summary>
        public struct PERSONALCONTACTCREATE_INPUT
        {
            /// <summary>建立者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string IV_COMPID { get; set; }
            /// <summary>個人客戶代號</summary>
            public string IV_PERSONALID { get; set; }
            /// <summary>個人客戶名稱</summary>
            public string IV_PERSONALNAME { get; set; }
            /// <summary>聯絡人姓名</summary>
            public string IV_CONTACTNAME { get; set; }
            /// <summary>聯絡人城市</summary>
            public string IV_CONTACTCITY { get; set; }
            /// <summary>聯絡人地址</summary>
            public string IV_CONTACTADDRESS { get; set; }
            /// <summary>聯絡人電話</summary>
            public string IV_CONTACTTEL { get; set; }
            /// <summary>聯絡人手機</summary>
            public string IV_CONTACTMOBILE { get; set; }
            /// <summary>聯絡人Email</summary>
            public string IV_CONTACTEMAIL { get; set; }
            /// <summary>是否要刪除</summary>
            public string IV_ISDELETE { get; set; }
        }
        #endregion

        #region 個人客戶聯絡人資料新增OUTPUT資訊
        /// <summary>個人客戶聯絡人資料新增OUTPUT資訊</summary>
        public struct PERSONALCONTACTCREATE_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑個人客戶聯絡人資料/修改 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓客戶報修進度查詢 ↓↓↓↓↓-----

        #region 客戶報修進度查詢接口
        [HttpPost]
        public ActionResult API_SRPROGRESSByCUSTOMER_GET(PROGRESS_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //   "IV_CUSTOMERID": "D97176270",
            //   "IV_CONTACTNAME": "張豐穎",
            //   "IV_STATUS": "ALL"            
            //}
            #endregion

            PROGRESS_OUTPUT ListOUT = new PROGRESS_OUTPUT();

            ListOUT = PROGRESS_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得客戶報修進度資料
        private PROGRESS_OUTPUT PROGRESS_GET(PROGRESS_INPUT beanIN)
        {
            PROGRESS_OUTPUT OUTBean = new PROGRESS_OUTPUT();

            try
            {
                string IV_CUSTOMERID = string.IsNullOrEmpty(beanIN.IV_CUSTOMERID) ? "" : beanIN.IV_CUSTOMERID.Trim();
                string IV_CONTACTNAME = string.IsNullOrEmpty(beanIN.IV_CONTACTNAME) ? "" : beanIN.IV_CONTACTNAME.Trim();
                string IV_STATUS = string.IsNullOrEmpty(beanIN.IV_STATUS) ? "" : beanIN.IV_STATUS.Trim();

                if (IV_CUSTOMERID == "")
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "客戶代號不得為空！";
                }
                else if (IV_STATUS == "")
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "狀態不得為空！";
                }
                else
                {
                    var tList = CMF.findPROGRESSByCustomer(IV_CUSTOMERID, IV_CONTACTNAME, IV_STATUS);

                    if (tList.Count == 0)
                    {
                        OUTBean.EV_MSGT = "E";
                        OUTBean.EV_MSG = "查無客戶報修進度資料，請重新查詢！";
                    }
                    else
                    {
                        OUTBean.EV_MSGT = "Y";
                        OUTBean.EV_MSG = "";
                        OUTBean.PROGRESS_LIST = tList;
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "PROGRESS_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 客戶報修進度查詢INPUT資訊
        /// <summary>客戶報修進度查詢資料INPUT資訊</summary>
        public struct PROGRESS_INPUT
        {
            /// <summary>法人客戶代號</summary>
            public string IV_CUSTOMERID { get; set; }
            /// <summary>報修人/聯絡人姓名</summary>
            public string IV_CONTACTNAME { get; set; }
            /// <summary>狀態(ALL.所有狀態、P.處理中、F.完修)</summary>
            public string IV_STATUS { get; set; }
        }
        #endregion

        #region 客戶報修進度查詢OUTPUT資訊
        /// <summary>客戶報修進度查詢OUTPUT資訊</summary>
        public struct PROGRESS_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>報修進度清單</summary>
            public List<PROGRESS_LIST> PROGRESS_LIST { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑客戶報修進度查詢 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓序號相關資訊查詢(產品序號資訊、保固SLA資訊(List)、服務案件資訊(List)、服務案件客戶聯絡人資訊(List)) ↓↓↓↓↓-----

        #region 查詢序號相關資訊接口
        [HttpPost]
        public ActionResult API_SERIALINFO_SEARCH(SERIALSEARCH_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_SERIAL": "SGH33223R6"            
            //}
            #endregion

            SERIALSEARCH_OUTPUT ListOUT = new SERIALSEARCH_OUTPUT();

            ListOUT = SERIALINFO_SEARCH(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得序號相關資訊
        private SERIALSEARCH_OUTPUT SERIALINFO_SEARCH(SERIALSEARCH_INPUT beanIN)
        {
            SERIALSEARCH_OUTPUT SROUT = new SERIALSEARCH_OUTPUT();

            bool tIsFormal = false;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string[] ArySERIAL = new string[1];

            #region 取得系統位址參數相關資訊
            SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

            tIsFormal = ParaBean.IsFormal;

            tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
            tONEURLName = ParaBean.ONEURLName;
            tBPMURLName = ParaBean.BPMURLName;
            tPSIPURLName = ParaBean.PSIPURLName;
            tAttachURLName = ParaBean.AttachURLName;
            #endregion

            #region 取得產品序號資訊
            var ProBean = CMF.findMaterialBySerial(beanIN.IV_SERIAL.Trim());

            if (ProBean.IV_SERIAL != null)
            {
                SROUT.EV_SERIAL = ProBean.IV_SERIAL;
                SROUT.EV_PRDID = ProBean.ProdID;
                SROUT.EV_PRDNAME = ProBean.Product;
                SROUT.EV_PRDNUMBER = ProBean.MFRPN;
                SROUT.EV_INSTALLID = ProBean.InstallNo;
                SROUT.EV_MSGT = "Y";
                SROUT.EV_MSG = "";
            }
            else
            {
                SROUT.EV_SERIAL = beanIN.IV_SERIAL.Trim();
                SROUT.EV_PRDID = "";
                SROUT.EV_PRDNAME = "";
                SROUT.EV_PRDNUMBER = "";
                SROUT.EV_INSTALLID = "";
                SROUT.EV_MSGT = "Y";
                SROUT.EV_MSG = "";
            }
            #endregion

            if (beanIN.IV_SERIAL.Trim() != "")
            {
                tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;

                #region 保固SLA資訊(List)
                List<SRWarranty> QueryToList = new List<SRWarranty>();    //查詢出來的清單                

                ArySERIAL[0] = beanIN.IV_SERIAL.Trim();

                QueryToList = CMF.ZFM_TICC_SERIAL_SEARCHWTYList(ArySERIAL, tBPMURLName, tONEURLName, tAPIURLName);
                QueryToList = QueryToList.OrderBy(x => x.SERIALID).ThenByDescending(x => x.WTYEDATE).ToList();

                SROUT.WTSLA_LIST = QueryToList;
                #endregion

                #region 服務案件主檔資訊清單
                List<SRIDINFO> QuerySRToList = new List<SRIDINFO>();    //查詢出來的清單

                QuerySRToList = CMF.findSRMAINList(pOperationID_GenerallySR, beanIN.IV_SERIAL.Trim());

                SROUT.SRMAIN_LIST = QuerySRToList;
                #endregion

                #region 服務案件客戶聯絡人資訊清單

                #region 先取得SRID清單
                List<string> tSRIDList = new List<string>();

                if (QuerySRToList.Count > 0)
                {
                    foreach (var SRBean in QuerySRToList)
                    {
                        if (!tSRIDList.Contains(SRBean.SRID))
                        {
                            tSRIDList.Add(SRBean.SRID);
                        }
                    }
                }
                #endregion

                #region 再執行查詢
                List<SRCONTACTINFO> QuerySRCONTACTToList = new List<SRCONTACTINFO>();    //查詢出來的清單

                QuerySRCONTACTToList = CMF.findSRCONTACTList(tSRIDList);

                SROUT.SRCONTACT_LIST = QuerySRCONTACTToList;
                #endregion

                #endregion
            }

            return SROUT;
        }
        #endregion

        #region 序號查詢INPUT資訊
        /// <summary>序號查詢INPUT資訊</summary>
        public struct SERIALSEARCH_INPUT
        {
            /// <summary>序號ID</summary>
            public string IV_SERIAL { get; set; }
        }
        #endregion

        #region 序號查詢OUTPUT資訊
        /// <summary>序號查詢OUTPUT資訊</summary>
        public struct SERIALSEARCH_OUTPUT
        {
            /// <summary>序號ID</summary>
            public string EV_SERIAL { get; set; }
            /// <summary>料號</summary>
            public string EV_PRDID { get; set; }
            /// <summary>料號說明</summary>
            public string EV_PRDNAME { get; set; }
            /// <summary>製造商零件號碼</summary>
            public string EV_PRDNUMBER { get; set; }
            /// <summary>裝機號碼</summary>
            public string EV_INSTALLID { get; set; }
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>保固SLA資訊清單</summary>
            public List<SRWarranty> WTSLA_LIST { get; set; }
            /// <summary>服務案件主檔資訊清單</summary>
            public List<SRIDINFO> SRMAIN_LIST { get; set; }
            /// <summary>服務案件客戶聯絡人資訊</summary>
            public List<SRCONTACTINFO> SRCONTACT_LIST { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑序號相關資訊查詢(產品序號資訊、保固SLA資訊(List)、服務案件資訊(List)、服務案件客戶聯絡人資訊(List)) ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓序號查詢【保固SLA資訊】接口 ↓↓↓↓↓----- 

        #region 查詢序號相關資訊接口
        [HttpPost]
        public ActionResult API_WTSLAINFO_SEARCH(WTSLASEARCH_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_SERIAL": "SGH33223R6"            
            //}
            #endregion

            WTSLASEARCH_OUTPUT ListOUT = new WTSLASEARCH_OUTPUT();

            ListOUT = WTSLAINFO_SEARCH(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得序號相關資訊
        private WTSLASEARCH_OUTPUT WTSLAINFO_SEARCH(WTSLASEARCH_INPUT beanIN)
        {
            WTSLASEARCH_OUTPUT SROUT = new WTSLASEARCH_OUTPUT();

            bool tIsFormal = false;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string[] ArySERIAL = new string[1];

            string IV_SERIAL = string.IsNullOrEmpty(beanIN.IV_SERIAL) ? "" : beanIN.IV_SERIAL.Trim();

            #region 取得系統位址參數相關資訊
            SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

            tIsFormal = ParaBean.IsFormal;

            tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
            tONEURLName = ParaBean.ONEURLName;
            tBPMURLName = ParaBean.BPMURLName;
            tPSIPURLName = ParaBean.PSIPURLName;
            tAttachURLName = ParaBean.AttachURLName;
            #endregion           

            try
            {
                if (IV_SERIAL != "")
                {
                    tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;

                    #region 保固SLA資訊(List)
                    List<SRWarranty> QueryToList = new List<SRWarranty>();    //查詢出來的清單                

                    ArySERIAL[0] = beanIN.IV_SERIAL.Trim();

                    QueryToList = CMF.ZFM_TICC_SERIAL_SEARCHWTYList(ArySERIAL, tBPMURLName, tONEURLName, tAPIURLName);
                    QueryToList = QueryToList.OrderBy(x => x.SERIALID).ThenByDescending(x => x.WTYEDATE).ToList();

                    SROUT.WTSLA_LIST = QueryToList;
                    #endregion

                    SROUT.EV_MSGT = "Y";
                    SROUT.EV_MSG = "";
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "WTSLAINFO_SEARCH_API", pMsg, "SYS");

                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = ex.Message;
            }

            return SROUT;
        }
        #endregion

        #region 序號查詢保固SLAINPUT資訊
        /// <summary>序號查詢保固SLAINPUT資訊</summary>
        public struct WTSLASEARCH_INPUT
        {
            /// <summary>序號ID</summary>
            public string IV_SERIAL { get; set; }
        }
        #endregion

        #region 序號查詢保固SLAOUTPUT資訊
        /// <summary>序號查詢保固SLAOUTPUT資訊</summary>
        public struct WTSLASEARCH_OUTPUT
        {           
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>保固SLA資訊清單</summary>
            public List<SRWarranty> WTSLA_LIST { get; set; }         
        }
        #endregion

        #endregion -----↑↑↑↑↑序號查詢【保固SLA資訊】接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓序號查詢【料號和料號說明】接口 ↓↓↓↓↓-----        

        #region 查詢料號資料接口
        [HttpPost]
        public ActionResult API_MATERIALINFOBySERIAL_GET(MATERIALINFOBySERIAL_INPUT beanIV)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_SERIAL": "SGH33223R6"          
            //}
            #endregion

            MATERIALINFOBySERIAL_OUTPUT ListOUT = new MATERIALINFOBySERIAL_OUTPUT();

            ListOUT = MATERIALINFOBySERIAL_GET(beanIV);

            return Json(ListOUT);
        }
        #endregion

        #region 取得料號資料
        private MATERIALINFOBySERIAL_OUTPUT MATERIALINFOBySERIAL_GET(MATERIALINFOBySERIAL_INPUT beanIN)
        {
            MATERIALINFOBySERIAL_OUTPUT OUTBean = new MATERIALINFOBySERIAL_OUTPUT();

            try
            {
                string IV_SERIAL = string.IsNullOrEmpty(beanIN.IV_SERIAL) ? "" : beanIN.IV_SERIAL.Trim();

                var tList = CMF.findMaterialBySerialList(IV_SERIAL);

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無料號資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得料號資料List
                    List<MATERIAL_LIST> tMATList = new List<MATERIAL_LIST>();

                    foreach (var bean in tList)
                    {
                        MATERIAL_LIST beanTEAM = new MATERIAL_LIST();

                        beanTEAM.EV_SERIAL = bean.IV_SERIAL;    //序號
                        beanTEAM.EV_PRDID = bean.ProdID;        //料號ID
                        beanTEAM.EV_PRDNAME = bean.Product;     //料號說明

                        tMATList.Add(beanTEAM);
                    }

                    OUTBean.MATERIAL_LIST = tMATList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "MATERIALINFOBySERIAL_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢料號資料INPUT資訊
        /// <summary>查詢料號資料INPUT資訊</summary>
        public struct MATERIALINFOBySERIAL_INPUT
        {
            /// <summary>序號ID</summary>
            public string IV_SERIAL { get; set; }
        }
        #endregion

        #region 查詢下拉選項清單OUTPUT資訊
        /// <summary>查詢下拉選項清單OUTPUT資訊</summary>
        public struct MATERIALINFOBySERIAL_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>物料清單</summary>
            public List<MATERIAL_LIST> MATERIAL_LIST { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑序號查詢【料號和料號說明】接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓SRID相關資訊查詢(服務主檔資訊、客戶聯絡窗口資訊清單、產品序號資訊清單、保固SLA檔資訊清單、處理與工時紀錄清單、零件更換資訊清單、物料訊息資訊清單、序號回報資訊清單) ↓↓↓↓↓-----

        #region 查詢SRID相關資訊接口
        [HttpPost]
        public ActionResult API_SRIDINFO_SEARCH(SRIDSEARCH_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_SRID": "612211250004"            
            //}
            #endregion

            SRIDSEARCH_OUTPUT ListOUT = new SRIDSEARCH_OUTPUT();

            ListOUT = SRIDINFO_SEARCH(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得SRID相關資訊
        private SRIDSEARCH_OUTPUT SRIDINFO_SEARCH(SRIDSEARCH_INPUT beanIN)
        {
            SRIDSEARCH_OUTPUT SROUT = new SRIDSEARCH_OUTPUT();

            bool tIsFormal = false;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string cStatusDesc = string.Empty;

            #region 取得系統位址參數相關資訊
            SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

            tIsFormal = ParaBean.IsFormal;

            tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
            tONEURLName = ParaBean.ONEURLName;
            tBPMURLName = ParaBean.BPMURLName;
            tPSIPURLName = ParaBean.PSIPURLName;
            tAttachURLName = ParaBean.AttachURLName;
            #endregion           

            #region 取得主檔資訊
            var MainBean = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == beanIN.IV_SRID.Trim());

            if (MainBean.cSRID != null)
            {
                string cBUKRS = CMF.findBUKRSByTeamID(MainBean.cTeamID);

                if (beanIN.IV_SRID.Trim().Substring(0, 2) == "61")
                {
                    cStatusDesc = CMF.findSysParameterDescription(pOperationID_GenerallySR, "OTHER", cBUKRS, "SRSTATUS", MainBean.cStatus);
                }
                else if (beanIN.IV_SRID.Trim().Substring(0, 2) == "63")
                {
                    cStatusDesc = CMF.findSysParameterDescription(pOperationID_InstallSR, "OTHER", cBUKRS, "SRSTATUS", MainBean.cStatus);
                }
                else if (beanIN.IV_SRID.Trim().Substring(0, 2) == "65")
                {
                    cStatusDesc = CMF.findSysParameterDescription(pOperationID_MaintainSR, "OTHER", cBUKRS, "SRSTATUS", MainBean.cStatus);
                }

                string cMASERVICETYPE = string.IsNullOrEmpty(MainBean.cMAServiceType) ? "" : MainBean.cMAServiceType;
                string cMAServiceTypeDesc = CMF.findSysParameterDescription(pOperationID_GenerallySR, "OTHER", cBUKRS, "SRMATYPE", MainBean.cMAServiceType);
                string cSRTYPEONE = string.IsNullOrEmpty(MainBean.cSRTypeOne) ? "" : MainBean.cSRTypeOne;
                string cSRTYPEONEDesc = CMF.findSRRepairTypeName(MainBean.cSRTypeOne);
                string cSRTYPESEC = string.IsNullOrEmpty(MainBean.cSRTypeSec) ? "" : MainBean.cSRTypeSec;
                string cSRTYPESECDesc = CMF.findSRRepairTypeName(MainBean.cSRTypeSec);
                string cSRTYPETHR = string.IsNullOrEmpty(MainBean.cSRTypeThr) ? "" : MainBean.cSRTypeThr;
                string cSRTYPETHRDesc = CMF.findSRRepairTypeName(MainBean.cSRTypeThr);
                string cSRPATHWAY = string.IsNullOrEmpty(MainBean.cSRPathWay) ? "" : MainBean.cSRPathWay;
                string cSRPATHWAYDesc = CMF.findSysParameterDescription(pOperationID_GenerallySR, "OTHER", cBUKRS, "SRPATH", MainBean.cSRPathWay);
                string cSRPROCESSWAY = string.IsNullOrEmpty(MainBean.cSRProcessWay) ? "" : MainBean.cSRProcessWay;
                string cSRPROCESSWAYDesc = CMF.findSysParameterDescription(pOperationID_GenerallySR, "OTHER", cBUKRS, "SRPROCESS", MainBean.cSRProcessWay);
                string cISSECFIX = string.IsNullOrEmpty(MainBean.cIsSecondFix) ? "" : MainBean.cIsSecondFix;
                string cISSECFIXDesc = MainBean.cIsSecondFix == "Y" ? "是" : "否";
                string cSRTEAM = CMF.findSRTeamIDandName(MainBean.cTeamID);
                string cMAINENG = CMF.findSREMPERPIDandNameByERPID(MainBean.cMainEngineerID);
                string cASSENGN = CMF.findSREMPERPIDandNameByERPID(MainBean.cAssEngineerID);
                string cTECHMAG = CMF.findSREMPERPIDandNameByERPID(MainBean.cTechManagerID);
                string cSALES = CMF.findSREMPERPIDandNameByERPID(MainBean.cSalesID);
                string cSECRETARY = CMF.findSREMPERPIDandNameByERPID(MainBean.cSecretaryID);
                string cAttachURL = CMF.findAttachUrlWithName(MainBean.cAttachement, tAttachURLName);
                string cAttachStockNo = CMF.findAttachUrlWithName(MainBean.cAttachementStockNo, tAttachURLName);
                string cREPAIRNAME = string.IsNullOrEmpty(MainBean.cRepairName) ? "" : MainBean.cRepairName;
                string cREPAIRADDR = string.IsNullOrEmpty(MainBean.cRepairAddress) ? "" : MainBean.cRepairAddress;
                string cREPAIRTEL = string.IsNullOrEmpty(MainBean.cRepairPhone) ? "" : MainBean.cRepairPhone;
                string cREPAIRMOB = string.IsNullOrEmpty(MainBean.cRepairMobile) ? "" : MainBean.cRepairMobile;
                string cREPAIREMAIL = string.IsNullOrEmpty(MainBean.cRepairEmail) ? "" : MainBean.cRepairEmail;
                string cSQEMP = string.IsNullOrEmpty(MainBean.cSQPersonID) ? "" : MainBean.cSQPersonID;
                string cSALESNO = string.IsNullOrEmpty(MainBean.cSalesNo) ? "" : MainBean.cSalesNo;
                string cSHIPMENTNO = string.IsNullOrEmpty(MainBean.cShipmentNo) ? "" : MainBean.cShipmentNo;

                #region 客戶故障狀況分類
                string cFaultGroup = string.IsNullOrEmpty(MainBean.cFaultGroup) ? "" : MainBean.cFaultGroup.TrimEnd(';');
                string cFaultGroupNote1 = string.IsNullOrEmpty(MainBean.cFaultGroupNote1) ? "" : MainBean.cFaultGroupNote1;
                string cFaultGroupNote2 = string.IsNullOrEmpty(MainBean.cFaultGroupNote2) ? "" : MainBean.cFaultGroupNote2;
                string cFaultGroupNote3 = string.IsNullOrEmpty(MainBean.cFaultGroupNote3) ? "" : MainBean.cFaultGroupNote3;
                string cFaultGroupNote4 = string.IsNullOrEmpty(MainBean.cFaultGroupNote4) ? "" : MainBean.cFaultGroupNote4;
                string cFaultGroupNoteOther = string.IsNullOrEmpty(MainBean.cFaultGroupNoteOther) ? "" : MainBean.cFaultGroupNoteOther;
                #endregion

                #region 客戶故障狀況
                string cFaultState = string.IsNullOrEmpty(MainBean.cFaultState) ? "" : MainBean.cFaultState.TrimEnd(';');
                string cFaultStateNote1 = string.IsNullOrEmpty(MainBean.cFaultStateNote1) ? "" : MainBean.cFaultStateNote1;
                string cFaultStateNote2 = string.IsNullOrEmpty(MainBean.cFaultStateNote2) ? "" : MainBean.cFaultStateNote2;
                string cFaultStateNoteOther = string.IsNullOrEmpty(MainBean.cFaultStateNoteOther) ? "" : MainBean.cFaultStateNoteOther;
                #endregion

                #region 故障零件規格料號
                string cFaultSpec = string.IsNullOrEmpty(MainBean.cFaultSpec) ? "" : MainBean.cFaultSpec.TrimEnd(';');
                string cFaultSpecNote1 = string.IsNullOrEmpty(MainBean.cFaultSpecNote1) ? "" : MainBean.cFaultSpecNote1;
                string cFaultSpecNote2 = string.IsNullOrEmpty(MainBean.cFaultSpecNote2) ? "" : MainBean.cFaultSpecNote2;
                string cFaultSpecNoteOther = string.IsNullOrEmpty(MainBean.cFaultSpecNoteOther) ? "" : MainBean.cFaultSpecNoteOther;
                #endregion                

                string[] cArySLA = CMF.findSRSLACondition(MainBean.cSRID);
                string cSLARESP = cArySLA[0];
                string cSLASRV = cArySLA[1];

                SROUT.SRID = MainBean.cSRID;
                SROUT.STATUS = MainBean.cStatus + "_" + cStatusDesc;
                SROUT.DESC = MainBean.cDesc;
                SROUT.NOTES = MainBean.cNotes;
                SROUT.CUSTOMER = MainBean.cCustomerID + "_" + MainBean.cCustomerName;
                SROUT.MASERVICETYPE = string.IsNullOrEmpty(cMASERVICETYPE) ? "" : cMASERVICETYPE + "_" + cMAServiceTypeDesc;
                SROUT.CRDATE = Convert.ToDateTime(MainBean.CreatedDate).ToString("yyyy-MM-dd");
                SROUT.SRTYPEONE = string.IsNullOrEmpty(cSRTYPEONE) ? "" : cSRTYPEONE + "_" + cSRTYPEONEDesc;
                SROUT.SRTYPESEC = string.IsNullOrEmpty(cSRTYPESEC) ? "" : cSRTYPESEC + "_" + cSRTYPESECDesc;
                SROUT.SRTYPETHR = string.IsNullOrEmpty(cSRTYPETHR) ? "" : cSRTYPETHR + "_" + cSRTYPETHRDesc;
                SROUT.SRPATHWAY = string.IsNullOrEmpty(cSRPATHWAY) ? "" : cSRPATHWAY + "_" + cSRPATHWAYDesc;
                SROUT.SRPROCESSWAY = string.IsNullOrEmpty(cSRPROCESSWAY) ? "" : cSRPROCESSWAY + "_" + cSRPROCESSWAYDesc;
                SROUT.DELAYREASON = MainBean.cDelayReason;
                SROUT.ISSECFIX = string.IsNullOrEmpty(cISSECFIX) ? "" : cISSECFIX + "_" + cISSECFIXDesc;
                SROUT.REPAIRNAME = cREPAIRNAME;
                SROUT.REPAIRADDR = cREPAIRADDR;
                SROUT.REPAIRTEL = cREPAIRTEL;
                SROUT.REPAIRMOB = cREPAIRMOB;
                SROUT.REPAIREMAIL = cREPAIREMAIL;
                SROUT.SRTEAM = cSRTEAM;
                SROUT.MAINENG = cMAINENG;
                SROUT.ASSENGN = cASSENGN;
                SROUT.TECHMAG = cTECHMAG;
                SROUT.SQEMP = string.IsNullOrEmpty(cSQEMP) ? "" : cSQEMP + "_" + MainBean.cSQPersonName;
                SROUT.SALES = cSALES;
                SROUT.ATTACHURL = cAttachURL;
                SROUT.SLARESP = cSLARESP;
                SROUT.SLASRV = cSLASRV;

                #region 客戶故障狀況分類
                SROUT.FAULTGROUP = cFaultGroup;
                SROUT.FAULTGROUPNOTE1 = cFaultGroupNote1;
                SROUT.FAULTGROUPNOTE2 = cFaultGroupNote2;
                SROUT.FAULTGROUPNOTE3 = cFaultGroupNote3;
                SROUT.FAULTGROUPNOTE4 = cFaultGroupNote4;
                SROUT.FAULTGROUPNOTEOTHER = cFaultGroupNoteOther;
                #endregion

                #region 客戶故障狀況
                SROUT.FAULTSTATE = cFaultState;
                SROUT.FAULTSTATENOTE1 = cFaultStateNote1;
                SROUT.FAULTSTATENOTE2 = cFaultStateNote2;
                SROUT.FAULTSTATENOTEOTHER = cFaultStateNoteOther;
                #endregion

                #region 故障零件規格料號
                SROUT.FAULTSPEC = cFaultSpec;
                SROUT.FAULTSPECNOTE1 = cFaultSpecNote1;
                SROUT.FAULTSPECNOTE2 = cFaultSpecNote2;
                SROUT.FAULTSPECNOTEOTHER = cFaultSpecNoteOther;
                #endregion             

                #region (63.裝機用)
                SROUT.SECRETARY = cSECRETARY;
                SROUT.SALESNO = cSALESNO;
                SROUT.SHIPMENTNO = cSHIPMENTNO;
                SROUT.ATTACHSTOCKNOURL = cAttachStockNo;
                #endregion

                SROUT.EV_MSGT = "Y";
                SROUT.EV_MSG = "";
            }
            else
            {
                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = "查無SRID相關資訊！";
            }
            #endregion

            if (MainBean.cSRID != "")
            {
                #region 【客戶聯絡人資訊】清單，(61.一般、63.裝機) 都顯示
                List<SRCONTACTINFO> SRCONTACT_LIST = CMF.findSRCONTACTINFO(MainBean.cSRID);
                SROUT.SRCONTACT_LIST = SRCONTACT_LIST;
                #endregion

                #region 【處理與工時紀錄資訊】清單，(61.一般、63.裝機) 都顯示
                List<SRRECORDINFO> SRRECORD_LIST = CMF.findSRRECORDINFO(MainBean.cSRID, tAttachURLName);
                SROUT.SRRECORD_LIST = SRRECORD_LIST;
                #endregion

                if (MainBean.cSRID.Substring(0, 2) == "61") //一般
                {
                    #region 【產品序號資訊】清單，(61.一般)才顯示
                    List<SRSERIALMATERIALINFO> SRSERIAL_LIST = CMF.findSRSERIALMATERIALINFO(MainBean.cSRID);
                    SROUT.SRSERIAL_LIST = SRSERIAL_LIST;
                    #endregion

                    #region 【保固SLA資訊】清單，(61.一般)才顯示
                    List<SRWTSLAINFO> SRWTSLA_LIST = CMF.findSRWTSLAINFO(MainBean.cSRID, tBPMURLName, tPSIPURLName);
                    SROUT.SRWTSLA_LIST = SRWTSLA_LIST;
                    #endregion

                    #region 【零件更換資訊】清單，(61.一般)才顯示
                    List<SRPARTSREPALCEINFO> SRPARTS_LIST = CMF.findSRPARTSREPALCEINFO(MainBean.cSRID);
                    SROUT.SRPARTS_LIST = SRPARTS_LIST;
                    #endregion
                }
                else if (MainBean.cSRID.Substring(0, 2) == "63") //裝機
                {
                    #region 【物料訊息資訊】清單，(63.裝機)才顯示
                    List<SRMATERIALlNFO> SRMATERIAL_LIST = CMF.findSRMATERIALlNFO(MainBean.cSRID);
                    SROUT.SRMATERIAL_LIST = SRMATERIAL_LIST;
                    #endregion

                    #region 【序號回報資訊】清單，(63.裝機)才顯示
                    List<SRSERIALFEEDBACKlNFO> SRFEEDBACK_LIST = CMF.findSRSERIALFEEDBACKlNFO(MainBean.cSRID, tAttachURLName);
                    SROUT.SRFEEDBACK_LIST = SRFEEDBACK_LIST;
                    #endregion
                }
            }

            return SROUT;
        }
        #endregion

        #region SRID查詢INPUT資訊
        /// <summary>SRID查詢INPUT資訊</summary>
        public struct SRIDSEARCH_INPUT
        {
            /// <summary>序號ID</summary>
            public string IV_SRID { get; set; }
        }
        #endregion

        #region SRID查詢OUTPUT資訊
        /// <summary>SRID查詢OUTPUT資訊</summary>
        public struct SRIDSEARCH_OUTPUT
        {
            /// <summary>服務案件ID</summary>
            public string SRID { get; set; }
            /// <summary>狀態</summary>
            public string STATUS { get; set; }
            /// <summary>說明</summary>
            public string DESC { get; set; }
            /// <summary>詳細描述</summary>
            public string NOTES { get; set; }
            /// <summary>客戶(</summary>
            public string CUSTOMER { get; set; }
            /// <summary>維護服務種類</summary>
            public string MASERVICETYPE { get; set; }
            /// <summary>建立開單時間</summary>
            public string CRDATE { get; set; }
            /// <summary>報修代碼(大類)</summary>
            public string SRTYPEONE { get; set; }
            /// <summary>報修代碼(中類)</summary>
            public string SRTYPESEC { get; set; }
            /// <summary>報修代碼(小類)</summary>
            public string SRTYPETHR { get; set; }
            /// <summary>報修管道</summary>
            public string SRPATHWAY { get; set; }
            /// <summary>處理方式</summary>
            public string SRPROCESSWAY { get; set; }
            /// <summary>延遲結案原因</summary>
            public string DELAYREASON { get; set; }
            /// <summary>是否為二修</summary>
            public string ISSECFIX { get; set; }
            /// <summary>報修人姓名</summary>
            public string REPAIRNAME { get; set; }
            /// <summary>報修人地址</summary>
            public string REPAIRADDR { get; set; }
            /// <summary>報修人電話</summary>
            public string REPAIRTEL { get; set; }
            /// <summary>報修人手機</summary>
            public string REPAIRMOB { get; set; }
            /// <summary>報修人Email</summary>
            public string REPAIREMAIL { get; set; }
            /// <summary>服務團隊</summary>
            public string SRTEAM { get; set; }
            /// <summary>主要工程師</summary>
            public string MAINENG { get; set; }
            /// <summary>協助工程師</summary>
            public string ASSENGN { get; set; }
            /// <summary>技術主管</summary>
            public string TECHMAG { get; set; }
            /// <summary>SQL人員</summary>
            public string SQEMP { get; set; }
            /// <summary>業務人員</summary>
            public string SALES { get; set; }
            /// <summary>業務祕書</summary>
            public string SECRETARY { get; set; }
            /// <summary>檢附文件URL</summary>
            public string ATTACHURL { get; set; }
            /// <summary>備料服務通知單文件URL</summary>
            public string ATTACHSTOCKNOURL { get; set; }
            /// <summary>回應條件</summary>
            public string SLARESP { get; set; }
            /// <summary>服務條件</summary>
            public string SLASRV { get; set; }
            /// <summary>銷售訂單號(63.裝機用)</summary>
            public string SALESNO { get; set; }
            /// <summary>出貨單號(63.裝機用)</summary>
            public string SHIPMENTNO { get; set; }

            #region 客戶故障狀況分類資訊
            /// <summary>客戶故障狀況分類(Z01.硬體、Z02.系統、Z03.服務、Z04.網路、Z99.其他)</summary>
            public string FAULTGROUP { get; set; }
            /// <summary>客戶故障狀況分類說明-硬體</summary>
            public string FAULTGROUPNOTE1 { get; set; }
            /// <summary>客戶故障狀況分類說明-系統</summary>
            public string FAULTGROUPNOTE2 { get; set; }
            /// <summary>客戶故障狀況分類說明-服務</summary>
            public string FAULTGROUPNOTE3 { get; set; }
            /// <summary>客戶故障狀況分類說明-網路</summary>
            public string FAULTGROUPNOTE4 { get; set; }
            /// <summary>客戶故障狀況分類說明-其他</summary>
            public string FAULTGROUPNOTEOTHER { get; set; }

            /// <summary>客戶故障狀況(Z01.面板燈號、Z02.管理介面(iLO、IMM、iDRAC)、Z99.其他)</summary>
            public string FAULTSTATE { get; set; }
            /// <summary>客戶故障狀況說明-面板燈號</summary>
            public string FAULTSTATENOTE1 { get; set; }
            /// <summary>客戶故障狀況說明-管理介面(iLO、IMM、iDRAC)</summary>
            public string FAULTSTATENOTE2 { get; set; }
            /// <summary>客戶故障狀況說明-其他</summary>
            public string FAULTSTATENOTEOTHER { get; set; }

            /// <summary>故障零件規格料號(Z01.零件規格、Z02.零件料號、Z99.其他)</summary>
            public string FAULTSPEC { get; set; }
            /// <summary>故障零件規格料號說明-零件規格</summary>
            public string FAULTSPECNOTE1 { get; set; }
            /// <summary>故障零件規格料號說明-零件料號</summary>
            public string FAULTSPECNOTE2 { get; set; }
            /// <summary>故障零件規格料號說明-其他</summary>
            public string FAULTSPECNOTEOTHER { get; set; }
            #endregion

            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>服務案件【客戶聯絡人資訊】清單</summary>
            public List<SRCONTACTINFO> SRCONTACT_LIST { get; set; }
            /// <summary>服務案件【產品序號資訊】清單</summary>
            public List<SRSERIALMATERIALINFO> SRSERIAL_LIST { get; set; }
            /// <summary>服務案件【保固SLA資訊】清單</summary>
            public List<SRWTSLAINFO> SRWTSLA_LIST { get; set; }
            /// <summary>服務案件【處理與工時紀錄資訊】清單</summary>
            public List<SRRECORDINFO> SRRECORD_LIST { get; set; }
            /// <summary>服務案件【零件更換資訊】清單</summary>
            public List<SRPARTSREPALCEINFO> SRPARTS_LIST { get; set; }
            /// <summary>服務案件【物料訊息資訊】清單</summary>
            public List<SRMATERIALlNFO> SRMATERIAL_LIST { get; set; }
            /// <summary>服務案件【序號回報資訊】清單</summary>
            public List<SRSERIALFEEDBACKlNFO> SRFEEDBACK_LIST { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑SRID相關資訊查詢(服務主檔資訊、客戶聯絡窗口資訊清單、產品序號資訊清單、保固SLA檔資訊清單、處理與工時紀錄清單、零件更換資訊清單、物料訊息資訊清單、序號回報資訊清單) ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓服務待辦清單查詢接口 ↓↓↓↓↓-----        

        #region 查詢服務待辦清單接口
        [HttpPost]
        public ActionResult API_SRTODOLIST_SEARCH(SRTODOLIST_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_EMPNO": "10000975"
            //}
            #endregion

            SRTODOLIST_OUTPUT ListOUT = new SRTODOLIST_OUTPUT();

            ListOUT = SRTODOLIST_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得服務待辦清單
        private SRTODOLIST_OUTPUT SRTODOLIST_GET(SRTODOLIST_INPUT beanIN)
        {
            SRTODOLIST_OUTPUT OUTBean = new SRTODOLIST_OUTPUT();

            string tERPID = string.Empty;

            try
            {
                #region 取得登入人員資訊
                tERPID = beanIN.IV_EMPNO;

                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(tERPID);
                #endregion                

                #region 一般服務
                //取得登入人員所負責的服務團隊
                List<string> tSRTeamList = CMF.findSRTeamMappingList(EmpBean.CostCenterID, EmpBean.DepartmentNO);

                //取得登入人員所有要負責的SRID                
                List<string[]> tList = CMF.findSRIDList(pOperationID_GenerallySR, pOperationID_InstallSR, pOperationID_MaintainSR, EmpBean.BUKRS, EmpBean.IsManager, EmpBean.EmployeeERPID, tSRTeamList);
                #endregion

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無服務待辦清單，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得員工資料List
                    List<SRTODOLISTINFO> tTODOList = new List<SRTODOLISTINFO>();

                    foreach (string[] tAry in tList)
                    {
                        //狀態非【新建】時的判斷
                        if (tAry[17] != "E0001")
                        {
                            if (tERPID == tAry[12]) //業務人員
                            {
                                continue;
                            }
                            else if (tERPID == tAry[13]) //業務祕書
                            {
                                continue;
                            }

                            if (tAry[10] != "")
                            {
                                if (tAry[17] == "E0007") //若狀態【E0007.技術支援升級】
                                {
                                    if (tAry[10].IndexOf(tERPID) == -1) //【不為技術主管】才跳過
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (tAry[10].IndexOf(tERPID) >= 0) //【為技術主管但非E0007(技術支援升級)】才跳過
                                    {
                                        if (tAry[7] != tERPID) //非【主要工程師】才跳過
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }

                            if (EmpBean.IsManager) //若是主管時
                            {
                                if (tAry[17] != "E0007") //若狀態非【E0007.技術支援升級】
                                {
                                    if (tERPID != tAry[7]) //非【主要工程師】才跳過
                                    {
                                        continue;
                                    }
                                }
                            }
                        }

                        SRTODOLISTINFO beanTODO = new SRTODOLISTINFO();

                        beanTODO.SRID = tAry[0];
                        beanTODO.CUSTOMERNAME = tAry[1];
                        beanTODO.REPAIRNAME = tAry[2];
                        beanTODO.CONTACTNAME = tAry[3];
                        beanTODO.SRDESC = tAry[4];
                        beanTODO.PATHWAY = tAry[5];
                        beanTODO.SRTYPE = tAry[6];
                        beanTODO.MAINENGNAME = tAry[8];
                        beanTODO.ASSENGNAME = tAry[9];
                        beanTODO.TECHMANAGER = tAry[11];
                        beanTODO.SLARESP = tAry[14];
                        beanTODO.SLASRV = tAry[15];
                        beanTODO.MODIFDATE = tAry[16];
                        beanTODO.STATUSDESC = tAry[18];

                        tTODOList.Add(beanTODO);
                    }

                    OUTBean.SRTODOLIST_LIST = tTODOList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "SRTODOLIST_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢服務待辦清單INPUT資訊
        /// <summary>查詢服務待辦清單INPUT資訊</summary>
        public struct SRTODOLIST_INPUT
        {
            /// <summary>員工編號(ERPID)</summary>
            public string IV_EMPNO { get; set; }
        }
        #endregion

        #region 查詢服務待辦清單OUTPUT資訊
        /// <summary>查詢服務待辦清單OUTPUT資訊</summary>
        public struct SRTODOLIST_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>服務待辦清單</summary>
            public List<SRTODOLISTINFO> SRTODOLIST_LIST { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑服務待辦清單查詢接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓服務明細-客戶聯絡窗口資訊更新 ↓↓↓↓↓-----

        #region ONE SERVICE 服務明細-客戶聯絡窗口資訊更新接口
        /// <summary>
        /// ONE SERVICE 技術主管異動接口
        /// </summary>
        /// <param name="beanIN"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_SRDETAILCONTACT_UPDATE(SRMain_SRDETAILCONTACT_INPUT beanIN)
        {
            #region Json範列格式
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //    "IV_CID": "1080",
            //    "IV_SRID" : "632304200001",
            //    "IV_CONTADDR": "臺北市文山區一壽街501號",
            //    "IV_CONTTE": "(04)23300560",
            //    "IV_CONTMOBILE": "0900123456",
            //    "IV_CONTEMAIL": "elvis.chang@etatung.com"
            //}
            #endregion

            SRMain_SRDETAILCONTACT_OUTPUT SROUT = new SRMain_SRDETAILCONTACT_OUTPUT();

            SROUT = SRDETAILCONTACT_UPDATE(beanIN);

            return Json(SROUT);
        }
        #endregion

        #region 客戶聯絡窗口資訊更新
        private SRMain_SRDETAILCONTACT_OUTPUT SRDETAILCONTACT_UPDATE(SRMain_SRDETAILCONTACT_INPUT bean)
        {
            SRMain_SRDETAILCONTACT_OUTPUT SROUT = new SRMain_SRDETAILCONTACT_OUTPUT();

            string pLoginName = string.Empty;
            string cSRID = string.Empty;
            int cID = string.IsNullOrEmpty(bean.IV_CID) ? 0 : int.Parse(bean.IV_CID.Trim());
            string IV_SRID = string.IsNullOrEmpty(bean.IV_SRID) ? "" : bean.IV_SRID.Trim();
            string IV_LOGINEMPNO = string.IsNullOrEmpty(bean.IV_LOGINEMPNO) ? "" : bean.IV_LOGINEMPNO.Trim();
            string IV_CONTNAME = string.IsNullOrEmpty(bean.IV_CONTNAME) ? "" : bean.IV_CONTNAME.Trim();
            string IV_CONTADDR = string.IsNullOrEmpty(bean.IV_CONTADDR) ? "" : bean.IV_CONTADDR.Trim();
            string IV_CONTTE = string.IsNullOrEmpty(bean.IV_CONTTE) ? "" : bean.IV_CONTTE.Trim();
            string IV_CONTMOBILE = string.IsNullOrEmpty(bean.IV_CONTMOBILE) ? "" : bean.IV_CONTMOBILE.Trim();
            string IV_CONTEMAIL = string.IsNullOrEmpty(bean.IV_CONTEMAIL) ? "" : bean.IV_CONTEMAIL.Trim();

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
            }

            try
            {
                if (cID == 0)
                {
                    #region 新增
                    TB_ONE_SRDetail_Contact SRC = new TB_ONE_SRDetail_Contact();

                    cSRID = IV_SRID;
                    SRC.cSRID = IV_SRID;
                    SRC.cContactName = IV_CONTNAME;
                    SRC.cContactAddress = IV_CONTADDR;
                    SRC.cContactPhone = IV_CONTTE;
                    SRC.cContactMobile = IV_CONTMOBILE;
                    SRC.cContactEmail = IV_CONTEMAIL;

                    SRC.Disabled = 0;
                    SRC.CreatedDate = DateTime.Now;
                    SRC.CreatedUserName = pLoginName;

                    dbOne.TB_ONE_SRDetail_Contact.Add(SRC);
                    #endregion
                }
                else
                { 
                    var beanD = dbOne.TB_ONE_SRDetail_Contact.FirstOrDefault(x => x.cID == cID);

                    if (beanD != null)
                    {
                        cSRID = beanD.cSRID;
                        beanD.cContactName = IV_CONTNAME;
                        beanD.cContactAddress = IV_CONTADDR;
                        beanD.cContactPhone = IV_CONTTE;
                        beanD.cContactMobile = IV_CONTMOBILE;
                        beanD.cContactEmail = IV_CONTEMAIL;

                        beanD.ModifiedDate = DateTime.Now;
                        beanD.ModifiedUserName = pLoginName;                        
                    }                    
                }

                int result = dbOne.SaveChanges();

                if (result <= 0)
                {
                    pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "更新失敗！" + Environment.NewLine;
                    CMF.writeToLog(cSRID, "SRDETAILCONTACT_UPDATE_API", pMsg, pLoginName);

                    SROUT.EV_MSGT = "E";
                    SROUT.EV_MSG = pMsg;
                }
                else
                {
                    SROUT.EV_MSGT = "Y";
                    SROUT.EV_MSG = "";

                    if (cID == 0) //新增
                    {
                        var beanC = dbOne.TB_ONE_SRDetail_Contact.OrderByDescending(x => x.cID).FirstOrDefault(x => x.cSRID == cSRID);

                        if (beanC != null)
                        {
                            SROUT.EV_CID = beanC.cID.ToString();
                        }
                    }
                    else
                    {
                        SROUT.EV_CID = cID.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(cSRID, "SRDETAILCONTACT_UPDATE_API", pMsg, pLoginName);

                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = ex.Message;
            }

            return SROUT;
        }
        #endregion

        #region 服務明細-客戶聯絡窗口資訊INPUT資訊
        /// <summary>服務明細-客戶聯絡窗口資訊INPUT資訊</summary>
        public struct SRMain_SRDETAILCONTACT_INPUT
        {
            /// <summary>修改者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>系統ID</summary>
            public string IV_CID { get; set; }
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }            
            /// <summary>聯絡人姓名</summary>
            public string IV_CONTNAME { get; set; }
            /// <summary>聯絡人地址</summary>
            public string IV_CONTADDR { get; set; }
            /// <summary>聯絡人電話</summary>
            public string IV_CONTTE { get; set; }
            /// <summary>聯絡人手機</summary>
            public string IV_CONTMOBILE { get; set; }
            /// <summary>聯絡人信箱</summary>
            public string IV_CONTEMAIL { get; set; }
        }
        #endregion

        #region 服務明細-客戶聯絡窗口資訊OUTPUT資訊
        /// <summary>服務明細-客戶聯絡窗口資訊OUTPUT資訊</summary>
        public struct SRMain_SRDETAILCONTACT_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
            /// <summary>系統ID</summary>
            public string EV_CID { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑服務明細-客戶聯絡窗口資訊更新 ↑↑↑↑↑-----    

        #region -----↓↓↓↓↓異動處理與工時紀錄相關接口 ↓↓↓↓↓-----        

        #region 新增處理與工時紀錄相關接口
        [HttpPost]
        public ActionResult API_SRRECORDINFO_CREATE(SRRECORDINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_SRID": "612212070001",
            //    "IV_EMPNO": "10010298",
            //    "IV_ReceiveTime": "2023-01-18 18:20",
            //    "IV_StartTime": "2023-01-18 18:25",
            //    "IV_ArriveTime": "2023-01-18 18:50",
            //    "IV_FinishTime": "2023-01-18 19:50",
            //    "IV_Desc": "TEST處理紀錄",
            //    "IV_SENDREPORT" : "Y",
            //    "IV_SRReportFileName" : "e0049b93-f077-4ad1-ba93-0968ad992d5b.pdf"
            //}
            #endregion

            SRRECORDINFO_OUTPUT ListOUT = new SRRECORDINFO_OUTPUT();

            ListOUT = SaveSRRECORDINFO(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 刪除處理與工時紀錄相關接口
        [HttpPost]
        public ActionResult API_SRRECORDINFO_DELETE(SRRECORDINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_SRID": "612212070001",
            //    "IV_EMPNO": "10010298",
            //    "IV_CID": "1003"
            //}
            #endregion

            SRRECORDINFO_OUTPUT ListOUT = new SRRECORDINFO_OUTPUT();

            ListOUT = SaveSRRECORDINFO(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 儲存處理與工時紀錄相關
        private SRRECORDINFO_OUTPUT SaveSRRECORDINFO(SRRECORDINFO_INPUT beanIN)
        {
            SRRECORDINFO_OUTPUT OUTBean = new SRRECORDINFO_OUTPUT();

            int cID = 0;

            bool tIsFormal = false;
            string cSRID = string.Empty;
            string cENGID = string.Empty;
            string cENGNAME = string.Empty;
            string cReceiveTime = string.Empty;
            string cStartTime = string.Empty;
            string cArriveTime = string.Empty;
            string cFinishTime = string.Empty;
            string cDesc = string.Empty;
            string cIsInternalWork = string.Empty;  //是否為內部作業
            string cSRReport = string.Empty;
            string cReportID = string.Empty;
            string cSRReportFileName = string.Empty;
            string cPDFPath = string.Empty;         //服務報告書路徑
            string cPDFFileName = string.Empty;     //服務報告書檔名
            string cSENDREPORT = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;

            try
            {
                #region 取得系統位址參數相關資訊
                SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

                tIsFormal = ParaBean.IsFormal;

                tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
                tONEURLName = ParaBean.ONEURLName;
                tBPMURLName = ParaBean.BPMURLName;
                tPSIPURLName = ParaBean.PSIPURLName;
                tAttachURLName = ParaBean.AttachURLName;
                #endregion

                cID = string.IsNullOrEmpty(beanIN.IV_CID) ? 0 : int.Parse(beanIN.IV_CID);
                cSRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID;
                cENGID = string.IsNullOrEmpty(beanIN.IV_EMPNO) ? "" : beanIN.IV_EMPNO;
                cReceiveTime = string.IsNullOrEmpty(beanIN.IV_ReceiveTime) ? "" : beanIN.IV_ReceiveTime;
                cStartTime = string.IsNullOrEmpty(beanIN.IV_StartTime) ? "" : beanIN.IV_StartTime;
                cArriveTime = string.IsNullOrEmpty(beanIN.IV_ArriveTime) ? "" : beanIN.IV_ArriveTime;
                cFinishTime = string.IsNullOrEmpty(beanIN.IV_FinishTime) ? "" : beanIN.IV_FinishTime;
                cDesc = string.IsNullOrEmpty(beanIN.IV_Desc) ? "" : beanIN.IV_Desc;
                cSRReportFileName = string.IsNullOrEmpty(beanIN.IV_SRReportFileName) ? "" : beanIN.IV_SRReportFileName;
                cSENDREPORT = string.IsNullOrEmpty(beanIN.IV_SENDREPORT) ? "" : beanIN.IV_SENDREPORT;
                cIsInternalWork = CMF.checkIsInternalWork(cSRID) ? "Y" : "N";

                #region 取得工程師/技術主管姓名
                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(cENGID);

                cENGNAME = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                #endregion

                if (cID == 0)
                {
                    #region 是否需要寄送服務報告書
                    if (cSENDREPORT == "Y")
                    {
                        #region 有簽名檔、無簽名檔、線上、遠端
                        cSRReport = cSRReportFileName.Replace(".pdf", "") + ",";

                        cPDFPath = Path.Combine(Server.MapPath("~/REPORT"), cSRReportFileName);
                        cPDFFileName = cSRReportFileName;
                        #endregion
                    }
                    else
                    {
                        #region 純附件
                        if (cSRReportFileName != "")
                        {
                            string[] tAryFileName = cSRReportFileName.Split(',');

                            foreach (string tFileName in tAryFileName)
                            {
                                string[] tAryGUID = tFileName.Split('.');
                                cSRReport += tAryGUID[0] + ",";
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region 新增紀錄
                    TB_ONE_SRDetail_Record SRRecord = new TB_ONE_SRDetail_Record();

                    TimeSpan Ts = Convert.ToDateTime(cFinishTime) - Convert.ToDateTime(cArriveTime);
                    double cWorkHours = Math.Ceiling(Ts.TotalMinutes);

                    SRRecord.cSRID = cSRID;
                    SRRecord.cEngineerID = cENGID;
                    SRRecord.cEngineerName = cENGNAME;
                    SRRecord.cReceiveTime = Convert.ToDateTime(cReceiveTime);
                    SRRecord.cStartTime = Convert.ToDateTime(cStartTime);
                    SRRecord.cArriveTime = Convert.ToDateTime(cArriveTime);
                    SRRecord.cFinishTime = Convert.ToDateTime(cFinishTime);
                    SRRecord.cWorkHours = Convert.ToDecimal(cWorkHours);
                    SRRecord.cDesc = cDesc;
                    SRRecord.cSRReport = cSRReport;
                    SRRecord.Disabled = 0;

                    SRRecord.CreatedDate = DateTime.Now;
                    SRRecord.CreatedUserName = cENGNAME;

                    dbOne.TB_ONE_SRDetail_Record.Add(SRRecord);
                    #endregion
                }
                else //刪除
                {
                    #region 刪除
                    var bean = dbOne.TB_ONE_SRDetail_Record.FirstOrDefault(x => x.cID == cID);

                    if (bean != null)
                    {
                        bean.Disabled = 1;

                        bean.ModifiedDate = DateTime.Now;
                        bean.ModifiedUserName = cENGNAME;
                    }
                    #endregion
                }

                var result = dbOne.SaveChanges();

                if (result <= 0)
                {
                    if (cID == 0) //新增
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新增失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                    }
                    else
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "刪除失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                    }

                    CMF.writeToLog(cSRID, "SaveSRRECORDINFO_API", pMsg, cENGNAME);

                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    if (cID == 0) //新增
                    {
                        var bean = dbOne.TB_ONE_SRDetail_Record.OrderByDescending(x => x.cID).FirstOrDefault(x => x.cSRID == cSRID);

                        if (bean != null)
                        {
                            OUTBean.EV_CID = bean.cID.ToString();
                        }
                    }
                    else
                    {
                        OUTBean.EV_CID = cID.ToString();
                    }

                    #region 若為內部作業就不寄送mail給客戶
                    if (cIsInternalWork == "Y")
                    {
                        cSENDREPORT = "N";
                    }
                    #endregion

                    if (cSENDREPORT == "Y")
                    {
                        CMF.callSendReport(pOperationID_GenerallySR, cSRID, cPDFPath, cPDFFileName, cENGNAME, tIsFormal); //呼叫發送服務報告書report給客戶
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(beanIN.IV_SRID, "SaveSRRECORDINFO_API", pMsg, cENGNAME);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
                OUTBean.EV_CID = "";
            }

            return OUTBean;
        }
        #endregion       

        #region 多張圖片上傳並轉成一份pdf
        /// <summary>
        /// 多張圖片上傳並轉成一份pdf
        /// </summary>
        /// <param name="picPathList">圖片路徑清單</param>        
        /// <param name="filename">檔名</param>
        /// <param name="srId">SRID</param>        
        /// <param name="mainEgnrName">處理工程師姓名</param>        
        /// <returns></returns>
        public bool UploadMultPics(List<string> picPathList, string filename, string srId, string mainEgnrName)
        {
            bool reValue = false;

            try
            {
                #region -- 將圖片轉成一份pdf --
                string pdfFileName = filename + ".pdf"; // 正式用                
                string pdfPath = Path.Combine(Server.MapPath("~/REPORT"), pdfFileName);

                iTextSharp.text.Document doc = new iTextSharp.text.Document();
                FileStream fs = new FileStream(pdfPath, FileMode.Create);
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                foreach (var picPath in picPathList)
                {
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(picPath);
                    var imgW = img.Width;
                    var imgH = img.Height;

                    //img.ScaleAbsolute(1200, 1600);
                    img.ScaleToFit(1200, 1600);
                    doc.SetPageSize(new iTextSharp.text.Rectangle(0, 0, 1200, 1600, 0));


					//如果圖片是橫的(寬大於長)
					if (imgW > imgH)
                    {
                        img.RotationDegrees = -90; //counterclockwise逆時針旋轉
                        doc.SetPageSize(new iTextSharp.text.Rectangle(0, 0, img.Height, img.Width, 0));
                    }
                    doc.NewPage();

					// 將圖片居中放置
					float x = (doc.PageSize.Width - img.ScaledWidth) / 2;
					float y = (doc.PageSize.Height - img.ScaledHeight) / 2;
					img.SetAbsolutePosition(x, y);
					//img.SetAbsolutePosition(0, 0);

                    writer.DirectContent.AddImage(img);
                }

                doc.Close();
                reValue = true;
                #endregion

                #region 刪除原圖檔(先不刪，以後可以手動上傳)
                //foreach (var picPath in picPathList)
                //{
                //    bool result = System.IO.File.Exists(picPath);
                //    if (result)
                //    {
                //        System.IO.File.Delete(picPath);
                //    }
                //}
                #endregion               
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(srId, "UploadMultPics_API", pMsg, mainEgnrName);
                CMF.SendMailByAPI("UploadMultPics_API", null, "elvis.chang@etatung.com", "", "", "UploadMultPics錯誤 - " + srId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "<br>" + ex.ToString(), null, null);
            }

            return reValue;
        }
        #endregion        

        #region 異動處理與工時紀錄相關INPUT資訊
        /// <summary>異動處理與工時紀錄相關INPUT資訊</summary>
        public struct SRRECORDINFO_INPUT
        {
            /// <summary>系統ID</summary>
            public string IV_CID { get; set; }
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>服務工程師ERPID/技術主管ERPID</summary>
            public string IV_EMPNO { get; set; }
            /// <summary>接單時間</summary>
            public string IV_ReceiveTime { get; set; }
            /// <summary>出發時間</summary>
            public string IV_StartTime { get; set; }
            /// <summary>到場時間</summary>
            public string IV_ArriveTime { get; set; }
            /// <summary>完成時間</summary>
            public string IV_FinishTime { get; set; }
            /// <summary>工時(分鐘)</summary>
            public string IV_WorkHours { get; set; }
            /// <summary>處理紀錄</summary>
            public string IV_Desc { get; set; }
            /// <summary>是否需要寄送服務報告書</summary>
            public string IV_SENDREPORT { get; set; }
            /// <summary>服務報告書檔名(多檔以逗號隔開)</summary>
            public string IV_SRReportFileName { get; set; }
        }
        #endregion

        #region 異動處理與工時紀錄相關OUTPUT資訊
        /// <summary>異動處理與工時紀錄相關OUTPUT資訊</summary>
        public struct SRRECORDINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
            /// <summary>系統ID</summary>
            public string EV_CID { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑異動處理與工時紀錄相關查詢接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓客戶手寫簽名圖片上傳並產生服務報告書pdf接口 ↓↓↓↓↓-----
        [HttpPost]
        public ActionResult API_SRSIGNPDFINFO_CREATE(SRSIGNPDFINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_SRID": "612212070001",
            //    "IV_EMPNO": "10010298",            
            //    "IV_StartTime": "2023-04-18 18:25",
            //    "IV_ArriveTime": "2023-04-18 18:50",
            //    "IV_FinishTime": "2023-04-18 19:50",
            //    "IV_Desc": "一、問題判斷：test
            //                二、問題處理過程：test
            //                三、結論：test",
            //    "IV_CusOpinion": "沒有意見",
            //    "IV_SRReportType": "SIGN",
            //    "IV_SRReportFile": "FILES" //用form-data傳檔案            
            //    "IV_SRReportFiles": "FILES" //用form-data傳檔案            
            //}
            #endregion

            SRSIGNPDFINFO_OUTPUT ListOUT = new SRSIGNPDFINFO_OUTPUT();

            ListOUT = UploadSignToPdf(beanIN);

            return Json(ListOUT);
        }

        #region 客戶手寫簽名圖片上傳並產生服務報告書pdf        
        public SRSIGNPDFINFO_OUTPUT UploadSignToPdf(SRSIGNPDFINFO_INPUT beanIN)
        {
            SRSIGNPDFINFO_OUTPUT OUTBean = new SRSIGNPDFINFO_OUTPUT();

            string IV_FileName = string.Empty;
            string cSRReport = string.Empty;
            string cReportID = string.Empty;
            string cPDFPath = string.Empty;         //服務報告書路徑
            string cPDFFileName = string.Empty;     //服務報告書檔名
            string IV_SRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID.Trim();
            string IV_StartTime = string.IsNullOrEmpty(beanIN.IV_StartTime) ? "" : beanIN.IV_StartTime.Trim();
            string IV_ArriveTime = string.IsNullOrEmpty(beanIN.IV_ArriveTime) ? "" : beanIN.IV_ArriveTime.Trim();
            string IV_FinishTime = string.IsNullOrEmpty(beanIN.IV_FinishTime) ? "" : beanIN.IV_FinishTime.Trim();
            string IV_Desc = string.IsNullOrEmpty(beanIN.IV_Desc) ? "" : beanIN.IV_Desc.Trim();
            string IV_EMPNO = string.IsNullOrEmpty(beanIN.IV_EMPNO) ? "" : beanIN.IV_EMPNO.Trim();
            string IV_CusOpinion = string.IsNullOrEmpty(beanIN.IV_CusOpinion) ? "" : beanIN.IV_CusOpinion.Trim();
            SRReportType IV_SRReportType = string.IsNullOrEmpty(beanIN.IV_SRReportType.ToString()) ? SRReportType.SIGN : beanIN.IV_SRReportType;

            HttpPostedFileBase upload = beanIN.IV_SRReportFile;
            HttpPostedFileBase[] uploadFiles = beanIN.IV_SRReportFiles;

            #region 取得執行人員姓名            
            string IV_EMPNONAME = string.Empty; //執行人員姓名

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_EMPNO);

            IV_EMPNONAME = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
            #endregion          

            #region -- 儲存上傳資料內容到資料庫 --
            TB_SERVICES_APP_STATE appBean = new TB_SERVICES_APP_STATE();

            try
            {
                if (upload != null)
                {
                    IV_FileName = upload.FileName;
                }
                else if (uploadFiles != null)
                {
                    foreach (var upload1 in uploadFiles)
                    {
                        if (upload1 != null)
                        {
                            IV_FileName += upload1.FileName + ",";
                        }
                    }

                    IV_FileName = IV_FileName.TrimEnd(',');
                }

                //local端測試時暫時註解
                appBean.SRID = IV_SRID;
                appBean.STATE = IV_SRID + " | " + IV_FileName + " | " + IV_StartTime + " | " + IV_ArriveTime + " | " + IV_FinishTime + " | " + IV_Desc + " | " + IV_EMPNO;
                appBean.INSERT_TIME = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                dbEIP.TB_SERVICES_APP_STATE.Add(appBean);

                int saveStatus = dbEIP.SaveChanges();
            }
            catch (Exception ex)
            {
                IV_SRID = IV_SRID == "" || IV_SRID == null ? "" : IV_SRID;
                appBean.STATE = IV_SRID + " | " + ex.Message;
                dbEIP.TB_SERVICES_APP_STATE.Add(appBean);

                int saveStatus = dbEIP.SaveChanges();
            }
            #endregion

            // 獲得需求單明細資料
            Dictionary<string, object> srdetail = CMF.GetSRDetail(IV_SRID, pOperationID_GenerallySR);

            pMsg += "Upload PDF Method start";

            string path = "";
            string PublishTarget = "";
            string TestEmail = "";
            string firstEgnrName = "";

            try
            {
                if (IV_SRReportType == SRReportType.SIGN || IV_SRReportType == SRReportType.ONLINE || IV_SRReportType == SRReportType.REMOTE) //有簽名檔、線上、遠端
                {
                    #region 有簽名檔、線上、遠端

                    #region -- 服務案件工時更新(前面已更新，此處僅設定呈現資料) --
                    //測試用
                    //IV_StartTime = "2023/01/15 08:00:00";
                    //IV_ArriveTime = "2023/01/15 08:30:00";
                    //IV_FinishTime = "2023/01/15 10:10:00";

                    JsonResult jsonResult;

                    string YYYYMMDDHHMMSS = "";
                    string ARRIVE = "";
                    string COMPT = "";
                    string DEPART = "";
                    string ENGINEER = "";
                    string LABOR = "";
                    string SRVDESC = "";
                    string CLIENTDESC = "";
                    string CSRID = "";
                    string OWNER = "";
                    string EMAIL = "";
                    string IV_CSR = "";
                    string IV_COUNTERIN = "";
                    string IV_COUNTEROUT = "";
                    string IV_LABOR = "";

                    try
                    {
                        if (!string.IsNullOrEmpty(IV_ArriveTime) && !string.IsNullOrEmpty(IV_FinishTime))
                        {
                            TimeSpan Ts = Convert.ToDateTime(IV_FinishTime) - Convert.ToDateTime(IV_ArriveTime);
                            IV_LABOR = Ts.TotalMinutes.ToString();
                        }

                        DEPART = IV_StartTime;
                        ARRIVE = IV_ArriveTime;
                        COMPT = IV_FinishTime;
                        ENGINEER = string.IsNullOrEmpty(IV_EMPNONAME) ? srdetail["EV_MAINENG"].ToString() : IV_EMPNONAME;
                        firstEgnrName = ENGINEER;
                        LABOR = IV_LABOR;
                    }
                    catch (Exception ex)
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因(必要參數有缺):" + ex.Message + Environment.NewLine;
                        pMsg += " 失敗行數：" + ex.ToString();

                        CMF.writeToLog(IV_SRID, "UploadSignToPdf_API", pMsg, IV_EMPNONAME);
                        CMF.SendMailByAPI("UploadSignToPdf_API", null, "elvis.chang@etatung.com", "", "", "UploadSignToPdf_API錯誤 - " + IV_SRID, pMsg, null, null);
                    }
                    #endregion

                    bool haveSignature = false;
                    string pngPath = "";

                    if (IV_SRReportType == SRReportType.SIGN)
                    {
                        #region -- 上傳簽名檔 --
                        try
                        {
                            if (upload != null && upload.ContentLength > 0)
                            {
                                haveSignature = true;

                                string fileName = upload.FileName.Replace(Path.GetExtension(upload.FileName), "") + ".png";
                                pngPath = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                                upload.SaveAs(pngPath);
                            }
                            else
                            {
                                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
                                haveSignature = true; //測試用(因為沒有上傳圖片，所以upload = null。haveSignature要是true，才會跑產生pdf)
                                pngPath = Path.Combine(Server.MapPath("~/img"), "white.png");
                            }

                            pMsg += "File Uploaded Successfully!!";
                        }
                        catch (Exception ex)
                        {
                            pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因(File upload failed):PDF沒產生QQ" + Environment.NewLine;
                            pMsg += " 失敗行數：" + ex.ToString();

                            CMF.writeToLog(IV_SRID, "UploadSignToPdf_API", pMsg, IV_EMPNONAME);
                            CMF.SendMailByAPI("UploadSignToPdf_API", null, "leon.huang@etatung.com;elvis.chang@etatung.com", "", "", "UploadSignToPdf_API錯誤 - " + IV_SRID, pMsg, null, null);
                        }
                        #endregion
                    }
                    else if (IV_SRReportType == SRReportType.ONLINE || IV_SRReportType == SRReportType.REMOTE)
                    {
                        haveSignature = true;
                    }

                    // TEST GET URL
                    // haveSignature = true;
                    var maintain_type = string.Empty; //20221005-用來記錄問卷類型

                    if (haveSignature)
                    {
                        #region // 產生 PDF

                        #region // 字型設定
                        BaseFont bfChinese = null;
                        try
                        {
                            bfChinese = BaseFont.CreateFont(Server.MapPath("~/fonts/") + "msjh.ttc,0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        }
                        catch (Exception ex)
                        {
                            pMsg += "Windows seems to lack font:" + ex.StackTrace;
                            bfChinese = BaseFont.CreateFont(Server.MapPath("~/fonts/") + "msjh.ttc,0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        }

                        iTextSharp.text.Font ChFont6 = new iTextSharp.text.Font(bfChinese, 6);
                        iTextSharp.text.Font ChFont8 = new iTextSharp.text.Font(bfChinese, 8);
                        iTextSharp.text.Font ChFont8U = new iTextSharp.text.Font(bfChinese, 8, iTextSharp.text.Font.UNDERLINE);
                        iTextSharp.text.Font ChFont8Blue = new iTextSharp.text.Font(bfChinese, 8, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 112, 192));
                        iTextSharp.text.Font ChFont8Red = new iTextSharp.text.Font(bfChinese, 8, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.RED);

                        iTextSharp.text.Font ChFont10 = new iTextSharp.text.Font(bfChinese, 10);
                        iTextSharp.text.Font ChFont10U = new iTextSharp.text.Font(bfChinese, 10, iTextSharp.text.Font.UNDERLINE);
                        iTextSharp.text.Font ChFont10Blue = new iTextSharp.text.Font(bfChinese, 10, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(0, 112, 192));
                        iTextSharp.text.Font ChFont10Red = new iTextSharp.text.Font(bfChinese, 10, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.RED);

                        iTextSharp.text.Font ChFont12 = new iTextSharp.text.Font(bfChinese, 12);
                        iTextSharp.text.Font ChFont12B = new iTextSharp.text.Font(bfChinese, 12, iTextSharp.text.Font.BOLD);

                        iTextSharp.text.Font ChFont16B = new iTextSharp.text.Font(bfChinese, 16, iTextSharp.text.Font.BOLD);
                        iTextSharp.text.Font ChFont16I = new iTextSharp.text.Font(bfChinese, 16, iTextSharp.text.Font.ITALIC);

                        // 打勾
                        BaseFont bfSymbol = null;
                        try
                        {
                            bfSymbol = BaseFont.CreateFont(Server.MapPath("~/fonts/") + "WINGDNG2.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        }
                        catch (Exception ex)
                        {
                            pMsg += "Windows seems to lack font:" + ex.StackTrace;
                            bfSymbol = BaseFont.CreateFont(Server.MapPath("~/fonts/") + "msjh.ttc,0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        }
                        iTextSharp.text.Font checkSymbol = new iTextSharp.text.Font(bfSymbol, 12);
                        #endregion

                        #region // 標頭
                        PdfPTable hTable = new PdfPTable(new float[] { 4, 15, 4 });
                        hTable.WidthPercentage = 92;

                        hTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER; // 無邊框

                        PdfPCell hcell1 = new PdfPCell(new iTextSharp.text.Phrase("NO." + IV_SRID, ChFont10U));
                        hcell1.Border = PdfPCell.NO_BORDER;
                        hTable.AddCell(hcell1);

                        // EV_DEPARTMENT 判斷是大世科還是群輝
                        string EV_DEPARTMENT = srdetail["EV_DEPARTMENT"].ToString();

                        bool isTSTI = true; //這邊都固定為T012-大世科

                        if (isTSTI)
                        {
                            // 組成 tsti 大同世界股份有限公司
                            iTextSharp.text.Chunk hchunk1 = new iTextSharp.text.Chunk("tst", ChFont16B);
                            iTextSharp.text.Chunk hchunk2 = new iTextSharp.text.Chunk("i", ChFont16I);
                            iTextSharp.text.Chunk hchunk3 = new iTextSharp.text.Chunk(" 大同世界股份有限公司", ChFont16B);
                            iTextSharp.text.Chunk hchunk4 = new iTextSharp.text.Chunk(Environment.NewLine + "www.etatung.com", ChFont10);
                            iTextSharp.text.Phrase hphrase1 = new iTextSharp.text.Phrase();
                            hphrase1.Add(hchunk1);
                            hphrase1.Add(hchunk2);
                            hphrase1.Add(hchunk3);
                            hphrase1.Add(hchunk4);
                            PdfPCell hcell2 = new PdfPCell(hphrase1);
                            hcell2.Border = PdfPCell.NO_BORDER;
                            hcell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            hcell2.Rowspan = 2;
                            hTable.AddCell(hcell2);
                        }
                        else
                        {
                            // 簽名圖檔
                            string chclogoPath = Server.MapPath("~/Images/chcti.png");
                            iTextSharp.text.Image chclogo = iTextSharp.text.Image.GetInstance(chclogoPath);
                            chclogo.ScalePercent(60);

                            PdfPCell hcell2 = new PdfPCell(chclogo);
                            hcell2.Border = PdfPCell.NO_BORDER;
                            hcell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            hcell2.Rowspan = 2;
                            hTable.AddCell(hcell2);
                        }

                        PdfPCell hcell3 = new PdfPCell();
                        hcell3.Border = PdfPCell.NO_BORDER;
                        hTable.AddCell(hcell3);

                        PdfPCell hcell4 = new PdfPCell(new iTextSharp.text.Phrase("客服專線", ChFont10));
                        hcell4.Border = PdfPCell.NO_BORDER;
                        hTable.AddCell(hcell4);

                        PdfPCell hcell5 = new PdfPCell(new iTextSharp.text.Phrase("報修網址", ChFont10));
                        hcell5.Border = PdfPCell.NO_BORDER;
                        hTable.AddCell(hcell5);

                        PdfPCell hcell6 = new PdfPCell(new iTextSharp.text.Phrase("0800-066038", ChFont8Blue));
                        hcell6.Border = PdfPCell.NO_BORDER;
                        hTable.AddCell(hcell6);

                        PdfPCell hcell7 = new PdfPCell(new iTextSharp.text.Phrase("客戶服務報告", ChFont12B));
                        hcell7.Border = PdfPCell.NO_BORDER;
                        hcell7.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        hcell7.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        hcell7.Rowspan = 2;
                        hTable.AddCell(hcell7);

                        PdfPCell hcell8 = new PdfPCell(new iTextSharp.text.Phrase("0800.etatung.com", ChFont8Blue));
                        hcell8.Border = PdfPCell.NO_BORDER;
                        hTable.AddCell(hcell8);

                        PdfPCell hcell9 = new PdfPCell(new iTextSharp.text.Phrase("(02)2598-5738", ChFont8Blue));
                        hcell9.Border = PdfPCell.NO_BORDER;
                        hTable.AddCell(hcell9);

                        PdfPCell hcell10 = new PdfPCell();
                        hcell10.Border = PdfPCell.NO_BORDER;
                        hTable.AddCell(hcell10);
                        #endregion

                        #region // 需求單內容
                        PdfPTable pTable = new PdfPTable(new float[] { 2, 2, 1, 2, 3, 2, 1, 3 });
                        pTable.WidthPercentage = 92;
                        pTable.SpacingBefore = 2;
                        pTable.DefaultCell.Border = 2;
                        pTable.DefaultCell.Padding = 4;

                        // 第一列
                        PdfPCell pcell1 = new PdfPCell(new iTextSharp.text.Phrase("客 戶 資 料", ChFont12B));
                        pcell1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell1.BorderWidthLeft = 1;
                        pcell1.BorderWidthRight = 1;
                        pcell1.BorderWidthTop = 1;
                        pcell1.Colspan = 5;
                        pcell1.Padding = 4;
                        pTable.AddCell(pcell1);

                        PdfPCell pcell2 = new PdfPCell(new iTextSharp.text.Phrase("服 務 編 號", ChFont12B));
                        pcell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell2.BorderWidthLeft = 0;
                        pcell2.BorderWidthRight = 1;
                        pcell2.BorderWidthTop = 1;
                        pcell2.Colspan = 3;
                        pcell2.Padding = 4;
                        pTable.AddCell(pcell2);

                        // 第二列
                        PdfPCell pcell3 = new PdfPCell(new iTextSharp.text.Phrase("公司名稱", ChFont10));
                        pcell3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell3.BorderWidthLeft = 1;
                        pcell3.BorderWidthRight = 0;
                        pcell3.BorderWidthTop = 0;
                        pcell3.Padding = 4;
                        pTable.AddCell(pcell3);

                        string CUSTOMER = srdetail["EV_CUSTOMER"].ToString();
                        PdfPCell pcell4 = new PdfPCell(new iTextSharp.text.Phrase(CUSTOMER, ChFont10));
                        pcell4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        pcell4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell4.BorderWidthRight = 1;
                        pcell4.BorderWidthTop = 0;
                        pcell4.Colspan = 4;
                        pcell4.Padding = 4;
                        pTable.AddCell(pcell4);

                        PdfPCell pcell5 = new PdfPCell(new iTextSharp.text.Phrase(IV_SRID, ChFont10));
                        pcell5.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell5.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell5.BorderWidthLeft = 0;
                        pcell5.BorderWidthRight = 1;
                        pcell5.BorderWidthTop = 0;
                        pcell5.Colspan = 3;
                        pcell5.Padding = 4;
                        pTable.AddCell(pcell5);

                        // 第三列
                        PdfPCell pcell6 = new PdfPCell(new iTextSharp.text.Phrase("部門 (單位)", ChFont10));
                        pcell6.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell6.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell6.BorderWidthLeft = 1;
                        pcell6.BorderWidthRight = 0;
                        pcell6.BorderWidthTop = 0;
                        pcell6.Padding = 4;
                        pTable.AddCell(pcell6);

                        PdfPCell pcell7 = new PdfPCell(new iTextSharp.text.Phrase("", ChFont10));
                        pcell7.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        pcell7.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell7.BorderWidthRight = 1;
                        pcell7.BorderWidthTop = 0;
                        pcell7.Colspan = 4;
                        pcell7.Padding = 4;
                        pTable.AddCell(pcell7);

                        PdfPCell pcell8 = new PdfPCell(new iTextSharp.text.Phrase("服 務 種 類", ChFont12B));
                        pcell8.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell8.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell8.BorderWidthLeft = 0;
                        pcell8.BorderWidthRight = 1;
                        pcell8.BorderWidthTop = 0;
                        pcell8.Colspan = 3;
                        pcell8.Padding = 4;
                        pTable.AddCell(pcell8);

                        // 第四列
                        PdfPCell pcell9 = new PdfPCell(new iTextSharp.text.Phrase("地址", ChFont10));
                        pcell9.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell9.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell9.BorderWidthLeft = 1;
                        pcell9.BorderWidthRight = 0;
                        pcell9.BorderWidthTop = 0;
                        pcell9.Padding = 4;
                        pTable.AddCell(pcell9);

                        PdfPCell pcell10 = new PdfPCell(new iTextSharp.text.Phrase(srdetail["EV_RADDR"].ToString(), ChFont10));
                        pcell10.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        pcell10.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell10.BorderWidthRight = 1;
                        pcell10.BorderWidthTop = 0;
                        pcell10.Colspan = 4;
                        pcell10.Padding = 4;
                        pTable.AddCell(pcell10);

                        //ZSR1 一般
                        //ZSR4 Survey
                        //ZSR5 定保
                        //ZSR3 裝機
                        string ZSR1 = "";
                        string ZSR5 = "";
                        string ZSR3 = "";
                        string EV_TYPE = srdetail.ContainsKey("EV_TYPE") ? srdetail["EV_TYPE"].ToString() : "ZSR1";
                        switch (EV_TYPE)
                        {
                            case "ZSR1":
                                ZSR1 = "R";
                                break;
                            case "ZSR5":
                                ZSR5 = "R";
                                break;
                            case "ZSR3":
                                ZSR3 = "R";
                                break;
                            default:
                                ZSR1 = "R";
                                break;
                        }

                        iTextSharp.text.Chunk pchunk1 = new iTextSharp.text.Chunk("維 修 ", ChFont10);
                        iTextSharp.text.Chunk pchunk2 = new iTextSharp.text.Chunk(ZSR1, checkSymbol);
                        iTextSharp.text.Chunk pchunk3 = new iTextSharp.text.Chunk("　 定 期 維 護 ", ChFont10);
                        iTextSharp.text.Chunk pchunk4 = new iTextSharp.text.Chunk(ZSR5, checkSymbol);
                        iTextSharp.text.Phrase pphrase1 = new iTextSharp.text.Phrase();
                        pphrase1.Add(pchunk1);
                        pphrase1.Add(pchunk2);
                        pphrase1.Add(pchunk3);
                        pphrase1.Add(pchunk4);
                        PdfPCell pcell11 = new PdfPCell(pphrase1);

                        pcell11.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell11.BorderWidthLeft = 0;
                        pcell11.BorderWidthRight = 1;
                        pcell11.BorderWidthTop = 0;
                        pcell11.BorderWidthBottom = 0;
                        pcell11.Colspan = 3;
                        pcell11.Padding = 4;
                        pTable.AddCell(pcell11);

                        // 第五列
                        PdfPCell pcell12 = new PdfPCell(new iTextSharp.text.Phrase("聯絡人", ChFont10));
                        pcell12.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell12.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell12.BorderWidthLeft = 1;
                        pcell12.BorderWidthRight = 0;
                        pcell12.BorderWidthTop = 0;
                        pcell12.BorderWidthBottom = 1;
                        pcell12.Rowspan = 3;
                        pcell12.Padding = 4;
                        pTable.AddCell(pcell12);

                        PdfPCell pcell13 = new PdfPCell(new iTextSharp.text.Phrase(srdetail["EV_REPORT"].ToString(), ChFont10));
                        pcell13.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell13.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell13.BorderWidthTop = 0;
                        pcell13.BorderWidthBottom = 1;
                        pcell13.Colspan = 2;
                        pcell13.Rowspan = 3;
                        pcell13.Padding = 4;
                        pTable.AddCell(pcell13);

                        PdfPCell pcell14 = new PdfPCell(new iTextSharp.text.Phrase("聯絡電話", ChFont10));
                        pcell14.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell14.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell14.BorderWidthLeft = 0;
                        pcell14.BorderWidthTop = 0;
                        pcell14.BorderWidthBottom = 1;
                        pcell14.Rowspan = 3;
                        pcell14.Padding = 4;
                        pTable.AddCell(pcell14);

                        PdfPCell pcell15 = new PdfPCell(new iTextSharp.text.Phrase(srdetail["EV_RTEL"].ToString(), ChFont10));
                        pcell15.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell15.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell15.BorderWidthLeft = 0;
                        pcell15.BorderWidthRight = 1;
                        pcell15.BorderWidthTop = 0;
                        pcell15.BorderWidthBottom = 1;
                        pcell15.Rowspan = 3;
                        pcell15.Padding = 4;
                        pTable.AddCell(pcell15);

                        iTextSharp.text.Chunk pchunk5 = new iTextSharp.text.Chunk("裝 機 ", ChFont10);
                        iTextSharp.text.Chunk pchunk6 = new iTextSharp.text.Chunk(ZSR3, checkSymbol);
                        iTextSharp.text.Phrase pphrase2 = new iTextSharp.text.Phrase();
                        pphrase2.Add(pchunk5);
                        pphrase2.Add(pchunk6);
                        PdfPCell pcell16 = new PdfPCell(pphrase2);

                        pcell16.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell16.BorderWidthLeft = 0;
                        pcell16.BorderWidthRight = 0;
                        pcell16.BorderWidthTop = 0;
                        pcell16.BorderWidthBottom = 1;
                        pcell16.Rowspan = 3;
                        pcell16.PaddingLeft = 5;
                        pcell16.PaddingRight = 0;
                        pcell16.PaddingTop = 5;
                        pcell16.PaddingBottom = 10;
                        pTable.AddCell(pcell16);

                        iTextSharp.text.Chunk pchunk7 = new iTextSharp.text.Chunk("訂單號碼 ", ChFont8);
                        iTextSharp.text.Chunk pchunk8 = new iTextSharp.text.Chunk(":　　　　　　　　　　　", ChFont8U);
                        iTextSharp.text.Chunk pchunk9 = new iTextSharp.text.Chunk("\n\n備料單號 ", ChFont8);
                        iTextSharp.text.Chunk pchunk10 = new iTextSharp.text.Chunk(":　　　　　　　　　　　", ChFont8U);
                        iTextSharp.text.Phrase pphrase3 = new iTextSharp.text.Phrase();
                        pphrase3.Add(pchunk7);
                        pphrase3.Add(pchunk8);
                        pphrase3.Add(pchunk9);
                        pphrase3.Add(pchunk10);
                        PdfPCell pcell17 = new PdfPCell(pphrase3);

                        pcell17.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        pcell17.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell17.BorderWidthLeft = 0;
                        pcell17.BorderWidthRight = 1;
                        pcell17.BorderWidthTop = 0;
                        pcell17.BorderWidthBottom = 1;
                        pcell17.Colspan = 2;
                        pcell17.Rowspan = 3;
                        pcell17.PaddingLeft = 0;
                        pcell17.PaddingRight = 0;
                        pcell17.PaddingTop = 5;
                        pcell17.PaddingBottom = 10;
                        pTable.AddCell(pcell17);
                        #endregion

                        #region // 產品訊息
                        // 第六列 -- 中間
                        PdfPCell pcell18 = new PdfPCell(new iTextSharp.text.Phrase("服 務 需 求", ChFont12B));
                        pcell18.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell18.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell18.BorderWidthLeft = 1;
                        pcell18.BorderWidthRight = 1;
                        pcell18.BorderWidthTop = 0;
                        pcell18.Colspan = 8;
                        pcell18.Padding = 4;
                        pTable.AddCell(pcell18);

                        // 第七列
                        // 取第一筆 Product 名稱
                        string PRDID = "";
                        string SNNO = "";
                        string PRDNUMBER = "";

                        if (EV_TYPE == "ZSR1") //一般
                        {
                            List<SNLIST> products = srdetail.ContainsKey("table_ET_SNLIST") ? (List<SNLIST>)srdetail["table_ET_SNLIST"] : null;

                            if (products.Count > 0)
                            {
                                //取第一筆機器序號
                                PRDID = products[0].PRDID;
                                SNNO = products[0].SNNO;
                                PRDNUMBER = products[0].PRDNUMBER;
                            }
                        }
                        else if (EV_TYPE == "ZSR3") //裝機
                        {
                            List<SFSNLIST> products = srdetail.ContainsKey("table_ET_SFSNLIST") ? (List<SFSNLIST>)srdetail["table_ET_SFSNLIST"] : null;

                            if (products.Count > 0)
                            {
                                foreach(var prod in products)
                                {
                                    SNNO += prod.SNNO + ";";
                                }

                                SNNO = SNNO.TrimEnd(';');                                
                            }
                        }

                        PdfPCell pcell19 = new PdfPCell(new iTextSharp.text.Phrase("產品名稱", ChFont10));
                        pcell19.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell19.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell19.BorderWidthLeft = 1;
                        pcell19.BorderWidthRight = 0;
                        pcell19.BorderWidthTop = 0;
                        pcell19.Padding = 4;
                        pcell19.FixedHeight = (18 * pcell19.Rowspan) + 10;
                        pTable.AddCell(pcell19);

                        PdfPCell pcell20 = new PdfPCell(new iTextSharp.text.Phrase(PRDID, ChFont10));
                        pcell20.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        pcell20.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell20.BorderWidthRight = 1;
                        pcell20.BorderWidthTop = 0;
                        pcell20.Colspan = (isTSTI) ? 7 : 4;
                        pcell20.Padding = 4;
                        pTable.AddCell(pcell20);

                        if (!isTSTI) // 群輝
                        {
                            PdfPCell pcell21 = new PdfPCell(new iTextSharp.text.Phrase("計數器\n（進）", ChFont10));
                            pcell21.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            pcell21.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            pcell21.BorderWidthLeft = 0;
                            pcell21.BorderWidthTop = 0;
                            pcell21.Padding = 4;
                            pTable.AddCell(pcell21);

                            string cin = srdetail["EV_COUNTIN"] != null ? srdetail["EV_COUNTIN"].ToString() : "";

                            PdfPCell pcell22 = new PdfPCell(new iTextSharp.text.Phrase(cin, ChFont10));
                            pcell22.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            pcell22.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            pcell22.BorderWidthLeft = 0;
                            pcell22.BorderWidthRight = 1;
                            pcell22.BorderWidthTop = 0;
                            pcell22.Colspan = 2;
                            pcell22.Padding = 4;
                            pTable.AddCell(pcell22);
                        }

                        // 第八列
                        iTextSharp.text.Chunk pck1 = new iTextSharp.text.Chunk("型號", ChFont10);
                        iTextSharp.text.Chunk pck2 = new iTextSharp.text.Chunk("(P/N)", ChFont10Red);
                        iTextSharp.text.Chunk pck3 = new iTextSharp.text.Chunk("\n\n序號", ChFont10);
                        iTextSharp.text.Chunk pck4 = new iTextSharp.text.Chunk("(S/N)", ChFont10Red);
                        iTextSharp.text.Phrase pph1 = new iTextSharp.text.Phrase();
                        pph1.Add(pck1);
                        pph1.Add(pck2);
                        pph1.Add(pck3);
                        pph1.Add(pck4);

                        PdfPCell pcell23 = new PdfPCell(pph1);
                        pcell23.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell23.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell23.BorderWidthLeft = 1;
                        pcell23.BorderWidthRight = 0;
                        pcell23.BorderWidthTop = 0;
                        pcell23.Padding = 4;
                        pcell23.FixedHeight = (18 * pcell23.Rowspan) + 10;
                        pTable.AddCell(pcell23);

                        PdfPCell pcell24 = new PdfPCell(new iTextSharp.text.Phrase(PRDNUMBER + "\n\n" + SNNO, ChFont10));
                        pcell24.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        pcell24.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell24.BorderWidthRight = 1;
                        pcell24.BorderWidthTop = 0;
                        pcell24.Colspan = (isTSTI) ? 7 : 4;
                        pcell24.Padding = 4;
                        pTable.AddCell(pcell24);

                        if (!isTSTI) // 群輝
                        {
                            PdfPCell pcell25 = new PdfPCell(new iTextSharp.text.Phrase("計數器\n（出）", ChFont10));
                            pcell25.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            pcell25.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            pcell25.BorderWidthLeft = 0;
                            pcell25.BorderWidthTop = 0;
                            pcell25.Padding = 4;
                            pTable.AddCell(pcell25);

                            string cout = srdetail["EV_COUNTOUT"] != null ? srdetail["EV_COUNTOUT"].ToString() : "";

                            PdfPCell pcell26 = new PdfPCell(new iTextSharp.text.Phrase(cout, ChFont10));
                            pcell26.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            pcell26.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            pcell26.BorderWidthLeft = 0;
                            pcell26.BorderWidthRight = 1;
                            pcell26.BorderWidthTop = 0;
                            pcell26.Colspan = 2;
                            pcell26.Padding = 4;
                            pTable.AddCell(pcell26);
                        }
                        #endregion

                        #region --CRM上說明
                        // 第九列
                        string PROBLEM = srdetail.ContainsKey("EV_PROBLEM") ? srdetail["EV_PROBLEM"].ToString() : "";
                        string CONTACT = srdetail["EV_CONTACT"].ToString();
                        string TEL = srdetail["EV_TEL"].ToString();
                        string ADDR = srdetail["EV_ADDR"].ToString();
                        string DESC_TEST = srdetail.ContainsKey("EV_DESC") ? srdetail["EV_DESC"].ToString() : ""; //抓說明

                        string tmp = "";
                        tmp += (!String.IsNullOrEmpty(PROBLEM)) ? PROBLEM : "";
                        tmp += (!String.IsNullOrEmpty(CONTACT)) ? " -- " + CONTACT : "";
                        tmp += (!String.IsNullOrEmpty(TEL)) ? " -- " + TEL : "";
                        tmp += (!String.IsNullOrEmpty(ADDR)) ? " -- " + ADDR : "";

                        //如果problem是空值，且EV_TYPE=ZSR-5(代表定維)，要去抓說明
                        if (EV_TYPE == "ZSR5" && PROBLEM == "")
                        {
                            tmp += (!String.IsNullOrEmpty(DESC_TEST)) ? DESC_TEST : "";
                        }
                        #endregion

                        #region  --需求事項
                        PdfPCell pcell27 = new PdfPCell(new iTextSharp.text.Phrase("需求事項", ChFont10));
                        pcell27.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        pcell27.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        pcell27.BorderWidthLeft = 1;
                        pcell27.BorderWidthRight = 0;
                        pcell27.BorderWidthTop = 0;
                        pcell27.Rowspan = 4;
                        pcell27.Padding = 4;
                        pcell27.FixedHeight = (14 * pcell27.Rowspan) + 5;
                        pTable.AddCell(pcell27);

                        PdfPCell pcell28 = new PdfPCell(new iTextSharp.text.Phrase(tmp, ChFont10));
                        pcell28.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        pcell28.BorderWidthRight = 1;
                        pcell28.BorderWidthTop = 0;
                        pcell28.Rowspan = 4;
                        pcell28.Colspan = (isTSTI) ? 7 : 4;
                        pcell28.Padding = 4;
                        pTable.AddCell(pcell28);

                        if (!isTSTI) // 群輝
                        {
                            PdfPCell pcell29 = new PdfPCell(new iTextSharp.text.Phrase("", ChFont10));
                            pcell29.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            pcell29.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            pcell29.BorderWidthLeft = 0;
                            pcell29.BorderWidthTop = 0;
                            pcell29.Rowspan = 2;
                            pcell29.Padding = 4;
                            pTable.AddCell(pcell29);

                            PdfPCell pcell30 = new PdfPCell(new iTextSharp.text.Phrase("", ChFont10));
                            pcell30.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            pcell30.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            pcell30.BorderWidthLeft = 0;
                            pcell30.BorderWidthRight = 1;
                            pcell30.BorderWidthTop = 0;
                            pcell30.Colspan = 2;
                            pcell30.Rowspan = 2;
                            pcell30.Padding = 4;
                            pTable.AddCell(pcell30);

                            PdfPCell pcell31 = new PdfPCell(new iTextSharp.text.Phrase("", ChFont10));
                            pcell31.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            pcell31.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            pcell31.BorderWidthLeft = 0;
                            pcell31.BorderWidthTop = 0;
                            pcell31.Rowspan = 2;
                            pcell31.Padding = 4;
                            pTable.AddCell(pcell31);

                            PdfPCell pcell32 = new PdfPCell(new iTextSharp.text.Phrase("", ChFont10));
                            pcell32.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            pcell32.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            pcell32.BorderWidthLeft = 0;
                            pcell32.BorderWidthRight = 1;
                            pcell32.BorderWidthTop = 0;
                            pcell32.Colspan = 2;
                            pcell32.Rowspan = 2;
                            pcell32.Padding = 4;
                            pTable.AddCell(pcell32);
                        }
                        #endregion

                        #region // 工時
                        PdfPCell tcell1 = new PdfPCell(new iTextSharp.text.Phrase("服務日期", ChFont10));
                        tcell1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell1.BorderWidthLeft = 1;
                        tcell1.BorderWidthRight = 0;
                        tcell1.BorderWidthTop = 0;
                        tcell1.Colspan = 2;
                        tcell1.Padding = 4;
                        pTable.AddCell(tcell1);

                        PdfPCell tcell2 = new PdfPCell(new iTextSharp.text.Phrase("出發", ChFont10));
                        tcell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell2.BorderWidthTop = 0;
                        tcell2.Colspan = 2;
                        tcell2.Padding = 4;
                        pTable.AddCell(tcell2);

                        PdfPCell tcell3 = new PdfPCell(new iTextSharp.text.Phrase("到場", ChFont10));
                        tcell3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell3.BorderWidthLeft = 0;
                        tcell3.BorderWidthTop = 0;
                        tcell3.Padding = 4;
                        pTable.AddCell(tcell3);

                        PdfPCell tcell4 = new PdfPCell(new iTextSharp.text.Phrase("完成", ChFont10));
                        tcell4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell4.BorderWidthLeft = 0;
                        tcell4.BorderWidthRight = 0;
                        tcell4.BorderWidthTop = 0;
                        tcell4.Colspan = 2;
                        tcell4.Padding = 4;
                        pTable.AddCell(tcell4);

                        PdfPCell tcell5 = new PdfPCell(new iTextSharp.text.Phrase("處理時間", ChFont10));
                        tcell5.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell5.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell5.BorderWidthRight = 1;
                        tcell5.BorderWidthTop = 0;
                        tcell5.Padding = 4;
                        pTable.AddCell(tcell5);

                        string strDEPARTDate = (DEPART != null && DEPART.Length >= 10) ? ARRIVE.Substring(0, 10).Replace("-", "／") : "";

                        // 工時部份 -- 內容列
                        PdfPCell tcell6 = new PdfPCell(new iTextSharp.text.Phrase(strDEPARTDate, ChFont10));
                        tcell6.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell6.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell6.BorderWidthLeft = 1;
                        tcell6.BorderWidthRight = 0;
                        tcell6.BorderWidthTop = 0;
                        tcell6.Colspan = 2;
                        tcell6.Padding = 4;
                        pTable.AddCell(tcell6);

                        string strDEPART = (DEPART != null && DEPART.Length >= 16) ? DEPART.Substring(11, 5).Replace("-", "／") : "";

                        PdfPCell tcell7 = new PdfPCell(new iTextSharp.text.Phrase(strDEPART, ChFont10));
                        tcell7.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell7.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell7.BorderWidthTop = 0;
                        tcell7.Colspan = 2;
                        tcell7.Padding = 4;
                        pTable.AddCell(tcell7);

                        string strARRIVE = (ARRIVE != null && ARRIVE.Length >= 16) ? ARRIVE.Substring(11, 5).Replace("-", "／") : "";

                        PdfPCell tcell8 = new PdfPCell(new iTextSharp.text.Phrase(strARRIVE, ChFont10));
                        tcell8.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell8.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell8.BorderWidthLeft = 0;
                        tcell8.BorderWidthTop = 0;
                        tcell8.Padding = 4;
                        pTable.AddCell(tcell8);

                        string strCOMPT = (COMPT != null && COMPT.Length >= 16) ? COMPT.Substring(11, 5).Replace("-", "／") : "";

                        PdfPCell tcell9 = new PdfPCell(new iTextSharp.text.Phrase(strCOMPT, ChFont10));
                        tcell9.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell9.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell9.BorderWidthLeft = 0;
                        tcell9.BorderWidthRight = 0;
                        tcell9.BorderWidthTop = 0;
                        tcell9.Colspan = 2;
                        tcell9.Padding = 4;
                        pTable.AddCell(tcell9);

                        // 處理工時字串
                        double hours = Math.Floor(float.Parse(LABOR) / 60);
                        double minutes = Math.Floor(float.Parse(LABOR) % 60);

                        string LABORTIME = "";
                        if (hours > 0) LABORTIME += Convert.ToInt32(hours) + "小時";
                        if (minutes > 0) LABORTIME += Convert.ToInt32(minutes) + "分";
                        if (hours == 0 && minutes == 0) LABORTIME += "1分";

                        PdfPCell tcell10 = new PdfPCell(new iTextSharp.text.Phrase(LABORTIME, ChFont10));
                        tcell10.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        tcell10.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        tcell10.BorderWidthRight = 1;
                        tcell10.BorderWidthTop = 0;
                        tcell10.Padding = 4;
                        pTable.AddCell(tcell10);
                        #endregion

                        #region // 服務說明
                        bool isOver1Survey = false;
                        PdfPTable _pTable = new PdfPTable(new float[] { 2, 2, 1, 2, 3, 2, 1, 3 });
                        _pTable.WidthPercentage = 92;
                        _pTable.SpacingBefore = 2;
                        _pTable.DefaultCell.Border = 2;
                        _pTable.DefaultCell.Padding = 4;

                        //先取出問券的svid、hash
                        string svid = "";
                        string hash = "";
                        int surveyCount = 0; //20221005更新
                        if (IV_SRID.Substring(0, 2) == "65")
                        {
                            var qSurveyInfo = appDB.TB_SURVEY_ANS_MAINTAIN.Where(x => x.srid == IV_SRID).OrderByDescending(x => x.UpdateTime);
                            if (qSurveyInfo != null && qSurveyInfo.Count() > 0)
                            {
                                surveyCount = 0;//20221005更新
                                List<PdfPTable> liPdfPTable = new List<PdfPTable>();
                                var int_totalreports = qSurveyInfo.Count() + 1; //20221007-共幾份報告+1=頁數

                                foreach (var surveyBean in qSurveyInfo)
                                {
                                    surveyCount++;
                                    svid = surveyBean.svid;
                                    hash = surveyBean.hash;
                                    List<string> surveyQnA = GetSurveyMaintainQnA(svid, hash);

                                    //20220719-有溫度、濕度的單位嗎?-->先確定是通訊類單有這種問題
                                    maintain_type = surveyBean.maintain_type;//20221005
                                    var opinions_from_engineer = surveyBean.opinions_from_engineer;

                                    //第一筆顯示在第一頁report
                                    if (surveyCount == 1)
                                    {
                                        //方框:、方框帶勾:R
                                        iTextSharp.text.Phrase proPhraseQus = new iTextSharp.text.Phrase();
                                        iTextSharp.text.Phrase proPhraseAns = new iTextSharp.text.Phrase();

                                        #region --20220715-只有一筆調查，但是題目+答案數>38，空間太小印不出來

                                        //太多內容給第二頁用的
                                        iTextSharp.text.Phrase proPhraseQus2 = new iTextSharp.text.Phrase();
                                        iTextSharp.text.Phrase proPhraseAns2 = new iTextSharp.text.Phrase();

                                        var int_totalquestions = surveyQnA.Count;

                                        //20220804-改採用問卷類別來判斷，不用int_totalquestions
                                        if (maintain_type != "communication")
                                        {
                                            //印一頁就可以印完
                                            //非通訊類的問卷
                                            for (var i = 0; i < surveyQnA.Count; i++)
                                            {
                                                //i是偶數 → 題目
                                                if (i % 2 == 0)
                                                {
                                                    //產品名稱、型號/序號合併成一欄
                                                    if (i == 0)
                                                    {
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("產品資訊：\n\n", ChFont10);
                                                        iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                        proPhraseQus.Add(proChunk1);
                                                        proPhraseQus.Add(proChunk2);
                                                    }
                                                    else if (i == 2) continue;
                                                    else
                                                    {
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i] + "\n", ChFont10);
                                                        iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                        proPhraseQus.Add(proChunk1);
                                                        proPhraseQus.Add(proChunk2);
                                                    }
                                                }
                                                //i是奇數 → 答案
                                                else
                                                {
                                                    if (i == 1)
                                                    {
                                                        //產品名稱、型號/序號合併成一欄
                                                        iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                        string productName = string.IsNullOrEmpty(surveyQnA[3]) ? surveyQnA[1] + "\n\n" : surveyQnA[1] + " (" + surveyQnA[3] + ")\n\n";
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(productName, ChFont10);
                                                        proPhraseAns.Add(proChunk0);
                                                        proPhraseAns.Add(proChunk1);
                                                    }
                                                    else if (i == 3) continue;
                                                    else
                                                    {
                                                        if (surveyQnA[i].Contains("正常"))
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[i].Contains("異常"))
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[i].Contains("無"))
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i].Replace("\n", "\n　 "), ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //一筆調查，內容太多，印到兩頁
                                            //通訊類問卷是這樣的
                                            //先印18題前面的
                                            for (var i = 0; i < 38; i++)
                                            {

                                                //i是偶數 → 題目
                                                if (i % 2 == 0)
                                                {
                                                    //產品名稱、型號/序號合併成一欄
                                                    if (i == 0)
                                                    {
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("產品資訊：\n\n", ChFont10);
                                                        iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                        proPhraseQus.Add(proChunk1);
                                                        proPhraseQus.Add(proChunk2);
                                                    }
                                                    else if (i == 2) continue;
                                                    else
                                                    {
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i] + "\n", ChFont10);
                                                        iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                        proPhraseQus.Add(proChunk1);
                                                        proPhraseQus.Add(proChunk2);
                                                    }
                                                }
                                                //i是奇數 → 答案
                                                else
                                                {
                                                    if (i == 1)
                                                    {
                                                        //產品名稱、型號/序號合併成一欄
                                                        iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                        string productName = string.IsNullOrEmpty(surveyQnA[3]) ? surveyQnA[1] + "\n\n" : surveyQnA[1] + " (" + surveyQnA[3] + ")\n\n";
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(productName, ChFont10);
                                                        proPhraseAns.Add(proChunk0);
                                                        proPhraseAns.Add(proChunk1);
                                                    }
                                                    else if (i == 3) continue;
                                                    else
                                                    {
                                                        if (surveyQnA[i].Contains("正常"))
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[i].Contains("異常"))
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[i].Contains("無"))
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else
                                                        {

                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣

                                                            //20220719-通訊類定維單，有溫度和濕度，希望pdf加上單位
                                                            var scale = string.Empty;

                                                            if (maintain_type == "communication")
                                                            {
                                                                if (i == 5)
                                                                {
                                                                    scale = "  度C";

                                                                }
                                                                else if (i == 7)
                                                                {
                                                                    scale = "  %";
                                                                }
                                                            }
                                                            else
                                                            {

                                                            }


                                                            //iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i].Replace("\n", "\n　 ") + scale, ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);


                                                        }
                                                    }
                                                }
                                            }

                                            //超出頁面的(page2)
                                            if (surveyQnA.Count > 38)
                                            {
                                                isOver1Survey = true;

                                                //太多內容，第二頁的題目和答案
                                                for (var j = 38; j < surveyQnA.Count; j++)
                                                {
                                                    //j是偶數 → 題目
                                                    if (j % 2 == 0)
                                                    {
                                                        //20220718-因為第一行會自動不對齊，所以加入文字
                                                        if (j == 38)
                                                        {
                                                            iTextSharp.text.Chunk proChunk3title = new iTextSharp.text.Chunk("產品資訊：\n\n", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4title = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                            proPhraseQus2.Add(proChunk3title);
                                                            proPhraseQus2.Add(proChunk4title);
                                                        }

                                                        iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk(surveyQnA[j] + "\n", ChFont10);
                                                        iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                        proPhraseQus2.Add(proChunk3);
                                                        proPhraseQus2.Add(proChunk4);

                                                    }
                                                    //i是奇數 → 答案
                                                    else
                                                    {

                                                        //20220718-因為第一行會自動不對齊，所以加入文字
                                                        //奇數題                                            
                                                        if (j == 39)
                                                        {
                                                            //產品名稱、型號/序號合併成一欄
                                                            iTextSharp.text.Chunk proChunk0title = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            string productName = string.IsNullOrEmpty(surveyQnA[3]) ? surveyQnA[1] + "\n\n" : surveyQnA[1] + " (" + surveyQnA[3] + ")\n\n";

                                                            iTextSharp.text.Chunk proChunk1title = new iTextSharp.text.Chunk(productName, ChFont10);
                                                            proPhraseAns2.Add(proChunk0title);
                                                            proPhraseAns2.Add(proChunk1title);
                                                        }

                                                        var item = surveyQnA[j];

                                                        ///20220719-定維表-通訊類，檢查者的意見，有一個選項是【本次定期保養均無問題】，會符合原條件
                                                        ///surveyQnA[j].Contains("無")，導致出錯
                                                        ///而且，如果要載入自行填寫，也可能會用到無、異常、正常等國字
                                                        ///修改(surveyQnA[j].Contains("正常")改為(surveyQnA[j]=="正常")


                                                        if (surveyQnA[j] == "正常")
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns2.Add(proChunk0);
                                                            proPhraseAns2.Add(proChunk1);
                                                            proPhraseAns2.Add(proChunk2);
                                                            proPhraseAns2.Add(proChunk3);
                                                            proPhraseAns2.Add(proChunk4);
                                                            proPhraseAns2.Add(proChunk5);
                                                            proPhraseAns2.Add(proChunk6);
                                                            proPhraseAns2.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[j] == "異常")
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns2.Add(proChunk0);
                                                            proPhraseAns2.Add(proChunk1);
                                                            proPhraseAns2.Add(proChunk2);
                                                            proPhraseAns2.Add(proChunk3);
                                                            proPhraseAns2.Add(proChunk4);
                                                            proPhraseAns2.Add(proChunk5);
                                                            proPhraseAns2.Add(proChunk6);
                                                            proPhraseAns2.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[j] == "無")
                                                        {

                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns2.Add(proChunk0);
                                                            proPhraseAns2.Add(proChunk1);
                                                            proPhraseAns2.Add(proChunk2);
                                                            proPhraseAns2.Add(proChunk3);
                                                            proPhraseAns2.Add(proChunk4);
                                                            proPhraseAns2.Add(proChunk5);
                                                            proPhraseAns2.Add(proChunk6);
                                                            proPhraseAns2.Add(proChunk7);
                                                        }
                                                        else
                                                        {

                                                            //20220722-通訊類(題目和答案數總和>38)最後一個項目，輸入大量文字，格式與其他不同 (首張問卷的後面)
                                                            if (j == (int_totalquestions - 1))
                                                            {
                                                                if (maintain_type == "communication" && opinions_from_engineer == "於下方顯示維護查修內容")
                                                                {

                                                                }
                                                                else
                                                                {
                                                                    iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                                    iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[j].Replace("\n", "\n　 "), ChFont10);
                                                                    iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                                    proPhraseAns2.Add(proChunk0);
                                                                    proPhraseAns2.Add(proChunk1);
                                                                    proPhraseAns2.Add(proChunk2);
                                                                }

                                                            }
                                                            else
                                                            {
                                                                //不是最後一題
                                                                iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                                iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[j].Replace("\n", "\n　 "), ChFont10);
                                                                iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                                proPhraseAns2.Add(proChunk0);
                                                                proPhraseAns2.Add(proChunk1);
                                                                proPhraseAns2.Add(proChunk2);
                                                            }


                                                        }

                                                    }
                                                }

                                            }


                                        }

                                        #endregion

                                        //iTextSharp.text.Chunk proChunkStart = new iTextSharp.text.Chunk("\n\n服務說明：\n\n", ChFont10);
                                        iTextSharp.text.Chunk proChunkStart = new iTextSharp.text.Chunk("\n\n", ChFont10);
                                        proPhraseQus.Add(proChunkStart);
                                        iTextSharp.text.Chunk proChunkDown = new iTextSharp.text.Chunk(IV_Desc, ChFont10);
                                        proPhraseQus.Add(proChunkDown);


                                        //題目
                                        PdfPCell scell1 = new PdfPCell(proPhraseQus);
                                        scell1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                        scell1.BorderWidthLeft = 1;
                                        scell1.BorderWidthRight = 0;
                                        scell1.BorderWidthTop = 0;
                                        scell1.Colspan = 5;
                                        scell1.Rowspan = 13;
                                        scell1.Padding = 8;
                                        scell1.FixedHeight = (18 * scell1.Rowspan) + 8;
                                        pTable.AddCell(scell1);

                                        //答案
                                        PdfPCell scell2 = new PdfPCell(proPhraseAns);
                                        scell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                        scell2.BorderWidthLeft = 0;
                                        scell2.BorderWidthRight = 1;
                                        scell2.BorderWidthTop = 0;
                                        scell2.Colspan = 4;
                                        scell2.Rowspan = 13;
                                        scell2.Padding = 8;
                                        scell2.FixedHeight = (18 * scell2.Rowspan) + 8;
                                        pTable.AddCell(scell2);

                                        ///--20220715-只有一筆調查，但是題目+答案數>38，存到第二頁的table (_pTable)
                                        ///--20220804-改採用問卷類別來判斷，不用int_totalquestions
                                        ///--20221005-加入頁碼設計
                                        #region --20220715-只有一筆調查，但是題目+答案數>38，存到第二頁的table (_pTable)
                                        if (maintain_type == "communication")
                                        {
                                            if (opinions_from_engineer == "於下方顯示維護查修內容")
                                            {
                                                //20220722-通訊類題目和答案太多，且最後一題答案，需要呈現大量文字
                                                iTextSharp.text.Chunk proChunkDown2 = new iTextSharp.text.Chunk(surveyQnA[surveyQnA.Count - 1].Replace("\n", "\n　 "), ChFont10);
                                                proPhraseQus2.Add(proChunkDown2);

                                            }

                                            ///--20220718--只有一筆調查，但是題目+答案數>38，導致服務說明印不出來
                                            ///--20220804-改採用問卷類別來判斷，不用int_totalquestions
                                            #region --服務說明

                                            //iTextSharp.text.Chunk proChunkStart3 = new iTextSharp.text.Chunk("\n\n服務說明：\n\n", ChFont10);
                                            iTextSharp.text.Chunk proChunkStart3 = new iTextSharp.text.Chunk("\n\n", ChFont10);
                                            proPhraseQus2.Add(proChunkStart);
                                            iTextSharp.text.Chunk proChunkDown3 = new iTextSharp.text.Chunk(IV_Desc, ChFont10);
                                            proPhraseQus2.Add(proChunkDown);

                                            #endregion

                                            #region 頁碼設計 --20221005
                                            var pagenumber = surveyCount + 1;
                                            var row = string.Empty;

                                            if (int_totalquestions <= 50)
                                            {
                                                row = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + "\n\n\n\n\n\n\n\n\n" + "\n\n\n\n\n\n\n\n\n";
                                            }
                                            else
                                            {
                                                row = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + "\n\n\n\n\n\n";
                                            }

                                            iTextSharp.text.Chunk proChunkDown4 = new iTextSharp.text.Chunk(row + "                【服務編號" + IV_SRID + "】第" + pagenumber + "頁，共" + int_totalreports + "頁\n", ChFont8);
                                            proPhraseAns2.Add(proChunkDown4);
                                            #endregion

                                            //題目
                                            PdfPCell scell3 = new PdfPCell(proPhraseQus2);
                                            //PdfPCell scell1 = new PdfPCell(new iTextSharp.text.Phrase("服務說明：\n\n" + processDetail, ChFont10));
                                            scell3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                            scell3.BorderWidthLeft = 1;
                                            scell3.BorderWidthRight = 0;
                                            scell3.BorderWidthTop = 1;
                                            scell3.BorderWidthBottom = 1;
                                            scell3.Colspan = 5;//5改為10會超出範圍
                                            ///20220804-因使用者可能會寫大量紀錄，將Rowspan 從13->16-->改為25 
                                            scell3.Rowspan = 25;//13改為16
                                            scell3.Padding = 8;
                                            scell3.FixedHeight = (18 * scell3.Rowspan) + 8;
                                            _pTable.AddCell(scell3);

                                            //答案
                                            PdfPCell scell4 = new PdfPCell(proPhraseAns2);
                                            scell4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                            scell4.BorderWidthLeft = 0;
                                            scell4.BorderWidthRight = 1;
                                            scell4.BorderWidthTop = 1;
                                            scell4.BorderWidthBottom = 1;
                                            scell4.Colspan = 4;
                                            ///20220804-因使用者可能會寫大量紀錄，將Rowspan 從13->16-->改為25 
                                            scell4.Rowspan = 25;
                                            scell4.Padding = 8;
                                            scell4.FixedHeight = (18 * scell4.Rowspan) + 8;
                                            _pTable.AddCell(scell4);

                                            if (surveyCount % 3 == 2)
                                            {
                                                liPdfPTable.Add(_pTable);
                                            }
                                        }
                                        #endregion

                                    }
                                    //超過一筆的放到第二頁
                                    else
                                    {
                                        //一頁放三筆維護紀錄(第2、5、8筆要另開頁面)
                                        //方框:、方框帶勾:R
                                        iTextSharp.text.Phrase proPhraseQus = new iTextSharp.text.Phrase();
                                        iTextSharp.text.Phrase proPhraseAns = new iTextSharp.text.Phrase();

                                        #region --20220725-太多內容給第二頁、通訊類意見題-Pharse
                                        //太多內容給第二頁用的
                                        iTextSharp.text.Phrase proPhraseQus2 = new iTextSharp.text.Phrase();
                                        iTextSharp.text.Phrase proPhraseAns2 = new iTextSharp.text.Phrase();

                                        //20220722-給通訊類最後一題
                                        iTextSharp.text.Phrase proPhraseAnsUnder = new iTextSharp.text.Phrase();
                                        #endregion

                                        ///--20220804-改採用問卷類別來判斷，不用int_totalquestions
                                        ///若以後有其他問卷超過19題(int_totalquestions>38)，會需要再調整
                                        #region --20220725-假如第2或第3筆的維護紀錄很長
                                        var int_totalquestions = surveyQnA.Count;
                                        if (maintain_type != "communication")
                                        {
                                            //印一頁就可以印完
                                            for (var i = 0; i < surveyQnA.Count; i++)
                                            {
                                                //i是偶數 → 題目
                                                if (i % 2 == 0)
                                                {
                                                    //產品名稱、型號/序號合併成一欄
                                                    if (i == 0)
                                                    {
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("產品資訊：\n\n", ChFont10);
                                                        iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                        proPhraseQus.Add(proChunk1);
                                                        proPhraseQus.Add(proChunk2);
                                                    }
                                                    else if (i == 2) continue;
                                                    else
                                                    {
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i] + "\n", ChFont10);
                                                        iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                        proPhraseQus.Add(proChunk1);
                                                        proPhraseQus.Add(proChunk2);
                                                    }
                                                }
                                                //i是奇數 → 答案
                                                else
                                                {
                                                    if (i == 1)
                                                    {
                                                        //產品名稱、型號/序號合併成一欄
                                                        iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                        string productName = string.IsNullOrEmpty(surveyQnA[3]) ? surveyQnA[1] + "\n\n" : surveyQnA[1] + " (" + surveyQnA[3] + ")\n\n";
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(productName, ChFont10);
                                                        proPhraseAns.Add(proChunk0);
                                                        proPhraseAns.Add(proChunk1);
                                                    }
                                                    else if (i == 3) continue;
                                                    else
                                                    {
                                                        if (surveyQnA[i].Contains("正常"))
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[i].Contains("異常"))
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[i].Contains("無"))
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i].Replace("\n", "\n　 "), ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //一筆調查，內容很多
                                            for (var i = 0; i < int_totalquestions; i++)
                                            {
                                                //i是偶數 → 題目
                                                if (i % 2 == 0)
                                                {
                                                    //產品名稱、型號/序號合併成一欄
                                                    if (i == 0)
                                                    {
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("產品資訊：\n\n", ChFont10);
                                                        iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                        proPhraseQus.Add(proChunk1);
                                                        proPhraseQus.Add(proChunk2);
                                                    }
                                                    else if (i == 2) continue;
                                                    else
                                                    {
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i] + "\n", ChFont10);
                                                        iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk(" ", checkSymbol);
                                                        proPhraseQus.Add(proChunk1);
                                                        proPhraseQus.Add(proChunk2);
                                                    }
                                                }
                                                //i是奇數 → 答案
                                                else
                                                {
                                                    if (i == 1)
                                                    {
                                                        //產品名稱、型號/序號合併成一欄
                                                        iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                        string productName = string.IsNullOrEmpty(surveyQnA[3]) ? surveyQnA[1] + "\n\n" : surveyQnA[1] + " (" + surveyQnA[3] + ")\n\n";
                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(productName, ChFont10);
                                                        proPhraseAns.Add(proChunk0);
                                                        proPhraseAns.Add(proChunk1);
                                                    }
                                                    else if (i == 3) continue;
                                                    else
                                                    {

                                                        ///20220719-定維表-通訊類，檢查者的意見，有一個選項是【本次定期保養均無問題】，會符合原條件
                                                        ///surveyQnA[j].Contains("無")，導致出錯
                                                        ///而且，如果要載入自行填寫，也可能會用到無、異常、正常等國字
                                                        ///修改(surveyQnA[j].Contains("正常")改為(surveyQnA[j]=="正常")
                                                        ///

                                                        if (surveyQnA[i] == "正常")
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[i] == "異常")
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else if (surveyQnA[i] == "無")
                                                        {
                                                            iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                            iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk("正 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk3 = new iTextSharp.text.Chunk("　 異 常 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk4 = new iTextSharp.text.Chunk("", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk5 = new iTextSharp.text.Chunk("　 無 ", ChFont10);
                                                            iTextSharp.text.Chunk proChunk6 = new iTextSharp.text.Chunk("R", checkSymbol);
                                                            iTextSharp.text.Chunk proChunk7 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                            proPhraseAns.Add(proChunk0);
                                                            proPhraseAns.Add(proChunk1);
                                                            proPhraseAns.Add(proChunk2);
                                                            proPhraseAns.Add(proChunk3);
                                                            proPhraseAns.Add(proChunk4);
                                                            proPhraseAns.Add(proChunk5);
                                                            proPhraseAns.Add(proChunk6);
                                                            proPhraseAns.Add(proChunk7);
                                                        }
                                                        else
                                                        {

                                                            //20220719-通訊類定維單，有溫度和濕度，希望pdf加上單位
                                                            var scale = string.Empty;

                                                            if (maintain_type != "communication")
                                                            {
                                                                //非通訊類問卷
                                                                iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                                iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i].Replace("\n", "\n　 "), ChFont10);
                                                                iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                                proPhraseAns.Add(proChunk0);
                                                                proPhraseAns.Add(proChunk1);
                                                                proPhraseAns.Add(proChunk2);

                                                            }
                                                            else if (maintain_type == "communication")
                                                            {

                                                                //通訊類問卷
                                                                if (i != (int_totalquestions - 1))
                                                                {
                                                                    if (i == 5)
                                                                    {
                                                                        scale = "  度C";

                                                                    }
                                                                    else if (i == 7)
                                                                    {
                                                                        scale = "  %";
                                                                    }

                                                                    iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                                    iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i].Replace("\n", "\n　 ") + scale, ChFont10);
                                                                    iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                                    proPhraseAns.Add(proChunk0);
                                                                    proPhraseAns.Add(proChunk1);
                                                                    proPhraseAns.Add(proChunk2);

                                                                }
                                                                else if (i == (int_totalquestions - 1))
                                                                {
                                                                    //20220722-通訊類(題目和答案數總和>38)最後一個項目，有可能輸入大量文字，導致格式與其他不同

                                                                    if (opinions_from_engineer == "於下方顯示維護查修內容")
                                                                    {
                                                                    }
                                                                    else
                                                                    {
                                                                        //opinions_from_engineer==本次定期保養均無問題
                                                                        iTextSharp.text.Chunk proChunk0 = new iTextSharp.text.Chunk(" ", checkSymbol); //加這行才可以跟題目的行高一樣
                                                                        iTextSharp.text.Chunk proChunk1 = new iTextSharp.text.Chunk(surveyQnA[i].Replace("\n", "\n　 ") + scale, ChFont10);
                                                                        iTextSharp.text.Chunk proChunk2 = new iTextSharp.text.Chunk("\n", ChFont10);
                                                                        proPhraseAns.Add(proChunk0);
                                                                        proPhraseAns.Add(proChunk1);
                                                                        proPhraseAns.Add(proChunk2);
                                                                    }
                                                                }
                                                            }



                                                        }
                                                    }
                                                }
                                            }

                                        }
                                        #endregion


                                        ///--20220804-改採用問卷類別來判斷，不用int_totalquestions
                                        ///若以後有其他問卷超過19題(int_totalquestions>38)，會需要再調整
                                        #region --20220725-修改第2.3頁pdf格式
                                        //題目、答案格式設定
                                        if (maintain_type != "communication")
                                        {
                                            //題目
                                            PdfPCell scell1 = new PdfPCell(proPhraseQus);
                                            //PdfPCell scell1 = new PdfPCell(new iTextSharp.text.Phrase("服務說明：\n\n" + processDetail, ChFont10));
                                            scell1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                            scell1.BorderWidthLeft = 1;
                                            scell1.BorderWidthRight = 0;
                                            scell1.BorderWidthTop = 1;
                                            scell1.BorderWidthBottom = 1;
                                            scell1.Colspan = 5;
                                            scell1.Rowspan = 13;
                                            scell1.Padding = 8;
                                            scell1.FixedHeight = (18 * scell1.Rowspan) + 8;
                                            _pTable.AddCell(scell1);

                                            //答案
                                            PdfPCell scell2 = new PdfPCell(proPhraseAns);
                                            scell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                            scell2.BorderWidthLeft = 0;
                                            scell2.BorderWidthRight = 1;
                                            scell2.BorderWidthTop = 1;
                                            scell2.BorderWidthBottom = 1;
                                            scell2.Colspan = 4;
                                            scell2.Rowspan = 13;
                                            scell2.Padding = 8;
                                            scell2.FixedHeight = (18 * scell2.Rowspan) + 8;
                                            _pTable.AddCell(scell2);
                                        }
                                        else
                                        {
                                            if (maintain_type == "communication" && opinions_from_engineer == "於下方顯示維護查修內容")
                                            {
                                                //20220722-通訊類題目和答案太多，且最後一題答案，需要呈現大量文字
                                                iTextSharp.text.Chunk proChunkDown2 = new iTextSharp.text.Chunk(surveyQnA[surveyQnA.Count - 1].Replace("\n", "\n　 "), ChFont10);
                                                proPhraseQus.Add(proChunkDown2);

                                            }

                                            ///--20220718--只有一筆調查，但是題目+答案數>38，導致服務說明印不出來
                                            ///--20220804-改採用問卷類別來判斷，不用int_totalquestions
                                            ///--20221005--加入頁碼
                                            #region --服務說明
                                            var pagenumber = surveyCount + 1;
                                            var row = string.Empty;

                                            //iTextSharp.text.Chunk proChunkStart3 = new iTextSharp.text.Chunk("\n\n服務說明：\n\n", ChFont10);
                                            iTextSharp.text.Chunk proChunkStart3 = new iTextSharp.text.Chunk("\n\n", ChFont10);
                                            proPhraseQus.Add(proChunkStart3);
                                            iTextSharp.text.Chunk proChunkDown3 = new iTextSharp.text.Chunk(IV_Desc, ChFont10);
                                            proPhraseQus.Add(proChunkDown3);
                                            if (int_totalquestions <= 50)
                                            {
                                                row = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + "\n\n\n\n\n\n\n\n\n" + "\n\n\n\n\n\n\n\n\n";
                                            }
                                            else
                                            {
                                                row = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + "\n\n\n\n\n\n\n";
                                            }

                                            iTextSharp.text.Chunk proChunkDown4 = new iTextSharp.text.Chunk(row + "                【服務編號" + IV_SRID + "】第" + pagenumber + "頁，共" + int_totalreports + "頁\n", ChFont8);
                                            proPhraseAns.Add(proChunkDown4);
                                            #endregion

                                            //題目
                                            PdfPCell scell1 = new PdfPCell(proPhraseQus);
                                            scell1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                            scell1.BorderWidthLeft = 1;
                                            scell1.BorderWidthRight = 0;
                                            scell1.BorderWidthTop = 1;
                                            scell1.BorderWidthBottom = 1;
                                            scell1.Colspan = 5;

                                            ///20220718-pdf長度FixedHeight有限制，若把Rowspan = int_totalquestions=66; FixedHeight最終=1196會看不到文字
                                            ///把RowSpan=26，會比較剛好
                                            ///假若有天題目+答案數>66，會需要再調
                                            ///20220804-因使用者可能會寫大量紀錄，將Rowspan 從13->26-->改為40 ///
                                            scell1.Rowspan = 40;
                                            scell1.Padding = 8;
                                            scell1.FixedHeight = (18 * scell1.Rowspan) + 8;
                                            _pTable.AddCell(scell1);

                                            //答案
                                            PdfPCell scell2 = new PdfPCell(proPhraseAns);
                                            scell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                            scell2.BorderWidthLeft = 0;
                                            scell2.BorderWidthRight = 1;
                                            scell2.BorderWidthTop = 1;
                                            scell2.BorderWidthBottom = 1;
                                            scell2.Colspan = 4;
                                            scell2.Rowspan = 40;
                                            scell2.Padding = 8;
                                            scell2.FixedHeight = (18 * scell2.Rowspan) + 8;
                                            _pTable.AddCell(scell2);
                                        }

                                        #endregion

                                        if (surveyCount % 3 == 2)
                                        {
                                            liPdfPTable.Add(_pTable);
                                        }
                                    }
                                }
                            }
                        }

                        //定維case且有問券ID及問券答案ID
                        if (IV_SRID.Substring(0, 2) == "65" && !string.IsNullOrEmpty(svid) && !string.IsNullOrEmpty(hash))
                        {

                        }
                        //其他照舊，單純顯示處理過程
                        else
                        {
                            //PdfPCell scell1 = new PdfPCell(new iTextSharp.text.Phrase("服務說明：\n\n" + IV_Desc, ChFont10));
                            PdfPCell scell1 = new PdfPCell(new iTextSharp.text.Phrase("\n\n" + IV_Desc, ChFont10));
                            scell1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                            scell1.BorderWidthLeft = 1;
                            scell1.BorderWidthRight = 1;
                            scell1.BorderWidthTop = 0;
                            scell1.Colspan = 8;
                            scell1.Rowspan = 13;
                            scell1.Padding = 8;
                            scell1.FixedHeight = (18 * scell1.Rowspan) + 8;
                            pTable.AddCell(scell1);
                        }
                        #endregion

                        #region // 零件、料號、客戶意見
                        PdfPCell ccell1 = new PdfPCell(new iTextSharp.text.Phrase("更換零件名稱", ChFont10));
                        ccell1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        ccell1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        ccell1.BorderWidthLeft = 1;
                        ccell1.BorderWidthRight = 0;
                        ccell1.BorderWidthTop = 0;
                        ccell1.Colspan = 2;
                        ccell1.Padding = 4;
                        pTable.AddCell(ccell1);

                        PdfPCell ccell2 = new PdfPCell(new iTextSharp.text.Phrase("料號或序號", ChFont10));
                        ccell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        ccell2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        ccell2.BorderWidthRight = 0;
                        ccell2.BorderWidthTop = 0;
                        ccell2.Colspan = 2;
                        ccell2.Padding = 4;
                        pTable.AddCell(ccell2);

                        string tProcessWay = string.Empty;
                        if (IV_SRReportType == SRReportType.ONLINE || IV_SRReportType == SRReportType.REMOTE)
                        {
                            tProcessWay = IV_SRReportType == SRReportType.ONLINE ? "線上" : "遠端";
                            tProcessWay = "處理方式-" + tProcessWay + "\n\n";
                        }

                        string SLASRV = (srdetail["EV_SLASRV"] != null) ? srdetail["EV_SLASRV"].ToString() : "";
                        string WTYKIND = (srdetail["EV_WTYKIND"] != null) ? srdetail["EV_WTYKIND"].ToString() : "";
                        IV_CusOpinion = (String.IsNullOrEmpty(IV_CusOpinion) || String.Compare(IV_CusOpinion, "null", true) == 0) ? "" : IV_CusOpinion;

                        PdfPCell ccell3 = new PdfPCell(new iTextSharp.text.Phrase("客戶意見 / 備註：\n\n" + tProcessWay + IV_CusOpinion + "\n\n" + SLASRV + "\n\n" + WTYKIND, ChFont10));
                        ccell3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        ccell3.BorderWidthRight = 0;
                        ccell3.BorderWidthTop = 0;
                        ccell3.BorderWidthBottom = 1;
                        ccell3.Colspan = 2;
                        ccell3.Rowspan = 8;
                        ccell3.Padding = 4;
                        pTable.AddCell(ccell3);

                        PdfPCell ccell4 = new PdfPCell(new iTextSharp.text.Phrase("服務工程師", ChFont10));
                        ccell4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                        ccell4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        ccell4.BorderWidthRight = 1;
                        ccell4.BorderWidthTop = 0;
                        ccell4.Colspan = 2;
                        ccell4.Padding = 4;
                        pTable.AddCell(ccell4);

                        List<XCLIST> xclist = srdetail.ContainsKey("table_ET_XCLIST") ? (List<XCLIST>)srdetail["table_ET_XCLIST"] : new List<XCLIST>();

                        for (int i = 0; i < 7; i++)
                        {
                            string CHANGEPART1 = (xclist != null && xclist.Count > i) ? xclist[i].CHANGEPARTNAME : "";  //更換零件名稱
                            string CHANGEPART2 = (xclist != null && xclist.Count > i) ? xclist[i].CHANGEPART : "";      //料號或序號

                            PdfPCell tmpcell1 = new PdfPCell(new iTextSharp.text.Phrase(CHANGEPART1, ChFont10));
                            tmpcell1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            tmpcell1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            tmpcell1.BorderWidthLeft = 1;
                            tmpcell1.BorderWidthRight = 0;
                            tmpcell1.BorderWidthTop = 0;
                            if (i == 6) tmpcell1.BorderWidthBottom = 1;
                            tmpcell1.Colspan = 2;
                            tmpcell1.Padding = 4;
                            pTable.AddCell(tmpcell1);

                            PdfPCell tmpcell2 = new PdfPCell(new iTextSharp.text.Phrase(CHANGEPART2, ChFont10));
                            tmpcell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            tmpcell2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                            tmpcell2.BorderWidthRight = 0;
                            tmpcell2.BorderWidthTop = 0;
                            if (i == 6) tmpcell2.BorderWidthBottom = 1;
                            tmpcell2.Colspan = 2;
                            tmpcell2.Padding = 4;
                            pTable.AddCell(tmpcell2);

                            switch (i)
                            {
                                case 0:
                                    // HP 認證工程師 EV_SQ
                                    string EV_SQ = srdetail["EV_SQ"].ToString();
                                    ENGINEER += (String.IsNullOrEmpty(EV_SQ)) ? "" : "\n\n" + EV_SQ;

                                    PdfPCell tmpcell3 = new PdfPCell(new iTextSharp.text.Phrase(ENGINEER, ChFont10));
                                    tmpcell3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                                    tmpcell3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                                    tmpcell3.BorderWidthRight = 1;
                                    tmpcell3.BorderWidthTop = 0;
                                    tmpcell3.Colspan = 2;
                                    tmpcell3.Rowspan = 2;
                                    tmpcell3.Padding = 4;
                                    pTable.AddCell(tmpcell3);

                                    break;
                                case 2:
                                    PdfPCell tmpcell4 = new PdfPCell(new iTextSharp.text.Phrase("客戶簽章", ChFont10));
                                    tmpcell4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                    tmpcell4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                                    tmpcell4.BorderWidthRight = 1;
                                    tmpcell4.BorderWidthTop = 0;
                                    tmpcell4.Colspan = 2;
                                    tmpcell4.Padding = 4;
                                    pTable.AddCell(tmpcell4);

                                    break;
                                case 3:
                                    // 簽名圖檔
                                    if (IV_SRReportType == SRReportType.SIGN)
                                    {
                                        iTextSharp.text.Image img1 = iTextSharp.text.Image.GetInstance(pngPath); // 正式用
                                        img1.RotationDegrees = 90; //counterclockwise                                                         
                                        img1.ScaleAbsolute(60f, 124f);

                                        PdfPCell tmpcell5 = new PdfPCell(img1);
                                        tmpcell5.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                                        tmpcell5.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                                        tmpcell5.BorderWidthRight = 1;
                                        tmpcell5.BorderWidthTop = 0;
                                        tmpcell5.BorderWidthBottom = 1;
                                        tmpcell5.Colspan = 2;
                                        tmpcell5.Rowspan = 4;
                                        tmpcell5.Padding = 4;
                                        pTable.AddCell(tmpcell5);
                                    }
                                    else if (IV_SRReportType == SRReportType.ONLINE || IV_SRReportType == SRReportType.REMOTE)
                                    {
                                        #region 線上和遠端先不放在客戶簽章欄位
                                        //string tShowText = IV_SRReportType == SRReportType.ONLINE ? "線上" : "遠端";
                                        string tShowText = string.Empty;

                                        PdfPCell tmpcell5 = new PdfPCell(new iTextSharp.text.Phrase(tShowText, ChFont10));
                                        tmpcell5.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                                        tmpcell5.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                                        tmpcell5.BorderWidthRight = 1;
                                        tmpcell5.BorderWidthTop = 0;
                                        tmpcell5.BorderWidthBottom = 1;
                                        tmpcell5.Colspan = 2;
                                        tmpcell5.Rowspan = 4;
                                        tmpcell5.Padding = 4;
                                        tmpcell5.FixedHeight = 75;
                                        pTable.AddCell(tmpcell5);
                                        #endregion
                                    }

                                    break;
                            }
                        }

                        #endregion

                        #region // 表尾
                        // 空白行
                        PdfPCell espace = new PdfPCell(new iTextSharp.text.Phrase("　", new iTextSharp.text.Font(bfChinese, 1)));
                        espace.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        espace.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                        espace.Border = iTextSharp.text.Rectangle.NO_BORDER; // 無邊框                    
                        espace.Colspan = 8;
                        pTable.AddCell(espace);

                        PdfPTable uTable = new PdfPTable(new float[] { 1 });
                        uTable.SpacingBefore = 2;
                        uTable.WidthPercentage = 92;
                        uTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER; // 無邊框

                        #region 20221005-通訊類問卷，第一頁加入頁碼
                        var endof85 = string.Empty;
                        var totalpage = surveyCount + 1;
                        if (IV_SRID.Substring(0, 2) == "65")
                        {
                            if (maintain_type == "communication")
                            {
                                endof85 = "                                     " + "【服務編號" + IV_SRID + "】第1頁，共" + totalpage + "頁";
                            }

                        }
                        #endregion


                        PdfPCell ucell1 = new PdfPCell(new iTextSharp.text.Phrase("* 如果您對本此服務滿意，請您在後續的滿意度抽樣訪問中回答「非常滿意」，謝謝。" + endof85, ChFont8));
                        ucell1.Border = PdfPCell.NO_BORDER;
                        uTable.AddCell(ucell1);

                        PdfPCell ucell2 = new PdfPCell(new iTextSharp.text.Phrase("** 如有任何建議，煩請提供服務編號及意見，傳送至0800@etatung.com，謝謝。", ChFont8));
                        ucell2.Border = PdfPCell.NO_BORDER;
                        uTable.AddCell(ucell2);

                        #endregion

                        pMsg += "PDF-Foot" + Environment.NewLine;

                        string fileOrgName = string.Empty;
                        string fileName = string.Empty;
                        string pdfPath = string.Empty;

                        // 產生 PDF
                        var doc1 = new iTextSharp.text.Document();

                        try
                        {
                            doc1 = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 15, 15); // left, right, top, bottom


                            #region 設定pdf檔案相關路徑
                            Guid fileGuid = Guid.NewGuid();

                            cReportID = CMF.GetReportSerialID(IV_SRID);

                            fileOrgName = cReportID + ".pdf";
                            fileName = fileGuid.ToString() + ".pdf";
                            pdfPath = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                            #endregion

                            #region table部份      
                            TB_ONE_DOCUMENT bean = new TB_ONE_DOCUMENT();

                            bean.ID = fileGuid;
                            bean.FILE_ORG_NAME = fileOrgName;
                            bean.FILE_NAME = fileName;
                            bean.FILE_EXT = ".pdf";
                            bean.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            dbOne.TB_ONE_DOCUMENT.Add(bean);
                            dbOne.SaveChanges();
                            #endregion

                            PdfWriter pdfwriter = PdfWriter.GetInstance(doc1, new FileStream(pdfPath, FileMode.Create));

                            doc1.Open();
                            doc1.Add(hTable);
                            doc1.Add(pTable);
                            doc1.Add(uTable);
                            doc1.AddTitle("[大同世界科技 服務案件ID：" + IV_SRID + " 已處理通知]");
                            doc1.AddAuthor("大同世界科技");
                            doc1.AddSubject("服務報告書");
                            doc1.AddKeywords("TSTI, SAP CRM, PDF");
                            doc1.AddCreator("Using iTextSharp");

                            //超過一筆的定維紀錄，需要放到第二頁
                            if (isOver1Survey)
                            {
                                doc1.NewPage();
                                doc1.Add(_pTable);
                            }

                            OUTBean.EV_MSGT = "Y";
                            OUTBean.EV_MSG = "";
                            OUTBean.EV_FILENAME = fileName;
                        }
                        catch (Exception ex)
                        {
                            pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因(Create PDF error):PDF沒產生QQ" + Environment.NewLine;
                            pMsg += " 失敗行數：" + ex.ToString();

                            CMF.writeToLog(IV_SRID, "UploadSignToPdf_API", pMsg, IV_EMPNONAME);
                            CMF.SendMailByAPI("UploadSignToPdf_API", null, "leon.huang@etatung.com;elvis.chang@etatung.com", "", "", "UploadSignToPdf_API錯誤 - " + IV_SRID, pMsg, null, null);
                        }
                        finally
                        {
                            if (doc1.IsOpen()) doc1.Close();
                        }
                        #endregion

                        pMsg += "PDF產生並上傳成功！" + Environment.NewLine;
                        CMF.writeToLog(IV_SRID, "UploadSignToPdf_API", pMsg, IV_EMPNONAME);
                    }
                    else
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因(File upload failed):參數沒問題，但PDF沒產生QQ" + Environment.NewLine;

                        CMF.writeToLog(IV_SRID, "UploadSignToPdf_API", pMsg, IV_EMPNONAME);
                        CMF.SendMailByAPI("UploadSignToPdf_API", null, "leon.huang@etatung.com;elvis.chang@etatung.com", "", "", "UploadSignToPdf_API錯誤 - " + IV_SRID, pMsg, null, null);
                    }
                    #endregion
                }
                else if (IV_SRReportType == SRReportType.NOSIGN) //無簽名檔
                {
                    #region 無簽名檔
                    if (uploadFiles != null && uploadFiles.Length > 0)
                    {
                        try
                        {
                            TB_ONE_DOCUMENT bean = new TB_ONE_DOCUMENT();

                            List<string> picPathList = new List<string>();

                            Guid fileGuid = Guid.NewGuid();

                            string fileId = string.Empty;
                            string fileOrgName = string.Empty;
                            string fileName = string.Empty;
                            path = string.Empty;

                            foreach (var upload1 in uploadFiles)
                            {
                                if (upload1 != null)
                                {
                                    fileGuid = Guid.NewGuid();

                                    fileId = fileGuid.ToString();
                                    fileOrgName = upload1.FileName;
                                    fileName = fileId + Path.GetExtension(upload1.FileName);
                                    path = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                                    upload1.SaveAs(path);

                                    picPathList.Add(path);
                                }
                            }

                            #region 將圖片轉成一份pdf
                            fileGuid = Guid.NewGuid();
                            cSRReport = fileGuid.ToString() + ",";

                            bool tIsOK = UploadMultPics(picPathList, fileGuid.ToString(), IV_SRID, IV_EMPNONAME);
                            #endregion

                            if (tIsOK)
                            {
                                #region 設定pdf檔案相關路徑
                                cReportID = CMF.GetReportSerialID(IV_SRID);

                                fileOrgName = cReportID + ".pdf";
                                fileName = fileGuid.ToString() + ".pdf";

                                cPDFPath = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                                cPDFFileName = fileName;
                                #endregion

                                #region table部份                                        
                                bean.ID = fileGuid;
                                bean.FILE_ORG_NAME = fileOrgName;
                                bean.FILE_NAME = fileName;
                                bean.FILE_EXT = ".pdf";
                                bean.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                dbOne.TB_ONE_DOCUMENT.Add(bean);
                                dbOne.SaveChanges();
                                #endregion

                                OUTBean.EV_MSGT = "Y";
                                OUTBean.EV_MSG = "";
                                OUTBean.EV_FILENAME = fileName;
                            }
                            else
                            {
                                OUTBean.EV_MSGT = "E";
                                OUTBean.EV_MSG = "【無簽名檔】圖片轉PDF失敗！";
                                OUTBean.EV_FILENAME = "";
                            }
                        }
                        catch (Exception ex)
                        {
                            pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "服務報告書/附件上傳失敗原因:" + ex.Message + Environment.NewLine;
                            pMsg += " 失敗行數：" + ex.ToString() + Environment.NewLine;

                            CMF.writeToLog(IV_SRID, "UploadSignToPdf_NOSIGN_API", pMsg, IV_EMPNONAME);
                            CMF.SendMailByAPI("UploadSignToPdf_NOSIGN_API", null, "leon.huang@etatung.com;elvis.chang@etatung.com", "", "", "UploadSignToPdf_NOSIGN_API錯誤 - " + IV_SRID, pMsg, null, null);
                        }
                    }
                    #endregion
                }
                else //純附件
                {
                    #region 純附件
                    if (uploadFiles != null && uploadFiles.Length > 0)
                    {
                        try
                        {
                            Guid fileGuid = Guid.NewGuid();

                            string fileId = string.Empty;
                            string fileOrgName = string.Empty;
                            string fileName = string.Empty;
                            string fileALLName = string.Empty;

                            foreach (var upload1 in uploadFiles)
                            {
                                if (upload1 != null)
                                {
                                    #region 檔案部份
                                    fileGuid = Guid.NewGuid();

                                    cSRReport += fileGuid.ToString() + ",";

                                    fileId = fileGuid.ToString();
                                    fileOrgName = upload1.FileName;
                                    fileName = fileId + Path.GetExtension(upload1.FileName);
                                    path = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                                    upload1.SaveAs(path);
                                    #endregion

                                    #region table部份                                        
                                    TB_ONE_DOCUMENT bean = new TB_ONE_DOCUMENT();

                                    bean.ID = fileGuid;
                                    bean.FILE_ORG_NAME = fileOrgName;
                                    bean.FILE_NAME = fileName;
                                    bean.FILE_EXT = Path.GetExtension(upload1.FileName);
                                    bean.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                    dbOne.TB_ONE_DOCUMENT.Add(bean);
                                    dbOne.SaveChanges();

                                    fileALLName += fileName + ",";
                                    #endregion
                                }
                            }

                            OUTBean.EV_MSGT = "Y";
                            OUTBean.EV_MSG = "";
                            OUTBean.EV_FILENAME = fileALLName.TrimEnd(',');
                        }
                        catch (Exception ex)
                        {
                            pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "【ATTACH】服務報告書/附件上傳失敗原因:" + ex.Message + Environment.NewLine;
                            pMsg += " 失敗行數：" + ex.ToString() + Environment.NewLine;

                            CMF.writeToLog(IV_SRID, "UploadSignToPdf_ATTACH_API", pMsg, IV_EMPNONAME);
                            CMF.SendMailByAPI("UploadSignToPdf_ATTACH_API", null, "leon.huang@etatung.com;elvis.chang@etatung.com", "", "", "UploadSignToPdf_ATTACH_API錯誤 - " + IV_SRID, pMsg, null, null);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因(File upload failed):PDF沒產生QQ" + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_SRID, "UploadSignToPdf_API", pMsg, IV_EMPNONAME);
                CMF.SendMailByAPI("UploadSignToPdf_API", null, "leon.huang@etatung.com;elvis.chang@etatung.com", "", "", "UploadSignToPdf_API錯誤 - " + IV_SRID, pMsg, null, null);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
                OUTBean.EV_FILENAME = "";
            }

            return OUTBean;
        }
        #endregion        

        #region -- 取得定維問券題目及答案 --
        /// <summary>
        /// 取得定維問券題目及答案
        /// </summary>
        /// <param name="svid">SurveyCake問券ID</param>
        /// <param name="hash">問券答案ID</param>
        /// <returns></returns>
        public List<string> GetSurveyMaintainQnA(string svid, string hash)
        {
            var ansBean = appDB.TB_SURVEY_ANS_MAINTAIN.FirstOrDefault(x => x.svid == svid && x.hash == hash);
            var qusBean = appDB.TB_SURVEY_QUS_MAINTAIN.Where(x => x.DISABLED == 0).OrderBy(x => x.SORT);
            List<string> liQnA = new List<string>();
            Dictionary<string, string> dicQnA = new Dictionary<string, string>();
            if (ansBean != null)
            {
                if (!dicQnA.ContainsKey("product_name")) dicQnA.Add("product_name", ansBean.product_name);
                if (!dicQnA.ContainsKey("product_serial_no")) dicQnA.Add("product_serial_no", ansBean.product_serial_no);

                if (!dicQnA.ContainsKey("ho_panel_led") && !string.IsNullOrEmpty(ansBean.ho_panel_led)) dicQnA.Add("ho_panel_led", ansBean.ho_panel_led);
                if (!dicQnA.ContainsKey("ho_cooling_fan") && !string.IsNullOrEmpty(ansBean.ho_cooling_fan)) dicQnA.Add("ho_cooling_fan", ansBean.ho_cooling_fan);
                if (!dicQnA.ContainsKey("ho_power_module") && !string.IsNullOrEmpty(ansBean.ho_power_module)) dicQnA.Add("ho_power_module", ansBean.ho_power_module);
                if (!dicQnA.ContainsKey("ho_raid_panel_led") && !string.IsNullOrEmpty(ansBean.ho_raid_panel_led)) dicQnA.Add("ho_raid_panel_led", ansBean.ho_raid_panel_led);
                if (!dicQnA.ContainsKey("ho_raid_cooling_fan") && !string.IsNullOrEmpty(ansBean.ho_raid_cooling_fan)) dicQnA.Add("ho_raid_cooling_fan", ansBean.ho_raid_cooling_fan);
                if (!dicQnA.ContainsKey("ho_raid_power_module") && !string.IsNullOrEmpty(ansBean.ho_raid_power_module)) dicQnA.Add("ho_raid_power_module", ansBean.ho_raid_power_module);
                if (!dicQnA.ContainsKey("ho_raid_event_log") && !string.IsNullOrEmpty(ansBean.ho_raid_event_log)) dicQnA.Add("ho_raid_event_log", ansBean.ho_raid_event_log);
                if (!dicQnA.ContainsKey("ho_manage_interface_event_log") && !string.IsNullOrEmpty(ansBean.ho_manage_interface_event_log)) dicQnA.Add("ho_manage_interface_event_log", ansBean.ho_manage_interface_event_log);
                if (!dicQnA.ContainsKey("ho_os_event_log") && !string.IsNullOrEmpty(ansBean.ho_os_event_log)) dicQnA.Add("ho_os_event_log", ansBean.ho_os_event_log);
                if (!dicQnA.ContainsKey("ho_equip_network_light") && !string.IsNullOrEmpty(ansBean.ho_equip_network_light)) dicQnA.Add("ho_equip_network_light", ansBean.ho_equip_network_light);
                if (!dicQnA.ContainsKey("ho_others") && !string.IsNullOrEmpty(ansBean.ho_others)) dicQnA.Add("ho_others", ansBean.ho_others);

                if (!dicQnA.ContainsKey("net_backup_config") && !string.IsNullOrEmpty(ansBean.net_backup_config)) dicQnA.Add("net_backup_config", ansBean.net_backup_config);
                if (!dicQnA.ContainsKey("net_log") && !string.IsNullOrEmpty(ansBean.net_log)) dicQnA.Add("net_log", ansBean.net_log);
                if (!dicQnA.ContainsKey("net_cpu") && !string.IsNullOrEmpty(ansBean.net_cpu)) dicQnA.Add("net_cpu", ansBean.net_cpu);
                if (!dicQnA.ContainsKey("net_memory") && !string.IsNullOrEmpty(ansBean.net_memory)) dicQnA.Add("net_memory", ansBean.net_memory);
                if (!dicQnA.ContainsKey("net_port") && !string.IsNullOrEmpty(ansBean.net_port)) dicQnA.Add("net_port", ansBean.net_port);
                if (!dicQnA.ContainsKey("net_others") && !string.IsNullOrEmpty(ansBean.net_others)) dicQnA.Add("net_others", ansBean.net_others);

                if (!dicQnA.ContainsKey("sto_panel_light") && !string.IsNullOrEmpty(ansBean.sto_panel_light)) dicQnA.Add("sto_panel_light", ansBean.sto_panel_light);
                if (!dicQnA.ContainsKey("sto_cooling_fan") && !string.IsNullOrEmpty(ansBean.sto_cooling_fan)) dicQnA.Add("sto_cooling_fan", ansBean.sto_cooling_fan);
                if (!dicQnA.ContainsKey("sto_power_module") && !string.IsNullOrEmpty(ansBean.sto_power_module)) dicQnA.Add("sto_power_module", ansBean.sto_power_module);
                if (!dicQnA.ContainsKey("sto_network_light") && !string.IsNullOrEmpty(ansBean.sto_network_light)) dicQnA.Add("sto_network_light", ansBean.sto_network_light);
                if (!dicQnA.ContainsKey("sto_fc_iscsi_light") && !string.IsNullOrEmpty(ansBean.sto_fc_iscsi_light)) dicQnA.Add("sto_fc_iscsi_light", ansBean.sto_fc_iscsi_light);
                if (!dicQnA.ContainsKey("sto_is_severe_error") && !string.IsNullOrEmpty(ansBean.sto_is_severe_error)) dicQnA.Add("sto_is_severe_error", ansBean.sto_is_severe_error);
                if (!dicQnA.ContainsKey("sto_capacity_over_20") && !string.IsNullOrEmpty(ansBean.sto_capacity_over_20)) dicQnA.Add("sto_capacity_over_20", ansBean.sto_capacity_over_20);
                if (!dicQnA.ContainsKey("sto_iops_over_60") && !string.IsNullOrEmpty(ansBean.sto_iops_over_60)) dicQnA.Add("sto_iops_over_60", ansBean.sto_iops_over_60);
                if (!dicQnA.ContainsKey("sto_others") && !string.IsNullOrEmpty(ansBean.sto_others)) dicQnA.Add("sto_others", ansBean.sto_others);

                if (!dicQnA.ContainsKey("vm_esxi_host_cpu") && !string.IsNullOrEmpty(ansBean.vm_esxi_host_cpu)) dicQnA.Add("vm_esxi_host_cpu", ansBean.vm_esxi_host_cpu);
                if (!dicQnA.ContainsKey("vm_esxi_host_memory") && !string.IsNullOrEmpty(ansBean.vm_esxi_host_memory)) dicQnA.Add("vm_esxi_host_memory", ansBean.vm_esxi_host_memory);
                if (!dicQnA.ContainsKey("vm_esxi_datastore") && !string.IsNullOrEmpty(ansBean.vm_esxi_datastore)) dicQnA.Add("vm_esxi_datastore", ansBean.vm_esxi_datastore);
                if (!dicQnA.ContainsKey("vm_guest_os_cpu") && !string.IsNullOrEmpty(ansBean.vm_guest_os_cpu)) dicQnA.Add("vm_guest_os_cpu", ansBean.vm_guest_os_cpu);
                if (!dicQnA.ContainsKey("vm_guest_os_memory") && !string.IsNullOrEmpty(ansBean.vm_guest_os_memory)) dicQnA.Add("vm_guest_os_memory", ansBean.vm_guest_os_memory);
                if (!dicQnA.ContainsKey("vm_high_availability_state") && !string.IsNullOrEmpty(ansBean.vm_high_availability_state)) dicQnA.Add("vm_high_availability_state", ansBean.vm_high_availability_state);
                if (!dicQnA.ContainsKey("vm_esxi_event_log") && !string.IsNullOrEmpty(ansBean.vm_esxi_event_log)) dicQnA.Add("vm_esxi_event_log", ansBean.vm_esxi_event_log);
                if (!dicQnA.ContainsKey("vm_horizon_view") && !string.IsNullOrEmpty(ansBean.vm_horizon_view)) dicQnA.Add("vm_horizon_view", ansBean.vm_horizon_view);
                if (!dicQnA.ContainsKey("vm_nsx") && !string.IsNullOrEmpty(ansBean.vm_nsx)) dicQnA.Add("vm_nsx", ansBean.vm_nsx);
                if (!dicQnA.ContainsKey("vm_vrops") && !string.IsNullOrEmpty(ansBean.vm_vrops)) dicQnA.Add("vm_vrops", ansBean.vm_vrops);
                if (!dicQnA.ContainsKey("vm_others") && !string.IsNullOrEmpty(ansBean.vm_others)) dicQnA.Add("vm_others", ansBean.vm_others);

                if (!dicQnA.ContainsKey("hy_event_log") && !string.IsNullOrEmpty(ansBean.hy_event_log)) dicQnA.Add("hy_event_log", ansBean.hy_event_log);
                if (!dicQnA.ContainsKey("hy_cluster_health") && !string.IsNullOrEmpty(ansBean.hy_cluster_health)) dicQnA.Add("hy_cluster_health", ansBean.hy_cluster_health);
                if (!dicQnA.ContainsKey("hy_datastore_usage") && !string.IsNullOrEmpty(ansBean.hy_datastore_usage)) dicQnA.Add("hy_datastore_usage", ansBean.hy_datastore_usage);
                if (!dicQnA.ContainsKey("hy_usage") && !string.IsNullOrEmpty(ansBean.hy_usage)) dicQnA.Add("hy_usage", ansBean.hy_usage);
                if (!dicQnA.ContainsKey("hy_availability_state") && !string.IsNullOrEmpty(ansBean.hy_availability_state)) dicQnA.Add("hy_availability_state", ansBean.hy_availability_state);
                if (!dicQnA.ContainsKey("hy_others") && !string.IsNullOrEmpty(ansBean.hy_others)) dicQnA.Add("hy_others", ansBean.hy_others);

                if (!dicQnA.ContainsKey("ad_event_log") && !string.IsNullOrEmpty(ansBean.ad_event_log)) dicQnA.Add("ad_event_log", ansBean.ad_event_log);
                if (!dicQnA.ContainsKey("ad_dcdiag_check") && !string.IsNullOrEmpty(ansBean.ad_dcdiag_check)) dicQnA.Add("ad_dcdiag_check", ansBean.ad_dcdiag_check);
                if (!dicQnA.ContainsKey("ad_replication_health") && !string.IsNullOrEmpty(ansBean.ad_replication_health)) dicQnA.Add("ad_replication_health", ansBean.ad_replication_health);
                if (!dicQnA.ContainsKey("ad_kcc_health") && !string.IsNullOrEmpty(ansBean.ad_kcc_health)) dicQnA.Add("ad_kcc_health", ansBean.ad_kcc_health);
                if (!dicQnA.ContainsKey("ad_others") && !string.IsNullOrEmpty(ansBean.ad_others)) dicQnA.Add("ad_others", ansBean.ad_others);

                if (!dicQnA.ContainsKey("ws_event_log") && !string.IsNullOrEmpty(ansBean.ws_event_log)) dicQnA.Add("ws_event_log", ansBean.ws_event_log);
                if (!dicQnA.ContainsKey("ws_services_health") && !string.IsNullOrEmpty(ansBean.ws_services_health)) dicQnA.Add("ws_services_health", ansBean.ws_services_health);
                if (!dicQnA.ContainsKey("ws_update_patch_approved") && !string.IsNullOrEmpty(ansBean.ws_update_patch_approved)) dicQnA.Add("ws_update_patch_approved", ansBean.ws_update_patch_approved);
                if (!dicQnA.ContainsKey("ws_others") && !string.IsNullOrEmpty(ansBean.ws_others)) dicQnA.Add("ws_others", ansBean.ws_others);

                if (!dicQnA.ContainsKey("net_eq_ip") && !string.IsNullOrEmpty(ansBean.net_eq_ip)) dicQnA.Add("net_eq_ip", ansBean.net_eq_ip);
                if (!dicQnA.ContainsKey("net_eq_light") && !string.IsNullOrEmpty(ansBean.net_eq_light)) dicQnA.Add("net_eq_light", ansBean.net_eq_light);
                if (!dicQnA.ContainsKey("net_eq_contact_loose") && !string.IsNullOrEmpty(ansBean.net_eq_contact_loose)) dicQnA.Add("net_eq_contact_loose", ansBean.net_eq_contact_loose);
                if (!dicQnA.ContainsKey("net_eq_port") && !string.IsNullOrEmpty(ansBean.net_eq_port)) dicQnA.Add("net_eq_port", ansBean.net_eq_port);
                if (!dicQnA.ContainsKey("net_eq_log_information") && !string.IsNullOrEmpty(ansBean.net_eq_log_information)) dicQnA.Add("net_eq_log_information", ansBean.net_eq_log_information);
                if (!dicQnA.ContainsKey("net_eq_others") && !string.IsNullOrEmpty(ansBean.net_eq_others)) dicQnA.Add("net_eq_others", ansBean.net_eq_others);

                if (!dicQnA.ContainsKey("others") && !string.IsNullOrEmpty(ansBean.others)) dicQnA.Add("others", ansBean.others);

                #region --20220711-通訊類的問題               
                if (!dicQnA.ContainsKey("env_temperature") && !string.IsNullOrEmpty(ansBean.env_temperature)) dicQnA.Add("env_temperature", ansBean.env_temperature);
                if (!dicQnA.ContainsKey("env_humidity") && !string.IsNullOrEmpty(ansBean.env_humidity)) dicQnA.Add("env_humidity", ansBean.env_humidity);
                if (!dicQnA.ContainsKey("env_ventilation") && !string.IsNullOrEmpty(ansBean.env_ventilation)) dicQnA.Add("env_ventilation", ansBean.env_ventilation);
                if (!dicQnA.ContainsKey("env_cleanness") && !string.IsNullOrEmpty(ansBean.env_cleanness)) dicQnA.Add("env_cleanness", ansBean.env_cleanness);

                if (!dicQnA.ContainsKey("power_backup_system") && !string.IsNullOrEmpty(ansBean.power_backup_system)) dicQnA.Add("power_backup_system", ansBean.power_backup_system);
                if (!dicQnA.ContainsKey("powg_type") && !string.IsNullOrEmpty(ansBean.powg_type)) dicQnA.Add("powg_type", ansBean.powg_type);
                if (!dicQnA.ContainsKey("powg_appearance") && !string.IsNullOrEmpty(ansBean.powg_appearance)) dicQnA.Add("powg_appearance", ansBean.powg_appearance);
                if (!dicQnA.ContainsKey("powg_unit_voltage") && !string.IsNullOrEmpty(ansBean.powg_unit_voltage)) dicQnA.Add("powg_unit_voltage", ansBean.powg_unit_voltage);
                if (!dicQnA.ContainsKey("powg_sum_voltage") && !string.IsNullOrEmpty(ansBean.powg_sum_voltage)) dicQnA.Add("powg_sum_voltage", ansBean.powg_sum_voltage);

                if (!dicQnA.ContainsKey("pows_type") && !string.IsNullOrEmpty(ansBean.pows_type)) dicQnA.Add("pows_type", ansBean.pows_type);
                if (!dicQnA.ContainsKey("pows_ac_ups_voltage") && !string.IsNullOrEmpty(ansBean.pows_ac_ups_voltage)) dicQnA.Add("pows_ac_ups_voltage", ansBean.pows_ac_ups_voltage);
                if (!dicQnA.ContainsKey("pows_dc_charger_voltage") && !string.IsNullOrEmpty(ansBean.pows_dc_charger_voltage)) dicQnA.Add("pows_dc_charger_voltage", ansBean.pows_dc_charger_voltage);
                if (!dicQnA.ContainsKey("pows_charger_value") && !string.IsNullOrEmpty(ansBean.pows_charger_value)) dicQnA.Add("pows_charger_value", ansBean.pows_charger_value);

                if (!dicQnA.ContainsKey("swi_filter_ventilation") && !string.IsNullOrEmpty(ansBean.swi_filter_ventilation)) dicQnA.Add("swi_filter_ventilation", ansBean.swi_filter_ventilation);
                if (!dicQnA.ContainsKey("swi_suppllier_led") && !string.IsNullOrEmpty(ansBean.swi_suppllier_led)) dicQnA.Add("swi_suppllier_led", ansBean.swi_suppllier_led);
                if (!dicQnA.ContainsKey("swi_interface_led") && !string.IsNullOrEmpty(ansBean.swi_interface_led)) dicQnA.Add("swi_interface_led", ansBean.swi_interface_led);
                if (!dicQnA.ContainsKey("swi_hardware_test") && !string.IsNullOrEmpty(ansBean.swi_hardware_test)) dicQnA.Add("swi_hardware_test", ansBean.swi_hardware_test);
                if (!dicQnA.ContainsKey("swi_software_text") && !string.IsNullOrEmpty(ansBean.swi_software_text)) dicQnA.Add("swi_software_text", ansBean.swi_software_text);
                if (!dicQnA.ContainsKey("swi_systemdata_copy") && !string.IsNullOrEmpty(ansBean.swi_systemdata_copy)) dicQnA.Add("swi_systemdata_copy", ansBean.swi_systemdata_copy);

                if (!dicQnA.ContainsKey("att_auto") && !string.IsNullOrEmpty(ansBean.att_auto)) dicQnA.Add("att_auto", ansBean.att_auto);
                if (!dicQnA.ContainsKey("att_voice_mailbox") && !string.IsNullOrEmpty(ansBean.att_voice_mailbox)) dicQnA.Add("att_voice_mailbox", ansBean.att_voice_mailbox);
                if (!dicQnA.ContainsKey("att_accounting") && !string.IsNullOrEmpty(ansBean.att_accounting)) dicQnA.Add("att_accounting", ansBean.att_accounting);
                if (!dicQnA.ContainsKey("att_keepmusic") && !string.IsNullOrEmpty(ansBean.att_keepmusic)) dicQnA.Add("att_keepmusic", ansBean.att_keepmusic);
                if (!dicQnA.ContainsKey("att_pc_controller") && !string.IsNullOrEmpty(ansBean.att_pc_controller)) dicQnA.Add("att_pc_controller", ansBean.att_pc_controller);

                if (!dicQnA.ContainsKey("tru_line_condition") && !string.IsNullOrEmpty(ansBean.tru_line_condition)) dicQnA.Add("tru_line_condition", ansBean.tru_line_condition);
                if (!dicQnA.ContainsKey("tru_co_interface") && !string.IsNullOrEmpty(ansBean.tru_co_interface)) dicQnA.Add("tru_co_interface", ansBean.tru_co_interface);
                if (!dicQnA.ContainsKey("tru_did_interface") && !string.IsNullOrEmpty(ansBean.tru_did_interface)) dicQnA.Add("tru_did_interface", ansBean.tru_did_interface);
                if (!dicQnA.ContainsKey("tru_ti_interface") && !string.IsNullOrEmpty(ansBean.tru_ti_interface)) dicQnA.Add("tru_ti_interface", ansBean.tru_ti_interface);
                if (!dicQnA.ContainsKey("tru_other_interface") && !string.IsNullOrEmpty(ansBean.tru_other_interface)) dicQnA.Add("tru_other_interface", ansBean.tru_other_interface);

                if (!dicQnA.ContainsKey("opinions_from_engineer") && !string.IsNullOrEmpty(ansBean.opinions_from_engineer)) dicQnA.Add("opinions_from_engineer", ansBean.opinions_from_engineer);
                if (!dicQnA.ContainsKey("opi_maintain") && !string.IsNullOrEmpty(ansBean.opi_maintain)) dicQnA.Add("opi_maintain", ansBean.opi_maintain);
                #endregion
            }

            foreach (var qa in dicQnA)
            {
                var qusDesc = qusBean.FirstOrDefault(x => x.ITEM_ALIAS == qa.Key);
                liQnA.Add(qusDesc.ITEM_NAME);
                liQnA.Add(qa.Value);
            }

            return liQnA;
        }

        /// <summary>
        /// 取得定維問券題目及答案(測試用)
        /// </summary>
        /// <param name="svid">SurveyCake問券ID</param>
        /// <param name="hash">問券答案ID</param>
        /// <returns></returns>
        public ActionResult GetSurveyMaintainValueQnA(string svid, string hash)
        {
            var ansBean = appDB.TB_SURVEY_ANS_MAINTAIN.FirstOrDefault(x => x.svid == svid && x.hash == hash);
            var qusBean = appDB.TB_SURVEY_QUS_MAINTAIN.OrderBy(x => x.SORT);
            List<string> liQnA = new List<string>();
            Dictionary<string, string> dicQnA = new Dictionary<string, string>();
            if (ansBean != null)
            {
                if (!dicQnA.ContainsKey("product_name")) dicQnA.Add("product_name", ansBean.product_name);
                if (!dicQnA.ContainsKey("product_serial_no")) dicQnA.Add("product_serial_no", ansBean.product_serial_no);
                if (!dicQnA.ContainsKey("ho_panel_led") && !string.IsNullOrEmpty(ansBean.ho_panel_led)) dicQnA.Add("ho_panel_led", ansBean.ho_panel_led);
                if (!dicQnA.ContainsKey("ho_cooling_fan") && !string.IsNullOrEmpty(ansBean.ho_cooling_fan)) dicQnA.Add("ho_cooling_fan", ansBean.ho_cooling_fan);
                if (!dicQnA.ContainsKey("ho_power_module") && !string.IsNullOrEmpty(ansBean.ho_power_module)) dicQnA.Add("ho_power_module", ansBean.ho_power_module);
                if (!dicQnA.ContainsKey("ho_raid_panel_led") && !string.IsNullOrEmpty(ansBean.ho_raid_panel_led)) dicQnA.Add("ho_raid_panel_led", ansBean.ho_raid_panel_led);
                if (!dicQnA.ContainsKey("ho_raid_cooling_fan") && !string.IsNullOrEmpty(ansBean.ho_raid_cooling_fan)) dicQnA.Add("ho_raid_cooling_fan", ansBean.ho_raid_cooling_fan);
                if (!dicQnA.ContainsKey("ho_raid_power_module") && !string.IsNullOrEmpty(ansBean.ho_raid_power_module)) dicQnA.Add("ho_raid_power_module", ansBean.ho_raid_power_module);
                if (!dicQnA.ContainsKey("ho_raid_event_log") && !string.IsNullOrEmpty(ansBean.ho_raid_event_log)) dicQnA.Add("ho_raid_event_log", ansBean.ho_raid_event_log);
                if (!dicQnA.ContainsKey("ho_manage_interface_event_log") && !string.IsNullOrEmpty(ansBean.ho_manage_interface_event_log)) dicQnA.Add("ho_manage_interface_event_log", ansBean.ho_manage_interface_event_log);
                if (!dicQnA.ContainsKey("ho_os_event_log") && !string.IsNullOrEmpty(ansBean.ho_os_event_log)) dicQnA.Add("ho_os_event_log", ansBean.ho_os_event_log);
                if (!dicQnA.ContainsKey("ho_equip_network_light") && !string.IsNullOrEmpty(ansBean.ho_equip_network_light)) dicQnA.Add("ho_equip_network_light", ansBean.ho_equip_network_light);
                if (!dicQnA.ContainsKey("ho_others") && !string.IsNullOrEmpty(ansBean.ho_others)) dicQnA.Add("ho_others", ansBean.ho_others);

                if (!dicQnA.ContainsKey("net_backup_config") && !string.IsNullOrEmpty(ansBean.net_backup_config)) dicQnA.Add("net_backup_config", ansBean.net_backup_config);
                if (!dicQnA.ContainsKey("net_log") && !string.IsNullOrEmpty(ansBean.net_log)) dicQnA.Add("net_log", ansBean.net_log);
                if (!dicQnA.ContainsKey("net_cpu") && !string.IsNullOrEmpty(ansBean.net_cpu)) dicQnA.Add("net_cpu", ansBean.net_cpu);
                if (!dicQnA.ContainsKey("net_memory") && !string.IsNullOrEmpty(ansBean.net_memory)) dicQnA.Add("net_memory", ansBean.net_memory);
                if (!dicQnA.ContainsKey("net_port") && !string.IsNullOrEmpty(ansBean.net_port)) dicQnA.Add("net_port", ansBean.net_port);
                if (!dicQnA.ContainsKey("net_others") && !string.IsNullOrEmpty(ansBean.net_others)) dicQnA.Add("net_others", ansBean.net_others);

                if (!dicQnA.ContainsKey("sto_panel_light") && !string.IsNullOrEmpty(ansBean.sto_panel_light)) dicQnA.Add("sto_panel_light", ansBean.sto_panel_light);
                if (!dicQnA.ContainsKey("sto_cooling_fan") && !string.IsNullOrEmpty(ansBean.sto_cooling_fan)) dicQnA.Add("sto_cooling_fan", ansBean.sto_cooling_fan);
                if (!dicQnA.ContainsKey("sto_power_module") && !string.IsNullOrEmpty(ansBean.sto_power_module)) dicQnA.Add("sto_power_module", ansBean.sto_power_module);
                if (!dicQnA.ContainsKey("sto_network_light") && !string.IsNullOrEmpty(ansBean.sto_network_light)) dicQnA.Add("sto_network_light", ansBean.sto_network_light);
                if (!dicQnA.ContainsKey("sto_fc_iscsi_light") && !string.IsNullOrEmpty(ansBean.sto_fc_iscsi_light)) dicQnA.Add("sto_fc_iscsi_light", ansBean.sto_fc_iscsi_light);
                if (!dicQnA.ContainsKey("sto_is_severe_error") && !string.IsNullOrEmpty(ansBean.sto_is_severe_error)) dicQnA.Add("sto_is_severe_error", ansBean.sto_is_severe_error);
                if (!dicQnA.ContainsKey("sto_capacity_over_20") && !string.IsNullOrEmpty(ansBean.sto_capacity_over_20)) dicQnA.Add("sto_capacity_over_20", ansBean.sto_capacity_over_20);
                if (!dicQnA.ContainsKey("sto_iops_over_60") && !string.IsNullOrEmpty(ansBean.sto_iops_over_60)) dicQnA.Add("sto_iops_over_60", ansBean.sto_iops_over_60);
                if (!dicQnA.ContainsKey("sto_others") && !string.IsNullOrEmpty(ansBean.sto_others)) dicQnA.Add("sto_others", ansBean.sto_others);

                if (!dicQnA.ContainsKey("vm_esxi_host_cpu") && !string.IsNullOrEmpty(ansBean.vm_esxi_host_cpu)) dicQnA.Add("vm_esxi_host_cpu", ansBean.vm_esxi_host_cpu);
                if (!dicQnA.ContainsKey("vm_esxi_host_memory") && !string.IsNullOrEmpty(ansBean.vm_esxi_host_memory)) dicQnA.Add("vm_esxi_host_memory", ansBean.vm_esxi_host_memory);
                if (!dicQnA.ContainsKey("vm_esxi_datastore") && !string.IsNullOrEmpty(ansBean.vm_esxi_datastore)) dicQnA.Add("vm_esxi_datastore", ansBean.vm_esxi_datastore);
                if (!dicQnA.ContainsKey("vm_guest_os_cpu") && !string.IsNullOrEmpty(ansBean.vm_guest_os_cpu)) dicQnA.Add("vm_guest_os_cpu", ansBean.vm_guest_os_cpu);
                if (!dicQnA.ContainsKey("vm_guest_os_memory") && !string.IsNullOrEmpty(ansBean.vm_guest_os_memory)) dicQnA.Add("vm_guest_os_memory", ansBean.vm_guest_os_memory);
                if (!dicQnA.ContainsKey("vm_high_availability_state") && !string.IsNullOrEmpty(ansBean.vm_high_availability_state)) dicQnA.Add("vm_high_availability_state", ansBean.vm_high_availability_state);
                if (!dicQnA.ContainsKey("vm_esxi_event_log") && !string.IsNullOrEmpty(ansBean.vm_esxi_event_log)) dicQnA.Add("vm_esxi_event_log", ansBean.vm_esxi_event_log);
                if (!dicQnA.ContainsKey("vm_horizon_view") && !string.IsNullOrEmpty(ansBean.vm_horizon_view)) dicQnA.Add("vm_horizon_view", ansBean.vm_horizon_view);
                if (!dicQnA.ContainsKey("vm_nsx") && !string.IsNullOrEmpty(ansBean.vm_nsx)) dicQnA.Add("vm_nsx", ansBean.vm_nsx);
                if (!dicQnA.ContainsKey("vm_vrops") && !string.IsNullOrEmpty(ansBean.vm_vrops)) dicQnA.Add("vm_vrops", ansBean.vm_vrops);
                if (!dicQnA.ContainsKey("vm_others") && !string.IsNullOrEmpty(ansBean.vm_others)) dicQnA.Add("vm_others", ansBean.vm_others);

                if (!dicQnA.ContainsKey("hy_event_log") && !string.IsNullOrEmpty(ansBean.hy_event_log)) dicQnA.Add("hy_event_log", ansBean.hy_event_log);
                if (!dicQnA.ContainsKey("hy_cluster_health") && !string.IsNullOrEmpty(ansBean.hy_cluster_health)) dicQnA.Add("hy_cluster_health", ansBean.hy_cluster_health);
                if (!dicQnA.ContainsKey("hy_datastore_usage") && !string.IsNullOrEmpty(ansBean.hy_datastore_usage)) dicQnA.Add("hy_datastore_usage", ansBean.hy_datastore_usage);
                if (!dicQnA.ContainsKey("hy_usage") && !string.IsNullOrEmpty(ansBean.hy_usage)) dicQnA.Add("hy_usage", ansBean.hy_usage);
                if (!dicQnA.ContainsKey("hy_availability_state") && !string.IsNullOrEmpty(ansBean.hy_availability_state)) dicQnA.Add("hy_availability_state", ansBean.hy_availability_state);
                if (!dicQnA.ContainsKey("hy_others") && !string.IsNullOrEmpty(ansBean.hy_others)) dicQnA.Add("hy_others", ansBean.hy_others);

                if (!dicQnA.ContainsKey("ad_event_log") && !string.IsNullOrEmpty(ansBean.ad_event_log)) dicQnA.Add("ad_event_log", ansBean.ad_event_log);
                if (!dicQnA.ContainsKey("ad_dcdiag_check") && !string.IsNullOrEmpty(ansBean.ad_dcdiag_check)) dicQnA.Add("ad_dcdiag_check", ansBean.ad_dcdiag_check);
                if (!dicQnA.ContainsKey("ad_replication_health") && !string.IsNullOrEmpty(ansBean.ad_replication_health)) dicQnA.Add("ad_replication_health", ansBean.ad_replication_health);
                if (!dicQnA.ContainsKey("ad_kcc_health") && !string.IsNullOrEmpty(ansBean.ad_kcc_health)) dicQnA.Add("ad_kcc_health", ansBean.ad_kcc_health);
                if (!dicQnA.ContainsKey("ad_others") && !string.IsNullOrEmpty(ansBean.ad_others)) dicQnA.Add("ad_others", ansBean.ad_others);

                if (!dicQnA.ContainsKey("ws_event_log") && !string.IsNullOrEmpty(ansBean.ws_event_log)) dicQnA.Add("ws_event_log", ansBean.ws_event_log);
                if (!dicQnA.ContainsKey("ws_services_health") && !string.IsNullOrEmpty(ansBean.ws_services_health)) dicQnA.Add("ws_services_health", ansBean.ws_services_health);
                if (!dicQnA.ContainsKey("ws_update_patch_approved") && !string.IsNullOrEmpty(ansBean.ws_update_patch_approved)) dicQnA.Add("ws_update_patch_approved", ansBean.ws_update_patch_approved);
                if (!dicQnA.ContainsKey("ws_others") && !string.IsNullOrEmpty(ansBean.ws_others)) dicQnA.Add("ws_others", ansBean.ws_others);

                if (!dicQnA.ContainsKey("net_eq_ip") && !string.IsNullOrEmpty(ansBean.net_eq_ip)) dicQnA.Add("net_eq_ip", ansBean.net_eq_ip);
                if (!dicQnA.ContainsKey("net_eq_light") && !string.IsNullOrEmpty(ansBean.net_eq_light)) dicQnA.Add("net_eq_light", ansBean.net_eq_light);
                if (!dicQnA.ContainsKey("net_eq_contact_loose") && !string.IsNullOrEmpty(ansBean.net_eq_contact_loose)) dicQnA.Add("net_eq_contact_loose", ansBean.net_eq_contact_loose);
                if (!dicQnA.ContainsKey("net_eq_port") && !string.IsNullOrEmpty(ansBean.net_eq_port)) dicQnA.Add("net_eq_port", ansBean.net_eq_port);
                if (!dicQnA.ContainsKey("net_eq_log_information") && !string.IsNullOrEmpty(ansBean.net_eq_log_information)) dicQnA.Add("net_eq_log_information", ansBean.net_eq_log_information);
                if (!dicQnA.ContainsKey("net_eq_others") && !string.IsNullOrEmpty(ansBean.net_eq_others)) dicQnA.Add("net_eq_others", ansBean.net_eq_others);

                if (!dicQnA.ContainsKey("others") && !string.IsNullOrEmpty(ansBean.others)) dicQnA.Add("others", ansBean.others);

                #region --20220711-通訊類的問題                
                if (!dicQnA.ContainsKey("env_temperature") && !string.IsNullOrEmpty(ansBean.env_temperature)) dicQnA.Add("env_temperature", ansBean.env_temperature);
                if (!dicQnA.ContainsKey("env_humidity") && !string.IsNullOrEmpty(ansBean.env_humidity)) dicQnA.Add("env_humidity", ansBean.env_humidity);
                if (!dicQnA.ContainsKey("env_ventilation") && !string.IsNullOrEmpty(ansBean.env_ventilation)) dicQnA.Add("env_ventilation", ansBean.env_ventilation);
                if (!dicQnA.ContainsKey("env_cleanness") && !string.IsNullOrEmpty(ansBean.env_cleanness)) dicQnA.Add("env_cleanness", ansBean.env_cleanness);

                if (!dicQnA.ContainsKey("power_backup_system") && !string.IsNullOrEmpty(ansBean.power_backup_system)) dicQnA.Add("power_backup_system", ansBean.power_backup_system);
                if (!dicQnA.ContainsKey("powg_type") && !string.IsNullOrEmpty(ansBean.powg_type)) dicQnA.Add("powg_type", ansBean.powg_type);
                if (!dicQnA.ContainsKey("powg_appearance") && !string.IsNullOrEmpty(ansBean.powg_appearance)) dicQnA.Add("powg_appearance", ansBean.powg_appearance);
                if (!dicQnA.ContainsKey("powg_unit_voltage") && !string.IsNullOrEmpty(ansBean.powg_unit_voltage)) dicQnA.Add("powg_unit_voltage", ansBean.powg_unit_voltage);
                if (!dicQnA.ContainsKey("powg_sum_voltage") && !string.IsNullOrEmpty(ansBean.powg_sum_voltage)) dicQnA.Add("powg_sum_voltage", ansBean.powg_sum_voltage);

                if (!dicQnA.ContainsKey("pows_type") && !string.IsNullOrEmpty(ansBean.pows_type)) dicQnA.Add("pows_type", ansBean.pows_type);
                if (!dicQnA.ContainsKey("pows_ac_ups_voltage") && !string.IsNullOrEmpty(ansBean.pows_ac_ups_voltage)) dicQnA.Add("pows_ac_ups_voltage", ansBean.pows_ac_ups_voltage);
                if (!dicQnA.ContainsKey("pows_dc_charger_voltage") && !string.IsNullOrEmpty(ansBean.pows_dc_charger_voltage)) dicQnA.Add("pows_dc_charger_voltage", ansBean.pows_dc_charger_voltage);
                if (!dicQnA.ContainsKey("pows_charger_value") && !string.IsNullOrEmpty(ansBean.pows_charger_value)) dicQnA.Add("pows_charger_value", ansBean.pows_charger_value);

                if (!dicQnA.ContainsKey("swi_filter_ventilation") && !string.IsNullOrEmpty(ansBean.swi_filter_ventilation)) dicQnA.Add("swi_filter_ventilation", ansBean.swi_filter_ventilation);
                if (!dicQnA.ContainsKey("swi_suppllier_led") && !string.IsNullOrEmpty(ansBean.swi_suppllier_led)) dicQnA.Add("swi_suppllier_led", ansBean.swi_suppllier_led);
                if (!dicQnA.ContainsKey("swi_interface_led") && !string.IsNullOrEmpty(ansBean.swi_interface_led)) dicQnA.Add("swi_interface_led", ansBean.swi_interface_led);
                if (!dicQnA.ContainsKey("swi_hardware_test") && !string.IsNullOrEmpty(ansBean.swi_hardware_test)) dicQnA.Add("swi_hardware_test", ansBean.swi_hardware_test);
                if (!dicQnA.ContainsKey("swi_software_text") && !string.IsNullOrEmpty(ansBean.swi_software_text)) dicQnA.Add("swi_software_text", ansBean.swi_software_text);
                if (!dicQnA.ContainsKey("swi_systemdata_copy") && !string.IsNullOrEmpty(ansBean.swi_systemdata_copy)) dicQnA.Add("swi_systemdata_copy", ansBean.swi_systemdata_copy);

                if (!dicQnA.ContainsKey("att_auto") && !string.IsNullOrEmpty(ansBean.att_auto)) dicQnA.Add("att_auto", ansBean.att_auto);
                if (!dicQnA.ContainsKey("att_voice_mailbox") && !string.IsNullOrEmpty(ansBean.att_voice_mailbox)) dicQnA.Add("att_voice_mailbox", ansBean.att_voice_mailbox);
                if (!dicQnA.ContainsKey("att_accounting") && !string.IsNullOrEmpty(ansBean.att_accounting)) dicQnA.Add("att_accounting", ansBean.att_accounting);
                if (!dicQnA.ContainsKey("att_keepmusic") && !string.IsNullOrEmpty(ansBean.att_keepmusic)) dicQnA.Add("att_keepmusic", ansBean.att_keepmusic);
                if (!dicQnA.ContainsKey("att_pc_controller") && !string.IsNullOrEmpty(ansBean.att_pc_controller)) dicQnA.Add("att_pc_controller", ansBean.att_pc_controller);

                if (!dicQnA.ContainsKey("tru_line_condition") && !string.IsNullOrEmpty(ansBean.tru_line_condition)) dicQnA.Add("tru_line_condition", ansBean.tru_line_condition);
                if (!dicQnA.ContainsKey("tru_co_interface") && !string.IsNullOrEmpty(ansBean.tru_co_interface)) dicQnA.Add("tru_co_interface", ansBean.tru_co_interface);
                if (!dicQnA.ContainsKey("tru_did_interface") && !string.IsNullOrEmpty(ansBean.tru_did_interface)) dicQnA.Add("tru_did_interface", ansBean.tru_did_interface);
                if (!dicQnA.ContainsKey("tru_ti_interface") && !string.IsNullOrEmpty(ansBean.tru_ti_interface)) dicQnA.Add("tru_ti_interface", ansBean.tru_ti_interface);
                if (!dicQnA.ContainsKey("tru_other_interface") && !string.IsNullOrEmpty(ansBean.tru_other_interface)) dicQnA.Add("tru_other_interface", ansBean.tru_other_interface);

                if (!dicQnA.ContainsKey("opinions_from_engineer") && !string.IsNullOrEmpty(ansBean.opinions_from_engineer)) dicQnA.Add("opinions_from_engineer", ansBean.opinions_from_engineer);
                if (!dicQnA.ContainsKey("opi_maintain") && !string.IsNullOrEmpty(ansBean.opi_maintain)) dicQnA.Add("opi_maintain", ansBean.opi_maintain);
                #endregion
            }

            foreach (var qa in dicQnA)
            {
                var qusDesc = qusBean.FirstOrDefault(x => x.ITEM_ALIAS == qa.Key);
                liQnA.Add(qusDesc.ITEM_NAME);
                liQnA.Add(qa.Value);
            }

            return Json(liQnA, JsonRequestBehavior.AllowGet);
        }
        #endregion        

        #region 客戶手寫簽名圖片上傳並產生服務報告書pdf INPUT資訊
        /// <summary>客戶手寫簽名圖片上傳並產生服務報告書pdf INPUT資訊</summary>
        public struct SRSIGNPDFINFO_INPUT
        {
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>服務工程師ERPID/技術主管ERPID</summary>
            public string IV_EMPNO { get; set; }
            /// <summary>出發時間</summary>
            public string IV_StartTime { get; set; }
            /// <summary>到場時間</summary>
            public string IV_ArriveTime { get; set; }
            /// <summary>完成時間</summary>
            public string IV_FinishTime { get; set; }
            /// <summary>處理紀錄</summary>
            public string IV_Desc { get; set; }
            /// <summary>客戶意見/備註</summary>
            public string IV_CusOpinion { get; set; }
            /// <summary>產生服務報告書圖檔方式(SIGN.有簽名檔、NOSIGN.無簽名檔、ONLINE.線上、REMOTE.遠端、ATTACH.純附件(不產生服務報告書))</summary>
            public SRReportType IV_SRReportType { get; set; }
            /// <summary>當產生服務報告書圖檔的方式為【SIGN.有名檔】時，才需要傳檔案</summary>
            public HttpPostedFileBase IV_SRReportFile { get; set; }
            /// <summary>當產生服務報告書圖檔的方式為【NOSIGN.無名檔】時，才需要傳檔案</summary>
            public HttpPostedFileBase[] IV_SRReportFiles { get; set; }

        }
        #endregion

        #region 客戶手寫簽名圖片上傳並產生服務報告書pdf OUTPUT資訊
        /// <summary>客戶手寫簽名圖片上傳並產生服務報告書pdf OUTPUT資訊</summary>
        public struct SRSIGNPDFINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
            /// <summary>服務報告書PDF檔名</summary>
            public string EV_FILENAME { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑客戶手寫簽名圖片上傳並產生服務報告書pdf接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓異動產品序號資訊相關接口 ↓↓↓↓↓-----        

        #region 新增產品序號資訊相關接口
        [HttpPost]
        public ActionResult API_SRPRODUCTSERIALNFO_CHAGE(SRPRODUCTSERIALNFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{                
            //    "IV_LOGINEMPNO": "99120894",
            //    "IV_CID": "1185",
            //    "IV_SRID" : "612212070001",
            //    "IV_SERIAL" : "SGH33223R0",
            //    "IV_MaterialID" : "G-M21161-001-3M",
            //    "IV_MaterialName" : "HP LCD BEZEL 13 HD"
            //}
            #endregion

            SRPRODUCTSERIALNFO_OUTPUT ListOUT = new SRPRODUCTSERIALNFO_OUTPUT();

            ListOUT = SaveSRPRODUCTSERIALNFO(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 刪除產品序號資訊相關接口
        [HttpPost]
        public ActionResult API_SRPRODUCTSERIALNFO_DELETE(SRPRODUCTSERIALNFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894", 
            //    "IV_SRID": "612212070001",
            //    "IV_CID": "1185"            
            //}
            #endregion

            SRPRODUCTSERIALNFO_OUTPUT ListOUT = new SRPRODUCTSERIALNFO_OUTPUT();

            beanIN.IV_ISDELETE = "Y";

            ListOUT = SaveSRPRODUCTSERIALNFO(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 儲存產品序號資訊相關
        private SRPRODUCTSERIALNFO_OUTPUT SaveSRPRODUCTSERIALNFO(SRPRODUCTSERIALNFO_INPUT beanIN)
        {
            SRPRODUCTSERIALNFO_OUTPUT OUTBean = new SRPRODUCTSERIALNFO_OUTPUT();

            int cID = 0;

            string IV_LOGINEMPNO = string.Empty;
            string IV_LOGINEMPName = string.Empty;
            string IV_SRID = string.Empty;
            string IV_SERIAL = string.Empty;
            string IV_MaterialID = string.Empty;
            string IV_MaterialName = string.Empty;
            string IV_ISDELETE = string.Empty;
            string cMaterialID = string.Empty;
            string cMaterialName = string.Empty;
            string cProductNumber = string.Empty;
            string cInstallID = string.Empty;

            try
            {
                cID = string.IsNullOrEmpty(beanIN.IV_CID) ? 0 : int.Parse(beanIN.IV_CID);
                IV_SRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID;
                IV_SERIAL = string.IsNullOrEmpty(beanIN.IV_SERIAL) ? "" : beanIN.IV_SERIAL;
                IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO;
                IV_MaterialID = string.IsNullOrEmpty(beanIN.IV_MaterialID) ? "" : beanIN.IV_MaterialID;
                IV_MaterialName = string.IsNullOrEmpty(beanIN.IV_MaterialName) ? "" : beanIN.IV_MaterialName;
                IV_ISDELETE = string.IsNullOrEmpty(beanIN.IV_ISDELETE) ? "N" : beanIN.IV_ISDELETE;               

                if (IV_MaterialName != "")
                {
                    cMaterialID = IV_MaterialID;
                    cMaterialName = IV_MaterialName;
                    cProductNumber = CMF.findMFRPNumber(IV_MaterialID);
                    cInstallID = CMF.findInstallNumber(IV_SERIAL);
                }
                else
                {
                    var ProBean = CMF.findMaterialBySerial(IV_MaterialID);

                    cMaterialID = string.IsNullOrEmpty(IV_MaterialID) ? ProBean.ProdID : IV_MaterialID;
                    cMaterialName = ProBean.Product;
                    cProductNumber = ProBean.MFRPN;
                    cInstallID = ProBean.InstallNo;
                }

                #region 取得登入人員姓名
                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

                IV_LOGINEMPName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                #endregion

                if (cID == 0)
                {
                    #region 新增
                    TB_ONE_SRDetail_Product SFB = new TB_ONE_SRDetail_Product();

                    SFB.cSRID = IV_SRID;
                    SFB.cSerialID = IV_SERIAL;
                    SFB.cMaterialID = cMaterialID;
                    SFB.cMaterialName = cMaterialName;
                    SFB.cProductNumber = cProductNumber;
                    SFB.cInstallID = cInstallID;
                    SFB.cNewSerialID = "";

                    SFB.Disabled = 0;
                    SFB.CreatedDate = DateTime.Now;
                    SFB.CreatedUserName = IV_LOGINEMPName;

                    dbOne.TB_ONE_SRDetail_Product.Add(SFB);
                    #endregion
                }                
                else //刪除
                {
                    if (IV_ISDELETE == "Y")
                    {
                        #region 刪除
                        var bean = dbOne.TB_ONE_SRDetail_Product.FirstOrDefault(x => x.cID == cID);

                        if (bean != null)
                        {
                            bean.Disabled = 1;

                            bean.ModifiedDate = DateTime.Now;
                            bean.ModifiedUserName = IV_LOGINEMPName;
                        }
                        #endregion                       
                    }
                    else
                    {
                        #region 更新
                        var SFB = dbOne.TB_ONE_SRDetail_Product.FirstOrDefault(x => x.cID == cID);

                        if (SFB != null)
                        {                            
                            SFB.cSerialID = IV_SERIAL;
                            SFB.cMaterialID = cMaterialID;
                            SFB.cMaterialName = cMaterialName;
                            SFB.cProductNumber = cProductNumber;
                            SFB.cInstallID = cInstallID;

                            SFB.Disabled = 0;
                            SFB.ModifiedDate = DateTime.Now;
                            SFB.ModifiedUserName = IV_LOGINEMPName;
                        }
                        #endregion
                    }
                }

                var result = dbOne.SaveChanges();

                if (result <= 0)
                {
                    if (cID == 0) //新增
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新增失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                    }
                    else
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "刪除失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                    }

                    CMF.writeToLog(IV_SRID, "SaveSRPRODUCTSERIALNFO_API", pMsg, IV_LOGINEMPName);

                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    if (cID == 0) //新增
                    {
                        var bean = dbOne.TB_ONE_SRDetail_Product.OrderByDescending(x => x.cID).FirstOrDefault(x => x.cSRID == IV_SRID);

                        if (bean != null)
                        {
                            OUTBean.EV_CID = bean.cID.ToString();
                        }
                    }
                    else
                    {
                        OUTBean.EV_CID = cID.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_SRID, "SaveSRPRODUCTSERIALNFO_API", pMsg, IV_LOGINEMPName);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
                OUTBean.EV_CID = "";
            }

            return OUTBean;
        }
        #endregion

        #region 異動產品序號資訊相關INPUT資訊
        /// <summary>異動產品序號資訊相關INPUT資訊</summary>
        public struct SRPRODUCTSERIALNFO_INPUT
        {
            /// <summary>系統ID</summary>
            public string IV_CID { get; set; }
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>序號ID</summary>
            public string IV_SERIAL { get; set; }
            /// <summary>登入者員工編號</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>更換零件料號ID</summary>
            public string IV_MaterialID { get; set; }
            /// <summary>料號說明</summary>
            public string IV_MaterialName { get; set; }
            /// <summary>是否要刪除(Y.是 N.否)</summary>
            public string IV_ISDELETE { get; set; }            
        }
        #endregion

        #region 異動產品序號資訊相關OUTPUT資訊
        /// <summary>異動產品序號資訊相關OUTPUT資訊</summary>
        public struct SRPRODUCTSERIALNFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
            /// <summary>系統ID</summary>
            public string EV_CID { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑異動產品序號資訊相關查詢接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓異動零件更換資訊相關接口 ↓↓↓↓↓-----        

        #region 新增零件更換資訊相關接口
        [HttpPost]
        public ActionResult API_SRPARTSREPALCEINFO_CREATE(SRPARTSREPALCEINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{                
            //    "IV_SRID" : "612212070001",
            //    "IV_EMPNO": "10010298",
            //    "IV_XCHP" : "OR04206822",
            //    "IV_MaterialID" : "G-M21161-001-3M",
            //    "IV_MaterialName" : "HP LCD BEZEL 13 HD",
            //    "IV_OldCT" : "",
            //    "IV_NewCT" : "",
            //    "IV_HPCT" : "",
            //    "IV_NewUEFI" : "",
            //    "IV_StandbySerialID" : "",
            //    "IV_HPCaseID" : "",
            //    "IV_ArriveDate" : "",
            //    "IV_ReturnDate" : "",
            //    "IV_PersonalDamage" : "N",
            //    "IV_Note" : "測試零件更換"
            //}
            #endregion

            SRPARTSREPALCEINFO_OUTPUT ListOUT = new SRPARTSREPALCEINFO_OUTPUT();

            ListOUT = SaveSRPARTSREPALCEINFO(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 刪除零件更換資訊相關接口
        [HttpPost]
        public ActionResult API_SRPARTSREPALCEINFO_DELETE(SRPARTSREPALCEINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_SRID": "612212070001",
            //    "IV_EMPNO": "10010298",
            //    "IV_CID": "1043"
            //}
            #endregion

            SRPARTSREPALCEINFO_OUTPUT ListOUT = new SRPARTSREPALCEINFO_OUTPUT();

            ListOUT = SaveSRPARTSREPALCEINFO(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 儲存零件更換資訊相關
        private SRPARTSREPALCEINFO_OUTPUT SaveSRPARTSREPALCEINFO(SRPARTSREPALCEINFO_INPUT beanIN)
        {
            SRPARTSREPALCEINFO_OUTPUT OUTBean = new SRPARTSREPALCEINFO_OUTPUT();

            int cID = 0;

            string cSRID = string.Empty;
            string cENGID = string.Empty;
            string cENGNAME = string.Empty;
            string cXCHP = string.Empty;
            string cMaterialID = string.Empty;
            string cMaterialName = string.Empty;
            string cOldCT = string.Empty;
            string cNewCT = string.Empty;
            string cHPCT = string.Empty;
            string cNewUEFI = string.Empty;
            string cStandbySerialID = string.Empty;
            string cHPCaseID = string.Empty;
            string cArriveDate = string.Empty;
            string cReturnDate = string.Empty;
            string cPersonalDamage = string.Empty;
            string cNote = string.Empty;

            try
            {
                cID = string.IsNullOrEmpty(beanIN.IV_CID) ? 0 : int.Parse(beanIN.IV_CID);
                cSRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID;
                cENGID = string.IsNullOrEmpty(beanIN.IV_EMPNO) ? "" : beanIN.IV_EMPNO;
                cXCHP = string.IsNullOrEmpty(beanIN.IV_XCHP) ? "" : beanIN.IV_XCHP;
                cMaterialID = string.IsNullOrEmpty(beanIN.IV_MaterialID) ? "" : beanIN.IV_MaterialID;
                cMaterialName = string.IsNullOrEmpty(beanIN.IV_MaterialName) ? "" : beanIN.IV_MaterialName;
                cOldCT = string.IsNullOrEmpty(beanIN.IV_OldCT) ? "" : beanIN.IV_OldCT;
                cNewCT = string.IsNullOrEmpty(beanIN.IV_NewCT) ? "" : beanIN.IV_NewCT;
                cHPCT = string.IsNullOrEmpty(beanIN.IV_HPCT) ? "" : beanIN.IV_HPCT;
                cNewUEFI = string.IsNullOrEmpty(beanIN.IV_NewUEFI) ? "" : beanIN.IV_NewUEFI;
                cStandbySerialID = string.IsNullOrEmpty(beanIN.IV_StandbySerialID) ? "" : beanIN.IV_StandbySerialID;
                cHPCaseID = string.IsNullOrEmpty(beanIN.IV_HPCaseID) ? "" : beanIN.IV_HPCaseID;
                cArriveDate = string.IsNullOrEmpty(beanIN.IV_ArriveDate) ? "" : beanIN.IV_ArriveDate;
                cReturnDate = string.IsNullOrEmpty(beanIN.IV_ReturnDate) ? "" : beanIN.IV_ReturnDate;
                cPersonalDamage = string.IsNullOrEmpty(beanIN.IV_PersonalDamage) ? "" : beanIN.IV_PersonalDamage;
                cNote = string.IsNullOrEmpty(beanIN.IV_Note) ? "" : beanIN.IV_Note;

                #region 取得工程師/技術主管姓名
                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(cENGID);

                cENGNAME = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                #endregion

                if (cID == 0)
                {
                    #region 新增
                    TB_ONE_SRDetail_PartsReplace SRParts = new TB_ONE_SRDetail_PartsReplace();

                    SRParts.cSRID = cSRID;
                    SRParts.cXCHP = cXCHP;
                    SRParts.cMaterialID = cMaterialID;
                    SRParts.cMaterialName = cMaterialName;
                    SRParts.cOldCT = cOldCT;
                    SRParts.cNewCT = cNewCT;
                    SRParts.cHPCT = cHPCT;
                    SRParts.cNewUEFI = cNewUEFI;
                    SRParts.cStandbySerialID = cStandbySerialID;
                    SRParts.cHPCaseID = cHPCaseID;
                    SRParts.cPersonalDamage = cPersonalDamage;
                    SRParts.cNote = cNote;
                    SRParts.Disabled = 0;

                    if (!string.IsNullOrEmpty(cArriveDate))
                    {
                        SRParts.cArriveDate = Convert.ToDateTime(cArriveDate);
                    }

                    if (!string.IsNullOrEmpty(cReturnDate))
                    {
                        SRParts.cReturnDate = Convert.ToDateTime(cReturnDate);
                    }

                    SRParts.CreatedDate = DateTime.Now;
                    SRParts.CreatedUserName = cENGNAME;

                    dbOne.TB_ONE_SRDetail_PartsReplace.Add(SRParts);
                    #endregion
                }
                else //刪除
                {
                    #region 刪除
                    var bean = dbOne.TB_ONE_SRDetail_PartsReplace.FirstOrDefault(x => x.cID == cID);

                    if (bean != null)
                    {
                        bean.Disabled = 1;

                        bean.ModifiedDate = DateTime.Now;
                        bean.ModifiedUserName = cENGNAME;
                    }
                    #endregion
                }

                var result = dbOne.SaveChanges();

                if (result <= 0)
                {
                    if (cID == 0) //新增
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新增失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                    }
                    else
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "刪除失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                    }

                    CMF.writeToLog(cSRID, "SaveSRPARTSREPALCEINFO_API", pMsg, cENGNAME);

                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    if (cID == 0) //新增
                    {
                        var bean = dbOne.TB_ONE_SRDetail_PartsReplace.OrderByDescending(x => x.cID).FirstOrDefault(x => x.cSRID == cSRID);

                        if (bean != null)
                        {
                            OUTBean.EV_CID = bean.cID.ToString();
                        }
                    }
                    else
                    {
                        OUTBean.EV_CID = cID.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(beanIN.IV_SRID, "SaveSRPARTSREPALCEINFO_API", pMsg, cENGNAME);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
                OUTBean.EV_CID = "";
            }

            return OUTBean;
        }
        #endregion

        #region 異動零件更換資訊相關INPUT資訊
        /// <summary>異動零件更換資訊相關INPUT資訊</summary>
        public struct SRPARTSREPALCEINFO_INPUT
        {
            /// <summary>系統ID</summary>
            public string IV_CID { get; set; }
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>服務工程師ERPID</summary>
            public string IV_EMPNO { get; set; }
            /// <summary>XC HP申請零件</summary>
            public string IV_XCHP { get; set; }
            /// <summary>更換零件料號ID</summary>
            public string IV_MaterialID { get; set; }
            /// <summary>料號說明</summary>
            public string IV_MaterialName { get; set; }
            /// <summary>Old CT</summary>
            public string IV_OldCT { get; set; }
            /// <summary>New CT</summary>
            public string IV_NewCT { get; set; }
            /// <summary>HP CT</summary>
            public string IV_HPCT { get; set; }
            /// <summary>New UEFI </summary>
            public string IV_NewUEFI { get; set; }
            /// <summary>備機序號</summary>
            public string IV_StandbySerialID { get; set; }
            /// <summary>HP Case ID</summary>
            public string IV_HPCaseID { get; set; }
            /// <summary>到貨日 </summary>
            public string IV_ArriveDate { get; set; }
            /// <summary>歸還日 </summary>
            public string IV_ReturnDate { get; set; }
            /// <summary>是否有人損</summary>
            public string IV_PersonalDamage { get; set; }
            /// <summary>備註</summary>
            public string IV_Note { get; set; }
        }
        #endregion

        #region 異動零件更換資訊相關OUTPUT資訊
        /// <summary>異動零件更換資訊相關OUTPUT資訊</summary>
        public struct SRPARTSREPALCEINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
            /// <summary>系統ID</summary>
            public string EV_CID { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑異動零件更換資訊相關查詢接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓異動派工工時紀錄相關接口 ↓↓↓↓↓-----        

        #region 異動派工工時紀錄相關接口
        [HttpPost]
        public ActionResult API_SRFIXRECORD_CHANGE(SRFIXRECORD_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_SRID": "612212070001",
            //    "IV_EMPNO": "10010298",
            //    "IV_ReceiveTime": "2023-01-18 18:20",
            //    "IV_StartTime": "2023-01-18 18:25",
            //    "IV_ArriveTime": "2023-01-18 18:50",
            //    "IV_FinishTime": "2023-01-18 19:50",            
            //    "IV_ISRENEW": "N",
            //    "IV_LocationS": "",
            //    "IV_LocationA": ""
            //}
            #endregion

            SRFIXRECORD_OUTPUT ListOUT = new SRFIXRECORD_OUTPUT();

            ListOUT = SaveSRFIXRECORD(beanIN);

            return Json(ListOUT);
        }
        #endregion       

        #region 儲存派工工時紀錄相關
        private SRFIXRECORD_OUTPUT SaveSRFIXRECORD(SRFIXRECORD_INPUT beanIN)
        {
            SRFIXRECORD_OUTPUT OUTBean = new SRFIXRECORD_OUTPUT();

            string cSRID = string.Empty;
            string cENGID = string.Empty;
            string cENGNAME = string.Empty;
            string cReceiveTime = string.Empty;
            string cStartTime = string.Empty;
            string cArriveTime = string.Empty;
            string cFinishTime = string.Empty;
            string cDeleteTime = string.Empty;
            string cISRENEW = string.Empty;
            string cLocationS = string.Empty;
            string cLocationA = string.Empty;

            try
            {
                cSRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID.Trim();
                cENGID = string.IsNullOrEmpty(beanIN.IV_EMPNO) ? "" : beanIN.IV_EMPNO.Trim();
                cReceiveTime = string.IsNullOrEmpty(beanIN.IV_ReceiveTime) ? "" : beanIN.IV_ReceiveTime.Trim();
                cStartTime = string.IsNullOrEmpty(beanIN.IV_StartTime) ? "" : beanIN.IV_StartTime.Trim();
                cArriveTime = string.IsNullOrEmpty(beanIN.IV_ArriveTime) ? "" : beanIN.IV_ArriveTime.Trim();
                cFinishTime = string.IsNullOrEmpty(beanIN.IV_FinishTime) ? "" : beanIN.IV_FinishTime.Trim();
                cDeleteTime = string.IsNullOrEmpty(beanIN.IV_DeleteTime) ? "" : beanIN.IV_DeleteTime.Trim();
                cISRENEW = string.IsNullOrEmpty(beanIN.IV_ISRENEW) ? "" : beanIN.IV_ISRENEW.Trim();
                cLocationS = string.IsNullOrEmpty(beanIN.IV_LocationS) ? "" : beanIN.IV_LocationS.Trim();
                cLocationA = string.IsNullOrEmpty(beanIN.IV_LocationA) ? "" : beanIN.IV_LocationA.Trim();

                #region 取得工程師/技術主管姓名
                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(cENGID);

                cENGNAME = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                #endregion

                var bean = dbOne.TB_ONE_SRFixRecord.FirstOrDefault(x => x.cSRID == cSRID && x.cEngineerID == cENGID);

                if (bean != null)
                {
                    if (cISRENEW == "Y")
                    {
                        #region 先刪除
                        dbOne.TB_ONE_SRFixRecord.Remove(bean);
                        #endregion

                        #region 再新增
                        TB_ONE_SRFixRecord SRRecord = SRFixRecord_Create(beanIN, cENGNAME);
                        dbOne.TB_ONE_SRFixRecord.Add(SRRecord);
                        #endregion
                    }
                    else
                    {
                        #region 修改
                        if (cReceiveTime != "")
                        {
                            bean.cReceiveTime = Convert.ToDateTime(cReceiveTime);
                        }

                        if (cStartTime != "")
                        {
                            bean.cStartTime = Convert.ToDateTime(cStartTime);
                        }

                        if (cArriveTime != "")
                        {
                            bean.cArriveTime = Convert.ToDateTime(cArriveTime);
                        }

                        if (cFinishTime != "")
                        {
                            bean.cFinishTime = Convert.ToDateTime(cFinishTime);
                        }

                        if (cDeleteTime != "")
                        {
                            bean.cDeleteTime = Convert.ToDateTime(cDeleteTime);
                        }

                        if (cLocationS != "")
                        {
                            bean.cLocationS = cLocationS;
                        }

                        if (cLocationA != "")
                        {
                            bean.cLocationA = cLocationA;
                        }

                        bean.ModifiedDate = DateTime.Now;
                        bean.ModifiedUserName = cENGNAME;
                        #endregion
                    }
                }
                else
                {
                    #region 新增
                    TB_ONE_SRFixRecord SRRecord = SRFixRecord_Create(beanIN, cENGNAME);
                    dbOne.TB_ONE_SRFixRecord.Add(SRRecord);
                    #endregion
                }

                var result = dbOne.SaveChanges();

                if (result <= 0)
                {
                    pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "異動失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;

                    CMF.writeToLog(cSRID, "SaveSRFIXRECORD_API", pMsg, cENGNAME);

                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(beanIN.IV_SRID, "SaveSRFIXRECORD_API", pMsg, cENGNAME);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 新增派工工時紀錄檔
        /// <summary>
        /// 新增派工工時紀錄檔
        /// </summary>
        /// <param name="beanIN">異動派工工時紀錄相關INPUT資訊</param>
        /// <param name="cENGNAME">新增人員姓名</param>
        /// <returns></returns>
        public TB_ONE_SRFixRecord SRFixRecord_Create(SRFIXRECORD_INPUT beanIN, string cENGNAME)
        {
            string cSRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID.Trim();
            string cENGID = string.IsNullOrEmpty(beanIN.IV_EMPNO) ? "" : beanIN.IV_EMPNO.Trim();
            string cReceiveTime = string.IsNullOrEmpty(beanIN.IV_ReceiveTime) ? "" : beanIN.IV_ReceiveTime.Trim();
            string cStartTime = string.IsNullOrEmpty(beanIN.IV_StartTime) ? "" : beanIN.IV_StartTime.Trim();
            string cArriveTime = string.IsNullOrEmpty(beanIN.IV_ArriveTime) ? "" : beanIN.IV_ArriveTime.Trim();
            string cFinishTime = string.IsNullOrEmpty(beanIN.IV_FinishTime) ? "" : beanIN.IV_FinishTime.Trim();
            string cDeleteTime = string.IsNullOrEmpty(beanIN.IV_DeleteTime) ? "" : beanIN.IV_DeleteTime.Trim();
            string cLocationS = string.IsNullOrEmpty(beanIN.IV_LocationS) ? "" : beanIN.IV_LocationS.Trim();
            string cLocationA = string.IsNullOrEmpty(beanIN.IV_LocationA) ? "" : beanIN.IV_LocationA.Trim();

            #region 新增
            TB_ONE_SRFixRecord SRRecord = new TB_ONE_SRFixRecord();

            SRRecord.cSRID = cSRID;
            SRRecord.cEngineerID = cENGID;
            SRRecord.cEngineerName = cENGNAME;

            if (cReceiveTime != "")
            {
                SRRecord.cReceiveTime = Convert.ToDateTime(cReceiveTime);
            }

            if (cStartTime != "")
            {
                SRRecord.cStartTime = Convert.ToDateTime(cStartTime);
            }

            if (cArriveTime != "")
            {
                SRRecord.cArriveTime = Convert.ToDateTime(cArriveTime);
            }

            if (cFinishTime != "")
            {
                SRRecord.cFinishTime = Convert.ToDateTime(cFinishTime);
            }

            if (cDeleteTime != "")
            {
                SRRecord.cDeleteTime = Convert.ToDateTime(cDeleteTime);
            }

            if (cLocationS != "")
            {
                SRRecord.cLocationS = cLocationS;
            }

            if (cLocationA != "")
            {
                SRRecord.cLocationA = cLocationA;
            }

            SRRecord.CreatedDate = DateTime.Now;
            SRRecord.CreatedUserName = cENGNAME;
            #endregion

            return SRRecord;
        }
        #endregion

        #region 異動派工工時紀錄相關INPUT資訊
        /// <summary>異動派工工時紀錄相關INPUT資訊</summary>
        public struct SRFIXRECORD_INPUT
        {
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>服務工程師ERPID/技術主管ERPID</summary>
            public string IV_EMPNO { get; set; }
            /// <summary>接單時間</summary>
            public string IV_ReceiveTime { get; set; }
            /// <summary>出發時間</summary>
            public string IV_StartTime { get; set; }
            /// <summary>到場時間</summary>
            public string IV_ArriveTime { get; set; }
            /// <summary>完成時間</summary>
            public string IV_FinishTime { get; set; }
            /// <summary>刪除時間</summary>
            public string IV_DeleteTime { get; set; }
            /// <summary>是否要重新新增</summary>
            public string IV_ISRENEW { get; set; }
            /// <summary>座標位置(接單)</summary>
            public string IV_LocationS { get; set; }
            /// <summary>座標位置(到場)</summary>
            public string IV_LocationA { get; set; }
        }
        #endregion

        #region 異動派工工時紀錄相關OUTPUT資訊
        /// <summary>異動派工工時紀錄相關OUTPUT資訊</summary>
        public struct SRFIXRECORD_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑異動派工工時紀錄相關查詢接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓異動序號回報資訊相關接口 ↓↓↓↓↓-----        

        #region 新增序號回報資訊相關接口
        [HttpPost]
        public ActionResult API_SRSERIALFEEDBACKINFO_CREATE(SRSERIALFEEDBACKINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{                
            //    "IV_LOGINEMPNO": "99120894",            
            //    "IV_SRID" : "632304200001",
            //    "IV_SERIAL" : "SGH33223R6",
            //    "IV_MaterialID" : "G-M21161-001-3M",
            //    "IV_MaterialName" : "HP LCD BEZEL 13 HD",            
            //    "IV_ConfigReport" : "裝機Config檔"
            //}
            #endregion

            SRSERIALFEEDBACKINFO_OUTPUT ListOUT = new SRSERIALFEEDBACKINFO_OUTPUT();

            ListOUT = SaveSRSERIALFEEDBACKINFO(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 刪除序號回報資訊相關接口
        [HttpPost]
        public ActionResult API_SRSERIALFEEDBACKINFO_DELETE(SRSERIALFEEDBACKINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894", 
            //    "IV_SRID": "632304200001",
            //    "IV_CID": "1043"
            //}
            #endregion

            SRSERIALFEEDBACKINFO_OUTPUT ListOUT = new SRSERIALFEEDBACKINFO_OUTPUT();

            ListOUT = SaveSRSERIALFEEDBACKINFO(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 儲存序號回報資訊相關
        private SRSERIALFEEDBACKINFO_OUTPUT SaveSRSERIALFEEDBACKINFO(SRSERIALFEEDBACKINFO_INPUT beanIN)
        {
            SRSERIALFEEDBACKINFO_OUTPUT OUTBean = new SRSERIALFEEDBACKINFO_OUTPUT();

            int cID = 0;

            string IV_LOGINEMPNO = string.Empty;
            string IV_LOGINEMPName = string.Empty;
            string IV_SRID = string.Empty;
            string IV_SERIAL = string.Empty;
            string IV_MaterialID = string.Empty;
            string IV_MaterialName = string.Empty;
            string IV_ConfigReport = string.Empty;
            string path = string.Empty;
            string fileId = string.Empty;
            string fileOrgName = string.Empty;
            string fileName = string.Empty;

            Guid fileGuid = Guid.NewGuid();

            try
            {
                cID = string.IsNullOrEmpty(beanIN.IV_CID) ? 0 : int.Parse(beanIN.IV_CID);
                IV_SRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID;
                IV_SERIAL = string.IsNullOrEmpty(beanIN.IV_SERIAL) ? "" : beanIN.IV_SERIAL;
                IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO;
                IV_MaterialID = string.IsNullOrEmpty(beanIN.IV_MaterialID) ? "" : beanIN.IV_MaterialID;
                IV_MaterialName = string.IsNullOrEmpty(beanIN.IV_MaterialName) ? "" : beanIN.IV_MaterialName;

                HttpPostedFileBase upload = beanIN.IV_ConfigReport;

                #region 取得登入人員姓名
                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

                IV_LOGINEMPName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                #endregion

                if (cID == 0)
                {
                    #region 裝機Config檔
                    if (upload != null)
                    {
                        #region 檔案部份  
                        fileGuid = Guid.NewGuid();
                        fileId = fileGuid.ToString();
                        fileOrgName = upload.FileName;
                        fileName = fileId + Path.GetExtension(upload.FileName);
                        path = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                        upload.SaveAs(path);
                        #endregion

                        #region table部份  
                        IV_ConfigReport = fileGuid + ",";
                        TB_ONE_DOCUMENT bean = new TB_ONE_DOCUMENT();

                        bean.ID = fileGuid;
                        bean.FILE_ORG_NAME = fileOrgName;
                        bean.FILE_NAME = fileName;
                        bean.FILE_EXT = Path.GetExtension(upload.FileName);
                        bean.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        dbOne.TB_ONE_DOCUMENT.Add(bean);
                        dbOne.SaveChanges();
                        #endregion
                    }
                    #endregion

                    #region 新增
                    TB_ONE_SRDetail_SerialFeedback SFB = new TB_ONE_SRDetail_SerialFeedback();

                    SFB.cSRID = IV_SRID;
                    SFB.cSerialID = IV_SERIAL;
                    SFB.cMaterialID = IV_MaterialID;
                    SFB.cMaterialName = IV_MaterialName;
                    SFB.cConfigReport = IV_ConfigReport;
                    SFB.Disabled = 0;

                    SFB.CreatedDate = DateTime.Now;
                    SFB.CreatedUserName = IV_LOGINEMPName;

                    dbOne.TB_ONE_SRDetail_SerialFeedback.Add(SFB);
                    #endregion
                }
                else //刪除
                {
                    #region 刪除
                    var bean = dbOne.TB_ONE_SRDetail_SerialFeedback.FirstOrDefault(x => x.cID == cID);

                    if (bean != null)
                    {
                        bean.Disabled = 1;

                        bean.ModifiedDate = DateTime.Now;
                        bean.ModifiedUserName = IV_LOGINEMPName;
                    }
                    #endregion
                }

                var result = dbOne.SaveChanges();

                if (result <= 0)
                {
                    if (cID == 0) //新增
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新增失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                    }
                    else
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "刪除失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                    }

                    CMF.writeToLog(IV_SRID, "SaveSRSERIALFEEDBACKINFO_API", pMsg, IV_LOGINEMPName);

                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    if (cID == 0) //新增
                    {
                        var bean = dbOne.TB_ONE_SRDetail_SerialFeedback.OrderByDescending(x => x.cID).FirstOrDefault(x => x.cSRID == IV_SRID);

                        if (bean != null)
                        {
                            OUTBean.EV_CID = bean.cID.ToString();
                        }
                    }
                    else
                    {
                        OUTBean.EV_CID = cID.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_SRID, "SaveSRSERIALFEEDBACKINFO_API", pMsg, IV_LOGINEMPName);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
                OUTBean.EV_CID = "";
            }

            return OUTBean;
        }
        #endregion

        #region 異動序號回報資訊相關INPUT資訊
        /// <summary>異動序號回報資訊相關INPUT資訊</summary>
        public struct SRSERIALFEEDBACKINFO_INPUT
        {
            /// <summary>系統ID</summary>
            public string IV_CID { get; set; }
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>序號ID</summary>
            public string IV_SERIAL { get; set; }
            /// <summary>登入者員工編號</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>更換零件料號ID</summary>
            public string IV_MaterialID { get; set; }
            /// <summary>料號說明</summary>
            public string IV_MaterialName { get; set; }
            /// <summary>裝機Config</summary>
            public HttpPostedFileBase IV_ConfigReport { get; set; }
        }
        #endregion

        #region 異動序號回報資訊相關OUTPUT資訊
        /// <summary>異動序號回報資訊相關OUTPUT資訊</summary>
        public struct SRSERIALFEEDBACKINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
            /// <summary>系統ID</summary>
            public string EV_CID { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑異動序號回報資訊相關查詢接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓裝機現況資訊相關接口 ↓↓↓↓↓-----        

        #region 查詢裝機現況資訊相關接口
        [HttpPost]
        public ActionResult API_CURRENTINSTALLINFO_GET(INSTALLLIST_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{                            
            //    "IV_SRID" : "8300030821"            
            //}
            #endregion

            INSTALLLIST_OUTPUT ListOUT = new INSTALLLIST_OUTPUT();

            ListOUT = INSTALLLIST_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 查詢裝機現況資訊清單
        private INSTALLLIST_OUTPUT INSTALLLIST_GET(INSTALLLIST_INPUT beanIN)
        {
            INSTALLLIST_OUTPUT OUTBean = new INSTALLLIST_OUTPUT();

            try
            {
                var bean = dbEIP.TB_SERVICES_APP_INSTALL.FirstOrDefault(x => x.SRID == beanIN.IV_SRID.Trim());      //【正式】
                //var bean = dbEIP.TB_SERVICES_APP_INSTALLTEMP.FirstOrDefault(x => x.SRID == beanIN.IV_SRID.Trim());  //【測試】

                if (bean == null)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無裝機現況清單，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    OUTBean.CID = bean.ID.ToString();
                    OUTBean.SRID = bean.SRID;
                    OUTBean.ACCOUNT = bean.ACCOUNT;
                    OUTBean.ERP_ID = bean.ERP_ID;
                    OUTBean.EMP_NAME = bean.EMP_NAME;
                    OUTBean.InstallDate = bean.InstallDate;
                    OUTBean.ExpectedDate = bean.ExpectedDate;
                    OUTBean.TotalQuantity = bean.TotalQuantity.ToString();
                    OUTBean.InstallQuantity = bean.InstallQuantity.ToString();
                    OUTBean.INSERT_TIME = bean.INSERT_TIME;
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "INSTALLLIST_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢裝機現況資訊相關INPUT資訊
        /// <summary>裝機現況資訊相關INPUT資訊</summary>
        public struct INSTALLLIST_INPUT
        {
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
        }
        #endregion

        #region 查詢裝機現況資訊相關OUTPUT資訊
        /// <summary>查詢裝機現況資訊相關OUTPUT資訊</summary>
        public struct INSTALLLIST_OUTPUT
        {
            /// <summary>系統ID</summary>
            public string CID { get; set; }
            /// <summary>服務案件ID</summary>
            public string SRID { get; set; }
            /// <summary>安裝工程師AD帳號</summary>
            public string ACCOUNT { get; set; }
            /// <summary>安裝工程師ERPID</summary>
            public string ERP_ID { get; set; }
            /// <summary>安裝工程師姓名</summary>
            public string EMP_NAME { get; set; }
            /// <summary>裝機起始日期</summary>
            public string InstallDate { get; set; }
            /// <summary>裝機完成日期</summary>
            public string ExpectedDate { get; set; }
            /// <summary>總安裝數量</summary>
            public string TotalQuantity { get; set; }
            /// <summary>已安裝數量</summary>
            public string InstallQuantity { get; set; }
            /// <summary>建立日期</summary>
            public string INSERT_TIME { get; set; }
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #region 更新裝機現況資訊相關接口
        [HttpPost]
        public ActionResult API_CURRENTINSTALLINFO_UPDATE(CURRENTINSTALLINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894", 
            //    "IV_SRID": "632306120001",
            //    "IV_CID": "202",
            //    "IV_InstallDate": "2023-06-12",
            //    "IV_ExpectedDate": "2023-06-12",
            //    "IV_TotalQuantity": "2",
            //    "IV_InstallQuantity": "1",
            //    "IV_IsFromAPP": "Y"
            //}
            #endregion

            CURRENTINSTALLINFO_OUTPUT ListOUT = new CURRENTINSTALLINFO_OUTPUT();

            ListOUT = SaveCURRENTINSTALLINFO(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 更新裝機現況資訊相關
        private CURRENTINSTALLINFO_OUTPUT SaveCURRENTINSTALLINFO(CURRENTINSTALLINFO_INPUT beanIN)
        {
            CURRENTINSTALLINFO_OUTPUT OUTBean = new CURRENTINSTALLINFO_OUTPUT();

            int cID = 0;
            int IV_InstallQuantity = 0;
            int IV_TotalQuantity = 0;

            string IV_LOGINEMPNO = string.Empty;
            string IV_LOGINEMPName = string.Empty;
            string IV_SRID = string.Empty;
            string IV_InstallDate = string.Empty;
            string IV_ExpectedDate = string.Empty;
            string IV_IsFromAPP = string.Empty;

            try
            {
                cID = string.IsNullOrEmpty(beanIN.IV_CID) ? 0 : int.Parse(beanIN.IV_CID);
                IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO;
                IV_SRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID;
                IV_InstallDate = string.IsNullOrEmpty(beanIN.IV_InstallDate) ? "" : beanIN.IV_InstallDate;
                IV_ExpectedDate = string.IsNullOrEmpty(beanIN.IV_ExpectedDate) ? "" : beanIN.IV_ExpectedDate;
                IV_InstallQuantity = string.IsNullOrEmpty(beanIN.IV_InstallQuantity) ? 0 : int.Parse(beanIN.IV_InstallQuantity);
                IV_TotalQuantity = string.IsNullOrEmpty(beanIN.IV_TotalQuantity) ? 0 : int.Parse(beanIN.IV_TotalQuantity);
                IV_IsFromAPP = string.IsNullOrEmpty(beanIN.IV_IsFromAPP) ? "" : beanIN.IV_IsFromAPP;

                #region 取得登入人員姓名
                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

                IV_LOGINEMPName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                #endregion

                #region 批次儲存APP_INSTALL檔
                string returnMsg = CMF.SaveTB_SERVICES_APP_INSTALL(EmpBean.EmployeeNO, IV_LOGINEMPName, EmpBean.EmployeeERPID, cID, IV_SRID,
                                                                     IV_TotalQuantity.ToString(), IV_InstallQuantity.ToString(), IV_InstallDate, IV_ExpectedDate, IV_IsFromAPP);

                if (returnMsg != "SUCCESS")
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";
                }
                #endregion
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_SRID, "SaveCURRENTINSTALLINFO_API", pMsg, IV_LOGINEMPName);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 更新裝機現況資訊相關INPUT資訊
        /// <summary>更新裝機現況資訊相關INPUT資訊</summary>
        public struct CURRENTINSTALLINFO_INPUT
        {
            /// <summary>登入者員工編號</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>系統ID</summary>
            public string IV_CID { get; set; }
            /// <summary>裝機起始日期</summary>
            public string IV_InstallDate { get; set; }
            /// <summary>裝機完成日期</summary>
            public string IV_ExpectedDate { get; set; }
            /// <summary>總安裝數量</summary>
            public string IV_TotalQuantity { get; set; }
            /// <summary>已安裝數量</summary>
            public string IV_InstallQuantity { get; set; }
            /// <summary>是否來自APP更新(Y.是 N.否)</summary>
            public string IV_IsFromAPP { get; set; }
        }
        #endregion

        #region 更新裝機現況資訊相關OUTPUT資訊
        /// <summary>更新裝機現況資訊相關OUTPUT資訊</summary>
        public struct CURRENTINSTALLINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑裝機現況資訊相關查詢接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓員工資料接口 ↓↓↓↓↓-----        

        #region 查詢員工資料接口
        [HttpPost]
        public ActionResult API_EMPLOYEEINFO_GET(EMPLOYEEINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_EMPNAME": "白旭翔",
            //    "IV_SRTEAM": "SRV.12300003"
            //}
            #endregion

            EMPLOYEEINFO_OUTPUT ListOUT = new EMPLOYEEINFO_OUTPUT();

            ListOUT = EMPLOYEEINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得員工資料
        private EMPLOYEEINFO_OUTPUT EMPLOYEEINFO_GET(EMPLOYEEINFO_INPUT beanIN)
        {
            EMPLOYEEINFO_OUTPUT OUTBean = new EMPLOYEEINFO_OUTPUT();

            try
            {
                string EMPCOMPID = "T012";
                string IV_EMPNAME = string.IsNullOrEmpty(beanIN.IV_EMPNAME) ? "" : beanIN.IV_EMPNAME.Trim();
                string IV_SRTEAM = string.IsNullOrEmpty(beanIN.IV_SRTEAM) ? "" : beanIN.IV_SRTEAM.Trim();

                var tList = CMF.findEMPINFO(IV_EMPNAME, IV_SRTEAM);

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無員工資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得員工資料List
                    List<EMPLOYEEINFO_LIST> tEMPList = new List<EMPLOYEEINFO_LIST>();

                    foreach (var bean in tList)
                    {
                        EMPLOYEEINFO_LIST beanEMP = new EMPLOYEEINFO_LIST();

                        switch (bean.Comp_Cde.Trim())
                        {
                            case "Comp-1":
                                EMPCOMPID = "T012";
                                break;
                            case "Comp-2":
                                EMPCOMPID = "T016";
                                break;
                            case "Comp-3":
                                EMPCOMPID = "C069";
                                break;
                            case "Comp-4":
                                EMPCOMPID = "T022";
                                break;
                        }

                        beanEMP.EMPID = bean.ERP_ID;
                        beanEMP.EMPNAME = bean.Name2;
                        beanEMP.EMPENNAME = bean.Name;
                        beanEMP.EMPACCOUNT = bean.Account;
                        beanEMP.EMPCOMPID = EMPCOMPID;

                        tEMPList.Add(beanEMP);
                    }

                    OUTBean.EMP_LIST = tEMPList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "EMPLOYEEINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢員工資料INPUT資訊
        /// <summary>查詢員工資料資料INPUT資訊</summary>
        public struct EMPLOYEEINFO_INPUT
        {
            /// <summary>員工姓名(英文名/中文名)</summary>
            public string IV_EMPNAME { get; set; }
            /// <summary>服務團隊代碼(非必填)</summary>
            public string IV_SRTEAM { get; set; }
        }
        #endregion

        #region 查詢員工資料OUTPUT資訊
        /// <summary>查詢員工資料OUTPUT資訊</summary>
        public struct EMPLOYEEINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>員工資料清單</summary>
            public List<EMPLOYEEINFO_LIST> EMP_LIST { get; set; }
        }

        public struct EMPLOYEEINFO_LIST
        {
            /// <summary>員工ERPID</summary>
            public string EMPID { get; set; }
            /// <summary>員工姓名(中文)</summary>
            public string EMPNAME { get; set; }
            /// <summary>員工姓名(英文)</summary>
            public string EMPENNAME { get; set; }
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string EMPCOMPID { get; set; }
            /// <summary>員工帳號</summary>
            public string EMPACCOUNT { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑員工資料接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓服務團隊資料接口 ↓↓↓↓↓-----        

        #region 查詢服務團隊資料接口
        [HttpPost]
        public ActionResult API_SRTEAMINFO_GET(SRTEAMINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_COMPID": "T012"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = SRTEAMINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得服務團隊資料
        private OPTION_OUTPUT SRTEAMINFO_GET(SRTEAMINFO_INPUT beanIN)
        {
            OPTION_OUTPUT OUTBean = new OPTION_OUTPUT();

            try
            {
                var tList = CMF.findSRTEAMINFO(beanIN.IV_COMPID);
                var tListDis = tList.Select(m => new { m.cTeamOldID, m.cTeamOldName }).Distinct().ToList();

                if (tListDis.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無服務團隊資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得服務團隊資料List
                    List<OPTION_LIST> tEMPList = new List<OPTION_LIST>();

                    foreach (var bean in tListDis)
                    {
                        OPTION_LIST beanTEAM = new OPTION_LIST();

                        beanTEAM.VALUE = bean.cTeamOldID;   //服務團隊ID
                        beanTEAM.TEXT = bean.cTeamOldName;  //服務團隊名稱

                        tEMPList.Add(beanTEAM);
                    }

                    OUTBean.OPTION_LIST = tEMPList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "SRTEAMINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢服務團隊資料INPUT資訊
        /// <summary>查詢服務團隊資料INPUT資訊</summary>
        public struct SRTEAMINFO_INPUT
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string IV_COMPID { get; set; }
        }
        #endregion        

        #endregion -----↑↑↑↑↑服務團隊資料接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓SQ人員資料接口 ↓↓↓↓↓-----        

        #region 查詢SQ人員資料接口
        [HttpPost]
        public ActionResult API_SQINFO_GET(SQINFO_INPUT beanIV)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_EMPNAME": "陳勁嘉"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = SQINFO_GET(beanIV);

            return Json(ListOUT);
        }
        #endregion

        #region 取得SQ人員資料
        private OPTION_OUTPUT SQINFO_GET(SQINFO_INPUT beanIN)
        {
            OPTION_OUTPUT OUTBean = new OPTION_OUTPUT();

            try
            {
                var tList = CMF.findSQINFO(beanIN.IV_EMPNAME.Trim());

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無SQ人員資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得SQ人員資料List
                    List<OPTION_LIST> tEMPList = new List<OPTION_LIST>();

                    foreach (var bean in tList)
                    {
                        OPTION_LIST beanTEAM = new OPTION_LIST();

                        beanTEAM.VALUE = bean.cFullKEY; //SQ人員ID
                        beanTEAM.TEXT = bean.cFullNAME; //SQ人員說明

                        tEMPList.Add(beanTEAM);
                    }

                    OUTBean.OPTION_LIST = tEMPList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "SQINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢SQ人員資料INPUT資訊
        /// <summary>查詢SQ人員資料INPUT資訊</summary>
        public struct SQINFO_INPUT
        {
            /// <summary>姓名/類別</summary>
            public string IV_EMPNAME { get; set; }
        }
        #endregion      

        #endregion -----↑↑↑↑↑服務團隊資料接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓料號(非備品)資料接口 ↓↓↓↓↓-----        

        #region 查詢料號(非備品)資料接口
        [HttpPost]
        public ActionResult API_MATERIALINFO_GET(MATERIALINFO_INPUT beanIV)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //    "IV_MATERIAL": "507284"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = MATERIALINFO_GET(beanIV);

            return Json(ListOUT);
        }
        #endregion

        #region 取得料號(非備品)資料
        private OPTION_OUTPUT MATERIALINFO_GET(MATERIALINFO_INPUT beanIN)
        {
            OPTION_OUTPUT OUTBean = new OPTION_OUTPUT();

            try
            {
                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(beanIN.IV_LOGINEMPNO);

                var tList = CMF.findMATERIALINFO(beanIN.IV_MATERIAL.Trim(), EmpBean.CompanyCode);

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無料號資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得料號資料List
                    List<OPTION_LIST> tEMPList = new List<OPTION_LIST>();

                    foreach (var bean in tList)
                    {
                        OPTION_LIST beanTEAM = new OPTION_LIST();

                        beanTEAM.VALUE = bean.MARA_MATNR;   //料號ID
                        beanTEAM.TEXT = bean.MAKT_TXZA1_ZF; //料號說明

                        tEMPList.Add(beanTEAM);
                    }

                    OUTBean.OPTION_LIST = tEMPList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "MATERIALINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢料號資料INPUT資訊
        /// <summary>查詢料號資料INPUT資訊</summary>
        public struct MATERIALINFO_INPUT
        {
            /// <summary>登入者員工編號</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>料號/料號說明</summary>
            public string IV_MATERIAL { get; set; }
        }
        #endregion      

        #endregion -----↑↑↑↑↑料號(非備品)資料接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓料號(for備品)資料接口 ↓↓↓↓↓-----        

        #region 查詢料號(for備品)資料接口
        [HttpPost]
        public ActionResult API_MATERIALSPAREINFO_GET(MATERIALINFO_INPUT beanIV)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894",
            //    "IV_MATERIAL": "507284"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = MATERIALSPARE_GET(beanIV);

            return Json(ListOUT);
        }
        #endregion

        #region 取得料號(for備品)資料
        private OPTION_OUTPUT MATERIALSPARE_GET(MATERIALINFO_INPUT beanIN)
        {
            OPTION_OUTPUT OUTBean = new OPTION_OUTPUT();

            try
            {
                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(beanIN.IV_LOGINEMPNO);

                var tList = CMF.findMATERIALSPAREINFO(beanIN.IV_MATERIAL.Trim(), EmpBean.CompanyCode);

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無料號資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得料號資料List
                    List<OPTION_LIST> tEMPList = new List<OPTION_LIST>();

                    foreach (var bean in tList)
                    {
                        OPTION_LIST beanTEAM = new OPTION_LIST();

                        beanTEAM.VALUE = bean.MARA_MATNR;   //料號ID
                        beanTEAM.TEXT = bean.MAKT_TXZA1_ZF; //料號說明

                        tEMPList.Add(beanTEAM);
                    }

                    OUTBean.OPTION_LIST = tEMPList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "MATERIALSPAREINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion      

        #endregion -----↑↑↑↑↑料號(for備品)資料接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓報修類別資料接口 ↓↓↓↓↓-----        

        #region 查詢報修類別(大類)資料接口
        [HttpPost]
        public ActionResult API_SRKINDONE_GET(SRKIND_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_COMPID": "T012"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = SRKIND_GET(beanIN, 1);

            return Json(ListOUT);
        }
        #endregion

        #region 查詢報修類別(中類)資料接口
        [HttpPost]
        public ActionResult API_SRKINDSEC_GET(SRKIND_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_COMPID": "T012",
            //    "IV_ONEID": "ZA02"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = SRKIND_GET(beanIN, 2);

            return Json(ListOUT);
        }
        #endregion

        #region 查詢報修類別(小類)資料接口
        [HttpPost]
        public ActionResult API_SRKINDTHR_GET(SRKIND_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_COMPID": "T012",
            //    "IV_ONEID": "ZA02",
            //    "IV_SECID": "ZB0201"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = SRKIND_GET(beanIN, 3);

            return Json(ListOUT);
        }
        #endregion

        #region 取得報修類別資料
        private OPTION_OUTPUT SRKIND_GET(SRKIND_INPUT beanIN, int tLevel)
        {
            OPTION_OUTPUT OUTBean = new OPTION_OUTPUT();

            try
            {
                List<SelectListItem> tList = new List<SelectListItem>();

                if (tLevel == 1) //大類
                {
                    tList = CMF.findFirstKINDList(beanIN.IV_COMPID);
                }
                else if (tLevel == 2) //中類
                {
                    tList = CMF.findSRTypeSecList(beanIN.IV_COMPID, beanIN.IV_ONEID);
                }
                else if (tLevel == 3) //小類
                {
                    tList = CMF.findSRTypeThrList(beanIN.IV_COMPID, beanIN.IV_SECID);
                }

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無報修類別資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得報修類別資料List
                    List<OPTION_LIST> tEMPList = new List<OPTION_LIST>();

                    foreach (var bean in tList)
                    {
                        OPTION_LIST beanTEAM = new OPTION_LIST();

                        beanTEAM.VALUE = bean.Value;   //報修類別ID
                        beanTEAM.TEXT = bean.Text;     //報修類別名稱

                        tEMPList.Add(beanTEAM);
                    }

                    OUTBean.OPTION_LIST = tEMPList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "SRKIND_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢報修類別資料INPUT資訊
        /// <summary>查詢報修類別資料INPUT資訊</summary>
        public struct SRKIND_INPUT
        {
            /// <summary>公司別ID(T012、T016、C069、T022)</summary>
            public string IV_COMPID { get; set; }
            /// <summary>類別ID(大類)</summary>
            public string IV_ONEID { get; set; }
            /// <summary>類別ID(中類)</summary>
            public string IV_SECID { get; set; }
        }
        #endregion        

        #endregion -----↑↑↑↑↑報修類別(大類)資料接口 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓下拉選項共用接口 ↓↓↓↓↓-----        

        #region 查詢維護服務種類接口
        [HttpPost]
        public ActionResult API_MASERVICETYPE_GET(OPTION_INPUT beanIV)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_COMPID": "T012"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = OPTION_GET(beanIV, "MASERVICETYPE");

            return Json(ListOUT);
        }
        #endregion

        #region 查詢報修管道接口
        [HttpPost]
        public ActionResult API_SRPATHWAY_GET(OPTION_INPUT beanIV)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_COMPID": "T012"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = OPTION_GET(beanIV, "SRPATHWAY");

            return Json(ListOUT);
        }
        #endregion       

        #region 查詢服務(一般/裝機/定維)案件狀態接口
        [HttpPost]
        public ActionResult API_SRSTATUS_GET(OPTION_INPUT beanIV)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_COMPID": "T012",
            //    "IV_CASETYPE": "ZSR1"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            string tFunction = "GENERALSRSTATUS";

            if (beanIV.IV_CASETYPE == "ZSR1")
            {
                tFunction = "GENERALSRSTATUS";
            }
            else if (beanIV.IV_CASETYPE == "ZSR3")
            {
                tFunction = "INSTALLSRSTATUS";
            }
            else if (beanIV.IV_CASETYPE == "ZSR5")
            {
                tFunction = "MAINTAINSRSTATUS";
            }

            ListOUT = OPTION_GET(beanIV, tFunction);

            return Json(ListOUT);
        }
        #endregion       

        #region 取得下拉選項清單
        private OPTION_OUTPUT OPTION_GET(OPTION_INPUT beanIV, string tFunction)
        {
            OPTION_OUTPUT OUTBean = new OPTION_OUTPUT();

            string SRType = pOperationID_GenerallySR;

            try
            {
                string tFunName = string.Empty;
                string tFunNo = string.Empty;

                switch (tFunction)
                {
                    case "MASERVICETYPE":
                        tFunName = "維護服務種類";
                        tFunNo = "SRMATYPE";
                        SRType = pOperationID_GenerallySR;
                        break;

                    case "SRPATHWAY":
                        tFunName = "報修管道";
                        tFunNo = "SRPATH";
                        SRType = pOperationID_GenerallySR;
                        break;

                    case "GENERALSRSTATUS": //一般
                        tFunName = "狀態";
                        tFunNo = "SRSTATUS";
                        break;

                    case "INSTALLSRSTATUS": //裝機 
                        tFunName = "狀態";
                        tFunNo = "SRSTATUS";
                        SRType = pOperationID_InstallSR;
                        break;

                    case "MAINTAINSRSTATUS": //定維
                        tFunName = "狀態";
                        tFunNo = "SRSTATUS";
                        SRType = pOperationID_MaintainSR;
                        break;
                }

                var tList = CMF.findOPTION(SRType, beanIV.IV_COMPID.Trim(), tFunNo);

                if (tList.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無" + tFunName + "，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得下拉選項List
                    List<OPTION_LIST> tOPTList = new List<OPTION_LIST>();

                    foreach (var bean in tList)
                    {
                        OPTION_LIST beanEMP = new OPTION_LIST();

                        beanEMP.VALUE = bean.Value;
                        beanEMP.TEXT = bean.Text;

                        tOPTList.Add(beanEMP);
                    }

                    OUTBean.OPTION_LIST = tOPTList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "OPTION_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #endregion -----↑↑↑↑↑下拉選項共用接口 ↑↑↑↑↑-----

        #region -----↓↓↓↓↓ 滿意度調查接口 ↓↓↓↓↓-----       

        #region 儲存SR滿意度調查結果
        [HttpPost]
        public ActionResult ApiSrSatisfyCreate(TB_ONE_SRSatisfy_Survey bean)
        {
            SRTODOLIST_OUTPUT OUTBean = new SRTODOLIST_OUTPUT();
            try
            {
                bean.CreatedDate = DateTime.Now;

                dbOne.TB_ONE_SRSatisfy_Survey.Add(bean);

                dbOne.SaveChanges();

                OUTBean.EV_MSGT = "Y";
                OUTBean.EV_MSG = "";
            }
            catch (Exception ex)
            {
                OUTBean.EV_MSGT = "N";
                OUTBean.EV_MSG = ex.Message;
            }
            return Json(OUTBean);
        }
        #endregion

        #endregion  -----↑↑↑↑↑ 滿意度調查接口 ↑↑↑↑↑-----     

        #region  -----↓↓↓↓↓合約管理相關 ↓↓↓↓↓-----    

        #region -----↓↓↓↓↓查詢是否可以讀取合約書PDF權限 ↓↓↓↓↓-----        

        #region 查詢是否可以讀取合約書PDF權限資料        
        [HttpPost]
        public ActionResult API_VIEWCONTRACTSMEMBERSINFO_GET(VIEWCONTRACTSMEMBERSINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO" : "10001567", 
            //    "IV_CONTRACTID" : "11204075", 
            //    "IV_SRTEAM": "SRV.12100000"
            //}
            #endregion

            VIEWCONTRACTSMEMBERSINFO_OUTPUT ListOUT = new VIEWCONTRACTSMEMBERSINFO_OUTPUT();

            ListOUT = VIEWCONTRACTSMEMBERSINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得是否可以讀取合約書PDF權限資料
        private VIEWCONTRACTSMEMBERSINFO_OUTPUT VIEWCONTRACTSMEMBERSINFO_GET(VIEWCONTRACTSMEMBERSINFO_INPUT beanIN)
        {
            VIEWCONTRACTSMEMBERSINFO_OUTPUT OUTBean = new VIEWCONTRACTSMEMBERSINFO_OUTPUT();

            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_CONTRACTID = string.IsNullOrEmpty(beanIN.IV_CONTRACTID) ? "" : beanIN.IV_CONTRACTID.Trim();
            string IV_SRTEAM = string.IsNullOrEmpty(beanIN.IV_SRTEAM) ? "" : beanIN.IV_SRTEAM.Trim();
            string ContractIDLimit = CMF.findSysParameterValue(pOperationID_Contract, "OTHER", "T012", "ContractIDLimit");

            string tSALES = string.Empty;
            string tSALESNAME = string.Empty;
            string tSALES_ASS = string.Empty;
            string tSALES_ASSNAME = string.Empty;
            string tMAINTAIN_SALES = string.Empty;
            string tMAINTAIN_SALESNAME = string.Empty;
            string tURL_LINK = string.Empty;
            string tORG_CODE = string.Empty;
            string tOBJ_NOTES = string.Empty;

            DataTable dtORG = new DataTable();

            Dictionary<string, string> DicORG = new Dictionary<string, string>(); //記錄服務組織人員

            bool tIsCanRead = false;    //判斷是否可以看合約書URL
            bool tIsExist = false;      //是否存在服務團隊

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            try
            {
                var beanM = dbOne.TB_ONE_ContractMain.FirstOrDefault(x => x.Disabled == 0 && x.cContractID == beanIN.IV_CONTRACTID);

                if (beanM != null)
                {
                    tSALES = beanM.cSoSales;
                    tSALESNAME = beanM.cSoSalesName;
                    tSALES_ASS = beanM.cSoSalesASS;
                    tSALES_ASSNAME = beanM.cSoSalesASSName;
                    tMAINTAIN_SALES = beanM.cMASales;
                    tMAINTAIN_SALESNAME = beanM.cMASalesName;

                    #region 判斷是否可以讀取合約書PDF權限
                    if (int.Parse(beanIN.IV_CONTRACTID) < int.Parse(ContractIDLimit))
                    {
                        #region 抓舊的組織                        
                        tIsExist = CMF.checkEmpIsExistSRTeamMapping_OLD(pOperationID_Contract, EmpBean.BUKRS, EmpBean.EmployeeNO);

                        if (!tIsExist)
                        {
                            tIsExist = CMF.checkEmpIsExist7X24List(pOperationID_Contract, EmpBean.BUKRS, EmpBean.EmployeeNO);  //取得7X24相關人員
                        }
                        #endregion
                    }
                    else
                    {
                        #region 抓新的組織
                        tIsExist = CMF.checkEmpIsExistSRTeamMapping(EmpBean.CostCenterID, EmpBean.DepartmentNO, IV_SRTEAM);

                        if (!tIsExist)
                        {
                            tIsExist = CMF.checkEmpIsExist7X24List(pOperationID_Contract, EmpBean.BUKRS, EmpBean.EmployeeNO);  //取得7X24相關人員
                        }
                        #endregion
                    }

                    //取得合約相關人員
                    CMF.SetDtORGPeople(tSALES, tSALESNAME, ref DicORG);
                    CMF.SetDtORGPeople(tSALES_ASS, tSALES_ASSNAME, ref DicORG);
                    CMF.SetDtORGPeople(tMAINTAIN_SALES, tMAINTAIN_SALESNAME, ref DicORG);

                    //判斷是否可以讀取合約書PDF
                    if (DicORG.Keys.Contains(IV_LOGINEMPNO) || tIsExist)
                    {
                        tIsCanRead = true;
                    }
                    #endregion

                    if (tIsCanRead)
                    {
                        OUTBean.EV_MSGT = "Y";
                        OUTBean.EV_MSG = "";
                        OUTBean.EV_IsCanRead = "Y";
                    }
                    else
                    {
                        OUTBean.EV_MSGT = "Y";
                        OUTBean.EV_MSG = "";
                        OUTBean.EV_IsCanRead = "N";
                    }
                }
                else
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無該文件編號相關資訊，請重新查詢！";
                    OUTBean.EV_IsCanRead = "N";
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "VIEWCONTRACTSMEMBERSINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
                OUTBean.EV_IsCanRead = "N";
            }

            return OUTBean;
        }
        #endregion       

        #region 查詢是否可以讀取合約書PDF權限資料INPUT資訊
        /// <summary>查詢是否可以讀取合約書PDF權限資料INPUT資訊</summary>
        public struct VIEWCONTRACTSMEMBERSINFO_INPUT
        {
            /// <summary>登入者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>文件編號</summary>
            public string IV_CONTRACTID { get; set; }
            /// <summary>服務團隊代碼</summary>
            public string IV_SRTEAM { get; set; }
        }
        #endregion

        #region 查詢是否可以讀取合約書PDF權限資料OUTPUT資訊
        /// <summary>查詢是否可以讀取合約書PDF權限資料OUTPUT資訊</summary>
        public struct VIEWCONTRACTSMEMBERSINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
            /// <summary>是否可以讀取合約書PDF(Y.是 N.否)</summary>
            public string EV_IsCanRead { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑查詢是否可以讀取合約書PDF權限 ↑↑↑↑↑-----

        #region -----↓↓↓↓↓合約主數據資料新增/異動時發送Mail通知 ↓↓↓↓↓-----        

        #region 合約主數據資料新增/異動時發送Mail通知資料        
        [HttpPost]
        public ActionResult API_CONTRACTCHANGE_SENDMAIL(CONTRACTCHANGE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO" : "99120894",
            //    "IV_CONTRACTID" : "11204075", 
            //    "IV_LOG": "地點_舊值【 台北市敦化南路二段65-67號10樓】 新值【 台北市敦化南路二段65-67號10樓之1】 "
            //}
            #endregion

            CONTRACTCHANGE_OUTPUT ListOUT = new CONTRACTCHANGE_OUTPUT();

            ListOUT = CONTRACTCHANGE_SENDMAIL(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 發送Mail通知資料
        private CONTRACTCHANGE_OUTPUT CONTRACTCHANGE_SENDMAIL(CONTRACTCHANGE_INPUT beanIN)
        {
            CONTRACTCHANGE_OUTPUT OUTBean = new CONTRACTCHANGE_OUTPUT();

            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_CONTRACTID = string.IsNullOrEmpty(beanIN.IV_CONTRACTID) ? "" : beanIN.IV_CONTRACTID.Trim();
            string IV_LOG = string.IsNullOrEmpty(beanIN.IV_LOG) ? "" : beanIN.IV_LOG.Trim();

            bool tIsFormal = false;
            string pLoginName = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;

            ContractCondition cCondition = ContractCondition.ADD;
            cCondition = string.IsNullOrEmpty(IV_LOG) ? ContractCondition.ADD : ContractCondition.SAVE;

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
            }

            #region 取得系統位址參數相關資訊
            SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

            tIsFormal = ParaBean.IsFormal;

            tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
            tONEURLName = ParaBean.ONEURLName;
            tBPMURLName = ParaBean.BPMURLName;
            tPSIPURLName = ParaBean.PSIPURLName;
            tAttachURLName = ParaBean.AttachURLName;
            #endregion

            try
            {
                #region 寄送Mail通知
                CMF.SetContractMailContent(cCondition, pOperationID_Contract, IV_CONTRACTID, IV_LOG, tONEURLName, pLoginName, tIsFormal);

                OUTBean.EV_MSGT = "Y";
                OUTBean.EV_MSG = "";
                #endregion
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "CONTRACTCHANGE_SENDMAIL_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion       

        #region 合約主數據資料新增/異動時發送Mail通知資料INPUT資訊
        /// <summary>合約主數據資料新增/異動時發送Mail通知資料INPUT資訊</summary>
        public struct CONTRACTCHANGE_INPUT
        {
            /// <summary>登入者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>文件編號</summary>
            public string IV_CONTRACTID { get; set; }
            /// <summary>LOG記錄</summary>
            public string IV_LOG { get; set; }
        }
        #endregion

        #region 合約主數據資料新增/異動時發送Mail通知資料OUTPUT資訊
        /// <summary>合約主數據資料新增/異動時發送Mail通知資料OUTPUT資訊</summary>
        public struct CONTRACTCHANGE_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑合約主數據資料異動時發送Mail通知 ↑↑↑↑↑-----

        #region -----↓↓↓↓↓合約工程師資料新增/異動時發送Mail通知 ↓↓↓↓↓-----        

        #region 合約工程師資料新增/異動時發送Mail通知資料        
        [HttpPost]
        public ActionResult API_CONTRACTENGCHANGE_SENDMAIL(CONTRACTENGCHANGE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO" : "99120894",
            //    "IV_CONTRACTID" : "10411208", 
            //    "IV_LOG": "工程師ERPID_舊值【 】 新值【99121417】 工程師姓名_舊值【 】 新值【胡崇閔 Choung.Hu】 是否為主要工程師_舊值【 】 新值【Y】",
            //    "IV_STATUS": "ADD",
            //    "IV_IsMain": "Y",
            //    "IV_OldMainEngineer": "",
            //    "IV_OldAssEngineer": ""
            //}
            #endregion

            CONTRACTENGCHANGE_OUTPUT ListOUT = new CONTRACTENGCHANGE_OUTPUT();

            ListOUT = CONTRACTENGCHANGE_SENDMAIL(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 發送Mail通知資料
        private CONTRACTENGCHANGE_OUTPUT CONTRACTENGCHANGE_SENDMAIL(CONTRACTENGCHANGE_INPUT beanIN)
        {
            CONTRACTENGCHANGE_OUTPUT OUTBean = new CONTRACTENGCHANGE_OUTPUT();

            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_CONTRACTID = string.IsNullOrEmpty(beanIN.IV_CONTRACTID) ? "" : beanIN.IV_CONTRACTID.Trim();
            string IV_LOG = string.IsNullOrEmpty(beanIN.IV_LOG) ? "" : beanIN.IV_LOG.Trim();
            string IV_STATUS = string.IsNullOrEmpty(beanIN.IV_STATUS) ? "" : beanIN.IV_STATUS.Trim();
            string IV_IsMain = string.IsNullOrEmpty(beanIN.IV_IsMain) ? "" : beanIN.IV_IsMain.Trim();
            string IV_OldMainEngineer = string.IsNullOrEmpty(beanIN.IV_OldMainEngineer) ? "" : beanIN.IV_OldMainEngineer.Trim();
            string IV_OldAssEngineer = string.IsNullOrEmpty(beanIN.IV_OldAssEngineer) ? "" : beanIN.IV_OldAssEngineer.Trim();

            bool tIsFormal = false;
            string pLoginName = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;

            ContractENGCondition cCondition = ContractENGCondition.ADD;

            switch(IV_STATUS)
            {
                case "ADD":
                    cCondition = ContractENGCondition.ADD;
                    break;

                case "EDIT":
                    cCondition = ContractENGCondition.EDIT;
                    break;

                case "DEL":
                    cCondition = ContractENGCondition.DEL;
                    break;
            }

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
            }

            #region 取得系統位址參數相關資訊
            SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

            tIsFormal = ParaBean.IsFormal;

            tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
            tONEURLName = ParaBean.ONEURLName;
            tBPMURLName = ParaBean.BPMURLName;
            tPSIPURLName = ParaBean.PSIPURLName;
            tAttachURLName = ParaBean.AttachURLName;
            #endregion

            try
            {
                #region 寄送Mail通知
                CMF.SetContractENGMailContent(cCondition, pOperationID_Contract, IV_CONTRACTID, IV_LOG, IV_IsMain, IV_OldMainEngineer, IV_OldAssEngineer, tONEURLName, pLoginName, tIsFormal);

                OUTBean.EV_MSGT = "Y";
                OUTBean.EV_MSG = "";
                #endregion
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "CONTRACTENGCHANGE_SENDMAIL_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion       

        #region 合約工程師資料新增/異動時發送Mail通知資料INPUT資訊
        /// <summary>合約工程師資料新增/異動時發送Mail通知資料INPUT資訊</summary>
        public struct CONTRACTENGCHANGE_INPUT
        {
            /// <summary>登入者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>文件編號</summary>
            public string IV_CONTRACTID { get; set; }
            /// <summary>LOG記錄</summary>
            public string IV_LOG { get; set; }
            /// <summary>執行狀態(ADD.新增 EDIT.編輯 DEL.刪除)</summary>
            public string IV_STATUS { get; set; }
            /// <summary>此次異動是否為主要工程師(Y.是 N.否)</summary>
            public string IV_IsMain { get; set; }
            /// <summary>原主要工程師ERPID</summary>
            public string IV_OldMainEngineer { get; set; }
            /// <summary>原協助工程師ERPID</summary>
            public string IV_OldAssEngineer { get; set; }
        }
        #endregion

        #region 合約工程師資料新增/異動時發送Mail通知資料OUTPUT資訊
        /// <summary>合約工程師資料新增/異動時發送Mail通知資料OUTPUT資訊</summary>
        public struct CONTRACTENGCHANGE_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑合約工程師資料異動時發送Mail通知 ↑↑↑↑↑-----

        #region -----↓↓↓↓↓ 建立ONE SERVICE 合約主數據接口 ↓↓↓↓↓-----        

        #region 建立ONE SERVICE 合約主數據資料        
        [HttpPost]
        public ActionResult API_CONTRACT_CREATE(CONTRACT_CREATE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO" : "99120894",
            //    "IV_BPMFORMNO" : "A2-20230626-001"           
            //}
            #endregion

            CONTRACT_CREATE_OUTPUT ListOUT = new CONTRACT_CREATE_OUTPUT();

            ListOUT = CONTRACT_CREATE(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 建立合約主數據資料
        private CONTRACT_CREATE_OUTPUT CONTRACT_CREATE(CONTRACT_CREATE_INPUT beanIN)
        {
            CONTRACT_CREATE_OUTPUT OUTBean = new CONTRACT_CREATE_OUTPUT();

            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_BPMFORMNO = string.IsNullOrEmpty(beanIN.IV_BPMFORMNO) ? "" : beanIN.IV_BPMFORMNO.Trim();

            bool tIsFormal = false;
            bool tIsExist = false; //判斷文件編號是否已存在

            string pLoginName = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string IV_CONTACT = string.Empty;

            ContractCondition cCondition = ContractCondition.ADD;

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
            }

            #region 取得系統位址參數相關資訊
            SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

            tIsFormal = ParaBean.IsFormal;

            tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
            tONEURLName = ParaBean.ONEURLName;
            tBPMURLName = ParaBean.BPMURLName;
            tPSIPURLName = ParaBean.PSIPURLName;
            tAttachURLName = ParaBean.AttachURLName;
            #endregion

            try
            {
                #region 取得BPM表單
                var beanBMP = CMF.findBPMCONTRACTINFO(IV_BPMFORMNO);
                IV_CONTACT = beanBMP.IV_CONTACT;
                #endregion

                #region BPM轉OneService合約主數據
                var beanMAIN = dbOne.TB_ONE_ContractMain.FirstOrDefault(x => x.Disabled == 0 && x.cContractID == IV_CONTACT);
                if (beanMAIN == null)
                {
                    TB_ONE_ContractMain beanM = new TB_ONE_ContractMain();

                    beanM.cContractID = beanBMP.IV_CONTACT;
                    beanM.cSoNo = beanBMP.IV_SONO;
                    beanM.cSoSales = beanBMP.IV_SALES;
                    beanM.cSoSalesName = CMF.findEmployeeNameInCludeLeave(beanBMP.IV_SALES);
                    beanM.cSoSalesASS = beanBMP.IV_ASSITANCE;
                    beanM.cSoSalesASSName = CMF.findEmployeeNameInCludeLeave(beanBMP.IV_ASSITANCE);
                    beanM.cMASales = beanBMP.IV_MAINTAIN_SALES;
                    beanM.cMASalesName = CMF.findEmployeeNameInCludeLeave(beanBMP.IV_MAINTAIN_SALES);
                    beanM.cCustomerID = beanBMP.IV_CUSTOMER;
                    beanM.cCustomerName = CMF.findCustName(beanBMP.IV_CUSTOMER);
                    beanM.cDesc = beanBMP.IV_SODESC;
                    beanM.cStartDate = beanBMP.IV_SDATE;
                    beanM.cEndDate = beanBMP.IV_EDATE;
                    beanM.cMACycle = beanBMP.IV_CYCLE;
                    beanM.cMANotes = beanBMP.IV_NOTES;
                    beanM.cMAAddress = beanBMP.IV_ADDR;
                    beanM.cSLARESP = beanBMP.IV_SLARESP;
                    beanM.cSLASRV = beanBMP.IV_SLASRV;
                    beanM.cContractNotes = beanBMP.IV_NOTE;
                    beanM.cTeamID = beanBMP.IV_ORGID;
                    beanM.cBillCycle = beanBMP.IV_REQPAY;
                    beanM.cBillNotes = beanBMP.IV_PAYNOTE;
                    beanM.cContactName = beanBMP.IV_ContactName;
                    beanM.cContactEmail = beanBMP.IV_ContactEmail;

                    if (beanBMP.IV_CUSTOMER == "") //供應商
                    {
                        beanM.cIsSubContract = "Y";
                    }
                    else //客戶
                    {
                        beanM.cIsSubContract = "N";
                    }

                    beanM.Disabled = 0;
                    beanM.CreatedDate = DateTime.Now;
                    beanM.CreatedUserName = pLoginName;

                    dbOne.TB_ONE_ContractMain.Add(beanM);
                }
                else
                {
                    tIsExist = true;
                }
                #endregion

                #region BPM轉OneService明細下包合約資料
                if (beanBMP.IV_CUSTOMER == "") //供應商
                {
                    var beanSub = dbOne.TB_ONE_ContractDetail_SUB.FirstOrDefault(x => x.Disabled == 0 && x.cContractID == beanBMP.IV_MAINID && x.cSubContractID == beanBMP.IV_CONTACT);
                    if (beanSub == null)
                    {
                        TB_ONE_ContractDetail_SUB beanD = new TB_ONE_ContractDetail_SUB();

                        beanD.cContractID = beanBMP.IV_MAINID;
                        beanD.cSubContractID = beanBMP.IV_CONTACT;
                        beanD.cSubSupplierID = beanBMP.IV_SUBNUMBER;
                        beanD.cSubSupplierName = beanBMP.IV_ContractUser;
                        beanD.cSubNotes = beanBMP.IV_NOTES;

                        beanD.Disabled = 0;
                        beanD.CreatedDate = DateTime.Now;
                        beanD.CreatedUserName = pLoginName;

                        dbOne.TB_ONE_ContractDetail_SUB.Add(beanD);
                    }
                    else
                    {                     
                        beanSub.cSubSupplierID = beanBMP.IV_SUBNUMBER;
                        beanSub.cSubSupplierName = beanBMP.IV_ContractUser;
                        beanSub.cSubNotes = beanBMP.IV_NOTES;

                        beanSub.Disabled = 0;
                        beanSub.CreatedDate = DateTime.Now;
                        beanSub.CreatedUserName = pLoginName;
                    }
                }
                #endregion

                if (!tIsExist)
                {
                    int result = dbOne.SaveChanges();

                    if (result <= 0)
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "儲存失敗" + Environment.NewLine;
                        CMF.writeToLog(IV_CONTACT, "CONTRACT_CREATE_API", pMsg, pLoginName);

                        OUTBean.EV_MSGT = "E";
                        OUTBean.EV_MSG = pMsg;
                    }
                    else
                    {
                        if (!tIsExist) //文件編號不存在才需要發Mail通知
                        {
                            #region 更新合約主數據(主約)的內部轉撥維護業務
                            if (beanBMP.IV_SUBCONTACT != "") //判斷若是有內部轉撥服務過來的，若是下包約時，要順便更新主約的維護業務
                            {                                
                                var beanM2 = dbOne.TB_ONE_ContractMain.FirstOrDefault(x => x.Disabled == 0 && x.cContractID == beanBMP.IV_SUBCONTACT);

                                if (beanM2 != null)
                                {
                                    beanM2.cMASales = beanBMP.IV_MAINTAIN_SALES;
                                    beanM2.cMASalesName = CMF.findEmployeeNameInCludeLeave(beanBMP.IV_MAINTAIN_SALES);

                                    dbOne.SaveChanges();
                                }                               
                            }                            
                            #endregion

                            #region 寄送Mail通知
                            if (beanBMP.IV_CUSTOMER != "") //客戶代表是主約才需要寄送Mail通知
                            {
                                CMF.SetContractMailContent(cCondition, pOperationID_Contract, IV_CONTACT, "", tONEURLName, pLoginName, tIsFormal);
                            }

                            OUTBean.EV_MSGT = "Y";
                            OUTBean.EV_MSG = "";
                            #endregion
                        }
                    }
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_CONTACT, "CONTRACT_CREATE_API", pMsg, pLoginName);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion       

        #region 建立ONE SERVICE 合約主數據資料INPUT資訊
        /// <summary>建立ONE SERVICE 合約主數據資料INPUT資訊</summary>
        public struct CONTRACT_CREATE_INPUT
        {
            /// <summary>登入者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>BPM表單編號</summary>
            public string IV_BPMFORMNO { get; set; }
        }
        #endregion

        #region 建立ONE SERVICE 合約主數據資料OUTPUT資訊
        /// <summary>建立ONE SERVICE 合約主數據資料OUTPUT資訊</summary>
        public struct CONTRACT_CREATE_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑建立ONE SERVICE 合約主數據接口 ↑↑↑↑↑-----

        #region -----↓↓↓↓↓ 更新ONE SERVICE 合約主數據接口 ↓↓↓↓↓----- 

        #region 更新ONE SERVICE 合約主數據資料        
        [HttpPost]
        public ActionResult API_CONTRACT_UPDATE(CONTRACT_UPDATE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO" : "99120894",
            //    "IV_ContractID" : "11204075",            
            //    "IV_SoSalesNO" : "99120993",
            //    "IV_SoSalesASSNO" : "99121437",
            //    "IV_MASalesNO" : "99120993",
            //    "IV_SoNo" : "123456789",
            //    "IV_BillCycle" : "202307_202310"
            //}
            #endregion

            CONTRACT_UPDATE_OUTPUT ListOUT = new CONTRACT_UPDATE_OUTPUT();

            ListOUT = CONTRACT_UPDATE(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 更新合約主數據資料
        private CONTRACT_UPDATE_OUTPUT CONTRACT_UPDATE(CONTRACT_UPDATE_INPUT beanIN)
        {
            CONTRACT_UPDATE_OUTPUT OUTBean = new CONTRACT_UPDATE_OUTPUT();

            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_ContractID = string.IsNullOrEmpty(beanIN.IV_ContractID) ? "" : beanIN.IV_ContractID.Trim();
            string IV_SoSalesNO = string.IsNullOrEmpty(beanIN.IV_SoSalesNO) ? "" : beanIN.IV_SoSalesNO.Trim();
            string IV_SoSalesASSNO = string.IsNullOrEmpty(beanIN.IV_SoSalesASSNO) ? "" : beanIN.IV_SoSalesASSNO.Trim();
            string IV_MASalesNO = string.IsNullOrEmpty(beanIN.IV_MASalesNO) ? "" : beanIN.IV_MASalesNO.Trim();
            string IV_SoNo = string.IsNullOrEmpty(beanIN.IV_SoNo) ? "" : beanIN.IV_SoNo.Trim();
            string IV_BillCycle = string.IsNullOrEmpty(beanIN.IV_ContractID) ? "" : beanIN.IV_BillCycle.Trim();

            bool tIsFormal = false;            

            string pLoginName = string.Empty;
            string tAPIURLName = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;            

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
            }

            #region 取得系統位址參數相關資訊
            SRSYSPARAINFO ParaBean = CMF.findSRSYSPARAINFO(pOperationID_GenerallySR);

            tIsFormal = ParaBean.IsFormal;

            tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;
            tONEURLName = ParaBean.ONEURLName;
            tBPMURLName = ParaBean.BPMURLName;
            tPSIPURLName = ParaBean.PSIPURLName;
            tAttachURLName = ParaBean.AttachURLName;
            #endregion

            try
            {
                var beanM = dbOne.TB_ONE_ContractMain.FirstOrDefault(x => x.Disabled == 0 && x.cContractID == IV_ContractID);
                
                if (beanM != null)
                {
                    if (IV_SoSalesNO != "")
                    {
                        beanM.cSoSales = IV_SoSalesNO;
                        beanM.cSoSalesName = CMF.findEmployeeNameInCludeLeave(IV_SoSalesNO);
                    }

                    if (IV_SoSalesASSNO != "")
                    {
                        beanM.cSoSalesASS = IV_SoSalesASSNO;
                        beanM.cSoSalesASSName = CMF.findEmployeeNameInCludeLeave(IV_SoSalesASSNO);
                    }

                    if (IV_MASalesNO != "")
                    {
                        beanM.cMASales = IV_MASalesNO;
                        beanM.cMASalesName = CMF.findEmployeeNameInCludeLeave(IV_MASalesNO);
                    }

                    if (IV_SoNo != "")
                    {
                        beanM.cSoNo = IV_SoNo;
                    }

                    if (IV_BillCycle != "")
                    {
                        beanM.cBillCycle = IV_BillCycle;
                    }

                    beanM.ModifiedDate = DateTime.Now;
                    beanM.ModifiedUserName = pLoginName;                    

                    int result = dbOne.SaveChanges();

                    if (result <= 0)
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "儲存失敗" + Environment.NewLine;
                        CMF.writeToLog(IV_ContractID, "CONTRACT_UPDATE_API", pMsg, pLoginName);

                        OUTBean.EV_MSGT = "E";
                        OUTBean.EV_MSG = pMsg;
                    }
                    else
                    {
                        OUTBean.EV_MSGT = "Y";
                        OUTBean.EV_MSG = "";
                    }
                }
                else
                {
                    pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "儲存失敗，找不到該合約主檔資訊！" + Environment.NewLine;
                    CMF.writeToLog(IV_ContractID, "CONTRACT_UPDATE_API", pMsg, pLoginName);

                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }                
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_ContractID, "CONTRACT_UPDATE_API", pMsg, pLoginName);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion       

        #region 更新ONE SERVICE 合約主數據資料INPUT資訊
        /// <summary>更新ONE SERVICE 合約主數據資料INPUT資訊</summary>
        public struct CONTRACT_UPDATE_INPUT
        {
            /// <summary>登入者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }            
            /// <summary>文件編號</summary>
            public string IV_ContractID { get; set; }            
            /// <summary>業務員ERPID</summary>
            public string IV_SoSalesNO { get; set; }
            /// <summary>業務祕書ERPID</summary>
            public string IV_SoSalesASSNO { get; set; }
            /// <summary>維護業務員ERPID</summary>
            public string IV_MASalesNO { get; set; }
            /// <summary>銷售訂單號</summary>
            public string IV_SoNo { get; set; }
            /// <summary>請款週期</summary>
            public string IV_BillCycle { get; set; }            
        }
        #endregion

        #region 更新ONE SERVICE 合約主數據資料OUTPUT資訊
        /// <summary>更新ONE SERVICE 合約主數據資料OUTPUT資訊</summary>
        public struct CONTRACT_UPDATE_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑更新ONE SERVICE 合約主數據接口 ↑↑↑↑↑-----

        #region -----↓↓↓↓↓ 作廢ONE SERVICE 合約主數據接口 ↓↓↓↓↓----- 

        #region 作廢ONE SERVICE 合約主數據資料        
        [HttpPost]
        public ActionResult API_CONTRACT_DELETE(CONTRACT_DELETE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO" : "99120894",
            //    "IV_ContractID" : "11204075"            
            //}
            #endregion

            CONTRACT_DELETE_OUTPUT ListOUT = new CONTRACT_DELETE_OUTPUT();

            ListOUT = CONTRACT_DELETE(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 更新合約主數據資料
        private CONTRACT_DELETE_OUTPUT CONTRACT_DELETE(CONTRACT_DELETE_INPUT beanIN)
        {
            CONTRACT_DELETE_OUTPUT OUTBean = new CONTRACT_DELETE_OUTPUT();

            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_ContractID = string.IsNullOrEmpty(beanIN.IV_ContractID) ? "" : beanIN.IV_ContractID.Trim();
            string pLoginName = string.Empty;           

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
            }          

            try
            {
                //停用合約主檔
                var beanM = dbOne.TB_ONE_ContractMain.FirstOrDefault(x => x.Disabled == 0 && x.cContractID == IV_ContractID);

                if (beanM != null)
                {
                    beanM.Disabled = 1;
                    beanM.ModifiedDate = DateTime.Now;
                    beanM.ModifiedUserName = pLoginName;

                    #region 停用合約明細-合約工程師檔
                    var beansEng = dbOne.TB_ONE_ContractDetail_ENG.Where(x => x.Disabled == 0 && x.cContractID == IV_ContractID);
                    foreach(var beanEng in beansEng)
                    {
                        beanEng.Disabled = 1;
                        beanEng.ModifiedDate = DateTime.Now;
                        beanEng.ModifiedUserName = pLoginName;
                    }
                    #endregion

                    #region 停用合約明細-合約維護標的檔
                    var beansObj = dbOne.TB_ONE_ContractDetail_OBJ.Where(x => x.Disabled == 0 && x.cContractID == IV_ContractID);
                    foreach (var beanObj in beansObj)
                    {
                        beanObj.Disabled = 1;
                        beanObj.ModifiedDate = DateTime.Now;
                        beanObj.ModifiedUserName = pLoginName;
                    }
                    #endregion

                    #region 停用合約明細-下包合約資料檔
                    var beansSub = dbOne.TB_ONE_ContractDetail_SUB.Where(x => x.Disabled == 0 && x.cContractID == IV_ContractID);
                    foreach (var beanSub in beansSub)
                    {
                        beanSub.Disabled = 1;
                        beanSub.ModifiedDate = DateTime.Now;
                        beanSub.ModifiedUserName = pLoginName;
                    }
                    #endregion

                    int result = dbOne.SaveChanges();

                    if (result <= 0)
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "儲存失敗" + Environment.NewLine;
                        CMF.writeToLog(IV_ContractID, "CONTRACT_DELETE_API", pMsg, pLoginName);

                        OUTBean.EV_MSGT = "E";
                        OUTBean.EV_MSG = pMsg;
                    }
                    else
                    {
                        OUTBean.EV_MSGT = "Y";
                        OUTBean.EV_MSG = "";
                    }
                }
                else
                {
                    pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "儲存失敗，找不到該合約主檔資訊！" + Environment.NewLine;
                    CMF.writeToLog(IV_ContractID, "CONTRACT_DELETE_API", pMsg, pLoginName);

                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_ContractID, "CONTRACT_DELETE_API", pMsg, pLoginName);

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion       

        #region 作廢ONE SERVICE 合約主數據資料INPUT資訊
        /// <summary>作廢ONE SERVICE 合約主數據資料INPUT資訊</summary>
        public struct CONTRACT_DELETE_INPUT
        {
            /// <summary>登入者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>文件編號</summary>
            public string IV_ContractID { get; set; }            
        }
        #endregion

        #region 作廢ONE SERVICE 合約主數據資料OUTPUT資訊
        /// <summary>作廢ONE SERVICE 合約主數據資料OUTPUT資訊</summary>
        public struct CONTRACT_DELETE_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑更新ONE SERVICE 合約主數據接口 ↑↑↑↑↑-----

        #endregion -----↑↑↑↑↑合約管理相關 ↑↑↑↑↑-----

        #region -----↓↓↓↓↓CALL RFC接口 ↓↓↓↓↓-----    

        #region 初始SapConnector
        public void initSapConnector()
        {
            #region 呼叫SAPERP正式區或測試區(true.正式區 false.測試區)
            bool tIsFormal = CMF.getCallSAPERPPara(pOperationID_GenerallySR); //取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)

            if (tIsFormal)
            {
                if (sapConnector == null)
                {
                    sapConnector = new SapConnector().getCRM_PRD(); //正式機                    
                }

                if (sapTatungConnector == null)
                {
                    sapTatungConnector = new SapConnector().getTatung_PRD(); //正式機
                }
            }
            else
            {
                if (sapConnector == null)
                {
                    sapConnector = new SapConnector().getCRM_QAS(); //測試環境
                }

                if (sapTatungConnector == null)
                {
                    sapTatungConnector = new SapConnector().getTatung_QAS(); //測試環境
                }
            }
            #endregion
        }
        #endregion

        #region -----↓↓↓↓↓查詢合約標的(暫時先不用) ↓↓↓↓↓-----

        #region 測試取得合約標的資料
        //[HttpPost]
        //public ActionResult callAPI_CONTRACTOBJINFO_GET(CONTRACTOBJINFO_INPUT beanIN)
        //{
        //    var beanList = GetAPI_CONTRACTOBJINFO_GET(beanIN);

        //    return Json(beanList);
        //}

        ///// <summary>
        ///// 測試取得合約標的資料
        ///// </summary>
        ///// <param name="beanIN"></param>
        //public CONTRACTOBJINFO_OUTPUT GetAPI_CONTRACTOBJINFO_GET(CONTRACTOBJINFO_INPUT beanIN)
        //{
        //    CONTRACTOBJINFO_OUTPUT OUTBean = new CONTRACTOBJINFO_OUTPUT();

        //    try
        //    {
        //        var client = new RestClient("http://localhost:32603/API/API_CONTRACTOBJINFO_GET");  //測試用            

        //        var request = new RestRequest();
        //        request.Method = RestSharp.Method.Post;

        //        Dictionary<Object, Object> parameters = new Dictionary<Object, Object>();
        //        parameters.Add("IV_CONTRACTID", beanIN.IV_CONTRACTID);

        //        request.AddHeader("Content-Type", "application/json");
        //        request.AddParameter("application/json", parameters, ParameterType.RequestBody);

        //        RestResponse response = client.Execute(request);

        //        #region 取得回傳訊息(成功或失敗)
        //        var data = (JObject)JsonConvert.DeserializeObject(response.Content);

        //        OUTBean.EV_MSGT = data["EV_MSGT"].ToString().Trim();
        //        OUTBean.EV_MSG = data["EV_MSG"].ToString().Trim();
        //        #endregion

        //        if (OUTBean.EV_MSGT == "Y")
        //        {
        //            #region 取得合約標的資料List
        //            var tList = (JArray)JsonConvert.DeserializeObject(data["CONTRACTOBJINFO_LIST"].ToString().Trim());

        //            if (tList != null)
        //            {
        //                List<CONTRACTOBJINFO_LIST> tCustList = new List<CONTRACTOBJINFO_LIST>();

        //                foreach (JObject bean in tList)
        //                {
        //                    CONTRACTOBJINFO_LIST beanCust = new CONTRACTOBJINFO_LIST();

        //                    beanCust.SUB_CONTRACTID = bean["SUB_CONTRACTID"].ToString().Trim();

        //                    tCustList.Add(beanCust);
        //                }

        //                OUTBean.CONTRACTOBJINFO_LIST = tCustList;
        //            }
        //            #endregion
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"callAPI_CONTRACTOBJINFO_GET Error: {ex}");
        //    }

        //    return OUTBean;
        //}
        #endregion

        #region 查詢合約標的資料        
        [HttpPost]
        public ActionResult API_CONTRACTOBJINFO_GET(CONTRACTOBJINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_CONTRACTID": "11104083"
            //}
            #endregion

            CONTRACTOBJINFO_OUTPUT ListOUT = new CONTRACTOBJINFO_OUTPUT();

            ListOUT = CONTRACTOBJINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得合約標的資料
        private CONTRACTOBJINFO_OUTPUT CONTRACTOBJINFO_GET(CONTRACTOBJINFO_INPUT beanIN)
        {
            CONTRACTOBJINFO_OUTPUT OUTBean = new CONTRACTOBJINFO_OUTPUT();

            string tOBJ_NOTES = string.Empty;

            try
            {
                initSapConnector();

                RfcFunctionMetadata ZFM_CONTRACT_GETALL_INFO = sapConnector.Repository.GetFunctionMetadata("ZFM_CONTRACT_GETALL_INFO");
                IRfcFunction function = ZFM_CONTRACT_GETALL_INFO.CreateFunction();

                function.SetValue("IV_CONTRACTID", beanIN.IV_CONTRACTID.Trim());
                function.Invoke(sapConnector);

                DataTable dtOBJ = CMF.SetRFCDataTable(function, "LT_CONTRACT_OBJ");

                if (dtOBJ.Rows.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無合約標的資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得合約標的資料List
                    List<CONTRACTOBJINFO_LIST> tObjList = new List<CONTRACTOBJINFO_LIST>();

                    foreach (DataRow dr in dtOBJ.Rows)
                    {
                        CONTRACTOBJINFO_LIST beanCust = new CONTRACTOBJINFO_LIST();

                        tOBJ_NOTES = dr["NOTE"].ToString().Replace("\r\n", "<br/>").Replace("\n", "<br/>");

                        beanCust.CONTRACTID = beanIN.IV_CONTRACTID.Trim();             //主約文件編號
                        beanCust.HOSTNAME = dr["HOSTNAME"].ToString();                //HostName
                        beanCust.SN = dr["SN"].ToString();                          //序號
                        beanCust.BRANDS = dr["BRANDS"].ToString();                   //廠牌
                        beanCust.MODEL = dr["MODEL"].ToString();                     //ProductModel
                        beanCust.LOCATION = dr["LOCATION"].ToString();                //Location
                        beanCust.PLACE = dr["PLACE"].ToString();                     //地點
                        beanCust.AREA = dr["AREA"].ToString();                       //區域
                        beanCust.RESPONSE_LEVEL = dr["RESPONSE_LEVEL"].ToString();     //回應條件
                        beanCust.SERVICE_LEVEL = dr["SERVICE_LEVEL"].ToString();       //服務條件
                        beanCust.NOTES = tOBJ_NOTES;                                //備註
                        beanCust.SUB_CONTRACTID = dr["SUB_CONTRACTID"].ToString();     //下包文件編號

                        tObjList.Add(beanCust);
                    }

                    OUTBean.CONTRACTOBJINFO_LIST = tObjList;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "CONTRACTOBJINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢合約標的資料INPUT資訊
        /// <summary>查詢合約標的資料INPUT資訊</summary>
        public struct CONTRACTOBJINFO_INPUT
        {
            /// <summary>合約編號</summary>
            public string IV_CONTRACTID { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑查詢合約標的(暫時先不用)↑↑↑↑↑-----        

        #region -----↓↓↓↓↓更新進出貨的資料 ↓↓↓↓↓-----

        #region API更新出貨的資料        
        [HttpPost]
        public ActionResult API_STOCKOUTINFO_UPDATE(STOCKITEMINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNAME": "張豐穎 Elvis Chang",
            //    "IV_SN": "SGH33223R6",
            //    "IV_SNNEW": "",
            //    "IV_MATERIALNO": "G-654081B21-057",
            //    "IV_PID": "DL360pG8 E5-2650/8G*2/146G15K*2/D/R/IC-E",
            //    "IV_SO": "1230021104",
            //    "IV_CUSTOMERID": "D16151427",           
            //    "IV_IO": "O"
            //}
            #endregion

            STOCKITEMINFO_OUTPUT ListOUT = new STOCKITEMINFO_OUTPUT();

            ListOUT = STOCKOUTINFO_UPDATE(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 更新出貨資料
        /// <summary>
        /// 更新出貨資料
        /// </summary>
        /// <param name="beanIN"></param>
        /// <returns></returns>
        public STOCKITEMINFO_OUTPUT STOCKOUTINFO_UPDATE(STOCKITEMINFO_INPUT beanIN)
        {
            STOCKITEMINFO_OUTPUT OUTBean = new STOCKITEMINFO_OUTPUT();

            OUTBean = callSaveStockOUT(beanIN);

            return OUTBean;
        }
        #endregion     

        #region 更新CRM出貨檔和儲存出貨資料檔
        /// <summary>
        /// 更新CRM出貨檔和儲存出貨資料檔
        /// </summary>
        /// <param name="beanIN"></param>
        /// <returns></returns>
        public STOCKITEMINFO_OUTPUT callSaveStockOUT(STOCKITEMINFO_INPUT beanIN)
        {
            STOCKITEMINFO_OUTPUT OUTBean = new STOCKITEMINFO_OUTPUT();
            STOCKITEMINFO_OUTPUT OUTBean_rfc = new STOCKITEMINFO_OUTPUT();

            string returnMsg = "";

            bool tIsUpdateSERIAL = false;   //是否要執行更新序號

            string IV_LOGINEMPNAME = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNAME) ? "" : beanIN.IV_LOGINEMPNAME;    //登入者姓名
            string IV_SN = string.IsNullOrEmpty(beanIN.IV_SN) ? "" : beanIN.IV_SN;                                //原序號
            string IV_SNNEW = string.IsNullOrEmpty(beanIN.IV_SNNEW) ? "" : beanIN.IV_SNNEW;                        //新序號
            string IV_MATERIALNO = string.IsNullOrEmpty(beanIN.IV_MATERIALNO) ? "" : beanIN.IV_MATERIALNO;          //物料編號
            string IV_PID = string.IsNullOrEmpty(beanIN.IV_PID) ? "" : beanIN.IV_PID;                             //規格/說明
            string IV_SO = string.IsNullOrEmpty(beanIN.IV_SO) ? "" : beanIN.IV_SO;                                //銷售訂單號
            string IV_CUSTOMERID = string.IsNullOrEmpty(beanIN.IV_CUSTOMERID) ? "" : beanIN.IV_CUSTOMERID;          //客戶ID
            string IV_PO = string.IsNullOrEmpty(beanIN.IV_PO) ? "" : beanIN.IV_PO;                                //進貨號碼
            string IV_VENDERNO = string.IsNullOrEmpty(beanIN.IV_VENDERNO) ? "" : beanIN.IV_VENDERNO;               //供應商ID
            string IV_VENDERNAME = string.IsNullOrEmpty(beanIN.IV_VENDERNAME) ? "" : beanIN.IV_VENDERNAME;          //供應商名稱
            string IV_IO = string.IsNullOrEmpty(beanIN.IV_IO) ? "" : beanIN.IV_IO;                                //O.出貨 I.進貨
            string tPNUMBER = string.Empty;                                                                    //製造商零件號碼
            string tBRAND = string.Empty;                                                                      //廠牌

            try
            {
                tIsUpdateSERIAL = false;    //預設False

                #region 更新CRM出貨檔
                OUTBean_rfc = callRFCSTOCKITEMINFO_UPDATE(beanIN);
                #endregion

                if (OUTBean_rfc.EV_MSGT == "Y")
                {
                    string[] tMAList = CMF.findMATERIALPNUMBERandBRAND(IV_MATERIALNO);
                    tPNUMBER = tMAList[0];
                    tBRAND = tMAList[1];

                    #region 更新Proxy出貨檔   
                    if (IV_SNNEW != "") //要更新序號
                    {
                        #region 判斷出貨資料是否序號有重覆，沒有才可以更新
                        var bean = dbProxy.STOCKOUT.FirstOrDefault(x => x.IV_SERIAL == IV_SNNEW);

                        if (bean == null)
                        {
                            tIsUpdateSERIAL = true;
                        }
                        else
                        {
                            OUTBean.EV_MSGT = "E";
                            OUTBean.EV_MSG = "新序號【" + IV_SNNEW + "】已存在，請重新輸入！";
                        }
                        #endregion

                        #region 可更新時，先新增再刪除
                        if (tIsUpdateSERIAL)
                        {
                            #region 先新增
                            var beanOri = dbProxy.STOCKOUT.FirstOrDefault(x => x.IV_SERIAL == IV_SN);

                            STOCKOUT stockOUT = new STOCKOUT();

                            stockOUT.IV_SERIAL = IV_SNNEW;
                            stockOUT.IV_SONO = IV_SO;
                            stockOUT.IV_DNDATE = beanOri.IV_DNDATE;
                            stockOUT.IV_CID = IV_CUSTOMERID;
                            stockOUT.IV_MATERIAL = IV_MATERIALNO;
                            stockOUT.IV_DESC = IV_PID;
                            stockOUT.IV_WTYID = beanOri.IV_WTYID;
                            stockOUT.IV_WTYDESC = beanOri.IV_WTYDESC;
                            stockOUT.IV_SDATE = beanOri.IV_SDATE;
                            stockOUT.IV_EDATE = beanOri.IV_EDATE;
                            stockOUT.IV_SLASRV = beanOri.IV_SLASRV;
                            stockOUT.IV_SLARESP = beanOri.IV_SLARESP;
                            stockOUT.IV_PNUMBER = tPNUMBER;
                            stockOUT.IV_BRAND = tBRAND;
                            stockOUT.CR_USER = IV_LOGINEMPNAME;
                            stockOUT.CR_DATE = DateTime.Now;

                            dbProxy.STOCKOUT.Add(stockOUT);
                            dbProxy.SaveChanges();
                            #endregion

                            #region 再刪除
                            dbProxy.STOCKOUT.Remove(beanOri);
                            dbProxy.SaveChanges();
                            #endregion

                            #region 再更新保固檔
                            var beansWTY = dbProxy.STOCKWTY.Where(x => x.IV_SERIAL == IV_SN);

                            foreach (var beanWTY in beansWTY)
                            {
                                beanWTY.IV_SERIAL = IV_SNNEW;
                                beanWTY.IV_CID = IV_CUSTOMERID;
                                beanWTY.IV_SONO = IV_SO;
                                beanWTY.UP_USER = IV_LOGINEMPNAME;
                                beanWTY.UP_DATE = DateTime.Now;
                            }

                            dbProxy.SaveChanges();
                            #endregion

                            OUTBean.EV_MSGT = "Y";
                            OUTBean.EV_MSG = "";
                        }
                        #endregion
                    }
                    else //不更新序號
                    {
                        #region 先更新出貨
                        var beanOUT = dbProxy.STOCKOUT.FirstOrDefault(x => x.IV_SERIAL == IV_SN);

                        if (beanOUT != null)
                        {
                            if (tIsUpdateSERIAL)
                            {
                                beanOUT.IV_SERIAL = IV_SNNEW;
                            }

                            beanOUT.IV_CID = IV_CUSTOMERID;
                            beanOUT.IV_SONO = IV_SO;
                            beanOUT.IV_MATERIAL = IV_MATERIALNO;
                            beanOUT.IV_DESC = IV_PID;
                            beanOUT.IV_PNUMBER = tPNUMBER;
                            beanOUT.IV_BRAND = tBRAND;
                            beanOUT.UP_USER = IV_LOGINEMPNAME;
                            beanOUT.UP_DATE = DateTime.Now;
                        }

                        dbProxy.SaveChanges();
                        #endregion

                        #region 再更新保固檔
                        var beansWTY = dbProxy.STOCKWTY.Where(x => x.IV_SERIAL == IV_SN);

                        foreach (var beanWTY in beansWTY)
                        {
                            beanWTY.IV_CID = IV_CUSTOMERID;
                            beanWTY.IV_SONO = IV_SO;
                            beanWTY.UP_USER = IV_LOGINEMPNAME;
                            beanWTY.UP_DATE = DateTime.Now;
                        }

                        dbProxy.SaveChanges();
                        #endregion

                        OUTBean.EV_MSGT = "Y";
                        OUTBean.EV_MSG = "";
                    }
                    #endregion                  
                }
                else
                {
                    OUTBean = OUTBean_rfc;
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "原序號【" + IV_SN + "】失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "callSaveStockOUT_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 呼叫RFC更新進出貨資料
        /// <summary>
        /// 呼叫RFC更新進出貨資料
        /// </summary>
        /// <param name="beanIN">傳入的資料</param>
        /// <returns></returns>
        public STOCKITEMINFO_OUTPUT callRFCSTOCKITEMINFO_UPDATE(STOCKITEMINFO_INPUT beanIN)
        {
            STOCKITEMINFO_OUTPUT OUTBean = new STOCKITEMINFO_OUTPUT();

            string pMsg = string.Empty;

            string IV_SN = string.IsNullOrEmpty(beanIN.IV_SN) ? "" : beanIN.IV_SN;                        //原序號
            string IV_SNNEW = string.IsNullOrEmpty(beanIN.IV_SNNEW) ? "" : beanIN.IV_SNNEW;                //新序號
            string IV_MATERIALNO = string.IsNullOrEmpty(beanIN.IV_MATERIALNO) ? "" : beanIN.IV_MATERIALNO;  //物料編號
            string IV_PID = string.IsNullOrEmpty(beanIN.IV_PID) ? "" : beanIN.IV_PID;                     //規格/說明
            string IV_SO = string.IsNullOrEmpty(beanIN.IV_SO) ? "" : beanIN.IV_SO;                        //銷售訂單號
            string IV_CUSTOMERID = string.IsNullOrEmpty(beanIN.IV_CUSTOMERID) ? "" : beanIN.IV_CUSTOMERID;  //客戶ID
            string IV_PO = string.IsNullOrEmpty(beanIN.IV_PO) ? "" : beanIN.IV_PO;                        //進貨號碼
            string IV_VENDERNO = string.IsNullOrEmpty(beanIN.IV_VENDERNO) ? "" : beanIN.IV_VENDERNO;        //供應商ID
            string IV_VENDERNAME = string.IsNullOrEmpty(beanIN.IV_VENDERNAME) ? "" : beanIN.IV_VENDERNAME;  //供應商名稱
            string IV_IO = string.IsNullOrEmpty(beanIN.IV_IO) ? "" : beanIN.IV_IO;                        //O.出貨 I.進貨

            try
            {
                initSapConnector();

                RfcFunctionMetadata ZFM_UPDATE_PROD_IN_OUT_INFO = sapConnector.Repository.GetFunctionMetadata("ZFM_UPDATE_PROD_IN_OUT_INFO");
                IRfcFunction function = ZFM_UPDATE_PROD_IN_OUT_INFO.CreateFunction();

                function.SetValue("SN", IV_SN);
                function.SetValue("SN_NEW", IV_SNNEW);
                function.SetValue("MATERIAL_NO", IV_MATERIALNO);
                function.SetValue("PID", IV_PID);
                function.SetValue("SO", IV_SO);
                function.SetValue("CUSTOMER_ID", IV_CUSTOMERID);
                function.SetValue("PO", IV_PO);
                function.SetValue("VENDER_NO", IV_VENDERNO);
                function.SetValue("VENDER_NAME", IV_VENDERNAME);
                function.Invoke(sapConnector);

                if (function.GetString("EV_MSG").ToString().Trim() == "E")
                {
                    pMsg += "序號：" + IV_SN + "類型：" + IV_IO + Environment.NewLine;
                    pMsg += "E,更新進出貨資料接口失敗：" + Environment.NewLine;

                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "callRFCSTOCKITEMINFO_UPDATE_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 更新進出貨資料INPUT資訊
        /// <summary>更新進出貨資料INPUT資訊</summary>
        public struct STOCKITEMINFO_INPUT
        {
            /// <summary>登入者姓名</summary>
            public string IV_LOGINEMPNAME { get; set; }
            /// <summary>原序號</summary>
            public string IV_SN { get; set; }
            /// <summary>新序號</summary>
            public string IV_SNNEW { get; set; }
            /// <summary>物料編號</summary>
            public string IV_MATERIALNO { get; set; }
            /// <summary>規格/說明</summary>
            public string IV_PID { get; set; }
            /// <summary>銷售訂單號</summary>
            public string IV_SO { get; set; }
            /// <summary>客戶ID</summary>
            public string IV_CUSTOMERID { get; set; }
            /// <summary>進貨號碼</summary>
            public string IV_PO { get; set; }
            /// <summary>供應商ID</summary>
            public string IV_VENDERNO { get; set; }
            /// <summary>供應商名稱</summary>
            public string IV_VENDERNAME { get; set; }
            /// <summary>O.出貨 I.進貨</summary>
            public string IV_IO { get; set; }
        }
        #endregion

        #region 更新進出貨資料OUTPUT資訊
        /// <summary>更新進出貨資料OUTPUT資訊</summary>
        public struct STOCKITEMINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑更新進出貨的資料 ↑↑↑↑↑-----

        #region -----↓↓↓↓↓查詢現行CRM合約主數據四個相關Table ↓↓↓↓↓-----

        #region 查詢現行合約主數據四個相關Table資料
        [HttpPost]
        public ActionResult API_CRMCONTRACTINFO_GET(CRMCONTRACTINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_CONTRACTID": "11204075"
            //}
            #endregion

            CRMCONTRACTINFO_OUTPUT ListOUT = new CRMCONTRACTINFO_OUTPUT();

            ListOUT = CRMCONTRACTINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得現行合約主數據四個相關Table的資料
        private CRMCONTRACTINFO_OUTPUT CRMCONTRACTINFO_GET(CRMCONTRACTINFO_INPUT beanIN)
        {
            CRMCONTRACTINFO_OUTPUT OUTBean = new CRMCONTRACTINFO_OUTPUT();

            DataTable dtMAIN = null;
            DataTable dtSUB = null;
            DataTable dtOBJ = null;
            DataTable dtENG = null;

            string pContractID = string.Empty;
            string cStartDate = string.Empty;
            string cEndDate = string.Empty;

            try
            {
                #region 註解
                initSapConnector();

                var beans = dbOne.TB_ONE_ContractIDTemp.ToList();

                foreach (var bean in beans)
                {
                    pContractID = bean.cContractID;

                    RfcFunctionMetadata ZFM_CONTRACT_GETALL_INFO = sapConnector.Repository.GetFunctionMetadata("ZFM_CONTRACT_GETALL_INFO");
                    IRfcFunction function = ZFM_CONTRACT_GETALL_INFO.CreateFunction();

                    function.SetValue("IV_CONTRACTID", pContractID);
                    function.Invoke(sapConnector);

                    dtMAIN = CMF.SetRFCDataTable(function, "LT_CONTRACT_MAIN");
                    dtSUB = CMF.SetRFCDataTable(function, "LT_CONTRACT_SUB");
                    dtOBJ = CMF.SetRFCDataTable(function, "LT_CONTRACT_OBJ");
                    dtENG = CMF.SetRFCDataTable(function, "LT_CONTRACT_ENG");

                    if (dtMAIN.Rows.Count == 0)
                    {
                        OUTBean.EV_MSGT = "E";
                        OUTBean.EV_MSG = "查無現行合約主數據四個相關Table資料，請重新查詢！";
                    }
                    else
                    {
                        OUTBean.EV_MSGT = "Y";
                        OUTBean.EV_MSG = "";

                        #region 寫入合約主數據4個Table

                        //try
                        //{
                        //    #region 合約主數據
                        //    if (dtMAIN.Rows.Count > 0)
                        //    {
                        //        string cContractID = pContractID;                                        //文件編號                        
                        //        string cSoNo = dtMAIN.Rows[0]["SONUMBER"].ToString();                                     //銷售單號                        
                        //        string cSoSales = dtMAIN.Rows[0]["SALES"].ToString().TrimStart('0').Trim();               //業務ERPID
                        //        string cSoSalesName = CMF.findEmployeeNameInCludeLeave(cSoSales);                      //業務                        
                        //        string cSoSalesASS = dtMAIN.Rows[0]["SALES_ASS"].ToString().TrimStart('0').Trim();        //業務祕書ERPID
                        //        string cSoSalesASSName = CMF.findEmployeeNameInCludeLeave(cSoSalesASS);                //業務祕書
                        //        string cMASales = dtMAIN.Rows[0]["MAINTAIN_SALES"].ToString().TrimStart('0').Trim();      //維護業務ERPID
                        //        string cMASalesName = CMF.findEmployeeNameInCludeLeave(cMASales);                     //維護業務                            
                        //        string cCustomerID = dtMAIN.Rows[0]["CUSTOMER_NUMBER"].ToString();                       //CRM客戶ID
                        //        string cCustomerName = CMF.findCustName(cCustomerID);                                 //CRM客戶
                        //        string cDesc = dtMAIN.Rows[0]["SO_DESC"].ToString();                                    //訂單說明                                                        
                        //        string cMACycle = dtMAIN.Rows[0]["PM_CYCLE"].ToString();                                //維護週期
                        //        string cMANotes = dtMAIN.Rows[0]["PM_NOTES"].ToString();                                //維護備註
                        //        string cMAAddress = dtMAIN.Rows[0]["PLACE"].ToString();                                //維護地址
                        //        string cSLARESP = dtMAIN.Rows[0]["RESPONSE_LEVEL"].ToString();                          //回應條件
                        //        string cSLASRV = dtMAIN.Rows[0]["SERVICE_LEVEL"].ToString();                            //服務條件
                        //        string cContractNotes = dtMAIN.Rows[0]["CONTRACT_NOTES"].ToString();                    //合約備註
                        //        string cContractReport = dtMAIN.Rows[0]["URL_LINK"].ToString();                        //合約書link
                        //        string cTeamID = dtMAIN.Rows[0]["ORG_CODE"].ToString().TrimStart('0').Trim();           //服務組織
                        //        string cIsSubContract = dtMAIN.Rows[0]["SUB_FLAG"].ToString() == "X" ? "Y" : "N";       //是否為下包合約
                        //        string cBillCycle = dtMAIN.Rows[0]["BILLABLE_TIME"].ToString();                        //請款期間
                        //        string cBillNotes = dtMAIN.Rows[0]["PAY_NOTE"].ToString();                            //請款備註

                        //        if (string.IsNullOrEmpty(dtMAIN.Rows[0]["START_DATE"].ToString()) || dtMAIN.Rows[0]["START_DATE"].ToString() == "0000-00-00")
                        //        {
                        //            cStartDate = "";
                        //        }
                        //        else
                        //        {
                        //            cStartDate = dtMAIN.Rows[0]["START_DATE"].ToString() + " 00:00:00";               //維護起始日期
                        //        }

                        //        if (string.IsNullOrEmpty(dtMAIN.Rows[0]["FINISH_DATE"].ToString()) || dtMAIN.Rows[0]["FINISH_DATE"].ToString() == "0000-00-00")
                        //        {
                        //            cEndDate = "";
                        //        }
                        //        else
                        //        {
                        //            cEndDate = dtMAIN.Rows[0]["FINISH_DATE"].ToString() + " 00:00:00";               //維護結束日期
                        //        }

                        //        #region 寫入Table
                        //        TB_ONE_ContractMain Main = new TB_ONE_ContractMain();

                        //        Main.cContractID = cContractID;
                        //        Main.cSoNo = cSoNo;
                        //        Main.cSoSales = cSoSales;
                        //        Main.cSoSalesName = cSoSalesName;
                        //        Main.cSoSalesASS = cSoSalesASS;
                        //        Main.cSoSalesASSName = cSoSalesASSName;
                        //        Main.cMASales = cMASales;
                        //        Main.cMASalesName = cMASalesName;
                        //        Main.cCustomerID = cCustomerID;
                        //        Main.cCustomerName = cCustomerName;
                        //        Main.cDesc = cDesc;

                        //        if (cStartDate != "")
                        //        {
                        //            Main.cStartDate = Convert.ToDateTime(cStartDate);
                        //            Main.cEndDate = Convert.ToDateTime(cEndDate);
                        //        }

                        //        Main.cMACycle = cMACycle;
                        //        Main.cMANotes = cMANotes;
                        //        Main.cMAAddress = cMAAddress;
                        //        Main.cSLARESP = cSLARESP;
                        //        Main.cSLASRV = cSLASRV;
                        //        Main.cContractNotes = cContractNotes;
                        //        Main.cContractReport = cContractReport;
                        //        Main.cTeamID = cTeamID;
                        //        Main.cIsSubContract = cIsSubContract;
                        //        Main.cBillCycle = cBillCycle;
                        //        Main.cBillNotes = cBillNotes;
                        //        Main.cInvalidReason = "";
                        //        Main.Disabled = 0;

                        //        dbOne.TB_ONE_ContractMain.Add(Main);
                        //        dbOne.SaveChanges();
                        //        #endregion
                        //    }
                        //    #endregion

                        //    #region 下包合約
                        //    if (dtSUB.Rows.Count > 0)
                        //    {
                        //        foreach (DataRow dr in dtSUB.Rows)
                        //        {
                        //            string cContractID = pContractID;                                                  //主約文件編號
                        //            string cSubContractID = dr["SUB_CONTRACTID"].ToString();                                           //下包文件編號
                        //            string cSubSupplierID = dr["SUBCONTRACT_BUSINESS_NUMBER"].ToString();                               //下包商ID
                        //            string cSubSupplierName = dr["SUBCONTRACT_NAME"].ToString();                                       //下包商名稱
                        //            string cSubNotes = dr["SUBCONTRACT_NOTES"].ToString();                                             //下包備註                                

                        //            #region 寫入Table
                        //            TB_ONE_ContractDetail_SUB DSub = new TB_ONE_ContractDetail_SUB();

                        //            DSub.cContractID = cContractID;
                        //            DSub.cSubContractID = cSubContractID;
                        //            DSub.cSubSupplierID = cSubSupplierID;
                        //            DSub.cSubSupplierName = cSubSupplierName;
                        //            DSub.cSubNotes = cSubNotes;
                        //            DSub.Disabled = 0;

                        //            dbOne.TB_ONE_ContractDetail_SUB.Add(DSub);
                        //            #endregion
                        //        }

                        //        dbOne.SaveChanges();
                        //    }
                        //    #endregion

                        //    #region 標的
                        //    if (dtOBJ.Rows.Count > 0)
                        //    {
                        //        foreach (DataRow dr in dtOBJ.Rows)
                        //        {
                        //            string cContractID = pContractID;            //主約文件編號
                        //            string cHostName = dr["HOSTNAME"].ToString();                //HostName
                        //            string cSerialID = dr["SN"].ToString();                     //序號
                        //            string cPID = dr["PID"].ToString();                         //PID
                        //            string cBrands = dr["BRANDS"].ToString();                   //廠牌
                        //            string cModel = dr["MODEL"].ToString();                     //ProductModel
                        //            string cLocation = dr["LOCATION"].ToString();               //Location
                        //            string cAddress = dr["PLACE"].ToString();                   //地點
                        //            string cArea = dr["AREA"].ToString();                       //區域
                        //            string cSLARESP = dr["RESPONSE_LEVEL"].ToString();           //回應條件
                        //            string cSLASRV = dr["SERVICE_LEVEL"].ToString();            //服務條件
                        //            string cNotes = dr["NOTE"].ToString();                     //備註
                        //            string cSubContractID = dr["SUB_CONTRACTID"].ToString();    //下包文件編號

                        //            #region 寫入Table
                        //            TB_ONE_ContractDetail_OBJ DObj = new TB_ONE_ContractDetail_OBJ();

                        //            DObj.cContractID = cContractID;
                        //            DObj.cHostName = cHostName;
                        //            DObj.cSerialID = cSerialID;
                        //            DObj.cPID = cPID;
                        //            DObj.cBrands = cBrands;
                        //            DObj.cModel = cModel;
                        //            DObj.cLocation = cLocation;
                        //            DObj.cAddress = cAddress;
                        //            DObj.cArea = cArea;
                        //            DObj.cSLARESP = cSLARESP;
                        //            DObj.cSLASRV = cSLASRV;
                        //            DObj.cNotes = cNotes;
                        //            DObj.cSubContractID = cSubContractID;
                        //            DObj.Disabled = 0;

                        //            dbOne.TB_ONE_ContractDetail_OBJ.Add(DObj);
                        //            #endregion
                        //        }

                        //        dbOne.SaveChanges();
                        //    }
                        //    #endregion

                        //    #region 工程師
                        //    if (dtENG.Rows.Count > 0)
                        //    {
                        //        foreach (DataRow dr in dtENG.Rows)
                        //        {
                        //            string cContractID = pContractID;                        //主約文件編號
                        //            string cEngineerID = dr["ENGINEER"].ToString().TrimStart('0').Trim();     //工程師ERPID
                        //            string cEngineerName = CMF.findEmployeeNameInCludeLeave(cEngineerID);   //工程師姓名
                        //            string cIsMainEngineer = dr["MAIN_FLAG"].ToString() == "X" ? "Y" : "N"; ;   //是否為主要工程師

                        //            #region 寫入Table
                        //            TB_ONE_ContractDetail_ENG DEng = new TB_ONE_ContractDetail_ENG();

                        //            DEng.cContractID = cContractID;
                        //            DEng.cEngineerID = cEngineerID;
                        //            DEng.cEngineerName = cEngineerName;
                        //            DEng.cIsMainEngineer = cIsMainEngineer;
                        //            DEng.Disabled = 0;

                        //            dbOne.TB_ONE_ContractDetail_ENG.Add(DEng);
                        //            #endregion
                        //        }

                        //        dbOne.SaveChanges();
                        //    }
                        //    #endregion
                        //}
                        //catch (Exception ex)
                        //{
                        //    pMsg += "文件編號【" + pContractID + "】，失敗原因：" + ex.Message + Environment.NewLine;
                        //}
                        #endregion
                    }
                }
                #endregion

                #region 寫入客戶聯絡人暫存檔
                //var beansM = dbProxy.CUSTOMER_ContactTemp;

                //foreach (var bean in beansM)
                //{
                //    CUSTOMER_Contact ConTemp = new CUSTOMER_Contact();

                //    ConTemp.ContactID = Guid.NewGuid();
                //    ConTemp.KNA1_KUNNR = bean.KNA1_KUNNR;
                //    ConTemp.KNA1_NAME1 = bean.KNA1_NAME1;
                //    ConTemp.KNB1_BUKRS = "T016";
                //    ConTemp.ContactType = "5";
                //    ConTemp.ContactName = bean.ContactName;
                //    ConTemp.ContactCity = bean.ContactCity;
                //    ConTemp.ContactAddress = bean.ContactAddress;
                //    ConTemp.ContactEmail = bean.ContactEmail;
                //    ConTemp.ContactPhone = bean.ContactPhone;
                //    ConTemp.ContactMobile = bean.ContactMobile;
                //    ConTemp.BpmNo = "GenerallySR";
                //    ConTemp.ModifiedUserName = "SYS";
                //    ConTemp.ModifiedDate = DateTime.Now;
                //    ConTemp.Disabled = 0;

                //    dbProxy.CUSTOMER_Contact.Add(ConTemp);
                //}

                //int result = dbProxy.SaveChanges();

                //if (result <= 0)
                //{
                //    pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新增CUSTOMER_ContactTEMP失敗！" + Environment.NewLine;
                //    CMF.writeToLog("", "CRMCONTACTINFO_GET_API", pMsg, "SYS");

                //    OUTBean.EV_MSGT = "E";
                //    OUTBean.EV_MSG = pMsg;
                //}
                //else
                //{
                //    OUTBean.EV_MSGT = "Y";
                //    OUTBean.EV_MSG = "";
                //}
                #endregion

                if (pMsg != "")
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = pMsg;
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "CRMCONTRACTINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢現行合約主數據四個相關Table資料INPUT資訊
        /// <summary>查詢現行合約主數據四個相關TableINPUT資訊</summary>
        public struct CRMCONTRACTINFO_INPUT
        {
            /// <summary>文件編號</summary>
            public string IV_CONTRACTID { get; set; }
        }
        #endregion

        #region 查詢現行合約主數據四個相關Table資料OUTPUT資訊
        /// <summary>查詢現行合約主數據四個相關Table資料OUTPUT資訊</summary>
        public struct CRMCONTRACTINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑查詢現行CRM合約主數據四個相關Table ↑↑↑↑↑-----

        #region -----↓↓↓↓↓查詢現行CRM客戶聯絡人 ↓↓↓↓↓-----

        #region 查詢現行CRM客戶聯絡人資料
        [HttpPost]
        public ActionResult API_CRMCONTACTINFO_GET(CRMCONTACTINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_CUSTOMEID": "D16151427"
            //}
            #endregion

            CRMCONTACTINFO_OUTPUT ListOUT = new CRMCONTACTINFO_OUTPUT();

            ListOUT = CRMCONTACTINFO_GET(beanIN);

            return Json(ListOUT);
        }
        #endregion

        #region 取得現行CRM客戶聯絡人的資料
        private CRMCONTACTINFO_OUTPUT CRMCONTACTINFO_GET(CRMCONTACTINFO_INPUT beanIN)
        {
            CRMCONTACTINFO_OUTPUT OUTBean = new CRMCONTACTINFO_OUTPUT();

            List<string> tTempList = new List<string>();
            string tOBJ_NOTES = string.Empty;
            string tTempValue = string.Empty;

            try
            {
                initSapConnector();

                RfcFunctionMetadata ZFM_CONTRACT_GETALL_INFO = sapConnector.Repository.GetFunctionMetadata("ZFM_TICC_CONTACT_GET");
                IRfcFunction function = ZFM_CONTRACT_GETALL_INFO.CreateFunction();

                function.SetValue("IV_BPNUMBER", beanIN.IV_CUSTOMEID.Trim());
                function.Invoke(sapConnector);

                DataTable dtOBJ = CMF.SetRFCDataTable(function, "ET_CONTACT");

                if (dtOBJ.Rows.Count == 0)
                {
                    OUTBean.EV_MSGT = "E";
                    OUTBean.EV_MSG = "查無現行CRM客戶聯絡人資料，請重新查詢！";
                }
                else
                {
                    OUTBean.EV_MSGT = "Y";
                    OUTBean.EV_MSG = "";

                    #region 取得現行CRM客戶聯絡人資料List
                    List<CRMCONTACTINFO_LIST> tObjList = new List<CRMCONTACTINFO_LIST>();

                    foreach (DataRow dr in dtOBJ.Rows)
                    {
                        if (dr["BPNUMBER"].ToString().Trim() != "" &&
                            (dr["LASTNAME"].ToString().Trim() != "" && !dr["LASTNAME"].ToString().Trim().Contains("停用")) &&
                            dr["STREET"].ToString().Trim() != "" && dr["CITY"].ToString().Trim() != "" &&
                            (dr["TEL"].ToString().Trim() != "" || dr["MOB"].ToString().Trim() != ""))
                        {
                            tTempValue = dr["BPNUMBER"].ToString().Trim() + "|" + dr["LASTNAME"].ToString().Trim();

                            if (!tTempList.Contains(tTempValue)) //判斷客戶ID、公司別、聯絡人姓名、聯絡門市不重覆才要顯示
                            {
                                tTempList.Add(tTempValue);

                                CRMCONTACTINFO_LIST beanCust = new CRMCONTACTINFO_LIST();

                                beanCust.BPNUMBER = dr["BPNUMBER"].ToString().Trim();  //客戶代號
                                beanCust.CUSTOMER = dr["CUSTOMER"].ToString().Trim();  //客戶名稱
                                beanCust.LASTNAME = dr["LASTNAME"].ToString().Trim();  //姓名
                                beanCust.TEL = dr["TEL"].ToString().Trim();           //電話
                                beanCust.MOB = dr["MOB"].ToString().Trim();           //手機
                                beanCust.EMAIL = dr["EMAIL"].ToString().Trim();       //Email
                                beanCust.STREET = dr["STREET"].ToString().Trim();     //街道地址
                                beanCust.CITY = dr["CITY"].ToString().Trim();         //城市                        

                                tObjList.Add(beanCust);
                            }
                        }
                    }

                    OUTBean.CRMCONTACTINFO_LIST = tObjList;
                    #endregion                    
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog("", "CRMCONTACTINFO_GET_API", pMsg, "SYS");

                OUTBean.EV_MSGT = "E";
                OUTBean.EV_MSG = ex.Message;
            }

            return OUTBean;
        }
        #endregion

        #region 查詢現行CRM客戶聯絡人資料INPUT資訊
        /// <summary>查詢現行CRM客戶聯絡人INPUT資訊</summary>
        public struct CRMCONTACTINFO_INPUT
        {
            /// <summary>客戶代號</summary>
            public string IV_CUSTOMEID { get; set; }
        }
        #endregion

        #region 查詢現行CRM客戶聯絡人資料OUTPUT資訊
        /// <summary>查詢現行CRM客戶聯絡人資料OUTPUT資訊</summary>
        public struct CRMCONTACTINFO_OUTPUT
        {
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>現行CRM客戶聯絡人資料清單</summary>
            public List<CRMCONTACTINFO_LIST> CRMCONTACTINFO_LIST { get; set; }
        }

        public struct CRMCONTACTINFO_LIST
        {
            /// <summary>法人客戶編號</summary>
            public string BPNUMBER;
            /// <summary>法人客戶名稱</summary>
            public string CUSTOMER;
            /// <summary>姓名</summary>
            public string LASTNAME;
            /// <summary>電話</summary>
            public string TEL;
            /// <summary>手機</summary>
            public string MOB;
            /// <summary>Email</summary>
            public string EMAIL;
            /// <summary>街道地址</summary>
            public string STREET;
            /// <summary>城市</summary>
            public string CITY;
        }
        #endregion

        #endregion -----↑↑↑↑↑查詢現行CRM客戶聯絡人 ↑↑↑↑↑-----

        #endregion -----↑↑↑↑↑CALL RFC接口 ↑↑↑↑↑-----        
    }

    #region 取得系統位址參數相關資訊
    public class SRSYSPARAINFO
    {
        /// <summary>呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)</summary>
        public bool IsFormal { get; set; }
        /// <summary>One Service URL</summary>
        public string ONEURLName { get; set; }
        /// <summary>BPM URL</summary>
        public string BPMURLName { get; set; }
        /// <summary>PSIP URL</summary>
        public string PSIPURLName { get; set; }
        /// <summary>附件URL</summary>
        public string AttachURLName { get; set; }
    }
    #endregion

    #region 取得附件相關資訊
    public class SRATTACHINFO
    {
        /// <summary>附件GUID</summary>
        public string ID { get; set; }
        /// <summary>附件原始檔名(含副檔名)</summary>
        public string FILE_ORG_NAME { get; set; }
        /// <summary>附件檔名(GUID，含副檔名)</summary>
        public string FILE_NAME { get; set; }
        /// <summary>副檔名</summary>
        public string FILE_EXT { get; set; }
        /// <summary>附件檔案路徑URL</summary>
        public string FILE_URL { get; set; }
        /// <summary>新增日期</summary>
        public string INSERT_TIME { get; set; }
    }
    #endregion

    #region 人員資訊相關
    public struct EmployeeBean
    {
        /// <summary>人員帳號</summary>
        public string EmployeeNO { get; set; }
        /// <summary>ERPID</summary>
        public string EmployeeERPID { get; set; }
        /// <summary>中文姓名</summary>
        public string EmployeeCName { get; set; }
        /// <summary>英文姓名</summary>
        public string EmployeeEName { get; set; }
        /// <summary>工作地點</summary>
        public string WorkPlace { get; set; }
        /// <summary>分機</summary>
        public string PhoneExt { get; set; }
        /// <summary>公司別(Comp-1、Comp-2、Comp-3、Comp-4)</summary>
        public string CompanyCode { get; set; }
        /// <summary>工廠別(T012、T016、C069、T022_</summary>
        public string BUKRS { get; set; }
        /// <summary>部門代號</summary>
        public string DepartmentNO { get; set; }
        /// <summary>部門名稱</summary>
        public string DepartmentName { get; set; }
        /// <summary>利潤中心</summary>
        public string ProfitCenterID { get; set; }
        /// <summary>成本中心</summary>
        public string CostCenterID { get; set; }
        /// <summary>人員Email</summary>
        public string EmployeeEmail { get; set; }
        /// <summary>人員ID(Person資料表ID)</summary>
        public string EmployeePersonID { get; set; }
        /// <summary>是否為主管(true.是 false.否)</summary>
        public bool IsManager { get; set; }
    }
    #endregion

    #region 服務待辦清單資訊
    public class SRTODOLISTINFO
    {
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>服務案件說明</summary>
        public string SRDESC { get; set; }
        /// <summary>客戶</summary>
        public string CUSTOMERNAME { get; set; }
        /// <summary>客戶報修人</summary>
        public string REPAIRNAME { get; set; }
        /// <summary>客戶聯絡人</summary>
        public string CONTACTNAME { get; set; }
        /// <summary>報修管道</summary>
        public string PATHWAY { get; set; }
        /// <summary>報修類別</summary>
        public string SRTYPE { get; set; }
        /// <summary>主要工程師</summary>
        public string MAINENGNAME { get; set; }
        /// <summary>協助工程師</summary>
        public string ASSENGNAME { get; set; }
        /// <summary>技術主管</summary>
        public string TECHMANAGER { get; set; }
        /// <summary>回應條件</summary>
        public string SLARESP { get; set; }
        /// <summary>服務條件</summary>
        public string SLASRV { get; set; }
        /// <summary>最後編輯日期</summary>
        public string MODIFDATE { get; set; }
        /// <summary>狀態</summary>
        public string STATUSDESC { get; set; }
    }
    #endregion

    #region 保固SLA資訊
    /// <summary>保固SLA資訊</summary>
    public class SRWarranty
    {
        /// <summary>序號</summary>
        public string SERIALID { get; set; }
        /// <summary>保固代號</summary>
        public string WTYID { get; set; }
        /// <summary>保固說明</summary>
        public string WTYName { get; set; }
        /// <summary>保固開始日期</summary>
        public string WTYSDATE { get; set; }
        /// <summary>保固結束日期</summary>
        public string WTYEDATE { get; set; }
        /// <summary>回應條件</summary>
        public string SLARESP { get; set; }
        /// <summary>服務條件</summary>
        public string SLASRV { get; set; }
        /// <summary>合約編號</summary>
        public string CONTRACTID { get; set; }
        /// <summary>合約編號Url</summary>
        public string CONTRACTIDUrl { get; set; }
        /// <summary>下包文件編號</summary>
        public string SUBCONTRACTID { get; set; }
        /// <summary>保固申請(BPM表單編號)</summary>
        public string BPMFormNo { get; set; }
        /// <summary>保固申請Url(BPM表單編號Url)</summary>
        public string BPMFormNoUrl { get; set; }
        /// <summary>客服主管建議</summary>
        public string ADVICE { get; set; }
        /// <summary>本次使用</summary>
        public string USED { get; set; }
        /// <summary>tr背景顏色Class</summary>
        public string BGColor { get; set; }
    }
    #endregion

    #region 服務案件主檔資訊
    /// <summary>服務案件主檔資訊</summary>
    public class SRIDINFO
    {
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>服務案件說明</summary>
        public string SRDESC { get; set; }
        /// <summary>服務案件開單日期</summary>
        public string SRDATE { get; set; }
        /// <summary>SR類型代號</summary>
        public string SRTYPE { get; set; }
        /// <summary>SR類型描述</summary>
        public string SRTDESC { get; set; }
        /// <summary>狀態ID</summary>
        public string STATUS { get; set; }
        /// <summary>狀態說明</summary>
        public string STATUSDESC { get; set; }
        /// <summary>服務報告書URL</summary>
        public string SRREPORTUrl { get; set; }
        /// <summary>主要工程師ERPID</summary>
        public string MAINENGID { get; set; }
        /// <summary>主要工程師姓名</summary>
        public string MAINENGNAME { get; set; }
        /// <summary>協助工程師姓名</summary>
        public string ASSENGNAME { get; set; }
        /// <summary>技術主管姓名</summary>
        public string TECHMAGNAME { get; set; }
    }
    #endregion

    #region 服務案件主檔資訊(For Mail)
    /// <summary>服務案件主檔資訊(For Mail)</summary>
    public class SRIDMAININFO
    {
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>狀態ID</summary>
        public string Status { get; set; }
        /// <summary>狀態說明(E0001.新建、E0002.L2處理中、E0003.報價中、E0004.3rd Party處理中、E0005.L3處理中、E0006.完修、E0012.HPGCSN 申請、E0013.HPGCSN 完成、E0014.駁回、E0015.取消 )</summary>
        public string StatusDesc { get; set; }
        /// <summary>服務案件種類</summary>
        public string SRCase { get; set; }
        /// <summary>公司名稱</summary>
        public string CompanyName { get; set; }
        /// <summary>服務團隊</summary>
        public string TeamNAME { get; set; }
        /// <summary>服務團隊主管</summary>
        public string TeamMGR { get; set; }
        /// <summary>派單人員</summary>
        public string CreateUser { get; set; }
        /// <summary>派單時間</summary>
        public string CreatedDate { get; set; }
        /// <summary>異動人員</summary>
        public string ModifiedUser { get; set; }
        /// <summary>異動時間</summary>
        public string ModifiedDate { get; set; }
        /// <summary>主要工程師</summary>
        public string MainENG { get; set; }
        /// <summary>協助工程師</summary>
        public string AssENG { get; set; }
        /// <summary>技術主管</summary>
        public string TechMGR { get; set; }
        /// <summary>業務人員</summary>
        public string SalesEMP { get; set; }
        /// <summary>業務祕書</summary>
        public string SecretaryEMP { get; set; }        
        /// <summary>合約文件編號</summary>
        public string ContractID { get; set; }
        /// <summary>維護服務種類</summary>
        public string MAServiceType { get; set; }
        /// <summary>是否為二修</summary>
        public string SecFix { get; set; }
        /// <summary>是否為內部作業</summary>
        public string InternalWork { get; set; }
        /// <summary>報修管道</summary>
        public string SRPathWay { get; set; }
        /// <summary>銷售訂單號</summary>
        public string SalesNo { get; set; }
        /// <summary>出貨單號</summary>
        public string ShipmentNo { get; set; }
        /// <summary>需求說明</summary>
        public string Desc { get; set; }
        /// <summary>詳細描述</summary>
        public string Notes { get; set; }
        /// <summary>客戶名稱</summary>
        public string CusName { get; set; }
        /// <summary>報修人</summary>
        public string RepairName { get; set; }
        /// <summary>報修人電話</summary>
        public string RepairPhone { get; set; }
        /// <summary>報修人手機</summary>
        public string RepairMobile { get; set; }
        /// <summary>報修人地址</summary>
        public string RepairAddress { get; set; }
        /// <summary>報修人Email</summary>
        public string RepairEmail { get; set; }
        
        /// <summary>派單人員</summary>
        public string CreateUserEmail { get; set; }
        /// <summary>異動人員</summary>
        public string ModifiedUserEmail { get; set; }
        /// <summary>服務團隊主管Email</summary>
        public string TeamMGREmail { get; set; }
        /// <summary>主要工程師Email</summary>
        public string MainENGEmail { get; set; }
        /// <summary>協助工程師Email</summary>
        public string AssENGEmail { get; set; }
        /// <summary>技術主管Email</summary>
        public string TechMGREmail { get; set; }
        /// <summary>業務人員Email</summary>
        public string SalesEmail { get; set; }
        /// <summary>業務祕書Email</summary>
        public string SecretaryEmail { get; set; }
    }
    #endregion

    #region 服務案件客戶聯絡人資訊
    /// <summary>服務案件客戶聯絡人資訊</summary>
    public class SRCONTACTINFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>聯絡人姓名</summary>
        public string CONTNAME { get; set; }
        /// <summary>聯絡人地址</summary>
        public string CONTADDR { get; set; }
        /// <summary>聯絡人電話</summary>
        public string CONTTEL { get; set; }
        /// <summary>聯絡人手機</summary>
        public string CONTMOBILE { get; set; }
        /// <summary>聯絡人信箱</summary>
        public string CONTEMAIL { get; set; }

    }
    #endregion

    #region 服務案件產品序號資訊
    /// <summary>服務案件產品序號資訊</summary>
    public class SRSERIALMATERIALINFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>序號</summary>
        public string SERIALID { get; set; }
        /// <summary>更換後序號</summary>
        public string NEWSERIALID { get; set; }
        /// <summary>物料代號</summary>
        public string PRDID { get; set; }
        /// <summary>機器型號</summary>
        public string PRDNAME { get; set; }
        /// <summary>製造商零件號碼</summary>
        public string PRDNUMBER { get; set; }
        /// <summary>裝機號碼</summary>
        public string INSTALLID { get; set; }
    }
    #endregion

    #region 服務案件保固SLA資訊
    /// <summary>服務案件保固SLA資訊</summary>
    public class SRWTSLAINFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>序號</summary>
        public string SERIALID { get; set; }
        /// <summary>保固代號</summary>
        public string WTYID { get; set; }
        /// <summary>保固說明</summary>
        public string WTYName { get; set; }
        /// <summary>保固開始日期</summary>
        public string WTYSDATE { get; set; }
        /// <summary>保固結束日期</summary>
        public string WTYEDATE { get; set; }
        /// <summary>回應條件</summary>
        public string SLARESP { get; set; }
        /// <summary>服務條件</summary>
        public string SLASRV { get; set; }
        /// <summary>合約編號</summary>
        public string CONTRACTID { get; set; }
        /// <summary>合約編號Url</summary>
        public string CONTRACTIDUrl { get; set; }
        /// <summary>下包文件編號</summary>
        public string SUBCONTRACTID { get; set; }
        /// <summary>保固申請(BPM表單編號)</summary>
        public string BPMFormNo { get; set; }
        /// <summary>保固申請Url(BPM表單編號Url)</summary>
        public string BPMFormNoUrl { get; set; }
        /// <summary>客服主管建議</summary>
        public string ADVICE { get; set; }
        /// <summary>本次使用</summary>
        public string USED { get; set; }
    }
    #endregion

    #region 服務案件處理與工時紀錄資訊
    /// <summary>服務案件處理與工時紀錄資訊</summary>
    public class SRRECORDINFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>服務工程師ERPID/技術主管ERPID</summary>
        public string ENGID { get; set; }
        /// <summary>服務工程師姓名/技術主管姓名</summary>
        public string ENGNAME { get; set; }
        /// <summary>接單時間</summary>
        public string ReceiveTime { get; set; }
        /// <summary>出發時間</summary>
        public string StartTime { get; set; }
        /// <summary>到場時間</summary>
        public string ArriveTime { get; set; }
        /// <summary>完成時間</summary>
        public string FinishTime { get; set; }
        /// <summary>工時(分鐘)</summary>
        public string WorkHours { get; set; }
        /// <summary>處理紀錄</summary>
        public string Desc { get; set; }
        /// <summary>服務報告書URL</summary>
        public string SRReportURL { get; set; }
    }
    #endregion

    #region 服務報告書資訊
    /// <summary>服務報告書資訊</summary>
    public class SRREPORTINFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>服務報告書原始檔名</summary>
        public string SRReportORG_NAME { get; set; }
        /// <summary>服務報告書檔名(GUID)</summary>
        public string SRReportNAME { get; set; }
        /// <summary>服務報告書路徑</summary>
        public string SRReportPath { get; set; }
        /// <summary>服務報告書URL</summary>
        public string SRReportURL { get; set; }
    }
    #endregion

    #region 服務案件零件更換資訊
    /// <summary>服務案件零件更換資訊</summary>
    public class SRPARTSREPALCEINFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>XC HP申請零件</summary>
        public string XCHP { get; set; }
        /// <summary>更換零件料號ID</summary>
        public string MaterialID { get; set; }
        /// <summary>料號說明</summary>
        public string MaterialName { get; set; }
        /// <summary>Old CT</summary>
        public string OldCT { get; set; }
        /// <summary>New CT</summary>
        public string NewCT { get; set; }
        /// <summary>HP CT</summary>
        public string HPCT { get; set; }
        /// <summary>New UEFI </summary>
        public string NewUEFI { get; set; }
        /// <summary>備機序號</summary>
        public string StandbySerialID { get; set; }
        /// <summary>HP Case ID</summary>
        public string HPCaseID { get; set; }
        /// <summary>到貨日 </summary>
        public string ArriveDate { get; set; }
        /// <summary>歸還日 </summary>
        public string ReturnDate { get; set; }
        /// <summary>是否有人損</summary>
        public string PersonalDamage { get; set; }
        /// <summary>備註</summary>
        public string Note { get; set; }
    }
    #endregion

    #region 服務案件物料訊息資訊
    /// <summary>服務案件物料訊息資訊</summary>
    public class SRMATERIALlNFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>物料代號</summary>
        public string MaterialID { get; set; }
        /// <summary>料號說明</summary>
        public string MaterialName { get; set; }
        /// <summary>數量</summary>
        public string Quantity { get; set; }
        /// <summary>基本內文</summary>
        public string BasicContent { get; set; }
        /// <summary>製造商零件號碼</summary>
        public string MFPNumber { get; set; }
        /// <summary>廠牌</summary>
        public string Brand { get; set; }
        /// <summary>產品階層</summary>
        public string ProductHierarchy { get; set; }
    }
    #endregion

    #region 服務案件序號回報資訊
    /// <summary>服務案件序號回報資訊</summary>
    public class SRSERIALFEEDBACKlNFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>序號</summary>
        public string SERIALID { get; set; }
        /// <summary>物料代號</summary>
        public string MaterialID { get; set; }
        /// <summary>料號說明</summary>
        public string MaterialName { get; set; }
        /// <summary>裝機Config檔URL</summary>
        public string ConfigReportURL { get; set; }
    }
    #endregion

    #region 服務團隊對照組織相關資訊
    /// <summary>服務團隊對照組織相關資訊</summary>
    public class SRTEAMORGINFO
    {
        /// <summary>服務團隊ID</summary>
        public string TEAMID { get; set; }
        /// <summary>服務團隊名稱</summary>

        public string TEAMNAME { get; set; }
        /// <summary>部門ID</summary>
        public string DEPTID { get; set; }
        /// <summary>部門名稱</summary>
        public string DEPTNAME { get; set; }
        /// <summary>部門主管ERPID</summary>
        public string DEPTMGRERPID { get; set; }
        /// <summary>部門主管帳號</summary>
        public string DEPTMGRACCOUNT { get; set; }
        /// <summary>部門主管姓名(中文+英文)</summary>
        public string DEPTMGRNAME { get; set; }
        /// <summary>部門主管Email</summary>
        public string DEPTMGREMAIL { get; set; }
    }
    #endregion

    #region 服務案件主要工程師/協助工程師/技術主管相關資訊
    /// <summary>服務案件主要工程師/協助工程師/技術主管相關資訊</summary>
    public class SREMPINFO
    {
        /// <summary>ERPID</summary>
        public string ERPID { get; set; }
        /// <summary>帳號</summary>
        public string ACCOUNT { get; set; }
        /// <summary>姓名(中文+英文)</summary>
        public string NAME { get; set; }
        /// <summary>Email</summary>
        public string EMAIL { get; set; }
    }
    #endregion

    #region 建立客戶聯絡人資訊
    /// <summary>建立客戶聯絡人資訊</summary>
    public class CREATECONTACTINFO
    {
        /// <summary>聯絡人姓名</summary>
        public string CONTNAME { get; set; }
        /// <summary>聯絡人地址</summary>
        public string CONTADDR { get; set; }
        /// <summary>聯絡人電話</summary>
        public string CONTTEL { get; set; }
        /// <summary>聯絡人手機</summary>
        public string CONTMOBILE { get; set; }
        /// <summary>聯絡人信箱</summary>
        public string CONTEMAIL { get; set; }

    }
    #endregion

    #region 建立物料訊息資訊
    /// <summary>建立物料訊息資訊</summary>
    public class CREATEMATERIAL
    {
        /// <summary>物料編號</summary>
        public string MATERIALID { get; set; }
        /// <summary>物料說明</summary>
        public string MATERIALNAME { get; set; }
        /// <summary>物料數量</summary>
        public string QTY { get; set; }
    }
    #endregion

    #region 建立序號回報資訊
    /// <summary>建立序號回報資訊</summary>
    public class CREATEFEEDBACK
    {
        /// <summary>序號</summary>
        public string SERIALID { get; set; }       
    }
    #endregion

    #region 物料相關資訊
    /// <summary>物料相關資訊</summary>
    public struct MaterialInfo
    {
        /// <summary>料號</summary>
        public string MaterialID { get; set; }
        /// <summary>料號說明</summary>
        public string MaterialName { get; set; }
        /// <summary>製造商零件號碼</summary>
        public string MFPNumber { get; set; }
        /// <summary>基本內文</summary>
        public string BasicContent { get; set; }
        /// <summary>廠牌</summary>
        public string Brand { get; set; }
        /// <summary>廠品階層</summary>
        public string ProductHierarchy { get; set; }
    }
    #endregion

    #region 客戶聯絡人資訊
    /// <summary>客戶聯絡人</summary>
    public struct PCustomerContact
    {
        /// <summary>GUID</summary>
        public string ContactID { get; set; }
        /// <summary>客戶ID</summary>
        public string CustomerID { get; set; }
        /// <summary>客戶名稱</summary>
        public string CustomerName { get; set; }
        /// <summary>公司別</summary>
        public string BUKRS { get; set; }
        /// <summary>聯絡人姓名</summary>
        public string Name { get; set; }
        /// <summary>聯絡人居住城市</summary>
        public string City { get; set; }
        /// <summary>聯絡人地址</summary>
        public string Address { get; set; }
        /// <summary>聯絡人Email</summary>
        public string Email { get; set; }
        /// <summary>聯絡人電話</summary>
        public string Phone { get; set; }
        /// <summary>聯絡人手機</summary>
        public string Mobile { get; set; }
        /// <summary>聯絡人門市ID</summary>
        public string Store { get; set; }
        /// <summary>聯絡人門市名稱</summary>
        public string StoreName { get; set; }
        /// <summary>來源表單</summary>
        public string BPMNo { get; set; }
    }
    #endregion   

    #region 關鍵字產品序號物料資訊
    /// <summary>關鍵字產品序號物料資訊</summary>
    public struct SerialMaterialInfo
    {
        /// <summary>序號</summary>
        public string IV_SERIAL { get; set; }
        /// <summary>物料代號</summary>
        public string ProdID { get; set; }
        /// <summary>機器型號</summary>
        public string Product { get; set; }
        /// <summary>製造商零件號碼</summary>
        public string MFRPN { get; set; }
        /// <summary>裝機號碼</summary>
        public string InstallNo { get; set; }
    }
    #endregion

    #region 查詢下拉清單INPUT資訊
    /// <summary>查詢下拉清單INPUT資訊</summary>
    public struct OPTION_INPUT
    {
        /// <summary>公司別ID(T012、T016、C069、T022)</summary>
        public string IV_COMPID { get; set; }
        /// <summary>服務案件種類(ZSR1.一般 ZSR3.裝機 ZSR5.定維)</summary>
        public string IV_CASETYPE { get; set; }
    }
    #endregion

    #region 查詢下拉選項清單OUTPUT資訊
    /// <summary>查詢下拉選項清單OUTPUT資訊</summary>
    public struct OPTION_OUTPUT
    {
        /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
        public string EV_MSGT { get; set; }
        /// <summary>消息內容</summary>
        public string EV_MSG { get; set; }

        /// <summary>下拉選項清單</summary>
        public List<OPTION_LIST> OPTION_LIST { get; set; }
    }

    public struct OPTION_LIST
    {
        /// <summary>ID</summary>
        public string VALUE { get; set; }
        /// <summary>名稱</summary>
        public string TEXT { get; set; }
    }
    #endregion

    #region 查詢合約標的資料OUTPUT資訊
    /// <summary>查詢合約標的資料OUTPUT資訊</summary>
    public struct CONTRACTOBJINFO_OUTPUT
    {
        /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
        public string EV_MSGT { get; set; }
        /// <summary>消息內容</summary>
        public string EV_MSG { get; set; }

        /// <summary>合約標的資料清單</summary>
        public List<CONTRACTOBJINFO_LIST> CONTRACTOBJINFO_LIST { get; set; }
    }

    public struct CONTRACTOBJINFO_LIST
    {
        /// <summary>主約文件編號</summary>
        public string CONTRACTID;
        /// <summary>HostName</summary>
        public string HOSTNAME;
        /// <summary>序號</summary>
        public string SN;
        /// <summary>廠牌</summary>
        public string BRANDS;
        /// <summary>ProductModel</summary>
        public string MODEL;
        /// <summary>Location</summary>
        public string LOCATION;
        /// <summary>地點</summary>
        public string PLACE;
        /// <summary>區域</summary>
        public string AREA;
        /// <summary>回應條件</summary>
        public string RESPONSE_LEVEL;
        /// <summary>服務條件</summary>
        public string SERVICE_LEVEL;
        /// <summary>備註</summary>
        public string NOTES;
        /// <summary>下包文件編號</summary>
        public string SUB_CONTRACTID;
    }
    #endregion

    #region 客戶報修進度清單
    public struct PROGRESS_LIST
    {
        /// <summary>報修單號</summary>
        public string SRID { get; set; }
        /// <summary>客戶名稱</summary>
        public string CUSTOMERNAME { get; set; }
        /// <summary>報修人姓名</summary>
        public string REPAIRNAME { get; set; }
        /// <summary>報修時間</summary>
        public string SRDATE { get; set; }
        /// <summary>機器明細</summary>
        public string PRODUCT { get; set; }
        /// <summary>問題描述</summary>
        public string DESC { get; set; }
        /// <summary>狀態</summary>
        public string STATUS { get; set; }
    }
    #endregion

    #region 料號和料號說明
    public struct MATERIAL_LIST
    {
        /// <summary>序號</summary>
        public string EV_SERIAL { get; set; }
        /// <summary>料號</summary>
        public string EV_PRDID { get; set; }
        /// <summary>料號說明</summary>
        public string EV_PRDNAME { get; set; }
    }
    #endregion

    #region -- struct SNLIST、ENGProcessLIST、XCLIST --
    public struct SNLIST
    {
        public string SNNO { get; set; }
        public string PRDID { get; set; }
        public string PRDNUMBER { get; set; }
    }

    public struct ENGProcessLIST
    {
        public string ENGID { get; set; }
        public string ENGNAME { get; set; }
        public string ENGEMAIL { get; set; }
    }

    public struct XCLIST
    {
        public string HPXC { get; set; }
        public string OLDCT { get; set; }
        public string NEWCT { get; set; }
        public string UEFI { get; set; }
        public string BACKUPSN { get; set; }
        public string HPCT { get; set; }
        public string CHANGEPART { get; set; }
        public string CHANGEPARTNAME { get; set; }
    }

    public struct SFSNLIST
    {
        public string SNNO { get; set; }        
    }
    #endregion

    #region 服務案件執行條件
    /// <summary>
    /// 服務案件執行條件
    /// </summary>
    public enum SRCondition
    {
        /// <summary>
        /// 新建
        /// </summary>
        ADD,

        /// <summary>
        /// 轉派主要工程師
        /// </summary>
        TRANS,

        /// <summary>
        /// 駁回
        /// </summary>
        REJECT,

        /// <summary>
        /// HPGCSN申請
        /// </summary>
        HPGCSN,

        /// <summary>
        /// HPGCSN完成
        /// </summary>
        HPGCSNDONE,

        /// <summary>
        /// 二修
        /// </summary>
        SECFIX,

        /// <summary>
        /// 保存
        /// </summary>
        SAVE,

        /// <summary>
        /// 技術支援升級
        /// </summary>
        SUPPORT,

        /// <summary>
        /// 3 Party
        /// </summary>
        THRPARTY,

        /// <summary>
        /// 取消
        /// </summary>
        CANCEL,

        /// <summary>
        /// 完修
        /// </summary>
        DONE,

        /// <summary>
        /// 維修/DOA
        /// </summary>
        DOA,

        /// <summary>
        /// 裝機完成
        /// </summary>
        INSTALLDONE,

        /// <summary>
        /// 定保完成
        /// </summary>
        MAINTAINDONE
    }
    #endregion

    #region 產生服務報告書圖檔方式
    /// <summary>
    /// 產生服務報告書圖檔方式
    /// </summary>
    public enum SRReportType
    {
        /// <summary>
        /// 有簽名檔
        /// </summary>
        SIGN,

        /// <summary>
        /// 無簽名檔
        /// </summary>
        NOSIGN,

        /// <summary>
        /// 線上
        /// </summary>
        ONLINE,

        /// <summary>
        /// 遠端
        /// </summary>
        REMOTE,

        /// <summary>
        /// 純附件
        /// </summary>
        ATTACH
    }
    #endregion    

    #region 合約主數據資訊(For Mail)
    /// <summary>合約主數據資訊(For Mail)</summary>
    public class CONTRACTMAININFO
    {
        /// <summary>文件編號</summary>
        public string ContractID { get; set; }
        /// <summary>服務團隊</summary>
        public string TeamNAME { get; set; }
        /// <summary>服務團隊主管</summary>
        public string TeamMGR { get; set; }
        /// <summary>主要工程師</summary>
        public string MainENG { get; set; }
        /// <summary>協助工程師</summary>
        public string AssENG { get; set; }
        /// <summary>業務人員</summary>
        public string SalesEMP { get; set; }
        /// <summary>業務祕書</summary>
        public string SecretaryEMP { get; set; }
        /// <summary>維護業務人員</summary>
        public string MASalesEMP { get; set; }
        /// <summary>合約中心單位管理員</summary>
        public string CenterMGR { get; set; }
        /// <summary>異動時間</summary>
        public string ModifiedDate { get; set; }

        /// <summary>銷售訂單</summary>
        public string SoNo { get; set; }
        /// <summary>客戶ID</summary>
        public string CustomerID { get; set; }
        /// <summary>客戶名稱</summary>
        public string CustomerName { get; set; }
        /// <summary>訂單說明</summary>
        public string Desc { get; set; }
        /// <summary>維護日期(起)</summary>
        public string StartDate { get; set; }
        /// <summary>維護日期(迄)</summary>
        public string EndDate { get; set; }
        /// <summary>維護週期</summary>
        public string MACycle { get; set; }
        /// <summary>維護備註</summary>
        public string MANotes { get; set; }
        /// <summary>維護地址</summary>
        public string MAAddress { get; set; }
        /// <summary>合約備註</summary>
        public string ContractNotes { get; set; }
        /// <summary>請款備註</summary>
        public string BillNotes { get; set; }
        /// <summary>Log記錄</summary>
        public string Logs { get; set; }

        /// <summary>服務團隊主管Email</summary>
        public string TeamMGREmail { get; set; }
        /// <summary>主要工程師Email</summary>
        public string MainENGEmail { get; set; }
        /// <summary>協助工程師Email</summary>
        public string AssENGEmail { get; set; }
        /// <summary>業務人員Email</summary>
        public string SalesEmail { get; set; }
        /// <summary>業務祕書Email</summary>
        public string SecretaryEmail { get; set; }
        /// <summary>維護業務人員Email</summary>
        public string MASalesEmail { get; set; }
        /// <summary>合約中心單位管理員Email</summary>
        public string CenterMGREmail { get; set; }
    }
    #endregion

    #region 合約工程師資訊(For Mail)
    /// <summary>合約工程師資訊(For Mail)</summary>
    public class CONTRACTENGINFO
    {
        /// <summary>文件編號</summary>
        public string ContractID { get; set; }
        /// <summary>是否為主要工程師(Y.是 N.否)</summary>
        public string IsMain { get; set; }
        /// <summary>主要工程師</summary>
        public string MainENG { get; set; }
        /// <summary>原主要工程師</summary>
        public string OldMainENG { get; set; }
        /// <summary>協助工程師</summary>
        public string AssENG { get; set; }
        /// <summary>原協助工程師</summary>
        public string OldAssENG { get; set; }        
        /// <summary>異動時間</summary>
        public string ModifiedDate { get; set; }       
     
        /// <summary>主要工程師Email</summary>
        public string MainENGEmail { get; set; }
        /// <summary>原主要工程師Email</summary>
        public string OldMainENGEmail { get; set; }
        /// <summary>協助工程師Email</summary>
        public string AssENGEmail { get; set; }        
        /// <summary>原協助工程師Email</summary>
        public string OldAssENGEmail { get; set; }
    }
    #endregion

    #region 合約主數據執行條件
    /// <summary>
    /// 合約主數據執行條件
    /// </summary>
    public enum ContractCondition
    {
        /// <summary>
        /// 新建
        /// </summary>
        ADD,

        /// <summary>
        /// 保存
        /// </summary>
        SAVE,
    }
    #endregion

    #region 合約工程師執行條件
    /// <summary>
    /// 合約工程師執行條件
    /// </summary>
    public enum ContractENGCondition
    {
        /// <summary>
        /// 新建
        /// </summary>
        ADD,

        /// <summary>
        /// 編輯
        /// </summary>
        EDIT,

        /// <summary>
        /// 刪除
        /// </summary>
        DEL,
    }
    #endregion

    #region BPM用印/內部轉撥服務資訊
    public class BPMCONTRACTINFO
    {
        /// <summary>主合約編號(申請內部轉撥時，用來記錄原主約編號)</summary>
        public string IV_SUBCONTACT { get; set; }
        /// <summary>合約編號</summary>
        public string IV_CONTACT { get; set; }
        /// <summary>SO號碼(非必填)</summary>
        public string IV_SONO { get; set; }        
        /// <summary>業務員ERPID</summary>
        public string IV_SALES { get; set; }
        /// <summary>業務祕書ERPID</summary>
        public string IV_ASSITANCE { get; set; }
        /// <summary>內部轉撥維護業務ERPID</summary>    
        public string IV_MAINTAIN_SALES { get; set; }
        /// <summary>CRM客戶代號</summary>
        public string IV_CUSTOMER { get; set; }
        /// <summary>訂單說明-內容簡述(目的)(若為供應商，對象和目的就寫在這邊)</summary>
        public string IV_SODESC { get; set; }
        /// <summary>維護開始</summary>
        public DateTime IV_SDATE { get; set; }
        /// <summary>維護結束</summary>
        public DateTime IV_EDATE { get; set; }
        /// <summary>請款週期</summary>    
        public string IV_REQPAY { get; set; }
        /// <summary>定期維護週期</summary>    
        public string IV_CYCLE { get; set; }
        /// <summary>維護備註/維護注意事項</summary>    
        public string IV_NOTES { get; set; }
        /// <summary>維護合約地址</summary>    
        public string IV_ADDR { get; set; }
        /// <summary>服務條件</summary>    
        public string IV_SLASRV { get; set; }
        /// <summary>回應條件</summary>    
        public string IV_SLARESP { get; set; }
        /// <summary>合約備註</summary>    
        public string IV_NOTE { get; set; }
        /// <summary>對象身份</summary>    
        public string IV_ContractVendor { get; set; }
        /// <summary>文件編號，主約(BPM的主約文件編號)</summary>    
        public string IV_MAINID { get; set; }
        /// <summary>下包廠商(供應商)統一編號</summary>    
        public string IV_SUBNUMBER { get; set; }
        /// <summary>下包廠商(供應商)名稱</summary>    
        public string IV_ContractUser { get; set; }
        /// <summary>負責團隊Code(合約技術團隊)</summary>    
        public string IV_ORGID { get; set; }
        /// <summary>出貨日期</summary>
        public string IV_DNDATE { get; set; }
        /// <summary>驗收日期</summary>
        public string IV_GRDATE { get; set; }
        /// <summary>建立時間</summary>
        public string IV_CREATET { get; set; }
        /// <summary>發票日期</summary>
        public string IV_INVDATE { get; set; }
        /// <summary>請款備註</summary>
        public string IV_PAYNOTE { get; set; }
        /// <summary>客戶聯絡人姓名</summary>
        public string IV_ContactName { get; set; }
        /// <summary>客戶聯絡人E-Mail</summary>
        public string IV_ContactEmail { get; set; }        
    }
    #endregion
}