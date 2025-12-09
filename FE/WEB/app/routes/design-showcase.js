import Route from '@ember/routing/route';

export default class DesignShowcaseRoute extends Route {
  model() {
    // Mock data per testing componenti
    return {
      performers: this.getMockPerformers(),
      quickFilters: this.getQuickFilters(),
    };
  }

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
        bio: 'Fitness & Lifestyle creator. Daily workout videos and nutrition tips for a healthy lifestyle.',
        minPrice: 9.99,
        channels: [
          { platformType: 'onlyfans', verified: true },
          { platformType: 'fansly', verified: false },
          { platformType: 'instagram', verified: true },
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
        bio: 'Gaming enthusiast and streamer. Join me for epic gaming sessions and behind-the-scenes content.',
        minPrice: 12.99,
        channels: [
          { platformType: 'onlyfans', verified: true },
          { platformType: 'tiktok', verified: true },
          { platformType: 'twitter', verified: false },
        ],
      },
      {
        id: 3,
        displayName: 'Luna Steele',
        avatarUrl: 'https://i.pravatar.cc/300?img=9',
        verified: false,
        isPremium: false,
        averageRating: 4.2,
        reviewCount: 45,
        bio: 'Cosplay artist creating magical transformations. New costume every week!',
        minPrice: 7.99,
        channels: [
          { platformType: 'fansly', verified: false },
          { platformType: 'instagram', verified: true },
        ],
      },
      {
        id: 4,
        displayName: 'Valentina Sky',
        avatarUrl: 'https://i.pravatar.cc/300?img=16',
        verified: true,
        isPremium: true,
        averageRating: 4.9,
        reviewCount: 234,
        bio: 'International model and content creator. Exclusive photoshoots and lifestyle vlogs.',
        minPrice: 19.99,
        channels: [
          { platformType: 'onlyfans', verified: true },
          { platformType: 'fansly', verified: true },
          { platformType: 'instagram', verified: true },
          { platformType: 'twitter', verified: true },
        ],
      },
    ];
  }

  getQuickFilters() {
    return [
      { id: 'onlyfans', label: 'OnlyFans', icon: '🔵' },
      { id: 'fansly', label: 'Fansly', icon: '🟣' },
      { id: 'verified', label: 'Verificati', icon: '✓' },
      { id: 'new', label: 'Nuovi', icon: '🆕' },
      { id: 'fitness', label: 'Fitness', icon: '💪' },
      { id: 'gaming', label: 'Gaming', icon: '🎮' },
    ];
  }
}
