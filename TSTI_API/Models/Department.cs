//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace TSTI_API.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Department
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public Nullable<int> DisplayOrder { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string FullName { get; set; }
        public string FullName2 { get; set; }
        public string LocationID { get; set; }
        public bool IsCostCenter { get; set; }
        public bool IsVirtual { get; set; }
        public string EMail { get; set; }
        public string ManagerID { get; set; }
        public string DeptCode1 { get; set; }
        public string DeptCode2 { get; set; }
        public string DeptCode3 { get; set; }
        public string DeptCode4 { get; set; }
        public string DeptCode5 { get; set; }
        public string PrintNum { get; set; }
        public int Level { get; set; }
        public Nullable<bool> VisitStore { get; set; }
        public string VisitStoreUnit { get; set; }
        public int Status { get; set; }
        public string JDE_Dept_No { get; set; }
        public string JDE_Dept_Nm { get; set; }
        public string CR_USER { get; set; }
        public Nullable<System.DateTime> CR_DATE { get; set; }
        public string USERSTAMP { get; set; }
        public Nullable<System.DateTime> DATESTAMP { get; set; }
        public bool IsBusinessUnit { get; set; }
        public string ProfitCenterID { get; set; }
        public string CostCenterID { get; set; }
        public string Comp_Cde { get; set; }
    }
}
