using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using SAP.Middleware.Connector;
using TSTI_API.Models;

namespace TSTI_API.Controllers
{
    public class CommonFunction
    {
        TSTIONEEntities dbOne = new TSTIONEEntities();
        MCSWorkflowEntities dbEIP = new MCSWorkflowEntities();
        ERP_PROXY_DBEntities dbProxy = new ERP_PROXY_DBEntities();
        PSIPEntities dbPSIP = new PSIPEntities();
        BIEntities dbBI = new BIEntities();

        /// <summary>全域變數</summary>
        string pMsg = "";

        public CommonFunction()
        {

        }

        #region 取得登入者資訊(傳入AD帳號)
        /// <summary>
        /// 取得登入者資訊(傳入AD帳號)
        /// </summary>
        /// <param name="keyword">AD帳號</param>
        /// <returns></returns>
        public EmployeeBean findEmployeeInfoByAccount(string keyword)
        {
            var beanE = dbEIP.Person.FirstOrDefault(x => x.Account.ToLower() == keyword.ToLower() && (x.Leave_Date == null && x.Leave_Reason == null));

            EmployeeBean empBean = findEmployeeInfo(beanE);

            return empBean;
        }
        #endregion

        #region 取得登入者資訊(傳入ERPID)
        /// <summary>
        /// 取得登入者資訊(傳入ERPID)
        /// </summary>
        /// <param name="keyword">ERPID</param>
        /// <returns></returns>
        public EmployeeBean findEmployeeInfoByERPID(string keyword)
        {
            var beanE = dbEIP.Person.FirstOrDefault(x => x.ERP_ID == keyword && (x.Leave_Date == null && x.Leave_Reason == null));

            EmployeeBean empBean = findEmployeeInfo(beanE);

            return empBean;
        }
        #endregion

        #region 取得登入者資訊
        /// <summary>
        /// 取得登入者資訊
        /// </summary>
        /// <param name="keyword">ERPID</param>
        /// <returns></returns>
        public EmployeeBean findEmployeeInfo(Person beanE)
        {
            EmployeeBean empBean = new EmployeeBean();

            bool tIsManager = false;            

            if (beanE != null)
            {
                empBean.EmployeeNO = beanE.Account.Trim();
                empBean.EmployeeERPID = beanE.ERP_ID.Trim();
                empBean.EmployeeCName = beanE.Name2.Trim();
                empBean.EmployeeEName = beanE.Name.Trim();
                empBean.WorkPlace = beanE.Work_Place.Trim();
                empBean.PhoneExt = beanE.Extension.Trim();
                empBean.CompanyCode = beanE.Comp_Cde.Trim();
                empBean.BUKRS = getBUKRS(beanE.Comp_Cde.Trim());
                empBean.EmployeeEmail = beanE.Email.Trim();
                empBean.EmployeePersonID = beanE.ID.Trim();

                #region 取得部門資訊
                var beanD = dbEIP.Department.FirstOrDefault(x => x.ID == beanE.DeptID);

                if (beanD != null)
                {
                    empBean.DepartmentNO = beanD.ID.Trim();
                    empBean.DepartmentName = beanD.Name2.Trim();
                    empBean.ProfitCenterID = beanD.ProfitCenterID.Trim();
                    empBean.CostCenterID = beanD.CostCenterID.Trim();
                }
                #endregion

                #region 取得是否為主管
                var beansManager = dbEIP.Department.Where(x => x.ManagerID == beanE.ID && x.Status == 0);

                if (beansManager.Count() > 0)
                {
                    tIsManager = true;
                }

                empBean.IsManager = tIsManager;
                #endregion
            }

            return empBean;
        }
        #endregion

        #region 取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)
        /// <summary>
        /// 取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>
        /// <returns></returns>
        public bool getCallSAPERPPara(string cOperationID)
        {
            bool reValue = false;

            string tValue = findSysParameterValue(cOperationID, "OTHER", "ALL", "SAPERP");

            reValue = Convert.ToBoolean(tValue);

            return reValue;
        }
        #endregion

        #region 取得系統參數清單
        /// <summary>
        /// 取得系統參數清單
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>
        /// <param name="cFunctionID">功能別(OTHER.其他自定義)</param>
        /// <param name="cCompanyID">公司別</param>
        /// <param name="cNo">參數No</param>        
        /// <returns></returns>
        public List<SelectListItem> findSysParameterList(string cOperationID, string cFunctionID, string cCompanyID, string cNo)
        {
            var tList = findSysParameterListItem(cOperationID, cFunctionID, cCompanyID, cNo);

            return tList;
        }
        #endregion

        #region 取得【資訊系統參數設定檔】的參數值清單(回傳SelectListItem)
        /// <summary>
        /// 取得【資訊系統參數設定檔】的參數值清單(回傳SelectListItem)
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>
        /// <param name="cFunctionID">功能別(OTHER.其他自定義)</param>
        /// <param name="cCompanyID">公司別(ALL.全集團、T012.大世科、T016.群輝、C069.大世科技上海、T022.協志科)</param>
        /// <param name="cNo">參數No</param>        
        /// <returns></returns>
        public List<SelectListItem> findSysParameterListItem(string cOperationID, string cFunctionID, string cCompanyID, string cNo)
        {
            var tList = new List<SelectListItem>();
            List<string> tTempList = new List<string>();

            string tKEY = string.Empty;
            string tNAME = string.Empty;

            var beans = dbPSIP.TB_ONE_SysParameter.OrderBy(x => x.cOperationID).ThenBy(x => x.cFunctionID).ThenBy(x => x.cCompanyID).OrderBy(x => x.cNo).
                                               Where(x => x.Disabled == 0 &&
                                                          x.cOperationID.ToString() == cOperationID &&
                                                          x.cFunctionID == cFunctionID.Trim() &&
                                                          x.cCompanyID == cCompanyID.Trim() &&
                                                          x.cNo == cNo.Trim());          

            foreach (var bean in beans)
            {
                if (!tTempList.Contains(bean.cValue))
                {
                    tList.Add(new SelectListItem { Text = bean.cDescription, Value = bean.cValue });
                    tTempList.Add(bean.cValue);
                }
            }

            return tList;
        }
        #endregion

        #region 取得【資訊系統參數設定檔】的參數值
        /// <summary>
        /// 取得【資訊系統參數設定檔】的參數值
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>
        /// <param name="cFunctionID">功能別(SENDMAIL.寄送Mail、ACCOUNT.取得人員帳號、OTHER.其他自定義)</param>
        /// <param name="cCompanyID">公司別(ALL.全集團、T012.大世科、T016.群輝、C069.大世科技上海、T022.協志科)</param>
        /// <param name="cNo">參數No</param>
        /// <returns></returns>
        public string findSysParameterValue(string cOperationID, string cFunctionID, string cCompanyID, string cNo)
        {
            string reValue = string.Empty;

            var bean = dbPSIP.TB_ONE_SysParameter.FirstOrDefault(x => x.Disabled == 0 &&
                                                                 x.cOperationID.ToString() == cOperationID &&
                                                                 x.cFunctionID == cFunctionID.Trim() &&
                                                                 x.cCompanyID == cCompanyID.Trim() &&
                                                                 x.cNo == cNo.Trim());

            if (bean != null)
            {
                reValue = bean.cValue;
            }

            return reValue;
        }
        #endregion

        #region 取得【資訊系統參數設定檔】的參數值說明
        /// <summary>
        /// 取得【資訊系統參數設定檔】的參數值
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>
        /// <param name="cFunctionID">功能別(SENDMAIL.寄送Mail、ACCOUNT.取得人員帳號、OTHER.其他自定義)</param>
        /// <param name="cCompanyID">公司別(ALL.全集團、T012.大世科、T016.群輝、C069.大世科技上海、T022.協志科)</param>
        /// <param name="cNo">參數No</param>
        /// <param name="cValue">參數值</param>
        /// <returns></returns>
        public string findSysParameterDescription(string cOperationID, string cFunctionID, string cCompanyID, string cNo, string cValue)
        {
            string reValue = string.Empty;

            var bean = dbPSIP.TB_ONE_SysParameter.FirstOrDefault(x => x.Disabled == 0 &&
                                                                 x.cOperationID.ToString() == cOperationID &&
                                                                 x.cFunctionID == cFunctionID.Trim() &&
                                                                 x.cCompanyID == cCompanyID.Trim() &&
                                                                 x.cNo == cNo.Trim() &&
                                                                 x.cValue == cValue.Trim());

            if (bean != null)
            {
                reValue = bean.cDescription;
            }

            return reValue;
        }
        #endregion        

        #region 取得SAP的公司別
        /// <summary>
        /// 取得SAP的公司別(T012、T016、C069、T022)
        /// </summary>
        /// <param name="tCompCode">公司別(Comp-1、Comp-2、Comp-3、Comp-4)</param>
        /// <returns></returns>
        public string getBUKRS(string tCompCode)
        {
            string reValue = "T012";

            switch (tCompCode.Trim())
            {
                case "Comp-1":
                    reValue = "T012";
                    break;
                case "Comp-2":
                    reValue = "T016";
                    break;
                case "Comp-3":
                    reValue = "C069";
                    break;
                case "Comp-4":
                    reValue = "T022";
                    break;
            }

            return reValue;
        }
        #endregion

        #region 取得客戶名稱
        /// <summary>
        /// 取得客戶名稱
        /// </summary>
        /// <param name="keyword">客戶代號</param>
        /// <returns></returns>
        public string findCustName(string keyword)
        {
            string reValue = string.Empty;

            if (keyword != "")
            {
                var bean = dbProxy.VIEW_CUSTOMER_2.FirstOrDefault(x => x.KNA1_KUNNR == keyword.Trim());

                if (bean != null)
                {
                    reValue = bean.KNA1_NAME1;
                }
            }

            return reValue;
        }
        #endregion

