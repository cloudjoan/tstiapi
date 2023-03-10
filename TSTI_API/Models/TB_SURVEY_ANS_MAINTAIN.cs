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
    
    public partial class TB_SURVEY_ANS_MAINTAIN
    {
        public string hash { get; set; }
        public string ID { get; set; }
        public string svid { get; set; }
        public string status { get; set; }
        public string emp_erpid { get; set; }
        public string emp_name { get; set; }
        public string submitTime { get; set; }
        public string srid { get; set; }
        public string product_name { get; set; }
        public string product_serial_no { get; set; }
        public string maintain_type { get; set; }
        public string host_engine { get; set; }
        public string ho_panel_led { get; set; }
        public string ho_cooling_fan { get; set; }
        public string ho_power_module { get; set; }
        public string ho_raid_panel_led { get; set; }
        public string ho_raid_cooling_fan { get; set; }
        public string ho_raid_power_module { get; set; }
        public string ho_raid_event_log { get; set; }
        public string ho_manage_interface_event_log { get; set; }
        public string ho_os_event_log { get; set; }
        public string ho_equip_network_light { get; set; }
        public string ho_others { get; set; }
        public string network { get; set; }
        public string net_backup_config { get; set; }
        public string net_log { get; set; }
        public string net_cpu { get; set; }
        public string net_memory { get; set; }
        public string net_port { get; set; }
        public string net_others { get; set; }
        public string storage { get; set; }
        public string sto_panel_light { get; set; }
        public string sto_cooling_fan { get; set; }
        public string sto_power_module { get; set; }
        public string sto_network_light { get; set; }
        public string sto_fc_iscsi_light { get; set; }
        public string sto_is_severe_error { get; set; }
        public string sto_capacity_over_20 { get; set; }
        public string sto_iops_over_60 { get; set; }
        public string sto_others { get; set; }
        public string vmware { get; set; }
        public string vm_esxi_host_cpu { get; set; }
        public string vm_esxi_host_memory { get; set; }
        public string vm_esxi_datastore { get; set; }
        public string vm_guest_os_cpu { get; set; }
        public string vm_guest_os_memory { get; set; }
        public string vm_high_availability_state { get; set; }
        public string vm_esxi_event_log { get; set; }
        public string vm_horizon_view { get; set; }
        public string vm_nsx { get; set; }
        public string vm_vrops { get; set; }
        public string vm_others { get; set; }
        public string hyper_v { get; set; }
        public string hy_event_log { get; set; }
        public string hy_cluster_health { get; set; }
        public string hy_datastore_usage { get; set; }
        public string hy_usage { get; set; }
        public string hy_availability_state { get; set; }
        public string hy_others { get; set; }
        public string ad { get; set; }
        public string ad_event_log { get; set; }
        public string ad_dcdiag_check { get; set; }
        public string ad_replication_health { get; set; }
        public string ad_kcc_health { get; set; }
        public string ad_others { get; set; }
        public string wsus { get; set; }
        public string ws_event_log { get; set; }
        public string ws_services_health { get; set; }
        public string ws_update_patch_approved { get; set; }
        public string ws_others { get; set; }
        public string network_equipment { get; set; }
        public string net_eq_ip { get; set; }
        public string net_eq_light { get; set; }
        public string net_eq_contact_loose { get; set; }
        public string net_eq_port { get; set; }
        public string net_eq_log_information { get; set; }
        public string net_eq_others { get; set; }
        public string others { get; set; }
        public string InsertTime { get; set; }
        public string UpdateTime { get; set; }
        public string communication { get; set; }
        public string enviornment_check { get; set; }
        public string env_temperature { get; set; }
        public string env_humidity { get; set; }
        public string env_ventilation { get; set; }
        public string env_cleanness { get; set; }
        public string power_backup_system { get; set; }
        public string powergroup_check { get; set; }
        public string powg_type { get; set; }
        public string powg_appearance { get; set; }
        public string powg_unit_voltage { get; set; }
        public string powg_sum_voltage { get; set; }
        public string power_supllier { get; set; }
        public string pows_type { get; set; }
        public string pows_ac_ups_voltage { get; set; }
        public string pows_dc_charger_voltage { get; set; }
        public string pows_charger_value { get; set; }
        public string switch_check { get; set; }
        public string swi_filter_ventilation { get; set; }
        public string swi_suppllier_led { get; set; }
        public string swi_interface_led { get; set; }
        public string swi_hardware_test { get; set; }
        public string swi_software_text { get; set; }
        public string swi_systemdata_copy { get; set; }
        public string attached_equipment { get; set; }
        public string att_auto { get; set; }
        public string att_voice_mailbox { get; set; }
        public string att_accounting { get; set; }
        public string att_keepmusic { get; set; }
        public string att_pc_controller { get; set; }
        public string trunk_error { get; set; }
        public string tru_line_condition { get; set; }
        public string tru_co_interface { get; set; }
        public string tru_did_interface { get; set; }
        public string tru_ti_interface { get; set; }
        public string tru_other_interface { get; set; }
        public string opinions_from_engineer { get; set; }
        public string opi_maintain { get; set; }
    }
}
