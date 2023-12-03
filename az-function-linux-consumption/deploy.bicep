param appName string
param location string = resourceGroup().location
param packageName string

var storageBlobDataOwnerRoleId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: '${appName}storage'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_ZRS'
  }
  properties: {
    defaultToOAuthAuthentication: true
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
}

resource releasesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: 'releases'
  parent: blobServices
  properties: {
    publicAccess: 'None'
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appName
  location: location
  sku: {
    name: 'Y1' 
    tier: 'Dynamic'
  }
  properties:{
    reserved: true
  }
}

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2021-09-30-preview' = {
  name: appName
  location: location
}

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: '${appName}-func'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}' : { }
    }
  }
  kind: 'functionapp,linux'
  properties: {
    reserved: true
    enabled: true
    serverFarmId: hostingPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '${storageAccount.properties.primaryEndpoints.blob}releases/${packageName}'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE_BLOB_MI_RESOURCE_ID'
          value: identity.id
        }
        {
          name: 'AzureWebJobsStorage__accountName'
          value: storageAccount.name
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedIdentity'
        }
        {
          name: 'AzureWebJobsStorage__clientId'
          value: identity.properties.clientId
        }
      ]
    }
  }
}

resource functionAppStorageBlodDataOwnerUserAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, storageBlobDataOwnerRoleId, identity.id)
  scope: storageAccount
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwnerRoleId)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
