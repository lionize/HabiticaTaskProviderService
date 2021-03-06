﻿using AutoMapper;
using Lionize.HabiticaTaskProvider.ApiModels.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Numerics;
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
        public async Task<SettingsGetterResponseItem[]> Get(CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirst("sub").Value);
            var models = await _userProfileSettingsService.ListAsync(userId, cancellationToken);

            return _mapper.Map<SettingsGetterResponseItem[]>(models);
        }

        [HttpPost]
        public async Task Post([FromBody] SettingsSetterRequest request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirst("sub").Value);

            var model = _mapper.Map<UserProfileSettingsUpdateModel>(request);

            await _userProfileSettingsService.CreateAsync(userId, model, cancellationToken);
        }

        [HttpPut("{id}")]
        public async Task Put(string id, [FromBody] SettingsSetterRequest request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirst("sub").Value);
            var profileId = BigInteger.Parse(id);

            var model = _mapper.Map<UserProfileSettingsUpdateModel>(request);

            await _userProfileSettingsService.UpdateAsync(profileId, userId, model, cancellationToken);
        }
    }
}