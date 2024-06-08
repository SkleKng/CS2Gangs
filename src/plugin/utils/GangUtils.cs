namespace plugin.utils
{
    internal enum GangRank
    {
        Member = 0,
        Officer = 1,
        Owner = 2
    }

    internal class GangUtils
    {
        public static string GetGangRankName(int? rank)
        {
            return rank switch
            {
                (int?)GangRank.Member => "Member",
                (int?)GangRank.Officer => "Officer",
                (int?)GangRank.Owner => "Owner",
                _ => "Unknown"
            };
        }

        public static GangRank GetGangRankFromName(string name)
        {
            return name.ToLower() switch
            {
                "member" => GangRank.Member,
                "officer" => GangRank.Officer,
                "owner" => GangRank.Owner,
                _ => GangRank.Member
            };
        }
    }
}

