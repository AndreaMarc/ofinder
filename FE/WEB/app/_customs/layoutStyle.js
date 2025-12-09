// Definisce lo stile del sito
export default {
  /**
   * GENERAL
   */
  fixedHeader: true, // if true, the header is always visible, otherwise it scrolls with the page
  fixedFooter: true, // if true, the footer is always visible, otherwise it scrolls with the page
  fixedSidebar: true, // if true, the sidebar is always visible, otherwise it scrolls with the page
  bodyTabsShadow: true,
  bodyTabsLine: true,
  appThemeWhite: true, // if false, class 'app-theme-gray' will be used
  headerShadow: true,
  sidebarShadow: true,

  /**
   * HEADER AREA
   * @param {string} headerLight
   * value 'white' for white text and icons (preferible on dark backgrounds);
   * value 'black' for black text and icons (preferible on light backgrounds);
   * value '' is preferable on white backgrounds
   */
  headerLight: 'black',

  /**
   * @param {string} headerBackground
   * Defines the background color of the header. Choose one of the colors listed below or
   * an empty string to assign no color. For example 'bg-primary'
   */
  headerBackground: 'bg-warning',

  /**
   * SIDEBAR AREA
   * @param {string} sidebarLight
   * value 'white' for white text and icons (preferible on dark backgrounds);
   * value 'black' for black text and icons (preferible on light backgrounds);
   * value '' is preferable on white backgrounds
   */
  sidebarLight: 'white',

  /**
   * @param {string} sidebarBackground
   * Defines the background color of the sidebar. Choose one of the colors listed below or
   * an empty string to assign no color. For example 'bg-primary'
   */
  sidebarBackground: 'bg-dark',
};

/**
 * AVAILABLE BACKGROUND COLORS
 *
 * SOLID:     bg-primary, bg-secondary, bg-success, bg-info, bg-warning, bg-danger, bg-focus, bg-alternate, bg-light, bg-dark
 * GRADIENT:  bg-happy-green, bg-premium-dark, bg-love-kiss, bg-grow-early, bg-strong-bliss, bg-warm-flame, bg-tempting-azure,
 *            bg-sunny-morning, bg-mean-fruit, bg-night-fade, bg-heavy-rain, bg-amy-crisp, bg-malibu-beach, bg-deep-blue,
 *            bg-mixed-hopes, bg-happy-itmeo, bg-happy-fisher, bg-arielle-smile, bg-ripe-malin, bg-vicious-stance,
 *            bg-midnight-bloom, bg-night-sky, bg-slick-carbon, bg-royal, bg-asteroid
 */
