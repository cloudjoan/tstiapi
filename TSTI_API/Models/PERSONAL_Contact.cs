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
    
    public partial class PERSONAL_Contact
    {
        public System.Guid ContactID { get; set; }
        public string KNA1_KUNNR { get; set; }
        public string KNA1_NAME1 { get; set; }
        public string KNB1_BUKRS { get; set; }
        public string ContactName { get; set; }
        public string ContactCity { get; set; }
        public string ContactAddress { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactMobile { get; set; }
        public Nullable<int> Disabled { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedUserName { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedUserName { get; set; }
    }
}
