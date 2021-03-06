﻿using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using TIKSN.Habitica.Rest;
using TIKSN.Lionize.HabiticaTaskProviderService.Business.Messages.Domain.Requests;
using TIKSN.Lionize.HabiticaTaskProviderService.Business.ProfileSettings;
using TIKSN.Lionize.HabiticaTaskProviderService.Business.Settings;
using TIKSN.Lionize.HabiticaTaskProviderService.Data.Repositories;

namespace TIKSN.Lionize.HabiticaTaskProviderService.WebAPI.BackgroundServices
{
    public class PullTodosBackgroundService : BackgroundService
    {
        private readonly ILogger<PullTodosBackgroundService> _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IProfileTodoRepository _profileTodoRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserProfileSettingsRepository _userProfileSettingsRepository;
        private readonly IUserProfileSettingsService _userProfileSettingsService;

        public PullTodosBackgroundService(
            IServiceProvider serviceProvider,
            IUserProfileSettingsRepository userProfileSettingsRepository,
            IProfileTodoRepository profileTodoRepository,
            IUserProfileSettingsService userProfileSettingsService,
            IMapper mapper,
            IMediator mediator,
            ILogger<PullTodosBackgroundService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _userProfileSettingsRepository = userProfileSettingsRepository ?? throw new ArgumentNullException(nameof(userProfileSettingsRepository));
            _userProfileSettingsService = userProfileSettingsService ?? throw new ArgumentNullException(nameof(userProfileSettingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _profileTodoRepository = profileTodoRepository ?? throw new ArgumentNullException(nameof(profileTodoRepository));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Constants retrieved from here https://habitica.fandom.com/wiki/Guidance_for_Comrades

            var userIds = await _userProfileSettingsRepository.ListUserIdsAsync(stoppingToken);

            foreach (var userId in userIds)
            {
                var profiles = await _userProfileSettingsRepository.ListAsync(userId, stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                using (var credentialSettings = scope.ServiceProvider.GetRequiredService<ICredentialSettingsStore>())
                {
                    var habiticaClient = scope.ServiceProvider.GetRequiredService<IHabiticaClient>();

                    foreach (var profile in profiles)
                    {
                        var credentials = await _userProfileSettingsService.GetCredentialAsync(profile.ID, stoppingToken);
                        credentialSettings.Store(credentials.HabiticaUserID, credentials.HabiticaApiToken);

                        try
                        {
                            _logger.LogInformation($"Pull todos for profile {profile.ID}");

                            await PullUserTodosAsync(habiticaClient, profile.ID, profile.UserID, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(30)); //TODO: Get From Configuration
                    }
                }
            }

            await Task.Delay(TimeSpan.FromHours(1)); //TODO: Get From Configuration
        }

        private async Task PullUserTodosAsync(IHabiticaClient habiticaClient, BigInteger profileID, Guid userID, CancellationToken cancellationToken)
        {
            var todos = await habiticaClient.GetUserToDosAsync(cancellationToken);
            var completedTodos = await habiticaClient.GetUserCompletedToDosAsync(cancellationToken);

            await UpdateRecordsAsync(profileID, userID, todos, cancellationToken);
            await UpdateRecordsAsync(profileID, userID, completedTodos, cancellationToken);
        }

        private async Task UpdateRecordsAsync(BigInteger profileID, Guid userID, Habitica.Models.UserTaskModel todos, CancellationToken cancellationToken)
        {
            if (todos.Success)
            {
                foreach (var todo in todos.Data)
                {
                    await _mediator.Send(new UpsertTodoRequest(todo, profileID, userID));
                }
            }
            else
            {
                _logger.LogError("Failed to get user todos.");
            }
        }
    }
}