using System;

namespace PatchKit.Api.Models.Main
{
[Serializable]
public struct App
{
    /// <summary>
    /// Initial app id.
    /// </summary>
    public int id;

    /// <summary>
    /// Application secret
    /// </summary>
    public string secret;

    /// <summary>
    /// Application platfrom
    /// </summary>
    public string platform;

    /// <summary>
    /// Application name
    /// </summary>
    public string name;

    /// <summary>
    /// Application display name.
    /// </summary>
    public string display_name;

    /// <summary>
    /// Application author.
    /// </summary>
    public string author;

    /// <summary>
    /// If set to true, application needs to contact keys server to get valid key_secret for content download.
    /// </summary>
    public bool use_keys;

    public string publish_method;

    /// <summary>
    /// Current number of publicly available version (does not count drafts). If 0, no version has been yet published.
    /// </summary>
    public int current_version;

    /// <summary>
    /// The number of lowest version that has a diff file available. For instance if player has version 3 and lowest_version_with_diff=4 then the player can upgrade from version 3 to 4 using diff file. But when lowest_version_with_diff=5 then it's not possible to apply a diff file and player has to re-download the game instead.
    /// </summary>
    public int lowest_version_with_diff;

    public int lowest_version_with_content;

    /// <summary>
    /// Tells the patcher what format should be used to display download speed unit. human_readable should display kilobytes unless download speed exceeds 1024 kilobytes/s, then megabytes should be displayed.
    /// </summary>
    public string PatcherDownlpatcher_download_speed_unitoadSpeedUnit;

    /// <summary>
    /// If set to true, no PatchKit logo or PatchKit reference should be visible on the patcher.
    /// </summary>
    public bool patcher_whitelabel;

    /// <summary>
    /// The secret of patcher to use.
    /// </summary>
    public string patcher_secret;

    /// <summary>
    /// Visible only for the application owner. Set to true if this application is a channel.
    /// </summary>
    public bool is_channel;
}
}