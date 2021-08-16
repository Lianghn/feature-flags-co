﻿using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class CosmosDbService: INoSqlService
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }


        #region old version true false status functions
        public async Task TrueFalseStatusUpdateItemAsync(string id, dynamic item)
        {
            await this._container.UpsertItemAsync<dynamic>(item, new PartitionKey(id));
        }
        public async Task<dynamic> TrueFalseStatusGetItemAsync(string id)
        {
            try
            {
                return await this._container.ReadItemAsync<dynamic>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
        public async Task<EnvironmentFeatureFlagUser> TrueFalseStatusAddEnvironmentFeatureFlagUserAsync(EnvironmentFeatureFlagUser item)
        {
            var newItem = await this._container.CreateItemAsync<EnvironmentFeatureFlagUser>(item, new PartitionKey(item.id), new ItemRequestOptions());
            return newItem;
        }
        public async Task<EnvironmentFeatureFlagUser> TrueFalseStatusGetEnvironmentFeatureFlagUserAsync(string id)
        {
            try
            {
                return await this._container.ReadItemAsync<EnvironmentFeatureFlagUser>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
        public async Task<int> TrueFalseStatusGetFeatureFlagTotalUsersAsync(string featureFlagId)
        {
            int envId = FeatureFlagKeyExtension.GetEnvIdByFeautreFlagId(featureFlagId);
            QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentFFUser'")
              .WithParameter("@environmentId", envId);
            using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (feedIterator.HasMoreResults)
                {
                    Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        return (int)item;
                    }
                }
            }
            return 0;
        }
        public async Task<int> TrueFalseStatusGetFeatureFlagHitUsersAsync(string featureFlagId)
        {
            int envId = FeatureFlagKeyExtension.GetEnvIdByFeautreFlagId(featureFlagId);
            QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentFFUser' and f.ResultValue = true")
              .WithParameter("@environmentId", envId);
            using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (feedIterator.HasMoreResults)
                {
                    Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        return (int)item;
                    }
                }
            }
            return 0;
        }
        #endregion

        public async Task<EnvironmentUser> AddEnvironmentUserAsync(EnvironmentUser item)
        {
            var newItem = await this._container.CreateItemAsync<EnvironmentUser>(item, new PartitionKey(item.Id), new ItemRequestOptions());
            return newItem;
        }


        public async Task<FeatureFlag> GetFeatureFlagAsync(string id)
        {
            return await this._container.ReadItemAsync<FeatureFlag>(id, new PartitionKey(id));
        }
        public async Task<EnvironmentUser> GetEnvironmentUserAsync(string id)
        {
            try
            {
                return await this._container.ReadItemAsync<EnvironmentUser>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }


        public async Task<FeatureFlag> CreateFeatureFlagAsync(CreateFeatureFlagViewModel param, string currentUserId, int projectId, int accountId)
        {
            var keyName = FeatureFlagKeyExtension.CreateNewFeatureFlagKeyName(param.EnvironmentId, param.Name);
            var featureFlagId = FeatureFlagKeyExtension.GetFeatureFlagId(keyName, param.EnvironmentId.ToString(), accountId.ToString(), projectId.ToString());
            var newFeatureFlag = new FeatureFlag()
            {
                Id = featureFlagId,
                EnvironmentId = param.EnvironmentId,
                FF = new FeatureFlagBasicInfo
                {
                    Id = featureFlagId,
                    LastUpdatedTime = DateTime.UtcNow,
                    KeyName = keyName,
                    EnvironmentId = param.EnvironmentId,
                    CreatorUserId = currentUserId,
                    Name = param.Name,
                    Status = param.Status,
                    VariationOptionWhenDisabled = new VariationOption()
                    {
                        DisplayOrder = 1,
                        LocalId = 1,
                        VariationValue = "true"
                    },
                    DefaultRulePercentageRollouts = new List<VariationOptionPercentageRollout>()
                    {
                        new VariationOptionPercentageRollout
                        {
                            RolloutPercentage = new double[2]{ 0, 1},
                            ValueOption = new VariationOption() {
                                DisplayOrder = 1,
                                LocalId = 1,
                                VariationValue = "true"
                            }
                        }
                    }
                },
                FFP = new List<FeatureFlagPrerequisite>(),
                FFTIUForFalse = new List<FeatureFlagTargetIndividualUser>(),
                FFTIUForTrue = new List<FeatureFlagTargetIndividualUser>(),
                FFTUWMTR = new List<FeatureFlagTargetUsersWhoMatchTheseRuleParam>(),
                VariationOptions = new List<VariationOption>() {
                    new VariationOption() {
                        DisplayOrder = 1,
                        LocalId = 1,
                        VariationValue = "true"
                    },
                    new VariationOption() {
                        DisplayOrder = 2,
                        LocalId = 2,
                        VariationValue = "false"
                    },
                },
                IsMultiOptionMode = true,
                TargetIndividuals = new List<TargetIndividualForVariationOption>()
            };
            return await _container.CreateItemAsync<FeatureFlag>(newFeatureFlag);
        }


        public async Task<ReturnJsonModel<FeatureFlag>> UpdateMultiValueOptionSupportedFeatureFlagAsync(FeatureFlag param)
        {
            try
            {
                if (param.FF.DefaultRulePercentageRollouts == null || param.FF.DefaultRulePercentageRollouts.Count == 0)
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, DefaultRulePercentageRollouts shouldn't be empty")
                    };
                if (param.FF.VariationOptionWhenDisabled == null)
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, MultiOptionValueWhenDisabled shouldn't be empty")
                    };
                if (param.FFP != null && param.FFP.Count > 0 && param.FFP.Any(p => p.ValueOptionsVariationValue == null))
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, all ValueOptionsVariationValue FFPs shouldn't be empty")
                    };
                if (param.FFTUWMTR != null && param.FFTUWMTR.Count > 0 && param.FFTUWMTR.Any(p => p.ValueOptionsVariationRuleValues == null || p.ValueOptionsVariationRuleValues.Count == 0))
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, ValueOptionsVariationRuleValues in all FFTUWMTRs shouldn't be empty")
                    };
                if (param.VariationOptions == null || param.VariationOptions.Count == 0)
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, VariationOptions shouldn't be empty")
                    };
                if (param.TargetIndividuals != null && param.TargetIndividuals.Count >= 0 && param.TargetIndividuals.Any(p => p.ValueOption == null))
                    return new ReturnJsonModel<FeatureFlag>()
                    {
                        StatusCode = 500,
                        Error = new Exception("In Multi Option supported mode, ValueOption in TargetIndividual shouldn't be empty")
                    };

                param.EnvironmentId = param.FF.EnvironmentId;
                param.Id = param.FF.Id;
                param.FF.LastUpdatedTime = DateTime.UtcNow;
                param.FF.DefaultRuleValue = null;
                param.FF.ValueWhenDisabled = null;


                if (param.FFTUWMTR != null && param.FFTUWMTR.Count > 0)
                {
                    foreach (var item in param.FFTUWMTR)
                    {
                        if (string.IsNullOrWhiteSpace(item.RuleId))
                        {
                            item.RuleId = Guid.NewGuid().ToString();
                        }
                    }
                }

                await this._container.UpsertItemAsync<FeatureFlag>(param);

                return new ReturnJsonModel<FeatureFlag>
                {
                    StatusCode = 200,
                    Data = param
                };
            }
            catch (Exception exp)
            {
                return new ReturnJsonModel<FeatureFlag>
                {
                    StatusCode = 500,
                    Error = exp
                };
            }

        }

        public async Task<FeatureFlag> UpdateFeatureFlagAsync(FeatureFlag param)
        {
            var originFF = await this.GetFlagAsync(param.Id);
            param.EnvironmentId = param.FF.EnvironmentId;
            param.Id = param.FF.Id;
            param.FF.LastUpdatedTime = DateTime.UtcNow;
            if (param.FFTUWMTR != null && param.FFTUWMTR.Count > 0)
            {
                foreach (var item in param.FFTUWMTR)
                {
                    if (string.IsNullOrWhiteSpace(item.RuleId))
                    {
                        item.RuleId = Guid.NewGuid().ToString();
                    }
                    else
                    {
                        var fftu = originFF.FFTUWMTR.FirstOrDefault(p => p.RuleId == item.RuleId);
                        if(fftu != null)
                        {
                            item.PercentageRolloutForFalseNumber = originFF.FFTUWMTR.FirstOrDefault(p => p.RuleId == item.RuleId).PercentageRolloutForFalseNumber;
                            item.PercentageRolloutForTrueNumber = originFF.FFTUWMTR.FirstOrDefault(p => p.RuleId == item.RuleId).PercentageRolloutForTrueNumber;
                        }
                    }
                }
            }
            if (originFF.FF.PercentageRolloutForFalse != null && originFF.FF.PercentageRolloutForTrue != null &&
                param.FF.PercentageRolloutForFalse != null && param.FF.PercentageRolloutForTrue != null)
            {
                param.FF.PercentageRolloutForFalseNumber = originFF.FF.PercentageRolloutForFalseNumber;
                param.FF.PercentageRolloutForTrueNumber = originFF.FF.PercentageRolloutForTrueNumber;
            }
            return await this._container.UpsertItemAsync<FeatureFlag>(param);
        }

        public async Task<FeatureFlag> ArchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param)
        {
            var originFF = await this.GetFlagAsync(param.FeatureFlagId);
            originFF.FF.LastUpdatedTime = DateTime.UtcNow;
            originFF.IsArchived = true;
            originFF.FF.Status = FeatureFlagStatutEnum.Disabled.ToString();

            return await this._container.UpsertItemAsync<FeatureFlag>(originFF);
        }

        public async Task<FeatureFlag> UnarchiveEnvironmentdFeatureFlagAsync(FeatureFlagArchiveParam param)
        {
            var originFF = await this.GetFlagAsync(param.FeatureFlagId);
            originFF.FF.LastUpdatedTime = DateTime.UtcNow;
            originFF.IsArchived = false;

            return await this._container.UpsertItemAsync<FeatureFlag>(originFF);
        }

        public async Task<EnvironmentUserProperty> UpdateEnvironmentUserPropertiesAsync(int environmentId, List<string> propertyName)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            EnvironmentUserProperty environmentUserProperty = null;
            try
            {
                environmentUserProperty = await this._container.ReadItemAsync<EnvironmentUserProperty>(id, new PartitionKey(id));
                if (propertyName != null && propertyName.Count > 0)
                {
                    foreach (var name in propertyName)
                    {
                        environmentUserProperty.Properties.Add(name);
                    }
                }
                environmentUserProperty.Properties = environmentUserProperty.Properties.Distinct().ToList();
                environmentUserProperty = await this._container.UpsertItemAsync<EnvironmentUserProperty>(environmentUserProperty);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                environmentUserProperty = (await this._container.CreateItemAsync<EnvironmentUserProperty>(
                    new EnvironmentUserProperty
                    {
                        Id = id,
                        Properties = propertyName ?? new List<string>(),
                        EnvironmentId = environmentId
                    }));
            }
            return environmentUserProperty;
        }

        public async Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesAsync(int environmentId)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            try
            {
                EnvironmentUserProperty returnModel = await this._container.ReadItemAsync<EnvironmentUserProperty>(id, new PartitionKey(id));
                returnModel.Properties.Add("KeyId");
                returnModel.Properties.Add("Name");
                returnModel.Properties.Add("Email");
                return returnModel;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new EnvironmentUserProperty()
                {
                    Properties = new List<string>() { "KeyId", "Name", "Email" }
                };
            }
        }


        public async Task<FeatureFlag> GetFlagAsync(string id)
        {
            return await this._container.ReadItemAsync<FeatureFlag>(id, new PartitionKey(id));
        }

        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<FeatureFlagBasicInfo>();
                var results = new List<FeatureFlag>();
            QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.IsArchived != true and f.ObjectType = 'FeatureFlag' offset @offsetNumber limit @pageSize")
                .WithParameter("@environmentId", environmentId)
                .WithParameter("@offsetNumber", pageIndex * pageSize)
                .WithParameter("@pageSize", pageSize);
            //using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>("select * from f where f.EnvironmentId = 1 and f.ObjectType = 'FeatureFlag'"))
            using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (feedIterator.HasMoreResults)
                {
                    Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        FeatureFlagBasicInfo ff = item.ToObject<FeatureFlag>().FF;
                        if (string.IsNullOrWhiteSpace(ff.Status))
                            ff.Status = FeatureFlagStatutEnum.Enabled.ToString();
                        returnResult.Add(ff);
                    }
                }
            }
            return returnResult;
        }

        public async Task<List<FeatureFlagBasicInfo>> GetEnvironmentArchivedFeatureFlagBasicInfoItemsAsync(int environmentId, int pageIndex = 0, int pageSize = 100)
        {
            var returnResult = new List<FeatureFlagBasicInfo>();
            var results = new List<FeatureFlag>();
            QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.IsArchived = true and f.ObjectType = 'FeatureFlag' offset @offsetNumber limit @pageSize")
                .WithParameter("@environmentId", environmentId)
                .WithParameter("@offsetNumber", pageIndex * pageSize)
                .WithParameter("@pageSize", pageSize);
            //using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>("select * from f where f.EnvironmentId = 1 and f.ObjectType = 'FeatureFlag'"))
            using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (feedIterator.HasMoreResults)
                {
                    Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        returnResult.Add(item.ToObject<FeatureFlag>().FF);
                    }
                }
            }
            return returnResult;
        }



        public async Task<int> QueryEnvironmentUsersCountAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace((searchText ?? "").Trim()))
            {
                QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser'")
                  .WithParameter("@environmentId", environmentId);
                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            return (int)item;
                        }
                    }
                }
            }
            else
            {
                QueryDefinition queryDefinition = new QueryDefinition("select value count(1) from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser' and (f.Name like '%@searchText%' or f.KeyId like '%@searchText%')")
                    .WithParameter("@environmentId", environmentId)
                    .WithParameter("@searchText", searchText);
                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            return (int)item;
                        }
                    }
                }
            }

            return 0;
        }



        public async Task<List<EnvironmentUser>> QueryEnvironmentUsersAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            List<EnvironmentUser> returnResult = new List<EnvironmentUser>();
            if (string.IsNullOrWhiteSpace((searchText ?? "").Trim()))
            {
                QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser' offset @offsetNumber limit @pageSize")
                  .WithParameter("@environmentId", environmentId)
                  .WithParameter("@offsetNumber", pageIndex * pageSize)
                  .WithParameter("@pageSize", pageSize);
                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            returnResult.Add(item.ToObject<EnvironmentUser>());
                        }
                    }
                }
                return returnResult;
            }
            else
            {
                QueryDefinition queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.ObjectType = 'EnvironmentUser' and (f.Name like '%@searchText%' or f.KeyId like '%@searchText%')")
                    .WithParameter("@environmentId", environmentId)
                    .WithParameter("@searchText", searchText);
                using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                        foreach (var item in response)
                        {
                            returnResult.Add(item.ToObject<EnvironmentUser>());
                        }
                    }
                }
                return returnResult;
            }
        }

        public async Task<EnvironmentUserProperty> GetEnvironmentUserPropertiesForCRUDAsync(int environmentId)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(environmentId);
            try
            {
                return await this._container.ReadItemAsync<EnvironmentUserProperty>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new EnvironmentUserProperty()
                {
                    EnvironmentId = environmentId,
                    Id = id,
                    Properties = new List<string>()
                };
            }
        }

        public async Task CreateOrUpdateEnvironmentUserPropertiesForCRUDAsync(EnvironmentUserProperty param)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(param.EnvironmentId);
            await this._container.UpsertItemAsync<EnvironmentUserProperty>(new EnvironmentUserProperty
            {
                Id = id,
                EnvironmentId = param.EnvironmentId,
                Properties = param.Properties,
                ObjectType = param.ObjectType
            });
        }

        public async Task UpsertEnvironmentUserAsync(EnvironmentUser param)
        {
            await this._container.UpsertItemAsync<EnvironmentUser>(param);
        }


        public async Task<List<PrequisiteFeatureFlagViewModel>> SearchPrequisiteFeatureFlagsAsync(int environmentId, string searchText = "", int pageIndex = 0, int pageSize = 20)
        {

            var returnResult = new List<PrequisiteFeatureFlagViewModel>();
            QueryDefinition queryDefinition = null;
            if (string.IsNullOrWhiteSpace((searchText ?? "").Trim()))
            {
                queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.IsArchived != true and f.ObjectType = 'FeatureFlag' offset @offsetNumber limit @pageSize")
                    .WithParameter("@environmentId", environmentId)
                    .WithParameter("@offsetNumber", pageIndex * pageSize)
                    .WithParameter("@pageSize", pageSize);
               
            }
            else
            {
                queryDefinition = new QueryDefinition("select * from f where f.EnvironmentId = @environmentId and f.IsArchived != true and f.ObjectType = 'FeatureFlag' and f.FF.Name like '%@searchText%'  offset @offsetNumber limit @pageSize")
                    .WithParameter("@environmentId", environmentId)
                    .WithParameter("@offsetNumber", pageIndex * pageSize)
                    .WithParameter("@pageSize", pageSize)
                    .WithParameter("@searchText", searchText);
            }
            using (FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition))
            {
                while (feedIterator.HasMoreResults)
                {
                    Microsoft.Azure.Cosmos.FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        FeatureFlag ff = item.ToObject<FeatureFlag>();
                        returnResult.Add(new PrequisiteFeatureFlagViewModel()
                        {
                            EnvironmentId = environmentId,
                            Id = ff.Id,
                            KeyName = ff.FF.KeyName,
                            Name = ff.FF.Name,
                            VariationOptions = ff.VariationOptions
                        });
                    }
                }
            }


            return returnResult;
        }
    }
}
