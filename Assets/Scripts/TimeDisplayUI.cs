using UnityEngine;
using TMPro;

public class TimeDisplayUI : MonoBehaviour
{
    public bool showDayCounter = true;

    [Header("References")]
    public DayNightCycle dayNight;   // drag your DayNightManager here
    public TMP_Text timeText;

    [Header("Format")]
    public bool use12HourClock = true;
    public bool showSeconds = false;

    void Awake()
    {
        if (timeText == null)
            timeText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (dayNight == null || timeText == null) return;

        // Convert 0..1 time to hours
        float totalHours = dayNight.time01 * 24f;

        int hours = Mathf.FloorToInt(totalHours);
        int minutes = Mathf.FloorToInt((totalHours - hours) * 60f);
        int seconds = Mathf.FloorToInt((((totalHours - hours) * 60f) - minutes) * 60f);
        int day = Mathf.FloorToInt(Time.time / (dayNight.dayLengthMinutes * 60f));

        string suffix = "";

        if (use12HourClock)
        {
            suffix = hours >= 12 ? " PM" : " AM";
            hours = hours % 12;
            if (hours == 0) hours = 12;
        }

        string timeString;

        if (showSeconds)
            timeString = $"{hours:00}:{minutes:00}:{seconds:00}{suffix}";
        else
            timeString = $"{hours:00}:{minutes:00}{suffix}";

        if (showDayCounter)
            timeText.text = $"Day: {dayNight.currentDay}  {timeString}";
        else
            timeText.text = timeString;

    }
}