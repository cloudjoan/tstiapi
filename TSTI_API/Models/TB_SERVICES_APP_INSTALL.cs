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
    
    public partial class TB_SERVICES_APP_INSTALL
    {
        public int ID { get; set; }
        public string SRID { get; set; }
        public string ACCOUNT { get; set; }
        public string ERP_ID { get; set; }
        public string EMP_NAME { get; set; }
        public string InstallDate { get; set; }
        public string ExpectedDate { get; set; }
        public Nullable<decimal> TotalQuantity { get; set; }
        public Nullable<decimal> InstallQuantity { get; set; }
        public string INSERT_TIME { get; set; }
        public string UPDATE_ACCOUNT { get; set; }
        public string UPDATE_EMP_NAME { get; set; }
        public string UPDATE_TIME { get; set; }
    }
}
