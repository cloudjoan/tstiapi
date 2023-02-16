using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;
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

        public CommonFunction()
        {

        }

        #region 取得登入者資訊
        public EmployeeBean findEmployeeInfo(string keyword)
        {
            EmployeeBean empBean = new EmployeeBean();

            var beanE = dbEIP.Person.FirstOrDefault(x => x.Account.ToLower() == keyword.ToLower() && (x.Leave_Date == null && x.Leave_Reason == null));

            if (beanE != null)
            {
                empBean.EmployeeNO = beanE.Account.Trim();
                empBean.EmployeeERPID = beanE.ERP_ID.Trim();
                empBean.EmployeeCName = beanE.Name2.Trim();
                empBean.EmployeeEName = beanE.Name.Trim();
                empBean.WorkPlace = beanE.Work_Place.Trim();
                empBean.PhoneExt = beanE.Extension.Trim();
                empBean.CompanyCode = beanE.Comp_Cde.Trim();
                empBean.EmployeeEmail = beanE.Email.Trim();
                empBean.EmployeePersonID = beanE.ID.Trim();

                var beanD = dbEIP.Department.FirstOrDefault(x => x.ID == beanE.DeptID);

                if (beanD != null)
                {
                    empBean.DepartmentNO = beanD.ID.Trim();
                    empBean.DepartmentName = beanD.Name2.Trim();
                }
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
        public List<Person> findEMPLOYEEINFO(string keyword)
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

        #region 取得人員中文+英文姓名
/// <summary>
/// 取得人員中文+英文姓名
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
        /// <returns></returns>
        public List<SRWarranty> ZFM_TICC_SERIAL_SEARCHWTYList(string[] ArySERIAL, string tURLName, string tSeverName)
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
    }
}