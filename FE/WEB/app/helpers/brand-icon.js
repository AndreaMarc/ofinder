import { helper } from '@ember/component/helper';
import { htmlSafe } from '@ember/template';
import * as simpleIcons from 'simple-icons';

/**
 * Brand Icon Helper
 * Renders brand logos from Simple Icons
 *
 * Usage:
 * {{brand-icon "onlyfans"}} - Default size 16px, brand color
 * {{brand-icon "instagram" size=24}} - Custom size
 * {{brand-icon "twitter" size=20 color="#1DA1F2"}} - Custom color
 */
export default helper(function brandIcon(
  [iconName],
  { size = 16, color } = {}
) {
  // Mappa nomi personalizzati ai nomi Simple Icons
  const iconMapping = {
    onlyfans: 'siOnlyfans',
    instagram: 'siInstagram',
    twitter: 'siX', // X (ex Twitter)
    x: 'siX',
    tiktok: 'siTiktok',
    youtube: 'siYoutube',
    snapchat: 'siSnapchat',
    telegram: 'siTelegram',
    threads: 'siThreads',
  };

  const simpleIconName =
    iconMapping[iconName?.toLowerCase()] || `si${iconName}`;
  const icon = simpleIcons[simpleIconName];

  if (!icon) {
    // Fallback: usa FontAwesome icons per piattaforme non supportate
    const fallbackIcons = {
      fansly: { icon: 'fa-star', color: '#7b68ee' },
      telegram: { icon: 'fa-paper-plane', color: '#0088cc' },
      threads: { icon: 'fa-at', color: '#000000' },
      other: { icon: 'fa-globe', color: '#6c757d' },
      default: { icon: 'fa-globe', color: '#6c757d' },
    };

    const fallback =
      fallbackIcons[iconName?.toLowerCase()] || fallbackIcons.default;
    const fallbackColor = color || fallback.color;

    const fallbackSvg = `
      <i class="fas ${fallback.icon}" style="color: ${fallbackColor}; font-size: ${size}px; display: inline-block; vertical-align: middle;"></i>
    `;

    return htmlSafe(fallbackSvg.trim());
  }

  // Usa il colore brand dell'icona se non specificato
  const fillColor = color || `#${icon.hex}`;

  // Genera SVG
  const svg = `
    <svg
      role="img"
      viewBox="0 0 24 24"
      xmlns="http://www.w3.org/2000/svg"
      width="${size}"
      height="${size}"
      fill="${fillColor}"
      style="display: inline-block; vertical-align: middle;"
    >
      <title>${icon.title}</title>
      <path d="${icon.path}"/>
    </svg>
  `;

  return htmlSafe(svg.trim());
});
