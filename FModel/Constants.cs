﻿using System.Reflection;
using CUE4Parse.UE4.Objects.Core.Misc;

namespace FModel;

public static class Constants
{
    public static readonly string APP_VERSION = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    public const string ZERO_64_CHAR = "0000000000000000000000000000000000000000000000000000000000000000";
    public static readonly FGuid ZERO_GUID = new(0U);

    public const string WHITE = "#DAE5F2";
    public const string GRAY = "#BBBBBB";
    public const string RED = "#E06C75";
    public const string GREEN = "#98C379";
    public const string YELLOW = "#E5C07B";
    public const string BLUE = "#528BCC";

    public const string ISSUE_LINK = "https://github.com/iAmAsval/FModel/issues/new/choose";
    public const string DONATE_LINK = "https://fmodel.app/donate?utm_source=fmodel&utm_medium=app&utm_campaign=donations";
    public const string DISCORD_LINK = "https://fmodel.app/discord?utm_source=fmodel&utm_medium=app&utm_campaign=discord-server";

    public const string _FN_LIVE_TRIGGER = "fortnite-live.manifest";
    public const string _VAL_LIVE_TRIGGER = "valorant-live.manifest";

    public const string _NO_PRESET_TRIGGER = "Hand Made";
}