        #region 取得法人客戶資料
        /// <summary>
        /// 取得法人客戶資料
        /// </summary>
        /// <param name="keyword">客戶代號/客戶名稱</param>
        /// <returns></returns>
        public List<VIEW_CUSTOMER_2> findCUSTOMERINFO(string keyword)
        {
            List<VIEW_CUSTOMER_2> tList = new List<VIEW_CUSTOMER_2>();

            if (keyword != "")
            {
                tList = dbProxy.VIEW_CUSTOMER_2.Where(x => x.KNA1_KUNNR.Contains(keyword.Trim()) || x.KNA1_NAME1.Contains(keyword.Trim())).Take(30).ToList();               
            }

            return tList;
        }
        #endregion

        #region 取得法人客戶聯絡人資料
        /// <summary>
        /// 取得法人客戶聯絡人資料
        /// </summary>
        /// <param name="CustomerID">客戶代號</param>
        /// <param name="CONTACTNAME">聯絡人姓名</param>        
        /// <param name="CONTACTTEL">聯絡人電話</param>
        /// <param name="CONTACTMOBILE">聯絡人手機</param>
        /// <param name="CONTACTEMAIL">聯絡人Email</param>
        /// <returns></returns>
        public List<PCustomerContact> findCONTACTINFO(string CustomerID, string CONTACTNAME, string CONTACTTEL, string CONTACTMOBILE, string CONTACTEMAIL)
        {
            var qPjRec = dbProxy.CUSTOMER_Contact.OrderByDescending(x => x.ModifiedDate).
                                               Where(x => (x.Disabled == null || x.Disabled != 1) && x.KNA1_KUNNR == CustomerID &&
                                                          x.ContactName != "" && x.ContactCity != "" &&
                                                          x.ContactAddress != "" && x.ContactPhone != "" &&
                                                          (string.IsNullOrEmpty(CONTACTNAME) ? true : x.ContactName.Contains(CONTACTNAME)) &&
                                                          (string.IsNullOrEmpty(CONTACTTEL) ? true : x.ContactPhone.Contains(CONTACTTEL)) &&
                                                          (string.IsNullOrEmpty(CONTACTMOBILE) ? true : x.ContactMobile.Contains(CONTACTMOBILE)) &&
                                                          (string.IsNullOrEmpty(CONTACTEMAIL) ? true : x.ContactEmail.Contains(CONTACTEMAIL))).ToList();

            List<string> tTempList = new List<string>();

            string tTempValue = string.Empty;
            string ContactMobile = string.Empty;

            List<PCustomerContact> liPCContact = new List<PCustomerContact>();
            if (qPjRec != null && qPjRec.Count() > 0)
            {
                foreach (var prBean in qPjRec)
                {
                    tTempValue = prBean.KNA1_KUNNR.Trim().Replace(" ", "") + "|" + prBean.KNB1_BUKRS.Trim().Replace(" ", "") + "|" + prBean.ContactEmail.Trim().Replace(" ", "");

                    if (!tTempList.Contains(tTempValue)) //判斷客戶ID、公司別、聯絡人名姓名不重覆才要顯示
                    {
                        tTempList.Add(tTempValue);

                        ContactMobile = string.IsNullOrEmpty(prBean.ContactMobile) ? "" : prBean.ContactMobile.Trim().Replace(" ", "");

                        PCustomerContact prDocBean = new PCustomerContact();

                        prDocBean.ContactID = prBean.ContactID.ToString();
                        prDocBean.CustomerID = prBean.KNA1_KUNNR.Trim().Replace(" ", "");
                        prDocBean.CustomerName = prBean.KNA1_NAME1.Trim().Replace(" ", "");
                        prDocBean.BUKRS = prBean.KNB1_BUKRS.Trim().Replace(" ", "");
                        prDocBean.Name = prBean.ContactName.Trim().Replace(" ", "");
                        prDocBean.City = prBean.ContactCity.Trim().Replace(" ", "");
                        prDocBean.Address = prBean.ContactAddress.Trim().Replace(" ", "");
                        prDocBean.Email = prBean.ContactEmail.Trim().Replace(" ", "");
                        prDocBean.Phone = prBean.ContactPhone.Trim().Replace(" ", "");
                        prDocBean.Mobile = ContactMobile;
                        prDocBean.BPMNo = prBean.BpmNo.Trim().Replace(" ", "");

                        liPCContact.Add(prDocBean);
                    }
                }
            }

            return liPCContact;
        }
        #endregion

        #region 取得員工資料
        /// <summary>
        /// 取得員工資料
        /// </summary>
        /// <param name="keyword">員工姓名(中文名/英文名)</param>
        /// <returns></returns>
        public List<Person> findEMPINFO(string keyword)
        {
            List<Person> tList = new List<Person>();

            if (keyword != "")
            {
                tList = dbEIP.Person.Where(x => (x.Leave_Date == null && x.Leave_Reason == null) && (x.Name.Contains(keyword) || x.Name2.Contains(keyword))).Take(10).ToList();
            }

            return tList;
        }
        #endregion

        #region 取得服務團隊資料
        /// <summary>
        /// 取得服務團隊資料
        /// </summary>        
        /// <param name="tCompanyID">公司別(T012、T016、C069、T022)</param>
        /// <returns></returns>        
        public List<TB_ONE_SRTeamMapping> findSRTEAMINFO(string tCompanyID)
        {
            List<TB_ONE_SRTeamMapping> tList = new List<TB_ONE_SRTeamMapping>();

            if (tCompanyID == "T012")
            {
                tList = dbOne.TB_ONE_SRTeamMapping.Where(x => x.Disabled == 0 && !(x.cTeamOldID.Contains("SRV.1217") || x.cTeamOldID.Contains("SRV.1227") || x.cTeamOldID.Contains("SRV.1237"))).ToList();
            }
            else if (tCompanyID == "T016")
            {
                tList = dbOne.TB_ONE_SRTeamMapping.Where(x => x.Disabled == 0 && (x.cTeamOldID.Contains("SRV.1217") || x.cTeamOldID.Contains("SRV.1227") || x.cTeamOldID.Contains("SRV.1237"))).ToList();
            }

            return tList;
        }
        #endregion

        #region 取得服務團隊對照組織相關資訊
        /// <summary>
        /// 取得服務團隊對照組織相關資訊
        /// </summary>
        /// <param name="cTeamID">服務團隊ID(多筆以;號隔開)</param>
        /// <returns></returns>        
        public List<SRTEAMORGINFO> findSRTEAMORGINFO(string cTeamID)
        {
            List<SRTEAMORGINFO> tList = new List<SRTEAMORGINFO>();            

            string[] AryTeamID = cTeamID.TrimEnd(';').Split(';');

            var beans = dbOne.TB_ONE_SRTeamMapping.Where(x => x.Disabled == 0 && AryTeamID.Contains(x.cTeamOldID));

            foreach(var bean in beans)
            {
                SRTEAMORGINFO SRTeam = new SRTEAMORGINFO();

                string[] AryMGRInfo = findDeptMGRInfo(bean.cTeamNewID);

                SRTeam.TEAMID = bean.cTeamOldID;
                SRTeam.TEAMNAME = bean.cTeamOldName;
                SRTeam.DEPTID = bean.cTeamNewID;
                SRTeam.DEPTNAME = bean.cTeamNewName;
                SRTeam.DEPTMGRERPID = AryMGRInfo[0];
                SRTeam.DEPTMGRACCOUNT = AryMGRInfo[1];
                SRTeam.DEPTMGRNAME = AryMGRInfo[2];
                SRTeam.DEPTMGREMAIL = AryMGRInfo[3];

                tList.Add(SRTeam);
            }

            return tList;
        }
        #endregion

        #region 取得服務團隊名稱
        /// <summary>
        /// 取得服務團隊名稱
        /// </summary>
        /// <param name="SRTeam">服務團隊對照組織相關資訊清單</param>
        /// <returns></returns>
        public string findSRTeamName(List<SRTEAMORGINFO> SRTeam)
        {
            string reValue = string.Empty;          

            foreach(var bean in SRTeam)
            {
                reValue += bean.TEAMNAME + ";";
            }

            reValue = reValue.TrimEnd(';');

            return reValue;
        }
        #endregion

        #region 取得服務團隊主管姓名
        /// <summary>
        /// 取得服務團隊主管姓名
        /// </summary>
        /// <param name="SRTeam">服務團隊對照組織相關資訊清單</param>
        /// <returns></returns>
        public string findSRTeamMGRName(List<SRTEAMORGINFO> SRTeam)
        {
            string reValue = string.Empty;

            foreach (var bean in SRTeam)
            {
                reValue += bean.DEPTMGRNAME + ";";
            }

            reValue = reValue.TrimEnd(';');

            return reValue;
        }
        #endregion

        #region 取得服務團隊主管Email
        /// <summary>
        /// 取得服務團隊主管Email
        /// </summary>
        /// <param name="SRTeam">服務團隊對照組織相關資訊清單</param>
        /// <returns></returns>
        public string findSRTeamMGREmail(List<SRTEAMORGINFO> SRTeam)
        {
            string reValue = string.Empty;

            foreach (var bean in SRTeam)
            {
                reValue += bean.DEPTMGREMAIL + ";";
            }

            reValue = reValue.TrimEnd(';');

            return reValue;
        }
        #endregion

