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
    
    public partial class VIEW_BULLETINForEip
    {
        public int bulletinID { get; set; }
        public int bulletinTypeID { get; set; }
        public int bulletinItem { get; set; }
        public string bulletinTarget { get; set; }
        public string bulletinTargetName { get; set; }
        public string bulletinSubject { get; set; }
        public string bulletinContent { get; set; }
        public Nullable<int> templateID { get; set; }
        public System.DateTime startDate { get; set; }
        public System.DateTime endDate { get; set; }
        public string currentType { get; set; }
        public string status { get; set; }
        public System.DateTime createTime { get; set; }
        public string createUserAccount { get; set; }
        public string createUserID { get; set; }
        public string createUserName { get; set; }
        public string rejectReason { get; set; }
        public string approveUser { get; set; }
        public string approveAgentUser { get; set; }
        public bool cancelMark { get; set; }
        public string modifyUser { get; set; }
        public Nullable<System.DateTime> modifyTime { get; set; }
        public Nullable<System.DateTime> cancelTime { get; set; }
        public string createUserMail { get; set; }
        public string approveUserMail { get; set; }
        public string approveAgentUserMail { get; set; }
        public string cancelUser { get; set; }
        public string Attachment { get; set; }
        public Nullable<int> bulletinParentTypeID { get; set; }
        public string bulletinParentTypeName { get; set; }
        public string bulletinUnitName { get; set; }
        public string bulletinUnitCode { get; set; }
        public bool MailMark { get; set; }
        public string MailTarget { get; set; }
        public string MailTargetName { get; set; }
        public Nullable<int> FN_No { get; set; }
        public string FN_Name { get; set; }
        public string Dept { get; set; }
        public string ErpID { get; set; }
        public string MailDept { get; set; }
        public string MailErpID { get; set; }
    }
}
