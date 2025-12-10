# OFinder - Database Schema Documentation

## Project Overview

**OFinder** is a platform for discovering and searching cam-girls, OnlyFans performers, and adult content creators across multiple platforms.

## Technology Stack

- **Backend**: MIT.FWK (.NET 8.0) - See `/BE/CLAUDE.md`
- **Frontend**: Ember.js 4.11.0 - See `/README.md`
- **Database**: SQL Server / MySQL (configurable via EF Core)

## Database Models

### 1. Performer

Main entity representing a content creator/performer.

**Table**: `performers`

| Field | Type | Description |
|-------|------|-------------|
| `id` | int (PK) | Primary key |
| `display_name` | nvarchar(100) | Display name |
| `bio` | text | Biography/description |
| `avatar_url` | nvarchar(500) | Profile picture URL |
| `verified` | bit | Verification status |
| `is_premium` | bit | Premium performer flag |
| `average_rating` | decimal(3,2) | Average rating (0-5) |
| `review_count` | int | Number of reviews |
| `min_price` | decimal(10,2) | Minimum subscription price |
| `created_at` | datetime | Creation timestamp |
| `updated_at` | datetime | Last update timestamp |

**Ember Model**: `/FE/WEB/app/models/performer.js`

**Relationships**:
- `hasMany('channel')` - Performer's social/platform channels

---

### 2. Channel

Represents a performer's presence on a specific platform (OnlyFans, Instagram, Telegram, etc.).

**Table**: `channels`

| Field | Type | Description |
|-------|------|-------------|
| `id` | int (PK) | Primary key |
| `performer_id` | int (FK) | Foreign key to performers |
| `platform` | nvarchar(50) | Platform name (OnlyFans, Fansly, Instagram, etc.) |
| `channel_type` | nvarchar(20) | **NEW** Channel type: `CamGirl`, `Performer`, `Escort` |
| `username_handle` | nvarchar(100) | Username/handle on platform |
| `profile_link` | nvarchar(500) | Direct link to profile |
| `note` | text | Additional notes |
| `created_at` | datetime | Creation timestamp |
| `updated_at` | datetime | Last update timestamp |

**Ember Model**: `/FE/WEB/app/models/channel.js`

**New Field - channel_type**:
- **Type**: `nvarchar(20)`
- **Enum Values**:
  - `CamGirl` - Live cam performer
  - `Performer` - Content creator (OnlyFans/Fansly)
  - `Escort` - Escort services
- **Purpose**: Allows users to filter performers by activity type
- **Filter Section**: "Tipo di Attività" (Activity Type) in search filters

**Relationships**:
- `belongsTo('performer')` - Parent performer
- `hasMany('channel-schedule')` - Live streaming schedules
- `hasMany('channel-content-type')` - Content types offered
- `belongsTo('channel-pricing')` - Pricing information

---

### 3. Channel Schedule

Represents live streaming schedules for a channel.

**Table**: `channel_schedules`

| Field | Type | Description |
|-------|------|-------------|
| `id` | int (PK) | Primary key |
| `channel_id` | int (FK) | Foreign key to channels |
| `day_of_week` | int | Day of week (0=Sunday, 6=Saturday) |
| `start_time` | time | Start time |
| `end_time` | time | End time |
| `time_zone` | nvarchar(50) | Timezone |
| `is_active` | bit | Active flag |
| `created_at` | datetime | Creation timestamp |
| `updated_at` | datetime | Last update timestamp |

**Ember Model**: `/FE/WEB/app/models/channel-schedule.js`

**Relationships**:
- `belongsTo('channel')` - Parent channel

---

### 4. Channel Content Type

Represents types of content published on a channel (photos, videos, live, etc.).

**Table**: `channel_content_types`

| Field | Type | Description |
|-------|------|-------------|
| `id` | int (PK) | Primary key |
| `channel_id` | int (FK) | Foreign key to channels |
| `content_type` | nvarchar(50) | Content type: `foto`, `video`, `live`, `abbigliamento`, `contenuti-extra` |
| `description` | text | Description |
| `is_available` | bit | Availability flag |
| `created_at` | datetime | Creation timestamp |
| `updated_at` | datetime | Last update timestamp |

**Ember Model**: `/FE/WEB/app/models/channel-content-type.js`

**Content Type Values**:
- `foto` - Photo content
- `video` - Video content
- `live` - Live streaming
- `abbigliamento` - Clothing/fashion content
- `contenuti-extra` - Extra content

**Relationships**:
- `belongsTo('channel')` - Parent channel

---

### 5. Channel Pricing

Represents pricing information for a channel.

**Table**: `channel_pricings`