        #region 取得服務請求L2工程師/指派工程師/技術主管相關資訊
        /// <summary>
        /// 服務請求L2工程師/指派工程師/技術主管相關資訊
        /// </summary>
        /// <param name="cERPID">員工編號(多筆以;號隔開)</param>
        /// <returns></returns>        
        public List<SREMPINFO> findSREMPINFO(string cERPID)
        {
            List<SREMPINFO> tList = new List<SREMPINFO>();

            string[] AryERPID = cERPID.TrimEnd(';').Split(';');

            var beans = dbEIP.Person.Where(x => (x.Leave_Date == null && x.Leave_Reason == null) && AryERPID.Contains(x.ERP_ID));

            foreach (var bean in beans)
            {
                SREMPINFO SREmp = new SREMPINFO();

                SREmp.ERPID = bean.ERP_ID;
                SREmp.ACCOUNT = bean.Account;
                SREmp.NAME = bean.Name2 + " " + bean.Name;
                SREmp.EMAIL = bean.Email;

                tList.Add(SREmp);
            }

            return tList;
        }
        #endregion

        #region 取得服務請求L2工程師/指派工程師/技術主管姓名
        /// <summary>
        /// 取得服務請求L2工程師/指派工程師/技術主管姓名
        /// </summary>
        /// <param name="SREmp">服務請求L2工程師/指派工程師/技術主管相關資訊清單</param>
        /// <returns></returns>
        public string findSREMPName(List<SREMPINFO> SREmp)
        {
            string reValue = string.Empty;

            foreach (var bean in SREmp)
            {
                reValue += bean.NAME + ";";
            }

            reValue = reValue.TrimEnd(';');

            return reValue;
        }
        #endregion

        #region 取得服務請求L2工程師/指派工程師/技術主管Email
        /// <summary>
        /// 取得服務請求L2工程師/指派工程師/技術主管Email
        /// </summary>
        /// <param name="SREmp">服務請求L2工程師/指派工程師/技術主管相關資訊清單</param>
        /// <returns></returns>
        public string findSREMPEmail(List<SREMPINFO> SREmp)
        {
            string reValue = string.Empty;

            foreach (var bean in SREmp)
            {
                reValue += bean.EMAIL + ";";
            }

            reValue = reValue.TrimEnd(';');

            return reValue;
        }
        #endregion

        #region 取得服務案件種類說明
        /// <summary>
        /// 取得服務案件種類說明
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <returns></returns>
        public string findSRIDType(string cSRID)
        {
            string reValue = string.Empty;

            switch (cSRID.Substring(0,2))
            {
                case "61":
                    reValue = "一般服務";
                    break;

                case "63":
                    reValue = "裝機服務";
                    break;

                case "65":
                    reValue = "定維服務";
                    break;
            }

            return reValue;
        }
        #endregion

        #region 取得該部門主管相關資訊
        /// <summary>
        /// 取得該部門主管相關資訊
        /// </summary>
        /// <param name="DEPTID">部門ID</param>        
        /// <returns></returns>
        public string[] findDeptMGRInfo(string DEPTID)
        {
            string[] reValue = new string[4];

            string tManagerID = string.Empty;

            var beanDept = dbEIP.Department.FirstOrDefault(x => x.ID == DEPTID);

            if (beanDept != null)
            {
                tManagerID = beanDept.ManagerID;

                if (tManagerID != "")
                {
                    var beanP = dbEIP.Person.FirstOrDefault(x => x.ID == tManagerID);

                    if (beanP != null)
                    {
                        reValue[0] = beanP.ERP_ID;                      //部門主管ERPID
                        reValue[1] = beanP.Account;                     //部門主管帳號
                        reValue[2] = beanP.Name2 + " " + beanP.Name;     //部門主管姓名
                        reValue[3] = beanP.Email;                       //部門主管Email
                    }
                }
            }

            return reValue;
        }
        #endregion 

        #region 取得所有第一階List清單(報修類別)
        /// <summary>
        /// 取得所有第一階List清單(報修類別)
        /// </summary>
        /// <param name="tCompanyID">公司別(T012、T016、C069、T022)</param>
        /// <returns></returns>
        public List<SelectListItem> findFirstKINDList(string tCompanyID)
        {
            List<string> tTempList = new List<string>();

            string tKIND_KEY = string.Empty;
            string tKIND_NAME = string.Empty;

            var beansKIND = dbOne.TB_ONE_SRRepairType.OrderBy(x => x.cKIND_KEY).Where(x => x.Disabled == 0 && x.cUP_KIND_KEY == "0");

            var tList = new List<SelectListItem>();            

            foreach (var bean in beansKIND)
            {
                if (!tTempList.Contains(bean.cKIND_KEY))
                {
                    tList.Add(new SelectListItem { Text = bean.cKIND_NAME, Value = bean.cKIND_KEY });
                    tTempList.Add(bean.cKIND_KEY);
                }
            }

            return tList;
        }
        #endregion

        #region 傳入第一階(大類)並取得第二階(中類)清單
        /// <summary>
        /// 傳入第一階(大類)並取得第二階(中類)清單
        /// </summary>
        /// <param name="tCompanyID">公司別(T012、T016、C069、T022)</param>
        /// <param name="keyword">第一階(大類)代碼</param>
        /// <returns></returns>
        public List<SelectListItem> findSRTypeSecList(string tCompanyID, string keyword)
        {
            List<string> tTempList = new List<string>();

            string tKIND_KEY = string.Empty;
            string tKIND_NAME = string.Empty;

            var beansKIND = dbOne.TB_ONE_SRRepairType.OrderBy(x => x.cKIND_KEY).Where(x => x.Disabled == 0 && x.cKIND_LEVEL == 2 && x.cUP_KIND_KEY == keyword);

            var tList = new List<SelectListItem>();            

            foreach (var bean in beansKIND)
            {
                if (!tTempList.Contains(bean.cKIND_KEY))
                {
                    tList.Add(new SelectListItem { Text = bean.cKIND_NAME, Value = bean.cKIND_KEY });
                    tTempList.Add(bean.cKIND_KEY);
                }
            }

            return tList;
        }
        #endregion

        #region 傳入第二階(中類)並取得第三階(小類)清單
        /// <summary>
        /// 傳入第二階(中類)並取得第三階(小類)清單
        /// </summary>
        /// <param name="tCompanyID">公司別(T012、T016、C069、T022)</param>
        /// <param name="keyword">第二階(中類)代碼</param>
        /// <returns></returns>
        public List<SelectListItem> findSRTypeThrList(string tCompanyID, string keyword)
        {
            List<string> tTempList = new List<string>();

            string tKIND_KEY = string.Empty;
            string tKIND_NAME = string.Empty;

            var beansKIND = dbOne.TB_ONE_SRRepairType.OrderBy(x => x.cKIND_KEY).Where(x => x.Disabled == 0 && x.cKIND_LEVEL == 3 && x.cUP_KIND_KEY == keyword);

            var tList = new List<SelectListItem>();            

            foreach (var bean in beansKIND)
            {
                if (!tTempList.Contains(bean.cKIND_KEY))
                {
                    tList.Add(new SelectListItem { Text = bean.cKIND_NAME, Value = bean.cKIND_KEY });
                    tTempList.Add(bean.cKIND_KEY);
                }
            }

            return tList;
        }
        #endregion      

        #region 取得SQ人員資料
        /// <summary>
        /// 取得SQ人員資料
        /// </summary>
        /// <param name="keyword">員工姓名(中文名/英文名)</param>
        /// <returns></returns>
        public List<TB_ONE_SRSQPerson> findSQINFO(string keyword)
        {
            List<TB_ONE_SRSQPerson> tList = new List<TB_ONE_SRSQPerson>();

            if (keyword != "")
            {
                tList = dbOne.TB_ONE_SRSQPerson.Where(x => x.Disabled == 0 & (x.cFullKEY.Contains(keyword.Trim()) || x.cFullNAME.Contains(keyword.Trim()))).Take(10).ToList();
            }

            return tList;
        }
        #endregion

        #region 取得料號資料
        /// <summary>
        /// 取得料號資料
        /// </summary>
        /// <param name="keyword">料號/料號說明</param>
        /// <returns></returns>
        public List<VIEW_MATERIAL_ByComp> findMATERIALINFO(string keyword)
        {
            List<VIEW_MATERIAL_ByComp> tList = new List<VIEW_MATERIAL_ByComp>();

            if (keyword != "")
            {
                tList = dbProxy.VIEW_MATERIAL_ByComp.Where(x => x.MARA_MATNR.Contains(keyword) || x.MAKT_TXZA1_ZF.Contains(keyword)).Take(8).ToList();
            }

            return tList;
        }
        #endregion

        #region 取得下拉選項List
        /// <summary>
        /// 取得下拉選項List
        /// </summary>
        /// <param name="pOperationID_GenerallySR">程式作業編號檔系統ID(一般服務)</param>
        /// <param name="tCompanyID">公司別(T012、T016、C069、T022)</param>
        /// <param name="tFunNo">功能名稱</param>
        /// <returns></returns>
        public List<SelectListItem> findOPTION(string pOperationID_GenerallySR, string tCompanyID, string tFunNo)
        {   
            List<SelectListItem> ListOPTION = findSysParameterList(pOperationID_GenerallySR, "OTHER", tCompanyID, tFunNo);

            return ListOPTION;
        }
        #endregion

        #region 取得SQ人員名稱
        /// <summary>
        /// 取得SQ人員名稱
        /// </summary>
        /// <param name="keyword">SQ人員ID</param>        
        /// <returns></returns>
        public string findSQPersonName(string keyword)
        {
            string reValue = string.Empty;

            if (keyword != "")
            {
                var bean = dbOne.TB_ONE_SRSQPerson.FirstOrDefault(x => x.Disabled == 0 & x.cFullKEY == keyword.Trim());

                if (bean != null)
                {
                    reValue = bean.cFullNAME;
                }
            }

            return reValue;
        }
        #endregion

