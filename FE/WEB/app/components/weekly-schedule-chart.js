import Component from '@glimmer/component';
import { action } from '@ember/object';
import { guidFor } from '@ember/object/internals';
import ApexCharts from 'apexcharts';

/**
 * WeeklyScheduleChart Component
 * Displays a compact bar chart showing weekly availability hours
 *
 * @param {Array} schedules - Array of schedule objects with day and time slots
 * @param {Number} height - Chart height in pixels (default: 120)
 */
export default class WeeklyScheduleChartComponent extends Component {
  chartInstance = null;
  chartId = `weekly-schedule-chart-${guidFor(this)}`;

  get chartHeight() {
    return this.args.height || 120;
  }

  /**
   * Calculate total hours for each day of the week from schedules
   */
  get weeklyHours() {
    const schedules = this.args.schedules || [];
    const dayHours = [0, 0, 0, 0, 0, 0, 0]; // Dom, Lun, Mar, Mer, Gio, Ven, Sab

    schedules.forEach((schedule) => {
      const dayOfWeek = schedule.dayOfWeek; // 0 = Sunday, 1 = Monday, ...
      const hours = this.calculateHoursFromSlots(schedule.timeSlots);
      dayHours[dayOfWeek] += hours;
    });

    return dayHours;
  }

  /**
   * Calculate hours from time slots like "9-12", "14-18"
   */
  calculateHoursFromSlots(timeSlots) {
    if (!timeSlots || timeSlots.length === 0) return 0;

    let totalHours = 0;
    timeSlots.forEach((slot) => {
      const [start, end] = slot.split('-').map((t) => parseInt(t, 10));
      totalHours += end - start;
    });
    return totalHours;
  }

  /**
   * Get top 3 days by availability hours
   */
  get topDays() {
    const dayNames = ['Dom', 'Lun', 'Mar', 'Mer', 'Gio', 'Ven', 'Sab'];
    const hoursWithIndex = this.weeklyHours.map((hours, index) => ({
      name: dayNames[index],
      hours,
      index,
    }));

    return hoursWithIndex
      .sort((a, b) => b.hours - a.hours)
      .slice(0, 3)
      .filter((day) => day.hours > 0);
  }

  @action
  setupChart(element) {
    const dayNames = ['Dom', 'Lun', 'Mar', 'Mer', 'Gio', 'Ven', 'Sab'];

    const options = {
      series: [
        {
          name: 'Ore disponibili',
          data: this.weeklyHours,
        },
      ],
      chart: {
        type: 'bar',
        height: this.chartHeight,
        sparkline: {
          enabled: true, // Compact mode without axes
        },
        toolbar: {
          show: false,
        },
      },
      plotOptions: {
        bar: {
          borderRadius: 3,
          distributed: true, // Each bar different color
          dataLabels: {
            position: 'top',
          },
        },
      },
      dataLabels: {
        enabled: false,
      },
      colors: [
        '#DB2777', // Pink (OFinder primary)
        '#F59E0B', // Amber (OFinder accent)
        '#8B5CF6', // Purple
        '#EC4899', // Pink variant
        '#F97316', // Orange
        '#A855F7', // Purple variant
        '#E11D48', // Red-pink
      ],
      xaxis: {
        categories: dayNames,
        labels: {
          show: false,
        },
      },
      yaxis: {
        labels: {
          show: false,
        },
      },
      tooltip: {
        enabled: true,
        theme: 'dark',
        y: {
          formatter: (value) => `${value}h disponibili`,
        },
      },
      grid: {
        show: false,
      },
    };

    this.chartInstance = new ApexCharts(element, options);
    this.chartInstance.render();
  }

  @action
  teardownChart() {
    if (this.chartInstance) {
      this.chartInstance.destroy();
      this.chartInstance = null;
    }
  }
}
