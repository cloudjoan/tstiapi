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
    
    public partial class TB_ONE_ContractDetail_SUB
    {
        public int cID { get; set; }
        public string cContractID { get; set; }
        public string cSubContractID { get; set; }
        public string cSubSupplierID { get; set; }
        public string cSubSupplierName { get; set; }
        public string cSubNotes { get; set; }
        public Nullable<int> Disabled { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedUserName { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedUserName { get; set; }
    }
}