| Field | Type | Description |
|-------|------|-------------|
| `id` | int (PK) | Primary key |
| `channel_id` | int (FK) | Foreign key to channels |
| `subscription_price` | decimal(10,2) | Monthly subscription price |
| `currency` | nvarchar(3) | Currency code (USD, EUR, etc.) |
| `has_free_tier` | bit | Free tier available |
| `pay_per_view` | bit | Pay-per-view content available |
| `tips_enabled` | bit | Tips enabled |
| `created_at` | datetime | Creation timestamp |
| `updated_at` | datetime | Last update timestamp |

**Ember Model**: `/FE/WEB/app/models/channel-pricing.js`

**Relationships**:
- `belongsTo('channel')` - Parent channel

---

## Search Filters

The search page (`/FE/WEB/app/routes/search.js`) provides the following filter sections:

### Filter Sections

1. **Posizione** (Location)
   - Country, Region, Province

2. **Piattaforme** (Platforms)
   - OnlyFans, Fansly, Instagram, Twitter/X, TikTok, YouTube, etc.

3. **Tipo di Attività** (Activity Type) - **NEW**
   - CamGirl
   - Performer (Content Creator)
   - Escort

4. **Tipi di Contenuto** (Content Types)
   - Foto (Photos)
   - Video (Videos)
   - Live (Live streaming)
   - Abbigliamento (Clothing/fashion)
   - Contenuti Extra (Extra content)

5. **Prezzo** (Price)
   - Price range slider ($0-$50+)

6. **Rating Minimo** (Minimum Rating)
   - 5 stars, 4+ stars, 3+ stars

7. **Stato** (Status)
   - Solo Verificati (Verified only)
   - Nuovi (New - last 30 days)
   - Attivi oggi (Active today)

### Quick Filters

- ✓ Verificati (Verified)
- ⭐ Premium
- ✨ Nuovi (New)
- 🏆 Più votati (Top Rated)

---

## Entity Relationships Diagram

```
performer (1) ──< (N) channel
                        │
                        ├──< (N) channel_schedule
                        ├──< (N) channel_content_type
                        └──< (1) channel_pricing
```

---

## Implementation Notes

### Frontend (Ember.js)

- **Models Location**: `/FE/WEB/app/models/`
- **Search Route**: `/FE/WEB/app/routes/search.js`
- **Search Controller**: `/FE/WEB/app/controllers/search.js`
- **Filter Component**: `/FE/WEB/app/components/filter-panel.js`
- **Performer Card**: `/FE/WEB/app/components/performer-card.hbs`

### Backend (.NET)

- **See**: `/BE/CLAUDE.md` for backend implementation details
- **Framework**: MIT.FWK (DDD + CQRS + Event Sourcing)
- **ORM**: Entity Framework Core 8.0
- **API**: JSON:API specification
- **Database Providers**: SQL Server, MySQL (configurable)

---

## Database Setup

### Connection String

Add to `dbconnections.json`:

```json
{
  "ConnectionStrings": {
    "JsonApiConnection": "Server=localhost;Database=OFinderDB;User Id=sa;Password=***;TrustServerCertificate=True"
  }
}
```

### Migrations

```bash
# Create migration
dotnet ef migrations add AddChannelType --context JsonApiDbContext

# Apply migration
dotnet ef database update --context JsonApiDbContext
```

### Auto-Migrations

Set in `customsettings.json`:

```json
{
  "EnableAutoMigrations": true,
  "DatabaseMigrationOrder": [
    "JsonApiDbContext"
  ]
}
```

---

## Development Status

### Completed
- ✅ Basic Ember models (performer, channel, channel-schedule, channel-content-type, channel-pricing)
- ✅ Search page with filters
- ✅ Performer card component
- ✅ Rating stars component
- ✅ Channel badges with platform icons
- ✅ Content type filters
- ✅ Channel type field added to model

### TODO
- ⏳ Connect frontend to backend API
- ⏳ Implement backend entities and DbContext
- ⏳ Add authentication/authorization
- ⏳ Implement performer detail page
- ⏳ Add reviews and rating system
- ⏳ Implement booking/scheduling system
- ⏳ Add payment integration
- ⏳ Implement activity type filters in UI

---

## Change Log

### 2025-12-10
- **Added** `channel_type` field to `channel` model
  - Type: `nvarchar(20)`
  - Enum: `CamGirl`, `Performer`, `Escort`
  - Purpose: Filter performers by activity type
  - Frontend model updated: `/FE/WEB/app/models/channel.js`

### Previous
- Initial database schema design
- Basic Ember models created
- Search filters implemented
- UI components created

---

## Contact & Documentation

- **Framework Documentation**: See `/README.md` (Frontend) and `/BE/CLAUDE.md` (Backend)
- **Project Repository**: [Internal]
- **Database Provider**: SQL Server / MySQL (configurable)
