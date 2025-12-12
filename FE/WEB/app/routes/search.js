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
      contentTypeFilters: this.getContentTypeFilters(),
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
        avatarUrl: '/assets/images/avatars/1.jpg',
        verified: true,
        isPremium: true,
        averageRating: 4.8,
        reviewCount: 127,
        minPrice: 9.99,
        bio: 'Content creator specializzata in lifestyle e fashion. Contenuti esclusivi ogni giorno.',
        channels: [
          {
            platformType: 'onlyfans',
            verified: true,
            url: 'https://onlyfans.com/sofia',
          },
          {
            platformType: 'fansly',
            verified: false,
            url: 'https://fansly.com/sofia',
          },
          {
            platformType: 'instagram',
            verified: true,
            url: 'https://instagram.com/sofia',
          },
          {
            platformType: 'telegram',
            verified: false,
            url: 'https://t.me/sofia',
          },
          {
            platformType: 'snapchat',
            verified: false,
            url: 'https://snapchat.com/add/sofia',
          },
        ],
        schedules: [
          { dayOfWeek: 1, timeSlots: ['14-18', '20-23'] }, // Lunedì: 7h
          { dayOfWeek: 2, timeSlots: ['15-19', '21-24'] }, // Martedì: 7h
          { dayOfWeek: 3, timeSlots: ['14-18'] }, // Mercoledì: 4h
          { dayOfWeek: 4, timeSlots: ['20-24'] }, // Giovedì: 4h
          { dayOfWeek: 5, timeSlots: ['18-22'] }, // Venerdì: 4h
          { dayOfWeek: 6, timeSlots: ['10-14', '20-24'] }, // Sabato: 8h
        ],
      },
      {
        id: 2,
        displayName: 'Emma Rose',
        avatarUrl: '/assets/images/avatars/5.jpg',
        verified: true,
        isPremium: false,
        averageRating: 4.5,
        reviewCount: 89,
        minPrice: 4.99,
        bio: 'Modella e influencer. Contenuti glamour e lifestyle esclusivi.',
        channels: [
          {
            platformType: 'onlyfans',
            verified: true,
            url: 'https://onlyfans.com/emma',
          },
          {
            platformType: 'twitter',
            verified: false,
            url: 'https://twitter.com/emma',
          },
          {
            platformType: 'threads',
            verified: true,
            url: 'https://threads.net/@emma',
          },
        ],
        schedules: [
          { dayOfWeek: 2, timeSlots: ['20-23'] }, // Martedì: 3h
          { dayOfWeek: 4, timeSlots: ['21-24'] }, // Giovedì: 3h
          { dayOfWeek: 6, timeSlots: ['14-18', '21-24'] }, // Sabato: 7h
        ],
      },
      {
        id: 3,
        displayName: 'Luna Star',
        avatarUrl: '/assets/images/avatars/9.jpg',
        verified: false,
        isPremium: true,
        averageRating: 4.9,
        reviewCount: 203,
        minPrice: 19.99,
        bio: 'Premium content creator. Contenuti esclusivi e personalizzati.',
        channels: [
          {
            platformType: 'onlyfans',
            verified: false,
            url: 'https://onlyfans.com/luna',
          },
          {
            platformType: 'fansly',
            verified: true,
            url: 'https://fansly.com/luna',
          },
          {
            platformType: 'tiktok',
            verified: false,
            url: 'https://tiktok.com/@luna',
          },
          {
            platformType: 'instagram',
            verified: true,
            url: 'https://instagram.com/luna',
          },
          {
            platformType: 'telegram',
            verified: true,
            url: 'https://t.me/luna',
          },
          {
            platformType: 'threads',
            verified: false,
            url: 'https://threads.net/@luna',
          },
        ],
        schedules: [
          { dayOfWeek: 0, timeSlots: ['18-24'] }, // Domenica: 6h
          { dayOfWeek: 1, timeSlots: ['19-24'] }, // Lunedì: 5h
          { dayOfWeek: 2, timeSlots: ['19-24'] }, // Martedì: 5h
          { dayOfWeek: 3, timeSlots: ['19-24'] }, // Mercoledì: 5h
          { dayOfWeek: 4, timeSlots: ['19-24'] }, // Giovedì: 5h
          { dayOfWeek: 5, timeSlots: ['18-24'] }, // Venerdì: 6h
          { dayOfWeek: 6, timeSlots: ['14-24'] }, // Sabato: 10h
        ],
      },
      {
        id: 4,
        displayName: 'Mia Johnson',
        avatarUrl: '/assets/images/avatars/8.jpg',
        verified: true,
        isPremium: false,
        averageRating: 4.2,
        reviewCount: 56,
        minPrice: 7.99,
        bio: 'Fitness model e lifestyle influencer. Contenuti motivazionali e lifestyle.',
        channels: [
          {
            platformType: 'onlyfans',
            verified: true,
            url: 'https://onlyfans.com/mia',
          },
          {
            platformType: 'instagram',
            verified: true,
            url: 'https://instagram.com/mia',
          },
          {
            platformType: 'youtube',
            verified: false,
            url: 'https://youtube.com/mia',
          },
          {
            platformType: 'snapchat',
            verified: true,
            url: 'https://snapchat.com/add/mia',
          },
        ],
        schedules: [
          { dayOfWeek: 1, timeSlots: ['6-8', '18-20'] }, // Lunedì: 4h (mattina + sera)
          { dayOfWeek: 3, timeSlots: ['6-8', '18-20'] }, // Mercoledì: 4h
          { dayOfWeek: 5, timeSlots: ['6-8', '18-22'] }, // Venerdì: 6h
          { dayOfWeek: 0, timeSlots: ['10-12'] }, // Domenica: 2h
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
      { id: 'new', label: 'Nuovi', icon: '✨', active: false },
      { id: 'topRated', label: 'Più votati', icon: '🏆', active: false },
    ];
  }

  /**
   * Platform filters for filter panel
   */
  getPlatformFilters() {
    return [
      { id: 'onlyfans', name: 'OnlyFans', emoji: '🔵', count: 234 },
      { id: 'fansly', name: 'Fansly', emoji: '🟣', count: 156 },
      { id: 'other', name: 'Altro', emoji: '🌐', count: 354 },
    ];
  }

  /**
   * Content Type filters for filter panel
   */
  getContentTypeFilters() {
    return [
      { id: 'foto', name: 'Foto', count: 234 },
      { id: 'video', name: 'Video', count: 189 },
      { id: 'live', name: 'Live', count: 156 },
      { id: 'abbigliamento', name: 'Abbigliamento', count: 45 },
      { id: 'contenuti-extra', name: 'Contenuti Extra', count: 78 },
    ];
  }
}
