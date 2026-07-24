using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;

[DisallowMultipleComponent]
public class DiscordManager : MonoBehaviour
{

    public static Discord.Discord discord;

    public static DiscordManager me;

    // Start is called before the first frame update
    void Awake()
    {
        if (me != null)
        {
            Destroy(gameObject);
            return;
        }
        me = this;

        // this is genuine bullshit
        bool discordRunning = false;
        for (int i = 0; i < System.Diagnostics.Process.GetProcesses().Length; i++)
        {
            if (System.Diagnostics.Process.GetProcesses()[i].ToString() == "System.Diagnostics.Process (Discord)")
            {
                discordRunning = true;
                break;
            }
        }

        if (discordRunning)
            discord = new Discord.Discord(1530329648033173696, (System.UInt64)Discord.CreateFlags.Default);

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (discord == null)
            return;
        
        discord.RunCallbacks();
    }

    public void SetActivity(string State, string Details)
    {
        if (discord == null)
            return;

        var activityManager = discord.GetActivityManager();
        var activity = new Discord.Activity
        {
            State = State,
            Details = Details
        };
        activityManager.UpdateActivity(activity, (res) =>
        {
            if (res == Discord.Result.Ok)
            {
                Debug.Log("set discord activity to \"" + Details + "\"");
            }
        });
    }

    public string GetUsername()
    {
        if (discord == null)
            return null;

        var userManager = discord.GetUserManager();
        var user = userManager.GetCurrentUser();

        return user.Username;
    }

    public Texture2D GetUserAvatar()
    {
        if (discord == null)
            return null;

        Texture2D ret = null;



        return ret;
    }

    public bool Available()
    {
        return discord != null;
    }
}
