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
    
    public partial class TB_APP_PUSH_NOTICE_MESSAGES
    {
        public int ID { get; set; }
        public string APP_ID { get; set; }
        public string MSG_TYPE { get; set; }
        public string MSG_TARGET { get; set; }
        public string MSG_TARGET_NAME { get; set; }
        public string MSG_TITLE { get; set; }
        public string MSG_CONTENT { get; set; }
        public string IMG_URL { get; set; }
        public string MSG_LINK { get; set; }
        public string MSG_LEVEL { get; set; }
        public string INSERT_TIME { get; set; }
        public string START_TIME { get; set; }
        public string END_TIME { get; set; }
        public string DISABLED { get; set; }
    }
}