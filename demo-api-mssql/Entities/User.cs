namespace WebApi.Entities
{
    public class User
    {
        public int id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string fullname { get; set; }
        public string role { get; set; }
        public string organization { get; set; }
        public string status { get; set; }
        public string create_dt { get; set; }
        public string update_dt { get; set; }
    }
}