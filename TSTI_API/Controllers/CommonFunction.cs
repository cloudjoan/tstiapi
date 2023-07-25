#region 更新歷程
/*
注意：若要更新正式區，請將搜尋「【測試】」，將它註解，並反註解「【正式】」，webconfig記得也要調整


*/
#endregion

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
        TAIFEntities dbBPM = new TAIFEntities();

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

        #region 取得系統位址參數相關資訊
        /// <summary>
        /// 取得系統位址參數相關資訊
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>        
        /// <returns></returns>
        public SRSYSPARAINFO findSRSYSPARAINFO(string cOperationID)
        {
            SRSYSPARAINFO OUTBean = new SRSYSPARAINFO();

            bool tIsFormal = getCallSAPERPPara(cOperationID); //取得呼叫SAPERP參數是正式區或測試區(true.正式區 false.測試區)          

            OUTBean.IsFormal = tIsFormal;

            if (tIsFormal)
            {
                #region 正式區                
                OUTBean.ONEURLName = @"https://oneservice.etatung.com/";
                OUTBean.BPMURLName = "tsti-bpm01.etatung.com.tw";
                OUTBean.PSIPURLName = "psip-prd-ap";
                OUTBean.AttachURLName = "tsticrmmbgw.etatung.com:8081";
                #endregion
            }
            else
            {
                #region 測試區                
                OUTBean.ONEURLName = @"http://172.31.7.56:32200";
                OUTBean.BPMURLName = "tsti-bpm01.etatung.com.tw";
                OUTBean.PSIPURLName = "psip-prd-ap";
                OUTBean.AttachURLName = "tsticrmmbgw.etatung.com:8082";
                #endregion
            }

            return OUTBean;
        }
        #endregion

        #region 取得所有客服人員帳號和Email
        /// <summary>
        /// 取得所有客服人員帳號和Email
        /// </summary>
        /// <param name="cOperationID">程式作業編號檔系統ID</param>
        /// <returns></returns>
        public Dictionary<string,string> getCUSTOMERSERVICEInfo(string cOperationID)
        {
            Dictionary<string, string> Dic = new Dictionary<string, string>();

            string tEmail = string.Empty;

            List<SelectListItem> tList = findSysParameterList(cOperationID, "ACCOUNT", "ALL", "CUSTOMERSERVICE");
            
            foreach(var bean in tList)
            {
                tEmail = bean.Value.Replace("etatung\\", "") + "@etatung.com";
                
                Dic.Add(bean.Value, tEmail);
            }            

            return Dic;
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

            var beans = dbPSIP.TB_ONE_SysParameter.OrderBy(x => x.cOperationID).ThenBy(x => x.cFunctionID).ThenBy(x => x.cCompanyID).ThenBy(x => x.cNo).ThenBy(x => x.cValue).
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
        public List<VIEW_CUSTOMER_WithAddress> findCUSTOMERINFO(string keyword)
        {
            List<VIEW_CUSTOMER_WithAddress> tList = new List<VIEW_CUSTOMER_WithAddress>();

            if (keyword != "")
            {
                tList = dbProxy.VIEW_CUSTOMER_WithAddress.Where(x => x.KNA1_KUNNR.Contains(keyword.Trim()) || x.KNA1_NAME1.Contains(keyword.Trim())).Take(30).ToList();               
            }

            return tList;
        }
        #endregion

        #region 取得法人客戶聯絡人資料
        /// <summary>
        /// 取得法人客戶聯絡人資料
        /// </summary>
        /// <param name="CustomerID">客戶代號/名稱</param>
        /// <param name="CONTACTNAME">聯絡人姓名</param>        
        /// <param name="CONTACTTEL">聯絡人電話</param>
        /// <param name="CONTACTMOBILE">聯絡人手機</param>
        /// <param name="CONTACTEMAIL">聯絡人Email</param>
        /// <returns></returns>
        public List<PCustomerContact> findCONTACTINFO(string CustomerID, string CONTACTNAME, string CONTACTTEL, string CONTACTMOBILE, string CONTACTEMAIL)
        {
            #region 註解
            //var qPjRec = dbProxy.CUSTOMER_Contact.OrderByDescending(x => x.ModifiedDate).
            //                                   Where(x => (x.Disabled == null || x.Disabled != 1) &&
            //                                              x.ContactName != "" && x.ContactCity != "" && x.ContactAddress != "" &&
            //                                              (x.ContactPhone != "" || (x.ContactMobile != "" && x.ContactMobile != null)) &&
            //                                              (string.IsNullOrEmpty(CustomerID) ? true : (x.KNA1_KUNNR.Contains(CustomerID) || x.KNA1_NAME1.Contains(CustomerID))) &&
            //                                              (string.IsNullOrEmpty(CONTACTNAME) ? true : x.ContactName.Contains(CONTACTNAME)) &&
            //                                              (string.IsNullOrEmpty(CONTACTTEL) ? true : x.ContactPhone.Contains(CONTACTTEL)) &&
            //                                              (string.IsNullOrEmpty(CONTACTMOBILE) ? true : x.ContactMobile.Contains(CONTACTMOBILE)) &&
            //                                              (string.IsNullOrEmpty(CONTACTEMAIL) ? true : x.ContactEmail.Contains(CONTACTEMAIL))).ToList();
            #endregion

            var qPjRec = dbProxy.CUSTOMER_Contact.OrderByDescending(x => x.ModifiedDate).
                                               Where(x => (x.Disabled == null || x.Disabled != 1) &&
                                                          x.ContactName != "" && x.ContactCity != "" && x.ContactAddress != "" &&                                                          
                                                          (string.IsNullOrEmpty(CustomerID) ? true : (x.KNA1_KUNNR.Contains(CustomerID) || x.KNA1_NAME1.Contains(CustomerID))) &&
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
                    tTempValue = prBean.KNA1_KUNNR.Trim().Replace(" ", "") + "|" + prBean.ContactName.Trim().Replace(" ", "");

                    if (!tTempList.Contains(tTempValue)) //判斷客戶ID、聯絡人姓名不重覆才要顯示
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
                        prDocBean.Store = "";
                        prDocBean.StoreName = "";
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
        /// <param name="PERSONALID">個人客戶代號/名稱</param>
        /// <param name="CONTACTNAME">聯絡人姓名</param>        
        /// <param name="CONTACTTEL">聯絡人電話</param>
        /// <param name="CONTACTMOBILE">聯絡人手機</param>
        /// <param name="CONTACTEMAIL">聯絡人Email</param>
        /// <returns></returns>
        public List<PCustomerContact> findPERSONALCONTACTINFO(string PERSONALID, string CONTACTNAME, string CONTACTTEL, string CONTACTMOBILE, string CONTACTEMAIL)
        {
            var qPjRec = dbProxy.PERSONAL_Contact.OrderByDescending(x => x.ModifiedDate).
                                               Where(x => x.Disabled == 0 && 
                                                          x.ContactName != "" && x.ContactCity != "" && x.ContactAddress != "" &&                                                           
                                                          (string.IsNullOrEmpty(PERSONALID) ? true : (x.KNA1_KUNNR.Contains(PERSONALID) || x.KNA1_NAME1.Contains(PERSONALID))) &&
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

        #region 取得客戶報修進度相關資訊
        /// <summary>
        /// 取得客戶報修進度相關資訊
        /// </summary>
        /// <param name="IV_CUSTOMERID">法人客戶代號</param>
        /// <param name="IV_CONTACTNAME">報修人/聯絡人姓名</param>
        /// <param name="IV_STATUS">狀態(ALL.所有狀態、P.處理中、F.完修)</param>
        /// <returns></returns>
        public List<PROGRESS_LIST> findPROGRESSByCustomer(string IV_CUSTOMERID, string IV_CONTACTNAME, string IV_STATUS)
        {
            List<PROGRESS_LIST> tList = new List<PROGRESS_LIST>();

            StringBuilder tSQL = new StringBuilder();
            string ttWhere = string.Empty;            

            #region 客戶代號
            if (!string.IsNullOrEmpty(IV_CUSTOMERID))
            {
                ttWhere += "AND M.cCustomerID = '" + IV_CUSTOMERID + "' " + Environment.NewLine;
            }
            #endregion

            #region 報修人/聯絡人姓名
            if (!string.IsNullOrEmpty(IV_CONTACTNAME))
            {
                ttWhere += "AND (M.cRepairName like N'%" + IV_CONTACTNAME + "%' or  C.cContactName like N'%" + IV_CONTACTNAME + "%') " + Environment.NewLine;                
            }
            #endregion

            #region 狀態
            if (!string.IsNullOrEmpty(IV_STATUS))
            {
                if (IV_STATUS == "ALL")
                {
                    ttWhere += "AND M.cStatus NOT IN ('E0014','E0015') " + Environment.NewLine; //非駁回、非取消
                }
                else if (IV_STATUS == "P")
                {
                    ttWhere += "AND M.cStatus NOT IN ('E0014','E0015','E0006') " + Environment.NewLine; //非駁回、非取消、非完修
                }
                else
                {
                    ttWhere += "AND M.cStatus = 'E0006' " + Environment.NewLine; //完修
                }
            }
            #endregion

            #region SQL語法
            tSQL.AppendLine(" Select M.cSRID,M.cCustomerName,M.cDesc,M.CreatedDate,M.cStatus,M.cRepairName,");
            tSQL.AppendLine("        (Select top 1 sp.cSerialID + '＃＃' + sp.cMaterialName + '＃＃' + sp.cProductNumber");            
            tSQL.AppendLine("         From TB_ONE_SRDetail_Product sp where M.cSRID = sp.cSRID AND sp.disabled = 0");
            tSQL.AppendLine("        ) as Products");
            tSQL.AppendLine(" From TB_ONE_SRMain M");
            tSQL.AppendLine(" left join TB_ONE_SRDetail_Contact C on M.cSRID = C.cSRID and C.Disabled= 0 ");
            tSQL.AppendLine(" Where 1=1 AND substring(M.cSRID,1,2) = '61' " + ttWhere);            
            #endregion

            DataTable dt = getDataTableByDb(tSQL.ToString(), "dbOne");
            DataTable dtProgress = DistinctTable(dt);

            foreach(DataRow dr in dtProgress.Rows)
            {
                PROGRESS_LIST bean = new PROGRESS_LIST();

                bean.SRID = dr["cSRID"].ToString();
                bean.CUSTOMERNAME = dr["cCustomerName"].ToString();
                bean.REPAIRNAME = dr["cRepairName"].ToString();
                bean.SRDATE = Convert.ToDateTime(dr["CreatedDate"].ToString()).ToString("yyyy-MM-dd");                
                bean.PRODUCT = dr["Products"].ToString().Replace("＃＃", "_").TrimEnd('_');
                bean.DESC = dr["cDesc"].ToString();
                bean.STATUS = dr["cStatus"].ToString() == "E0006" ? "完修" : "處理中";

                tList.Add(bean);
            }

            return tList;
        }
        #endregion

        #region 客戶報修進度查詢Distinct SRID
        /// <summary>
        /// 客戶報修進度查詢Distinct SRID
        /// </summary>
        /// <param name="dtSource">傳入的DataTable</param>
        /// <returns></returns>
        public DataTable DistinctTable(DataTable dtSource)
        {
            DataTable dt = dtSource.Clone();
            DataTable dtDistinct = dtSource.DefaultView.ToTable(true, new string[] { "cSRID" }); //取得distinct SRID

            int count = dtDistinct.Rows.Count;

            for (int i = 0; i < count; i++)
            {
                string tFiler = "cSRID = '" + dtDistinct.Rows[i][0].ToString() + "'";

                DataRow[] drs = dtSource.Select(tFiler);

                foreach (DataRow dr in drs)
                {
                    #region 只要塞入第一筆DataTable
                    dt.Rows.Add(dr.ItemArray);
                    #endregion

                    break;
                }
            }

            return dt;
        }
        #endregion

        #region 取得員工資料
        /// <summary>
        /// 取得員工資料
        /// </summary>
        /// <param name="keyword">員工姓名(中文名/英文名)</param>
        /// <param name="IV_SRTEAM">服務團隊ID(多筆用;隔開)</param>
        /// <returns></returns>
        public List<Person> findEMPINFO(string keyword, string IV_SRTEAM)
        {
            List<Person> tList = new List<Person>();

            if (keyword != "")
            {
                if (IV_SRTEAM == "")
                {
                    tList = dbEIP.Person.Where(x => (x.Leave_Date == null && x.Leave_Reason == null) && (x.Name.Contains(keyword) || x.Name2.Contains(keyword))).Take(10).ToList();
                }
                else
                {
                    List<string> tDeptList = findALLDeptIDListbyTeamID(IV_SRTEAM);

                    tList = dbEIP.Person.Where(x => (x.Leave_Date == null && x.Leave_Reason == null) && (tDeptList.Contains(x.DeptID)) && (x.Name.Contains(keyword) || x.Name2.Contains(keyword))).Take(10).ToList();
                }
            }

            return tList;
        }
        #endregion

        #region 取得服務團隊ID下所對應的部門(含子部門)
        /// <summary>
        /// 取得服務團隊ID下所對應的部門(含子部門)
        /// </summary>
        /// <param name="cTeamOldId">服務團隊ID(多筆用;隔開)</param>
        /// <returns></returns>
        public List<string> findALLDeptIDListbyTeamID(string cTeamOldId)
        {
            List<string> tAllDeptIDList = new List<string>();

            if (cTeamOldId != "")
            {
                List<string> tDeptIDList = findDeptIDListbyTeamID(cTeamOldId);

                foreach (string tValue in tDeptIDList)
                {
                    List<string> tLIst = GetALLSubDeptID(tValue);

                    tAllDeptIDList.AddRange(tLIst);
                }
            }

            return tAllDeptIDList;
        }
        #endregion

        #region 取得服務團隊ID所對應的部門代號清單
        /// <summary>
        /// 取得服務團隊ID所對應的部門代號清單
        /// </summary>
        /// <param name="cTeamOldId">服務團隊ID</param>
        /// <returns></returns>
        public List<string> findDeptIDListbyTeamID(string cTeamOldId)
        {
            List<string> tList = new List<string>();
            List<string> tTeamList = cTeamOldId.Split(';').ToList();

            string reValue = string.Empty;

            var beans = dbOne.TB_ONE_SRTeamMapping.Where(x => x.Disabled == 0 && tTeamList.Contains(x.cTeamOldID));

            foreach (var bean in beans)
            {
                if (!tList.Contains(bean.cTeamNewID))
                {
                    tList.Add(bean.cTeamNewID);
                }
            }

            return tList;
        }
        #endregion

        #region 取得所有組織的DataTable
        /// <summary>
        /// 取得所有組織的DataTable
        /// </summary>
        /// <returns></returns>
        protected DataTable GetOrgDt()
        {
            DataTable dt = new DataTable();

            string cmmStr = @"select ID,ParentID,Name,Level from Department where ((Status='0' and Level <> '0') or (Status='0' and ParentID IS NULL)) AND NOT(ID LIKE '9%' or ID like '12GH%') ";

            dt = getDataTableByDb(cmmStr, "dbEIP");

            return dt;
        }
        #endregion

        #region 傳入最上層部門ID，並取得底下所有子部門ID
        /// <summary>
        /// 傳入最上層部門ID，並取得底下所有子部門ID
        /// </summary>
        /// <param name="tParentID">上層部門ID</param>
        /// <returns></returns>
        public List<string> GetALLSubDeptID(string tParentID)
        {
            List<string> tList = new List<string>();

            string reValue = string.Empty;
            string tmpNodeID = string.Empty;
            string tAllDept = string.Empty;

            DataTable dt = GetOrgDt(); //取得所有組織的DataTable
            DataRow[] rows = dt.Select("ID = '" + tParentID + "'");

            bool rc;

            foreach (DataRow row in rows)
            {
                tmpNodeID = row["ID"].ToString();

                tAllDept = tmpNodeID + ",";

                rc = AddNodes_Dept(tmpNodeID, ref dt, ref tAllDept);
            }

            reValue = tAllDept.TrimEnd(',');

            tList = reValue.Split(',').ToList();

            return tList;
        }
        #endregion

        #region 取得子節點，遞廻(部門代號)
        private bool AddNodes_Dept(string PID, ref DataTable dt, ref string tAllDept)
        {
            try
            {
                string tmpNodeID;

                DataRow[] rows = dt.Select("ParentID = '" + PID + "'");

                if (rows.GetUpperBound(0) >= 0)
                {
                    bool rc;

                    foreach (DataRow row in rows)
                    {
                        tmpNodeID = row["ID"].ToString();

                        tAllDept += tmpNodeID + ",";

                        rc = AddNodes_Dept(tmpNodeID, ref dt, ref tAllDept);
                    }
                }

                rows = null;

                return true;
            }
            catch
            {
                return false;
            }
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
        /// <param name="tCompanyID">公司別(ALL、T012、T016、C069、T022)</param>
        /// <returns></returns>        
        public List<TB_ONE_SRTeamMapping> findSRTEAMINFO(string tCompanyID)
        {
            List<TB_ONE_SRTeamMapping> tList = new List<TB_ONE_SRTeamMapping>();

            string tSRVID = string.Empty;

            if (tCompanyID.ToUpper() != "ALL")
            {
                tSRVID = "SRV." + tCompanyID.Substring(2, 2);

                tList = dbOne.TB_ONE_SRTeamMapping.Where(x => x.Disabled == 0 && (x.cTeamOldID.Contains(tSRVID))).ToList();
            }
            else
            {
                tList = dbOne.TB_ONE_SRTeamMapping.Where(x => x.Disabled == 0).ToList();
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

            List<string> tList = new List<string>();

            foreach(var bean in SRTeam)
            {
                if (!tList.Contains(bean.TEAMNAME))
                {
                    reValue += bean.TEAMNAME + ";";
                    
                    tList.Add(bean.TEAMNAME);
                }
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

        #region 取得服務案件主要工程師/協助工程師/技術主管，員工ERPID_中文+英文姓名
        /// <summary>
        /// 服務案件主要工程師/協助工程師/技術主管相關資訊，員工ERPID_中文+英文姓名
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

        #region 取得服務案件主要工程師/協助工程師/技術主管相關資訊
        /// <summary>
        /// 服務案件主要工程師/協助工程師/技術主管相關資訊
        /// </summary>
        /// <param name="cERPID">員工編號(多筆以;號隔開)</param>
        /// <returns></returns>        
        public List<SREMPINFO> findSREMPINFO(string cERPID)
        {
            List<SREMPINFO> tList = new List<SREMPINFO>();

            cERPID = string.IsNullOrEmpty(cERPID) ? "" : cERPID;

            if (cERPID != "")
            {
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
            }

            return tList;
        }
        #endregion

        #region 取得服務案件主要工程師/協助工程師/技術主管姓名
        /// <summary>
        /// 取得服務案件主要工程師/協助工程師/技術主管姓名
        /// </summary>
        /// <param name="SREmp">服務案件主要工程師/協助工程師/技術主管相關資訊清單</param>
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

        #region 取得服務案件主要工程師/協助工程師/技術主管Email
        /// <summary>
        /// 取得服務案件主要工程師/協助工程師/技術主管Email
        /// </summary>
        /// <param name="SREmp">服務案件主要工程師/協助工程師/技術主管相關資訊清單</param>
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
                SRSerial.SERIALID = bean.cSerialID;
                SRSerial.NEWSERIALID = bean.cNewSerialID;
                SRSerial.PRDID = bean.cMaterialID;
                SRSerial.PRDNAME = bean.cMaterialName;
                SRSerial.PRDNUMBER = bean.cProductNumber;
                SRSerial.INSTALLID = bean.cInstallID;

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

        #region 取得附件/服務報告書URL(多筆以;號隔開)含附件原始檔名
        /// <summary>
        /// 取得附件/服務報告書URL(多筆以;號隔開)含附件原始檔名
        /// </summary>
        /// <param name="tAttach">附件GUID(多筆以,號隔開)</param>
        /// <param name="tAttachURLName">附件URL站台名稱</param>
        /// <returns></returns>
        public string findAttachUrlWithName(string tAttach, string tAttachURLName)
        {
            string reValue = string.Empty;

            List<SRATTACHINFO> SR_List = findSRATTACHINFO(tAttach, tAttachURLName);

            foreach (var bean in SR_List)
            {
                reValue += bean.FILE_ORG_NAME + "|" + bean.FILE_URL + ";";
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

            if (tAttach != "")
            {
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
            }

            return tList;
        }
        #endregion

        #region 服務報告書/附件相關資訊
        /// <summary>
        /// 服務報告書/附件相關資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <param name="tAttachURLName">附件URL站台名稱</param>
        /// <param name="tAttachPath">附件路徑名稱</param>
        /// <returns></returns>        
        public List<SRREPORTINFO> findSRREPORTINFO(string cSRID, string tAttachURLName, string tAttachPath)
        {
            List<SRREPORTINFO> tList = new List<SRREPORTINFO>();

            string cReceiveTime = string.Empty;
            string cStartTime = string.Empty;
            string cArriveTime = string.Empty;
            string cFinishTime = string.Empty;
            string cURLName = string.Empty;
            string cSRReportURL = string.Empty;

            var beans = dbOne.TB_ONE_SRDetail_Record.Where(x => x.Disabled == 0 && x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                List<SRATTACHINFO> SRAttach = findSRATTACHINFO(bean.cSRReport, tAttachURLName);

                foreach (var SRbean in SRAttach)
                {
                    SRREPORTINFO SRReport = new SRREPORTINFO();

                    SRReport.CID = bean.cID.ToString();
                    SRReport.SRID = cSRID;
                    SRReport.SRReportORG_NAME = SRbean.FILE_ORG_NAME;
                    SRReport.SRReportNAME = SRbean.FILE_NAME;
                    SRReport.SRReportPath = Path.Combine(tAttachPath, SRbean.FILE_NAME);
                    SRReport.SRReportURL = SRbean.FILE_URL;

                    tList.Add(SRReport);
                    break;
                }
            }

            return tList;
        }
        #endregion

        #region 裝機Config相關資訊
        /// <summary>
        /// 裝機Config相關資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <param name="tAttachURLName">附件URL站台名稱</param>
        /// <param name="tAttachPath">附件路徑名稱</param>
        /// <returns></returns>        
        public List<SRREPORTINFO> findSRCONFIGINFO(string cSRID, string tAttachURLName, string tAttachPath)
        {
            List<SRREPORTINFO> tList = new List<SRREPORTINFO>();

            string cReceiveTime = string.Empty;
            string cStartTime = string.Empty;
            string cArriveTime = string.Empty;
            string cFinishTime = string.Empty;
            string cURLName = string.Empty;
            string cSRReportURL = string.Empty;

            var beans = dbOne.TB_ONE_SRDetail_SerialFeedback.Where(x => x.Disabled == 0 && x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                List<SRATTACHINFO> SRAttach = findSRATTACHINFO(bean.cConfigReport, tAttachURLName);

                foreach (var SRbean in SRAttach)
                {
                    SRREPORTINFO SRReport = new SRREPORTINFO();

                    #region 先暫時註解，預設這裡只會上傳裝機Config
                    //if (SRbean.FILE_ORG_NAME.IndexOf(cSRID) != -1) //原始檔名有含SRID才是裝機Config
                    //{
                    //    SRReport.SRID = cSRID;
                    //    SRReport.SRReportORG_NAME = SRbean.FILE_ORG_NAME;
                    //    SRReport.SRReportNAME = SRbean.FILE_NAME;
                    //    SRReport.SRReportPath = Path.Combine(tAttachPath, SRbean.FILE_NAME);
                    //    SRReport.SRReportURL = SRbean.FILE_URL;

                    //    tList.Add(SRReport);
                    //    break;
                    //}
                    #endregion

                    SRReport.CID = bean.cID.ToString();
                    SRReport.SRID = cSRID;
                    SRReport.SRReportORG_NAME = SRbean.FILE_ORG_NAME;
                    SRReport.SRReportNAME = SRbean.FILE_NAME;
                    SRReport.SRReportPath = Path.Combine(tAttachPath, SRbean.FILE_NAME);
                    SRReport.SRReportURL = SRbean.FILE_URL;

                    tList.Add(SRReport);
                    break;
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

        #region 取得物料訊息資訊
        /// <summary>
        /// 取得物料訊息資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <returns></returns>        
        public List<SRMATERIALlNFO> findSRMATERIALlNFO(string cSRID)
        {
            List<SRMATERIALlNFO> tList = new List<SRMATERIALlNFO>();            

            var beans = dbOne.TB_ONE_SRDetail_MaterialInfo.Where(x => x.Disabled == 0 && x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                SRMATERIALlNFO SRPart = new SRMATERIALlNFO();

                SRPart.CID = bean.cID.ToString();
                SRPart.SRID = bean.cSRID;                
                SRPart.MaterialID = bean.cMaterialID;
                SRPart.MaterialName = bean.cMaterialName;
                SRPart.Quantity = bean.cQuantity.ToString();
                SRPart.BasicContent = bean.cBasicContent;
                SRPart.MFPNumber = bean.cMFPNumber;
                SRPart.Brand = bean.cBrand;
                SRPart.ProductHierarchy = bean.cProductHierarchy;                

                tList.Add(SRPart);
            }

            return tList;
        }
        #endregion    

        #region 取得序號回報資訊
        /// <summary>
        /// 取得序號回報資訊
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <returns></returns>        
        public List<SRSERIALFEEDBACKlNFO> findSRSERIALFEEDBACKlNFO(string cSRID)
        {
            List<SRSERIALFEEDBACKlNFO> tList = new List<SRSERIALFEEDBACKlNFO>();

            var beans = dbOne.TB_ONE_SRDetail_SerialFeedback.Where(x => x.Disabled == 0 && x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                SRSERIALFEEDBACKlNFO SRPart = new SRSERIALFEEDBACKlNFO();

                SRPart.CID = bean.cID.ToString();
                SRPart.SRID = bean.cSRID;
                SRPart.SERIALID = bean.cSerialID;
                SRPart.MaterialID = bean.cMaterialID;
                SRPart.MaterialName = bean.cMaterialName;                

                tList.Add(SRPart);
            }

            return tList;
        }
        #endregion    

        #region 取得序號回報資訊(傳AttachURL)
        /// <summary>
        /// 取得序號回報資訊(傳AttachURL)
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <param name="tAttachURLName"></param>
        /// <returns></returns>        
        public List<SRSERIALFEEDBACKlNFO> findSRSERIALFEEDBACKlNFO(string cSRID, string tAttachURLName)
        {
            List<SRSERIALFEEDBACKlNFO> tList = new List<SRSERIALFEEDBACKlNFO>();

            string cConfigReportURL = string.Empty;

            var beans = dbOne.TB_ONE_SRDetail_SerialFeedback.Where(x => x.Disabled == 0 && x.cSRID == cSRID);

            foreach (var bean in beans)
            {
                SRSERIALFEEDBACKlNFO SRPart = new SRSERIALFEEDBACKlNFO();

                cConfigReportURL = findAttachUrl(bean.cConfigReport, tAttachURLName);

                SRPart.CID = bean.cID.ToString();
                SRPart.SRID = bean.cSRID;
                SRPart.SERIALID = bean.cSerialID;
                SRPart.MaterialID = bean.cMaterialID;
                SRPart.MaterialName = bean.cMaterialName;
                SRPart.ConfigReportURL = cConfigReportURL;

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
        public List<VIEW_MATERIAL_ByComp> findMATERIALINFO(string keyword, string tCompCde)
        {
            List<VIEW_MATERIAL_ByComp> tList = new List<VIEW_MATERIAL_ByComp>();

            string tPLANT = string.Empty;

            if (tCompCde == "Comp-1")
            {
                tPLANT = "12G1";
            }
            else if (tCompCde == "Comp-2")
            {
                tPLANT = "16G1";
            }

            if (keyword != "")
            {
                tList = dbProxy.VIEW_MATERIAL_ByComp.Where(x => (x.MARA_MATNR.Contains(keyword) || x.MAKT_TXZA1_ZF.Contains(keyword)) && x.MARD_WERKS == tPLANT).Take(8).ToList();
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

        #region 取得產品序號相關資訊(單筆)
        /// <summary>
        /// 取得產品序號相關資訊(單筆)
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

        #region 取得產品序號相關資訊(清單)
        /// <summary>
        /// 取得產品序號相關資訊(清單)
        /// </summary>
        /// <param name="IV_SERIAL">序號ID</param>
        /// <returns></returns>
        public List<SerialMaterialInfo> findMaterialBySerialList(string IV_SERIAL)
        {
            List<SerialMaterialInfo> ProBeans = new List<SerialMaterialInfo>();

            if (IV_SERIAL != "")
            {
                var beans = dbProxy.STOCKALL.Where(x => x.IV_SERIAL.Contains(IV_SERIAL));

                foreach(var bean in beans)
                {
                    SerialMaterialInfo ProBean = new SerialMaterialInfo();

                    ProBean.IV_SERIAL = bean.IV_SERIAL;
                    ProBean.ProdID = bean.ProdID;
                    ProBean.Product = bean.Product;
                    ProBean.MFRPN = findMFRPNumber(bean.ProdID);
                    ProBean.InstallNo = findInstallNumber(IV_SERIAL);

                    ProBeans.Add(ProBean);
                }
            }

            return ProBeans;
        }
        #endregion

        #region 取得ERPID(傳入英文姓名)
        /// <summary>
        /// 取得ERPID(傳入英文姓名)
        /// </summary>
        /// <param name="keyword">英文姓名</param>        
        /// <returns></returns>
        public string findEmployeeERPIDByEName(string keyword)
        {
            string reValue = string.Empty;

            if (keyword != "")
            {
                var bean = dbEIP.Person.FirstOrDefault(x => (x.Leave_Date == null && x.Leave_Reason == null) && x.Name == keyword.Trim());

                if (bean != null)
                {
                    reValue = bean.ERP_ID.Trim();
                }
            }

            return reValue;
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

        #region 取得人員中文+英文姓名(傳入ERPIID)含離職人員
        /// <summary>
        /// 取得人員中文+英文姓名(傳入ERPIID)含離職人員
        /// </summary>
        /// <param name="keyword">ERPID</param>        
        /// <returns></returns>
        public string findEmployeeNameInCludeLeave(string keyword)
        {
            string reValue = string.Empty;

            if (keyword != "")
            {
                var bean = dbEIP.Person.FirstOrDefault(x => x.ERP_ID == keyword.Trim());

                if (bean != null)
                {
                    if (bean.Leave_Reason == null)
                    {
                        reValue = bean.Name2 + " " + bean.Name;
                    }
                    else
                    {
                        reValue = bean.Name2 + " " + bean.Name + "(離職)";                        
                    }
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

        #region 取得製造商零件號碼和廠牌
        /// <summary>
        /// 取得製造商零件號碼和廠牌
        /// </summary>
        /// <param name="IV_MATERIAL">物料代號</param>
        /// <returns>[0]製造商零件號碼 [1]廠牌</returns>
        public string[] findMATERIALPNUMBERandBRAND(string IV_MATERIAL)
        {
            string[] reValue = new string[2];
            string MVKE_PRODH = string.Empty;   //物料階層

            #region 取得製造商零件號碼
            var beanM = dbProxy.MATERIAL.FirstOrDefault(x => x.MARA_MATNR == IV_MATERIAL.Trim() && x.MARA_MFRPN != "");

            if (beanM != null)
            {
                reValue[0] = beanM.MARA_MFRPN;
                MVKE_PRODH = beanM.MVKE_PRODH;
            }
            #endregion

            #region 取得廠牌
            if (MVKE_PRODH != "")
            {
                var beanF = dbProxy.F0005.FirstOrDefault(x => x.MODT == "MM" && x.ALIAS == "ProHierarchy" &&
                                                           x.CODET == "3" && x.CODETS == MVKE_PRODH.Substring(3, 3));

                if (beanF != null)
                {
                    reValue[1] = beanF.DSC1;
                }
                else
                {
                    reValue[1] = "Other";
                }
            }
            else
            {
                reValue[1] = "Other";
            }
            #endregion

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

        #region 取得物料相關資訊
        /// <summary>
        /// 取得物料相關資訊
        /// </summary>
        /// <param name="ProdID">料號</param>        
        /// <returns></returns>
        public MaterialInfo findMaterialInfo(string ProdID)
        {
            MaterialInfo ProBean = new MaterialInfo();

            var bean = dbProxy.MATERIAL.FirstOrDefault(x => x.MARA_MATNR.Contains(ProdID.Trim()));

            if (bean != null)
            {
                ProBean.MaterialID = bean.MARA_MATNR;
                ProBean.MaterialName = bean.MAKT_TXZA1_ZF;
                ProBean.MFPNumber = bean.MARA_MFRPN;
                ProBean.BasicContent = string.IsNullOrEmpty(bean.BasicContent) ? "" : bean.BasicContent;
                ProBean.ProductHierarchy = bean.MVKE_PRODH;
                ProBean.Brand = findMATERIALPNUMBERandBRAND(bean.MARA_MATNR)[1];
            }

            return ProBean;
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

            #region 取得裝機號碼
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
        /// <param name="tONEURLName">OneService站台名稱</param>
        /// <param name="tAPIURLName">API站台名稱</param>
        /// <returns></returns>
        public List<SRWarranty> ZFM_TICC_SERIAL_SEARCHWTYList(string[] ArySERIAL, string tBPMURLName, string tONEURLName, string tAPIURLName)
        {
            List<SRWarranty> QueryToList = new List<SRWarranty>();

            string cWTYID = string.Empty;
            string cWTYName = string.Empty;
            string cWTYSDATE = string.Empty;
            string cWTYEDATE = string.Empty;
            string cSLARESP = string.Empty;
            string cSLASRV = string.Empty;
            string cContractID = string.Empty;            
            string tBPMNO = string.Empty;            
            string tAdvice = string.Empty;
            string tBGColor = "table-success";
            
            int pCount = 0;

            try
            {
                //var client = new RestClient("http://tsti-sapapp01.etatung.com.tw/api/ticc");

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

                    #region 註解(舊的呼叫RFC)
                    //if (IV_SERIAL != null)
                    //{
                    //    var request = new RestRequest();
                    //    request.Method = RestSharp.Method.Post;

                    //    Dictionary<Object, Object> parameters = new Dictionary<Object, Object>();
                    //    parameters.Add("SAP_FUNCTION_NAME", "ZFM_TICC_SERIAL_SEARCH");
                    //    parameters.Add("IV_SERIAL", IV_SERIAL);

                    //    request.AddHeader("Content-Type", "application/json");
                    //    request.AddParameter("application/json", parameters, ParameterType.RequestBody);

                    //    RestResponse response = client.Execute(request);

                    //    var data = (JObject)JsonConvert.DeserializeObject(response.Content);

                    //    tLength = int.Parse(data["ET_WARRANTY"]["Length"].ToString());                          //保固共有幾筆

                    //    for (int i = 0; i < tLength; i++)
                    //    {
                    //        cContractIDURL = "";
                    //        tBPMNO = "";
                    //        tURL = "";

                    //        cWTYID = data["ET_WARRANTY"]["SyncRoot"][i]["wTYCODEField"].ToString().Trim();       //保固
                    //        cWTYName = data["ET_WARRANTY"]["SyncRoot"][i]["wTYCODEField"].ToString().Trim();     //保固說明
                    //        cWTYSDATE = data["ET_WARRANTY"]["SyncRoot"][i]["wTYSTARTField"].ToString().Trim();   //保固開始日期
                    //        cWTYEDATE = data["ET_WARRANTY"]["SyncRoot"][i]["wTYENDField"].ToString().Trim();     //保固結束日期                                                          
                    //        cSLARESP = data["ET_WARRANTY"]["SyncRoot"][i]["sLASRVField"].ToString().Trim();      //回應條件
                    //        cSLASRV = data["ET_WARRANTY"]["SyncRoot"][i]["sLARESPField"].ToString().Trim();      //服務條件
                    //        cContractID = data["ET_WARRANTY"]["SyncRoot"][i]["cONTRACTField"].ToString().Trim(); //合約編號
                    //        tBPMNO = data["ET_WARRANTY"]["SyncRoot"][i]["bPM_NOField"].ToString().Trim();        //BPM表單編號
                    //        tAdvice = data["ET_WARRANTY"]["SyncRoot"][i]["aDVICEField"].ToString().Trim();       //客服主管建議

                    //        #region 取得BPM Url
                    //        if (cContractID != "")
                    //        {
                    //            tBPMNO = findContractSealsFormNo(cContractID);
                    //            cSUB_CONTRACTID = findContractSealsOBJSubNo(tAPIURLName, cContractID);

                    //            try
                    //            {
                    //                Int32 ContractID = Int32.Parse(cContractID);

                    //                if (ContractID >= 10506151 && ContractID != 10506152 && ContractID != 10506157) //新的用印申請單
                    //                {
                    //                    tURL = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/ContractSeals/ContractSealsForm.aspx?FormNo=" + tBPMNO;
                    //                }
                    //                else //舊的用印申請單
                    //                {
                    //                    tURL = "http://" + tBPMURLName + "/ContractSeals/_layouts/FormServer.aspx?XmlLocation=%2fContractSeals%2fBPMContractSealsForm%2f" + tBPMNO + ".xml&ClientInstalled=true&DefaultItemOpen=1&source=/_layouts/TSTI.SharePoint.BPM/CloseWindow.aspx";
                    //                }

                    //                cContractIDURL = "http://" + tPSIPURLName + "/Spare/QueryContractInfo?CONTRACTID=" + cContractID; //合約編號URL
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                cContractIDURL = "";
                    //                tBPMNO = "";
                    //                tURL = "";
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (tBPMNO.IndexOf("WTY") != -1)
                    //            {
                    //                tURL = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/Warranty/WarrantyForm.aspx?FormNo=" + tBPMNO;
                    //            }
                    //            else
                    //            {
                    //                tURL = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Form/Guarantee/GuaranteeForm.aspx?FormNo=" + tBPMNO;
                    //            }
                    //        }
                    //        #endregion

                    //        #region 取得清單
                    //        SRWarranty QueryInfo = new SRWarranty();

                    //        QueryInfo.SERIALID = IV_SERIAL;            //序號                        
                    //        QueryInfo.WTYID = cWTYID;                  //保固
                    //        QueryInfo.WTYName = cWTYName;              //保固說明
                    //        QueryInfo.WTYSDATE = cWTYSDATE;            //保固開始日期
                    //        QueryInfo.WTYEDATE = cWTYEDATE;            //保固結束日期                                                          
                    //        QueryInfo.SLARESP = cSLARESP;              //回應條件
                    //        QueryInfo.SLASRV = cSLASRV;                //服務條件
                    //        QueryInfo.CONTRACTID = cContractID;        //合約編號
                    //        QueryInfo.CONTRACTIDUrl = cContractIDURL;  //合約編號Url
                    //        QueryInfo.SUBCONTRACTID = cSUB_CONTRACTID; //下包文件編號
                    //        QueryInfo.BPMFormNo = tBPMNO;              //BPM表單編號                        
                    //        QueryInfo.BPMFormNoUrl = tURL;             //BPM URL
                    //        QueryInfo.ADVICE = tAdvice;               //客服主管建議                                          
                    //        QueryInfo.USED = "N";
                    //        QueryInfo.BGColor = tBGColor;             //tr背景顏色Class

                    //        QueryToList.Add(QueryInfo);
                    //        #endregion
                    //    }
                    //}
                    #endregion

                    if (IV_SERIAL != null)
                    {
                        if (IV_SERIAL.ToUpper() != "NA" && IV_SERIAL.ToUpper() != "N/A")
                        {
                            #region 取得進出貨資料檔
                            var beansW = dbProxy.STOCKWTY.Where(x => x.IV_SERIAL == IV_SERIAL);

                            foreach (var bean in beansW)
                            {
                                cWTYID = bean.IV_WTYID;                                                 //保固
                                cWTYName = bean.IV_WTYID;                                               //保固說明
                                cWTYSDATE = Convert.ToDateTime(bean.IV_SDATE).ToString("yyyy-MM-dd");       //保固開始日期
                                cWTYEDATE = Convert.ToDateTime(bean.IV_EDATE).ToString("yyyy-MM-dd");       //保固結束日期
                                cSLARESP = bean.IV_SLARESP;                                             //回應條件
                                cSLASRV = bean.IV_SLASRV;                                               //服務條件
                                cContractID = "";                                                      //合約編號
                                tBPMNO = string.IsNullOrEmpty(bean.BPM_NO) ? "" : bean.BPM_NO;              //BPM表單編號
                                tAdvice = string.IsNullOrEmpty(bean.ADVICE) ? "" : bean.ADVICE;             //客服主管建議

                                var QueryInfo = setSRWarranty(IV_SERIAL, cWTYID, cWTYName, cWTYSDATE, cWTYEDATE, cSLARESP, cSLASRV, cContractID, tBPMNO,
                                                              tAdvice, tBGColor, tAPIURLName, tBPMURLName, tONEURLName);
                                QueryToList.Add(QueryInfo);
                            }
                            #endregion

                            #region 取得合約標的檔
                            var beansObj = dbOne.TB_ONE_ContractDetail_OBJ.Where(x => x.Disabled == 0 && x.cSerialID == IV_SERIAL);

                            foreach (var bean in beansObj)
                            {
                                string[] AryValue = findContracSLADate(bean.cContractID);

                                tBPMNO = "";    //BPM表單編號

                                cWTYID = "";                   //保固
                                cWTYName = "";                 //保固說明
                                cWTYSDATE = AryValue[0];       //保固開始日期
                                cWTYEDATE = AryValue[1];       //保固結束日期
                                cSLARESP = bean.cSLARESP;       //回應條件
                                cSLASRV = bean.cSLASRV;         //服務條件
                                cContractID = bean.cContractID; //合約編號                            
                                tAdvice = "";                  //客服主管建議

                                var QueryInfo = setSRWarranty(IV_SERIAL, cWTYID, cWTYName, cWTYSDATE, cWTYEDATE, cSLARESP, cSLASRV, cContractID, tBPMNO,
                                                              tAdvice, tBGColor, tAPIURLName, tBPMURLName, tONEURLName);
                                QueryToList.Add(QueryInfo);
                            }
                            #endregion
                        }
                    }                   
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return QueryToList;
        }
        #endregion

        #region 設定保固SLA資訊
        /// <summary>
        /// 設定保固SLA資訊
        /// </summary>
        /// <param name="IV_SERIAL">序號</param>
        /// <param name="cWTYID">保固</param>
        /// <param name="cWTYName">保固說明</param>
        /// <param name="cWTYSDATE">保固開始日期</param>
        /// <param name="cWTYEDATE">保固結束日期</param>
        /// <param name="cSLARESP">回應條件</param>
        /// <param name="cSLASRV">服務條件</param>
        /// <param name="cContractID">合約編號</param>
        /// <param name="tBPMNO">BPM表單編號</param>
        /// <param name="tAdvice">本次使用</param>
        /// <param name="tBGColor">tr背景顏色Class</param>
        /// <param name="tAPIURLName">API站台名稱</param>
        /// <param name="tBPMURLName">BPM站台名稱</param>
        /// <param name="tONEURLName">OneService站台名稱</param>
        /// <returns></returns>
        public SRWarranty setSRWarranty(string IV_SERIAL, string cWTYID, string cWTYName, string cWTYSDATE, string cWTYEDATE, string cSLARESP, string cSLASRV, 
                                      string cContractID, string tBPMNO, string tAdvice, string tBGColor, string tAPIURLName, string tBPMURLName, string tONEURLName)
        {
            string cContractIDURL = string.Empty;   //合約編號Url
            string cSUB_CONTRACTID = string.Empty;  //下包文件編號             
            string tURL = string.Empty;             //BPM URL            

            #region 取得BPM Url
            if (cContractID != "")
            {
                tBPMNO = findContractSealsFormNo(cContractID);
                cSUB_CONTRACTID = findContractSealsOBJSubNo(cContractID);

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

                    cContractIDURL = tONEURLName + "/Contract/ContractMain?ContractID=" + cContractID; //合約編號URL
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
                    if (tBPMNO != "")
                    {
                        tURL = "http://" + tBPMURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Form/Guarantee/GuaranteeForm.aspx?FormNo=" + tBPMNO;
                    }
                    else
                    {
                        tURL = "";
                    }
                }
            }
            #endregion

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
            QueryInfo.USED = "N";                     //本次使用
            QueryInfo.BGColor = tBGColor;             //tr背景顏色Class

            return QueryInfo;
        }
        #endregion

        #region 取得合約主數據保固起迄日期
        /// <summary>
        /// 取得合約主數據保固起迄日期
        /// </summary>
        /// <param name="cContractID">文件編號</param>
        /// <returns></returns>
        public string[] findContracSLADate(string cContractID)
        {
            string[] AryValue = new string[2];
            AryValue[0] = "";
            AryValue[1] = "";

            var beanM = dbOne.TB_ONE_ContractMain.FirstOrDefault(x => x.Disabled == 0 && x.cContractID == cContractID);

            if (beanM != null)
            {
                AryValue[0] = Convert.ToDateTime(beanM.cStartDate).ToString("yyyy-MM-dd");
                AryValue[1] = Convert.ToDateTime(beanM.cEndDate).ToString("yyyy-MM-dd");
            }

            return AryValue;
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

        #region 取得服務案件主檔資訊清單
        /// <summary>
        /// 取得服務案件主檔資訊清單
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

        #region 服務案件客戶聯絡人資訊清單
        /// <summary>
        /// 服務案件客戶聯絡人資訊清單
        /// </summary>
        /// <param name="tSRIDList">服務案件ID清單</param>
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
        /// <param name="cContractID">合約編號</param>
        /// <returns></returns>
        public string findContractSealsOBJSubNo(string cContractID)
        {
            string reValue = string.Empty;
            string SUB_CONTRACTID = string.Empty;

            if (cContractID != "")
            {
                var beansSub = dbOne.TB_ONE_ContractDetail_SUB.Where(x => x.Disabled == 0 && x.cContractID == cContractID);

                foreach(var bean in beansSub)
                {
                    reValue += bean.cSubContractID + Environment.NewLine;
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

        #region 傳入DataTable並過濾重覆人員
        /// <summary>
        /// 傳入DataTable並過濾重覆人員
        /// </summary>
        /// <param name="dtORG">組織人員</param>
        /// <param name="DicORG">傳入的Dic</param>
        public void SetDtORGPeople(DataTable dtORG, ref Dictionary<string, string> DicORG)
        {
            string tEMPNO = string.Empty;

            foreach (DataRow dr in dtORG.Rows)
            {
                tEMPNO = dr["EMPNO"].ToString().TrimStart('0').Trim();

                if (!DicORG.Keys.Contains(tEMPNO))
                {
                    DicORG.Add(tEMPNO, dr["EMPNAME"].ToString().Trim());
                }
            }
        }
        #endregion

        #region 傳入ERPID並過濾重覆人員
        /// <summary>
        /// 傳入ERPID並過濾重覆人員
        /// </summary>
        /// <param name="tERPID">ERPID</param>
        /// <param name="tNAME">姓名</param>
        /// <param name="DicORG">傳入的Dic</param>
        public void SetDtORGPeople(string tERPID, string tNAME, ref Dictionary<string, string> DicORG)
        {
            if (!DicORG.Keys.Contains(tERPID))
            {
                DicORG.Add(tERPID, tNAME);
            }
        }
        #endregion

        #region 批次儲存APP_INSTALL檔
        /// <summary>
        /// 批次儲存APP_INSTALL檔
        /// </summary>
        /// <param name="pLoginAccount">登入者帳號</param>
        /// <param name="pLoginName">登入者姓名</param>
        /// <param name="pLoginERPID">登入者ERPID</param>        
        /// <param name="cID">系統ID</param>
        /// <param name="SRID">SRID</param>
        /// <param name="TotalQuantity">總安裝數量</param>
        /// <param name="InstallQuantity">已安裝數量</param>
        /// <param name="InstallDate">裝機起始日期</param>
        /// <param name="ExpectedDate">裝機完成日期</param>
        /// <param name="tIsFromAPP">是否來自APP的更新(Y.是 N.否)</param>
        /// <returns></returns>
        public string SaveTB_SERVICES_APP_INSTALL(string pLoginAccount, string pLoginName, string pLoginERPID, int cID,
                                                 string SRID, string TotalQuantity, string InstallQuantity,
                                                 string InstallDate, string ExpectedDate, string tIsFromAPP)
        {
            string returnMsg = "SUCCESS";

            try
            {
                #region APP_INSTALL檔(【測試】)
                if (cID != 0)
                {
                    var beanAPP = dbEIP.TB_SERVICES_APP_INSTALLTEMP.FirstOrDefault(x => x.ID == cID);

                    if (beanAPP != null)
                    {
                        if (TotalQuantity != "0")
                        {
                            if (tIsFromAPP == "Y")
                            {
                                beanAPP.TotalQuantity = Convert.ToDecimal(TotalQuantity);
                                beanAPP.InstallQuantity = Convert.ToDecimal(InstallQuantity);
                                beanAPP.InstallDate = InstallDate;
                                beanAPP.ExpectedDate = ExpectedDate;
                                beanAPP.UPDATE_ACCOUNT = pLoginAccount;
                                beanAPP.UPDATE_EMP_NAME = pLoginName;
                                beanAPP.UPDATE_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else //非來自APP更新，只要更新總安裝數量就好
                            {
                                beanAPP.TotalQuantity = Convert.ToDecimal(TotalQuantity);           //總安裝數量
                                beanAPP.UPDATE_ACCOUNT = pLoginAccount;                           //變更者帳號                                
                                beanAPP.UPDATE_EMP_NAME = pLoginName;                             //變更者姓名
                                beanAPP.UPDATE_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  //更新日期
                            }
                        }
                    }
                }
                else
                {
                    TB_SERVICES_APP_INSTALLTEMP beanA = new TB_SERVICES_APP_INSTALLTEMP();

                    beanA.TotalQuantity = Convert.ToDecimal(TotalQuantity);             //總安裝數量
                    beanA.InstallQuantity = Convert.ToDecimal(InstallQuantity);         //已安裝數量
                    beanA.InstallDate = InstallDate;                                 //裝機起始日期
                    beanA.ExpectedDate = ExpectedDate;                               //裝機完成日期 
                    beanA.SRID = SRID;                                              //SRID
                    beanA.ACCOUNT = pLoginAccount;                                   //登入者帳號
                    beanA.ERP_ID = pLoginERPID;                                      //登入者ERPID
                    beanA.EMP_NAME = pLoginName;                                     //登入者姓名
                    beanA.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");   //建立日期                            

                    dbEIP.TB_SERVICES_APP_INSTALLTEMP.Add(beanA);
                }

                int result = dbEIP.SaveChanges();

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

                    writeToLog(SRID, "SaveTB_SERVICES_APP_INSTALL", pMsg, pLoginName);
                }
                #endregion

                #region APP_INSTALL檔(【正式】)
                //if (cID != 0)
                //{
                //    var beanAPP = dbEIP.TB_SERVICES_APP_INSTALL.FirstOrDefault(x => x.ID == cID);

                //    if (beanAPP != null)
                //    {
                //        if (TotalQuantity != "0")
                //        {
                //            if (tIsFromAPP == "Y")
                //            {
                //                beanAPP.TotalQuantity = Convert.ToDecimal(TotalQuantity);
                //                beanAPP.InstallQuantity = Convert.ToDecimal(InstallQuantity);
                //                beanAPP.InstallDate = InstallDate;
                //                beanAPP.ExpectedDate = ExpectedDate;
                //                beanAPP.UPDATE_ACCOUNT = pLoginAccount;
                //                beanAPP.UPDATE_EMP_NAME = pLoginName;
                //                beanAPP.UPDATE_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //            }
                //            else //非來自APP更新，只要更新總安裝數量就好
                //            {
                //                beanAPP.TotalQuantity = Convert.ToDecimal(TotalQuantity);           //總安裝數量
                //                beanAPP.UPDATE_ACCOUNT = pLoginAccount;                           //變更者帳號                                
                //                beanAPP.UPDATE_EMP_NAME = pLoginName;                             //變更者姓名
                //                beanAPP.UPDATE_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  //更新日期
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    TB_SERVICES_APP_INSTALL beanA = new TB_SERVICES_APP_INSTALL();

                //    beanA.TotalQuantity = Convert.ToDecimal(TotalQuantity);             //總安裝數量
                //    beanA.InstallQuantity = Convert.ToDecimal(InstallQuantity);         //已安裝數量
                //    beanA.InstallDate = InstallDate;                                 //裝機起始日期
                //    beanA.ExpectedDate = ExpectedDate;                               //裝機完成日期 
                //    beanA.SRID = SRID;                                              //SRID
                //    beanA.ACCOUNT = pLoginAccount;                                   //登入者帳號
                //    beanA.ERP_ID = pLoginERPID;                                      //登入者ERPID
                //    beanA.EMP_NAME = pLoginName;                                     //登入者姓名
                //    beanA.INSERT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");   //建立日期                            

                //    dbEIP.TB_SERVICES_APP_INSTALL.Add(beanA);
                //}

                //int result = dbEIP.SaveChanges();

                //if (result <= 0)
                //{
                //    if (cID == 0) //新增
                //    {
                //        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "新增失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                //    }
                //    else
                //    {
                //        pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "刪除失敗！請確認輸入的資料是否有誤！" + Environment.NewLine;
                //    }

                //    writeToLog(SRID, "SaveTB_SERVICES_APP_INSTALL", pMsg, pLoginName);
                //}
                #endregion
            }
            catch (Exception ex)
            {
                returnMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "【SaveTB_SERVICES_APP_INSTALL】更新失敗！原因：" + ex.Message + Environment.NewLine;
                writeToLog(SRID, "SaveTB_SERVICES_APP_INSTALL", returnMsg, pLoginName);                
            }

            return returnMsg;
        }
        #endregion

        #region 判斷是否為內部作業(true.是 false.否)
        /// <summary>
        /// 判斷是否為內部作業(true.是 false.否)
        /// </summary>
        /// <param name="cSRID">SRID</param>
        /// <returns></returns>
        public bool checkIsInternalWork(string cSRID)
        {
            bool reValue = false;

            var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == cSRID);

            if (beanM != null)
            {
                if (beanM.cIsInternalWork == "Y")
                {
                    reValue = true;
                }
            }

            return reValue;
        }
        #endregion

        #region 傳入服務團隊並取得公司名稱
        /// <summary>
        /// 傳入服務團隊並取得公司名稱
        /// </summary>
        /// <param name="cTeamID">服務團隊ID</param>
        /// <returns></returns>
        public string findCompanyNameByTeamID(string cTeamID)
        {
            string reValue = "大同世界科技";

            if (!string.IsNullOrEmpty(cTeamID))
            {
                switch (cTeamID.Substring(0, 6))
                {
                    case "SRV.12":
                        reValue = "大同世界科技";
                        break;

                    case "SRV.16":
                        reValue = "群輝商務科技";
                        break;

                    case "SRV.22":
                        reValue = "協志聯合科技";
                        break;
                }
            }

            return reValue;
        }
        #endregion

        #region -----↓↓↓↓↓待辦清單 ↓↓↓↓↓-----

        #region 取得登入人員所有要負責的SRID
        /// <summary>
        /// 取得登入人員所有要負責的SRID
        /// </summary>
        /// <param name="cOperationID_GenerallySR">程式作業編號檔系統ID(一般)</param>
        /// <param name="cOperationID_InstallSR">程式作業編號檔系統ID(裝機)</param>
        /// <param name="cOperationID_MaintainSR">程式作業編號檔系統ID(定維)</param>
        /// <param name="cCompanyID">公司別</param>
        /// <param name="IsManager">true.管理員 false.非管理員</param>
        /// <param name="tERPID">登入人員ERPID</param>
        /// <param name="tTeamList">可觀看服務團隊清單</param>        
        /// <returns></returns>
        public List<string[]> findSRIDList(string cOperationID_GenerallySR, string cOperationID_InstallSR, string cOperationID_MaintainSR, 
                                         string cCompanyID, bool IsManager, string tERPID, List<string> tTeamList)
        {

            List<string[]> SRIDUserToList = new List<string[]>();   //組SRID清單

            SRIDUserToList = getSRIDToDoList(cOperationID_GenerallySR, cOperationID_InstallSR, cOperationID_MaintainSR, cCompanyID, IsManager, tERPID, tTeamList);

            return SRIDUserToList;
        }
        #endregion

        #region 取得一般服務SRID負責清單
        /// <summary>
        /// 取得一般服務SRID負責清單
        /// </summary>
        /// <param name="cOperationID_GenerallySR">程式作業編號檔系統ID(一般)</param>
        /// <param name="cOperationID_InstallSR">程式作業編號檔系統ID(裝機)</param>
        /// <param name="cOperationID_MaintainSR">程式作業編號檔系統ID(定維)</param>
        /// <param name="cCompanyID">公司別</param>
        /// <param name="IsManager">true.管理員 false.非管理員</param>
        /// <param name="tERPID">登入人員ERPID</param>
        /// <param name="tTeamList">可觀看服務團隊清單</param>        
        /// <returns></returns>
        private List<string[]> getSRIDToDoList(string cOperationID_GenerallySR, string cOperationID_InstallSR, string cOperationID_MaintainSR, 
                                            string cCompanyID, bool IsManager, string tERPID, List<string> tTeamList)
        {
            List<string[]> SRIDUserToList = new List<string[]>();   //組SRID清單

            string[] tArySLA = new string[2];
            
            string tSRContactName = string.Empty;       //客戶聯絡人
            string tSRPathWay = string.Empty;           //報修管理
            string tSRType = string.Empty;              //報修類別
            string tMainEngineerID = string.Empty;      //主要工程師ERPID
            string tMainEngineerName = string.Empty;    //主要工程師姓名            
            string tAssEngineerName = string.Empty;     //協助工程師姓名
            string tTechManagerID = string.Empty;       //技術主管ERPID            
            string tTechManagerName = string.Empty;     //技術主管姓名
            string tSalesID = string.Empty;             //業務人員ERPID
            string tSecretaryID = string.Empty;         //業務祕書ERPID
            string tModifiedDate = string.Empty;        //最後編輯日期
            string tSTATUSDESC = string.Empty;          //狀態說明
            string tSLARESP = string.Empty;             //回應條件
            string tSLASRV = string.Empty;              //服務條件           

            List<string> tListAssAndTech = new List<string>();                          //記錄所有協助工程師和所有技術主管的ERPID
            Dictionary<string, string> tDicAssAndTech = new Dictionary<string, string>();  //記錄所有協助工程師和所有技術主管的<ERPID,中、英文姓名>

            var tSRContact_List = findSRDetailContactList();

            List<TB_ONE_SRMain> beans = new List<TB_ONE_SRMain>();            

            List<SelectListItem> ListStatus = findSRStatus(cOperationID_GenerallySR, cOperationID_InstallSR, cOperationID_MaintainSR, cCompanyID);
            
            if (IsManager)
            {
                string tWhere = TrnasTeamListToWhere(tTeamList);

                string tSQL = @"select * from TB_ONE_SRMain
                                   where 
                                   (cStatus <> 'E0015' and cStatus <> 'E0006' and cStatus <> 'E0010' and cStatus <> 'E0017') and 
                                   (
                                        (
                                            (cMainEngineerID = '{0}') or (cSalesID = '{0}') or (cSecretaryID = '{0}') or (cTechManagerID like '%{0}%')
                                        )
                                        {1}
                                   )";

                tSQL = string.Format(tSQL, tERPID, tWhere);

                DataTable dt = getDataTableByDb(tSQL, "dbOne");

                #region 先取得所有協助工程師和技術主管的ERPID
                foreach (DataRow dr in dt.Rows)
                {
                    #region 協助工程師
                    findListAssAndTech(ref tListAssAndTech, dr["cAssEngineerID"].ToString());
                    #endregion

                    #region 技術主管
                    findListAssAndTech(ref tListAssAndTech, dr["cTechManagerID"].ToString());
                    #endregion
                }
                #endregion

                #region 再取得所有協助工程師和技術主管的中文姓名
                tDicAssAndTech = findListEmployeeInfo(tListAssAndTech);
                #endregion

                foreach (DataRow dr in dt.Rows)
                {
                    tSRContactName = TransSRDetailContactName(tSRContact_List, dr["cSRID"].ToString());
                    tSRPathWay = TransSRPATH(cOperationID_GenerallySR, cCompanyID, dr["cSRPathWay"].ToString());
                    tSRType = TransSRType(dr["cSRTypeOne"].ToString(), dr["cSRTypeSec"].ToString(), dr["cSRTypeThr"].ToString());
                    tMainEngineerID = dr["cMainEngineerID"].ToString();
                    tMainEngineerName = dr["cMainEngineerName"].ToString();
                    tAssEngineerName = TransEmployeeName(tDicAssAndTech, dr["cAssEngineerID"].ToString());
                    tTechManagerName = TransEmployeeName(tDicAssAndTech, dr["cTechManagerID"].ToString());
                    tTechManagerID = dr["cTechManagerID"].ToString();
                    tSalesID = dr["cSalesID"].ToString();
                    tSecretaryID = dr["cSecretaryID"].ToString();
                    tModifiedDate = dr["ModifiedDate"].ToString() != "" ? Convert.ToDateTime(dr["ModifiedDate"].ToString()).ToString("yyyy-MM-dd HH:mm:ss") : "";
                    tSTATUSDESC = TransSRSTATUS(ListStatus, dr["cStatus"].ToString());
                    tArySLA = findSRSLACondition(dr["cSRID"].ToString());
                    tSLARESP = tArySLA[0];
                    tSLASRV = tArySLA[1];

                    #region 組待處理服務
                    string[] ProcessInfo = new string[19];

                    ProcessInfo[0] = dr["cSRID"].ToString();             //SRID
                    ProcessInfo[1] = dr["cCustomerName"].ToString();      //客戶
                    ProcessInfo[2] = dr["cRepairName"].ToString();        //客戶報修人
                    ProcessInfo[3] = tSRContactName;                    //客戶聯絡人
                    ProcessInfo[4] = dr["cDesc"].ToString();             //說明
                    ProcessInfo[5] = tSRPathWay;                        //報修管道
                    ProcessInfo[6] = tSRType;                           //報修類別
                    ProcessInfo[7] = tMainEngineerID;                   //主要工程師ERPID
                    ProcessInfo[8] = tMainEngineerName;                 //主要工程師姓名
                    ProcessInfo[9] = tAssEngineerName;                  //協助工程師姓名
                    ProcessInfo[10] = tTechManagerID;                   //技術主管ERPID
                    ProcessInfo[11] = tTechManagerName;                 //技術主管姓名
                    ProcessInfo[12] = tSalesID;                         //業務人員ERPID
                    ProcessInfo[13] = tSecretaryID;                     //業務祕書ERPID
                    ProcessInfo[14] = tSLARESP;                         //回應條件
                    ProcessInfo[15] = tSLASRV;                          //服務條件
                    ProcessInfo[16] = tModifiedDate;                    //最後編輯日期                    
                    ProcessInfo[17] = dr["cStatus"].ToString();           //狀態
                    ProcessInfo[18] = tSTATUSDESC;                      //狀態+狀態說明                    

                    SRIDUserToList.Add(ProcessInfo);
                    #endregion
                }
            }
            else
            {
                beans = dbOne.TB_ONE_SRMain.Where(x => (x.cStatus != "E0015" && x.cStatus != "E0006" && x.cStatus != "E0010" && x.cStatus != "E0017") && 
                                                    (x.cMainEngineerID == tERPID || x.cSalesID == tERPID || x.cSecretaryID == tERPID || x.cTechManagerID.Contains(tERPID) || x.cAssEngineerID.Contains(tERPID))
                                                ).ToList();

                #region 先取得所有協助工程師和技術主管的ERPID
                foreach (var bean in beans)
                {
                    #region 協助工程師
                    findListAssAndTech(ref tListAssAndTech, bean.cAssEngineerID);
                    #endregion

                    #region 技術主管
                    findListAssAndTech(ref tListAssAndTech, bean.cTechManagerID);
                    #endregion
                }
                #endregion

                #region 再取得所有協助工程師和技術主管的中文姓名
                tDicAssAndTech = findListEmployeeInfo(tListAssAndTech);
                #endregion

                foreach (var bean in beans)
                {
                    tSRContactName = TransSRDetailContactName(tSRContact_List, bean.cSRID);
                    tSRPathWay = TransSRPATH(cOperationID_GenerallySR, cCompanyID, bean.cSRPathWay);
                    tSRType = TransSRType(bean.cSRTypeOne, bean.cSRTypeSec, bean.cSRTypeThr);
                    tMainEngineerID = string.IsNullOrEmpty(bean.cMainEngineerID) ? "" : bean.cMainEngineerID;
                    tMainEngineerName = string.IsNullOrEmpty(bean.cMainEngineerName) ? "" : bean.cMainEngineerName;
                    tAssEngineerName = TransEmployeeName(tDicAssAndTech, bean.cAssEngineerID);
                    tTechManagerName = TransEmployeeName(tDicAssAndTech, bean.cTechManagerID);
                    tTechManagerID = string.IsNullOrEmpty(bean.cTechManagerID) ? "" : bean.cTechManagerID;
                    tSalesID = string.IsNullOrEmpty(bean.cSalesID) ? "" : bean.cSalesID;
                    tSecretaryID = string.IsNullOrEmpty(bean.cSecretaryID) ? "" : bean.cSecretaryID;
                    tModifiedDate = bean.ModifiedDate == DateTime.MinValue ? "" : Convert.ToDateTime(bean.ModifiedDate).ToString("yyyy-MM-dd HH:mm:ss");
                    tSTATUSDESC = TransSRSTATUS(ListStatus, bean.cStatus);
                    tArySLA = findSRSLACondition(bean.cSRID);
                    tSLARESP = tArySLA[0];
                    tSLASRV = tArySLA[1];

                    #region 組待處理服務
                    string[] ProcessInfo = new string[19];

                    ProcessInfo[0] = bean.cSRID;            //SRID
                    ProcessInfo[1] = bean.cCustomerName;     //客戶
                    ProcessInfo[2] = bean.cRepairName;       //客戶報修人
                    ProcessInfo[3] = tSRContactName;       //客戶聯絡人
                    ProcessInfo[4] = bean.cDesc;            //說明
                    ProcessInfo[5] = tSRPathWay;           //報修管道
                    ProcessInfo[6] = tSRType;              //報修類別
                    ProcessInfo[7] = tMainEngineerID;      //主要工程師ERPID
                    ProcessInfo[8] = tMainEngineerName;    //主要工程師姓名
                    ProcessInfo[9] = tAssEngineerName;     //協助工程師姓名
                    ProcessInfo[10] = tTechManagerID;      //技術主管ERPID
                    ProcessInfo[11] = tTechManagerName;    //技術主管姓名
                    ProcessInfo[12] = tSalesID;            //業務人員ERPID
                    ProcessInfo[13] = tSecretaryID;        //業務祕書ERPID
                    ProcessInfo[14] = tSLARESP;            //回應條件
                    ProcessInfo[15] = tSLASRV;             //服務條件
                    ProcessInfo[16] = tModifiedDate;       //最後編輯日期
                    ProcessInfo[17] = bean.cStatus;         //狀態
                    ProcessInfo[18] = tSTATUSDESC;         //狀態+狀態說明                    

                    SRIDUserToList.Add(ProcessInfo);
                    #endregion
                }
            }

            return SRIDUserToList;
        }
        #endregion

        #region 取得服務狀態清單
        /// <summary>
        /// 取得服務狀態清單
        /// </summary>
        /// <param name="cOperationID_GenerallySR">程式作業編號檔系統ID(一般)</param>
        /// <param name="cOperationID_InstallSR">程式作業編號檔系統ID(裝機)</param>
        /// <param name="cOperationID_MaintainSR">程式作業編號檔系統ID(定維)</param>
        /// <param name="cCompanyID">公司別</param>
        /// <returns></returns>
        public List<SelectListItem> findSRStatus(string cOperationID_GenerallySR, string cOperationID_InstallSR, string cOperationID_MaintainSR, string cCompanyID)
        {
            List<SelectListItem> ListTempStatus = new List<SelectListItem>();

            List<SelectListItem> ListStatus_Gen = findSysParameterList(cOperationID_GenerallySR, "OTHER", cCompanyID, "SRSTATUS");  //一般服務
            List<SelectListItem> ListStatus_Ins = findSysParameterList(cOperationID_InstallSR, "OTHER", cCompanyID, "SRSTATUS");  //裝機服務
            List<SelectListItem> ListStatus_Man = findSysParameterList(cOperationID_MaintainSR, "OTHER", cCompanyID, "SRSTATUS");  //定維服務

            ListTempStatus = findSRStatus(ListTempStatus, ListStatus_Gen);
            ListTempStatus = findSRStatus(ListTempStatus, ListStatus_Ins);
            ListTempStatus = findSRStatus(ListTempStatus, ListStatus_Man);

            return ListTempStatus;
        }
        #endregion

        #region 取得服務狀態清單
        /// <summary>
        /// 取得服務狀態清單
        /// </summary>
        /// <param name="ListOriStatus">來源的清單</param>
        /// <param name="ListInputStatus">傳入卻比對的清單</param>
        public List<SelectListItem> findSRStatus(List<SelectListItem> ListOriStatus, List<SelectListItem> ListInputStatus)
        {
            List<SelectListItem> ListTempStatus = new List<SelectListItem>();
            ListTempStatus.AddRange(ListOriStatus);

            if (ListTempStatus.Count == 0)
            {
                foreach (var beanG in ListInputStatus)
                {
                    ListTempStatus.Add(new SelectListItem { Text = beanG.Text, Value = beanG.Value });
                }
            }
            else
            {
                foreach (var beanG in ListInputStatus)
                {
                    bool tIsMatch = false;

                    foreach (var bean in ListOriStatus)
                    {
                        if (beanG.Value == bean.Value)
                        {
                            tIsMatch = true;
                            break;
                        }                        
                    }

                    if (!tIsMatch) //不符合才新增
                    {
                        ListTempStatus.Add(new SelectListItem { Text = beanG.Text, Value = beanG.Value });
                    }
                }
            }

            return ListTempStatus;
        }
        #endregion

        #region 取得所有協助工程師和技術主管的ERPID清單
        /// <summary>
        /// 取得所有協助工程師和技術主管的ERPID清單
        /// </summary>
        /// <param name="tList">ERPID清單</param>
        /// <param name="tOriERPID">傳入的ERPID</param>
        public void findListAssAndTech(ref List<string> tList, string tOriERPID)
        {
            tOriERPID = string.IsNullOrEmpty(tOriERPID) ? "" : tOriERPID.Trim();

            if (tOriERPID != "")
            {
                string[] tAryAssERPID = tOriERPID.ToString().Split(';');

                foreach (string tERPID in tAryAssERPID)
                {
                    if (tERPID != "")
                    {
                        if (!tList.Contains(tERPID))
                        {
                            tList.Add(tERPID);
                        }
                    }
                }
            }
        }
        #endregion

        #region 取得所有傳入員工ERPID清單，並回傳ERPID和中、英文姓名清單
        /// <summary>
        /// 取得所有傳入員工ERPID清單，並回傳ERPID和中、英文姓名清單
        /// </summary>
        /// <param name="tERPID_List">員工ERPID清單</param>
        /// <returns></returns>
        public Dictionary<string, string> findListEmployeeInfo(List<string> tERPID_List)
        {
            Dictionary<string, string> reDic = new Dictionary<string, string>();

            var beans = dbEIP.Person.Where(x => tERPID_List.Contains(x.ERP_ID));

            foreach (var bean in beans)
            {
                reDic.Add(bean.ERP_ID, bean.Name2 + " " + bean.Name);
            }

            return reDic;
        }
        #endregion

        #region 取得人員中、英文姓名
        /// <summary>
        /// 取得人員中、英文姓名
        /// </summary>
        /// <param name="Dic">ERPID,中、英文姓名清單</param>
        /// <param name="tOriERPID">ERPID(多人，用分號隔開)</param>
        /// <returns></returns>
        public string TransEmployeeName(Dictionary<string, string> Dic, string tOriERPID)
        {
            string reValue = string.Empty;

            tOriERPID = string.IsNullOrEmpty(tOriERPID) ? "" : tOriERPID.Trim();

            if (tOriERPID != "")
            {
                string[] tAryERPID = tOriERPID.Split(';');

                foreach (string tERPID in tAryERPID)
                {
                    var tName = Dic.FirstOrDefault(x => x.Key == tERPID).Value;

                    reValue += tName + "<br/>";
                }
            }

            return reValue;
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

        #region 判斷登入人員是否有在傳入的服務團隊裡(true.有 false.否)，抓新組織
        /// <summary>
        /// 判斷登入人員是否有在傳入的服務團隊裡(true.有 false.否)，抓新組織
        /// </summary>
        /// <param name="tCostCenterID">登入人員部門成本中心ID</param>
        /// <param name="tDeptID">登入人員部門ID</param>
        /// <param name="tTeamOldID">服務團隊ID</param>
        /// <returns></returns>
        public bool checkEmpIsExistSRTeamMapping(string tCostCenterID, string tDeptID, string tTeamOldID)
        {
            bool reValue = false;

            var beans = dbOne.TB_ONE_SRTeamMapping.Where(x => x.Disabled == 0 && (x.cTeamNewID == tCostCenterID || x.cTeamNewID == tDeptID));

            foreach (var beansItem in beans)
            {
                if (tTeamOldID.Contains(beansItem.cTeamOldID))
                {
                    reValue = true;
                    break;
                }
            }

            return reValue;
        }
        #endregion

        #region 判斷登入人員是否有在傳入的服務團隊裡(true.有 false.否)，抓舊組織
        /// <summary>
        /// 判斷登入人員是否有在傳入的服務團隊裡(true.有 false.否)，抓舊組織
        /// </summary>
        /// <param name="pOperationID_Contract">程式作業編號檔系統ID(合約主數據查詢/維護)</param>
        /// <param name="tBUKRS">公司別(T012、T016、C069、T022)</param>
        /// <param name="tAccountNo">AD帳號</param>
        /// <returns></returns>
        public bool checkEmpIsExistSRTeamMapping_OLD(string pOperationID_Contract, string tBUKRS, string tAccountNo)
        {
            bool reValue = false;

            var bean = dbPSIP.TB_ONE_RoleParameter.FirstOrDefault(x => x.Disabled == 0 && x.cOperationID.ToString() == pOperationID_Contract &&
                                                                    x.cFunctionID == "PERSON" && x.cCompanyID == tBUKRS &&
                                                                    x.cNo == "OLDORG" && x.cValue.ToLower() == tAccountNo.ToLower());

            if (bean != null)
            {
                reValue = true;
            }

            return reValue;
        }
        #endregion

        #region 判斷登入人員是否有在7X24工程師清單裡(true.有 false.否)
        /// <summary>
        /// 判斷登入人員是否有在7X24工程師清單裡(true.有 false.否)
        /// </summary>        
        /// <param name="pOperationID_Contract">程式作業編號檔系統ID(合約主數據查詢/維護)</param>
        /// <param name="tBUKRS">公司別(T012、T016、C069、T022)</param>
        /// <param name="tAccountNo">AD帳號</param>
        /// <returns></returns>
        public bool checkEmpIsExist7X24List(string pOperationID_Contract, string tBUKRS, string tAccountNo)
        {
            bool reValue = false;

            var bean = dbPSIP.TB_ONE_RoleParameter.FirstOrDefault(x => x.Disabled == 0 && x.cOperationID.ToString() == pOperationID_Contract && 
                                                                   x.cFunctionID == "PERSON" && x.cCompanyID == tBUKRS &&
                                                                   x.cNo == "7X24" && x.cValue.ToLower() == tAccountNo.ToLower());

            if (bean != null)
            {
                reValue = true;
            }

            return reValue;
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

        #region 取得服務案件狀態值說明
        /// <summary>
        /// 取得服務案件狀態值說明
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

        #region 取得服務案件case內容
        /// <summary>
        /// 取得服務案件case內容
        /// </summary>
        /// <param name="IV_SRID">SRID</param>
        /// <param name="pOperationID_GenerallySR">程式作業編號檔系統ID(一般服務)</param>
        /// <returns></returns>
        public Dictionary<string, object> GetSRDetail(string IV_SRID, string pOperationID_GenerallySR)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();

            string EV_TYPE = "ZSR1";
            string EV_CONTACT = string.Empty;
            string EV_ADDR = string.Empty;
            string EV_TEL = string.Empty;
            string EV_MOBILE = string.Empty;
            string EV_EMAIL = string.Empty;
            string EV_SQ = string.Empty;
            string EV_SLASRV = string.Empty;
            string EV_SLARESP = string.Empty;
            string EV_WTYKIND = string.Empty;
            string MAServiceType = string.Empty;
            string EV_CompanyName = string.Empty;

            var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == IV_SRID);

            if (beanM != null)
            {
                switch (IV_SRID.Substring(0, 2))
                {
                    case "61": //一般服務
                        EV_TYPE = "ZSR1";
                        EV_CONTACT = string.IsNullOrEmpty(beanM.cRepairName) ? "": beanM.cRepairName;
                        EV_ADDR = string.IsNullOrEmpty(beanM.cRepairAddress) ? "" : beanM.cRepairAddress;
                        EV_TEL = string.IsNullOrEmpty(beanM.cRepairPhone) ? "" : beanM.cRepairPhone;
                        EV_MOBILE = string.IsNullOrEmpty(beanM.cRepairMobile) ? "" : beanM.cRepairMobile;
                        EV_EMAIL = string.IsNullOrEmpty(beanM.cRepairEmail) ? "" : beanM.cRepairEmail;
                        MAServiceType = string.IsNullOrEmpty(beanM.cMAServiceType) ? "" : beanM.cMAServiceType;
                        EV_SQ = string.IsNullOrEmpty(beanM.cSQPersonName) ? "" : beanM.cSQPersonName;
                        break;
                    case "63": //裝機服務
                        EV_TYPE = "ZSR3";                        
                        break;
                    case "65": //定維服務
                        EV_TYPE = "ZSR5";
						EV_CONTACT = string.IsNullOrEmpty(beanM.cRepairName) ? "" : beanM.cRepairName;
						EV_ADDR = string.IsNullOrEmpty(beanM.cRepairAddress) ? "" : beanM.cRepairAddress;
						EV_TEL = string.IsNullOrEmpty(beanM.cRepairPhone) ? "" : beanM.cRepairPhone;
						EV_MOBILE = string.IsNullOrEmpty(beanM.cRepairMobile) ? "" : beanM.cRepairMobile;
						EV_EMAIL = string.IsNullOrEmpty(beanM.cRepairEmail) ? "" : beanM.cRepairEmail;
						MAServiceType = string.IsNullOrEmpty(beanM.cMAServiceType) ? "" : beanM.cMAServiceType;
						EV_SQ = string.IsNullOrEmpty(beanM.cSQPersonName) ? "" : beanM.cSQPersonName;
						break;
                }

                EmployeeBean EmpBean = new EmployeeBean();
                EmpBean = findEmployeeInfoByERPID(beanM.cMainEngineerID);

                string[] tArySLA = findSRSLACondition(IV_SRID);
                EV_SLARESP = tArySLA[0];
                EV_SLASRV = tArySLA[1];

                EV_WTYKIND = findSysParameterDescription(pOperationID_GenerallySR, "OTHER", EmpBean.BUKRS, "SRMATYPE", MAServiceType);

                EV_CompanyName = findCompanyNameByTeamID(beanM.cTeamID);

                results.Add("EV_CUSTOMER", beanM.cCustomerName);       //客戶名稱
                results.Add("EV_DESC", beanM.cDesc);                  //說明                
                results.Add("EV_PROBLEM", beanM.cNotes);              //詳細描述
                results.Add("EV_MAINENG", beanM.cMainEngineerName);    //主要工程師姓名
                results.Add("EV_MAINENGID", beanM.cMainEngineerID);    //主要工程師ERPID                
                results.Add("EV_CONTACT", EV_CONTACT);               //報修人姓名
                results.Add("EV_ADDR", EV_ADDR);                    //報修人地址
                results.Add("EV_TEL", EV_TEL);                      //報修人電話
                results.Add("EV_MOBILE", EV_MOBILE);                //報修人手機
                results.Add("EV_EMAIL", EV_EMAIL);                  //報修人Email
                results.Add("EV_TYPE", EV_TYPE);                   //服務種類
                results.Add("EV_COUNTIN", "");                      //計數器進(群輝用，先保持空白)
                results.Add("EV_COUNTOUT", "");                     //計數器出(群輝用，先保持空白)
                results.Add("EV_SQ", EV_SQ);                       //SQ人員名稱
                results.Add("EV_DEPARTMENT", EmpBean.BUKRS);        //公司別(T012、T016、C069、t022)
                results.Add("EV_SLASRV", EV_SLASRV);               //SLA服務條件
                results.Add("EV_WTYKIND", EV_WTYKIND);             //維護服務種類(Z01.保固內、Z02.保固外、Z03.合約、Z04.3rd Party)
                results.Add("EV_CompanyName", EV_CompanyName);     //公司名稱
            }

            if (!string.IsNullOrEmpty(IV_SRID))
            {
                #region 聯絡人窗口資訊(共用)
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

                #region 產品序號資訊(一般)
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

                #region 處理與工時紀錄(共用)
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

                #region 零件更換資訊(一般)
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

                #region 序號回報資訊(裝機)
                var beanD_SF = dbOne.TB_ONE_SRDetail_SerialFeedback.Where(x => x.Disabled == 0 && x.cSRID == IV_SRID);

                List<SFSNLIST> sfsnList = new List<SFSNLIST>();

                foreach (var beanD in beanD_SF)
                {
                    SFSNLIST snBean = new SFSNLIST();
                    snBean.SNNO = beanD.cSerialID;              //機器序號                    

                    sfsnList.Add(snBean);
                }

                results.Add("table_ET_SFSNLIST", sfsnList);
                #endregion
            }

            return results;
        }
        #endregion

        #region 取得客戶聯絡資訊檔清單
        /// <summary>
        /// 取得客戶聯絡資訊檔清單
        /// </summary>
        /// <returns></returns>
        public List<TB_ONE_SRDetail_Contact> findSRDetailContactList()
        {
            var beans = dbOne.TB_ONE_SRDetail_Contact.Where(x => x.Disabled == 0).ToList();

            return beans;
        }
        #endregion

        #region 取得客戶聯絡資訊檔的聯絡人名稱By List
        /// <summary>
        /// 取得客戶聯絡資訊檔的聯絡人名稱By List
        /// </summary>
        /// <param name="tList">服務團隊清單</param>
        /// <param name="tSRID">SRID</param>
        /// <returns></returns>
        public string TransSRDetailContactName(List<TB_ONE_SRDetail_Contact> tList, string tSRID)
        {
            string reValue = string.Empty;

            var beans = tList.Where(x => x.cSRID == tSRID);

            foreach (var bean in beans)
            {
                reValue += bean.cContactName + "<br/>";
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

        #region 取得合約明細-工程師指派檔之工程師員工編號(回傳多筆以;號隔開)
        /// <summary>
        ///  取得合約明細-工程師指派檔之工程師員工編號(回傳多筆以;號隔開)
        /// </summary>
        /// <param name="cContractID">文件編號</param>
        /// <param name="cIsMainEngineer">是否為主要工程師(Y/N)</param>
        /// <returns></returns>
        public string findContractDetailENG(string cContractID, string cIsMainEngineer)
        {
            string reValue = string.Empty;

            var beans = dbOne.TB_ONE_ContractDetail_ENG.Where(x => x.Disabled == 0 && x.cContractID == cContractID && x.cIsMainEngineer == cIsMainEngineer);

            foreach(var bean in beans)
            {
                reValue += bean.cEngineerID + ";";
            }

            return reValue.TrimEnd(';');
        }
        #endregion

        #endregion -----↑↑↑↑↑待辦清單 ↑↑↑↑↑-----   

        #region -----↓↓↓↓↓服務Mail相關 ↓↓↓↓↓-----

        #region 取得【共用】案件種類的郵件主旨
        /// <summary>
        /// 取得【共用】案件種類的郵件主旨
        /// </summary>
        /// <param name="cCondition">服務案件執行條件(ADD.新建、TRANS.轉派主要工程師、REJECT.駁回、HPGCSN.HPGCSN申請、HPGCSNDONE.HPGCSN完成、SECFIX.二修、SAVE.保存、SUPPORT.技術支援升級、THRPARTY.3Party、CANCEL.取消、DONE.完修 DOA.維修/DOA INSTALLING.裝機中 INSTALLDONE.裝機完成 MAINTAINDONE.定保完成)</param>
        /// <param name="SRID">服務ID</param>
        /// <param name="CusName">客戶名稱</param>
        /// <param name="TeamNAME">服務團隊</param>
        /// <param name="SRCase">服務案件種類</param>
        /// <param name="MainENG">主要工程師</param>
        /// <param name="SRPathWay">報修管道</param>
        /// <returns></returns>
        public string findGenerallySRMailSubject(SRCondition cCondition, string SRID, string CusName, string TeamNAME, string SRCase, string MainENG, string SRPathWay)
        {
            string reValue = string.Empty;

            if (SRPathWay != "")
            {
                SRPathWay = "[" + SRPathWay + "] ";
            }

            switch (cCondition)
            {
                case SRCondition.ADD:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 派單通知[<服務ID>]                  
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 派單通知[" + SRID + "]";
                    break;

                case SRCondition.TRANS:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類>[<服務ID>]已轉到[<主要工程師>]名下，請留意！
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + "[" + SRID + "]已轉到[" + MainENG + "]名下，請留意！";
                    break;

                case SRCondition.REJECT:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 主管審核駁回通知[<服務ID>]
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 主管審核駁回通知[" + SRID + "]";
                    break;

                case SRCondition.HPGCSN:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 派單通知[<服務ID>]，需下料。
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 派單通知[" + SRID + "]，需下料。";
                    break;

                case SRCondition.HPGCSNDONE:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 派單通知[<服務ID>]，HPGCSN 已完成。
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 派單通知[" + SRID + "]，HPGCSN 已完成。";
                    break;

                case SRCondition.SECFIX:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 二修通知[<服務ID>]
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 二修通知[" + SRID + "]";
                    break;

                case SRCondition.SUPPORT:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 技術支援升級通知[<服務ID>]
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 技術支援升級通知[" + SRID + "]";
                    break;

                case SRCondition.SAVE:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 異動通知[<服務ID>]
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 異動通知[" + SRID + "]";
                    break;

                case SRCondition.THRPARTY:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 3Party通知[<服務ID>]
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 3Party通知[" + SRID + "]";
                    break;

                case SRCondition.CANCEL:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 取消通知[<服務ID>]，請關注！
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 取消通知[" + SRID + "]，請關注！";
                    break;

                case SRCondition.DONE:
                    //[<報修管道>] [<客戶名稱>] <服務團隊>_<服務案件種類> 完修通知[<服務ID>]，已完修！
                    reValue = SRPathWay + "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 完修通知[" + SRID + "]，已完修！";
                    break;

                case SRCondition.DOA:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 維修/DOA通知[<服務ID>]
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 維修/DOA通知[" + SRID + "]";
                    break;

                case SRCondition.INSTALLDONE:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 裝機完成通知[<服務ID>]，已裝機完成！
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 裝機完成通知[" + SRID + "]，已裝機完成！";
                    break;

                case SRCondition.MAINTAINDONE:
                    //[<客戶名稱>] <服務團隊>_<服務案件種類> 定保完成通知[<服務ID>]，已定保完成！
                    reValue = "[" + CusName + "] " + TeamNAME + "_" + SRCase + " 定保完成通知[" + SRID + "]，已定保完成！";
                    break;
            }

            return reValue;
        }
        #endregion

        #region 取得【一般服務】案件種類的郵件主旨(for客戶)
        /// <summary>
        /// 取得【一般服務】案件種類的郵件主旨(for客戶)
        /// </summary>
        /// <param name="cCondition">服務案件執行條件(ADD.新建、TRANS.轉派主要工程師、REJECT.駁回、HPGCSN.HPGCSN申請、HPGCSNDONE.HPGCSN完成、SECFIX.二修、SAVE.保存、SUPPORT.技術支援升級、THRPARTY.3Party、CANCEL.取消、DONE.完修 DOA.維修/DOA INSTALLING.裝機中 INSTALLDONE.裝機完成 MAINTAINDONE.定保完成)</param>
        /// <param name="SRID">服務ID</param>
        /// <param name="CusName">客戶名稱</param>  
        /// <param name="SRPathWay">報修管道</param>
        /// <param name="CompanyName">公司名稱</param>
        /// <returns></returns>
        public string findGenerallySRMailSubject_ToCustomer(SRCondition cCondition, string SRID, string CusName, string SRPathWay, string CompanyName)
        {
            string reValue = string.Empty;

            if (SRPathWay != "")
            {
                SRPathWay = "[" + SRPathWay + "] ";
            }

            switch (cCondition)
            {
                case SRCondition.ADD:
                    //大同世界科技[<服務ID>] ，報修通知                 
                    reValue = SRPathWay + CompanyName + "[" + SRID + "]，報修通知";
                    break;               

                case SRCondition.DONE:
                    //大同世界科技[<客戶名稱>]您的服務[<服務ID>]已完成
                    reValue = SRPathWay + CompanyName + "[" + CusName + "] 您的服務[" + SRID + "]已完修";
                    break;
            }

            return reValue;
        }
        #endregion       

        #region 取得【共用】案件客戶報修窗口資訊Html Table
        /// <summary>
        /// 取得【共用】案件客戶報修窗口資訊Html Table
        /// </summary>
        /// <param name="SRRepair_List">服務案件客戶報修人資訊清單</param>
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
                strHTML.AppendLine("<div style='width:800;'>");
                //strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>客戶名稱："+ CusName +"</p>");
                strHTML.AppendLine("    <p>[客戶報修窗口資訊]</p>");
                strHTML.AppendLine("    <table style='width:100%;font-family:微軟正黑體;' align='left' border='1'>");
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

        #region 取得【共用】案件客戶聯絡窗口資訊Html Table
        /// <summary>
        /// 取得【共用】案件客戶聯絡窗口資訊Html Table
        /// </summary>
        /// <param name="SRContact_List">服務案件客戶聯絡人資訊清單</param>
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
                strHTML.AppendLine("<div style='width:800;'>");
                //strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>[客戶聯絡窗口資訊]</p>");
                strHTML.AppendLine("    <table style='width:100%;font-family:微軟正黑體;' align='left' border='1'>");
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
            }

            reValue = strHTML.ToString();

            return reValue;
        }
        #endregion

        #region 取得【共用】案件處理與工時紀錄資訊Html Table
        /// <summary>
        /// 取得【共用】案件處理與工時紀錄資訊Html Table
        /// </summary>
        /// <param name="SRRecord_List">服務案件處理與工時紀錄資訊清單</param>
        /// <param name="SRReport_List">服務報告書資訊清單</param>
        /// <returns></returns>
        public string findGenerallySRRecord_Table(List<SRRECORDINFO> SRRecord_List, List<SRREPORTINFO> SRReport_List)
        {
            #region 格式
            //[處理與工時紀錄資料]												
            //服務工程師姓名	接單時間	         出發時間	         到場時間	         完成時間	        工時(分鐘)	處理紀錄	             服務報告書/附件
            //OOO	         2023-01-19 21:20	2023-01-19 21:25	2023-01-19 21:50	2023-01-19 22:55	65	         TEST處理紀錄03100945	URL
            #endregion

            StringBuilder strHTML = new StringBuilder();
            string reValue = string.Empty;            
            string tHypeLink = string.Empty;

            if (SRRecord_List.Count > 0)
            {
                strHTML.AppendLine("<div style='width:800;'>");
                //strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>[處理與工時紀錄資訊]</p>");
                strHTML.AppendLine("    <table style='width:100%;font-family:微軟正黑體;' align='left' border='1'>");
                strHTML.AppendLine("        <tr>");
                strHTML.AppendLine("            <td>服務工程師姓名</td>");
                strHTML.AppendLine("            <td>接單時間</td>");
                strHTML.AppendLine("            <td>出發時間</td>");
                strHTML.AppendLine("            <td>到場時間</td>");
                strHTML.AppendLine("            <td>完成時間</td>");
                strHTML.AppendLine("            <td>工時(分鐘)</td>");
                strHTML.AppendLine("            <td>處理紀錄</td>");
                strHTML.AppendLine("            <td>服務報告書/附件</td>");
                strHTML.AppendLine("        </tr>");

                foreach (var bean in SRRecord_List)
                {
                    tHypeLink = SetSRREPORTUrl_Html(SRReport_List, bean.CID);

                    strHTML.AppendLine("        <tr>");
                    strHTML.AppendLine("            <td>" + bean.ENGNAME + "</td>");
                    strHTML.AppendLine("            <td>" + bean.ReceiveTime + "</td>");
                    strHTML.AppendLine("            <td>" + bean.StartTime + "</td>");
                    strHTML.AppendLine("            <td>" + bean.ArriveTime + "</td>");
                    strHTML.AppendLine("            <td>" + bean.FinishTime + "</td>");
                    strHTML.AppendLine("            <td>" + bean.WorkHours + "</td>");
                    strHTML.AppendLine("            <td>" + bean.Desc.Replace("\r\n", "<br/>") + "</td>");
                    strHTML.AppendLine("            <td>" + tHypeLink + "</td>");
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

        #region 組服務報告書html的Url
        /// <summary>
        /// 組服務報告書html的Url
        /// </summary>
        /// <param name="SRReport_List">服務報告書清單</param>
        /// <param name="cID">系統ID</param>
        /// <returns></returns>
        public string SetSRREPORTUrl_Html(List<SRREPORTINFO> SRReport_List, string cID)
        {
            string reValue = string.Empty;
            int Count = 1;

            var beans = SRReport_List.Where(x => x.CID == cID);

            foreach (var bean in beans)
            {
                if (bean.SRReportURL != "")
                {
                    reValue += "<span><a href = " + bean.SRReportURL + "><span>服務報告書(附件)" + Count.ToString() + "</span></a></span></br>";
                    Count++;
                }
            }            

            return reValue;
        }
        #endregion

        #region 取得【一般服務】案件產品序號資訊Html Table
        /// <summary>
        /// 取得【一般服務】案件產品序號資訊Html Table
        /// </summary>
        /// <param name="SRSeiral_List">服務案件產品序號資訊清單</param>
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
                strHTML.AppendLine("<div style='width:800;'>");
                //strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>[產品序號資訊]</p>");
                strHTML.AppendLine("    <table style='width:100%;font-family:微軟正黑體;' align='left' border='1'>");
                strHTML.AppendLine("        <tr>");
                strHTML.AppendLine("            <td>序號</td>");
                strHTML.AppendLine("            <td>更換後序號</td>");
                strHTML.AppendLine("            <td>機器型號</td>");
                strHTML.AppendLine("            <td>Product Number</td>");
                strHTML.AppendLine("            <td>料號</td>");
                strHTML.AppendLine("            <td>裝機資訊</td>");
                strHTML.AppendLine("        </tr>");

                foreach (var bean in SRSeiral_List)
                {
                    strHTML.AppendLine("        <tr>");
                    strHTML.AppendLine("            <td>" + bean.SERIALID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.NEWSERIALID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.PRDNAME + "</td>");
                    strHTML.AppendLine("            <td>" + bean.PRDNUMBER + "</td>");
                    strHTML.AppendLine("            <td>" + bean.PRDID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.INSTALLID + "</td>");
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

        #region 取得【一般服務】案件零件更換資訊Html Table
        /// <summary>
        /// 取得【一般服務】案件零件更換資訊Html Table
        /// </summary>
        /// <param name="SRParts_List">服務案件零件更換資訊清單</param>
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
                strHTML.AppendLine("<div style='width:800;'>");
                //strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>[零件更換資訊]</p>");
                strHTML.AppendLine("    <table style='width:100%;font-family:微軟正黑體;' align='left' border='1'>");
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
            }

            reValue = strHTML.ToString();

            return reValue;
        }
        #endregion

        #region 取得【裝機服務】物料訊息資訊Html Table
        /// <summary>
        /// 取得【裝機服務】物料訊息資訊Html Table
        /// </summary>
        /// <param name="SRMaterial_List">服務案件物料訊息資訊清單</param>
        /// <returns></returns>
        public string findInstallSRMaterial_Table(List<SRMATERIALlNFO> SRMaterial_List)
        {
            #region 格式
            //[物料訊息資訊]
            //物料代號 料號說明    數量 基本內文    製造商零件號碼 廠牌  產品階層
            //OOO      OOO         O     OOO        OOO            OOO   OOO
            #endregion

            StringBuilder strHTML = new StringBuilder();
            string reValue = string.Empty;            

            if (SRMaterial_List.Count > 0)
            {
                strHTML.AppendLine("<div style='width:800;'>");
                //strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>[物料訊息資訊]</p>");
                strHTML.AppendLine("    <table style='width:100%;font-family:微軟正黑體;' align='left' border='1'>");
                strHTML.AppendLine("        <tr>");                
                strHTML.AppendLine("            <td>物料代號</td>");
                strHTML.AppendLine("            <td>料號說明</td>");
                strHTML.AppendLine("            <td>數量</td>");
                strHTML.AppendLine("            <td>基本內文</td>");
                strHTML.AppendLine("            <td>製造商零件號碼</td>");
                strHTML.AppendLine("            <td>廠牌</td>");
                strHTML.AppendLine("            <td>產品階層</td>");                
                strHTML.AppendLine("        </tr>");

                foreach (var bean in SRMaterial_List)
                {
                    strHTML.AppendLine("        <tr>");                    
                    strHTML.AppendLine("            <td>" + bean.MaterialID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.MaterialName + "</td>");
                    strHTML.AppendLine("            <td>" + bean.Quantity + "</td>");
                    strHTML.AppendLine("            <td>" + bean.BasicContent.Replace("\r\n", "<br/>") + "</td>");
                    strHTML.AppendLine("            <td>" + bean.MFPNumber + "</td>");
                    strHTML.AppendLine("            <td>" + bean.Brand + "</td>");
                    strHTML.AppendLine("            <td>" + bean.ProductHierarchy + "</td>");                    
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

        #region 取得【裝機服務】序號回報資訊Html Table
        /// <summary>
        /// 取得【裝機服務】序號回報資訊Html Table
        /// </summary>        
        /// <param name="SRFeedBack_List">服務案件序號回報資訊清單</param>
        /// <param name="SRConfig_List">裝機Config資訊清單</param>
        /// <returns></returns>
        public string findInstallSRFeedBack_Table(List<SRSERIALFEEDBACKlNFO> SRFeedBack_List, List<SRREPORTINFO> SRConfig_List)
        {
            #region 格式
            //[序號回報資訊]
            //序號     物料代號      料號說明   裝機Config檔
            //SGH1OOO  DL360pG8OOO   654081OOO   URL
            #endregion

            StringBuilder strHTML = new StringBuilder();
            string reValue = string.Empty;
            string tHypeLink = string.Empty;            

            if (SRFeedBack_List.Count > 0)
            {
                strHTML.AppendLine("<div style='width:800;'>");
                //strHTML.AppendLine("    <p>&nbsp;</p>");
                strHTML.AppendLine("    <p>[序號回報資訊]</p>");
                strHTML.AppendLine("    <table style='width:100%;font-family:微軟正黑體;' align='left' border='1'>");
                strHTML.AppendLine("        <tr>");
                strHTML.AppendLine("            <td>序號</td>");
                strHTML.AppendLine("            <td>物料代號</td>");
                strHTML.AppendLine("            <td>料號說明</td>");                
                strHTML.AppendLine("            <td>裝機Config檔</td>");                
                strHTML.AppendLine("        </tr>");

                foreach (var bean in SRFeedBack_List)
                {
                    tHypeLink = SetIstallConfigUrl_Html(SRConfig_List, bean.CID);

                    strHTML.AppendLine("        <tr>");
                    strHTML.AppendLine("            <td>" + bean.SERIALID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.MaterialID + "</td>");
                    strHTML.AppendLine("            <td>" + bean.MaterialName + "</td>");
                    strHTML.AppendLine("            <td>" + tHypeLink + "</td>");                    
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

        #region 組裝機Config檔html的Url
        /// <summary>
        /// 組裝機Config檔html的Url
        /// </summary>
        /// <param name="SRConfig_List">裝機Config檔清單</param>
        /// <param name="cID">系統ID</param>
        /// <returns></returns>
        public string SetIstallConfigUrl_Html(List<SRREPORTINFO> SRConfig_List, string cID)
        {
            string reValue = string.Empty;
            int Count = 1;

            var beans = SRConfig_List.Where(x => x.CID == cID);

            foreach(var bean in beans)
            {
                if (bean.SRReportURL != "")
                {
                    reValue += "<span><a href = " + bean.SRReportURL + "><span>裝機Config檔" + Count.ToString() + "</span></a></span></br>";
                    Count++;
                }
            }

            return reValue;
        }
        #endregion

        #region 組服務案件Mail相關資訊
        /// <summary>
        /// 組服務案件Mail相關資訊
        /// </summary>
        /// <param name="cCondition">服務案件執行條件(ADD.新建、TRANS.轉派主要工程師、REJECT.駁回、HPGCSN.HPGCSN申請、HPGCSNDONE.HPGCSN完成、SECFIX.二修、SAVE.保存、SUPPORT.技術支援升級、THRPARTY.3Party、CANCEL.取消、DONE.完修 DOA.維修/DOA INSTALLING.裝機中 INSTALLDONE.裝機完成 MAINTAINDONE.定保完成)</param>
        /// <param name="cOperationID_GenerallySR">程式作業編號檔系統ID(一般服務)</param>
        /// <param name="cOperationID_InstallSR">程式作業編號檔系統ID(裝機服務)</param>
        /// <param name="cOperationID_MaintainSR">程式作業編號檔系統ID(定維服務)</param>
        /// <param name="cBUKRS">公司別(T012、T016、C069、T022)</param>
        /// <param name="cSRID">SRID(服務案件ID)</param>           
        /// <param name="tONEURLName">One Service站台名稱</param>        
        /// <param name="tAttachURLName">附件站台名稱</param>
        /// <param name="tAttachPath">附件路徑名稱</param>
        /// <param name="cLoginName">登入人員姓名</param>
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>
        public void SetSRMailContent(SRCondition cCondition, string cOperationID_GenerallySR, string cOperationID_InstallSR, string cOperationID_MaintainSR, 
                                    string cBUKRS, string cSRID, string tONEURLName, string tAttachURLName, string tAttachPath, string cLoginName, bool tIsFormal)
        {          
            string tMailToTemp = string.Empty;
            string tMailCcTemp = string.Empty; 
            string tMailBCcTemp = string.Empty;

            string tMailTo = string.Empty;              //收件者            
            string tMailCc = string.Empty;              //副本            
            string tMailBCc = string.Empty;             //密件副本
            string tHypeLink = string.Empty;            //超連結
            string tSeverName = string.Empty;           //主機名稱

            string cCompanyName = string.Empty;         //公司名稱
            string cSRCase = string.Empty;              //服務案件種類            
            string cCreateUser = string.Empty;          //派單人員
            string cCreateUserEmail = string.Empty;     //派單人員Email
            string cTeamName = string.Empty;            //服務團隊
            string cTeamMGR = string.Empty;             //服務團隊主管
            string cTeamMGREmail = string.Empty;        //服務團隊主管Email
            string cMainENG = string.Empty;             //主要工程師
            string cMainENGEmail = string.Empty;        //主要工程師Email
            string cAssENG = string.Empty;              //協助工程師
            string cAssENGEmail = string.Empty;         //協助工程師Email
            string cTechMGR = string.Empty;             //技術主管
            string cTechMGREmail = string.Empty;        //技術主管Email
            string cSalesEMP = string.Empty;            //業務人員
            string cSalesEMPEmail = string.Empty;       //業務人員Email
            string cSecretaryEMP = string.Empty;        //業務祕書
            string cSecretaryEMPEmail = string.Empty;   //業務祕書Email

            string cStatus = string.Empty;              //狀態
            string cStatusDesc = string.Empty;          //狀態說明            
            string cCreatedDate = string.Empty;         //派單時間
            string cContractID = string.Empty;          //合約文件編號
            string cMAServiceType = string.Empty;       //維護服務種類
            string cSecFix = string.Empty;              //是否為二修
            string cInternalWork = string.Empty;        //是否為內部作業
            string cSRPathWay = string.Empty;           //報修管道
            string cSalesNo = string.Empty;             //銷售訂單號
            string cShipmentNo = string.Empty;          //出貨單號
            string cDesc = string.Empty;                //需求說明            
            string cNotes = string.Empty;               //詳細描述

            string cCusName = string.Empty;             //客戶名稱            
            string cRepairName = string.Empty;          //報修人
            string cRepairPhone = string.Empty;         //報修人電話
            string cRepairMobile = string.Empty;        //報修人手機
            string cRepairAddress = string.Empty;       //報修人地址
            string cRepairEmail = string.Empty;         //報修人Email

            try
            {   
                List<SRTEAMORGINFO> SRTeam_List = new List<SRTEAMORGINFO>();
                List<SREMPINFO> SRCreateUser_List = new List<SREMPINFO>();
                List<SREMPINFO> SRMainENG_List = new List<SREMPINFO>();
                List<SREMPINFO> SRAssENG_List = new List<SREMPINFO>();
                List<SREMPINFO> SRTechMGR_List = new List<SREMPINFO>();
                List<SREMPINFO> SRSalesEMP_List = new List<SREMPINFO>();
                List<SREMPINFO> SRSecretaryEMP_List = new List<SREMPINFO>();

                List<SRCONTACTINFO> SRRepair_List = new List<SRCONTACTINFO>();
                List<SRCONTACTINFO> SRContact_List = new List<SRCONTACTINFO>();
                List<SRRECORDINFO> SRRecord_List = new List<SRRECORDINFO>();

                List<SRSERIALMATERIALINFO> SRSeiral_List = new List<SRSERIALMATERIALINFO>();
                List<SRPARTSREPALCEINFO> SRParts_List = new List<SRPARTSREPALCEINFO>();                
                List<SRMATERIALlNFO> SRMaterial_List = new List<SRMATERIALlNFO>();
                List<SRSERIALFEEDBACKlNFO> SRFeedBack_List = new List<SRSERIALFEEDBACKlNFO>();                

                List<SRREPORTINFO> SRReport_List = new List<SRREPORTINFO>();
                List<SRREPORTINFO> SRConfig_List = new List<SRREPORTINFO>();

                var beanM = dbOne.TB_ONE_SRMain.FirstOrDefault(x => x.cSRID == cSRID);

                if (beanM != null)
                {
                    #region -----↓↓↓↓↓主檔 ↓↓↓↓↓-----
                    SRIDMAININFO SRMain = new SRIDMAININFO();

                    #region 派單人員相關
                    string[] AryUser = cLoginName.Split(' ');
                    string cCreateUserERPID = string.Empty;

                    if (AryUser.Length == 2)
                    {
                        cCreateUserERPID = findEmployeeERPIDByEName(AryUser[1]);
                        
                        SRCreateUser_List = findSREMPINFO(cCreateUserERPID);
                        cCreateUser = cLoginName;
                        cCreateUserEmail = findSREMPEmail(SRCreateUser_List);
                    }
                    else
                    {
                        cCreateUser = cLoginName;
                    }
                    #endregion

                    #region 服務團隊相關
                    cCompanyName = findCompanyNameByTeamID(beanM.cTeamID);
                    SRTeam_List = findSRTEAMORGINFO(beanM.cTeamID);
                    cSRCase = findSRIDType(cSRID);
                    cTeamName = findSRTeamName(SRTeam_List);
                    cTeamMGR = findSRTeamMGRName(SRTeam_List);
                    cTeamMGREmail = findSRTeamMGREmail(SRTeam_List);
                    #endregion

                    #region 主要工程師/協助工程師/技術主管相關
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

                    #region 業務人員/業務祕書相關
                    SRSalesEMP_List = findSREMPINFO(beanM.cSalesID);
                    cSalesEMP = findSREMPName(SRSalesEMP_List);
                    cSalesEMPEmail = findSREMPEmail(SRSalesEMP_List);

                    SRSecretaryEMP_List = findSREMPINFO(beanM.cSecretaryID);
                    cSecretaryEMP = findSREMPName(SRSecretaryEMP_List);
                    cSecretaryEMPEmail = findSREMPEmail(SRSecretaryEMP_List);
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
                    cCreatedDate = Convert.ToDateTime(beanM.CreatedDate).ToString("yyyy-MM-dd HH:mm");
                    cStatus = beanM.cStatus;

                    if (cSRID.Substring(0, 2) == "61")
                    {
                        cStatusDesc = findSysParameterDescription(cOperationID_GenerallySR, "OTHER", cBUKRS, "SRSTATUS", beanM.cStatus);
                    }
                    else if (cSRID.Substring(0, 2) == "63")
                    {
                        cStatusDesc = findSysParameterDescription(cOperationID_InstallSR, "OTHER", cBUKRS, "SRSTATUS", beanM.cStatus);
                    }
                    else if (cSRID.Substring(0, 2) == "65")
                    {
                        cStatusDesc = findSysParameterDescription(cOperationID_MaintainSR, "OTHER", cBUKRS, "SRSTATUS", beanM.cStatus);
                        cContractID = beanM.cContractID;
                    }

                    cMAServiceType = findSysParameterDescription(cOperationID_GenerallySR, "OTHER", cBUKRS, "SRMATYPE", beanM.cMAServiceType); 
                    cSecFix = beanM.cIsSecondFix;
                    cInternalWork = string.IsNullOrEmpty(beanM.cIsInternalWork) ? "N" : beanM.cIsInternalWork;
                    cSRPathWay = findSysParameterDescription(cOperationID_GenerallySR, "OTHER", cBUKRS, "SRPATH", beanM.cSRPathWay);
                    cSalesNo = string.IsNullOrEmpty(beanM.cSalesNo) ? "" : beanM.cSalesNo;
                    cShipmentNo = string.IsNullOrEmpty(beanM.cShipmentNo) ? "" : beanM.cShipmentNo;
                    cDesc = beanM.cDesc;
                    cNotes = beanM.cNotes;
                    #endregion

                    SRMain.SRID = cSRID;
                    SRMain.Status = cStatus;
                    SRMain.StatusDesc = cStatusDesc;
                    SRMain.SRCase = cSRCase;
                    SRMain.CompanyName = cCompanyName;
                    SRMain.CreateUser = cCreateUser;
                    SRMain.TeamNAME = cTeamName;
                    SRMain.TeamMGR = cTeamMGR;
                    SRMain.MainENG = cMainENG;
                    SRMain.AssENG = cAssENG;
                    SRMain.TechMGR = cTechMGR;
                    SRMain.SalesEMP = cSalesEMP;
                    SRMain.SecretaryEMP = cSecretaryEMP;
                    SRMain.ContractID = cContractID;
                    SRMain.CreatedDate = cCreatedDate;
                    SRMain.MAServiceType = cMAServiceType;
                    SRMain.SecFix = cSecFix;
                    SRMain.InternalWork = cInternalWork;
                    SRMain.SRPathWay = cSRPathWay;
                    SRMain.SalesNo = cSalesNo;
                    SRMain.ShipmentNo = cShipmentNo;
                    SRMain.Desc = string.IsNullOrEmpty(cSRPathWay) ? cDesc : "【" + cSRPathWay + "】" + cDesc;
                    SRMain.Notes = cNotes;                    

                    SRMain.CusName = cCusName;
                    SRMain.RepairName = cRepairName;
                    SRMain.RepairPhone = cRepairPhone;
                    SRMain.RepairMobile = cRepairMobile;
                    SRMain.RepairAddress = cRepairAddress;
                    SRMain.RepairEmail = cRepairEmail;

                    SRMain.CreateUserEmail = cCreateUserEmail;
                    SRMain.TeamMGREmail = cTeamMGREmail;
                    SRMain.MainENGEmail = cMainENGEmail;
                    SRMain.AssENGEmail = cAssENGEmail;
                    SRMain.TechMGREmail = cTechMGREmail;
                    SRMain.SalesEmail = cSalesEMPEmail;
                    SRMain.SecretaryEmail = cSecretaryEMPEmail;
                    #endregion -----↑↑↑↑↑Mail相關 ↑↑↑↑↑-----  

                    #region -----↓↓↓↓↓客戶報修窗口資料 ↓↓↓↓↓-----
                    SRRepair_List = findSRREPAIRINFO(cSRID, cRepairName, cRepairPhone, cRepairMobile, cRepairAddress, cRepairEmail);
                    #endregion -----↑↑↑↑↑客戶報修窗口資料 ↑↑↑↑↑----- 

                    #region -----↓↓↓↓↓客戶聯絡窗口資料 ↓↓↓↓↓-----
                    SRContact_List = findSRCONTACTINFO(cSRID);
                    #endregion -----↑↑↑↑↑客戶聯絡窗口資料 ↑↑↑↑↑----- 

                    #region -----↓↓↓↓↓處理與工時紀錄資料 ↓↓↓↓↓-----
                    SRRecord_List = findSRRECORDINFO(cSRID, tAttachURLName);
                    #endregion -----↑↑↑↑↑處理與工時紀錄資料 ↑↑↑↑↑----- 

                    #region -----↓↓↓↓↓產品序號資訊 ↓↓↓↓↓-----
                    SRSeiral_List = findSRSERIALMATERIALINFO(cSRID);
                    #endregion -----↑↑↑↑↑產品序號資訊 ↑↑↑↑↑----- 

                    #region -----↓↓↓↓↓零件更換資訊 ↓↓↓↓↓-----
                    SRParts_List = findSRPARTSREPALCEINFO(cSRID);
                    #endregion -----↑↑↑↑↑零件更換資訊 ↑↑↑↑↑-----                     

                    #region -----↓↓↓↓↓物料訊息資訊 ↓↓↓↓↓-----
                    SRMaterial_List = findSRMATERIALlNFO(cSRID);
                    #endregion -----↑↑↑↑↑物料訊息資訊 ↑↑↑↑↑-----                     

                    #region -----↓↓↓↓↓序號回報資訊 ↓↓↓↓↓-----
                    SRFeedBack_List = findSRSERIALFEEDBACKlNFO(cSRID);
                    #endregion -----↑↑↑↑↑序號回報資訊 ↑↑↑↑↑-----

                    #region -----↓↓↓↓↓服務報告書/附件資訊 ↓↓↓↓↓-----
                    SRReport_List = findSRREPORTINFO(cSRID, tAttachURLName, tAttachPath);
                    #endregion -----↑↑↑↑↑服務報告書/附件資訊 ↑↑↑↑↑-----  

                    #region -----↓↓↓↓↓裝機Config資訊 ↓↓↓↓↓-----
                    SRConfig_List = findSRCONFIGINFO(cSRID, tAttachURLName, tAttachPath);
                    #endregion -----↑↑↑↑↑裝機Config資訊 ↑↑↑↑↑-----

                    #region 發送服務案件Mail相關資訊(for客戶)，一般服務(新建或完修)才要發給客戶
                    if (cSRID.Substring(0, 2) == "61")
                    {
                        if (cCondition == SRCondition.ADD || cCondition == SRCondition.DONE)
                        {
                            if (SRMain.InternalWork != "Y")
                            {
                                SendSRMail_ToCustomer(cCondition, cSRID, cLoginName, tIsFormal, SRMain, SRRepair_List, SRContact_List, SRSeiral_List, SRReport_List);
                            }
                        }
                    }
                    #endregion

                    #region 發送服務案件Mail相關資訊  
                    SendSRMail(cCondition, cSRID, tONEURLName, cLoginName, tIsFormal, SRMain, SRRepair_List, SRContact_List, SRRecord_List, SRSeiral_List, SRParts_List, SRReport_List, SRMaterial_List, SRFeedBack_List, SRConfig_List);
                    #endregion
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

        #region 發送服務案件Mail相關資訊
        /// <summary>
        /// 發送服務案件Mail相關資訊
        /// </summary>
        /// <param name="cCondition">服務案件執行條件(ADD.新建、TRANS.轉派主要工程師、REJECT.駁回、HPGCSN.HPGCSN申請、HPGCSNDONE.HPGCSN完成、SECFIX.二修、SAVE.保存、SUPPORT.技術支援升級、THRPARTY.3Party、CANCEL.取消、DONE.完修 DOA.維修/DOA INSTALLING.裝機中 INSTALLDONE.裝機完成 MAINTAINDONE.定保完成)</param>
        /// <param name="cSRID">SRID</param>
        /// <param name="tONEURLName">One Service站台名稱</param>
        /// <param name="cLoginName">登入人員姓名</param>
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>
        /// <param name="SRMain">服務案件主檔資訊(For Mail)</param>
        /// <param name="SRRepair_List">服務案件客戶報修人資訊清單</param>
        /// <param name="SRContact_List">服務案件客戶聯絡人資訊清單</param>
        /// <param name="SRRecord_List">服務案件處理與工時紀錄資訊清單</param>
        /// <param name="SRSeiral_List">服務案件產品序號資訊清單</param>
        /// <param name="SRParts_List">服務案件零件更換資訊清單</param>
        /// <param name="SRReport_List">服務案件服務報告書資訊清單</param>
        /// <param name="SRMaterial_List">服務案件物料訊息資訊清單</param>
        /// <param name="SRFeedBack_List">服務案件序號回報資訊清單</param>
        /// <param name="SRConfig_List">服務案件裝機Config資訊清單</param>
        public void SendSRMail(SRCondition cCondition, string cSRID, string tONEURLName, string cLoginName, bool tIsFormal, SRIDMAININFO SRMain, 
                              List<SRCONTACTINFO> SRRepair_List, List<SRCONTACTINFO> SRContact_List, List<SRRECORDINFO> SRRecord_List, List<SRSERIALMATERIALINFO> SRSeiral_List, 
                              List<SRPARTSREPALCEINFO> SRParts_List, List<SRREPORTINFO> SRReport_List, List<SRMATERIALlNFO> SRMaterial_List,
                              List<SRSERIALFEEDBACKlNFO> SRFeedBack_List, List<SRREPORTINFO> SRConfig_List)
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
            string EndString = string.Empty;       
            string tSRRepair_Table = string.Empty;
            string tSRContact_Table = string.Empty;
            string tSRSeiral_Table = string.Empty;
            string tSRParts_Table = string.Empty;
            string tSRMaterial_Table = string.Empty;
            string tSRFeedBack_Table = string.Empty;
            string tSRRecord_Talbe = string.Empty;

            try
            {
                #region 取得收件者
                if (cCondition == SRCondition.HPGCSN || cCondition == SRCondition.HPGCSNDONE) //HPGCSN申請、HPGCSN完成
                {
                    #region 主要工程師
                    if (SRMain.MainENGEmail != "")
                    {
                        tMailToTemp = SRMain.MainENGEmail;
                    }
                    #endregion

                    #region 若為E0012.HPGCSN 申請、E0013.HPGCSN 完成則要給所有客服人員
                    Dictionary<string, string> Dic = getCUSTOMERSERVICEInfo(pSysOperationID);

                    foreach (KeyValuePair<string, string> item in Dic)
                    {
                        tMailToTemp += item.Value + ";";
                    }
                    #endregion
                }
                else
                {
                    if (SRMain.MainENGEmail != "") //有指派主要工程師
                    {
                        tMailToTemp = SRMain.MainENGEmail + SRMain.AssENGEmail + SRMain.TechMGREmail;
                    }
                    else //未指派主要工程師
                    {
                        tMailToTemp = SRMain.TeamMGREmail + SRMain.MainENGEmail + SRMain.AssENGEmail + SRMain.TechMGREmail;
                    }
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
                //有服務團隊主管
                if (SRMain.TeamMGREmail != "")
                {
                    tMailCcTemp += SRMain.TeamMGREmail;
                }

                //業務人員
                if (SRMain.SalesEmail != "")
                {
                    tMailCcTemp += SRMain.SalesEmail;
                }

                //若為(63.裝機、65.定維)才要取業務祕書
                if (cSRID.Substring(0,2) == "63" || cSRID.Substring(0, 2) == "65")
                {
                    //業務祕書
                    if (SRMain.SecretaryEmail != "")
                    {
                        tMailCcTemp += SRMain.SecretaryEmail;
                    }
                }

                //若為(61.一般)才要取派單人員
                if (cSRID.Substring(0, 2) == "61")
                {
                    if (SRMain.CreateUserEmail != "") //派單人員Email
                    {
                        tMailCcTemp += SRMain.CreateUserEmail;
                    }
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
                tMailBCcTemp = "Jordan.Chang@etatung.com;Elvis.Chang@etatung.com"; //測試用，等都正常了就註解掉

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
                    //tMailTo = "Elvis.Chang@etatung.com";
                    tMailCc += ";Elvis.Chang@etatung.com;Jordan.Chang@etatung.com;Cara.Tien@etatung.com";
                }
                #endregion

                #region 郵件主旨
                string tMailSubject = findGenerallySRMailSubject(cCondition, cSRID, SRMain.CusName, SRMain.TeamNAME, SRMain.SRCase, SRMain.MainENG, SRMain.SRPathWay);

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
                //主要工程師:	L3MGR
                //協助工程師:	ASSEngineer
                //技術主管:	TechMGR
                //狀態:	處理中
                //派單人員： 張豐穎 Elvis.Chang
                //派單時間:	2016/1/5 13:36
                //合約文件編號:	2540/7/23 00:00
                //維護服務種類:	保固內
                //是否為二修:	是
                //需求說明:	pls. support BUG FIX onsite
                //詳細描述:	硬體破損

                //客戶名稱：OOO股份有限公司
                //剩下參考明細Table的Html

                //請儘速至One Sevice 系統處理，謝謝！
                //查看待辦清單 =>超連結(http://172.31.7.56:32200/ServiceRequest/ToDoList)
                //提醒您：此為系統發送信函請勿直接回覆此信。                

                //-------此信件由系統管理員發出，請勿回覆此信件-------
                #endregion

                string tMailBody = string.Empty;

                if (tStatus == "E0015") //取消
                {
                    if (cSRID.Substring(0, 2) == "61") //一般
                    {
                        tHypeLink = tONEURLName + "/ServiceRequest/GenerallySR?SRID=" + cSRID;
                    }
                    else if (cSRID.Substring(0, 2) == "63") //裝機
                    {
                        tHypeLink = tONEURLName + "/ServiceRequest/InstallSR?SRID=" + cSRID;
                    }
                    else if (cSRID.Substring(0, 2) == "65") //定維
                    {
                        tHypeLink = tONEURLName + "/ServiceRequest/MaintainSR?SRID=" + cSRID;
                    }                    
                }
                else
                {
                    tHypeLink = tONEURLName + "/ServiceRequest/ToDoList";
                }

                if (SRMain.ContractID != "")
                {
                    tContractID = "合約文件編號：【" + SRMain.ContractID + "】</br>";
                }
                
                if (SRMain.SecFix == "Y")
                {
                    tSecFix = "是否為二修：【" + SRMain.SecFix + "】</br>";
                }

                if (cCondition != SRCondition.DONE && cCondition != SRCondition.INSTALLDONE && cCondition != SRCondition.MAINTAINDONE && cCondition != SRCondition.CANCEL)
                {
                    EndString = "<p>請儘速至One Sevice 系統處理，謝謝！</p><p><span><a href=" + tHypeLink + "><span>查看待辦清單</span></a></span></p>";
                }

                #region 取得【共用】案件客戶報修窗口資訊Html Table
                tSRRepair_Table = findGenerallySRRepair_Table(SRRepair_List, SRMain.CusName);
                #endregion

                #region 取得【共用】案件客戶聯絡窗口資訊Html Table
                tSRContact_Table = findGenerallySRContact_Table(SRContact_List);
                #endregion

                #region 取得【共用】案件處理與工時紀錄資訊Html Table
                tSRRecord_Talbe = findGenerallySRRecord_Table(SRRecord_List, SRReport_List);
                #endregion

                #region 取得【一般服務】案件產品序號資訊Html Table
                tSRSeiral_Table = findGenerallySRSeiral_Table(SRSeiral_List);
                #endregion

                #region 取得【一般服務】案件零件更換資訊Html Table
                tSRParts_Table = findGenerallySRParts_Table(SRParts_List);
                #endregion

                #region 取得【裝機服務】物料訊息資訊Html Table
                tSRMaterial_Table = findInstallSRMaterial_Table(SRMaterial_List);
                #endregion

                #region 取得【裝機服務】序號回報資訊Html Table
                tSRFeedBack_Table = findInstallSRFeedBack_Table(SRFeedBack_List, SRConfig_List);
                #endregion

                if (cSRID.Substring(0, 2) == "61") //一般
                {
                    tMailBody = GetMailBody("ONEGenerally_MAIL");
                }
                else if (cSRID.Substring(0, 2) == "63") //裝機
                {
                    tMailBody = GetMailBody("ONEInstall_MAIL");
                }
                else if (cSRID.Substring(0, 2) == "65") //定維
                {
                    tMailBody = GetMailBody("ONEMaintain_MAIL");
                }

                tMailBody = tMailBody.Replace("【<SRID>】", cSRID).Replace("【<SRCase>】", SRMain.SRCase).Replace("【<TeamNAME>】", SRMain.TeamNAME);
                tMailBody = tMailBody.Replace("【<TeamMGR>】", SRMain.TeamMGR).Replace("【<MainENG>】", SRMain.MainENG).Replace("【<AssENG>】", SRMain.AssENG);                
                tMailBody = tMailBody.Replace("【<TechMGR>】", SRMain.TechMGR).Replace("【<StatusDesc>】", SRMain.StatusDesc).Replace("【<CreatedUser>】", SRMain.CreateUser).Replace("【<CreatedDate>】", SRMain.CreatedDate);
                tMailBody = tMailBody.Replace("【<SalesNo>】", SRMain.SalesNo).Replace("【<ShipmentNo>】", SRMain.ShipmentNo).Replace("【<AttachementStockNoUrl>】", SRMain.CreatedDate);

                tMailBody = tMailBody.Replace("<ContractID>", tContractID).Replace("【<MAServiceType>】", SRMain.MAServiceType).Replace("<SecFix>", tSecFix);
                tMailBody = tMailBody.Replace("【<Desc>】", SRMain.Desc).Replace("【<Notes>】", SRMain.Notes).Replace("【<SalesName>】", SRMain.SalesEMP);                
                tMailBody = tMailBody.Replace("<SRRepair_List>", tSRRepair_Table).Replace("<SRContact_List>", tSRContact_Table);
                tMailBody = tMailBody.Replace("<SRSeiral_List>", tSRSeiral_Table).Replace("<SRParts_List>", tSRParts_Table);
                tMailBody = tMailBody.Replace("<SRMaterial_List>", tSRMaterial_Table).Replace("<SRFeedBack_List>", tSRFeedBack_Table);
                tMailBody = tMailBody.Replace("<SRRecord_List>", tSRRecord_Talbe);
                tMailBody = tMailBody.Replace("【<EndString>】", EndString);
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

        #region 發送服務案件Mail相關資訊(for客戶)
        /// <summary>
        /// 發送服務案件Mail相關資訊(for客戶)
        /// </summary>
        /// <param name="cCondition">服務案件執行條件(ADD.新建、TRANS.轉派主要工程師、REJECT.駁回、HPGCSN.HPGCSN申請、HPGCSNDONE.HPGCSN完成、SECFIX.二修、SAVE.保存、SUPPORT.技術支援升級、THRPARTY.3Party、CANCEL.取消、DONE.完修 DOA.維修/DOA INSTALLING.裝機中 INSTALLDONE.裝機完成 MAINTAINDONE.定保完成)</param>
        /// <param name="cSRID">SRID</param>        
        /// <param name="cLoginName">登入人員姓名</param>
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>
        /// <param name="SRMain">服務案件主檔資訊(For Mail)</param>
        /// <param name="SRRepair_List">服務案件客戶報修人資訊清單</param>   
        /// <param name="SRContact_List">服務案件客戶聯絡人資訊清單</param>
        /// <param name="SRSeiral_List">服務案件產品序號資訊清單</param>
        /// <param name="SRReport_List">服務報告書清單</param>
        public void SendSRMail_ToCustomer(SRCondition cCondition, string cSRID, string cLoginName, bool tIsFormal, SRIDMAININFO SRMain, 
                                         List<SRCONTACTINFO> SRRepair_List, List<SRCONTACTINFO> SRContact_List, 
                                         List<SRSERIALMATERIALINFO> SRSeiral_List, List<SRREPORTINFO> SRReport_List)
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

            string tCompanyName = string.Empty;     //公司名稱
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
                //先抓聯絡人Email
                foreach(var SRCon in SRContact_List)
                {
                    if (SRCon.CONTEMAIL != "")
                    {
                        tMailToTemp = SRCon.CONTEMAIL;
                        break;
                    }
                }

                //若沒有聯絡人Email，再抓報修人Email
                if (tMailToTemp == "")
                {
                    if (SRMain.RepairEmail != "")
                    {
                        tMailToTemp = SRMain.RepairEmail;
                    }
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
                if (SRMain.TeamMGREmail != "") //服務團隊主管Email
                {
                    tMailCcTemp += SRMain.TeamMGREmail;
                }

                if (SRMain.MainENGEmail != "") //主要工程師Email
                {
                    tMailCcTemp += SRMain.MainENGEmail;
                }

                if (SRMain.CreateUserEmail != "") //派單人員Email
                {
                    tMailCcTemp += SRMain.CreateUserEmail;
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
                tMailBCcTemp = "Jordan.Chang@etatung.com;Elvis.Chang@etatung.com"; //測試用，等都正常了就註解掉

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
                    tMailTo = "Elvis.Chang@etatung.com;Jordan.Chang@etatung.com;Cara.Tien@etatung.com";
                    //tMailCc = "Elvis.Chang@etatung.com";
                }
                #endregion              

                #region 郵件主旨
                string tMailSubject = findGenerallySRMailSubject_ToCustomer(cCondition, cSRID, SRMain.CusName, SRMain.SRPathWay, SRMain.CompanyName);

                tMailSubject = strTest + tMailSubject;
                #endregion

                #region 郵件內容
                string tMailBody = string.Empty;                

                #region 取得【一般服務】案件客戶報修窗口資訊Html Table
                tSRRepair_Table = findGenerallySRRepair_Table(SRRepair_List, SRMain.CusName);               
                #endregion

                #region 取得【一般服務】案件客戶聯絡窗口資訊Html Table
                tSRContact_Table = findGenerallySRContact_Table(SRContact_List);
                #endregion

                #region 取的產品序號資訊檔(基本上只會有一個序號，所以只要抓第一筆就行了)
                string MaterialName = "";
                string SerialID = "";
                string ProductNumber = "";

                foreach(var bean in SRSeiral_List)
                {
                    MaterialName = bean.PRDNAME;
                    SerialID = bean.SERIALID;
                    ProductNumber = bean.PRDNUMBER;
                    
                    break;
                }
                #endregion

                tHypeLink = @"https://0800.etatung.com/survey_oneservice.aspx?srId=" + cSRID;

                if (cCondition == SRCondition.ADD) //新建
                {
                    #region 內容格式參考(一般服務)新建
                    //親愛的客戶，您好
                    //我們已經收到您的報修需求，會盡速處理您的問題!
                    //以下是報修內容：
                    //報修時間： 2023-03-10 13:33
                    //報修單號： 61OOO
                    //機器明細： DL360pG8OOO_SGH1OOO_654081OOO
                    //問題描述： TEST 888

                    //客戶名稱：OOO股份有限公司
                    //[客戶報修窗口資料]												
                    //報修人	報修人電話	報修人手機	報修人地址	報修人Email								
                    //OOO	042OOO	09OOO	台北市OOO	TEST@OOO	

                    //若後續維修上有任何問題，請儘速與我們連絡 0800-066-038，謝謝!!                   
                    //-------此信件由系統管理員發出，請勿回覆此信件-------
                    #endregion

                    tMailBody = GetMailBody("ONECustomerRepair_MAIL");                    
                }
                else if (cCondition == SRCondition.DONE) //完修
                {
                    #region 內容格式參考(一般服務)完修
                    //親愛的客戶您好，
                    //您的服務已完修，明細如下：                    
                    //服務案件ID：8100190298
                    //產品：      430G7/i5-10210U/8G/500G/S256/acB/Cam/W10
                    //產品描述：  6YX14AV
                    //產品序號：  5CD0149HZP
                    //案件開始日期：2023/1/5
                    //案件結束日期：2023/1/6

                    //客戶名稱：OOO股份有限公司
                    //[客戶報修窗口資料]												
                    //報修人	報修人電話	報修人手機	報修人地址	報修人Email								
                    //OOO	042OOO	09OOO	台北市OOO	TEST@OOO	

                    //[客戶聯絡窗口資料]
                    //聯絡人	聯絡人電話	聯絡人手機	聯絡人地址	聯絡人Email								
                    //OOO	042OOO	09OOO	台北市OOO	TEST@OOO                    

                    //邀請您協助填寫滿意度調查，請點選下方網址，謝謝！
                    //滿意度網址

                    //若後續維修上有任何問題，請儘速與我們連絡 0800-066-038，謝謝!!                    
                    //-------此信件由系統管理員發出，請勿回覆此信件-------
                    #endregion

                    tMailBody = GetMailBody("ONECustomerFinished_MAIL");                    
                }

                tMailBody = tMailBody.Replace("【<CreatedDate>】", Convert.ToDateTime(SRMain.CreatedDate).ToString("yyyy-MM-dd")).Replace("【<FinishedDate>】", DateTime.Now.ToString("yyyy-MM-dd"));
                tMailBody = tMailBody.Replace("【<SRID>】", cSRID).Replace("【<MaterialName>】", MaterialName).Replace("【<SerialID>】", SerialID).Replace("【<ProductNumber>】", ProductNumber);
                tMailBody = tMailBody.Replace("【<Notes>】", SRMain.Notes);
                tMailBody = tMailBody.Replace("<SRRepair_List>", tSRRepair_Table).Replace("<SRContact_List>", tSRContact_Table);
                tMailBody = tMailBody.Replace("【<tHypeLink>】", tHypeLink);

                if (MaterialName == "" && SerialID == "" && ProductNumber == "") //若沒有機器明細，則不顯示
                {
                    tMailBody = tMailBody.Replace("</br>機器明細：__", "");
                }
                #endregion              

                if (tMailTo != "") //有收件者才要寄
                {
                    //呼叫寄送Mail
                    SendMailByAPI("SendSRMail_API", "Crmwebadmin@etatung.com", tMailTo, tMailCc, tMailBCc, tMailSubject, tMailBody, "", "");
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                writeToLog(cSRID, "SendSRMail_ToCustomer", pMsg, cLoginName);
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
            string CompanyName = string.Empty;

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

            CompanyName = srdetail["EV_CompanyName"].ToString();

            pdfFileName = CompanyName + "客戶服務報告書[" + srId + "].pdf";

            //一併發送給主要工程師及支援工程師
            List<string> ccs = new List<string>();

            #region 主要工程師
            if (srdetail["EV_MAINENGID"].ToString() != "")
            {
                string tEmail = findEMPEmail(srdetail["EV_MAINENGID"].ToString());

                if (tEmail != "" && !ccs.Contains(tEmail))
                {
                    ccs.Add(tEmail);
                }
            }
            #endregion

            #region 工時紀錄檔裡的人員
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
            #endregion

            if (email != "") //有收件者才要寄
            {
                SendReport(email, string.Join(";", ccs), srId, CUSTOMER, ENGINEER, NOTES, pdfPath, pdfFileName, mainEgnrName, tIsFormal); //發送服務報告書report給客戶
            }
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
            //    <br/>您此次的服務已完修，提供服務報告書如附件，謝謝!!<br/>" +
            //    <br/>[服務明細]" +
            //    <br/>服務ID：【<SRID>】" +
            //    <br/>客戶名稱：【<CUSTOMER>】" +
            //    <br/>負責工程師：【<ENGINEER>】" +
            //    <br/>需求事項：【<DESC>】" +
            //    <br/>狀態：已完修<br/>" +
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
                    receiver = "elvis.chang@etatung.com;Jordan.Chang@etatung.com;Cara.Tien@etatung.com";
                    //ccs = "elvis.chang@etatung.com;Jordan.Chang@etatung.com;Cara.Tien@etatung.com";
                }
                #endregion

                tMailSubject = strTest + "[大同世界科技 服務ID：" + SRID + " 已完修通知]";

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

        #endregion -----↑↑↑↑↑服務Mail相關 ↑↑↑↑↑-----  

        #region -----↓↓↓↓↓合約管理Mail相關 ↓↓↓↓↓-----

        #region 組合約主數據Mail相關資訊
        /// <summary>
        /// 組合約主數據Mail相關資訊
        /// </summary>        
        /// <param name="cCondition">合約主數據執行條件(ADD.新建、SAVE.保存)</param>
        /// <param name="cOperationID_Contract">程式作業編號檔系統ID(合約主數據查詢/維護)</param>  
        /// <param name="cContractID">文件編號</param>
        /// <param name="cLog">LOG記錄</param>
        /// <param name="tONEURLName">One Service站台名稱</param>
        /// <param name="cLoginName">登入人員姓名</param>
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>
        public void SetContractMailContent(ContractCondition cCondition, string cOperationID_Contract, string cContractID, string cLog, string tONEURLName, string cLoginName, bool tIsFormal)
        {
            string tMailToTemp = string.Empty;
            string tMailCcTemp = string.Empty;
            string tMailBCcTemp = string.Empty;

            string tMailTo = string.Empty;                  //收件者            
            string tMailCc = string.Empty;                  //副本            
            string tMailBCc = string.Empty;                 //密件副本
            string tHypeLink = string.Empty;                //超連結
            string tSeverName = string.Empty;               //主機名稱
            string tEngineerID = string.Empty;              //工程師ERPID

            string cTeamName = string.Empty;                //服務團隊
            string cTeamMGR = string.Empty;                 //服務團隊主管
            string cTeamMGREmail = string.Empty;            //服務團隊主管Email
            string cMainENG = string.Empty;                 //主要工程師
            string cMainENGEmail = string.Empty;            //主要工程師Email
            string cAssENG = string.Empty;                  //協助工程師
            string cAssENGEmail = string.Empty;             //協助工程師Email
            string cTechMGR = string.Empty;                 //技術主管
            string cTechMGREmail = string.Empty;            //技術主管Email
            string cSalesEMP = string.Empty;                //業務人員
            string cSalesEMPEmail = string.Empty;           //業務人員Email            
            string cSecretaryEMP = string.Empty;            //業務祕書
            string cSecretaryEMPEmail = string.Empty;       //業務祕書Email
            string cMASalesEMP = string.Empty;              //維護業務人員
            string cMASalesEMPEmail = string.Empty;         //維護業務人員Email                                                           

            string cSoNo = string.Empty;                    //銷售訂單號
            string cCustomerID = string.Empty;              //客戶ID
            string cCustomerName = string.Empty;            //客戶名稱        
            string cDesc = string.Empty;                    //訂單說明
            string cStartDate = string.Empty;               //維護日期(起)
            string cEndDate = string.Empty;                 //維護日期(迄)
            string cMACycle = string.Empty;                 //維護週期        
            string cMANotes = string.Empty;                 //維護備註        
            string cMAAddress = string.Empty;               //維護地址        
            string cContractNotes = string.Empty;           //合約備註          
            string cBillNotes = string.Empty;               //請款備註
            string cModifiedDate = string.Empty;            //異動日期

            try
            {
                List<SRTEAMORGINFO> Team_List = new List<SRTEAMORGINFO>();
                List<SREMPINFO> MainENG_List = new List<SREMPINFO>();
                List<SREMPINFO> AssENG_List = new List<SREMPINFO>();                
                List<SREMPINFO> SalesEMP_List = new List<SREMPINFO>();
                List<SREMPINFO> SecretaryEMP_List = new List<SREMPINFO>();
                List<SREMPINFO> MASalesEMP_List = new List<SREMPINFO>();               

                var beanM = dbOne.TB_ONE_ContractMain.FirstOrDefault(x => x.Disabled == 0 && x.cContractID == cContractID);

                if (beanM != null)
                {
                    #region -----↓↓↓↓↓主檔 ↓↓↓↓↓-----
                    CONTRACTMAININFO ContractMain = new CONTRACTMAININFO();

                    #region 服務團隊相關
                    Team_List = findSRTEAMORGINFO(beanM.cTeamID);                    
                    cTeamName = findSRTeamName(Team_List);
                    cTeamMGR = findSRTeamMGRName(Team_List);
                    cTeamMGREmail = findSRTeamMGREmail(Team_List);
                    #endregion

                    #region 主要工程師/協助工程師相關
                    tEngineerID = findContractDetailENG(cContractID, "Y");
                    if (tEngineerID != "")
                    {
                        MainENG_List = findSREMPINFO(tEngineerID);
                        cMainENG = findSREMPName(MainENG_List);
                        cMainENGEmail = findSREMPEmail(MainENG_List);
                    }

                    tEngineerID = findContractDetailENG(cContractID, "N");
                    if (tEngineerID != "")
                    {
                        AssENG_List = findSREMPINFO(tEngineerID);
                        cAssENG = findSREMPName(AssENG_List);
                        cAssENGEmail = findSREMPEmail(AssENG_List);
                    }
                    #endregion

                    #region 業務人員/業務祕書/維護業務相關
                    SalesEMP_List = findSREMPINFO(beanM.cSoSales);
                    cSalesEMP = findSREMPName(SalesEMP_List);
                    cSalesEMPEmail = findSREMPEmail(SalesEMP_List);

                    SecretaryEMP_List = findSREMPINFO(beanM.cSoSalesASS);
                    cSecretaryEMP = findSREMPName(SecretaryEMP_List);
                    cSecretaryEMPEmail = findSREMPEmail(SecretaryEMP_List);

                    MASalesEMP_List = findSREMPINFO(beanM.cMASales);
                    cMASalesEMP = findSREMPName(SalesEMP_List);
                    cMASalesEMPEmail = findSREMPEmail(SalesEMP_List);
                    #endregion                   

                    #region 其他主檔相關  
                    cSoNo = beanM.cSoNo;
                    cCustomerID = beanM.cCustomerID;
                    cCustomerName = beanM.cCustomerName;
                    cDesc = beanM.cDesc;
                    cStartDate = Convert.ToDateTime(beanM.cStartDate).ToString("yyyy-MM-dd");
                    cEndDate = Convert.ToDateTime(beanM.cEndDate).ToString("yyyy-MM-dd");
                    cMACycle = beanM.cMACycle;
                    cMANotes = beanM.cMANotes;
                    cMAAddress = beanM.cMAAddress;
                    cContractNotes = beanM.cContractNotes;
                    cBillNotes = beanM.cBillNotes;
                    cModifiedDate = beanM.ModifiedDate == null ? "" : Convert.ToDateTime(beanM.ModifiedDate).ToString("yyyy-MM-dd");
                    #endregion

                    ContractMain.ContractID = cContractID;                 
                    ContractMain.TeamNAME = cTeamName;
                    ContractMain.TeamMGR = cTeamMGR;
                    ContractMain.MainENG = cMainENG;
                    ContractMain.AssENG = cAssENG;                    
                    ContractMain.SalesEMP = cSalesEMP;
                    ContractMain.SecretaryEMP = cSecretaryEMP;
                    ContractMain.MASalesEMP = cMASalesEMP;                    
                    ContractMain.ModifiedDate = cModifiedDate;

                    ContractMain.SoNo = cSoNo;
                    ContractMain.CustomerID = cCustomerID;
                    ContractMain.CustomerName = cCustomerName;
                    ContractMain.Desc = cDesc;                    
                    ContractMain.StartDate = cStartDate;
                    ContractMain.EndDate = cEndDate;
                    ContractMain.MACycle = cMACycle;
                    ContractMain.MANotes = cMANotes;
                    ContractMain.MAAddress = cMAAddress;
                    ContractMain.ContractNotes = cContractNotes;
                    ContractMain.BillNotes = cBillNotes;

                    ContractMain.TeamMGREmail = cTeamMGREmail;
                    ContractMain.MainENGEmail = cMainENGEmail;
                    ContractMain.AssENGEmail = cAssENGEmail;                    
                    ContractMain.SalesEmail = cSalesEMPEmail;
                    ContractMain.SecretaryEmail = cSecretaryEMPEmail;
                    ContractMain.MASalesEmail = cMASalesEMPEmail;
                    #endregion -----↑↑↑↑↑Mail相關 ↑↑↑↑↑-----                  

                    #region 發送合約主數據Mail相關資訊  
                    SendContractMail(cCondition, cContractID, cLog, tONEURLName, cLoginName, tIsFormal, ContractMain);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                writeToLog(cContractID, "SetContractMailContent", pMsg, cLoginName);
            }
        }
        #endregion

        #region 發送合約主數據Mail相關資訊
        /// <summary>
        /// 發送合約主數據Mail相關資訊
        /// </summary>        
        /// <param name="cCondition">合約主數據執行條件(ADD.新建、SAVE.保存)</param>
        /// <param name="cContractID">文件編號</param>
        /// <param name="cLog">LOG記錄</param>
        /// <param name="tONEURLName">One Service站台名稱</param>
        /// <param name="cLoginName">登入人員姓名</param>
        /// <param name="tIsFormal">是否為正式區(true.是 false.不是)</param>        
        /// <param name="ContractMain">合約主數據資訊(For Mail)</param>
        public void SendContractMail(ContractCondition cCondition, string cContractID, string cLog, string tONEURLName, string cLoginName, bool tIsFormal, CONTRACTMAININFO ContractMain)
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

            try
            {
                #region 取得收件者 
                if (ContractMain.TeamMGREmail != "") //有服務團隊主管
                {
                    tMailToTemp += ContractMain.TeamMGREmail;
                }

                if (ContractMain.MainENGEmail != "") //有指派主要工程師
                {
                    tMailToTemp += ContractMain.MainENGEmail;
                }

                if (ContractMain.AssENGEmail != "") //有指派協助工程師
                {
                    tMailToTemp += ContractMain.AssENGEmail;
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
                if (ContractMain.SalesEmail != "") //業務人員
                {
                    tMailCcTemp += ContractMain.SalesEmail;
                }

                if (ContractMain.SecretaryEmail != "") //業務祕書
                {
                    tMailCcTemp += ContractMain.SecretaryEmail;
                }

                if (ContractMain.MASalesEmail != "") //維護業務
                {
                    tMailCcTemp += ContractMain.MASalesEmail;
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
                tMailBCcTemp = "Jordan.Chang@etatung.com;Elvis.Chang@etatung.com"; //測試用，等都正常了就註解掉

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
                    //tMailTo = "Elvis.Chang@etatung.com";
                    tMailCc += ";Elvis.Chang@etatung.com;Jordan.Chang@etatung.com;Cara.Tien@etatung.com";
                }
                #endregion

                #region 郵件主旨
                string tMailSubject = findContractMailSubject(cCondition, cContractID, ContractMain.Desc);

                tMailSubject = strTest + tMailSubject;
                #endregion

                #region 郵件內容

                #region 內容格式參考(合約主數據)                

                #region 新建的內容
                //主管您好，有一份新的合約在CRM中建立，請您上線指派主要工程師：

                //[合約主數據內容]
                //客戶名稱：D86383738/台灣恩悌悌系統股份有限公司
                //合約文件編號：11204075
                //合約備註：原11104100續約；下包商:力麗、零壹  ※此筆含電腦、網路維護
                //請款備註：
                //訂單號碼：
                //訂單/合約說明：FY23NTT_大廣國際機房維護
                //合約開始日期：2023-04-01
                //合約結束日期：2024-03-31
                //維護週期：202306_202309_202312_202403
                //維護備註：1. 維護期間:2023/04/01-2024/3/31  2. SLA:724
                //維護地址：台灣恩悌悌NTT for 大廣國際
                //業務：謝旻樺 Fion.Hsieh
                //維護業務：謝旻樺 Fion.Hsieh

                //請儘速至One Sevice 合約管理系統處理，謝謝！
                //查看合約主數據內容 =>超連結(http://172.31.7.56:32200/Contract/ContractMain?ContractID=11204075)
                //提醒您：此為系統發送信函請勿直接回覆此信。                

                //-------此信件由系統管理員發出，請勿回覆此信件-------
                #endregion

                #region 保存的內容
                //親愛的主管/同仁您好,

                //[合約主數據內容]
                //客戶名稱：D86383738/台灣恩悌悌系統股份有限公司
                //合約文件編號：11204075
                //合約備註：原11104100續約；下包商:力麗、零壹  ※此筆含電腦、網路維護
                //請款備註：
                //訂單號碼：
                //訂單/合約說明：FY23NTT_大廣國際機房維護
                //合約開始日期：2023-04-01
                //合約結束日期：2024-03-31
                //維護週期：202306_202309_202312_202403
                //維護備註：1. 維護期間:2023/04/01-2024/3/31  2. SLA:724
                //維護地址：台灣恩悌悌NTT for 大廣國際
                //業務：謝旻樺 Fion.Hsieh
                //維護業務：謝旻樺 Fion.Hsieh
                //異動人員：張豐穎 Elvis.Chang
                //異動時間：2023-06-14
                //異動記錄：地點_舊值【 台北市敦化南路二段65-67號10樓】 新值【 台北市敦化南路二段65-67號10樓之1】 

                //請儘速至One Sevice 合約管理系統處理，謝謝！
                //查看合約主數據內容 =>超連結(http://172.31.7.56:32200/Contract/ContractMain?ContractID=11204075)
                //提醒您：此為系統發送信函請勿直接回覆此信。                

                //-------此信件由系統管理員發出，請勿回覆此信件-------
                #endregion

                #endregion

                tHypeLink = tONEURLName + "/Contract/ContractMain?ContractID=" + cContractID;

                string tMailBody = GetMailBody("ONEContract_MAIL");
                string Title = cCondition == ContractCondition.ADD ? "主管您好，有一份新的合約在CRM中建立，請您上線指派主要工程師：" : "親愛的主管/同仁您好,";
                string CustomerName = ContractMain.CustomerID + "/" + ContractMain.CustomerName;
                string Desc = ContractMain.Desc;
                string MANotes = ContractMain.MANotes;
                string ModifiedUserName = cCondition == ContractCondition.ADD ? "" : "異動人員：" + cLoginName + "</br>";
                string ModifiedDate = cCondition == ContractCondition.ADD ? "" : "異動時間：" + ContractMain.ModifiedDate + "</br>";
                string Record = cCondition == ContractCondition.ADD ? "" : "異動記錄：" + cLog;

                tMailBody = tMailBody.Replace("【<Title>】", Title).Replace("【<CustomerName>】", CustomerName).Replace("【<ContractID>】", ContractMain.ContractID);
                tMailBody = tMailBody.Replace("【<ContractNotes>】", ContractMain.ContractNotes).Replace("【<BillNotes>】", ContractMain.BillNotes);
                tMailBody = tMailBody.Replace("【<SoNo>】", ContractMain.SoNo).Replace("【<Desc>】", Desc);
                tMailBody = tMailBody.Replace("【<StartDate>】", ContractMain.StartDate).Replace("【<EndDate>】", ContractMain.EndDate);
                tMailBody = tMailBody.Replace("【<MACycle>】", ContractMain.MACycle).Replace("【<MANotes>】", MANotes);
                tMailBody = tMailBody.Replace("【<MAAddress>】", ContractMain.MAAddress).Replace("【<SalesEMP>】", ContractMain.SalesEMP);
                tMailBody = tMailBody.Replace("【<MASalesEMP>】", ContractMain.MASalesEMP).Replace("【<ModifiedDate>】", ModifiedDate);
                tMailBody = tMailBody.Replace("【<ModifiedUserName>】", ModifiedUserName).Replace("【<Record>】", Record).Replace("【<tHypeLink>】", tHypeLink);                
                #endregion               

                //呼叫寄送Mail
                SendMailByAPI("SendSRMail_API", null, tMailTo, tMailCc, tMailBCc, tMailSubject, tMailBody, "", "");
            }
            catch (Exception ex)
            {
                pMsg += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "失敗原因:" + ex.Message + Environment.NewLine;
                pMsg += " 失敗行數：" + ex.ToString();

                writeToLog(cContractID, "SendContractMail", pMsg, cLoginName);
            }
        }
        #endregion

        #region 取得【共用】合約主數據的郵件主旨
        /// <summary>
        /// 取得【共用】合約主數據的郵件主旨
        /// </summary>
        /// <param name="cCondition">合約主據數執行條件(ADD.新建、SAVE.保存)</param>
        /// <param name="cContractID">文件編號</param>
        /// <param name="cDesc">訂單說明</param>        
        /// <returns></returns>
        public string findContractMailSubject(ContractCondition cCondition, string cContractID, string cDesc)
        {
            string reValue = string.Empty;

            switch (cCondition)
            {
                case ContractCondition.ADD:                    
                    //[One Service]合約建檔完成[<文件編號>_<訂單說明>]
                    reValue = "[One Service] 合約建檔完成 [" + cContractID + "_" + cDesc + "]";
                    break;            

                case ContractCondition.SAVE:
                    //[One Service]合約主數據異動通知[<文件編號>_<訂單說明>]
                    reValue = "[One Service] 合約主數據異動通知 [" + cContractID + "_" + cDesc + "]";
                    break;               
            }

            return reValue;
        }
        #endregion

        #region 取得【用印或內部轉撥服務】BPM表單相關資料
        /// <summary>
        /// 取得【用印或內部轉撥服務】BPM表單相關資料
        /// </summary>
        /// <param name="BPMNO">表單編號</param>
        /// <returns></returns>
        public BPMCONTRACTINFO findBPMCONTRACTINFO(string BPMNO)
        {
            BPMCONTRACTINFO beanM = new BPMCONTRACTINFO();

            bool tIsA2Form = false; //判斷是否為用印申請單

            if (BPMNO.Substring(0,2) == "A2")
            {
                tIsA2Form = true;
            }
            
            if (tIsA2Form)
            {
                #region 用印申請單
                var bean = dbBPM.tblForm_ContractSeals.FirstOrDefault(x => x.cSystem_FormNo == BPMNO);

                if (bean != null)
                {
                    beanM.IV_CONTACT = bean.cContent_ContractID;
                    beanM.IV_SUBCONTACT = "";
                    beanM.IV_SONO = "";                    
                    beanM.IV_SALES = findEmployeeByERP_ID(bean.cApplyUser_EmployeeNO);
                    beanM.IV_ASSITANCE = bean.cContent_Secretary;

                    beanM.IV_ContractVendor = bean.cContent_ContractVendor.ToString();

                    if (beanM.IV_ContractVendor == "0") //客戶
                    {
                        beanM.IV_CUSTOMER = bean.cContent_ContractUserID;
                    }
                    else
                    {
                        beanM.IV_CUSTOMER = "";
                    }

                    if (beanM.IV_ContractVendor == "1") //供應商
                    {
                        beanM.IV_SODESC = bean.cContent_ContractUserID + "_" + bean.cContent_ContractDesc;
                        beanM.IV_SUBNUMBER = bean.cContent_ContractUserID;
                        beanM.IV_ContractUser = bean.cContent_ContractUser;
                    }
                    else
                    {
                        beanM.IV_SODESC = bean.cContent_ContractDesc;
                    }

                    beanM.IV_SDATE = Convert.ToDateTime(bean.cContent_ContractStrDate);
                    beanM.IV_EDATE = Convert.ToDateTime(bean.cContent_ContractEndDate);
                    beanM.IV_REQPAY = bean.cContent_ContractInfo_MakeMoney;
                    beanM.IV_CYCLE = bean.cContent_ContractInfo_Maintain;
                    beanM.IV_NOTES = bean.cContent_ContractInfo_Note;
                    beanM.IV_ADDR = bean.cContent_ContractInfo_Address;
                    beanM.IV_SLASRV = bean.cContent_ContractInfo_Service;
                    beanM.IV_SLARESP = bean.cContent_ContractInfo_Responses;
                    beanM.IV_NOTE = bean.cContent_ContractInfo_Memo;
                    beanM.IV_ORGID = bean.cContent_ContractInfo_Team;
                    beanM.IV_MAINID = bean.cContent_ContractInfo_MainNo;
                    beanM.IV_PAYNOTE = bean.cContent_ContractInfo_PayNote;
                    beanM.IV_MAINTAIN_SALES = bean.cContent_MaintainSales;
                    beanM.IV_ContactName = "";
                    beanM.IV_ContactEmail = "";
                }
                #endregion
            }
            else
            {
                #region 內部轉撥服務單
                var bean = dbBPM.tblForm_SubContractSeals.FirstOrDefault(x => x.cSystem_FormNo == BPMNO);

                if (bean != null)
                {
                    beanM.IV_ContractVendor = bean.cContent_ContractVendor.ToString();

                    if (beanM.IV_ContractVendor == "0") //客戶
                    {
                        beanM.IV_CUSTOMER = bean.cContent_ContractUserID;
                        beanM.IV_SODESC = bean.cContent_ContractDesc;                        
                        beanM.IV_SALES = findEmployeeInCludeLeaveEByERP_ID(bean.cContent_MainUser_EmployeeNO); //主約業務
                        
                    }
                    else if (beanM.IV_ContractVendor == "1") //供應商
                    {
                        beanM.IV_CUSTOMER = "";
                        beanM.IV_SODESC = bean.cContent_ContractUserID + "_" + bean.cContent_ContractDesc;
                        beanM.IV_SUBNUMBER = bean.cContent_ContractUserID;
                        beanM.IV_ContractUser = bean.cContent_ContractUser;
                        beanM.IV_SALES = findEmployeeByERP_ID(bean.cApplyUser_EmployeeNO); //維護業務
                    }
                    else //其他
                    {
                        beanM.IV_CUSTOMER = "";
                        beanM.IV_SODESC = bean.cContent_ContractDesc;
                        beanM.IV_SALES = findEmployeeByERP_ID(bean.cApplyUser_EmployeeNO); //維護業務
                    }

                    beanM.IV_CONTACT = bean.cContent_ContractID;
                    beanM.IV_SUBCONTACT = bean.cContent_ContractInfo_MainNo;
                    beanM.IV_SONO = "";                    
                    beanM.IV_ASSITANCE = bean.cContent_Secretary;
                    beanM.IV_SDATE = Convert.ToDateTime(bean.cContent_ContractStrDate);
                    beanM.IV_EDATE = Convert.ToDateTime(bean.cContent_ContractEndDate);
                    beanM.IV_REQPAY = bean.cContent_ContractInfo_MakeMoney;
                    beanM.IV_CYCLE = bean.cContent_ContractInfo_Maintain;
                    beanM.IV_NOTES = bean.cContent_ContractInfo_Note;
                    beanM.IV_ADDR = bean.cContent_ContractInfo_Address;
                    beanM.IV_SLASRV = bean.cContent_ContractInfo_Service;
                    beanM.IV_SLARESP = bean.cContent_ContractInfo_Responses;
                    beanM.IV_NOTE = bean.cContent_ContractInfo_Memo;
                    beanM.IV_ORGID = bean.cContent_ContractInfo_Team;
                    beanM.IV_MAINID = bean.cContent_ContractInfo_MainNo;
                    beanM.IV_PAYNOTE = bean.cContent_ContractInfo_PayNote;
                    beanM.IV_MAINTAIN_SALES = bean.cContent_MaintainSales;
                    beanM.IV_ContactName = "";
                    beanM.IV_ContactEmail = "";
                }
                #endregion
            }

            return beanM;
        }
        #endregion

        #region 傳入員工帳號，取得ERP_ID
        /// <summary>
        /// 傳入員工帳號，取得ERP_ID
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public string findEmployeeByERP_ID(string keyword)
        {
            tblEmployee tEmployee = dbBPM.tblEmployee.FirstOrDefault(x => x.cEmployee_NO == keyword &&
                                                                       x.cEmployee_LeaveReason == null &&
                                                                       x.cEmployee_LeaveDay == null);

            return tEmployee.cEmployee_ERPID;
        }
        #endregion

        #region 傳入員工帳號(含離職人員)，取得ERP_ID
        /// <summary>
        /// 傳入員工帳號(含離職人員)，取得ERP_ID
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public string findEmployeeInCludeLeaveEByERP_ID(string keyword)
        {
            tblEmployee tEmployee = dbBPM.tblEmployee.OrderByDescending(x => x.pk).FirstOrDefault(x => x.cEmployee_NO == keyword);

            return tEmployee.cEmployee_ERPID;
        }
        #endregion

        #endregion -----↑↑↑↑↑合約管理Mail相關 ↑↑↑↑↑-----  

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