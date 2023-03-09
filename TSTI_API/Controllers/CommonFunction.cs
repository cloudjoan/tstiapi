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

        /// <summary>
        /// 程式作業編號檔系統ID(ALL，固定的GUID)
        /// </summary>
        string pSysOperationID = "F8EFC55F-FA77-4731-BB45-2F2147244A2D";

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

        #region 取得個人客戶資料
        /// <summary>
        /// 取得個人客戶資料
        /// </summary>
        /// <param name="keyword">客戶代號/客戶名稱</param>
        /// <returns></returns>
        public List<PERSONAL_Contact> findPERSONALINFO(string keyword)
        {
            List<PERSONAL_Contact> tList = new List<PERSONAL_Contact>();

            if (keyword != "")
            {
                tList = dbProxy.PERSONAL_Contact.Where(x => x.Disabled == 0 && x.KNA1_KUNNR.Contains(keyword.Trim()) || x.KNA1_NAME1.Contains(keyword.Trim())).Take(30).ToList();
            }

            return tList;
        }
        #endregion

        #region 取得個人客戶聯絡人資料
        /// <summary>
        /// 取得個人客戶聯絡人資料
        /// </summary>
        /// <param name="PERSONALID">個人客戶代號</param>
        /// <param name="CONTACTNAME">聯絡人姓名</param>        
        /// <param name="CONTACTTEL">聯絡人電話</param>
        /// <param name="CONTACTMOBILE">聯絡人手機</param>
        /// <param name="CONTACTEMAIL">聯絡人Email</param>
        /// <returns></returns>
        public List<PCustomerContact> findPERSONALCONTACTINFO(string PERSONALID, string CONTACTNAME, string CONTACTTEL, string CONTACTMOBILE, string CONTACTEMAIL)
        {
            var qPjRec = dbProxy.PERSONAL_Contact.OrderByDescending(x => x.ModifiedDate).
                                               Where(x => x.Disabled == 0 && x.KNA1_KUNNR == PERSONALID &&
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

                    if (!tTempList.Contains(tTempValue)) //判斷個人客戶ID、公司別、聯絡人名姓名不重覆才要顯示
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
                        prDocBean.BPMNo = "GenerallySR";

                        liPCContact.Add(prDocBean);
                    }
                }
            }

            return liPCContact;
        }
        #endregion

        #region 新增時取得個人客戶流水號ID
        /// <summary>
        /// 新增時取得個人客戶流水號ID
        /// </summary>
        /// <returns></returns>
        public string findPERSONALISerialID()
        {
            string reValue = string.Empty;

            int tSerialID = 1;

            var bean = dbProxy.PERSONAL_Contact.OrderByDescending(x => x.KNA1_KUNNR).FirstOrDefault();

            if (bean != null)
            {
                tSerialID = int.Parse(bean.KNA1_KUNNR.Replace("P", "")) + 1;
                reValue = "P" + tSerialID.ToString().PadLeft(8, '0');
            }
            else
            {
                reValue = "P00000001";
            }

            return reValue;
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

        #region 取得員工Email(傳入ERPID)
        /// <summary>
        /// 取得員工Email(傳入ERPID)
        /// </summary>
        /// <param name="tERPID">ERPID</param>
        /// <returns></returns>
        public string findEMPEmail(string tERPID)
        {
            string reValue = string.Empty;

            var bean = dbEIP.Person.FirstOrDefault(x => (x.Leave_Date == null && x.Leave_Reason == null) && x.ERP_ID == tERPID.Trim());

            if (bean != null)
            {
                reValue = bean.Email;
            }

            return reValue;
        }
        #endregion

        #region 傳入服務團隊ID並取得公司別
        /// <summary>
        /// 傳入服務團隊ID並取得公司別
        /// </summary>
        /// <param name="TeamID">服務團隊ID</param>
        /// <returns></returns>
        public string findBUKRSByTeamID(string TeamID)
        {
            string reValue = "T012";

            string[] AryTeamID = TeamID.Trim(';').Split(';');

            foreach (string ID in AryTeamID)
            {
                var bean = dbOne.TB_ONE_SRTeamMapping.FirstOrDefault(x => x.Disabled == 0 && x.cTeamOldID == ID);

                if (bean != null)
                {
                    switch (bean.cTeamNewID.Substring(0, 2))
                    {
                        case "12":
                            reValue = "T012";
                            break;

                        case "16":
                            reValue = "T016";
                            break;

                        case "69":
                            reValue = "C069";
                           break;

                        case "22":
                            reValue = "T022";
                            break;
                    }
                }

                break;
            }

            return reValue;
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

        #region 取得服務團隊ID_名稱
        /// <summary>
        /// 取得服務團隊ID_名稱
        /// </summary>
        /// <param name="cTeamID">服務團隊ID(多筆以;號隔開)</param>
        /// <returns></returns>
        public string findSRTeamIDandName(string cTeamID)
        {
            string reValue = string.Empty;
            
            string[] AryTeamID = cTeamID.TrimEnd(';').Split(';');

            var beans = dbOne.TB_ONE_SRTeamMapping.Where(x => x.Disabled == 0 && AryTeamID.Contains(x.cTeamOldID));

            foreach (var bean in beans)
            {
                reValue += bean.cTeamOldID + "_" + bean.cTeamOldName + ";";
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

            return reValue;
        }
        #endregion

        #region 取得服務請求L2工程師/指派工程師/技術主管，員工ERPID_中文+英文姓名
        /// <summary>
        /// 服務請求L2工程師/指派工程師/技術主管相關資訊，員工ERPID_中文+英文姓名
        /// </summary>
        /// <param name="cERPID">員工編號(多筆以;號隔開)</param>
        /// <returns></returns>        
        public string findSREMPERPIDandNameByERPID(string cERPID)
        {
            string reValue = string.Empty;

            cERPID = string.IsNullOrEmpty(cERPID) ? "" : cERPID;

            string[] AryERPID = cERPID.TrimEnd(';').Split(';');

            var beans = dbEIP.Person.Where(x => (x.Leave_Date == null && x.Leave_Reason == null) && AryERPID.Contains(x.ERP_ID));

            foreach (var bean in beans)
            {
                reValue += bean.ERP_ID + "_" + bean.Name2 + " " + bean.Name + ";";
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

        #region 取得保固SLA資訊的合約編號有勾選本次使用的號碼
        /// <summary>
        /// 取得保固SLA資訊的合約編號有勾選本次使用的號碼
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <returns></returns>
        public string findSRContractID(string cSRID)
        {
            string reValue = string.Empty;

            var bean = dbOne.TB_ONE_SRDetail_Warranty.FirstOrDefault(x => x.cUsed == "Y" && x.cContractID != "" && x.cSRID == cSRID);

            if (bean != null)
            {
                reValue = bean.cContractID;
            }

            return reValue;
        }
        #endregion

        #region 取得保固SLA資訊的「回應條件和服務條件」有勾選本次使用的號碼
        /// <summary>
        /// 取得保固SLA資訊的「回應條件和服務條件」有勾選本次使用的號碼
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <returns></returns>
        public string[] findSRSLACondition(string cSRID)
        {
            string[] reValue = new string[2];
            reValue[0] = "";
            reValue[1] = "";

            var bean = dbOne.TB_ONE_SRDetail_Warranty.FirstOrDefault(x => x.cUsed == "Y" && x.cSRID == cSRID);

            if (bean != null)
            {
                reValue[0] = bean.cSLARESP;
                reValue[1] = bean.cSLASRV;
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

        #region 取得客戶報修窗口相關資訊
        /// <summary>
        /// 取得客戶報修窗口相關資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>        
        /// <param name="cRepairName">報修人</param>
        /// <param name="cRepairPhone">報修人電話</param>
        /// <param name="cRepairMobile">報修人手機</param>
        /// <param name="cRepairAddress">報修人地址</param>
        /// <param name="cRepairEmail">報修人Email</param>
        /// <returns></returns>
        public List<SRCONTACTINFO> findSRREPAIRINFO(string cSRID, string cRepairName, string cRepairPhone, string cRepairMobile, string cRepairAddress, string cRepairEmail)
        {
            List<SRCONTACTINFO> tList = new List<SRCONTACTINFO>();

            SRCONTACTINFO SRCon = new SRCONTACTINFO();

            SRCon.SRID = cSRID;
            SRCon.CONTNAME = cRepairName;
            SRCon.CONTADDR = cRepairAddress;
            SRCon.CONTTEL = cRepairPhone;
            SRCon.CONTMOBILE = cRepairMobile;
            SRCon.CONTEMAIL = cRepairEmail;

            tList.Add(SRCon);

            return tList;
        }
        #endregion

        #region 取得客戶聯絡窗口相關資訊
        /// <summary>
        /// 取得客戶聯絡窗口相關資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <returns></returns>        
        public List<SRCONTACTINFO> findSRCONTACTINFO(string cSRID)
        {
            List<SRCONTACTINFO> tList = new List<SRCONTACTINFO>();            

            var beans = dbOne.TB_ONE_SRDetail_Contact.Where(x => x.Disabled == 0 && x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                SRCONTACTINFO SRCon = new SRCONTACTINFO();               

                SRCon.SRID = bean.cSRID;
                SRCon.CONTNAME = bean.cContactName;
                SRCon.CONTADDR = bean.cContactAddress;
                SRCon.CONTTEL = bean.cContactPhone;
                SRCon.CONTMOBILE = bean.cContactMobile;
                SRCon.CONTEMAIL = bean.cContactEmail;
                
                tList.Add(SRCon);
            }

            return tList;
        }
        #endregion

        #region 取得產品序號資訊
        /// <summary>
        /// 取得產品序號資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <returns></returns>        
        public List<SRSERIALMATERIALINFO> findSRSERIALMATERIALINFO(string cSRID)
        {
            List<SRSERIALMATERIALINFO> tList = new List<SRSERIALMATERIALINFO>();

            var beans = dbOne.TB_ONE_SRDetail_Product.Where(x => x.Disabled == 0 && x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                SRSERIALMATERIALINFO SRSerial = new SRSERIALMATERIALINFO();

                SRSerial.SRID = bean.cSRID;
                SRSerial.SerialID = bean.cSerialID;
                SRSerial.MaterialID = bean.cMaterialID;
                SRSerial.MaterialName = bean.cMaterialName;
                SRSerial.ProductNumber = bean.cProductNumber;
                SRSerial.InstallID = bean.cInstallID;

                tList.Add(SRSerial);
            }

            return tList;
        }
        #endregion

        #region 取得保固SLA資訊
        /// <summary>
        /// 取得保固SLA資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <param name="tBPMURLName">BPM站台名稱</param>
        /// <param name="tPSIPURLName">PSIP站台名稱</param>        
        /// <returns></returns>        
        public List<SRWTSLAINFO> findSRWTSLAINFO(string cSRID, string tBPMURLName, string tPSIPURLName)
        {
            List<SRWTSLAINFO> tList = new List<SRWTSLAINFO>();

            string cCONTRACTIDUrl = string.Empty;            
            string cBPMFormNoUrl = string.Empty;
            string cSUBCONTRACTID = string.Empty;            
            string cUSED = string.Empty;            

            var beans = dbOne.TB_ONE_SRDetail_Warranty.Where(x => x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                cSUBCONTRACTID = string.IsNullOrEmpty(bean.cSubContractID) ? "" : bean.cSubContractID.Trim();
                cUSED = bean.cUsed == "Y" ? "Y_使用" : "N_不使用";                

                #region 取得BPM Url
                if (bean.cContractID != "")
                {
                    try
                    {
                        Int32 ContractID = Int32.Parse(bean.cContractID);

                        if (ContractID >= 10506151 && ContractID != 10506152 && ContractID != 10506157) //新的用印申請單
                        {
                            cBPMFormNoUrl = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/ContractSeals/ContractSealsForm.aspx?FormNo=" + bean.cBPMFormNo;
                        }
                        else //舊的用印申請單
                        {
                            cBPMFormNoUrl = "http://" + tBPMURLName + "/ContractSeals/_layouts/FormServer.aspx?XmlLocation=%2fContractSeals%2fBPMContractSealsForm%2f" + bean.cBPMFormNo + ".xml&ClientInstalled=true&DefaultItemOpen=1&source=/_layouts/TSTI.SharePoint.BPM/CloseWindow.aspx";
                        }

                        cCONTRACTIDUrl = "http://" + tPSIPURLName + "/Spare/QueryContractInfo?CONTRACTID=" + bean.cContractID; //合約編號URL
                    }
                    catch (Exception ex)
                    {
                        cCONTRACTIDUrl = "";                        
                        cBPMFormNoUrl = "";
                    }
                }
                else
                {
                    if (bean.cBPMFormNo.IndexOf("WTY") != -1)
                    {
                        cBPMFormNoUrl = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/Warranty/WarrantyForm.aspx?FormNo=" + bean.cBPMFormNo;
                    }
                    else
                    {
                        cBPMFormNoUrl = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Form/Guarantee/GuaranteeForm.aspx?FormNo=" + bean.cBPMFormNo;
                    }
                }
                #endregion

                SRWTSLAINFO SRWTSAL = new SRWTSLAINFO();

                SRWTSAL.SRID = bean.cSRID;
                SRWTSAL.SERIALID = bean.cSerialID;
                SRWTSAL.WTYID = bean.cWTYID;
                SRWTSAL.WTYName = bean.cWTYName;
                SRWTSAL.WTYSDATE = Convert.ToDateTime(bean.cWTYSDATE).ToString("yyyy-MM-dd");
                SRWTSAL.WTYEDATE = Convert.ToDateTime(bean.cWTYEDATE).ToString("yyyy-MM-dd");
                SRWTSAL.SLARESP = bean.cSLARESP;

                SRWTSAL.SLASRV = bean.cSLASRV;
                SRWTSAL.CONTRACTID = bean.cContractID;
                SRWTSAL.CONTRACTIDUrl = cCONTRACTIDUrl;
                SRWTSAL.SUBCONTRACTID = cSUBCONTRACTID;
                SRWTSAL.BPMFormNo = bean.cBPMFormNo;
                SRWTSAL.BPMFormNoUrl = cBPMFormNoUrl;
                SRWTSAL.ADVICE = bean.cAdvice;
                SRWTSAL.USED = cUSED;

                tList.Add(SRWTSAL);
            }

            return tList;
        }
        #endregion

        #region 取得處理與工時紀錄資訊
        /// <summary>
        /// 處理與工時紀錄資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <param name="tAttachURLName">附件URL站台名稱</param>
        /// <returns></returns>        
        public List<SRRECORDINFO> findSRRECORDINFO(string cSRID, string tAttachURLName)
        {
            List<SRRECORDINFO> tList = new List<SRRECORDINFO>();

            string cReceiveTime = string.Empty;
            string cStartTime = string.Empty;
            string cArriveTime = string.Empty;
            string cFinishTime = string.Empty;
            string cURLName = string.Empty;
            string cSRReportURL = string.Empty;
          
            var beans = dbOne.TB_ONE_SRDetail_Record.Where(x => x.Disabled == 0 && x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                SRRECORDINFO SRRecord = new SRRECORDINFO();

                cReceiveTime = bean.cReceiveTime == null ? "" : Convert.ToDateTime(bean.cReceiveTime).ToString("yyyy-MM-dd HH:mm");
                cStartTime = bean.cStartTime == null ? "" : Convert.ToDateTime(bean.cStartTime).ToString("yyyy-MM-dd HH:mm");
                cArriveTime = bean.cArriveTime == null ? "" : Convert.ToDateTime(bean.cArriveTime).ToString("yyyy-MM-dd HH:mm");
                cFinishTime = bean.cFinishTime == null ? "" : Convert.ToDateTime(bean.cFinishTime).ToString("yyyy-MM-dd HH:mm");
                cSRReportURL = findAttachUrl(bean.cSRReport, tAttachURLName);

                SRRecord.CID = bean.cID.ToString();
                SRRecord.SRID = bean.cSRID;
                SRRecord.ENGID = bean.cEngineerID;
                SRRecord.ENGNAME = bean.cEngineerName;
                SRRecord.ReceiveTime = cReceiveTime;
                SRRecord.StartTime = cStartTime;
                SRRecord.ArriveTime = cArriveTime;
                SRRecord.FinishTime = cFinishTime;
                SRRecord.WorkHours = bean.cWorkHours.ToString();
                SRRecord.Desc = bean.cDesc;
                SRRecord.SRReportURL = cSRReportURL;                

                tList.Add(SRRecord);
            }

            return tList;
        }
        #endregion

        #region 取得處理與工時紀錄裡的服務報告書序號
        /// <summary>
        /// 取得處理與工時紀錄裡的服務報告書序號
        /// </summary>
        /// <param name="cSRID">SRID</param>        
        /// <returns></returns>
        public string GetReportSerialID(string cSRID)
        {
            string strCNO = "";          

            #region 取號
            var bean = dbOne.TB_ONE_ReportFormat.FirstOrDefault(x => x.cTitle == cSRID);

            if (bean == null) //若沒有資料，則新增一筆當月的資料
            {
                TB_ONE_ReportFormat FormNoTable = new TB_ONE_ReportFormat();

                FormNoTable.cTitle = cSRID;                
                FormNoTable.cNO = "00";

                dbOne.TB_ONE_ReportFormat.Add(FormNoTable);
                dbOne.SaveChanges();
            }

            bean = dbOne.TB_ONE_ReportFormat.FirstOrDefault(x => x.cTitle == cSRID);

            if (bean != null)
            {
                strCNO = cSRID + "_" + (int.Parse(bean.cNO) + 1).ToString().PadLeft(2, '0');
                bean.cNO = (int.Parse(bean.cNO) + 1).ToString().PadLeft(2, '0');

                dbOne.SaveChanges();
            }
            #endregion

            return strCNO;
        }
        #endregion     

        #region 取得附件/服務報告書URL(多筆以;號隔開)
        /// <summary>
        /// 取得附件/服務報告書URL(多筆以;號隔開)
        /// </summary>
        /// <param name="tAttach">附件GUID(多筆以,號隔開)</param>
        /// <param name="tAttachURLName">附件URL站台名稱</param>
        /// <returns></returns>
        public string findAttachUrl(string tAttach, string tAttachURLName)
        {
            string reValue = string.Empty;

            List<SRATTACHINFO> SR_List = findSRATTACHINFO(tAttach, tAttachURLName);

            foreach(var bean in SR_List)
            {
                reValue += bean.FILE_URL + ";";
            }

            reValue = reValue.TrimEnd(';');

            return reValue;
        }
        #endregion

        #region 取得附件相關資訊
        /// <summary>
        /// 取得附件相關資訊
        /// </summary>
        /// <param name="tAttach">附件GUID(多筆以,號隔開)</param>
        /// <param name="tAttachURLName">附件URL站台名稱</param>
        /// <returns></returns>
        public List<SRATTACHINFO> findSRATTACHINFO(string tAttach, string tAttachURLName)
        {
            #region 範例Url
            //http://tsticrmmbgw.etatung.com:8082/CSreport/a7f12260-0168-4cf8-a321-c2d410ac3536.txt
            #endregion

            tAttach = string.IsNullOrEmpty(tAttach) ? "" : tAttach;

            List<SRATTACHINFO> tList = new List<SRATTACHINFO>();              

            string tURL = string.Empty;
            string[] tAryAttach = tAttach.TrimEnd(',').Split(',');            

            foreach (string tKey in tAryAttach)
            {
                var bean = dbOne.TB_ONE_DOCUMENT.FirstOrDefault(x => x.ID.ToString() == tKey);

                if (bean != null)
                {
                    SRATTACHINFO beanSR = new SRATTACHINFO();

                    tURL = "http://" + tAttachURLName + "/CSreport/" + bean.FILE_NAME;

                    beanSR.ID = tKey;
                    beanSR.FILE_ORG_NAME = bean.FILE_ORG_NAME;
                    beanSR.FILE_NAME = bean.FILE_NAME;
                    beanSR.FILE_EXT = bean.FILE_EXT;
                    beanSR.FILE_URL = tURL;
                    beanSR.INSERT_TIME = bean.INSERT_TIME;

                    tList.Add(beanSR);
                }
            }

            return tList;
        }
        #endregion

        #region 取得零件更換資訊
        /// <summary>
        /// 取得零件更換資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <returns></returns>        
        public List<SRPARTSREPALCEINFO> findSRPARTSREPALCEINFO(string cSRID)
        {
            List<SRPARTSREPALCEINFO> tList = new List<SRPARTSREPALCEINFO>();

            string cArriveDate = string.Empty;
            string cReturnDate = string.Empty;

            var beans = dbOne.TB_ONE_SRDetail_PartsReplace.Where(x => x.Disabled == 0 && x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                SRPARTSREPALCEINFO SRPart = new SRPARTSREPALCEINFO();

                cArriveDate = bean.cArriveDate == null ? "" : Convert.ToDateTime(bean.cArriveDate).ToString("yyyy-MM-dd");
                cReturnDate = bean.cReturnDate == null ? "" : Convert.ToDateTime(bean.cReturnDate).ToString("yyyy-MM-dd");

                SRPart.CID = bean.cID.ToString();
                SRPart.SRID = bean.cSRID;
                SRPart.XCHP = bean.cXCHP;
                SRPart.MaterialID = bean.cMaterialID;
                SRPart.MaterialName = bean.cMaterialName;
                SRPart.OldCT = bean.cOldCT;
                SRPart.NewCT = bean.cNewCT;
                SRPart.HPCT = bean.cHPCT;
                SRPart.NewUEFI = bean.cNewUEFI;
                SRPart.StandbySerialID = bean.cStandbySerialID;
                SRPart.HPCaseID = bean.cHPCaseID;
                SRPart.ArriveDate = cArriveDate;
                SRPart.ReturnDate = cReturnDate;
                SRPart.PersonalDamage = bean.cPersonalDamage;
                SRPart.Note = bean.cNote;

                tList.Add(SRPart);
            }

            return tList;
        }
        #endregion    

        #region 取得Mail Body
        /// <summary>
        /// 取得Mail Body
        /// </summary>
        /// <param name="tMAIL_TYPE">MAIL TYPE</param>
        /// <returns></returns>
        public string GetMailBody(string tMAIL_TYPE)
        {
            string reValue = string.Empty;

            var bean = dbProxy.TB_MAIL_CONTENT.FirstOrDefault(x => x.MAIL_TYPE == tMAIL_TYPE);

            if (bean != null)
            {
                reValue = bean.MAIL_CONTENT;
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
        /// <param name="tBPMURLName">BPM站台名稱</param>
        /// <param name="tPSIPURLName">PSIP站台名稱</param>
        /// <param name="tAPIURLName">API站台名稱</param>
        /// <returns></returns>
        public List<SRWarranty> ZFM_TICC_SERIAL_SEARCHWTYList(string[] ArySERIAL, string tBPMURLName, string tPSIPURLName, string tAPIURLName)
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
                                        tURL = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/ContractSeals/ContractSealsForm.aspx?FormNo=" + tBPMNO;
                                    }
                                    else //舊的用印申請單
                                    {
                                        tURL = "http://" + tBPMURLName + "/ContractSeals/_layouts/FormServer.aspx?XmlLocation=%2fContractSeals%2fBPMContractSealsForm%2f" + tBPMNO + ".xml&ClientInstalled=true&DefaultItemOpen=1&source=/_layouts/TSTI.SharePoint.BPM/CloseWindow.aspx";
                                    }

                                    cContractIDURL = "http://" + tPSIPURLName + "/Spare/QueryContractInfo?CONTRACTID=" + cContractID; //合約編號URL
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
                                    tURL = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/Warranty/WarrantyForm.aspx?FormNo=" + tBPMNO;
                                }
                                else
                                {
                                    tURL = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Form/Guarantee/GuaranteeForm.aspx?FormNo=" + tBPMNO;
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

            string tBUKRS = string.Empty;
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

                    tBUKRS = findBUKRSByTeamID(bean.cTeamID);
                    tSTATUSDESC = findSysParameterDescription(pOperationID_GenerallySR, "OTHER", tBUKRS, "SRSTATUS", bean.cStatus);
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

            string[] tArySLA = new string[2];

            string tSRPathWay = string.Empty;           //報修管理
            string tSRType = string.Empty;              //報修類別
            string tMainEngineerID = string.Empty;      //L2工程師ERPID
            string tMainEngineerName = string.Empty;    //L2工程師姓名            
            string cTechManagerID = string.Empty;       //技術主管ERPID            
            string tModifiedDate = string.Empty;        //最後編輯日期
            string tSTATUSDESC = string.Empty;          //狀態說明
            string tSLARESP = string.Empty;             //回應條件
            string tSLASRV = string.Empty;              //服務條件

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
                    tArySLA = findSRSLACondition(dr["cSRID"].ToString());
                    tSLARESP = tArySLA[0];
                    tSLASRV = tArySLA[1];

                    #region 組待處理服務
                    string[] ProcessInfo = new string[14];

                    ProcessInfo[0] = dr["cSRID"].ToString();             //SRID
                    ProcessInfo[1] = dr["cCustomerName"].ToString();      //客戶
                    ProcessInfo[2] = dr["cRepairName"].ToString();        //客戶報修人
                    ProcessInfo[3] = dr["cDesc"].ToString();             //說明
                    ProcessInfo[4] = tSRPathWay;                        //報修管道
                    ProcessInfo[5] = tSRType;                           //報修類別
                    ProcessInfo[6] = tMainEngineerID;                   //L2工程師ERPID
                    ProcessInfo[7] = tMainEngineerName;                 //L2工程師姓名
                    ProcessInfo[8] = cTechManagerID;                    //技術主管ERPID                    
                    ProcessInfo[9] = tSLARESP;                          //回應條件
                    ProcessInfo[10] = tSLASRV;                          //服務條件
                    ProcessInfo[11] = tModifiedDate;                    //最後編輯日期                    
                    ProcessInfo[12] = dr["cStatus"].ToString();           //狀態
                    ProcessInfo[13] = tSTATUSDESC;                      //狀態+狀態說明

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
                    tArySLA = findSRSLACondition(bean.cSRID);
                    tSLARESP = tArySLA[0];
                    tSLASRV = tArySLA[1];

                    #region 組待處理服務
                    string[] ProcessInfo = new string[14];

                    ProcessInfo[0] = bean.cSRID;            //SRID
                    ProcessInfo[1] = bean.cCustomerName;     //客戶
                    ProcessInfo[2] = bean.cRepairName;       //客戶報修人
                    ProcessInfo[3] = bean.cDesc;            //說明
                    ProcessInfo[4] = tSRPathWay;           //報修管道
                    ProcessInfo[5] = tSRType;              //報修類別
                    ProcessInfo[6] = tMainEngineerID;      //L2工程師ERPID
                    ProcessInfo[7] = tMainEngineerName;    //L2工程師姓名
                    ProcessInfo[8] = cTechManagerID;       //技術主管ERPID                                        
                    ProcessInfo[9] = tSLARESP;             //回應條件
                    ProcessInfo[10] = tSLASRV;             //服務條件
                    ProcessInfo[11] = tModifiedDate;       //最後編輯日期
                    ProcessInfo[12] = bean.cStatus;         //狀態
                    ProcessInfo[13] = tSTATUSDESC;         //狀態+狀態說明

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

        #region 取得服務請求case內容
        /// <summary>
        /// 取得服務請求case內容
        /// </summary>
        /// <param name="IV_SRID">SRID</param>
        /// <param name="pOperationID_GenerallySR">程式作業編號檔系統ID(一般服務)</param>
        /// <returns></returns>
        public Dictionary<string, object> GetSRDetail(string IV_SRID, string pOperationID_GenerallySR)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();

            string EV_TYPE = "ZSR1";            
            string EV_SLASRV = string.Empty;
            string EV_SLARESP = string.Empty;
            string EV_WTYKIND = string.Empty;

            var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == IV_SRID);

            if (beanM != null)
            {
                switch (IV_SRID.Substring(0, 2))
                {
                    case "61": //一般服務
                        EV_TYPE = "ZSR1";
                        break;
                    case "63": //裝機服務
                        EV_TYPE = "ZSR3";
                        break;
                    case "65": //定維服務
                        EV_TYPE = "ZSR5";
                        break;
                }

                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = findEmployeeInfoByERPID(beanM.cMainEngineerID);

                string[] tArySLA = findSRSLACondition(IV_SRID);
                EV_SLARESP = tArySLA[0];
                EV_SLASRV = tArySLA[1];

                EV_WTYKIND = findSysParameterDescription(pOperationID_GenerallySR, "OTHER", EmpBean.BUKRS, "SRMATYPE", beanM.cMAServiceType);

                results.Add("EV_CUSTOMER", beanM.cCustomerName);       //客戶名稱
                results.Add("EV_DESC", beanM.cDesc);                  //說明                
                results.Add("EV_PROBLEM", beanM.cNotes);              //詳細描述
                results.Add("EV_MAINENG", beanM.cMainEngineerName);    //L2工程師姓名
                results.Add("EV_MAINENGID", beanM.cMainEngineerID);    //L2工程師ERPID                
                results.Add("EV_CONTACT", beanM.cRepairName);         //報修人姓名
                results.Add("EV_ADDR", beanM.cRepairAddress);         //報修人地址
                results.Add("EV_TEL", beanM.cRepairPhone);           //報修人電話
                results.Add("EV_MOBILE", beanM.cRepairMobile);       //報修人手機
                results.Add("EV_EMAIL", beanM.cRepairEmail);         //報修人Email
                results.Add("EV_TYPE", EV_TYPE);                   //服務種類
                results.Add("EV_COUNTIN", "");                      //計數器進(群輝用，先保持空白)
                results.Add("EV_COUNTOUT", "");                     //計數器出(群輝用，先保持空白)
                results.Add("EV_SQ", beanM.cSQPersonName);          //SQ人員名稱
                results.Add("EV_DEPARTMENT", EmpBean.BUKRS);        //公司別(T012、T016、C069、t022)
                results.Add("EV_SLASRV", EV_SLASRV);               //SLA服務條件
                results.Add("EV_WTYKIND", EV_WTYKIND);             //維護服務種類(Z01.保固內、Z02.保固外、Z03.合約、Z04.3rd Party)
            }

            if (!string.IsNullOrEmpty(IV_SRID))
            {
                #region 聯絡人窗口資訊
                var beanD_Con = dbOne.TB_ONE_SRDetail_Contact.FirstOrDefault(x => x.Disabled == 0 && x.cSRID == IV_SRID);

                if (beanD_Con != null)
                {
                    results.Add("EV_REPORT", beanD_Con.cContactName);       //聯絡人姓名                    
                    results.Add("EV_RADDR", beanD_Con.cContactAddress);     //聯絡人地址
                    results.Add("EV_RTEL", beanD_Con.cContactPhone);        //聯絡人電話
                    results.Add("EV_RMOBILE", beanD_Con.cContactMobile);    //聯絡人手機
                    results.Add("EV_EMAIL_R", beanD_Con.cContactEmail);     //聯絡人Email
                }
                #endregion

                #region 產品序號資訊
                var beanD_Prd = dbOne.TB_ONE_SRDetail_Product.Where(x => x.Disabled == 0 && x.cSRID == IV_SRID);

                List<SNLIST> snList = new List<SNLIST>();

                foreach (var beanD in beanD_Prd)
                {
                    SNLIST snBean = new SNLIST();
                    snBean.SNNO = beanD.cSerialID;              //機器序號
                    snBean.PRDID = beanD.cMaterialName;         //機器型號
                    snBean.PRDNUMBER = beanD.cProductNumber;    //Product Number

                    snList.Add(snBean);
                }

                results.Add("table_ET_SNLIST", snList);
                #endregion

                #region 處理與工時紀錄
                var beanD_Rec = dbOne.TB_ONE_SRDetail_Record.Where(x => x.Disabled == 0 && x.cSRID == IV_SRID);

                List<ENGProcessLIST> ENGPList = new List<ENGProcessLIST>();

                foreach (var beanD in beanD_Rec)
                {
                    ENGProcessLIST ENGBean = new ENGProcessLIST();
                    ENGBean.ENGID = beanD.cEngineerID;
                    ENGBean.ENGNAME = beanD.cEngineerName;
                    ENGBean.ENGEMAIL = findEMPEmail(beanD.cEngineerID);

                    ENGPList.Add(ENGBean);
                }

                results.Add("table_ET_LABORLIST", ENGPList);
                #endregion

                #region 零件更換資訊
                var beanD_Part = dbOne.TB_ONE_SRDetail_PartsReplace.Where(x => x.Disabled == 0 && x.cSRID == IV_SRID);

                List<XCLIST> xcList = new List<XCLIST>();

                foreach (var beanD in beanD_Part)
                {
                    XCLIST xcBean = new XCLIST();
                    xcBean.HPXC = beanD.cXCHP;                  //XC HP申請零件
                    xcBean.OLDCT = beanD.cOldCT;                //OLD CT
                    xcBean.NEWCT = beanD.cNewCT;                //NEW CT
                    xcBean.UEFI = beanD.cNewUEFI;               //UEFI
                    xcBean.BACKUPSN = beanD.cStandbySerialID;    //備機序號
                    xcBean.HPCT = beanD.cHPCT;                 //HPCT
                    xcBean.CHANGEPART = beanD.cMaterialID;      //更換零件ID
                    xcBean.CHANGEPARTNAME = beanD.cMaterialName; //料號說明

                    xcList.Add(xcBean);
                }
                results.Add("table_ET_XCLIST", xcList);
                #endregion
            }

            return results;
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

        #region 取得【一般服務】案件種類的郵件主旨
        /// <summary>
        /// 取得【一般服務】案件種類的郵件主旨
        /// </summary>
        /// <param name="cCondition">服務請求執行條件(ADD.新建、TRANS.轉派L2工程師、REJECT.駁回、HPGCSN.HPGCSN申請、SECFIX.二修、SAVE.保存、THRPARTY.3Party、CANCEL.取消)</param>
        /// <param name="SRID">服務ID</param>
        /// <param name="CusName">客戶名稱</param>
        /// <param name="TeamNAME">服務團隊</param>
        /// <param name="SRCase">服務案件種類</param>
        /// <param name="MainENG">L2工程師</param>
        /// <returns></returns>
        public string findGenerallySRMailSubject(SRCondition cCondition, string SRID, string CusName, string TeamNAME, string SRCase, string MainENG)
        {
            string reValue = string.Empty;

            switch (cCondition)
            {
                case SRCondition.ADD:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 派單通知[<服務ID>]                  
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 派單通知[" + SRID + "]";
                    break;

                case SRCondition.TRANS:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類>[<服務ID>]已轉到[<L2工程師>]名下，請留意！
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + "[" + SRID + "]已轉到[" + MainENG + "]名下，請留意！";
                    break;

                case SRCondition.REJECT:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 主管審核駁回通知[<服務ID>]
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 主管審核駁回通知[" + SRID + "]";
                    break;

                case SRCondition.HPGCSN:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 派單通知[<服務ID>]，需下料。
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 派單通知[" + SRID + "]，需下料。";
                    break;

                case SRCondition.SECFIX:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 二修通知[<服務ID>]
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 二修通知[" + SRID + "]";
                    break;

                case SRCondition.SUPPORT:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 技術支援升級通知[<服務ID>]
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 技術支援升級通知[" + SRID + "]";
                    break;

                case SRCondition.SAVE:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 異動通知[<服務ID>]
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 異動通知[" + SRID + "]";
                    break;

                case SRCondition.THRPARTY:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 3Party通知[<服務ID>]
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 3Party通知[" + SRID + "]";
                    break;

                case SRCondition.CANCEL:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 取消通知[<服務ID>]，請關注！
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 取消通知[" + SRID + "]，請關注！";
                    break;

                case SRCondition.DONE:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 完修通知[<服務ID>]
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 完修通知[" + SRID + "]";
                    break;
            }

            return reValue;
        }
        #endregion

        #region 取得【一般服務】案件客戶報修窗口資訊Html Table
        /// <summary>
        /// 取得【一般服務】案件客戶報修窗口資訊Html Table
        /// </summary>
        /// <param name="SRRepair_List">服務請求客戶報修人資訊清單</param>
        /// <param name="CusName">客戶名稱</param>
        /// <returns></returns>
        public string findGenerallySRRepair_Table(List<SRCONTACTINFO> SRRepair_List, string CusName)
        {
            #region 格式
            //客戶名稱：OOO股份有限公司
            //[客戶報修窗口資料]												
            //報修人	報修人電話	報修人手機	報修人地址	報修人Email								
            //OOO	042OOO	09OOO	台北市OOO	TEST@OOO								            
            #endregion

            StringBuilder strHTML = new StringBuilder();
            string reValue = string.Empty;

            if (SRRepair_List.Count > 0)
            {
                strHTML.AppendLine("<div>");
                strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>客戶名稱："+ CusName +"</p>");
                strHTML.AppendLine("    <p>[客戶報修窗口資訊]</p>");
                strHTML.AppendLine("    <table style='width:720pt;font-family:微軟正黑體;' align='left' border='1'>");
                strHTML.AppendLine("        <tr>");
                strHTML.AppendLine("            <td>報修人</td>");
                strHTML.AppendLine("            <td>報修人電話</td>");
                strHTML.AppendLine("            <td>報修人手機</td>");
                strHTML.AppendLine("            <td>報修人地址</td>");
                strHTML.AppendLine("            <td>報修人Email</td>");
                strHTML.AppendLine("        </tr>");

                foreach (var bean in SRRepair_List)
                {
                    strHTML.AppendLine("        <tr>");
                    strHTML.AppendLine("            <td>" + bean.CONTNAME + "</td>");
                    strHTML.AppendLine("            <td>" + bean.CONTTEL + "</td>");
                    strHTML.AppendLine("            <td>" + bean.CONTMOBILE + "</td>");
                    strHTML.AppendLine("            <td>" + bean.CONTADDR + "</td>");
                    strHTML.AppendLine("            <td>" + bean.CONTEMAIL + "</td>");
                    strHTML.AppendLine("        </tr>");
                }

                strHTML.AppendLine("    </table>");
                strHTML.AppendLine("</div>");
                strHTML.AppendLine("<p>&nbsp;</p>");                
            }

            reValue = strHTML.ToString();

            return reValue;
        }
        #endregion

        #region 取得【一般服務】案件客戶聯絡窗口資訊Html Table
        /// <summary>
        /// 取得【一般服務】案件客戶聯絡窗口資訊Html Table
        /// </summary>
        /// <param name="SRContact_List">服務請求客戶聯絡人資訊清單</param>
        /// <returns></returns>
        public string findGenerallySRContact_Table(List<SRCONTACTINFO> SRContact_List)
        {
            #region 格式
            //[客戶聯絡窗口資料]												
            //聯絡人	聯絡人電話	聯絡人手機	聯絡人地址	聯絡人Email								
            //OOO	042OOO	09OOO	台北市OOO	TEST@OOO								
            //OOO	042OOO	09OOO	台北市OOO	TEST@OOO
            #endregion

            StringBuilder strHTML = new StringBuilder();
            string reValue = string.Empty;

            if (SRContact_List.Count > 0)
            {
                strHTML.AppendLine("<div>");
                strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>[客戶聯絡窗口資訊]</p>");
                strHTML.AppendLine("    <table style='width:720pt;font-family:微軟正黑體;' align='left' border='1'>");
                strHTML.AppendLine("        <tr>");
                strHTML.AppendLine("            <td>聯絡人</td>");
                strHTML.AppendLine("            <td>聯絡人電話</td>");
                strHTML.AppendLine("            <td>聯絡人手機</td>");
                strHTML.AppendLine("            <td>聯絡人地址</td>");
                strHTML.AppendLine("            <td>聯絡人Email</td>");
                strHTML.AppendLine("        </tr>");

                foreach (var bean in SRContact_List)
                {
                    strHTML.AppendLine("        <tr>");
                    strHTML.AppendLine("            <td>" + bean.CONTNAME + "</td>");
                    strHTML.AppendLine("            <td>" + bean.CONTTEL + "</td>");
                    strHTML.AppendLine("            <td>" + bean.CONTMOBILE + "</td>");
                    strHTML.AppendLine("            <td>" + bean.CONTADDR + "</td>");
                    strHTML.AppendLine("            <td>" + bean.CONTEMAIL + "</td>");
                    strHTML.AppendLine("        </tr>");
                }

                strHTML.AppendLine("    </table>");
                strHTML.AppendLine("</div>");
                strHTML.AppendLine("<p>&nbsp;</p>");
                strHTML.AppendLine("<p>&nbsp;</p>");
            }

            reValue = strHTML.ToString();

            return reValue;
        }
        #endregion

        #region 取得【一般服務】案件產品序號資訊Html Table
        /// <summary>
        /// 取得【一般服務】案件產品序號資訊Html Table
        /// </summary>
        /// <param name="SRSeiral_List">服務請求產品序號資訊清單</param>
        /// <returns></returns>
        public string findGenerallySRSeiral_Table(List<SRSERIALMATERIALINFO> SRSeiral_List)
        {
            #region 格式
            //[產品序號資訊]
            //序號	機器型號	Product Number	料號	裝機資訊
            //SGH1OOO	DL360pG8OOO	654081OOO	507281OOO	63OOO								
            //SGH2OOO	DL360pG8OOO	654082OOO	507282OOO	63OOO
            #endregion

            StringBuilder strHTML = new StringBuilder();
            string reValue = string.Empty;

            if (SRSeiral_List.Count > 0)
            {
                strHTML.AppendLine("<div>");
                strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>[產品序號資訊]</p>");
                strHTML.AppendLine("    <table style='width:720pt;font-family:微軟正黑體;' align='left' border='1'>");
                strHTML.AppendLine("        <tr>");
                strHTML.AppendLine("            <td>序號</td>");
                strHTML.AppendLine("            <td>機器型號</td>");
                strHTML.AppendLine("            <td>Product Number</td>");
                strHTML.AppendLine("            <td>料號</td>");
                strHTML.AppendLine("            <td>裝機資訊</td>");
                strHTML.AppendLine("        </tr>");

                foreach (var bean in SRSeiral_List)
                {
                    strHTML.AppendLine("        <tr>");
                    strHTML.AppendLine("            <td>" + bean.SerialID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.MaterialName + "</td>");
                    strHTML.AppendLine("            <td>" + bean.ProductNumber + "</td>");
                    strHTML.AppendLine("            <td>" + bean.MaterialID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.InstallID + "</td>");
                    strHTML.AppendLine("        </tr>");
                }

                strHTML.AppendLine("    </table>");
                strHTML.AppendLine("</div>");
                strHTML.AppendLine("<p>&nbsp;</p>");
                strHTML.AppendLine("<p>&nbsp;</p>");
            }

            reValue = strHTML.ToString();

            return reValue;
        }
        #endregion

        #region 取得【一般服務】案件零件更換資訊Html Table
        /// <summary>
        /// 取得【一般服務】案件零件更換資訊Html Table
        /// </summary>
        /// <param name="SRParts_List">服務請求零件更換資訊清單</param>
        /// <returns></returns>
        public string findGenerallySRParts_Table(List<SRPARTSREPALCEINFO> SRParts_List)
        {
            #region 格式
            //[零件更換資訊]												
            //XC HP申請零件	更換零件料號ID	料號說明	Old CT	New CT	HP CT	New UEFI	備機序號	HP Case ID	到貨日	歸還日	是否有人損	備註
            //OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	Y	OOO
            //OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	N	OOO	
            #endregion

            StringBuilder strHTML = new StringBuilder();
            string reValue = string.Empty;

            if (SRParts_List.Count > 0)
            {
                strHTML.AppendLine("<div>");
                strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>[零件更換資訊]</p>");
                strHTML.AppendLine("    <table style='width:720pt;font-family:微軟正黑體;' align='left' border='1'>");
                strHTML.AppendLine("        <tr>");
                strHTML.AppendLine("            <td>XC HP申請零件</td>");
                strHTML.AppendLine("            <td>更換零件料號ID</td>");
                strHTML.AppendLine("            <td>料號說明</td>");
                strHTML.AppendLine("            <td>Old CT</td>");
                strHTML.AppendLine("            <td>New CT</td>");
                strHTML.AppendLine("            <td>HP CT</td>");
                strHTML.AppendLine("            <td>New UEFI</td>");
                strHTML.AppendLine("            <td>備機序號</td>");
                strHTML.AppendLine("            <td>HP Case ID</td>");
                strHTML.AppendLine("            <td>到貨日</td>");
                strHTML.AppendLine("            <td>歸還日</td>");
                strHTML.AppendLine("            <td>是否有人損</td>");
                strHTML.AppendLine("            <td>備註</td>");
                strHTML.AppendLine("        </tr>");

                foreach (var bean in SRParts_List)
                {
                    strHTML.AppendLine("        <tr>");
                    strHTML.AppendLine("            <td>" + bean.XCHP + "</td>");
                    strHTML.AppendLine("            <td>" + bean.MaterialID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.MaterialName + "</td>");
                    strHTML.AppendLine("            <td>" + bean.OldCT + "</td>");
                    strHTML.AppendLine("            <td>" + bean.NewCT + "</td>");
                    strHTML.AppendLine("            <td>" + bean.HPCT + "</td>");
                    strHTML.AppendLine("            <td>" + bean.NewUEFI + "</td>");
                    strHTML.AppendLine("            <td>" + bean.StandbySerialID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.HPCaseID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.ArriveDate + "</td>");
                    strHTML.AppendLine("            <td>" + bean.ReturnDate + "</td>");
                    strHTML.AppendLine("            <td>" + bean.PersonalDamage + "</td>");
                    strHTML.AppendLine("            <td>" + bean.Note.Replace("\r\n", "<br/>") + "</td>");
                    strHTML.AppendLine("        </tr>");
                }

                strHTML.AppendLine("    </table>");
                strHTML.AppendLine("</div>");
                strHTML.AppendLine("<p>&nbsp;</p>");
                strHTML.AppendLine("<p>&nbsp;</p>");
            }

            reValue = strHTML.ToString();

            return reValue;
        }
        #endregion

        #region 組服務請求Mail相關資訊
        /// <summary>
        /// 組服務請求Mail相關資訊
        /// </summary>
        /// <param name="cCondition">服務請求執行條件(ADD.新建、TRANS.轉派L2工程師、REJECT.駁回、SECFIX.二修、SAVE.保存、THRPARTY.3Party、CANCEL.取消、DONE.完修)</param>
        /// <param name="cOperationID_GenerallySR">程式作業編號檔系統ID</param>
        /// <param name="cBUKRS">公司別(T012、T016、C069、T022)</param>
        /// <param name="cSRID">SRID(服務案件ID)</param>           
        /// <param name="tONEURLName">One Service站台名稱</param>        
        /// <param name="cLoginName">登入人員姓名</param>
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>
        public void SetSRMailContent(SRCondition cCondition, string cOperationID_GenerallySR, string cBUKRS, string cSRID, string tONEURLName, string cLoginName, bool tIsFormal)
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

            string cStatus = string.Empty;          //狀態
            string cStatusDesc = string.Empty;      //狀態說明            
            string cCreatedDate = string.Empty;     //派單時間
            string cContractID = string.Empty;      //合約文件編號
            string cMAServiceType = string.Empty;   //維護服務種類
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
                List<SRTEAMORGINFO> SRTeam_List = new List<SRTEAMORGINFO>();
                List<SREMPINFO> SRMainENG_List = new List<SREMPINFO>();
                List<SREMPINFO> SRAssENG_List = new List<SREMPINFO>();
                List<SREMPINFO> SRTechMGR_List = new List<SREMPINFO>();
                List<SRCONTACTINFO> SRRepair_List = new List<SRCONTACTINFO>();
                List<SRCONTACTINFO> SRContact_List = new List<SRCONTACTINFO>();
                List<SRSERIALMATERIALINFO> SRSeiral_List = new List<SRSERIALMATERIALINFO>();
                List<SRPARTSREPALCEINFO> SRParts_List = new List<SRPARTSREPALCEINFO>();                

                var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == cSRID);

                if (beanM != null)
                {
                    #region -----↓↓↓↓↓主檔 ↓↓↓↓↓-----
                    SRIDMAININFO SRMain = new SRIDMAININFO();

                    #region 服務團隊相關
                    SRTeam_List = findSRTEAMORGINFO(beanM.cTeamID);
                    cSRCase = findSRIDType(cSRID);
                    cTeamName = findSRTeamName(SRTeam_List);
                    cTeamMGR = findSRTeamMGRName(SRTeam_List);
                    cTeamMGREmail = findSRTeamMGREmail(SRTeam_List);
                    #endregion

                    #region L2工程師/指派工程師/技術主管相關
                    SRMainENG_List = findSREMPINFO(beanM.cMainEngineerID);
                    cMainENG = findSREMPName(SRMainENG_List);
                    cMainENGEmail = findSREMPEmail(SRMainENG_List);

                    SRAssENG_List = findSREMPINFO(beanM.cAssEngineerID);
                    cAssENG = findSREMPName(SRAssENG_List);
                    cAssENGEmail = findSREMPEmail(SRAssENG_List);

                    SRTechMGR_List = findSREMPINFO(beanM.cTechManagerID);
                    cTechMGR = findSREMPName(SRTechMGR_List);
                    cTechMGREmail = findSREMPEmail(SRTechMGR_List);
                    #endregion                  

                    #region 客戶報修窗口資料
                    cCusName = beanM.cCustomerName;
                    cRepairName = beanM.cRepairName;
                    cRepairPhone = beanM.cRepairPhone;
                    cRepairMobile = beanM.cRepairMobile;
                    cRepairAddress = beanM.cRepairAddress;
                    cRepairEmail = beanM.cRepairEmail;
                    #endregion

                    #region 其他主檔相關
                    cContractID = findSRContractID(cSRID);
                    cCreatedDate = Convert.ToDateTime(beanM.CreatedDate).ToString("yyyy-MM-dd");
                    cStatus = beanM.cStatus;
                    cStatusDesc = findSysParameterDescription(cOperationID_GenerallySR, "OTHER", cBUKRS, "SRSTATUS", beanM.cStatus);                    
                    cMAServiceType = findSysParameterDescription(cOperationID_GenerallySR, "OTHER", cBUKRS, "SRMATYPE", beanM.cMAServiceType); 
                    cSecFix = beanM.cIsSecondFix;
                    cDesc = beanM.cDesc;
                    cNotes = beanM.cNotes;
                    #endregion

                    SRMain.SRID = cSRID;
                    SRMain.Status = cStatus;
                    SRMain.StatusDesc = cStatusDesc;
                    SRMain.SRCase = cSRCase;
                    SRMain.TeamNAME = cTeamName;
                    SRMain.TeamMGR = cTeamMGR;
                    SRMain.MainENG = cMainENG;
                    SRMain.AssENG = cAssENG;
                    SRMain.TechMGR = cTechMGR;
                    SRMain.ContractID = cContractID;
                    SRMain.CreatedDate = cCreatedDate;
                    SRMain.MAServiceType = cMAServiceType;
                    SRMain.SecFix = cSecFix;
                    SRMain.Desc = cDesc;
                    SRMain.Notes = cNotes;

                    SRMain.CusName = cCusName;
                    SRMain.RepairName = cRepairName;
                    SRMain.RepairPhone = cRepairPhone;
                    SRMain.RepairMobile = cRepairMobile;
                    SRMain.RepairAddress = cRepairAddress;
                    SRMain.RepairEmail = cRepairEmail;

                    SRMain.TeamMGREmail = cTeamMGREmail;
                    SRMain.MainENGEmail = cMainENGEmail;
                    SRMain.AssENGEmail = cAssENGEmail;
                    SRMain.TechMGREmail = cTechMGREmail;
                    #endregion -----↑↑↑↑↑Mail相關 ↑↑↑↑↑-----  

                    #region -----↓↓↓↓↓客戶報修窗口資料 ↓↓↓↓↓-----
                    SRRepair_List = findSRREPAIRINFO(cSRID, cRepairName, cRepairPhone, cRepairMobile, cRepairAddress, cRepairEmail);
                    #endregion -----↑↑↑↑↑客戶報修窗口資料 ↑↑↑↑↑----- 

                    #region -----↓↓↓↓↓客戶聯絡窗口資料 ↓↓↓↓↓-----
                    SRContact_List = findSRCONTACTINFO(cSRID);
                    #endregion -----↑↑↑↑↑客戶聯絡窗口資料 ↑↑↑↑↑----- 

                    #region -----↓↓↓↓↓產品序號資訊 ↓↓↓↓↓-----
                    SRSeiral_List = findSRSERIALMATERIALINFO(cSRID);
                    #endregion -----↑↑↑↑↑產品序號資訊 ↑↑↑↑↑----- 

                    #region -----↓↓↓↓↓零件更換資訊 ↓↓↓↓↓-----
                    SRParts_List = findSRPARTSREPALCEINFO(cSRID);
                    #endregion -----↑↑↑↑↑零件更換資訊 ↑↑↑↑↑----- 

                    SendSRMail(cCondition, cSRID, tONEURLName, cLoginName, tIsFormal, SRMain, SRRepair_List, SRContact_List, SRSeiral_List, SRParts_List); //發送服務請求Mail相關資訊
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                writeToLog(cSRID, "SetSRMailContent", pMsg, cLoginName);
            }
        }
        #endregion

        #region 發送服務請求Mail相關資訊
        /// <summary>
        /// 發送服務請求Mail相關資訊
        /// </summary>
        /// <param name="cCondition">服務請求執行條件(ADD.新建、TRANS.轉派L2工程師、REJECT.駁回、SECFIX.二修、SAVE.保存、THRPARTY.3Party、CANCEL.取消)</param>
        /// <param name="cSRID">SRID</param>
        /// <param name="tONEURLName">One Service站台名稱</param>
        /// <param name="cLoginName">登入人員姓名</param>
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>
        /// <param name="SRMain">服務請求主檔資訊(For Mail)</param>
        /// <param name="SRRepair_List">服務請求客戶報修人資訊清單</param>
        /// <param name="SRContact_List">服務請求客戶聯絡人資訊清單</param>
        /// <param name="SRSeiral_List">服務請求產品序號資訊清單</param>
        /// <param name="SRParts_List">服務請求零件更換資訊清單</param>
        public void SendSRMail(SRCondition cCondition, string cSRID, string tONEURLName, string cLoginName, bool tIsFormal, SRIDMAININFO SRMain, 
                              List<SRCONTACTINFO> SRRepair_List, List<SRCONTACTINFO> SRContact_List, List<SRSERIALMATERIALINFO> SRSeiral_List, List<SRPARTSREPALCEINFO> SRParts_List)
        {
            List<string> tMailToList = new List<string>();
            List<string> tMailCcList = new List<string>();
            List<string> tMailBCcList = new List<string>();

            string tMailToTemp = string.Empty;
            string tMailCcTemp = string.Empty;
            string tMailBCcTemp = string.Empty;

            string tMailTo = string.Empty;          //收件者            
            string tMailCc = string.Empty;          //副本            
            string tMailBCc = string.Empty;         //密件副本
            string tHypeLink = string.Empty;        //超連結            

            string tStatus = string.Empty;          //狀態(E0001.新建、E0002.L2處理中、E0003.報價中、E0004.3rd Party處理中、E0005.L3處理中、E0006.完修、E0012.HPGCSN 申請、E0013.HPGCSN 完成、E0014.駁回、E0015.取消 )
            string tContractID = string.Empty;      //合約文件編號
            string tSecFix = string.Empty;          //是否為二修
            string tSRRepair_Table = string.Empty;
            string tSRContact_Table = string.Empty;
            string tSRSeiral_Table = string.Empty;
            string tSRParts_Table = string.Empty;

            try
            {
                #region 取得收件者
                if (SRMain.MainENGEmail != "") //有指派L2工程師
                {
                    tMailToTemp = SRMain.MainENGEmail + SRMain.AssENGEmail + SRMain.TechMGREmail;
                }
                else //未指派L2工程師
                {
                    tMailToTemp = SRMain.TeamMGREmail + SRMain.MainENGEmail + SRMain.AssENGEmail + SRMain.TechMGREmail;
                }

                if (tMailToTemp != "")
                {
                    foreach (string tValue in tMailToTemp.TrimEnd(';').Split(';'))
                    {
                        if (!tMailToList.Contains(tValue))
                        {
                            tMailToList.Add(tValue);

                            tMailTo += tValue + ";";
                        }
                    }

                    tMailTo = tMailTo.TrimEnd(';');
                }
                #endregion

                #region 取得副本
                if (SRMain.MainENGEmail != "") //有指派L2工程師
                {
                    tMailCcTemp = SRMain.TeamMGREmail;
                }               

                if (tMailCcTemp != "")
                {
                    foreach (string tValue in tMailCcTemp.TrimEnd(';').Split(';'))
                    {
                        if (!tMailCcList.Contains(tValue))
                        {
                            tMailCcList.Add(tValue);

                            tMailCc += tValue + ";";
                        }
                    }

                    tMailCc = tMailCc.TrimEnd(';');
                }
                #endregion

                #region 取得密件副本
                tMailBCcTemp = "Elvis.Chang@etatung.com"; //測試用，等都正常了就註解掉

                if (tMailBCcTemp != "")
                {
                    foreach (string tValue in tMailBCcTemp.TrimEnd(';').Split(';'))
                    {
                        if (!tMailBCcList.Contains(tValue))
                        {
                            tMailBCcList.Add(tValue);

                            tMailBCc += tValue + ";";
                        }
                    }

                    tMailBCc = tMailBCc.TrimEnd(';');
                }
                #endregion

                #region 是否為測試區
                string strTest = string.Empty;

                if (!tIsFormal)
                {
                    strTest = "【*測試*】";
                }
                #endregion

                #region 郵件主旨
                string tMailSubject = findGenerallySRMailSubject(cCondition, cSRID, SRMain.CusName, SRMain.TeamNAME, SRMain.SRCase, SRMain.MainENG);

                tMailSubject = strTest + tMailSubject;
                #endregion

                #region 郵件內容

                #region 內容格式參考(一般服務)                
                //親愛的主管/同仁您好，	
                //[服務案件明細]	
                //服務案件ID:	61OOO
                //服務案件種類:	一般服務
                //服務團隊:	L2電腦系統(含PS)-北區
                //服務團隊主管:	L2MGR
                //L2工程師:	L3MGR
                //指派工程師:	ASSEngineer
                //技術主管:	TechMGR
                //狀態:	處理中
                //派單時間:	2016/1/5 13:36
                //合約文件編號:	2540/7/23 00:00
                //維護服務種類:	保固內
                //是否為二修:	是
                //需求說明:	pls. support BUG FIX onsite
                //詳細描述:	硬體破損

                //客戶名稱：OOO股份有限公司
                //[客戶報修窗口資料]												
                //報修人	報修人電話	報修人手機	報修人地址	報修人Email								
                //OOO	042OOO	09OOO	台北市OOO	TEST@OOO	

                //[客戶聯絡窗口資料]												
                //聯絡人	聯絡人電話	聯絡人手機	聯絡人地址	聯絡人Email								
                //OOO	042OOO	09OOO	台北市OOO	TEST@OOO								
                //OOO	042OOO	09OOO	台北市OOO	TEST@OOO								

                //[產品序號資訊]												
                //序號	機器型號	Product Number	料號	裝機資訊								
                //SGH1OOO	DL360pG8OOO	654081OOO	507281OOO	63OOO								
                //SGH2OOO	DL360pG8OOO	654082OOO	507282OOO	63OOO								

                //[零件更換資訊]												
                //XC HP申請零件	更換零件料號ID	料號說明	Old CT	New CT	HP CT	New UEFI	備機序號	HP Case ID	到貨日	歸還日	是否有人損	備註
                //OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	Y	OOO
                //OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	OOO	N	OOO	

                //請儘速至One Sevice 系統處理，謝謝！
                //查看待辦清單 =>超連結(http://172.31.7.56:32200/ServiceRequest/ToDoList)
                //提醒您：此為系統發送信函請勿直接回覆此信。                

                //-------此信件由系統管理員發出，請勿回覆此信件-------
                #endregion

                string tMailBody = string.Empty;

                if (tStatus == "E0015") //取消
                {
                    tHypeLink = "http://" + tONEURLName + "/ServiceRequest/GenerallySR?SRID=" + cSRID;
                }
                else
                {
                    tHypeLink = "http://" + tONEURLName + "/ServiceRequest/ToDoList";
                }

                if (SRMain.ContractID != "")
                {
                    tContractID = "<p>合約文件編號：【" + SRMain.ContractID + "】</p>";
                }
                
                if (SRMain.SecFix == "Y")
                {
                    tSecFix = "<p>是否為二修：【" + SRMain.SecFix + "】</p>";
                }

                #region 取得【一般服務】案件客戶報修窗口資訊Html Table
                tSRRepair_Table = findGenerallySRRepair_Table(SRRepair_List, SRMain.CusName);
                #endregion

                #region 取得【一般服務】案件客戶聯絡窗口資訊Html Table
                tSRContact_Table = findGenerallySRContact_Table(SRContact_List);
                #endregion

                #region 取得【一般服務】案件產品序號資訊Html Table
                tSRSeiral_Table = findGenerallySRSeiral_Table(SRSeiral_List);
                #endregion

                #region 取得【一般服務】案件零件更換資訊Html Table
                tSRParts_Table = findGenerallySRParts_Table(SRParts_List);
                #endregion

                tMailBody = GetMailBody("ONEGenerally_MAIL");

                tMailBody = tMailBody.Replace("【<SRID>】", cSRID).Replace("【<SRCase>】", SRMain.SRCase).Replace("【<TeamNAME>】", SRMain.TeamNAME);
                tMailBody = tMailBody.Replace("【<TeamMGR>】", SRMain.TeamMGR).Replace("【<MainENG>】", SRMain.MainENG).Replace("【<AssENG>】", SRMain.AssENG);                
                tMailBody = tMailBody.Replace("【<TechMGR>】", SRMain.TechMGR).Replace("【<StatusDesc>】", SRMain.StatusDesc).Replace("【<CreatedDate>】", SRMain.CreatedDate);
                tMailBody = tMailBody.Replace("<ContractID>", tContractID).Replace("【<MAServiceType>】", SRMain.MAServiceType).Replace("<SecFix>", tSecFix);
                tMailBody = tMailBody.Replace("【<Desc>】", SRMain.Desc).Replace("【<Notes>】", SRMain.Notes);                
                tMailBody = tMailBody.Replace("<SRRepair_List>", tSRRepair_Table).Replace("<SRContact_List>", tSRContact_Table);
                tMailBody = tMailBody.Replace("<SRSeiral_List>", tSRSeiral_Table).Replace("<SRParts_List>", tSRParts_Table);
                tMailBody = tMailBody.Replace("【<tHypeLink>】", tHypeLink);
                #endregion

                #region 測試用
                tMailTo = "elvis.chang@etatung.com";
                tMailCc = "";
                tMailBCc = "";
                #endregion

                //呼叫寄送Mail
                SendMailByAPI("SendSRMail_API", null, tMailTo, tMailCc, tMailBCc, tMailSubject, tMailBody, "", "");
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                writeToLog(cSRID, "SendSRMail", pMsg, cLoginName);
            }
        }
        #endregion

        #region 呼叫發送服務報告書report給客戶
        /// <summary>
        /// 呼叫發送服務報告書report給客戶
        /// </summary>
        /// <param name="pOperationID_GenerallySR">程式作業編號檔系統ID(一般服務)</param>        
        /// <param name="srId">SRID</param>
        /// <param name="pdfPath">服務報告書檔案路徑</param>
        /// <param name="pdfFileName">服務報告書檔名</param>
        /// <param name="mainEgnrName">處理工程師姓名</param>
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>
        public void callSendReport(string pOperationID_GenerallySR, string srId, string pdfPath, string pdfFileName, string mainEgnrName, bool tIsFormal)
        {
            #region -- 發送Report給客戶 --
            string email = string.Empty;
            string CUSTOMER = string.Empty;
            string ENGINEER = string.Empty;
            string NOTES = string.Empty;

            // 獲得需求單明細資料
            Dictionary<string, object> srdetail = GetSRDetail(srId, pOperationID_GenerallySR);

            //設定寄信內容變數
            //如果客戶(報修人)沒有email，則先寄給rita，原本是空值
            email = (srdetail["EV_EMAIL"] != null && !String.IsNullOrEmpty(srdetail["EV_EMAIL"].ToString())) ? srdetail["EV_EMAIL"].ToString() : "";

            //客戶聯絡人
            if (srdetail["EV_EMAIL_R"] != null && !string.IsNullOrEmpty(srdetail["EV_EMAIL_R"].ToString()))
            {
                if (email.IndexOf(srdetail["EV_EMAIL_R"].ToString()) == -1)
                {
                    email += ";" + srdetail["EV_EMAIL_R"].ToString();
                }
            }

            CUSTOMER = srdetail["EV_CUSTOMER"].ToString();
            if (CUSTOMER.IndexOf(' ') != -1)
            {
                CUSTOMER = CUSTOMER.Split(' ')[0];
            }

            ENGINEER = string.IsNullOrEmpty(mainEgnrName) ? srdetail["EV_MAINENG"].ToString() : mainEgnrName;

            NOTES = srdetail["EV_PROBLEM"].ToString();

            //一併發送給L2工程師及支援工程師
            List<string> ccs = new List<string>();
            List<ENGProcessLIST> lbList = srdetail.ContainsKey("table_ET_LABORLIST") ? (List<ENGProcessLIST>)srdetail["table_ET_LABORLIST"] : new List<ENGProcessLIST>();

            foreach (var lbBean in lbList)
            {
                string _engrErpId = lbBean.ENGID;

                if (!string.IsNullOrEmpty(_engrErpId))
                {
                    string tEmail = findEMPEmail(_engrErpId);

                    if (tEmail != "" && !ccs.Contains(tEmail))
                    {
                        ccs.Add(tEmail);
                    }
                    else continue;
                }
                else continue;
            }         

            SendReport(email, string.Join(";", ccs), srId, CUSTOMER, ENGINEER, NOTES, pdfPath, pdfFileName, mainEgnrName, tIsFormal); //發送服務報告書report給客戶
            #endregion
        }
        #endregion

        #region 發送服務報告書report給客戶
        /// <summary>
        /// 發送服務報告書report給客戶
        /// </summary>
        /// <param name="receiver">收件人</param>
        /// <param name="ccs">副本</param>
        /// <param name="SRID">SRID</param>
        /// <param name="CUSTOMER">客戶名稱</param>
        /// <param name="ENGINEER">負責工程師姓名</param>
        /// <param name="NOTES">詳細描述</param>        
        /// <param name="pdfPath">服務報告書檔案路徑</param>
        /// <param name="pdfFileName">服務報告書檔名</param>
        /// <param name="mainEgnrName">處理工程師姓名</param>
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>
        /// <returns></returns>
        private void SendReport(string receiver, string ccs, string SRID, string CUSTOMER, string ENGINEER, string NOTES, string pdfPath, string pdfFileName, string mainEgnrName, bool tIsFormal)
        {
            #region 範例內容
            //<body style="font-family:微軟正黑體; ">親愛的用戶您好，<br/>" +
            //    <br/>您此次的服務已處理，提供服務報告書如附件，謝謝!!<br/>" +
            //    <br/>[服務明細]" +
            //    <br/>服務ID：【<SRID>】" +
            //    <br/>客戶名稱：【<CUSTOMER>】" +
            //    <br/>負責工程師：【<ENGINEER>】" +
            //    <br/>需求事項：【<DESC>】" +
            //    <br/>狀態：已處理<br/>" +
            //<body>
            #endregion

            try
            {
                string tMailSubject = string.Empty;
                string tMailBody = string.Empty;

                #region 是否為測試區
                string strTest = string.Empty;

                if (!tIsFormal)
                {
                    strTest = "【*測試*】";
                    receiver = "elvis.chang@etatung.com";
                    ccs = "elvis.chang@etatung.com";
                }
                #endregion

                tMailSubject = strTest + "[大同世界科技 服務ID：" + SRID + " 已處理通知]";

                tMailBody = GetMailBody("ONESendReport_MAIL");
                tMailBody = tMailBody.Replace("【<SRID>】", SRID).Replace("【<CUSTOMER>】", CUSTOMER);
                tMailBody = tMailBody.Replace("【<ENGINEER>】", ENGINEER).Replace("【<NOTES>】", NOTES);

                SendMailByAPI("SendReport_API", "Crmwebadmin@etatung.com", receiver, ccs, "", tMailSubject, tMailBody, pdfFileName, pdfPath);
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                writeToLog(SRID, "SendReport_API", pMsg, mainEgnrName);
                SendMailByAPI("SendReport_API", null, "elvis.chang@etatung.com", "", "", "發送服務報告書report給客戶錯誤 - " + SRID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "<br>" + ex.ToString(), null, null);
            }
        }
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