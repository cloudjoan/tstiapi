﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TSTI_API.Models;

namespace TSTI_API.Controllers
{
    public class APIController : Controller
    {
        TESTEntities testDB = new TESTEntities();
        TSTIONEEntities dbOne = new TSTIONEEntities();

        CommonFunction CMF = new CommonFunction();

        /// <summary>
        /// 程式作業編號檔系統ID(ALL，固定的GUID)
        /// </summary>
        string pSysOperationID = "F8EFC55F-FA77-4731-BB45-2F2147244A2D";

        /// <summary>
        /// 程式作業編號檔系統ID(一般服務)
        /// </summary>
        static string pOperationID_GenerallySR = "869FC989-1049-4266-ABDE-69A9B07BCD0A";

        static string API_KEY = "6xdTlREsMbFd0dBT28jhb5W3BNukgLOos";


        /// <summary>全域變數</summary>
        string pMsg = "";

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

        #region -----↓↓↓↓↓一般服務請求 ↓↓↓↓↓-----

        /// <summary>
        /// 建立ONE SERVICE報修SR（一般服務請求）接口
        /// </summary>
        /// <param name="bean"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GENERALSR_CREATEByAPI(SRMain_GENERALSR_INPUT bean)
        {
            SRMain_GENERALSR_OUTPUT SROUT = new SRMain_GENERALSR_OUTPUT();

            SROUT = SaveGenerallySR(bean, "ADD"); //新增

            return Json(SROUT);
        }

