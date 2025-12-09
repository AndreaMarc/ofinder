import Route from '@ember/routing/route';
import { inject as service } from '@ember/service';

/**
 * Search Route - Public performer search page
 * Accessible to both logged and unlogged users
 *
 * Features:
 * - Public search for all users
 * - Richer content for logged users
 * - Mock data for now (TODO: connect to backend API)
 */
export default class SearchRoute extends Route {
  @service session;

  model() {
    return {
      performers: this.getMockPerformers(),
      quickFilters: this.getQuickFilters(),
      platformFilters: this.getPlatformFilters(),
      categoryFilters: this.getCategoryFilters(),
      isLoggedIn: this.session.isAuthenticated,
    };
  }

  /**
   * Mock performers data
   * TODO: Replace with actual API call to backend
   */
  getMockPerformers() {
    return [
      {
        id: 1,
        displayName: 'Sofia Martinez',
        avatarUrl: 'https://i.pravatar.cc/300?img=1',
        verified: true,
        isPremium: true,
        averageRating: 4.8,
        reviewCount: 127,
        minPrice: 9.99,
        bio: 'Content creator specializzata in lifestyle e fashion. Contenuti esclusivi ogni giorno.',
        channels: [
          { platformType: 'onlyfans', verified: true, url: 'https://onlyfans.com/sofia' },
          { platformType: 'fansly', verified: false, url: 'https://fansly.com/sofia' },
          { platformType: 'instagram', verified: true, url: 'https://instagram.com/sofia' },
        ],
      },
      {
        id: 2,
        displayName: 'Emma Rose',
        avatarUrl: 'https://i.pravatar.cc/300?img=5',
        verified: true,
        isPremium: false,
        averageRating: 4.5,
        reviewCount: 89,
        minPrice: 4.99,
        bio: 'Modella e influencer. Contenuti glamour e lifestyle esclusivi.',
        channels: [
          { platformType: 'onlyfans', verified: true, url: 'https://onlyfans.com/emma' },
          { platformType: 'twitter', verified: false, url: 'https://twitter.com/emma' },
        ],
      },
      {
        id: 3,
        displayName: 'Luna Star',
        avatarUrl: 'https://i.pravatar.cc/300?img=9',
        verified: false,
        isPremium: true,
        averageRating: 4.9,
        reviewCount: 203,
        minPrice: 19.99,
        bio: 'Premium content creator. Contenuti esclusivi e personalizzati.',
        channels: [
          { platformType: 'onlyfans', verified: false, url: 'https://onlyfans.com/luna' },
          { platformType: 'fansly', verified: true, url: 'https://fansly.com/luna' },
          { platformType: 'tiktok', verified: false, url: 'https://tiktok.com/@luna' },
          { platformType: 'instagram', verified: true, url: 'https://instagram.com/luna' },
        ],
      },
      {
        id: 4,
        displayName: 'Mia Johnson',
        avatarUrl: 'https://i.pravatar.cc/300?img=20',
        verified: true,
        isPremium: false,
        averageRating: 4.2,
        reviewCount: 56,
        minPrice: 7.99,
        bio: 'Fitness model e lifestyle influencer. Contenuti motivazionali e lifestyle.',
        channels: [
          { platformType: 'onlyfans', verified: true, url: 'https://onlyfans.com/mia' },
          { platformType: 'instagram', verified: true, url: 'https://instagram.com/mia' },
          { platformType: 'youtube', verified: false, url: 'https://youtube.com/mia' },
        ],
      },
    ];
  }

  /**
   * Quick filters for search bar
   */
  getQuickFilters() {
    return [
      { id: 'verified', label: 'Verificati', icon: '✓', active: false },
      { id: 'premium', label: 'Premium', icon: '⭐', active: false },
      { id: 'new', label: 'Nuovi', icon: '🆕', active: false },
      { id: 'top-rated', label: 'Top Rated', icon: '🔥', active: false },
    ];
  }

  /**
   * Platform filters for filter panel
   */
  getPlatformFilters() {
    return [
      { id: 'onlyfans', name: 'OnlyFans', emoji: '🔵', count: 234 },
      { id: 'fansly', name: 'Fansly', emoji: '🟣', count: 156 },
      { id: 'instagram', name: 'Instagram', emoji: '📸', count: 189 },
      { id: 'twitter', name: 'Twitter/X', emoji: '🐦', count: 98 },
      { id: 'tiktok', name: 'TikTok', emoji: '🎵', count: 67 },
    ];
  }

  /**
   * Category filters for filter panel
   */
  getCategoryFilters() {
    return [
      { id: 'lifestyle', name: 'Lifestyle', count: 145 },
      { id: 'fitness', name: 'Fitness', count: 89 },
      { id: 'fashion', name: 'Fashion', count: 112 },
      { id: 'glamour', name: 'Glamour', count: 203 },
      { id: 'cosplay', name: 'Cosplay', count: 56 },
    ];
  }
}
