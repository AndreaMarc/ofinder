import Component from '@glimmer/component';

/**
 * ChannelBadge Component
 * Badge colorato per indicare piattaforma social (OnlyFans, Fansly, ecc.)
 *
 * @param {Object} channel - Oggetto channel con platformType e verified
 * @param {String} size - Dimensione: 'sm' | 'md' (default: 'md')
 * @param {Boolean} clickable - Badge cliccabile (default: false)
 * @param {Function} onClick - Callback click (se clickable)
 */

const CHANNEL_CONFIG = {
  onlyfans: {
    name: 'OnlyFans',
    color: '#00AFF0',
    emoji: '🔵',
    icon: null,
  },
  fansly: {
    name: 'Fansly',
    color: '#7B68EE',
    emoji: '🟣',
    icon: null,
  },
  instagram: {
    name: 'Instagram',
    color: '#E4405F',
    emoji: '📸',
    icon: null,
  },
  twitter: {
    name: 'Twitter/X',
    color: '#1DA1F2',
    emoji: '🐦',
    icon: null,
  },
  tiktok: {
    name: 'TikTok',
    color: '#000000',
    emoji: '🎵',
    icon: null,
  },
  youtube: {
    name: 'YouTube',
    color: '#FF0000',
    emoji: '📺',
    icon: null,
  },
  snapchat: {
    name: 'Snapchat',
    color: '#FFFC00',
    emoji: '👻',
    icon: null,
  },
  other: {
    name: 'Other',
    color: '#6C757D',
    emoji: '🌐',
    icon: null,
  },
};

export default class ChannelBadgeComponent extends Component {
  get channelConfig() {
    const platformType = this.args.channel?.platformType || 'other';
    return CHANNEL_CONFIG[platformType.toLowerCase()] || CHANNEL_CONFIG.other;
  }

  get channelName() {
    return this.channelConfig.name;
  }

  get channelColor() {
    return this.channelConfig.color;
  }

  get channelEmoji() {
    return this.channelConfig.emoji;
  }

  get channelIcon() {
    return this.channelConfig.icon;
  }

  get fontSize() {
    const size = this.args.size || 'md';
    return size === 'sm' ? '11px' : '13px';
  }

  get textColor() {
    // TikTok e Snapchat hanno background chiaro, servono testo scuro
    const platformType = this.args.channel?.platformType?.toLowerCase();
    if (platformType === 'tiktok' || platformType === 'snapchat') {
      return '#212529';
    }
    return 'white';
  }
}
