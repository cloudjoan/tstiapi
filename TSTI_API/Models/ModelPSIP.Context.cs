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
    
    public partial class PSIPEntities : DbContext
    {
        public PSIPEntities()
            : base("name=PSIPEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<TB_PIS_INSTALLMaterial> TB_PIS_INSTALLMaterial { get; set; }
        public virtual DbSet<TB_ONE_SysParameter> TB_ONE_SysParameter { get; set; }
        public virtual DbSet<TB_ONE_RoleParameter> TB_ONE_RoleParameter { get; set; }
        public virtual DbSet<VIEW_BULLETINForEip> VIEW_BULLETINForEip { get; set; }
        public virtual DbSet<TB_BULLETIN_TYPE> TB_BULLETIN_TYPE { get; set; }
        public virtual DbSet<TB_BULLETIN_FN_TYPE> TB_BULLETIN_FN_TYPE { get; set; }
        public virtual DbSet<VIEW_BULLETINForEip_SIMPLE> VIEW_BULLETINForEip_SIMPLE { get; set; }
    }
}
