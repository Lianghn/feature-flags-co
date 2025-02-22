﻿using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.DataSync;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.ViewModels.FeatureFlagTrigger;

namespace FeatureFlags.APIs.Services
{
    public interface INoSqlService
    {
        Task<List<T>> GetEnvironmentDataAsync<T>(int envId);
        Task SaveEnvironmentDataAsync(int accountId, int projectId, int envId, EnvironmentDataViewModel data);

        Task<List<EnvironmentUser>> QueryEnvironmentUsersAsync(string searchText, int environmentId, int pageIndex,
            int pageSize);

        Task<int> QueryEnvironmentUsersCountAsync(string searchText, int environmentId, int pageIndex, int pageSize);
        Task<EnvironmentUser> GetEnvironmentUserAsync(string id);
        Task<FeatureFlag> GetFeatureFlagAsync(string id);
        Task<EnvironmentUser> AddEnvironmentUserAsync(EnvironmentUser item);

        Task<List<FeatureFlagBasicInfo>> GetEnvironmentFeatureFlagBasicInfoItemsAsync(int environmentId,
            int pageIndex = 0, int pageSize = 100);

        Task<List<PrequisiteFeatureFlagViewModel>> SearchPrequisiteFeatureFlagsAsync(int environmentId,
            string searchText = "", int pageIndex = 0, int pageSize = 20);

        Task<List<FeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(int environmentId,
            int pageIndex = 0, int pageSize = 100);

        Task<FeatureFlag> GetFlagAsync(string id);
        Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesForCRUDAsync(int environmentId);
        Task CreateOrUpdateEnvironmentUserPropertiesForCRUDAsync(EnvironmentUserProperty param);

        Task<FeatureFlag> CreateFeatureFlagAsync(CreateFeatureFlagViewModel param, string currentUserId, int projectId,
            int accountId);

        Task<FeatureFlag> UpdateFeatureFlagAsync(FeatureFlag param);
        Task<ReturnJsonModel<FeatureFlag>> UpdateMultiValueOptionSupportedFeatureFlagAsync(FeatureFlag param);
        Task<FeatureFlag> ArchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param);
        Task<FeatureFlag> UnarchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param);

        Task<EnvironmentUserProperty>
            UpdateEnvironmentUserPropertiesAsync(int environmentId, List<string> propertyName);

        Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesAsync(int environmentId);

        Task UpsertEnvironmentUserAsync(EnvironmentUser param);

        Task<List<FeatureFlagTrigger>> GetFlagTriggersByFfIdAsync(string id);

        Task TriggerFeatureFlagByFlagTriggerAsync(string token);

        Task<FeatureFlagTriggerViewModel> CreateFlagTriggerAsync(FeatureFlagTriggerViewModel trigger);

        Task<FeatureFlagTrigger> DisableFlagTriggerAsync(string id, string featureFlagId);

        Task<FeatureFlagTrigger> ArchiveFlagTriggerAsync(string id, string featureFlagId);

        Task<FeatureFlagTrigger> EnableFlagTriggerAsync(string id, string featureFlagId);

        Task<FeatureFlagTrigger> ResetFlagTriggerTokenAsync(string id, string featureFlagId);
    }
}