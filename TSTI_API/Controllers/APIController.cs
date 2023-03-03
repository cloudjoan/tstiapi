using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using SAP.Middleware.Connector;
using System.Web.Mvc;
using TSTI_API.Models;

namespace TSTI_API.Controllers
{
    public class APIController : Controller
    {
        TESTEntities testDB = new TESTEntities();
        TSTIONEEntities dbOne = new TSTIONEEntities();
        ERP_PROXY_DBEntities dbProxy = new ERP_PROXY_DBEntities();

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

            if(result > 0)
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

        #region -----↓↓↓↓↓一般服務請求建立 ↓↓↓↓↓-----

        #region 建立ONE SERVICE報修SR（一般服務請求）接口
        /// <summary>
        /// 建立ONE SERVICE報修SR（一般服務請求）接口
        /// </summary>
        /// <param name="bean"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_GENERALSR_CREATE(SRMain_GENERALSR_INPUT beanIN)
        {
            #region Json範列格式，一筆(建立GENERALSR_CREATEByAPI)
            //{
            //     "IV_LOGINEMPNO": "99120894",
            //     "IV_CUSTOMER": "D03251108",
            //     "IV_SRTEAM": "SRV.12211000",
            //     "IV_RKIND": "Z01",
            //     "IV_PATHWAY": "Z01",
            //     "IV_DESC": "Test20230106",
            //     "IV_LTXT": "Test20230106詳細說明",
            //     "IV_MKIND1": "ZA01",
            //     "IV_MKIND2": "ZB0101",
            //     "IV_MKIND3": "ZC010101",
            //     "IV_REPAIRNAME": "王炯凱",
            //     "IV_REPAIRTEL": "02-2506-2121#1239",
            //     "IV_REPAIRADDR": "台北市中山區松江路121號13樓",
            //     "IV_REPAIREMAIL": "CARRYWANG@hotaimotor.com.tw",
            //     "IV_CONTNAME": "周可斌",
            //     "IV_CONTTEL": "(02)6638-6888EXT.13104",
            //     "IV_CONTADDR": "信義區菸廠路88號12樓",
            //     "IV_CONTEMAIL": "AlexChou@taiwanmobile.com",
            //     "IV_EMPNO": "10001567",
            //     "IV_SQEMPID": "ZC103",
            //     "IV_SERIAL": "SGH33223R6",
            //     "IV_SNPID": "G-654081B21-057",
            //     "IV_WTY": "OM363636",
            //     "IV_REFIX": "N",
            //     "CREATECONTACT_LIST": [
            //        {
            //            "SRID": "612211250004",
            //            "CONTNAME": "賴淑瑛",
            //            "CONTADDR": "台北市信義區菸廠路88號12樓",
            //            "CONTTEL": "(02)6638-6888#13158",
            //            "CONTMOBILE": "",
            //            "CONTEMAIL": "winnielai@taiwanmobile.com"
            //        },
            //        {
            //            "SRID": "612211250004",
            //            "CONTNAME": "廖勇翔",
            //            "CONTADDR": "台北市信義區菸廠路88號12樓",
            //            "CONTTEL": "02-6638-6888#13124",
            //            "CONTMOBILE": "",
            //            "CONTEMAIL": "AllenLiao@taiwanmobile.com"
            //        }                 
            //    ]
            //}
            #endregion

            SRMain_GENERALSR_OUTPUT SROUT = new SRMain_GENERALSR_OUTPUT();

            SROUT = SaveGenerallySR(beanIN, "ADD"); //新增

            return Json(SROUT);
        }
        #endregion

        #region 儲存一般服務請求
        /// <summary>
        /// 儲存一般服務請求
        /// </summary>
        /// <param name="bean">一般服務請求主檔資訊</param>
        /// <param name="tType">ADD.新增 EDIT.修改</param>
        /// <returns></returns>
        private SRMain_GENERALSR_OUTPUT SaveGenerallySR(SRMain_GENERALSR_INPUT bean, string tType)
        {
            SRMain_GENERALSR_OUTPUT SROUT = new SRMain_GENERALSR_OUTPUT();            
            
            string pLoginName = string.Empty;
            string pSRID = string.Empty;
            string OldCStatus = string.Empty;
            string tONEURLName = string.Empty;
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;
            string tInvoiceNo = string.Empty;
            string tInvoiceItem = string.Empty;

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
            string IV_EMPNO = string.IsNullOrEmpty(bean.IV_EMPNO) ? "" : bean.IV_EMPNO.Trim();
            string IV_SQEMPID = string.IsNullOrEmpty(bean.IV_SQEMPID) ? "" : bean.IV_SQEMPID.Trim();
            string IV_SERIAL = string.IsNullOrEmpty(bean.IV_SERIAL) ? "" : bean.IV_SERIAL.Trim();
            string IV_SNPID = string.IsNullOrEmpty(bean.IV_SNPID) ? "" : bean.IV_SNPID.Trim();
            string IV_WTY = string.IsNullOrEmpty(bean.IV_WTY) ? "" : bean.IV_WTY.Trim();
            string IV_REFIX = string.IsNullOrEmpty(bean.IV_REFIX) ? "" : bean.IV_REFIX.Trim();

            string CCustomerName = CMF.findCustName(IV_CUSTOMER);
            string CSqpersonName = CMF.findSQPersonName(IV_SQEMPID);
            string CMainEngineerName = CMF.findEmployeeName(IV_EMPNO);

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName;
            }

            bool tIsFormal = CMF.getCallSAPERPPara(pOperationID_GenerallySR); //取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)          

            if (tIsFormal)
            {
                tONEURLName = "172.31.7.56:32200";
                tBPMURLName = "tsti-bpm01.etatung.com.tw";
                tPSIPURLName = "psip-prd-ap";
                tAttachURLName = "tsticrmmbgw.etatung.com:8082";
            }
            else
            {
                tONEURLName = "172.31.7.56:32200";
                tBPMURLName = "tsti-bpm01.etatung.com.tw";
                tPSIPURLName = "psip-prd-ap";
                tAttachURLName = "tsticrmmbgw.etatung.com:8082";
            }

