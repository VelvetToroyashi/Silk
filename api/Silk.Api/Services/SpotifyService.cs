using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Silk.Api.ApiResponses.Spotify;

namespace Silk.Api.Services
{
    public sealed class SpotifyService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string _accessToken;
        private string _refreshToken;
        private DateTime? _tokenExpiry;
        
        private readonly HttpClient _spotifyHttpClient = new();
        
        private const string TokenisationUrl = "https://accounts.spotify.com/api/token";
        private const string TrackLookupUrl = "https://api.spotify.com/v1/search";
        private const string ArtistLookupUrl = "https://api.spotify.com/v1/artists";
        
        public SpotifyService(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            
            CheckAndRefreshTokens().GetAwaiter().GetResult();
        }
        
        public async Task CheckAndRefreshTokens()
        {
            if (_tokenExpiry == null || _tokenExpiry.Value <= (DateTime.Now - TimeSpan.FromMinutes(5)))
            {
                //Initial authentication - if tokens are not available or about to expire within 5 minutes
                
                _spotifyHttpClient.DefaultRequestHeaders.Clear();
                _spotifyHttpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"))}");
                
                HttpResponseMessage result = await _spotifyHttpClient.PostAsync(TokenisationUrl, new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                }));

                if (!result.IsSuccessStatusCode)
                {
                    _accessToken = null;
                    _refreshToken = null;
                    _tokenExpiry = null;
                    
                    throw new AuthenticationException("Unable to tokenise Spotify credentials");
                }

                SpotifyTokenModel tokens = JsonConvert.DeserializeObject<SpotifyTokenModel>(await result.Content.ReadAsStringAsync());

                _accessToken = tokens!.AccessToken;
                _refreshToken = tokens!.RefreshToken;
                _tokenExpiry = DateTime.Now + TimeSpan.FromSeconds(tokens!.TokenExpiry);
            }
            else if(_refreshToken != null && _tokenExpiry.Value <= (DateTime.Now - TimeSpan.FromMinutes(10)))
            {
                //TODO this will probably never be hit as Spotify doesn't seem to send refresh tokens
                //Pokey was here *mlem*
                
                //Refresh authentication - if tokens are about to expire within 10 minutes
                
                _spotifyHttpClient.DefaultRequestHeaders.Clear();
                _spotifyHttpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"))}");
                
                HttpResponseMessage result = await _spotifyHttpClient.PostAsync(TokenisationUrl, new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", _refreshToken)
                }));

                if (!result.IsSuccessStatusCode)
                {
                    _accessToken = null;
                    _refreshToken = null;
                    _tokenExpiry = null;

                    throw new AuthenticationException("Unable to refresh Spotify token");
                }

                SpotifyTokenModel tokens = JsonConvert.DeserializeObject<SpotifyTokenModel>(await result.Content.ReadAsStringAsync());

                _accessToken = tokens!.AccessToken;
                _tokenExpiry = DateTime.Now + TimeSpan.FromSeconds(tokens.TokenExpiry);
            }
        }
        
        public async Task<ApiResponse<SpotifyPagingModel<SpotifyTrackModel>>> GetTrack(string search)
        {
            await CheckAndRefreshTokens();
            
            _spotifyHttpClient.DefaultRequestHeaders.Clear();
            _spotifyHttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
                
            HttpResponseMessage result = await _spotifyHttpClient.GetAsync($"{TrackLookupUrl}?q={WebUtility.UrlEncode(search)}&type=track");

            if (!result.IsSuccessStatusCode) return null;

            string raw = await result.Content.ReadAsStringAsync();
            Dictionary<string, SpotifyPagingModel<SpotifyTrackModel>> page = JsonConvert.DeserializeObject<Dictionary<string, SpotifyPagingModel<SpotifyTrackModel>>>(raw);

            return new ApiResponse<SpotifyPagingModel<SpotifyTrackModel>>
            {
                StatusCode = result.StatusCode,
                Raw = raw,
                Response = page!["tracks"]
            };
        }

        public async Task<ApiResponse<SpotifyArtistModel>> GetArtist(string artistId)
        {
            await CheckAndRefreshTokens();
            
            _spotifyHttpClient.DefaultRequestHeaders.Clear();
            _spotifyHttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
            
            HttpResponseMessage result = await _spotifyHttpClient.GetAsync($"{ArtistLookupUrl}/{WebUtility.UrlEncode(artistId)}");

            if (!result.IsSuccessStatusCode) return null;

            string raw = await result.Content.ReadAsStringAsync();
            SpotifyArtistModel page = JsonConvert.DeserializeObject<SpotifyArtistModel>(raw);

            return new ApiResponse<SpotifyArtistModel>
            {
                StatusCode = result.StatusCode,
                Raw = raw,
                Response = page
            };
        }
    }
}