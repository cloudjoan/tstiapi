using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        #region 取得法人客戶聯絡人資料
        /// <summary>
        /// 取得法人客戶聯絡人資料
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
        /// <param name="CONTACTEMAIL">聯絡人Email</param>
        /// <returns></returns>
        public List<PCustomerContact> findCONTACTINFO(string CustomerID, string CONTACTNAME,  string CONTACTTEL, string CONTACTEMAIL)
        {
            var qPjRec = dbProxy.CUSTOMER_Contact.OrderByDescending(x => x.ModifiedDate).
                                               Where(x => (x.Disabled == null || x.Disabled != 1) && x.KNA1_KUNNR == CustomerID &&
                                                          x.ContactName != "" && x.ContactCity != "" &&
                                                          x.ContactAddress != "" && x.ContactPhone != "" &&
                                                          (string.IsNullOrEmpty(CONTACTNAME) ? true : x.ContactName.Contains(CONTACTNAME)) &&
                                                          (string.IsNullOrEmpty(CONTACTTEL) ? true : x.ContactPhone.Contains(CONTACTTEL)) &&
                                                          (string.IsNullOrEmpty(CONTACTEMAIL) ? true : x.ContactEmail.Contains(CONTACTEMAIL))).ToList();

            List<string> tTempList = new List<string>();

            string tTempValue = string.Empty;

            List<PCustomerContact> liPCContact = new List<PCustomerContact>();
            if (qPjRec != null && qPjRec.Count() > 0)
            {
                foreach (var prBean in qPjRec)
                {
                    tTempValue = prBean.KNA1_KUNNR.Trim().Replace(" ", "") + "|" + prBean.KNB1_BUKRS.Trim().Replace(" ", "") + "|" + prBean.ContactName.Trim().Replace(" ", "");

                    if (!tTempList.Contains(tTempValue)) //判斷客戶ID、公司別、聯絡人名姓名不重覆才要顯示
                    {
                        tTempList.Add(tTempValue);

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
                        prDocBean.BPMNo = prBean.BpmNo.Trim().Replace(" ", "");

                        liPCContact.Add(prDocBean);
                    }
                }
            }

            return liPCContact;
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
        /// <param name="NowCount">目前的項次</param>
        /// <param name="tURLName">BPM站台名稱</param>
        /// <param name="tSeverName">PSIP站台名稱</param>
        /// <returns></returns>
        public List<SRWarranty> ZFM_TICC_SERIAL_SEARCHWTYList(string[] ArySERIAL, ref int NowCount, string tURLName, string tSeverName)
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
                            NowCount++;

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

                            #region 取得BPM Url
                            if (cContractID != "")
                            {
                                tBPMNO = findContractSealsFormNo(cContractID);

                                try
                                {
                                    Int32 ContractID = Int32.Parse(cContractID);

                                    if (ContractID >= 10506151 && ContractID != 10506152 && ContractID != 10506157) //新的用印申請單
                                    {
                                        tURL = "http://" + tURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/ContractSeals/ContractSealsForm.aspx?FormNo=" + tBPMNO + " target=_blank";
                                    }
                                    else //舊的用印申請單
                                    {
                                        tURL = "http://" + tURLName + "/ContractSeals/_layouts/FormServer.aspx?XmlLocation=%2fContractSeals%2fBPMContractSealsForm%2f" + tBPMNO + ".xml&ClientInstalled=true&DefaultItemOpen=1&source=/_layouts/TSTI.SharePoint.BPM/CloseWindow.aspx target=_blank";
                                    }

                                    cContractIDURL = "http://" + tSeverName + "/Spare/QueryContractInfo?CONTRACTID=" + cContractID + " target=_blank"; //合約編號URL
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
                                    tURL = "http://" + tURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Rwd/Warranty/WarrantyForm.aspx?FormNo=" + tBPMNO + " target=_blank";
                                }
                                else
                                {
                                    tURL = "http://" + tURLName + "/sites/bpm/_layouts/Taif/BPM/Page/Form/Guarantee/GuaranteeForm.aspx?FormNo=" + tBPMNO + " target=_blank";
                                }
                            }
                            #endregion

                            #region 取得清單
                            SRWarranty QueryInfo = new SRWarranty();

                            QueryInfo.cID = NowCount.ToString();        //系統ID
                            QueryInfo.cSerialID = IV_SERIAL;            //序號                        
                            QueryInfo.cWTYID = cWTYID;                  //保固
                            QueryInfo.cWTYName = cWTYName;              //保固說明
                            QueryInfo.cWTYSDATE = cWTYSDATE;            //保固開始日期
                            QueryInfo.cWTYEDATE = cWTYEDATE;            //保固結束日期                                                          
                            QueryInfo.cSLARESP = cSLARESP;              //回應條件
                            QueryInfo.cSLASRV = cSLASRV;                //服務條件
                            QueryInfo.cContractID = cContractID;        //合約編號
                            QueryInfo.cContractIDUrl = cContractIDURL;  //合約編號Url
                            QueryInfo.cBPMFormNo = tBPMNO;              //BPM表單編號                        
                            QueryInfo.cBPMFormNoUrl = tURL;             //BPM URL                    
                            QueryInfo.cUsed = "N";
                            QueryInfo.cBGColor = tBGColor;             //tr背景顏色Class

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