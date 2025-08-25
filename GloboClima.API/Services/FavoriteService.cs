using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using GloboClima.API.Exceptions;
using GloboClima.API.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GloboClima.API.Services
{
    public class FavoriteService : IDisposable
    {
        private readonly IDynamoDBContext _context;
        private readonly ILogger<FavoriteService> _logger;
        private bool _disposed;

        public FavoriteService(IAmazonDynamoDB dynamoDB, ILogger<FavoriteService> logger)
        {
            if (dynamoDB == null) throw new ArgumentNullException(nameof(dynamoDB));
            
            var config = new DynamoDBContextConfig 
            { 
                Conversion = DynamoDBEntryConversion.V2,
                ConsistentRead = true
            };
            
            _context = new DynamoDBContext(dynamoDB, config);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Favorite?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            }

            try
            {
                return await _context.LoadAsync<Favorite>(id);
            }
            catch (AmazonDynamoDBException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Favorite with ID {FavoriteId} not found", id);
                return null;
            }
            catch (Exception ex) when (!(ex is AmazonDynamoDBException))
            {
                _logger.LogError(ex, "Error retrieving favorite with ID {FavoriteId}", id);
                throw new ApiException(HttpStatusCode.InternalServerError, "An error occurred while retrieving the favorite");
            }
        }

        public async Task SaveAsync(Favorite favorite)
        {
            try
            {
                favorite.CreatedAt = DateTime.UtcNow;
                await _context.SaveAsync(favorite);
                _logger.LogInformation("Successfully saved favorite {FavoriteId} for user {Username}", 
                    favorite.Id, favorite.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving favorite for user {Username}", favorite.Username);
                throw new ApiException(HttpStatusCode.InternalServerError, "An error occurred while saving the favorite");
            }
        }

        public async Task<List<Favorite>> GetAllAsync(string username)
        {
            try
            {
                var conditions = new List<ScanCondition>
                {
                    new ScanCondition("Username", ScanOperator.Equal, username)
                };

                var result = await _context.ScanAsync<Favorite>(conditions).GetRemainingAsync();
                _logger.LogInformation("Retrieved {Count} favorites for user {Username}", result.Count, username);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving favorites for user {Username}", username);
                throw new ApiException(HttpStatusCode.InternalServerError, "An error occurred while retrieving favorites");
            }
        }

        public async Task DeleteAsync(string id)
        {
            try
            {
                await _context.DeleteAsync<Favorite>(id);
                _logger.LogInformation("Successfully deleted favorite {FavoriteId}", id);
            }
            catch (AmazonDynamoDBException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Attempted to delete non-existent favorite {FavoriteId}", id);
                throw new ApiException(HttpStatusCode.NotFound, $"Favorite with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting favorite {FavoriteId}", id);
                throw new ApiException(HttpStatusCode.InternalServerError, "An error occurred while deleting the favorite");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                
                _disposed = true;
            }
        }

        ~FavoriteService()
        {
            Dispose(false);
        }
    }
}
