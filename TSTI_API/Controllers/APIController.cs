using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
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
            //     "IV_REPAIREMAIL": "elvis.chang@etatung.com",            
            //     "IV_EMPNO": "10001567",
            //     "IV_SQEMPID": "ZC103",
            //     "IV_SERIAL": "SGH33223R6",
            //     "IV_SNPID": "G-654081B21-057",
            //     "IV_WTY": "OM363636",
            //     "IV_REFIX": "N",
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
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
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

        #endregion -----↑↑↑↑↑一般服務案件建立 ↑↑↑↑↑-----    

        #region -----↓↓↓↓↓一般服務案件狀態更新 ↓↓↓↓↓-----

        #region ONE SERVICE（一般服務案件）狀態更新接口
        /// <summary>
        /// ONE SERVICE（一般服務案件）狀態更新接口
        /// </summary>
        /// <param name="bean"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult API_GENERALSRSTATUS_UPDATE(SRMain_GENERALSRSTATUS_INPUT beanIN)
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

        #region 更新（一般服務案件）狀態
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
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
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

                            case "E0013": //HPGCSN 完成
                                tCondition = SRCondition.HPGCSNDONE;
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

        #region 一般服務案件狀態更新INPUT資訊
        /// <summary>一般服務案件狀態更新INPUT資訊</summary>
        public struct SRMain_GENERALSRSTATUS_INPUT
        {
            /// <summary>修改者員工編號ERPID</summary>
            public string IV_LOGINEMPNO { get; set; }
            /// <summary>服務案件ID</summary>
            public string IV_SRID { get; set; }
            /// <summary>服務狀態ID</summary>
            public string IV_STATUS { get; set; }            
        }
        #endregion

        #region 一般服務案件狀態更新OUTPUT資訊
        /// <summary>一般服務案件狀態更新OUTPUT資訊</summary>
        public struct SRMain_GENERALSRSTATUS_OUTPUT
        {            
            /// <summary>消息類型(E.處理失敗 Y.處理成功)</summary>
            public string EV_MSGT { get; set; }
            /// <summary>消息內容</summary>
            public string EV_MSG { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑一般服務案件狀態更新 ↑↑↑↑↑-----    

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
            //    "IV_LOGINEMPNO": "99120894",
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
            //    "IV_LOGINEMPNO": "99120894",
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
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
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
                pLoginName = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
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
            /// <summary>服務案件主檔資訊清單</summary>
            public List<SRIDINFO> SRMAIN_LIST { get; set; }
            /// <summary>服務案件客戶聯絡人資訊</summary>
            public List<SRCONTACTINFO> SRCONTACT_LIST { get; set; }
        }
        #endregion

        #endregion -----↑↑↑↑↑序號相關資訊查詢(產品序號資訊、保固SLA資訊(List)、服務案件資訊(List)、服務案件客戶聯絡人資訊(List)) ↑↑↑↑↑-----  

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
            //    "IV_SRReportType": "NOSIGN",
            //    "IV_SRReportFiles": "FILES" //用form-data傳檔案
            //    "IV_SRReportFileName" : "";
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

        #region 取得處理與工時紀錄相關
        private SRRECORDINFO_OUTPUT SaveSRRECORDINFO(SRRECORDINFO_INPUT beanIN)
        {
            SRRECORDINFO_OUTPUT OUTBean = new SRRECORDINFO_OUTPUT();

            int cID = 0;

            string cSRID = string.Empty;
            string cENGID = string.Empty;
            string cENGNAME = string.Empty;
            string cReceiveTime = string.Empty;
            string cStartTime = string.Empty;
            string cArriveTime = string.Empty;
            string cFinishTime = string.Empty;
            string cDesc = string.Empty;            
            string cSRReport = string.Empty;
            string cReportID = string.Empty;            
            string cSRReportFileName = string.Empty;
            string cPDFPath = string.Empty;         //服務報告書路徑
            string cPDFFileName = string.Empty;     //服務報告書檔名

            try
            {
                bool tIsFormal = CMF.getCallSAPERPPara(pOperationID_GenerallySR); //取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)          

                cID = string.IsNullOrEmpty(beanIN.IV_CID) ? 0 : int.Parse(beanIN.IV_CID);
                cSRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID;
                cENGID = string.IsNullOrEmpty(beanIN.IV_EMPNO) ? "" : beanIN.IV_EMPNO;                
                cReceiveTime = string.IsNullOrEmpty(beanIN.IV_ReceiveTime) ? "" : beanIN.IV_ReceiveTime;
                cStartTime = string.IsNullOrEmpty(beanIN.IV_StartTime) ? "" : beanIN.IV_StartTime;
                cArriveTime = string.IsNullOrEmpty(beanIN.IV_ArriveTime) ? "" : beanIN.IV_ArriveTime;
                cFinishTime = string.IsNullOrEmpty(beanIN.IV_FinishTime) ? "" : beanIN.IV_FinishTime;
                cDesc = string.IsNullOrEmpty(beanIN.IV_Desc) ? "" : beanIN.IV_Desc;
                cSRReportFileName = string.IsNullOrEmpty(beanIN.IV_SRReportFileName) ? "" : beanIN.IV_SRReportFileName;

                #region 取得工程師/技術主管姓名
                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = CMF.findEmployeeInfoByERPID(cENGID);
                
                cENGNAME = EmpBean.EmployeeCName + " " + EmpBean.EmployeeEName;
                #endregion

                if (cID == 0)
                {
                    #region 檔案上傳
                    HttpPostedFileBase[] uploadFiles = beanIN.IV_SRReportFiles;

                    if (beanIN.IV_SRReportType == SRReportType.NOSIGN)
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
                                string path = string.Empty;                                

                                foreach (var upload in uploadFiles)
                                {
                                    fileGuid = Guid.NewGuid();                                   

                                    fileId = fileGuid.ToString();
                                    fileOrgName = upload.FileName;
                                    fileName = fileId + Path.GetExtension(upload.FileName);
                                    path = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                                    upload.SaveAs(path);

                                    picPathList.Add(path);                                    
                                }

                                #region 將圖片轉成一份pdf
                                fileGuid = Guid.NewGuid();
                                cSRReport = fileGuid.ToString() + ",";
                                
                                bool tIsOK = UploadMultPics(picPathList, fileGuid.ToString(), cSRID, cENGNAME, tIsFormal);                               
                                #endregion

                                if (tIsOK)
                                {
                                    #region 設定pdf檔案相關路徑
                                    cReportID = CMF.GetReportSerialID(cSRID);

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
                                    #endregion
                                }
                            }
                            catch (Exception ex)
                            {
                                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "服務報告書/附件上傳失敗原因:" + ex.Message + Environment.NewLine;
                                pMsg += " 失敗行數：" + ex.ToString() + Environment.NewLine;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        if (beanIN.IV_SRReportType == SRReportType.ATTACH)
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
                                    string path = string.Empty;

                                    foreach (var upload in uploadFiles)
                                    {
                                        #region 檔案部份
                                        fileGuid = Guid.NewGuid();

                                        cSRReport += fileGuid.ToString() + ",";

                                        fileId = fileGuid.ToString();
                                        fileOrgName = upload.FileName;
                                        fileName = fileId + Path.GetExtension(upload.FileName);
                                        path = Path.Combine(Server.MapPath("~/REPORT"), fileName);
                                        upload.SaveAs(path);
                                        #endregion

                                        #region table部份                                        
                                        TB_ONE_DOCUMENT bean = new TB_ONE_DOCUMENT();

                                        bean.ID = fileGuid;
                                        bean.FILE_ORG_NAME = fileOrgName;
                                        bean.FILE_NAME = fileName;
                                        bean.FILE_EXT = Path.GetExtension(upload.FileName);
                                        bean.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                        dbOne.TB_ONE_DOCUMENT.Add(bean);
                                        #endregion
                                    }
                                }
                                catch (Exception ex)
                                {
                                    pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "【ATTACH】服務報告書/附件上傳失敗原因:" + ex.Message + Environment.NewLine;
                                    pMsg += " 失敗行數：" + ex.ToString() + Environment.NewLine;
                                }
                            }
                            #endregion
                        }
                        else if (beanIN.IV_SRReportType == SRReportType.SIGN)
                        {
                            #region 有簽名檔
                            cSRReport = cSRReportFileName.Replace(".pdf", "") + ",";

                            cPDFPath = Path.Combine(Server.MapPath("~/REPORT"), cSRReportFileName);
                            cPDFFileName = cSRReportFileName;
                            #endregion
                        }
                    }
                    #endregion

                    #region 新增
                    TB_ONE_SRDetail_Record SRRecord = new TB_ONE_SRDetail_Record();                    

                    TimeSpan Ts = Convert.ToDateTime(cFinishTime) - Convert.ToDateTime(cArriveTime);

                    SRRecord.cSRID = cSRID;
                    SRRecord.cEngineerID = cENGID;
                    SRRecord.cEngineerName = cENGNAME;
                    SRRecord.cReceiveTime = Convert.ToDateTime(cReceiveTime);
                    SRRecord.cStartTime = Convert.ToDateTime(cStartTime);
                    SRRecord.cArriveTime = Convert.ToDateTime(cArriveTime);
                    SRRecord.cFinishTime = Convert.ToDateTime(cFinishTime);
                    SRRecord.cWorkHours = Convert.ToDecimal(Ts.TotalMinutes);
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

                    CMF.callSendReport(pOperationID_GenerallySR, cSRID, cPDFPath, cPDFFileName, cENGNAME, tIsFormal); //呼叫發送服務報告書report給客戶
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
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>
        /// <returns></returns>
        public bool UploadMultPics(List<string> picPathList, string filename, string srId, string mainEgnrName, bool tIsFormal)
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

                    //壓縮照片
                    img.ScaleAbsolute(1200, 1600);
                    doc.SetPageSize(new iTextSharp.text.Rectangle(0, 0, 1200, 1600, 0));
                    
                    //如果圖片是橫的(寬大於長)
                    if (imgW > imgH)
                    {
                        img.RotationDegrees = -90; //counterclockwise逆時針旋轉
                        doc.SetPageSize(new iTextSharp.text.Rectangle(0, 0, img.Height, img.Width, 0));
                    }
                    doc.NewPage();
                    img.SetAbsolutePosition(0, 0);
                    writer.DirectContent.AddImage(img);                   
                }

                doc.Close();
                reValue = true;
                #endregion

                #region 刪除原圖檔
                foreach (var picPath in picPathList)
                {
                    bool result = System.IO.File.Exists(picPath);
                    if (result)
                    {
                        System.IO.File.Delete(picPath);
                    }
                }
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
            /// <summary>產生服務報告書圖檔方式(SIGN.有簽名檔 NOSIGN.無簽名檔 ATTACH.純附件)</summary>
            public SRReportType IV_SRReportType { get; set; }
            /// <summary>服務報告書圖檔</summary>
            public HttpPostedFileBase[] IV_SRReportFiles { get; set; }
            /// <summary>服務報告書檔名(當產生服務報告書圖檔的方式為【SIGN.有簽名檔】時，才需要傳GUID檔名)</summary>
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
            //    "IV_StartTime": "2023-01-18 18:25",
            //    "IV_ArriveTime": "2023-01-18 18:50",
            //    "IV_FinishTime": "2023-01-18 19:50",
            //    "IV_Desc": "TEST處理紀錄",
            //    "IV_CusOpinion": "沒有意見",
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

            string IV_SRID = string.IsNullOrEmpty(beanIN.IV_SRID) ? "" : beanIN.IV_SRID.Trim();
            string IV_StartTime = string.IsNullOrEmpty(beanIN.IV_StartTime) ? "" : beanIN.IV_StartTime.Trim();
            string IV_ArriveTime = string.IsNullOrEmpty(beanIN.IV_ArriveTime) ? "" : beanIN.IV_ArriveTime.Trim();
            string IV_FinishTime = string.IsNullOrEmpty(beanIN.IV_FinishTime) ? "" : beanIN.IV_FinishTime.Trim();
            string IV_Desc = string.IsNullOrEmpty(beanIN.IV_Desc) ? "" : beanIN.IV_Desc.Trim();
            string IV_EMPNO = string.IsNullOrEmpty(beanIN.IV_EMPNO) ? "" : beanIN.IV_EMPNO.Trim();
            string IV_CusOpinion = string.IsNullOrEmpty(beanIN.IV_CusOpinion) ? "" : beanIN.IV_CusOpinion.Trim();
            
            HttpPostedFileBase upload = beanIN.IV_SRReportFile;

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
                //local端測試時暫時註解
                appBean.SRID = IV_SRID;
                appBean.STATE = IV_SRID + " | " + upload.FileName + " | " + IV_StartTime + " | " + IV_ArriveTime + " | " + IV_FinishTime + " | " + IV_Desc + " | " + IV_EMPNO;
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

                #region -- 上傳簽名檔 --
                bool haveSignature = false;
                string pngPath = "";

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

                    PdfPCell hcell1 = new PdfPCell(new iTextSharp.text.Phrase("NO.", ChFont10U));
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

                    List<SNLIST> products = srdetail.ContainsKey("table_ET_SNLIST") ? (List<SNLIST>)srdetail["table_ET_SNLIST"] : null;

                    if (products != null)
                    {
                        //取第一筆機器序號
                        PRDID = products[0].PRDID;
                        SNNO = products[0].SNNO;
                        PRDNUMBER = products[0].PRDNUMBER;
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
                    if (IV_SRID.Substring(0, 2) == "85")
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

                                    iTextSharp.text.Chunk proChunkStart = new iTextSharp.text.Chunk("\n\n服務說明：\n\n", ChFont10);
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

                                        iTextSharp.text.Chunk proChunkStart3 = new iTextSharp.text.Chunk("\n\n服務說明：\n\n", ChFont10);
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

                                        iTextSharp.text.Chunk proChunkStart3 = new iTextSharp.text.Chunk("\n\n服務說明：\n\n", ChFont10);
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
                    if (IV_SRID.Substring(0, 2) == "85" && !string.IsNullOrEmpty(svid) && !string.IsNullOrEmpty(hash))
                    {

                    }
                    //其他照舊，單純顯示處理過程
                    else
                    {
                        PdfPCell scell1 = new PdfPCell(new iTextSharp.text.Phrase("服務說明：\n\n" + IV_Desc, ChFont10));
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

                    string SLASRV = (srdetail["EV_SLASRV"] != null) ? srdetail["EV_SLASRV"].ToString() : "";
                    string WTYKIND = (srdetail["EV_WTYKIND"] != null) ? srdetail["EV_WTYKIND"].ToString() : "";
                    IV_CusOpinion = (String.IsNullOrEmpty(IV_CusOpinion) || String.Compare(IV_CusOpinion, "null", true) == 0) ? "" : IV_CusOpinion;

                    PdfPCell ccell3 = new PdfPCell(new iTextSharp.text.Phrase("客戶意見 / 備註：\n\n" + IV_CusOpinion + "\n\n" + SLASRV + "\n\n" + WTYKIND, ChFont10));
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
                    if (IV_SRID.Substring(0, 2) == "85")
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

                        string cReportID = CMF.GetReportSerialID(IV_SRID);

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
            /// <summary>簽名圖檔</summary>
            public HttpPostedFileBase IV_SRReportFile { get; set; }            
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

        #region 取得零件更換資訊相關
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

        #region 查詢一般服務案件狀態接口
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
        /// <summary>服務案件ID</summary>
        public string SRID { get; set; }
        /// <summary>服務案件說明</summary>
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

    #region 服務案件客戶聯絡人資訊
    /// <summary>服務案件客戶聯絡人資訊</summary>
    public class SRCONTACTINFO
    {
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
        /// <summary>服務案件ID</summary>
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

    #region 服務案件保固SLA資訊
    /// <summary>服務案件保固SLA資訊</summary>
    public class SRWTSLAINFO
    {
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

    #region 服務案件L2工程師/指派工程師/技術主管相關資訊
    /// <summary>服務案件L2工程師/指派工程師/技術主管相關資訊</summary>
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
        DONE
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
        /// 純附件
        /// </summary>
        ATTACH
    }
    #endregion
}