            try
            {
                if (tType == "ADD") 
                {
                    #region 新增主檔
                    TB_ONE_SRMain beanM = new TB_ONE_SRMain();

                    pSRID = GetSRID("61");

                    //主表資料
                    beanM.cSRID = pSRID;
                    beanM.cStatus = IV_EMPNO != "" ? "E0005" : "E0001";    //新增時若有L2工程師，則預設為L3.處理中，反之則預設為新建
                    beanM.cCustomerName = CCustomerName;
                    beanM.cCustomerID = IV_CUSTOMER;                    
                    beanM.cDesc = IV_DESC;
                    beanM.cNotes = IV_LTXT;
                    beanM.cMAServiceType = IV_RKIND;
                    beanM.cSRTypeOne = IV_MKIND1;
                    beanM.cSRTypeSec = IV_MKIND2;
                    beanM.cSRTypeThr = IV_MKIND3;
                    beanM.cSRPathWay = IV_PATHWAY;
                    beanM.cSRProcessWay = "";
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

                    beanM.CreatedDate = DateTime.Now;
                    beanM.CreatedUserName = pLoginName;

                    dbOne.TB_ONE_SRMain.Add(beanM);
                    #endregion

                    #region 新增【客戶聯絡人資訊】明細
                    if (bean.CREATECONTACT_LIST != null)
                    {
                        foreach(var beanCon in bean.CREATECONTACT_LIST)
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

                    #region 新增【產品序號資訊】明細
                    string[] PRcSerialID = IV_SERIAL.Split(';');
                    string[] PRcMaterialID = IV_SNPID.Split(';');
                    string PRcMaterialName = string.Empty;
                    string PRcProductNumber = string.Empty;
                    string PRcInstallID = string.Empty;

                    int countPR = PRcSerialID.Length;

                    for (int i = 0; i < countPR; i++)
                    {
                        if (IV_SERIAL != "")
                        {
                            TB_ONE_SRDetail_Product beanD = new TB_ONE_SRDetail_Product();

                            PRcMaterialName = CMF.findMaterialName(PRcMaterialID[i]);
                            PRcProductNumber = CMF.findMFRPNumber(PRcMaterialID[i]);
                            PRcInstallID = CMF.findInstallNumber(IV_SERIAL);

                            beanD.cSRID = pSRID;
                            beanD.cSerialID = PRcSerialID[i];
                            beanD.cMaterialID = PRcMaterialID[i];
                            beanD.cMaterialName = PRcMaterialName;
                            beanD.cProductNumber = PRcProductNumber;
                            beanD.cInstallID = PRcInstallID;
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
                            string tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;

                            QueryToList = CMF.ZFM_TICC_SERIAL_SEARCHWTYList(PRcSerialID, tBPMURLName, tPSIPURLName, tAPIURLName);

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
                            //                    tURL = "http://" + tURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/Warranty/WarrantyForm.aspx?FormNo=" + bean.BpmNo + " target=_blank";
                            //                }
                            //                else
                            //                {
                            //                    tURL = "http://" + tURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Form/Guarantee/GuaranteeForm.aspx?FormNo=" + bean.BpmNo + " target=_blank";
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

                    foreach(var beanSR in QueryToList)
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
                            switch (tWTYID.Substring(0,2))
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
                        CMF.SetSRMailContent(SRCondition.ADD, pOperationID_GenerallySR, EmpBean.BUKRS, pSRID, tONEURLName, pLoginName, tIsFormal);
                        #endregion
                    }
                }
                else
                {
                    #region 註解
                    ////#region 修改主檔  
                    //var beanNowM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == pSRID);

                    ////主表資料
                    //OldCStatus = beanNowM.CStatus;

                    //beanNowM.CStatus = CStatus;
                    //beanNowM.CCustomerName = CCustomerName;
                    //beanNowM.CCustomerId = CCustomerId;                    
                    //beanNowM.CDesc = CDesc;
                    //beanNowM.CNotes = CNotes;
                    //beanNowM.CMaserviceType = CMaserviceType;
                    //beanNowM.CSrtypeOne = CSrtypeOne;
                    //beanNowM.CSrtypeSec = CSrtypeSec;
                    //beanNowM.CSrtypeThr = CSrtypeThr;
                    //beanNowM.CSrpathWay = CSrpathWay;
                    //beanNowM.CSrprocessWay = CSrprocessWay;
                    //beanNowM.CIsSecondFix = CIsSecondFix;
                    //beanNowM.CRepairName = CRepairName;
                    //beanNowM.CRepairAddress = CRepairAddress;
                    //beanNowM.CRepairPhone = CRepairPhone;
                    //beanNowM.CRepairEmail = CRepairEmail;
                    //beanNowM.CContacterName = CContacterName;
                    //beanNowM.CContactAddress = CContactAddress;
                    //beanNowM.CContactPhone = CContactPhone;
                    //beanNowM.CContactEmail = CContactEmail;
                    //beanNowM.CTeamId = CTeamId;
                    //beanNowM.CSqpersonId = CSqpersonId;
                    //beanNowM.CSqpersonName = CSqpersonName;
                    //beanNowM.CSalesName = CSalesName;
                    //beanNowM.CSalesId = CSalesId;
                    //beanNowM.CMainEngineerName = CMainEngineerName;
                    //beanNowM.CMainEngineerId = CMainEngineerId;
                    //beanNowM.CAssEngineerId = CAssEngineerId;
                    //beanNowM.CSystemGuid = Guid.NewGuid();

                    //beanNowM.ModifiedDate = DateTime.Now;
                    //beanNowM.ModifiedUserName = LoginUser_Name;
                    //#endregion

                    //#region -----↓↓↓↓↓產品序號資訊↓↓↓↓↓-----

                    //#region 刪除明細                    
                    //dbOne.TbOneSrdetailProducts.RemoveRange(dbOne.TbOneSrdetailProducts.Where(x => x.Disabled == 0 && x.CSrid == pSRID));
                    //#endregion

                    //#region 新增明細
                    //string[] PRcSerialID = formCollection["tbx_PRcSerialID"];
                    //string[] PRcMaterialID = formCollection["tbx_PRcMaterialID"];
                    //string[] PRcMaterialName = formCollection["tbx_PRcMaterialName"];
                    //string[] PRcProductNumber = formCollection["tbx_PRcProductNumber"];
                    //string[] PRcInstallID = formCollection["tbx_PRcInstallID"];
                    //string[] PRcDisabled = formCollection["hid_PRcDisabled"];

                    //int countPR = PRcSerialID.Length;

                    //for (int i = 0; i < countPR; i++)
                    //{
                    //    TbOneSrdetailProduct beanD = new TbOneSrdetailProduct();

                    //    beanD.CSrid = pSRID;
                    //    beanD.CSerialId = PRcSerialID[i];
                    //    beanD.CMaterialId = PRcMaterialID[i];
                    //    beanD.CMaterialName = PRcMaterialName[i];
                    //    beanD.CProductNumber = PRcProductNumber[i];
                    //    beanD.CInstallId = PRcInstallID[i];
                    //    beanD.Disabled = int.Parse(PRcDisabled[i]);

                    //    beanD.CreatedDate = DateTime.Now;
                    //    beanD.CreatedUserName = LoginUser_Name;

                    //    dbOne.TbOneSrdetailProducts.Add(beanD);
                    //}
                    //#endregion

                    //#endregion -----↑↑↑↑↑產品序號資訊 ↑↑↑↑↑-----

                    //#region -----↓↓↓↓↓保固SLA資訊↓↓↓↓↓-----

                    //#region 刪除明細
                    //dbOne.TbOneSrdetailWarranties.RemoveRange(dbOne.TbOneSrdetailWarranties.Where(x => x.CSrid == pSRID));
                    //#endregion

                    //#region 新增明細
                    //string[] WAcSerialID = formCollection["hidcSerialID"];
                    //string[] WAcWTYID = formCollection["hidcWTYID"];
                    //string[] WAcWTYName = formCollection["hidcWTYName"];
                    //string[] WAcWTYSDATE = formCollection["hidcWTYSDATE"];
                    //string[] WAcWTYEDATE = formCollection["hidcWTYEDATE"];
                    //string[] WAcSLARESP = formCollection["hidcSLARESP"];
                    //string[] WAcSLASRV = formCollection["hidcSLASRV"];
                    //string[] WAcContractID = formCollection["hidcContractID"];
                    //string[] WAcBPMFormNo = formCollection["hidcBPMFormNo"];
                    //string[] WACheckUsed = formCollection["hid_CheckUsed"];

                    //int countWA = WAcSerialID.Length;

                    //for (int i = 0; i < countWA; i++)
                    //{
                    //    TbOneSrdetailWarranty beanD = new TbOneSrdetailWarranty();

                    //    beanD.CSrid = pSRID;
                    //    beanD.CSerialId = WAcSerialID[i];
                    //    beanD.CWtyid = WAcWTYID[i];
                    //    beanD.CWtyname = WAcWTYName[i];

                    //    if (WAcWTYSDATE[i] != "")
                    //    {
                    //        beanD.CWtysdate = Convert.ToDateTime(WAcWTYSDATE[i]);
                    //    }

                    //    if (WAcWTYEDATE[i] != "")
                    //    {
                    //        beanD.CWtyedate = Convert.ToDateTime(WAcWTYEDATE[i]);
                    //    }

                    //    beanD.CSlaresp = WAcSLARESP[i];
                    //    beanD.CSlasrv = WAcSLASRV[i];
                    //    beanD.CContractId = WAcContractID[i];
                    //    beanD.CBpmformNo = WAcBPMFormNo[i];
                    //    beanD.CUsed = WACheckUsed[i];

                    //    beanD.CreatedDate = DateTime.Now;
                    //    beanD.CreatedUserName = LoginUser_Name;

                    //    dbOne.TbOneSrdetailWarranties.Add(beanD);
                    //}
                    //#endregion

                    //#endregion -----↑↑↑↑↑保固SLA資訊 ↑↑↑↑↑-----

                    //#region -----↓↓↓↓↓處理與工時紀錄↓↓↓↓↓-----

                    //#region 刪除明細
                    //dbOne.TbOneSrdetailRecords.RemoveRange(dbOne.TbOneSrdetailRecords.Where(x => x.Disabled == 0 && x.CSrid == pSRID));
                    //#endregion

                    //#region 新增明細
                    //string[] REcEngineerName = formCollection["tbx_REcEngineerName"];
                    //string[] REcEngineerID = formCollection["hid_REcEngineerID"];
                    //string[] REcReceiveTime = formCollection["tbx_REcReceiveTime"];
                    //string[] REcStartTime = formCollection["tbx_REcStartTime"];
                    //string[] REcArriveTime = formCollection["tbx_REcArriveTime"];
                    //string[] REcFinishTime = formCollection["tbx_REcFinishTime"];
                    //string[] REcWorkHours = formCollection["tbx_REcWorkHours"];
                    //string[] REcDesc = formCollection["tbx_REcDesc"];
                    //string[] REcSRReport = formCollection["hid_filezoneRE"];
                    //string[] REcDisabled = formCollection["hid_REcDisabled"];

                    //int countRE = REcEngineerName.Length;

                    //for (int i = 0; i < countRE; i++)
                    //{
                    //    TbOneSrdetailRecord beanD = new TbOneSrdetailRecord();

                    //    beanD.CSrid = pSRID;
                    //    beanD.CEngineerName = REcEngineerName[i];
                    //    beanD.CEngineerId = REcEngineerID[i];

                    //    if (REcReceiveTime[i] != "")
                    //    {
                    //        beanD.CReceiveTime = Convert.ToDateTime(REcReceiveTime[i]);
                    //    }

                    //    if (REcStartTime[i] != "")
                    //    {
                    //        beanD.CStartTime = Convert.ToDateTime(REcStartTime[i]);
                    //    }

                    //    if (REcArriveTime[i] != "")
                    //    {
                    //        beanD.CArriveTime = Convert.ToDateTime(REcArriveTime[i]);
                    //    }

                    //    if (REcFinishTime[i] != "")
                    //    {
                    //        beanD.CFinishTime = Convert.ToDateTime(REcFinishTime[i]);
                    //    }

                    //    beanD.CWorkHours = decimal.Parse(REcWorkHours[i]);
                    //    beanD.CDesc = REcDesc[i];
                    //    beanD.CSrreport = REcSRReport[i];
                    //    beanD.Disabled = int.Parse(REcDisabled[i]);

                    //    beanD.CreatedDate = DateTime.Now;
                    //    beanD.CreatedUserName = LoginUser_Name;

                    //    dbOne.TbOneSrdetailRecords.Add(beanD);
                    //}
                    //#endregion

                    //#endregion -----↑↑↑↑↑處理與工時紀錄 ↑↑↑↑↑-----

                    //#region -----↓↓↓↓↓零件更換資訊↓↓↓↓↓-----

                    //#region 刪除明細                   
                    //dbOne.TbOneSrdetailPartsReplaces.RemoveRange(dbOne.TbOneSrdetailPartsReplaces.Where(x => x.Disabled == 0 && x.CSrid == pSRID));
                    //#endregion

                    //#region 新增明細
                    //string[] PAcXCHP = formCollection["tbx_PAcXCHP"];
                    //string[] PAcMaterialID = formCollection["tbx_PAcMaterialID"];
                    //string[] PAcMaterialName = formCollection["tbx_PAcMaterialName"];
                    //string[] PAcOldCT = formCollection["tbx_PAcOldCT"];
                    //string[] PAcNewCT = formCollection["tbx_PAcNewCT"];
                    //string[] PAcHPCT = formCollection["tbx_PAcHPCT"];
                    //string[] PAcNewUEFI = formCollection["tbx_PAcNewUEFI"];
                    //string[] PAcStandbySerialID = formCollection["tbx_PAcStandbySerialID"];
                    //string[] PAcHPCaseID = formCollection["tbx_PAcHPCaseID"];
                    //string[] PAcArriveDate = formCollection["tbx_PAcArriveDate"];
                    //string[] PAcReturnDate = formCollection["tbx_PAcReturnDate"];
                    //string[] PAcMaterialItem = formCollection["tbx_PAcMaterialItem"];
                    //string[] PAcNote = formCollection["tbx_PAcNote"];
                    //string[] PAcDisabled = formCollection["hid_PAcDisabled"];

                    //int countPA = PAcMaterialID.Length;

                    //for (int i = 0; i < countPA; i++)
                    //{
                    //    TbOneSrdetailPartsReplace beanD = new TbOneSrdetailPartsReplace();

                    //    beanD.CSrid = pSRID;
                    //    beanD.CXchp = PAcXCHP[i];
                    //    beanD.CMaterialId = PAcMaterialID[i];
                    //    beanD.CMaterialName = PAcMaterialName[i];
                    //    beanD.COldCt = PAcOldCT[i];
                    //    beanD.CNewCt = PAcNewCT[i];
                    //    beanD.CHpct = PAcHPCT[i];
                    //    beanD.CNewUefi = PAcNewUEFI[i];
                    //    beanD.CStandbySerialId = PAcStandbySerialID[i];
                    //    beanD.CHpcaseId = PAcHPCaseID[i];

                    //    if (PAcArriveDate[i] != "")
                    //    {
                    //        beanD.CArriveDate = Convert.ToDateTime(PAcArriveDate[i]);
                    //    }

                    //    if (PAcReturnDate[i] != "")
                    //    {
                    //        beanD.CReturnDate = Convert.ToDateTime(PAcReturnDate[i]);
                    //    }

                    //    beanD.CMaterialItem = PAcMaterialItem[i];
                    //    beanD.CNote = PAcNote[i];
                    //    beanD.Disabled = int.Parse(PAcDisabled[i]);

                    //    beanD.CreatedDate = DateTime.Now;
                    //    beanD.CreatedUserName = LoginUser_Name;

                    //    dbOne.TbOneSrdetailPartsReplaces.Add(beanD);
                    //}
                    //#endregion

                    //#endregion -----↑↑↑↑↑零件更換資訊 ↑↑↑↑↑-----

                    //int result = dbOne.SaveChanges();

                    //if (result <= 0)
                    //{
                    //    pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "提交失敗(編輯)" + Environment.NewLine;
                    //    CMF.writeToLog(pSRID, "SaveGenerallySR", pMsg, LoginUser_Name);
                    //}
                    //else
                    //{
                    //    #region 紀錄修改log
                    //    string tLog = "SR狀態_舊值: " + OldCStatus + "; 新值: " + CStatus;
                    //    CMF.writeToLog(pSRID, "SaveGenerallySR", tLog, LoginUser_Name);
                    //    #endregion
                    //}
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();                

                CMF.writeToLog(pSRID, "SaveGenerallySR_API", pMsg, pLoginName);

                SROUT.EV_SRID = pSRID;
                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = ex.Message;
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

        #region 一般服務請求主檔INPUT資訊
        /// <summary>一般服務請求主檔INPUT資訊</summary>
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
            /// <summary>服務請求說明</summary>
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
            /// <summary>L2工程師員工編號</summary>
            public string IV_EMPNO { get; set; }
            /// <summary>SQ人員ID</summary>
            public string IV_SQEMPID { get; set; }
            /// <summary>序號ID</summary>
            public string IV_SERIAL { get; set; }
            /// <summary>物料代號</summary>
            public string IV_SNPID { get; set; }
            /// <summary>保固代號(若是合約則傳入合約編號)</summary>
            public string IV_WTY { get; set; }
            /// <summary>是否為二修(Y.是 N.否)</summary>
            public string IV_REFIX { get; set; }

            /// <summary>服務請求客戶聯絡人資訊</summary>
            public List<CREATECONTACTINFO> CREATECONTACT_LIST { get; set; }
        }
        #endregion

        #region 一般服務請求主檔OUTPUT資訊
        /// <summary>一般服務請求主檔OUTPUT資訊</summary>
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

        #endregion -----↑↑↑↑↑一般服務請求建立 ↑↑↑↑↑-----    

        #region -----↓↓↓↓↓一般服務請求狀態更新 ↓↓↓↓↓-----

        #region ONE SERVICE（一般服務請求）狀態更新接口
        /// <summary>
        /// ONE SERVICE（一般服務請求）狀態更新接口
        /// </summary>
        /// <param name="bean"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GENERALSRSTATUS_UPDATE(SRMain_GENERALSRSTATUS_INPUT beanIN)
        {
            #region Json範列格式
            //{
            //     "IV_LOGINEMPNO": "99120894",
            //     "IV_SRID": "612211250004",
            //     "IV_STATUS": "E0005"            
            //}
            #endregion

            SRMain_GENERALSRSTATUS_OUTPUT SROUT = new SRMain_GENERALSRSTATUS_OUTPUT();

            SROUT = GenerallySRSTATUS_Update(beanIN);

            return Json(SROUT);
        }
        #endregion

        #region 更新（一般服務請求）狀態
        private SRMain_GENERALSRSTATUS_OUTPUT GenerallySRSTATUS_Update(SRMain_GENERALSRSTATUS_INPUT bean)
        {
            SRMain_GENERALSRSTATUS_OUTPUT SROUT = new SRMain_GENERALSRSTATUS_OUTPUT();

            string pLoginName = string.Empty;            
            string tONEURLName = string.Empty;
            string IV_LOGINEMPNO = string.IsNullOrEmpty(bean.IV_LOGINEMPNO) ? "" : bean.IV_LOGINEMPNO.Trim();
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
                pLoginName = EmpBean.EmployeeCName;
            }

            bool tIsFormal = CMF.getCallSAPERPPara(pOperationID_GenerallySR); //取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)          

            if (tIsFormal)
            {
                tONEURLName = "172.31.7.56:32200";                     
            }
            else
            {
                tONEURLName = "172.31.7.56:32200";             
            }          

            try
            {               
                SRCondition tCondition = new SRCondition();

                var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == IV_SRID);

                if (beanM != null)
                {
                    #region 判斷寄送Mail的狀態
                    if (beanM.cStatus != IV_STATUS)
                    {
                        switch(IV_STATUS)
                        {
                            case "E0002": //L2處理中
                            case "E0003": //報價中
                            case "E0005": //L3處理中
                            case "E0013": //HPGCSN 完成
                                tCondition = SRCondition.SAVE;
                                break;

                            case "E0004": //3rd Party處理中                            
                                tCondition = SRCondition.THRPARTY;
                                break;

                            case "E0006": //完修                   
                                tCondition = SRCondition.DONE;
                                break;

                            case "E0007": //技術支援升級           
                                tCondition = SRCondition.SUPPORT;
                                break;

                            case "E0012": //HPGCSN 申請           
                                tCondition = SRCondition.HPGCSN;
                                break;

                            case "E0014": //駁回       
                                tCondition = SRCondition.REJECT;
                                break;

                            case "E0015": //取消
                                tCondition = SRCondition.CANCEL;
                                break;
                        }
                    }
                    else
                    {
                        tCondition = SRCondition.SAVE;
                    }
                    #endregion

                    beanM.cStatus = IV_STATUS;
                    beanM.ModifiedDate = DateTime.Now;
                    beanM.ModifiedUserName = pLoginName;

                    int result = dbOne.SaveChanges();

                    if (result <= 0)
                    {
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "更新失敗！" + Environment.NewLine;
                        CMF.writeToLog(IV_SRID, "SaveGenerallySR_API", pMsg, pLoginName);

                        SROUT.EV_MSGT = "E";
                        SROUT.EV_MSG = pMsg;
                    }
                    else
                    {
                        SROUT.EV_MSGT = "Y";
                        SROUT.EV_MSG = "";

                        #region 寄送Mail通知
                        CMF.SetSRMailContent(tCondition, pOperationID_GenerallySR, EmpBean.BUKRS, IV_SRID, tONEURLName, pLoginName, tIsFormal);
                        #endregion
                    }
                }                
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                CMF.writeToLog(IV_SRID, "GenerallySRSTATUS_Update_API", pMsg, pLoginName);
                
                SROUT.EV_MSGT = "E";
                SROUT.EV_MSG = ex.Message;
            }

            return SROUT;
        }
        #endregion

        #region 一般服務請求狀態更新INPUT資訊
        /// <summary>一般服務請求狀態更新INPUT資訊</summary>
        public struct SRMain_GENERALSRSTATUS_INPUT
        {
            /// <summary>修改者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>服務請求ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>服務狀態ID</summary>
            public string IV_STATUS { get; set; }            
        }
        #endregion

        #region 一般服務請求狀態更新OUTPUT資訊
        /// <summary>一般服務請求狀態更新OUTPUT資訊</summary>
        public struct SRMain_GENERALSRSTATUS_OUTPUT
        {            
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑一般服務請求狀態更新 ↑↑↑↑↑-----    

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

            try
            {
                var tList = CMF.findCUSTOMERINFO(beanIN.IV_CUSTOME.Trim());

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
                        CUSTOMERINFO_LIST beanCust = new CUSTOMERINFO_LIST();

                        beanCust.CUSTOMERID = bean.KNA1_KUNNR.Trim();
                        beanCust.CUSTOMERNAME = bean.KNA1_NAME1.Trim();

                        tCustList.Add(beanCust);
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

        #region 查詢法人客戶資料INPUT資訊
        /// <summary>查詢法人客戶資料資料INPUT資訊</summary>
        public struct CUSTOMERINFO_INPUT
        {
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
            /// <summary>客戶代號</summary>
            public string CUSTOMERID { get; set; }
            /// <summary>客戶名稱</summary>
            public string CUSTOMERNAME { get; set; }           
        }
        #endregion

        #endregion -----↑↑↑↑↑法人客戶資料 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓法人客戶聯絡人資料 ↓↓↓↓↓-----

        #region 查詢法人客戶聯絡人資料接口
        [HttpPost]
        public ActionResult API_CONTACTINFO_GET(CONTACTINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //   "IV_CUSTOMEID": "D16151427",
            //   "IV_CONTACTNAME": "",
            //   "IV_CONTACTTEL": "",
            //   "IV_CONTACTMOBILE": "",
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
                string CUSTOMEID = string.IsNullOrEmpty(beanIN.IV_CUSTOMEID) ? "" : beanIN.IV_CUSTOMEID.Trim();
                string CONTACTNAME = string.IsNullOrEmpty(beanIN.IV_CONTACTNAME) ? "" : beanIN.IV_CONTACTNAME.Trim();
                string CONTACTTEL = string.IsNullOrEmpty(beanIN.IV_CONTACTTEL) ? "" : beanIN.IV_CONTACTTEL.Trim();
                string CONTACTMOBILE = string.IsNullOrEmpty(beanIN.IV_CONTACTMOBILE) ? "" : beanIN.IV_CONTACTMOBILE.Trim();
                string CONTACTEMAIL = string.IsNullOrEmpty(beanIN.IV_CONTACTEMAIL) ? "" : beanIN.IV_CONTACTEMAIL.Trim();

                var tList = CMF.findCONTACTINFO(CUSTOMEID, CONTACTNAME, CONTACTTEL, CONTACTMOBILE, CONTACTEMAIL);

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

                        beanCust.CONTACTNAME = bean.Name;
                        beanCust.CONTACTCITY = bean.City;
                        beanCust.CONTACTADDRESS = bean.Address;
                        beanCust.CONTACTTEL = bean.Phone;
                        beanCust.CONTACTMOBILE = bean.Mobile;
                        beanCust.CONTACTEMAIL = bean.Email;

                        tCustList.Add(beanCust);
                    }

                    OUTBean.CONTACTINFO_LIST = tCustList;
                    #endregion
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
            /// <summary>法人客戶代號</summary>
            public string IV_CUSTOMEID { get; set; }
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

        #endregion -----↑↑↑↑↑法人客戶聯絡人資料建立 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓法人客戶聯絡人資料建立/修改 ↓↓↓↓↓-----

        #region 法人客戶聯絡人資料新增接口
        [HttpPost]
        public ActionResult API_CONTACT_CREATE(CONTACTCREATE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "etatung\\elvis.chang",
            //    "IV_CUSTOMEID": "D16151427",
            //    "IV_CONTACTNAME": "張豐穎",
            //    "IV_CONTACTCITY": "台中市",
            //    "IV_CONTACTADDRESS": "南屯區五權西路二段236號6樓之1",
            //    "IV_CONTACTTEL": "04-24713300",
            //    "IV_CONTACTMOBILE": "0972",
            //    "IV_CONTACTEMAIL": "elvis.chang@etatung.com"
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
            //    "IV_LOGINEMPNO": "etatung\\elvis.chang",
            //    "IV_CUSTOMEID": "D16151427",
            //    "IV_CONTACTNAME": "張豐穎",
            //    "IV_CONTACTCITY": "台中市",
            //    "IV_CONTACTADDRESS": "南屯區五權西路二段236號6樓之1",
            //    "IV_CONTACTTEL": "04-24713300",
            //    "IV_CONTACTMOBILE": "0972",
            //    "IV_CONTACTEMAIL": "elvis.chang@etatung.com",
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
            string cBUKRS = "T012";
            string pLoginName = string.Empty;

            string CCustomerName = CMF.findCustName(beanIN.IV_CUSTOMEID);
            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_ISDELETE = string.IsNullOrEmpty(beanIN.IV_ISDELETE) ? "" : beanIN.IV_ISDELETE.Trim();

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName;
                cBUKRS = EmpBean.BUKRS;
            }

            try
            {
                var bean = dbProxy.CUSTOMER_Contact.FirstOrDefault(x => (x.Disabled == null || x.Disabled != 1) && x.BpmNo == tBpmNo && 
                                                                     x.KNB1_BUKRS == cBUKRS && x.KNA1_KUNNR == beanIN.IV_CUSTOMEID.Trim() && x.ContactName == beanIN.IV_CONTACTNAME.Trim());

                if (bean != null) //修改
                {
                    bean.ContactCity = beanIN.IV_CONTACTCITY.Trim();
                    bean.ContactAddress = beanIN.IV_CONTACTADDRESS.Trim();
                    bean.ContactPhone = beanIN.IV_CONTACTTEL.Trim();
                    bean.ContactMobile = beanIN.IV_CONTACTMOBILE.Trim();
                    bean.ContactEmail = beanIN.IV_CONTACTEMAIL.Trim();

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
                    bean1.KNA1_KUNNR = beanIN.IV_CUSTOMEID.Trim();
                    bean1.KNA1_NAME1 = CCustomerName;
                    bean1.KNB1_BUKRS = cBUKRS;
                    bean1.ContactType = "5"; //One Service
                    bean1.ContactName = beanIN.IV_CONTACTNAME.Trim();
                    bean1.ContactCity = beanIN.IV_CONTACTCITY.Trim();
                    bean1.ContactAddress = beanIN.IV_CONTACTADDRESS.Trim();
                    bean1.ContactPhone = beanIN.IV_CONTACTTEL.Trim();
                    bean1.ContactMobile = beanIN.IV_CONTACTMOBILE.Trim();
                    bean1.ContactEmail = beanIN.IV_CONTACTEMAIL.Trim();
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

            try
            {
                var tList = CMF.findPERSONALINFO(beanIN.IV_PERSONAL.Trim());

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
                        PERSONALINFO_LIST beanCust = new PERSONALINFO_LIST();

                        beanCust.PERSONALID = bean.KNA1_KUNNR.Trim();
                        beanCust.PERSONALNAME = bean.KNA1_NAME1.Trim();

                        tCustList.Add(beanCust);
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

        #region 查詢個人客戶資料INPUT資訊
        /// <summary>查詢個人客戶資料資料INPUT資訊</summary>
        public struct PERSONALINFO_INPUT
        {
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
            /// <summary>個人客戶代號</summary>
            public string PERSONALID { get; set; }
            /// <summary>個人客戶名稱</summary>
            public string PERSONALNAME { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑個人客戶資料 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓個人客戶聯絡人資料 ↓↓↓↓↓-----

        #region 查詢個人客戶聯絡人資料接口
        [HttpPost]
        public ActionResult API_PERSONALCONTACTINFO_GET(PERSONALCONTACTINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
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
                string PERSONALID = string.IsNullOrEmpty(beanIN.IV_PERSONALID) ? "" : beanIN.IV_PERSONALID.Trim();
                string CONTACTNAME = string.IsNullOrEmpty(beanIN.IV_CONTACTNAME) ? "" : beanIN.IV_CONTACTNAME.Trim();
                string CONTACTTEL = string.IsNullOrEmpty(beanIN.IV_CONTACTTEL) ? "" : beanIN.IV_CONTACTTEL.Trim();
                string CONTACTMOBILE = string.IsNullOrEmpty(beanIN.IV_CONTACTMOBILE) ? "" : beanIN.IV_CONTACTMOBILE.Trim();
                string CONTACTEMAIL = string.IsNullOrEmpty(beanIN.IV_CONTACTEMAIL) ? "" : beanIN.IV_CONTACTEMAIL.Trim();

                var tList = CMF.findPERSONALCONTACTINFO(PERSONALID, CONTACTNAME, CONTACTTEL, CONTACTMOBILE, CONTACTEMAIL);

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

        #endregion -----↑↑↑↑↑個人客戶聯絡人資料建立 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓個人客戶聯絡人資料建立/修改 ↓↓↓↓↓-----

        #region 個人客戶聯絡人資料新增接口
        [HttpPost]
        public ActionResult API_PERSONALCONTACT_CREATE(PERSONALCONTACTCREATE_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_LOGINEMPNO": "99120894",            
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
            
            string cBUKRS = "T012";
            string pLoginName = string.Empty;
            
            string IV_LOGINEMPNO = string.IsNullOrEmpty(beanIN.IV_LOGINEMPNO) ? "" : beanIN.IV_LOGINEMPNO.Trim();
            string IV_PERSONALID = string.IsNullOrEmpty(beanIN.IV_PERSONALID) ? "" : beanIN.IV_PERSONALID.Trim();
            string IV_PERSONALNAME = string.IsNullOrEmpty(beanIN.IV_PERSONALNAME) ? "" : beanIN.IV_PERSONALNAME.Trim();
            string IV_ISDELETE = string.IsNullOrEmpty(beanIN.IV_ISDELETE) ? "" : beanIN.IV_ISDELETE.Trim();

            EmployeeBean EmpBean = new EmployeeBean();
            EmpBean = CMF.findEmployeeInfoByERPID(IV_LOGINEMPNO);

            if (string.IsNullOrEmpty(EmpBean.EmployeeCName))
            {
                pLoginName = IV_LOGINEMPNO;
            }
            else
            {
                pLoginName = EmpBean.EmployeeCName;
                cBUKRS = EmpBean.BUKRS;
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
                        bean.ContactCity = beanIN.IV_CONTACTCITY.Trim();
                        bean.ContactAddress = beanIN.IV_CONTACTADDRESS.Trim();
                        bean.ContactPhone = beanIN.IV_CONTACTTEL.Trim();
                        bean.ContactMobile = beanIN.IV_CONTACTMOBILE.Trim();
                        bean.ContactEmail = beanIN.IV_CONTACTEMAIL.Trim();

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
                        bean1.ContactName = beanIN.IV_CONTACTNAME.Trim();
                        bean1.ContactCity = beanIN.IV_CONTACTCITY.Trim();
                        bean1.ContactAddress = beanIN.IV_CONTACTADDRESS.Trim();
                        bean1.ContactPhone = beanIN.IV_CONTACTTEL.Trim();
                        bean1.ContactMobile = beanIN.IV_CONTACTMOBILE.Trim();
                        bean1.ContactEmail = beanIN.IV_CONTACTEMAIL.Trim();
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

        #region -----↓↓↓↓↓序號相關資訊查詢(產品序號資訊、保固SLA資訊(List)、服務請求資訊(List)、服務請求客戶聯絡人資訊(List)) ↓↓↓↓↓-----

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

            bool tIsFormal = CMF.getCallSAPERPPara(pOperationID_GenerallySR); //取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)
            string tURLName = string.Empty;
            string tSeverName = string.Empty;
            string[] ArySERIAL = new string[1];

            if (tIsFormal)
            {
                tURLName = "tsti-bpm01.etatung.com.tw";
                tSeverName = "psip-prd-ap";
            }
            else
            {
                tURLName = "bpm-qas";
                tSeverName = "psip-qas";
            }

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
                string tAPIURLName = @"https://" + HttpContext.Request.Url.Authority;

                #region 保固SLA資訊(List)
                List<SRWarranty> QueryToList = new List<SRWarranty>();    //查詢出來的清單                

                ArySERIAL[0] = beanIN.IV_SERIAL.Trim();

                QueryToList = CMF.ZFM_TICC_SERIAL_SEARCHWTYList(ArySERIAL, tURLName, tSeverName, tAPIURLName);
                QueryToList = QueryToList.OrderBy(x => x.SERIALID).ThenByDescending(x => x.WTYEDATE).ToList();

                SROUT.WTSLA_LIST = QueryToList;
                #endregion

                #region 服務請求主檔資訊清單
                List<SRIDINFO> QuerySRToList = new List<SRIDINFO>();    //查詢出來的清單

                QuerySRToList = CMF.findSRMAINList(pOperationID_GenerallySR, beanIN.IV_SERIAL.Trim());

                SROUT.SRMAIN_LIST = QuerySRToList;
                #endregion

                #region 服務請求客戶聯絡人資訊清單

                #region 先取得SRID清單
                List<string> tSRIDList = new List<string>();

                if (QuerySRToList.Count > 0)
                {
                    foreach(var SRBean in QuerySRToList)
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
            /// <summary>服務請求主檔資訊清單</summary>
            public List<SRIDINFO> SRMAIN_LIST { get; set; }
            /// <summary>服務請求客戶聯絡人資訊</summary>
            public List<SRCONTACTINFO> SRCONTACT_LIST { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑序號相關資訊查詢(產品序號資訊、保固SLA資訊(List)、服務請求資訊(List)、服務請求客戶聯絡人資訊(List)) ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓SRID相關資訊查詢(服務主檔資訊、客戶聯絡窗口資訊清單、產品序號資訊清單、保固SLA檔資訊清單、處理與工時紀錄清單、零件更換資訊清單) ↓↓↓↓↓-----

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

            bool tIsFormal = CMF.getCallSAPERPPara(pOperationID_GenerallySR); //取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)
            string tBPMURLName = string.Empty;
            string tPSIPURLName = string.Empty;
            string tAttachURLName = string.Empty;

            if (tIsFormal)
            {
                tBPMURLName = "tsti-bpm01.etatung.com.tw";
                tPSIPURLName = "psip-prd-ap";
                tAttachURLName = "tsticrmmbgw.etatung.com:8082";
            }
            else
            {
                tBPMURLName = "tsti-bpm01.etatung.com.tw";
                tPSIPURLName = "psip-prd-ap";
                tAttachURLName = "tsticrmmbgw.etatung.com:8082";
            }

            #region 取得主檔資訊
            var MainBean = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == beanIN.IV_SRID.Trim());

            if (MainBean.cSRID != null)
            {
                string cBUKRS = CMF.findBUKRSByTeamID(MainBean.cTeamID);
                string cStatusDesc = CMF.findSysParameterDescription(pOperationID_GenerallySR, "OTHER", cBUKRS, "SRSTATUS", MainBean.cStatus);             
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
                string cAttchURL = CMF.findAttachUrl(MainBean.cAttachement, tAttachURLName);
                string cREPAIRNAME = string.IsNullOrEmpty(MainBean.cRepairName) ? "" : MainBean.cRepairName;
                string cREPAIRADDR = string.IsNullOrEmpty(MainBean.cRepairAddress) ? "" : MainBean.cRepairAddress;
                string cREPAIRTEL = string.IsNullOrEmpty(MainBean.cRepairPhone) ? "" : MainBean.cRepairPhone;
                string cREPAIRMOB = string.IsNullOrEmpty(MainBean.cRepairMobile) ? "" : MainBean.cRepairMobile;
                string cREPAIREMAIL = string.IsNullOrEmpty(MainBean.cRepairEmail) ? "" : MainBean.cRepairEmail;
                string cSQEMP = string.IsNullOrEmpty(MainBean.cSQPersonID) ? "" : MainBean.cSQPersonID;

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
                SROUT.ATTACHURL = cAttchURL;

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
                #region 【客戶聯絡人資訊】清單
                List<SRCONTACTINFO> SRCONTACT_LIST = CMF.findSRCONTACTINFO(MainBean.cSRID);
                SROUT.SRCONTACT_LIST = SRCONTACT_LIST;
                #endregion

                #region 【產品序號資訊】清單
                List<SRSERIALMATERIALINFO> SRSERIAL_LIST = CMF.findSRSERIALMATERIALINFO(MainBean.cSRID);
                SROUT.SRSERIAL_LIST = SRSERIAL_LIST;
                #endregion

                #region 【保固SLA資訊】清單
                List<SRWTSLAINFO> SRWTSLA_LIST = CMF.findSRWTSLAINFO(MainBean.cSRID, tBPMURLName, tPSIPURLName);
                SROUT.SRWTSLA_LIST = SRWTSLA_LIST;
                #endregion

                #region 【處理與工時紀錄資訊】清單
                List<SRRECORDINFO> SRRECORD_LIST = CMF.findSRRECORDINFO(MainBean.cSRID, tAttachURLName);
                SROUT.SRRECORD_LIST = SRRECORD_LIST;
                #endregion

                #region 【零件更換資訊】清單
                List<SRPARTSREPALCEINFO> SRPARTS_LIST = CMF.findSRPARTSREPALCEINFO(MainBean.cSRID);
                SROUT.SRPARTS_LIST = SRPARTS_LIST;
                #endregion
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
            /// <summary>服務請求ID</summary>
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
            /// <summary>L2工程師</summary>
            public string MAINENG { get; set; }
            /// <summary>指派工程師</summary>
            public string ASSENGN { get; set; }
            /// <summary>技術主管</summary>
            public string TECHMAG { get; set; }
            /// <summary>SQL人員</summary>
            public string SQEMP { get; set; }
            /// <summary>計費業務</summary>
            public string SALES { get; set; }
            /// <summary>檢附文件URL</summary>
            public string ATTACHURL { get; set; }
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }

            /// <summary>服務請求【客戶聯絡人資訊】清單</summary>
            public List<SRCONTACTINFO> SRCONTACT_LIST { get; set; }
            /// <summary>服務請求【產品序號資訊】清單</summary>
            public List<SRSERIALMATERIALINFO> SRSERIAL_LIST { get; set; }
            /// <summary>服務請求【保固SLA資訊】清單</summary>
            public List<SRWTSLAINFO> SRWTSLA_LIST { get; set; }
            /// <summary>服務請求【處理與工時紀錄資訊】清單</summary>
            public List<SRRECORDINFO> SRRECORD_LIST { get; set; }
            /// <summary>服務請求【零件更換資訊】清單</summary>
            public List<SRPARTSREPALCEINFO> SRPARTS_LIST { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑SRID相關資訊查詢(服務主檔資訊、客戶聯絡窗口資訊清單、產品序號資訊清單、保固SLA檔資訊清單、處理與工時紀錄清單、零件更換資訊清單) ↑↑↑↑↑-----  

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
                List<string[]> tList = CMF.findSRIDList(pOperationID_GenerallySR, EmpBean.BUKRS, EmpBean.IsManager, EmpBean.EmployeeERPID, tSRTeamList, "61");                
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
                        if (EmpBean.IsManager && tAry[12] != "E0001")
                        {
                            //判斷是主管且【不為L2工程師】且【也不為技術主管】才跳過
                            if (tERPID != tAry[6] && tAry[8].IndexOf(tERPID) == -1)
                            {
                                continue;
                            }
                        }

                        SRTODOLISTINFO beanTODO = new SRTODOLISTINFO();

                        beanTODO.SRID = tAry[0];
                        beanTODO.CUSTOMERNAME = tAry[1];
                        beanTODO.REPAIRNAME = tAry[2];
                        beanTODO.SRDESC = tAry[3];                        
                        beanTODO.PATHWAY = tAry[4];
                        beanTODO.SRTYPE = tAry[5];
                        beanTODO.MAINENGNAME = tAry[7];
                        beanTODO.SLARESP = tAry[9];
                        beanTODO.SLASRV = tAry[10];
                        beanTODO.MODIFDATE = tAry[11];
                        beanTODO.STATUSDESC = tAry[13];

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

        #region -----↓↓↓↓↓員工資料接口 ↓↓↓↓↓-----        

        #region 查詢員工資料接口
        [HttpPost]
        public ActionResult API_EMPLOYEEINFO_GET(EMPLOYEEINFO_INPUT beanIN)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_EMPNAME": "張豐穎"
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

                var tList = CMF.findEMPINFO(beanIN.IV_EMPNAME.Trim());

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

                if (tList.Count == 0)
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

                    foreach (var bean in tList)
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

        #region -----↓↓↓↓↓料號資料接口 ↓↓↓↓↓-----        

        #region 查詢料號資料接口
        [HttpPost]
        public ActionResult API_MATERIALINFO_GET(MATERIALINFO_INPUT beanIV)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_MATERIAL": "507284"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = MATERIALINFO_GET(beanIV);

            return Json(ListOUT);
        }
        #endregion

        #region 取得料號資料
        private OPTION_OUTPUT MATERIALINFO_GET(MATERIALINFO_INPUT beanIN)
        {
            OPTION_OUTPUT OUTBean = new OPTION_OUTPUT();

            try
            {
                var tList = CMF.findMATERIALINFO(beanIN.IV_MATERIAL.Trim());

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
            /// <summary>料號/料號說明</summary>
            public string IV_MATERIAL { get; set; }
        }
        #endregion      

        #endregion -----↑↑↑↑↑服務團隊資料接口 ↑↑↑↑↑-----  

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

        #region 查詢一般服務請求狀態接口
        [HttpPost]
        public ActionResult API_GENERALSRSTATUS_GET(OPTION_INPUT beanIV)
        {
            #region Json範列格式(傳入格式)
            //{
            //    "IV_COMPID": "T012"
            //}
            #endregion

            OPTION_OUTPUT ListOUT = new OPTION_OUTPUT();

            ListOUT = OPTION_GET(beanIV, "GENERALSRSTATUS");

            return Json(ListOUT);
        }
        #endregion       

        #region 取得下拉選項清單
        private OPTION_OUTPUT OPTION_GET(OPTION_INPUT beanIV, string tFunction)
        {
            OPTION_OUTPUT OUTBean = new OPTION_OUTPUT();

            try
            {
                string tFunName = string.Empty;
                string tFunNo = string.Empty;

                switch (tFunction)
                {
                    case "MASERVICETYPE":
                        tFunName = "維護服務種類";
                        tFunNo = "SRMATYPE";
                        break;

                    case "SRPATHWAY":
                        tFunName = "報修管道";
                        tFunNo = "SRPATH";
                        break;

                    case "GENERALSRSTATUS":
                        tFunName = "狀態";
                        tFunNo = "SRSTATUS";
                        break;
                }

                var tList = CMF.findOPTION(pOperationID_GenerallySR, beanIV.IV_COMPID.Trim(), tFunNo);

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

        #endregion -----↑↑↑↑↑CALL RFC接口 ↑↑↑↑↑-----        
    }

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
        /// <summary>服務請求ID</summary>
        public string SRID { get; set; }
        /// <summary>服務請求說明</summary>
        public string SRDESC { get; set; }
        /// <summary>客戶</summary>
        public string CUSTOMERNAME { get; set; }
        /// <summary>客戶報修人</summary>
        public string REPAIRNAME { get; set; }
        /// <summary>報修管道</summary>
        public string PATHWAY { get; set; }
        /// <summary>報修類別</summary>
        public string SRTYPE { get; set; }
        /// <summary>L2工程師</summary>
        public string MAINENGNAME { get; set; }
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

    #region 服務請求主檔資訊
    /// <summary>服務請求主檔資訊</summary>
    public class SRIDINFO
    {
        /// <summary>服務請求ID</summary>
        public string SRID { get; set; }
        /// <summary>服務請求說明</summary>
        public string SRDESC { get; set; }
        /// <summary>服務請求開單日期</summary>
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
        /// <summary>L2工程師ERPID</summary>
        public string MAINENGID { get; set; }
        /// <summary>L2工程師姓名</summary>
        public string MAINENGNAME { get; set; }
        /// <summary>指派工程師姓名</summary>
        public string ASSENGNAME { get; set; }
        /// <summary>技術主管姓名</summary>
        public string TECHMAGNAME { get; set; }
    }
    #endregion

    #region 服務請求主檔資訊(For Mail)
    /// <summary>服務請求主檔資訊(For Mail)</summary>
    public class SRIDMAININFO
    {
        /// <summary>服務請求ID</summary>
        public string SRID { get; set; }
        /// <summary>狀態ID</summary>
        public string Status { get; set; }
        /// <summary>狀態說明(E0001.新建、E0002.L2處理中、E0003.報價中、E0004.3rd Party處理中、E0005.L3處理中、E0006.完修、E0012.HPGCSN 申請、E0013.HPGCSN 完成、E0014.駁回、E0015.取消 )</summary>
        public string StatusDesc { get; set; }
        /// <summary>服務案件種類</summary>
        public string SRCase { get; set; }
        /// <summary>服務團隊</summary>
        public string TeamNAME { get; set; }
        /// <summary>服務團隊主管</summary>
        public string TeamMGR { get; set; }
        /// <summary>L2工程師</summary>
        public string MainENG { get; set; }
        /// <summary>指派工程師</summary>
        public string AssENG { get; set; }
        /// <summary>技術主管</summary>
        public string TechMGR { get; set; }        
        /// <summary>派單時間</summary>
        public string CreatedDate { get; set; }
        /// <summary>合約文件編號</summary>
        public string ContractID { get; set; }
        /// <summary>維護服務種類</summary>
        public string MAServiceType { get; set; }
        /// <summary>是否為二修</summary>
        public string SecFix { get; set; }
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

        /// <summary>服務團隊主管Email</summary>
        public string TeamMGREmail { get; set; }
        /// <summary>L2工程師Email</summary>
        public string MainENGEmail { get; set; }
        /// <summary>指派工程師Email</summary>
        public string AssENGEmail { get; set; }
        /// <summary>技術主管Email</summary>
        public string TechMGREmail { get; set; }
    }
    #endregion

    #region 服務請求客戶聯絡人資訊
    /// <summary>服務請求客戶聯絡人資訊</summary>
    public class SRCONTACTINFO
    {
        /// <summary>服務請求ID</summary>
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

    #region 服務請求產品序號資訊
    /// <summary>服務請求產品序號資訊</summary>
    public class SRSERIALMATERIALINFO
    {
        /// <summary>服務請求ID</summary>
        public string SRID { get; set; }
        /// <summary>序號</summary>
        public string SerialID { get; set; }
        /// <summary>物料代號</summary>
        public string MaterialID { get; set; }
        /// <summary>機器型號</summary>
        public string MaterialName { get; set; }
        /// <summary>製造商零件號碼</summary>
        public string ProductNumber { get; set; }
        /// <summary>裝機號碼</summary>
        public string InstallID { get; set; }
    }
    #endregion

    #region 服務請求保固SLA資訊
    /// <summary>服務請求保固SLA資訊</summary>
    public class SRWTSLAINFO
    {
        /// <summary>服務請求ID</summary>
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

    #region 服務請求處理與工時紀錄資訊
    /// <summary>服務請求處理與工時紀錄資訊</summary>
    public class SRRECORDINFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務請求ID</summary>
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

    #region 服務請求零件更換資訊
    /// <summary>服務請求零件更換資訊</summary>
    public class SRPARTSREPALCEINFO
    {
        /// <summary>系統ID</summary>
        public string CID { get; set; }
        /// <summary>服務請求ID</summary>
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

    #region 服務請求L2工程師/指派工程師/技術主管相關資訊
    /// <summary>服務請求L2工程師/指派工程師/技術主管相關資訊</summary>
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

    #region 服務請求執行條件
    /// <summary>
    /// 服務請求執行條件
    /// </summary>
    public enum SRCondition
    {
        /// <summary>
        /// 新建
        /// </summary>
        ADD,

        /// <summary>
        /// 轉派L2工程師
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
        DONE
    }
    #endregion
}