        /// <summary>
        /// 儲存一般服務請求
        /// </summary>
        /// <param name="bean">一般服務請求主檔資訊</param>
        /// <param name="tType">ADD.新增 EDIT.修改</param>
        /// <returns></returns>
        public SRMain_GENERALSR_OUTPUT SaveGenerallySR(SRMain_GENERALSR_INPUT bean, string tType)
        {
            SRMain_GENERALSR_OUTPUT SROUT = new SRMain_GENERALSR_OUTPUT();
            
            
            string pLoginName = string.Empty;
            string pSRID = string.Empty;
            string OldCStatus = string.Empty;
            string tURLName = string.Empty;
            string tSeverName = string.Empty;
            string tInvoiceNo = string.Empty;
            string tInvoiceItem = string.Empty;

            string IV_LOGINACCOUNT = string.IsNullOrEmpty(bean.IV_LOGINACCOUNT) ? "" : bean.IV_LOGINACCOUNT;
            string IV_CUSTOMER = string.IsNullOrEmpty(bean.IV_CUSTOMER) ? "" : bean.IV_CUSTOMER;
            string IV_REPAIRNAME = string.IsNullOrEmpty(bean.IV_REPAIRNAME) ? "" : bean.IV_REPAIRNAME;
            string IV_SRTEAM = string.IsNullOrEmpty(bean.IV_SRTEAM) ? "" : bean.IV_SRTEAM;
            string IV_RKIND = string.IsNullOrEmpty(bean.IV_RKIND) ? "" : bean.IV_RKIND;
            string IV_PATHWAY = string.IsNullOrEmpty(bean.IV_PATHWAY) ? "" : bean.IV_PATHWAY;
            string IV_DESC = string.IsNullOrEmpty(bean.IV_DESC) ? "" : bean.IV_DESC;
            string IV_LTXT = string.IsNullOrEmpty(bean.IV_LTXT) ? "" : bean.IV_LTXT;
            string IV_MKIND1 = string.IsNullOrEmpty(bean.IV_MKIND1) ? "" : bean.IV_MKIND1;
            string IV_MKIND2 = string.IsNullOrEmpty(bean.IV_MKIND2) ? "" : bean.IV_MKIND2;
            string IV_MKIND3 = string.IsNullOrEmpty(bean.IV_MKIND3) ? "" : bean.IV_MKIND3;
            string IV_CONTNAME = string.IsNullOrEmpty(bean.IV_CONTNAME) ? "" : bean.IV_CONTNAME;
            string IV_CONTTEL = string.IsNullOrEmpty(bean.IV_CONTTEL) ? "" : bean.IV_CONTTEL;
            string IV_CONTADDR = string.IsNullOrEmpty(bean.IV_CONTADDR) ? "" : bean.IV_CONTADDR;
            string IV_CONTEMAIL = string.IsNullOrEmpty(bean.IV_CONTEMAIL) ? "" : bean.IV_CONTEMAIL;
            string IV_EMPNO = string.IsNullOrEmpty(bean.IV_EMPNO) ? "" : bean.IV_EMPNO;
            string IV_SQEMPID = string.IsNullOrEmpty(bean.IV_SQEMPID) ? "" : bean.IV_SQEMPID;
            string IV_SERIAL = string.IsNullOrEmpty(bean.IV_SERIAL) ? "" : bean.IV_SERIAL;
            string IV_SNPID = string.IsNullOrEmpty(bean.IV_SNPID) ? "" : bean.IV_SNPID;
            string IV_WTY = string.IsNullOrEmpty(bean.IV_WTY) ? "" : bean.IV_WTY;
            string IV_REFIX = string.IsNullOrEmpty(bean.IV_REFIX) ? "" : bean.IV_REFIX;

            string CCustomerName = CMF.findCustName(IV_CUSTOMER);
            string CSqpersonName = CMF.findSQPersonName(IV_SQEMPID);
            string CMainEngineerName = CMF.findEmployeeName(IV_EMPNO);

            CommonFunction.EmployeeBean EmpBean = new CommonFunction.EmployeeBean();
            EmpBean = CMF.findEmployeeInfo(IV_LOGINACCOUNT);

            pLoginName = EmpBean.EmployeeCName;          

            bool tIsFormal = CMF.getCallSAPERPPara(pOperationID_GenerallySR); //取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)

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
            
            try
            {
                if (tType == "ADD") 
                {
                    #region 新增主檔
                    TB_ONE_SRMain beanM = new TB_ONE_SRMain();

                    pSRID = GetSRID("61");

                    //主表資料
                    beanM.cSRID = pSRID;
                    beanM.cStatus = IV_EMPNO != "" ? "E0005" : "E0001";    //新增時若有主要工程師，則預設為L3.處理中，反之則預設為新建
                    beanM.cCustomerName = CCustomerName;
                    beanM.cCustomerID = IV_CUSTOMER;
                    beanM.cRepairName = IV_REPAIRNAME;
                    beanM.cDesc = IV_DESC;
                    beanM.cNotes = IV_LTXT;
                    beanM.cMAServiceType = IV_RKIND;
                    beanM.cSRTypeOne = IV_MKIND1;
                    beanM.cSRTypeSec = IV_MKIND2;
                    beanM.cSRTypeThr = IV_MKIND3;
                    beanM.cSRPathWay = IV_PATHWAY;
                    beanM.cSRProcessWay = "";
                    beanM.cIsSecondFix = IV_REFIX;
                    beanM.cContacterName = IV_CONTNAME;
                    beanM.cContactAddress = IV_CONTADDR;
                    beanM.cContactPhone = IV_CONTTEL;
                    beanM.cContactEmail = IV_CONTEMAIL;
                    beanM.cTeamID = IV_SRTEAM;
                    beanM.cSQPersonID = IV_SQEMPID;
                    beanM.cSQPersonName = CSqpersonName;
                    beanM.cSalesName = "";
                    beanM.cSalesID = "";
                    beanM.cMainEngineerName = CMainEngineerName;
                    beanM.cMainEngineerID = IV_EMPNO;
                    beanM.cAssEngineerID = "";
                    beanM.cSystemGUID = Guid.NewGuid();

                    beanM.CreatedDate = DateTime.Now;
                    beanM.CreatedUserName = pLoginName;

                    dbOne.TB_ONE_SRMain.Add(beanM);
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
                    #endregion

                    #region 新增【保固SLA資訊】明細
                    int NowCount = 0;

                    List<SRWarranty> QueryToList = new List<SRWarranty>();    //查詢出來的清單    

                    #region 呼叫RFC並回傳保固SLA Table清單
                    if (countPR > 0)
                    {
                        QueryToList = CMF.ZFM_TICC_SERIAL_SEARCHWTYList(PRcSerialID, ref NowCount, tURLName, tSeverName);

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

                        QueryToList = QueryToList.OrderBy(x => x.cSerialID).ThenByDescending(x => x.cWTYEDATE).ToList();
                    }
                    #endregion                   

                    foreach(var beanSR in QueryToList)
                    {
                        TB_ONE_SRDetail_Warranty beanD = new TB_ONE_SRDetail_Warranty();                        

                        beanD.cSRID = pSRID;
                        beanD.cSerialID = beanSR.cSerialID;
                        beanD.cWTYID = beanSR.cWTYID;
                        beanD.cWTYName = beanSR.cWTYName;

                        if (beanSR.cWTYSDATE != "")
                        {
                            beanD.cWTYSDATE = Convert.ToDateTime(beanSR.cWTYSDATE);
                        }

                        if (beanSR.cWTYEDATE != "")
                        {
                           beanD.cWTYEDATE = Convert.ToDateTime(beanSR.cWTYEDATE);
                        }

                        beanD.cSLARESP = beanSR.cSLARESP;
                        beanD.cSLASRV = beanSR.cSLASRV;
                        beanD.cContractID = beanSR.cContractID;
                        beanD.cBPMFormNo = beanSR.cBPMFormNo;

                        #region 判斷是否有指定使用
                        if (beanSR.cWTYID == IV_WTY.Trim())
                        {
                            beanD.cUsed = "Y";
                        }
                        else
                        {
                            beanD.cUsed = beanSR.cUsed;
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
                        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新建失敗" + Environment.NewLine;
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
                    }
                }
                else
                {
                    #region 註解
                    //#region 修改主檔  
                    //    var beanNowM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == pSRID);

                    ////主表資料
                    //OldCStatus = beanNowM.CStatus;

                    //beanNowM.CStatus = CStatus;
                    //beanNowM.CCustomerName = CCustomerName;
                    //beanNowM.CCustomerId = CCustomerId;
                    //beanNowM.CRepairName = CRepairName;
                    //beanNowM.CDesc = CDesc;
                    //beanNowM.CNotes = CNotes;
                    //beanNowM.CMaserviceType = CMaserviceType;
                    //beanNowM.CSrtypeOne = CSrtypeOne;
                    //beanNowM.CSrtypeSec = CSrtypeSec;
                    //beanNowM.CSrtypeThr = CSrtypeThr;
                    //beanNowM.CSrpathWay = CSrpathWay;
                    //beanNowM.CSrprocessWay = CSrprocessWay;
                    //beanNowM.CIsSecondFix = CIsSecondFix;
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
            /// <summary>建立者AD帳號</summary>
            public string IV_LOGINACCOUNT { get; set; }
            /// <summary>客戶ID</summary>
            public string IV_CUSTOMER { get; set; }
            /// <summary>報修人姓名</summary>
            public string IV_REPAIRNAME { get; set; }
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
            /// <summary>聯絡人姓名</summary>
            public string IV_CONTNAME { get; set; }
            /// <summary>聯絡人電話</summary>
            public string IV_CONTTEL { get; set; }
            /// <summary>聯絡人地址</summary>
            public string IV_CONTADDR { get; set; }
            /// <summary>聯絡人Email</summary>
            public string IV_CONTEMAIL { get; set; }
            /// <summary>主要工程師員工編號</summary>
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

        #endregion -----↑↑↑↑↑一般服務請求 ↑↑↑↑↑-----  
    }

    #region 保固SLA資訊
    /// <summary>保固SLA資訊</summary>
    public class SRWarranty
    {
        /// <summary>系統ID</summary>
        public string cID { get; set; }
        /// <summary>序號</summary>
        public string cSerialID { get; set; }
        /// <summary>保固代號</summary>
        public string cWTYID { get; set; }
        /// <summary>保固說明</summary>
        public string cWTYName { get; set; }
        /// <summary>保固開始日期</summary>
        public string cWTYSDATE { get; set; }
        /// <summary>保固結束日期</summary>
        public string cWTYEDATE { get; set; }
        /// <summary>回應條件</summary>
        public string cSLARESP { get; set; }
        /// <summary>服務條件</summary>
        public string cSLASRV { get; set; }
        /// <summary>合約編號</summary>
        public string cContractID { get; set; }
        /// <summary>合約編號Url</summary>
        public string cContractIDUrl { get; set; }
        /// <summary>保固申請(BPM表單編號)</summary>
        public string cBPMFormNo { get; set; }
        /// <summary>保固申請Url(BPM表單編號Url)</summary>
        public string cBPMFormNoUrl { get; set; }
        /// <summary>本次使用</summary>
        public string cUsed { get; set; }
        /// <summary>tr背景顏色Class</summary>
        public string cBGColor { get; set; }
    }
    #endregion
}