﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ERP_PROXY_DBEntities : DbContext
    {
        public ERP_PROXY_DBEntities()
            : base("name=ERP_PROXY_DBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<VIEW_CUSTOMER_2> VIEW_CUSTOMER_2 { get; set; }
        public virtual DbSet<VIEW_MATERIAL_ByComp> VIEW_MATERIAL_ByComp { get; set; }
        public virtual DbSet<MATERIAL> MATERIAL { get; set; }
        public virtual DbSet<F4501> F4501 { get; set; }
        public virtual DbSet<STOCKALL> STOCKALL { get; set; }
        public virtual DbSet<CUSTOMER_Contact> CUSTOMER_Contact { get; set; }
        public virtual DbSet<TB_MAIL_CONTENT> TB_MAIL_CONTENT { get; set; }
    }
}
