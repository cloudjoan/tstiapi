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
    
    public partial class TSTIONEEntities : DbContext
    {
        public TSTIONEEntities()
            : base("name=TSTIONEEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<TB_ONE_SRDetail_Contact> TB_ONE_SRDetail_Contact { get; set; }
        public virtual DbSet<TB_ONE_SRDetail_Record> TB_ONE_SRDetail_Record { get; set; }
        public virtual DbSet<TB_ONE_SRDetail_Warranty> TB_ONE_SRDetail_Warranty { get; set; }
        public virtual DbSet<TB_ONE_LOG> TB_ONE_LOG { get; set; }
        public virtual DbSet<TB_ONE_SRCustomerEmailMapping> TB_ONE_SRCustomerEmailMapping { get; set; }
        public virtual DbSet<TB_ONE_SRIDFormat> TB_ONE_SRIDFormat { get; set; }
        public virtual DbSet<TB_ONE_SRRepairType> TB_ONE_SRRepairType { get; set; }
        public virtual DbSet<TB_ONE_SRSQPerson> TB_ONE_SRSQPerson { get; set; }
        public virtual DbSet<TB_ONE_SRTeamMapping> TB_ONE_SRTeamMapping { get; set; }
        public virtual DbSet<TB_ONE_DOCUMENT> TB_ONE_DOCUMENT { get; set; }
        public virtual DbSet<TB_ONE_ReportFormat> TB_ONE_ReportFormat { get; set; }
        public virtual DbSet<TB_ONE_SRDetail_Product> TB_ONE_SRDetail_Product { get; set; }
        public virtual DbSet<TB_ONE_SRFixRecord> TB_ONE_SRFixRecord { get; set; }
        public virtual DbSet<TB_ONE_SRDetail_MaterialInfo> TB_ONE_SRDetail_MaterialInfo { get; set; }
        public virtual DbSet<TB_ONE_SRDetail_PartsReplace> TB_ONE_SRDetail_PartsReplace { get; set; }
        public virtual DbSet<TB_ONE_SRDetail_SerialFeedback> TB_ONE_SRDetail_SerialFeedback { get; set; }
        public virtual DbSet<TB_ONE_SRSatisfy_Survey> TB_ONE_SRSatisfy_Survey { get; set; }
        public virtual DbSet<TB_ONE_ContractDetail_ENG> TB_ONE_ContractDetail_ENG { get; set; }
        public virtual DbSet<TB_ONE_ContractDetail_OBJ> TB_ONE_ContractDetail_OBJ { get; set; }
        public virtual DbSet<TB_ONE_ContractDetail_SUB> TB_ONE_ContractDetail_SUB { get; set; }
        public virtual DbSet<TB_ONE_ContractMain> TB_ONE_ContractMain { get; set; }
        public virtual DbSet<TB_ONE_ContractIDTemp> TB_ONE_ContractIDTemp { get; set; }
        public virtual DbSet<TB_ONE_SRMain> TB_ONE_SRMain { get; set; }
    }
}