        #region 取得產品序號相關資訊
        /// <summary>
        /// 取得產品序號相關資訊
        /// </summary>
        /// <param name="IV_SERIAL">序號ID</param>
        /// <returns></returns>
        public SerialMaterialInfo findMaterialBySerial(string IV_SERIAL)
        {
            SerialMaterialInfo ProBean = new SerialMaterialInfo();

            if (IV_SERIAL != "")
            {
                var bean = dbProxy.STOCKALL.FirstOrDefault(x => x.IV_SERIAL == IV_SERIAL.Trim());
                
                if (bean != null)
                {
                    ProBean.IV_SERIAL = bean.IV_SERIAL;
                    ProBean.ProdID = bean.ProdID;
                    ProBean.Product = bean.Product;
                    ProBean.MFRPN = findMFRPNumber(bean.ProdID);
                    ProBean.InstallNo = findInstallNumber(IV_SERIAL);
                }              
            }

            return ProBean;
        }
        #endregion

        #region 取得人員中文+英文姓名(傳入ERPIID)
        /// <summary>
        /// 取得人員中文+英文姓名(傳入ERPIID)
        /// </summary>
        /// <param name="keyword">ERPID</param>        
        /// <returns></returns>
        public string findEmployeeName(string keyword)
        {
            string reValue = string.Empty;

            if (keyword != "")
            { 
                var bean = dbEIP.Person.FirstOrDefault(x => (x.Leave_Date == null && x.Leave_Reason == null) && x.ERP_ID == keyword.Trim());

                if (bean != null)
                {
                    reValue = bean.Name2 + " " + bean.Name;
                }
            }

            return reValue;
        }
        #endregion       

        #region 取得物料說明
        /// <summary>
        /// 取得物料說明
        /// </summary>
        /// <param name="keyword">關鍵字</param>        
        /// <returns></returns>
        public string findMaterialName(string keyword)
        {
            string reValue = string.Empty;

            if (keyword != "")
            {
                var bean = dbProxy.VIEW_MATERIAL_ByComp.FirstOrDefault(x => x.MARA_MATNR == keyword.Trim());

                if (bean != null)
                {
                    reValue = bean.MAKT_TXZA1_ZF;
                }
            }

            return reValue;
        }
        #endregion

        #region 取得製造商零件號碼
        /// <summary>
        /// 取得製造商零件號碼
        /// </summary>        
        /// <param name="IV_MATERIAL">物料代號</param>
        /// <returns></returns>
        public string findMFRPNumber(string IV_MATERIAL)
        {
            string reValue = string.Empty;

            #region 取得製造商零件號碼
            var beanM = dbProxy.MATERIAL.FirstOrDefault(x => x.MARA_MATNR == IV_MATERIAL.Trim());

            if (beanM != null)
            {
                reValue = beanM.MARA_MFRPN;
            }
            #endregion           

            return reValue;
        }
        #endregion

        #region 取得裝機號碼(83 or 63)
        /// <summary>
        /// 取得裝機號碼(83 or 63)
        /// </summary>        
        /// <param name="IV_SERIAL">序號</param>
        /// <returns></returns>
        public string findInstallNumber(string IV_SERIAL)
        {
            string reValue = string.Empty;

            #region 取得製造商零件號碼
            var beanM = dbPSIP.TB_PIS_INSTALLMaterial.FirstOrDefault(x => x.SRSerial == IV_SERIAL.Trim());

            if (beanM != null)
            {
                reValue = beanM.SRID;
            }
            #endregion           

            return reValue;
        }
        #endregion

