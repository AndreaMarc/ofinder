using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Services
{
    /// <summary>
    /// Implementation of Performer-specific queries.
    /// Auto-discovered via I*ManualService pattern.
    /// </summary>
    public class PerformerManualService : IPerformerManualService
    {
        private readonly JsonApiDbContext _context;
        private readonly ILogService _logService;

        public PerformerManualService(JsonApiDbContext context, ILogService logService = null)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<List<PerformerWithStatsDTO>> GetTopRatedPerformersAsync(int limit = 10, int minReviews = 1)
        {
            try
            {
                var topPerformers = await _context.Performers
                    .Include(p => p.User)
                    .Include(p => p.PerformerReviews)
                    .Include(p => p.PerformerViews)
                    .Include(p => p.UserFavorites)
                    .Where(p => p.IsActive && p.PerformerReviews.Count >= minReviews)
                    .AsNoTracking()
                    .Select(p => new PerformerWithStatsDTO
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        Email = p.User.Email,
                        FirstName = p.User.FirstName,
                        LastName = p.User.LastName,
                        Description = p.Description,
                        IsVerified = p.IsVerified,
                        AvgRating = p.PerformerReviews.Any() ? p.PerformerReviews.Average(r => r.Rating) : 0,
                        TotalReviews = p.PerformerReviews.Count,
                        TotalViews = p.PerformerViews.Count,
                        TotalFavorites = p.UserFavorites.Count,
                        CreatedAt = p.CreatedAt
                    })
                    .OrderByDescending(p => p.AvgRating)
                    .ThenByDescending(p => p.TotalReviews)
                    .Take(limit)
                    .ToListAsync();

                return topPerformers;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in GetTopRatedPerformersAsync: {ex.Message}");
                return new List<PerformerWithStatsDTO>();
            }
        }

        public async Task<ChannelStatsDTO> GetChannelStatsAsync(string performerId)
        {
            try
            {
                var channels = await _context.Channels
                    .Include(c => c.ChannelContentTypes)
                    .Include(c => c.ChannelPricing)
                    .Where(c => c.PerformerId == performerId && c.IsActive)
                    .AsNoTracking()
                    .ToListAsync();

                if (!channels.Any())
                    return null;

                var channelsByPlatform = channels
                    .GroupBy(c => c.Platform.ToString())
                    .Select(g => new PlatformChannelDTO
                    {
                        Platform = g.Key,
                        Count = g.Count(),
                        UsernameHandles = g.Select(c => c.UsernameHandle).Where(u => !string.IsNullOrEmpty(u)).ToList()
                    })
                    .ToList();

                var allPricing = channels
                    .Where(c => c.ChannelPricing != null)
                    .Select(c => c.ChannelPricing)
                    .ToList();

                var stats = new ChannelStatsDTO
                {
                    PerformerId = performerId,
                    TotalChannels = channels.Count,
                    ChannelsByPlatform = channelsByPlatform,
                    TotalContentTypes = channels.SelectMany(c => c.ChannelContentTypes).Count(),
                    PricingRanges = new PricingRangesDTO
                    {
                        MonthlySubscriptionMin = allPricing.Where(p => p.MonthlySubscriptionFrom.HasValue)
                            .Any() ? allPricing.Where(p => p.MonthlySubscriptionFrom.HasValue).Min(p => p.MonthlySubscriptionFrom) : null,
                        MonthlySubscriptionMax = allPricing.Where(p => p.MonthlySubscriptionTo.HasValue)
                            .Any() ? allPricing.Where(p => p.MonthlySubscriptionTo.HasValue).Max(p => p.MonthlySubscriptionTo) : null,
                        PhotoSaleMin = allPricing.Where(p => p.PhotoSaleFrom.HasValue)
                            .Any() ? allPricing.Where(p => p.PhotoSaleFrom.HasValue).Min(p => p.PhotoSaleFrom) : null,
                        PhotoSaleMax = allPricing.Where(p => p.PhotoSaleTo.HasValue)
                            .Any() ? allPricing.Where(p => p.PhotoSaleTo.HasValue).Max(p => p.PhotoSaleTo) : null,
                        VideoSaleMin = allPricing.Where(p => p.VideoSaleFrom.HasValue)
                            .Any() ? allPricing.Where(p => p.VideoSaleFrom.HasValue).Min(p => p.VideoSaleFrom) : null,
                        VideoSaleMax = allPricing.Where(p => p.VideoSaleTo.HasValue)
                            .Any() ? allPricing.Where(p => p.VideoSaleTo.HasValue).Max(p => p.VideoSaleTo) : null,
                        LivePublicMin = allPricing.Where(p => p.LivePublicFrom.HasValue)
                            .Any() ? allPricing.Where(p => p.LivePublicFrom.HasValue).Min(p => p.LivePublicFrom) : null,
                        LivePublicMax = allPricing.Where(p => p.LivePublicTo.HasValue)
                            .Any() ? allPricing.Where(p => p.LivePublicTo.HasValue).Max(p => p.LivePublicTo) : null,
                        LivePrivateMin = allPricing.Where(p => p.LivePrivateFrom.HasValue)
                            .Any() ? allPricing.Where(p => p.LivePrivateFrom.HasValue).Min(p => p.LivePrivateFrom) : null,
                        LivePrivateMax = allPricing.Where(p => p.LivePrivateTo.HasValue)
                            .Any() ? allPricing.Where(p => p.LivePrivateTo.HasValue).Max(p => p.LivePrivateTo) : null
                    }
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in GetChannelStatsAsync({performerId}): {ex.Message}");
                return null;
            }
        }

        public async Task<PerformerDashboardDTO> GetPerformerDashboardAsync(string performerId)
        {
            try
            {
                var performer = await _context.Performers
                    .Include(p => p.PerformerReviews)
                    .Include(p => p.PerformerViews)
                    .Include(p => p.UserFavorites)
                    .Include(p => p.Channels)
                    .Include(p => p.PerformerServices)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == performerId);

                if (performer == null)
                    return null;

                var dashboard = new PerformerDashboardDTO
                {
                    PerformerId = performerId,
                    TotalViews = performer.PerformerViews.Count,
                    TotalReviews = performer.PerformerReviews.Count,
                    AvgRating = performer.PerformerReviews.Any() ? performer.PerformerReviews.Average(r => r.Rating) : 0,
                    TotalFavorites = performer.UserFavorites.Count,
                    TotalChannels = performer.Channels.Count(c => c.IsActive),
                    TotalServices = performer.PerformerServices.Count(s => s.IsActive),
                    MemberSince = performer.CreatedAt,
                    LastUpdated = performer.UpdatedAt
                };

                return dashboard;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in GetPerformerDashboardAsync({performerId}): {ex.Message}");
                return null;
            }
        }

        public async Task<bool> TrackPerformerViewAsync(string performerId, string userId)
        {
            try
            {
                // Check if view already exists (UNIQUE constraint)
                var existingView = await _context.PerformerViews
                    .FirstOrDefaultAsync(v => v.PerformerId == performerId && v.UserId == userId);

                if (existingView != null)
                {
                    // Update viewed timestamp
                    existingView.ViewedAt = DateTime.UtcNow;
                    _context.PerformerViews.Update(existingView);
                }
                else
                {
                    // Create new view record
                    var view = new PerformerView
                    {
                        Id = Guid.NewGuid().ToString(),
                        PerformerId = performerId,
                        UserId = userId,
                        ViewedAt = DateTime.UtcNow
                    };
                    await _context.PerformerViews.AddAsync(view);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in TrackPerformerViewAsync({performerId}, {userId}): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ToggleFavoriteAsync(string userId, string performerId)
        {
            try
            {
                var existing = await _context.UserFavorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.PerformerId == performerId);

                if (existing != null)
                {
                    // Remove favorite
                    _context.UserFavorites.Remove(existing);
                }
                else
                {
                    // Add favorite
                    var favorite = new UserFavorite
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        PerformerId = performerId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.UserFavorites.AddAsync(favorite);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in ToggleFavoriteAsync({userId}, {performerId}): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> HasUserReviewedAsync(string performerId, string userId)
        {
            try
            {
                return await _context.PerformerReviews
                    .AnyAsync(r => r.PerformerId == performerId && r.UserId == userId);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in HasUserReviewedAsync({performerId}, {userId}): {ex.Message}");
                return false;
            }
        }
    }
}
