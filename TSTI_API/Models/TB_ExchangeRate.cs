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
    
    public partial class TB_ExchangeRate
    {
        public int ID { get; set; }
        public string Currency { get; set; }
        public string CR_Date { get; set; }
        public Nullable<decimal> Buy { get; set; }
        public Nullable<decimal> Sell { get; set; }
        public Nullable<decimal> CashBuy { get; set; }
        public Nullable<decimal> CashSell { get; set; }
        public Nullable<decimal> RangeA { get; set; }
        public Nullable<decimal> RangeB { get; set; }
        public Nullable<decimal> SAP_Exchange { get; set; }
        public string CR_USER { get; set; }
    }
}