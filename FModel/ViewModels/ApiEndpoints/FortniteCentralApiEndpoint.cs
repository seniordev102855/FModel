﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FModel.Framework;
using FModel.ViewModels.ApiEndpoints.Models;
using RestSharp;
using Serilog;

namespace FModel.ViewModels.ApiEndpoints;

public class FortniteCentralApiEndpoint : AbstractApiProvider
{
    public FortniteCentralApiEndpoint(RestClient client) : base(client)
    {
    }

    public async Task<AesResponse> GetAesKeysAsync(CancellationToken token)
    {
        var request = new FRestRequest("https://fortnitecentral.gmatrixgames.ga/api/v1/aes")
        {
            OnBeforeDeserialization = resp => { resp.ContentType = "application/json; charset=utf-8"; }
        };
        var response = await _client.ExecuteAsync<AesResponse>(request, token).ConfigureAwait(false);
        Log.Information("[{Method}] [{Status}({StatusCode})] '{Resource}'", request.Method, response.StatusDescription, (int) response.StatusCode, response.ResponseUri?.OriginalString);
        return response.Data;
    }

    public AesResponse GetAesKeys(CancellationToken token)
    {
        return GetAesKeysAsync(token).GetAwaiter().GetResult();
    }

    public async Task<MappingsResponse[]> GetMappingsAsync(CancellationToken token)
    {
        var request = new FRestRequest("https://fortnitecentral.gmatrixgames.ga/api/v1/mappings")
        {
            OnBeforeDeserialization = resp => { resp.ContentType = "application/json; charset=utf-8"; }
        };
        var response = await _client.ExecuteAsync<MappingsResponse[]>(request, token).ConfigureAwait(false);
        Log.Information("[{Method}] [{Status}({StatusCode})] '{Resource}'", request.Method, response.StatusDescription, (int) response.StatusCode, response.ResponseUri?.OriginalString);
        return response.Data;
    }

    public MappingsResponse[] GetMappings(CancellationToken token)
    {
        return GetMappingsAsync(token).GetAwaiter().GetResult();
    }

    public async Task<Dictionary<string, Dictionary<string, string>>> GetHotfixesAsync(CancellationToken token, string language = "en")
    {
        var request = new FRestRequest("https://fortnitecentral.gmatrixgames.ga/api/v1/hotfixes")
        {
            OnBeforeDeserialization = resp => { resp.ContentType = "application/json; charset=utf-8"; }
        };
        request.AddParameter("lang", language);
        var response = await _client.ExecuteAsync<Dictionary<string, Dictionary<string, string>>>(request, token).ConfigureAwait(false);
        Log.Information("[{Method}] [{Status}({StatusCode})] '{Resource}'", request.Method, response.StatusDescription, (int) response.StatusCode, response.ResponseUri?.OriginalString);
        return response.Data;
    }

    public Dictionary<string, Dictionary<string, string>> GetHotfixes(CancellationToken token, string language = "en")
    {
        return GetHotfixesAsync(token, language).GetAwaiter().GetResult();
    }
}
