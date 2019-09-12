using System;

namespace TFABot
{
    public class clsUser : ISpreadsheet<clsUser>
    {
        public clsUser()
        {
        }

        [ASheetColumnHeader(true, "discord")]
        public string DiscordName { get; set; }
        [ASheetColumnHeader("name")]
        public string Name { get; set; }
        [ASheetColumnHeader("tel")]
        public string Tel { get; set; }
        [ASheetColumnHeader("mail")]
        public string email { get; set; }
        [ASheetColumnHeader("zone")]
        public string TimeZone { get; set; }
        [ASheetColumnHeader("weight")]
        public int Weight { get; set; }
        [ASheetColumnHeader("time from", "timefrom")]
        public TimeSpan TimeFrom { get; set; }
        [ASheetColumnHeader("time to", "timeto")]
        public TimeSpan TimeTo { get; set; }

        [ASheetColumnHeader("keyword")]
        public string[] KeywordAlert { get; set; }

        public void Update(clsUser user)
        {
            if (DiscordName != user.DiscordName) throw new Exception("index name does not match");

            Tel = user.Tel;
            Name = user.Name;
            email = user.email;
            TimeZone = user.TimeZone;
            Weight = user.Weight;
            TimeFrom = user.TimeFrom;
            TimeTo = user.TimeTo;
            KeywordAlert = user.KeywordAlert;
        }

        public string PostPopulate()
        {
            try
            {
                GetUserTime();  //Test to see if we can get the time.
                return null;
            }
            catch (Exception ex)
            {
                return $"Error: {Name} has invalid Timezone {ex.Message}";
            }
        }

        public DateTime GetUserTime()
        {
            return DateTime.UtcNow.ToAbvTimeZone(TimeZone);
        }

        public bool OnDuty
        {
            get
            {
                return GetUserTime().TimeBetween(TimeFrom, TimeTo);
            }
        }
    }
}
