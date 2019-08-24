﻿using Lionize.HabiticaTaskProvider.ApiModels.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TIKSN.Lionize.HabiticaTaskProviderService.WebAPI.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        [HttpGet]
        public async Task<SettingsGetterResponse> Get()
        {
            return null;
        }

        [HttpPut("{id}")]
        public async Task Put(string id, [FromBody] SettingsSetterRequest request)
        {
        }
    }
}