        #region 呼叫RFC並回傳SLA Table清單
        /// <summary>
        /// 呼叫RFC並回傳SLA Table清單
        /// </summary>        
        /// <param name="ArySERIAL">序號Array</param>        
        /// <param name="tURLName">BPM站台名稱</param>
        /// <param name="tSeverName">PSIP站台名稱</param>
        /// <param name="tAPIURLName">API站台名稱</param>
        /// <returns></returns>
        public List<SRWarranty> ZFM_TICC_SERIAL_SEARCHWTYList(string[] ArySERIAL, string tURLName, string tSeverName, string tAPIURLName)
        {
            List<SRWarranty> QueryToList = new List<SRWarranty>();

            string cWTYID = string.Empty;
            string cWTYName = string.Empty;
            string cWTYSDATE = string.Empty;
            string cWTYEDATE = string.Empty;
            string cSLARESP = string.Empty;
            string cSLASRV = string.Empty;
            string cContractID = string.Empty;
            string cContractIDURL = string.Empty;
            string cSUB_CONTRACTID = string.Empty;
            string tBPMNO = string.Empty;
            string tURL = string.Empty;
            string tAdvice = string.Empty;
            string tBGColor = "table-success";

            int tLength = 0;
            int pCount = 0;

            try
            {
                var client = new RestClient("http://tsti-sapapp01.etatung.com.tw/api/ticc");

                foreach (string IV_SERIAL in ArySERIAL)
                {
                    if (pCount % 2 == 0)
                    {
                        tBGColor = "";
                    }
                    else
                    {
                        tBGColor = "table-success";
                    }

                    if (IV_SERIAL != null)
                    {
                        var request = new RestRequest();
                        request.Method = RestSharp.Method.Post;

                        Dictionary<Object, Object> parameters = new Dictionary<Object, Object>();
                        parameters.Add("SAP_FUNCTION_NAME", "ZFM_TICC_SERIAL_SEARCH");
                        parameters.Add("IV_SERIAL", IV_SERIAL);

                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", parameters, ParameterType.RequestBody);

                        RestResponse response = client.Execute(request);

                        var data = (JObject)JsonConvert.DeserializeObject(response.Content);

                        tLength = int.Parse(data["ET_WARRANTY"]["Length"].ToString());                          //保固共有幾筆

                        for (int i = 0; i < tLength; i++)
                        {
                            cContractIDURL = "";
                            tBPMNO = "";
                            tURL = "";

                            cWTYID = data["ET_WARRANTY"]["SyncRoot"][i]["wTYCODEField"].ToString().Trim();       //保固
                            cWTYName = data["ET_WARRANTY"]["SyncRoot"][i]["wTYCODEField"].ToString().Trim();     //保固說明
                            cWTYSDATE = data["ET_WARRANTY"]["SyncRoot"][i]["wTYSTARTField"].ToString().Trim();   //保固開始日期
                            cWTYEDATE = data["ET_WARRANTY"]["SyncRoot"][i]["wTYENDField"].ToString().Trim();     //保固結束日期                                                          
                            cSLARESP = data["ET_WARRANTY"]["SyncRoot"][i]["sLASRVField"].ToString().Trim();      //回應條件
                            cSLASRV = data["ET_WARRANTY"]["SyncRoot"][i]["sLARESPField"].ToString().Trim();      //服務條件
                            cContractID = data["ET_WARRANTY"]["SyncRoot"][i]["cONTRACTField"].ToString().Trim(); //合約編號
                            tBPMNO = data["ET_WARRANTY"]["SyncRoot"][i]["bPM_NOField"].ToString().Trim();        //BPM表單編號
                            tAdvice = data["ET_WARRANTY"]["SyncRoot"][i]["aDVICEField"].ToString().Trim();       //客服主管建議

                            #region 取得BPM Url
                            if (cContractID != "")
                            {
                                tBPMNO = findContractSealsFormNo(cContractID);
                                cSUB_CONTRACTID = findContractSealsOBJSubNo(tAPIURLName, cContractID);

                                try
                                {
                                    Int32 ContractID = Int32.Parse(cContractID);

                                    if (ContractID >= 10506151 && ContractID != 10506152 && ContractID != 10506157) //新的用印申請單
                                    {
                                        tURL = "http://" + tURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/ContractSeals/ContractSealsForm.aspx?FormNo=" + tBPMNO;
                                    }
                                    else //舊的用印申請單
                                    {
                                        tURL = "http://" + tURLName + "/ContractSeals/_layouts/FormServer.aspx?XmlLocation=%2fContractSeals%2fBPMContractSealsForm%2f" + tBPMNO + ".xml&ClientInstalled=true&DefaultItemOpen=1&source=/_layouts/TSTI.SharePoint.BPM/CloseWindow.aspx";
                                    }

                                    cContractIDURL = "http://" + tSeverName + "/Spare/QueryContractInfo?CONTRACTID=" + cContractID; //合約編號URL
                                }
                                catch (Exception ex)
                                {
                                    cContractIDURL = "";
                                    tBPMNO = "";
                                    tURL = "";
                                }
                            }
                            else
                            {
                                if (tBPMNO.IndexOf("WTY") != -1)
                                {
                                    tURL = "http://" + tURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/Warranty/WarrantyForm.aspx?FormNo=" + tBPMNO;
                                }
                                else
                                {
                                    tURL = "http://" + tURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Form/Guarantee/GuaranteeForm.aspx?FormNo=" + tBPMNO;
                                }
                            }
                            #endregion

                            #region 取得清單
                            SRWarranty QueryInfo = new SRWarranty();
                            
                            QueryInfo.SERIALID = IV_SERIAL;            //序號                        
                            QueryInfo.WTYID = cWTYID;                  //保固
                            QueryInfo.WTYName = cWTYName;              //保固說明
                            QueryInfo.WTYSDATE = cWTYSDATE;            //保固開始日期
                            QueryInfo.WTYEDATE = cWTYEDATE;            //保固結束日期                                                          
                            QueryInfo.SLARESP = cSLARESP;              //回應條件
                            QueryInfo.SLASRV = cSLASRV;                //服務條件
                            QueryInfo.CONTRACTID = cContractID;        //合約編號
                            QueryInfo.CONTRACTIDUrl = cContractIDURL;  //合約編號Url
                            QueryInfo.SUBCONTRACTID = cSUB_CONTRACTID; //下包文件編號
                            QueryInfo.BPMFormNo = tBPMNO;              //BPM表單編號                        
                            QueryInfo.BPMFormNoUrl = tURL;             //BPM URL
                            QueryInfo.ADVICE = tAdvice;               //客服主管建議                                          
                            QueryInfo.USED = "N";
                            QueryInfo.BGColor = tBGColor;             //tr背景顏色Class

                            QueryToList.Add(QueryInfo);
                            #endregion
                        }
                    }

                    pCount++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return QueryToList;
        }
        #endregion

        #region 傳入ERPID並回傳「中文姓名+英文姓名」，若有多筆以分號隔開
        /// <summary>
        /// 傳入ERPID並回傳「中文姓名+英文姓名」，若有多筆以分號隔開
        /// </summary>
        /// <param name="tERPIDList"></param>
        /// <returns></returns>
        public string findEmployeeCENameByERPID(string tERPIDList)
        {
            string reValue = string.Empty;

            tERPIDList = string.IsNullOrEmpty(tERPIDList) ? "" : tERPIDList;

            if (tERPIDList != "")
            {
                string[] AryERPID = tERPIDList.Split(';');

                foreach(string tERPID in AryERPID)
                {
                    var bean = dbEIP.Person.FirstOrDefault(x => x.ERP_ID == tERPID);

                    if (bean != null)
                    {
                        reValue += bean.Name2 + " " + bean.Name + ";";
                    }
                }

                reValue = reValue.TrimEnd(';');
            }

            return reValue;
        }
        #endregion

        #region 取得服務請求主檔資訊清單
        /// <summary>
        /// 取得服務請求主檔資訊清單
        /// </summary>
        /// <param name="pOperationID_GenerallySR">程式作業編號檔系統ID(一般服務)</param>
        /// <param name="IV_SERIAL">序號</param>
        /// <returns></returns>
        public List<SRIDINFO> findSRMAINList(string pOperationID_GenerallySR, string IV_SERIAL)
        {
            List<SRIDINFO> QuerySRToList = new List<SRIDINFO>();     //查詢出來的清單
            List<string> tListSRID = new List<string>();            //SRID清單

            string tSRTYPE = string.Empty;
            string tSRTDESC = string.Empty;
            string tSTATUSDESC = string.Empty;
            string tSRREPORTUrl = string.Empty;
            string tASSENGNAME = string.Empty;
            string tTECHMAGNAME = string.Empty;

            var beansP = dbOne.TB_ONE_SRDetail_Product.Where(x => x.Disabled == 0 & x.cSerialID == IV_SERIAL);

            foreach(var bean in beansP)
            {
                if (!tListSRID.Contains(bean.cSRID))
                {
                    tListSRID.Add(bean.cSRID);
                }
            }

            foreach(string tSRID in tListSRID)
            {
                var bean = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == tSRID);

                if (bean != null)
                {
                    SRIDINFO SRinfo = new SRIDINFO();

                    switch(tSRID.Substring(0,2))
                    {
                        case "61":
                        case "81":
                            tSRTYPE = "Z01";
                            tSRTDESC = "一般服務";
                            break;

                        case "63":
                        case "83":
                            tSRTYPE = "Z02";
                            tSRTDESC = "裝機服務";
                            break;

                        case "65":
                        case "85":
                            tSRTYPE = "Z03";
                            tSRTDESC = "定維服務";
                            break;
                    }

                    tSTATUSDESC = findSysParameterDescription(pOperationID_GenerallySR, "OTHER", "T012", "SRSTATUS", bean.cStatus);
                    tSRREPORTUrl = findSRReportURL(tSRID);
                    tASSENGNAME = findEmployeeCENameByERPID(bean.cAssEngineerID);
                    tTECHMAGNAME = findEmployeeCENameByERPID(bean.cTechManagerID);

                    SRinfo.SRID = tSRID;
                    SRinfo.SRDESC = bean.cDesc;
                    SRinfo.SRDATE = Convert.ToDateTime(bean.CreatedDate).ToString("yyyy-MM-dd HH:mm:ss");
                    SRinfo.SRTYPE = tSRTYPE;
                    SRinfo.SRTDESC = tSRTDESC;
                    SRinfo.STATUS = bean.cStatus;
                    SRinfo.STATUSDESC = tSTATUSDESC;
                    SRinfo.SRREPORTUrl = tSRREPORTUrl;
                    SRinfo.MAINENGID = bean.cMainEngineerID;
                    SRinfo.MAINENGNAME = bean.cMainEngineerName;
                    SRinfo.ASSENGNAME = tASSENGNAME;
                    SRinfo.TECHMAGNAME = tTECHMAGNAME;

                    QuerySRToList.Add(SRinfo);
                }
            }

            return QuerySRToList;
        }
        #endregion

        #region 服務請求客戶聯絡人資訊清單
        /// <summary>
        /// 服務請求客戶聯絡人資訊清單
        /// </summary>
        /// <param name="tSRIDList">服務請求ID清單</param>
        /// <returns></returns>
        public List<SRCONTACTINFO> findSRCONTACTList(List<string> tSRIDList)
        {
            List<SRCONTACTINFO> QuerySRToList = new List<SRCONTACTINFO>();     //查詢出來的清單            

            foreach (string tSRID in tSRIDList)
            {
                var beans = dbOne.TB_ONE_SRDetail_Contact.Where(x => x.Disabled == 0 & x.cSRID == tSRID);

                foreach (var bean in beans)
                {
                    SRCONTACTINFO SRinfo = new SRCONTACTINFO();

                    SRinfo.SRID = bean.cSRID;
                    SRinfo.CONTNAME = bean.cContactName;
                    SRinfo.CONTADDR = bean.cContactAddress;
                    SRinfo.CONTTEL = bean.cContactPhone;
                    SRinfo.CONTMOBILE = bean.cContactMobile;
                    SRinfo.CONTEMAIL = bean.cContactEmail;

                    QuerySRToList.Add(SRinfo);
                }
            }

            return QuerySRToList;
        }
        #endregion

        #region 取得工時紀錄檔裡的服務報告書url
        /// <summary>
        /// 取得工時紀錄檔裡的服務報告書url
        /// </summary>
        /// <param name="tSRID"></param>
        /// <returns></returns>
        public string findSRReportURL(string tSRID)
        {
            string reValue = string.Empty;

            var beans = dbOne.TB_ONE_SRDetail_Record.Where(x => x.Disabled == 0 && x.cSRID == tSRID);

            foreach(var bean in beans)
            {
                reValue += bean.cSRReport;
            }

            if (reValue != "")
            {
                reValue = reValue.TrimEnd(',').Replace(",", ";");
            }

            return reValue;
        }
        #endregion

        #region 傳入合約編號並取得BPM用印申請單表單編號
        /// <summary>
        /// 傳入合約編號並取得BPM用印申請單表單編號
        /// </summary>
        /// <returns></returns>
        public string findContractSealsFormNo(string NO)
        {
            string reValue = string.Empty;

            var bean = dbProxy.F4501.FirstOrDefault(x => x.NO == NO);

            if (bean != null)
            {
                if (bean.BPMNo != null)
                {
                    reValue = bean.BPMNo.Trim();
                }
            }

            return reValue;
        }
        #endregion

        #region 傳入合約編號並取得CRM合約標的的下包文件編號
        /// <summary>
        /// 傳入合約編號並取得CRM合約標的的下包文件編號
        /// </summary>
        /// <param name="tAPIURLName">API站台名稱</param>
        /// <param name="NO">合約編號</param>
        /// <returns></returns>
        public string findContractSealsOBJSubNo(string tAPIURLName, string NO)
        {
            string reValue = string.Empty;
            string SUB_CONTRACTID = string.Empty;
            string tURL = tAPIURLName + "/API/API_CONTRACTOBJINFO_GET";

            var client = new RestClient(tURL);

            if (NO != null)
            {
                CONTRACTOBJINFO_OUTPUT OUTBean = new CONTRACTOBJINFO_OUTPUT();

                var request = new RestRequest();
                request.Method = RestSharp.Method.Post;

                Dictionary<Object, Object> parameters = new Dictionary<Object, Object>();
                parameters.Add("IV_CONTRACTID", NO);

                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", parameters, ParameterType.RequestBody);

                RestResponse response = client.Execute(request);

                #region 取得回傳訊息(成功或失敗)
                if (response.Content != null)
                {
                    var data = (JObject)JsonConvert.DeserializeObject(response.Content);

                    OUTBean.EV_MSGT = data["EV_MSGT"].ToString().Trim();
                    OUTBean.EV_MSG = data["EV_MSG"].ToString().Trim();
                    #endregion

                    if (OUTBean.EV_MSGT == "Y")
                    {
                        #region 取得合約標的資料List
                        var tList = (JArray)JsonConvert.DeserializeObject(data["CONTRACTOBJINFO_LIST"].ToString().Trim());

                        if (tList != null)
                        {
                            foreach (JObject bean in tList)
                            {
                                SUB_CONTRACTID = bean["SUB_CONTRACTID"].ToString().Trim();

                                if (SUB_CONTRACTID != "")
                                {
                                    break;
                                }
                            }

                            reValue = SUB_CONTRACTID;
                        }
                        #endregion
                    }
                }                
            }

            return reValue;
        }
        #endregion

        #region 取得RFC相關DataTable
        /// <summary>
        /// 取得RFC相關DataTable
        /// </summary>
        /// <param name="function">傳入的RFC Fcuntion</param>
        /// <param name="tableName">tableName</param>
        /// <returns></returns>
        public DataTable SetRFCDataTable(IRfcFunction function, string tableName)
        {
            IRfcTable rfcTable = function.GetTable(tableName);

            DataTable dt = new DataTable();

            for (int i = 0; i < rfcTable.ElementCount; i++)
            {
                RfcElementMetadata rfcMeta = rfcTable.GetElementMetadata(i);
                dt.Columns.Add(rfcMeta.Name);
            }
            foreach (IRfcStructure row in rfcTable)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < rfcTable.ElementCount; i++)
                {
                    RfcElementMetadata rfcMeta = rfcTable.GetElementMetadata(i);
                    dr[rfcMeta.Name] = row.GetString(rfcMeta.Name);
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }
        #endregion       

        #region -----↓↓↓↓↓待辦清單 ↓↓↓↓↓-----

        #region 取得登入人員所有要負責的SRID
        /// <summary>
        /// 取得登入人員所有要負責的SRID
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>
        /// <param name="cCompanyID">公司別</param>
        /// <param name="IsManager">true.管理員 false.非管理員</param>
        /// <param name="tERPID">登入人員ERPID</param>
        /// <param name="tTeamList">可觀看服務團隊清單</param>
        /// <param name="tType">61.一般服務 63.裝機服務...</param>
        /// <returns></returns>
        public List<string[]> findSRIDList(string cOperationID, string cCompanyID, bool IsManager, string tERPID, List<string> tTeamList, string tType)
        {

            List<string[]> SRIDUserToList = new List<string[]>();   //組SRID清單

            switch (tType)
            {
                case "61":  //一般服務
                    SRIDUserToList = getSRIDLis_Generally(cOperationID, cCompanyID, IsManager, tERPID, tTeamList);
                    break;

                case "63":  //裝機服務

                    break;
            }

            return SRIDUserToList;
        }
        #endregion

        #region 取得一般服務SRID負責清單
        /// <summary>
        /// 取得一般服務SRID負責清單
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>
        /// <param name="cCompanyID">公司別</param>
        /// <param name="IsManager">true.管理員 false.非管理員</param>
        /// <param name="tERPID">登入人員ERPID</param>
        /// <param name="tTeamList">可觀看服務團隊清單</param>
        /// <returns></returns>
        private List<string[]> getSRIDLis_Generally(string cOperationID, string cCompanyID, bool IsManager, string tERPID, List<string> tTeamList)
        {
            List<string[]> SRIDUserToList = new List<string[]>();   //組SRID清單

            string tSRPathWay = string.Empty;           //報修管理
            string tSRType = string.Empty;              //報修類別
            string tMainEngineerID = string.Empty;      //L2工程師ERPID
            string tMainEngineerName = string.Empty;    //L2工程師姓名            
            string cTechManagerID = string.Empty;       //技術主管ERPID            
            string tModifiedDate = string.Empty;        //最後編輯日期
            string tSTATUSDESC = string.Empty;          //狀態說明

            List<TB_ONE_SRMain> beans = new List<TB_ONE_SRMain>();

            List<SelectListItem> ListStatus = findSysParameterList(cOperationID, "OTHER", cCompanyID, "SRSTATUS");

            if (IsManager)
            {
                string tWhere = TrnasTeamListToWhere(tTeamList);

                string tSQL = @"select * from TB_ONE_SRMain
                                   where 
                                   (cStatus <> 'E0015' and cStatus <> 'E0006') and 
                                   (
                                        (
                                            (CMainEngineerId = '{0}') or (cTechManagerID like '%{0}%')
                                        )
                                        {1}
                                   )";

                tSQL = string.Format(tSQL, tERPID, tWhere);

                DataTable dt = getDataTableByDb(tSQL, "dbOne");

                foreach (DataRow dr in dt.Rows)
                {
                    tSRPathWay = TransSRPATH(cOperationID, cCompanyID, dr["cSRPathWay"].ToString());
                    tSRType = TransSRType(dr["cSRTypeOne"].ToString(), dr["cSRTypeSec"].ToString(), dr["cSRTypeThr"].ToString());
                    tMainEngineerID = dr["cMainEngineerID"].ToString();
                    tMainEngineerName = dr["cMainEngineerName"].ToString();
                    cTechManagerID = dr["cTechManagerID"].ToString();
                    tModifiedDate = dr["ModifiedDate"].ToString() != "" ? Convert.ToDateTime(dr["ModifiedDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss") : "";
                    tSTATUSDESC = TransSRSTATUS(ListStatus, dr["cStatus"].ToString());

                    #region 組待處理服務
                    string[] ProcessInfo = new string[12];

                    ProcessInfo[0] = dr["cSRID"].ToString();             //SRID
                    ProcessInfo[1] = dr["cCustomerName"].ToString();      //客戶
                    ProcessInfo[2] = dr["cRepairName"].ToString();        //客戶報修人
                    ProcessInfo[3] = dr["cDesc"].ToString();             //說明
                    ProcessInfo[4] = tSRPathWay;                        //報修管道
                    ProcessInfo[5] = tSRType;                           //報修類別
                    ProcessInfo[6] = tMainEngineerID;                   //L2工程師ERPID
                    ProcessInfo[7] = tMainEngineerName;                 //L2工程師姓名
                    ProcessInfo[8] = cTechManagerID;                    //技術主管ERPID                    
                    ProcessInfo[9] = tModifiedDate;                     //最後編輯日期
                    ProcessInfo[10] = dr["cStatus"].ToString();           //狀態
                    ProcessInfo[11] = tSTATUSDESC;                      //狀態+狀態說明

                    SRIDUserToList.Add(ProcessInfo);
                    #endregion
                }
            }
            else
            {
                beans = dbOne.TB_ONE_SRMain.Where(x => (x.cStatus != "E0015" && x.cStatus != "E0006") && (x.cMainEngineerID == tERPID || x.cTechManagerID.Contains(tERPID) || x.cAssEngineerID.Contains(tERPID))).ToList();

                foreach (var bean in beans)
                {
                    tSRPathWay = TransSRPATH(cOperationID, cCompanyID, bean.cSRPathWay);
                    tSRType = TransSRType(bean.cSRTypeOne, bean.cSRTypeSec, bean.cSRTypeThr);
                    tMainEngineerID = string.IsNullOrEmpty(bean.cMainEngineerID) ? "" : bean.cMainEngineerID;
                    tMainEngineerName = string.IsNullOrEmpty(bean.cMainEngineerName) ? "" : bean.cMainEngineerName;
                    cTechManagerID = string.IsNullOrEmpty(bean.cTechManagerID) ? "" : bean.cTechManagerID;
                    tModifiedDate = bean.ModifiedDate == DateTime.MinValue ? "" : Convert.ToDateTime(bean.ModifiedDate).ToString("yyyy-MM-dd HH:mm:ss");
                    tSTATUSDESC = TransSRSTATUS(ListStatus, bean.cStatus);

                    #region 組待處理服務
                    string[] ProcessInfo = new string[12];

                    ProcessInfo[0] = bean.cSRID;            //SRID
                    ProcessInfo[1] = bean.cCustomerName;     //客戶
                    ProcessInfo[2] = bean.cRepairName;       //客戶報修人
                    ProcessInfo[3] = bean.cDesc;            //說明
                    ProcessInfo[4] = tSRPathWay;           //報修管道
                    ProcessInfo[5] = tSRType;              //報修類別
                    ProcessInfo[6] = tMainEngineerID;      //L2工程師ERPID
                    ProcessInfo[7] = tMainEngineerName;    //L2工程師姓名
                    ProcessInfo[8] = cTechManagerID;       //技術主管ERPID                    
                    ProcessInfo[9] = tModifiedDate;        //最後編輯日期
                    ProcessInfo[10] = bean.cStatus;         //狀態
                    ProcessInfo[11] = tSTATUSDESC;         //狀態+狀態說明

                    SRIDUserToList.Add(ProcessInfo);
                    #endregion
                }
            }

            return SRIDUserToList;
        }
        #endregion       

        #region 將服務團隊清單轉成where條件
        private string TrnasTeamListToWhere(List<string> tTeamList)
        {
            string reValue = string.Empty;

            int count = tTeamList.Count;
            int i = 0;

            foreach (var tTeam in tTeamList)
            {
                if (i == count - 1)
                {
                    reValue += "cTeamID like '%" + tTeam + "%'";
                }
                else
                {
                    reValue += "cTeamID like '%" + tTeam + "%' or ";
                }

                i++;
            }

            if (reValue != "")
            {
                reValue = " or (" + reValue + ")";
            }

            return reValue;
        }
        #endregion

        #region 取得登入人員所負責的服務團隊
        /// <summary>
        /// 取得登入人員所負責的服務團隊
        /// </summary>
        /// <param name="tCostCenterID">登入人員部門成本中心ID</param>
        /// <param name="tDeptID">登入人員部門ID</param>
        /// <returns></returns>
        public List<string> findSRTeamMappingList(string tCostCenterID, string tDeptID)
        {
            List<string> tList = new List<string>();

            var beans = dbOne.TB_ONE_SRTeamMapping.Where(x => x.Disabled == 0 && (x.cTeamNewID == tCostCenterID || x.cTeamNewID == tDeptID));

            foreach (var beansItem in beans)
            {
                if (!tList.Contains(beansItem.cTeamOldID))
                {
                    tList.Add(beansItem.cTeamOldID);
                }
            }

            return tList;
        }
        #endregion

        #region 取得報修管道參數值說明
        /// <summary>
        /// 取得報修管道參數值說明
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>
        /// <param name="cCompanyID">公司別</param>
        /// <param name="cSRPathWay">報修管道ID</param>
        /// <returns></returns>
        public string TransSRPATH(string cOperationID, string cCompanyID, string cSRPathWay)
        {
            string tValue = findSysParameterDescription(cOperationID, "OTHER", cCompanyID, "SRPATH", cSRPathWay);

            return tValue;
        }
        #endregion

        #region 取得服務請求狀態值說明
        /// <summary>
        /// 取得服務請求狀態值說明
        /// </summary>
        /// <param name="ListStatus">狀態清單</param>
        /// <param name="cSTATUS">狀態</param>
        /// <returns></returns>
        public string TransSRSTATUS(List<SelectListItem> ListStatus, string cSTATUS)
        {
            string tValue = string.Empty;

            var result = ListStatus.SingleOrDefault(x => x.Value == cSTATUS);

            if (result != null)
            {
                tValue = result.Value + "_" + result.Text;
            }

            return tValue;
        }
        #endregion

        #region 取得報修類別說明
        /// <summary>
        /// 取得報修類別說明
        /// </summary>
        /// <param name="cSRTypeOne">大類</param>
        /// <param name="cSRTypeSec">中類</param>
        /// <param name="cSRTypeThr">小類</param>
        /// <returns></returns>
        public string TransSRType(string cSRTypeOne, string cSRTypeSec, string cSRTypeThr)
        {
            string reValue = string.Empty;

            if (!string.IsNullOrEmpty(cSRTypeOne))
            {
                reValue += findSRRepairTypeName(cSRTypeOne) + ",";
            }

            if (!string.IsNullOrEmpty(cSRTypeSec))
            {
                reValue += findSRRepairTypeName(cSRTypeSec) + ",";
            }

            if (!string.IsNullOrEmpty(cSRTypeThr))
            {
                reValue += findSRRepairTypeName(cSRTypeThr);
            }

            return reValue;
        }
        #endregion

        #region 取得一般服務(報修類別說明)
        /// <summary>
        /// 取得一般服務(報修類別說明)
        /// </summary>
        /// <param name="cKindKey">報修類別ID</param>
        /// <returns></returns>
        public string findSRRepairTypeName(string cKindKey)
        {
            string reValue = string.Empty;

            var bean = dbOne.TB_ONE_SRRepairType.FirstOrDefault(x => x.cKIND_KEY == cKindKey);

            if (bean != null)
            {
                reValue = bean.cKIND_NAME;
            }

            return reValue;
        }
        #endregion       

        #region 傳入語法回傳DataTable(根據資料庫名稱)
        /// <summary>
        /// 傳入語法回傳DataTable(根據資料庫名稱)
        /// </summary>
        /// <param name="tSQL">SQL語法</param>
        /// <param name="dbName">資料庫名稱(dbOne; dbEIP; dbProxy; dbPSIP; dbBI)</param>
        /// <returns></returns>
        public DataTable getDataTableByDb(string tSQL, string dbName)
        {
            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection();

            switch (dbName)
            {
                case "dbOne":
                    con = (SqlConnection)dbOne.Database.Connection;
                    break;               
                case "dbEIP":
                    con = (SqlConnection)dbEIP.Database.Connection;
                    break;
                case "dbProxy":
                    con = (SqlConnection)dbProxy.Database.Connection;
                    break;
                case "dbPSIP":
                    con = (SqlConnection)dbPSIP.Database.Connection;
                    break;
                case "dbBI":
                    con = (SqlConnection)dbBI.Database.Connection;
                    break;
            }

            SqlCommand cmd = new SqlCommand(tSQL);
            cmd.Connection = con;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand.CommandTimeout = 600; //設定timeout為600秒
            sda.Fill(dt);

            return dt;
        }
        #endregion

        #endregion -----↑↑↑↑↑待辦清單 ↑↑↑↑↑-----   

        #region -----↓↓↓↓↓Mail相關 ↓↓↓↓↓-----

        #region 組服務請求Mail相關資訊
        /// <summary>
        /// 組服務請求Mail相關資訊
        /// </summary>        
        /// <param name="pOperationID_GenerallySR">程式作業編號檔系統ID</param>
        /// <param name="cBUKRS">公司別(T012、T016、C069、T022)</param>
        /// <param name="cSRID">SRID(服務案件ID)</param>         
        /// <param name="pLoginName">登入人員姓名</param>
        public void SetSRMailContent(string pOperationID_GenerallySR, string cBUKRS, string cSRID, string pLoginName)
        {          
            string tMailToTemp = string.Empty;
            string tMailCcTemp = string.Empty; 
            string tMailBCcTemp = string.Empty;

            string tMailTo = string.Empty;          //收件者            
            string tMailCc = string.Empty;          //副本            
            string tMailBCc = string.Empty;         //密件副本
            string tHypeLink = string.Empty;        //超連結
            string tSeverName = string.Empty;       //主機名稱
            
            string cSRCase = string.Empty;          //服務案件種類            
            string cTeamName = string.Empty;        //服務團隊
            string cTeamMGR = string.Empty;         //服務團隊主管
            string cTeamMGREmail = string.Empty;    //服務團隊主管Email
            string cMainENG = string.Empty;         //L2工程師
            string cMainENGEmail = string.Empty;    //L2工程師Email
            string cAssENG = string.Empty;          //指派工程師
            string cAssENGEmail = string.Empty;     //指派工程師Email
            string cTechMGR = string.Empty;         //技術主管
            string cTechMGREmail = string.Empty;    //技術主管Email
            string cContractID = string.Empty;      //合約文件編號
            string cCreatedDate = string.Empty;     //派單時間
            string cSecFix = string.Empty;          //是否為二修
            string cDesc = string.Empty;            //需求說明            
            string cNotes = string.Empty;           //詳細描述

            string cCusName = string.Empty;         //客戶名稱            
            string cRepairName = string.Empty;      //報修人
            string cRepairPhone = string.Empty;     //報修人電話
            string cRepairMobile = string.Empty;    //報修人手機
            string cRepairAddress = string.Empty;   //報修人地址
            string cRepairEmail = string.Empty;     //報修人Email

            try
            {
                SRIDINFOByMail SRMain = new SRIDINFOByMail();
                SRCONTACTINFO SRContact = new SRCONTACTINFO();
                SRPARTSREPALCEINFO SRParts = new SRPARTSREPALCEINFO();
                
                List<SRTEAMORGINFO> SRTeam = new List<SRTEAMORGINFO>();
                List<SREMPINFO> SRMainENG = new List<SREMPINFO>();
                List<SREMPINFO> SRAssENG = new List<SREMPINFO>();
                List<SREMPINFO> SRTechMGR = new List<SREMPINFO>();

                bool tIsFormal = getCallSAPERPPara(pOperationID_GenerallySR); //是否為正式區(true.是 false.不是)

                if (tIsFormal)
                {
                    tSeverName = "172.31.7.56:32200";
                }
                else
                {
                    tSeverName = "172.31.7.56:32200";
                }

                var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == cSRID);

                if (beanM != null)
                {
                    #region 服務團隊相關
                    SRTeam = findSRTEAMORGINFO(beanM.cTeamID);
                    cSRCase = findSRIDType(cSRID);
                    cTeamName = findSRTeamName(SRTeam);
                    cTeamMGR = findSRTeamMGRName(SRTeam);
                    cTeamMGREmail = findSRTeamMGREmail(SRTeam);
                    #endregion

                    #region L2工程師/指派工程師/技術主管相關
                    SRMainENG = findSREMPINFO(beanM.cMainEngineerID);
                    cMainENG = findSREMPName(SRMainENG);
                    cMainENGEmail = findSREMPEmail(SRMainENG);

                    SRAssENG = findSREMPINFO(beanM.cAssEngineerID);
                    cAssENG = findSREMPName(SRAssENG);
                    cAssENGEmail = findSREMPEmail(SRAssENG);

                    SRTechMGR = findSREMPINFO(beanM.cTechManagerID);
                    cTechMGR = findSREMPName(SRTechMGR);
                    cTechMGREmail = findSREMPEmail(SRTechMGR);
                    #endregion                    

                    SRMain.SRID = cSRID;
                    SRMain.Status = beanM.cStatus;
                    SRMain.SRCase = cSRCase;
                    SRMain.TeamNAME = cTeamName;
                    SRMain.TeamMGR = cTeamMGR;
                    SRMain.MainENG = cMainENG;
                    SRMain.AssENG = cAssENG;
                    SRMain.TechMGR = cTechMGR;
                    SRMain.SRID = cSRID;
                    SRMain.SRID = cSRID;
                    SRMain.SRID = cSRID;

                }              
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                writeToLog(cSRID, "SetSRMailContent", pMsg, pLoginName);
            }
        }
        #endregion

        #region 發送服務請求Mail相關資訊
        ///// <summary>
        ///// 組服務請求Mail相關資訊
        ///// </summary>        
        ///// <param name="pOperationID_GenerallySR">程式作業編號檔系統ID</param>
        ///// <param name="cSRID">SRID</param>         
        ///// <param name="pLoginName">登入人員姓名</param>
        //public void SendSRMail(string pOperationID_GenerallySR, string cSRID, string pLoginName)
        //{
        //    List<string> tMailToList = new List<string>();
        //    List<string> tMailCcList = new List<string>();
        //    List<string> tMailBCcList = new List<string>();

        //    string tMailToTemp = string.Empty;
        //    string tMailCcTemp = string.Empty;
        //    string tMailBCcTemp = string.Empty;

        //    string tMailTo = string.Empty;          //收件者            
        //    string tMailCc = string.Empty;          //副本            
        //    string tMailBCc = string.Empty;         //密件副本
        //    string tHypeLink = string.Empty;        //超連結
        //    string tSeverName = string.Empty;       //主機名稱

        //    string tStatus = string.Empty;          //狀態(E0001.新建、E0002.L2處理中、E0003.報價中、E0004.3rd Party處理中、E0005.L3處理中、E0006.完修、E0012.HPGCSN 申請、E0013.HPGCSN 完成、E0014.駁回、E0015.取消 )

        //    try
        //    {
        //        bool tIsFormal = getCallSAPERPPara(pOperationID_GenerallySR); //是否為正式區(true.是 false.不是)

        //        if (tIsFormal)
        //        {
        //            tSeverName = "172.31.7.56:32200";
        //        }
        //        else
        //        {
        //            tSeverName = "172.31.7.56:32200";
        //        }

        //        var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == cSRID);

        //        if (beanM != null)
        //        {

        //        }

        //        #region 取得收件者
        //        if (tMailToTemp != "")
        //        {
        //            foreach (string tValue in tMailToTemp.TrimEnd(';').Split(';'))
        //            {
        //                if (!tMailToList.Contains(tValue))
        //                {
        //                    tMailToList.Add(tValue);

        //                    tMailTo += tValue + ";";
        //                }
        //            }

        //            tMailTo = tMailTo.TrimEnd(';');
        //        }
        //        #endregion

        //        #region 取得副本
        //        if (tMailCcTemp != "")
        //        {
        //            foreach (string tValue in tMailCcTemp.TrimEnd(';').Split(';'))
        //            {
        //                if (!tMailCcList.Contains(tValue))
        //                {
        //                    tMailCcList.Add(tValue);

        //                    tMailCc += tValue + ";";
        //                }
        //            }

        //            tMailCc = tMailCc.TrimEnd(';');
        //        }
        //        #endregion

        //        #region 取得密件副本
        //        if (tMailBCcTemp != "")
        //        {
        //            foreach (string tValue in tMailBCcTemp.TrimEnd(';').Split(';'))
        //            {
        //                if (!tMailBCcList.Contains(tValue))
        //                {
        //                    tMailBCcList.Add(tValue);

        //                    tMailBCc += tValue + ";";
        //                }
        //            }

        //            tMailBCc = tMailBCc.TrimEnd(';');
        //        }
        //        #endregion

        //        #region 是否為測試區
        //        string strTest = string.Empty;

        //        if (!tIsFormal)
        //        {
        //            strTest = "【*測試*】";
        //        }
        //        #endregion

        //        #region 郵件主旨
        //        //備品維修
        //        //(待發料)備品維修_陳大明_台灣大哥大股份有限公司_8100002643
        //        //((狀態)借用類型_申請人_客戶_SRID)

        //        //內部借用
        //        //(待備品主管判斷備品周轉)內部借用_陳大明_20201001～20201031
        //        //((狀態)借用類型_申請人_借用起訖)

        //        string tMailSubject = string.Empty;

        //        //tMailSubject = strTest + "(" + tStageName + ")" + cApplicationType + "_" + cApplyUser_Name + "_" + cSRCustName + "_" + cSRID;
        //        #endregion

        //        #region 郵件內容

        //        #region 內容格式參考(備品維修)                
        //        //備品借用單SP-20200701-0010請協助發料，謝謝。
        //        //[服務案件明細]
        //        //服務案件ID: 8100002643
        //        //借用人:田巧如
        //        //填表人:吳若華
        //        //建立時間: 2020/10/08 12:58:05
        //        //客戶名稱: 台灣大哥大股份有限公司
        //        //需求說明: 【網路報修】加盟店-電腦維修無法連結印表機
        //        //主機訊息(序號，主機P/N，主機規格/說明): SGH747T67N，OOO，DL360

        //        //[備品待辦清單]
        //        //查看待辦清單 =>超連結(http://psip-qas/Spare/Index?FormNo=SP-20200701-0010&SRID=8100002643&NowStage=3)

        //        //-------此信件由系統管理員發出，請勿回覆此信件-------
        //        #endregion

        //        #region 內容格式參考(內部借用)                
        //        //備品借用單SP-20200701-0010請協助判斷備品周轉，謝謝。
        //        //[服務案件明細]
        //        //借用人:田巧如
        //        //填表人:吳若華
        //        //建立時間: 2020/10/08 12:58:05
        //        //借用起訖: 20201001~20201031
        //        //申請說明: POC電腦維修無法連結印表機

        //        //[備品待辦清單]
        //        //查看待辦清單 =>超連結(http://psip-qas/Spare/Index?FormNo=SP-20200701-0010&NowStage=2)

        //        //-------此信件由系統管理員發出，請勿回覆此信件-------
        //        #endregion

        //        string tMailBody = string.Empty;

        //        if (tStatus == "E0015") //取消
        //        {
        //            tHypeLink = "http://" + tSeverName + "/ServiceRequest/GenerallySR?SRID=" + cSRID;
        //        }
        //        else
        //        {
        //            tHypeLink = "http://" + tSeverName + "/ServiceRequest/ToDoList";
        //        }

        //        tMailBody = GetMailBody("WSSpareINTERNAL_MAIL");

        //        tMailBody = tMailBody.Replace("【<cFormNo>】", cFormNo).Replace("【<tStageName2>】", tStageName2);
        //        tMailBody = tMailBody.Replace("【<cApplyUser_Name>】", cApplyUser_Name).Replace("【<cFillUser_Name>】", cFillUser_Name);
        //        tMailBody = tMailBody.Replace("【<CreatedDate>】", CreatedDate).Replace("【<cStartDate>】", cStartDate).Replace("【<cEndDate>】", cEndDate);
        //        tMailBody = tMailBody.Replace("【<cApplicationNote>】", cApplicationNote).Replace("【<cSRInfo>】", cSRInfo).Replace("【<tNextStage>】", tNextStage);
        //        tMailBody = tMailBody.Replace("【<tComment>】", tComment).Replace("【<tHypeLink>】", tHypeLink);
        //        #endregion

        //        //呼叫寄送Mail
        //        SendMailByAPI("SendSRMail_API", null, tMailTo, tMailCc, tMailBCc, tMailSubject, tMailBody, "", "");
        //    }
        //    catch (Exception ex)
        //    {
        //        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
        //        pMsg += " 失敗行數：" + ex.ToString();

        //        writeToLog(cSRID, "SendSRMail", pMsg, pLoginName);
        //    }
        //}
        #endregion

        #region 以訊息中心發送Mail(新版)
        /// <summary>
        /// Email寄送 API
        /// </summary>
        /// <param name="eventName">事件名稱 </param>
        /// <param name="sender">設定寄件者：如為空或 null，則預設用 IC@etatung.com為寄件者 </param>
        /// <param name="recipients">收件者：用 ;分隔 </param>
        /// <param name="ccs">副本：用 ;分隔。如果沒有，就給空值或 null</param>
        /// <param name="bccs">密碼副本：用 ;分隔。如果沒有，就給空值或 null</param>
        /// <param name="subject">標題 </param>
        /// <param name="content">內容 </param>
        /// <param name="attachFileNames">附檔檔名：用 ;分隔 (※項目必需跟附檔路徑匹配 )。如果沒有，就給空值或 null</param>
        /// <param name="attachFilePaths">附檔路徑：用 ;分隔 (※項目必需跟附檔檔名匹配 )。如果沒有，就給空值或 null</param>
        public void SendMailByAPI(string eventName, string sender, string recipients, string ccs, string bccs, string subject, string content, string attachFileNames, string attachFilePaths)
        {
            WebRequest browser = WebRequest.Create("http://psip-prd-ap:8080/Ajax/SendMailAPI");
            browser.Method = "POST";
            browser.ContentType = "application/x-www-form-urlencoded";

            //附檔轉換成附檔轉換成base64
            List<string> attachFileBase64s = new List<string>();
            if (!string.IsNullOrEmpty(attachFilePaths))
            {
                var _attachFilePaths = attachFilePaths.Split(';');
                foreach (var attachFilePath in _attachFilePaths)
                {
                    attachFileBase64s.Add(Convert.ToBase64String(File.ReadAllBytes(attachFilePath)));
                }
            }

            NameValueCollection postParams = HttpUtility.ParseQueryString(string.Empty);
            postParams.Add("eventName", eventName);
            postParams.Add("sender", sender);
            postParams.Add("recipients", recipients);
            postParams.Add("ccs", ccs);
            postParams.Add("bccs", bccs);
            postParams.Add("subject", subject);
            postParams.Add("content", content);
            postParams.Add("attachFileNames", attachFileNames);
            postParams.Add("attachFileBase64s", string.Join(";", attachFileBase64s));

            //要發送的字串轉為要發送的字串轉為byte[]
            byte[] byteArray = Encoding.UTF8.GetBytes(postParams.ToString());
            using (Stream reqStream = browser.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }//end using

            //API回傳的字串回傳的字串
            string responseStr = "";

            //發出發出Request
            using (WebResponse response = browser.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = sr.ReadToEnd();
                }//end using
            }

            System.Diagnostics.Debug.WriteLine(responseStr);
        }
        #endregion

        #endregion -----↑↑↑↑↑Mail相關 ↑↑↑↑↑-----  

        #region 寫log 
        /// <summary>
        /// 寫log
        /// </summary>
        /// <param name="pSRID">目前SRID</param>
        /// <param name="tMethodName">方法目錄</param>
        /// <param name="tContent">內容</param>
        /// <param name="LoginUser_Name">登入人員姓名</param>
        public void writeToLog(string pSRID, string tMethodName, string tContent, string LoginUser_Name)
        {
            string tSRID = string.Empty;

            if (pSRID != null)
            {
                tSRID = pSRID;
            }

            #region 紀錄log
            TB_ONE_LOG logBean = new TB_ONE_LOG
            {
                cSRID = tSRID,
                EventName = tMethodName,
                Log = tContent,
                CreatedUserName = LoginUser_Name,
                CreatedDate = DateTime.Now
            };

            dbOne.TB_ONE_LOG.Add(logBean);
            dbOne.SaveChanges();
            #endregion
        }
        #endregion      
    }
}