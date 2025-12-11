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
  telegram: {
    name: 'Telegram',
    color: '#0088CC',
    emoji: '✈️',
    icon: null,
  },
  threads: {
    name: 'Threads',
    color: '#000000',
    emoji: '🧵',
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
  get platformType() {
    return this.args.channel?.platformType?.toLowerCase() || 'other';
  }

  get channelConfig() {
    return CHANNEL_CONFIG[this.platformType] || CHANNEL_CONFIG.other;
  }

  get channelName() {
    return this.channelConfig.name;
  }

  get channelEmoji() {
    return this.channelConfig.emoji;
  }

  get channelIcon() {
    return this.channelConfig.icon;
  }
}
