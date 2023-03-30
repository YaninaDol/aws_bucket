namespace aws_bucket.Model
{
    public class UserBlocker
    {
        private DateTime blockedUntil;

        public void BlockForOneDay()
        {
            blockedUntil = DateTime.Now.AddDays(1);
        }

        public void BlockForOneWeek()
        {
            blockedUntil = DateTime.Now.AddDays(7);
        }

        public void BlockForOneMonth()
        {
            blockedUntil = DateTime.Now.AddMonths(1);
        }

        public void BlockForOneYear()
        {
            blockedUntil = DateTime.Now.AddYears(1);
        }

        public bool IsBlocked()
        {
            return DateTime.Now < blockedUntil;
        }
    }
}
