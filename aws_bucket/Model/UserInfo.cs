namespace aws_bucket.Model
{
    public class UserInfo
    { 
        public int Id { get; set; } 
        public string? Login { get; set; }
        public string? Package { get; set; }
        public DateTime? blockedUntil { get; set; }

    }
}
