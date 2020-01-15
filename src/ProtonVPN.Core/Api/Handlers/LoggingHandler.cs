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

using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ProtonVPN.Core.Api.Handlers
{
    /// <summary>
    /// Logs all Http requests and responses.
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        public LoggingHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var req = $"{request.Method.Method} \"{request.RequestUri.LocalPath}\"";
            try
            {
                _logger.Info(req);
                var result = await base.SendAsync(request, cancellationToken);
                _logger.Info($"{req}: {(int)result.StatusCode} {result.StatusCode}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"{req} failed: {ex.CombinedMessage()}");
                throw;
            }
        }
    }
}
