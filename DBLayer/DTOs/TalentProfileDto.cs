namespace ESOF.WebApp.DBLayer.DTOs
{
    public class TalentProfileDto
    {
        public string profile_name { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public float price { get; set; }
        public float privacy { get; set; }
        public int fk_user_id { get; set; }
    }
}