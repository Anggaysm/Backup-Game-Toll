using UnityEngine;

public static class GameData
{
    public static string selectedZone
    {
        get
        {
            return PlayerPrefs.GetString(
                "SelectedZone",
                "Cililitan"
            );
        }

        set
        {
            PlayerPrefs.SetString(
                "SelectedZone",
                value
            );

            PlayerPrefs.Save();
        }
    }
}