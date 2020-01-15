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

using System;
using System.Threading.Tasks;
using ProtonVPN.Update.Helpers;

namespace ProtonVPN.Update.Files.Validatable
{
    /// <summary>
    /// Wraps known exceptions of <see cref="ValidatableFile"/> into <see cref="AppUpdateException"/>.
    /// </summary>
    internal class SafeValidatableFile : IValidatableFile
    {
        private readonly IValidatableFile _origin;

        public SafeValidatableFile(IValidatableFile origin)
        {
            _origin = origin;
        }

        public async Task<bool> Valid(string filename, string checkSum)
        {
            try
            {
                return await _origin.Valid(filename, checkSum);
            }
            catch (Exception e) when (e.IsFileAccessException())
            {
                throw new AppUpdateException("Failed to validate downloaded file", e);
            }
        }
    }
}
