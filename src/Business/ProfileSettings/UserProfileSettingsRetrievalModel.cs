﻿using System.Numerics;

namespace TIKSN.Lionize.HabiticaTaskProviderService.Business.ProfileSettings
{
    public class UserProfileSettingsRetrievalModel
    {
        public string HabiticaUserID { get; set; }

        public BigInteger ID { get; set; }

        public string FullName { get; set; }

        public string Username { get; set; }
    }
}