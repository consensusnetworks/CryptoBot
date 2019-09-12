using System;
using System.Collections.Generic;
using System.Text;

namespace TFABot
{
    public class clsAlarmManager
    {
        List<clsAlarm> AlarmList = new List<clsAlarm>();

        public clsAlarmManager()
        {
        }

        public void New(clsAlarm Alarm)
        {
            Alarm.Process();
            AlarmList.Add(Alarm);
        }

        public void Clear(clsAlarm Alarm, string message = null)
        {
            if (AlarmList.Contains(Alarm))
            {
                Alarm.Clear(message);
                AlarmList.Remove(Alarm);
            }

        }

        public void Process()
        {
            //Process all Alarms, removing expired alarms.
            //  AlarmList.RemoveAll(x=>!x.Process());

            foreach (var alarm in AlarmList)
            {
                try
                {
                    alarm.Process();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Alarm Process: {ex.Message}");
                }
            }

        }

        public new string ToString()
        {
            var message = new StringBuilder();

            if (AlarmList.Count == 0)
            {
                message.AppendLine("no alarms");
            }
            else
            {
                message.Append("```");
                foreach (var alarm in AlarmList)
                {
                    message.AppendLine(alarm.ToString());
                }
                message.Append("```");
            }
            return message.ToString();
        }

        internal void Remove(clsAlarm Alarm)
        {
            if (Alarm != null && AlarmList.Contains(Alarm))
            {
                AlarmList.Remove(Alarm);
            }
        }
    }
}
