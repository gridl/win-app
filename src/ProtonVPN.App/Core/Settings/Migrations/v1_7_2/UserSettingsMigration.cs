﻿/*
 * Copyright (c) 2020 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using ProtonVPN.Core.Profiles.Cached;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Storage;
using System;
using System.Linq;

namespace ProtonVPN.Core.Settings.Migrations.v1_7_2
{
    internal class UserSettingsMigration : IUserSettingsMigration
    {
        private const string SettingsVersionKey = "SettingsVersion";
        private const string UserSettingsMigratedKey = "UserSettingsMigrated";

        private readonly ServerManager _serverManager;
        private readonly ISettingsStorage _appSettings;
        private readonly ISettingsStorage _userSettings;

        public UserSettingsMigration(
            ServerManager serverManager,
            ISettingsStorage appSettings,
            UserSettings userSettings)
        {
            _serverManager = serverManager;
            _appSettings = appSettings;
            _userSettings = userSettings;
        }

        public Version ToVersion => new Version(1, 7, 2);

        public void Apply()
        {
            if (!string.IsNullOrEmpty(SettingsVersion))
                return;

            if (UserSettingsMigrated)
                return;

            MigrateProfiles();
            MigrateSettings();
            UserSettingsMigrated = true;

            SettingsVersion = "1.7.2";
        }

        private void MigrateProfiles()
        {
            var appProfiles = _appSettings.Get<ProfileV1[]>("Profiles") ?? new ProfileV1[0];

            var cachedProfileData = new CachedProfileDataContract
            {
                Local = appProfiles
                    .Select(p => new MigratedProfile(p, _serverManager))
                    .Where(p => p.HasValue)
                    .Select(p => p.Value)
                    .ToList()
            };
            _userSettings.Set("Profiles", cachedProfileData);

            _appSettings.Set<ProfileV1[]>("Profiles", null);
        }

        private void MigrateSettings()
        {
            MigrateToPerUser<string>("AutoConnect");
            MigrateToPerUser<string>("QuickConnect");

            MigrateToPerUser<string>("Uid");
            MigrateToPerUser<string>("AccessToken");
            MigrateToPerUser<string>("RefreshToken");

            MigrateToPerUser<string>("VpnPlan");
            MigrateToPerUser<sbyte>("MaxTier");
            MigrateToPerUser<int>("Delinquent");
            MigrateToPerUser<int>("ExpirationTime");
            MigrateToPerUser<int>("MaxConnect");
            MigrateToPerUser<int>("Services");
            MigrateToPerUser<string>("VpnUsername");
            MigrateToPerUser<string>("VpnPassword");

            MigrateToPerUser<bool>("WelcomeModalShown");
            MigrateToPerUser<long>("TrialExpirationTime");
            MigrateToPerUser<bool>("AboutToExpireModalShown");
            MigrateToPerUser<bool>("ExpiredModalShown");
            MigrateToPerUser<int>("OnboardingStep");
        }

        private void MigrateToPerUser<T>(string key)
        {
            _userSettings.Set(key, _appSettings.Get<T>(key));
            _appSettings.Set(key, default(T));
        }

        private string SettingsVersion
        {
            get => _userSettings.Get<string>(SettingsVersionKey);
            set => _userSettings.Set(SettingsVersionKey, value);
        }

        private bool UserSettingsMigrated
        {
            get => _appSettings.Get<bool>(UserSettingsMigratedKey);
            set => _appSettings.Set(UserSettingsMigratedKey, value);
        }
    }
}
