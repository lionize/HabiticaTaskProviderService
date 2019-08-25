﻿using AutoMapper;
using Lionize.HabiticaTaskProvider.ApiModels.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using TIKSN.Lionize.HabiticaTaskProviderService.Business.ProfileSettings;

namespace TIKSN.Lionize.HabiticaTaskProviderService.WebAPI.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserProfileSettingsService _userProfileSettingsService;

        public SettingsController(IMapper mapper, IUserProfileSettingsService userProfileSettingsService)
        {
            _userProfileSettingsService = userProfileSettingsService ?? throw new ArgumentNullException(nameof(userProfileSettingsService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<SettingsGetterResponse> Get(CancellationToken cancellationToken)
        {
            var models = await _userProfileSettingsService.ListAsync(Guid.Parse(User.Identity.Name), cancellationToken);

            return new SettingsGetterResponse
            {
                Settings = _mapper.Map<SettingsGetterItem[]>(models)
            };
        }

        [HttpPut("{id}")]
        public async Task Put(string id, [FromBody] SettingsSetterRequest request)
        {
        }
    }
}