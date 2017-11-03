import { Inject, LOCALE_ID } from '@angular/core';
import { DateAdapter, MatDateFormats } from '@angular/material';
import { Moment, isMoment, Locale } from 'moment';
import * as moment from 'moment';

export const MOMENT_DATE_FORMATS: MatDateFormats = {
    parse: {
        dateInput: 'DD/MM/YYYY'
    },
    display: {
        dateInput: 'DD/MM/YYYY',
        monthYearLabel: 'MMMM Y',
        dateA11yLabel: 'DD/MM/YYYY',
        monthYearA11yLabel: 'MMMM Y'
    }
};

const dateNames: string[] = [];
for (let date = 1; date <= 31; date++) {
    dateNames.push(String(date));
}

export class MomentDateAdapter extends DateAdapter<Moment> {

    private localeData: Locale;

    public constructor( @Inject(LOCALE_ID) locale: string) {
        super();
        this.setLocale(locale);
    }

    setLocale(locale: any): void {
        console.info('setLocale', locale);
        this.locale = locale;
        this.localeData = moment.localeData(locale);
    }

    getYear(date: Moment | Date): number {
        date = this.checkDate(date);
        return date.year();
    }

    getMonth(date: Moment | Date): number {
        date = this.checkDate(date);
        return date.month();
    }

    getDate(date: Moment | Date): number {
        date = this.checkDate(date);
        return date.date();
    }

    getDayOfWeek(date: Moment | Date): number {
        date = this.checkDate(date);
        return date.day();
    }

    getMonthNames(style: 'long' | 'short' | 'narrow'): string[] {
        switch (style) {
            case 'long':
                return this.localeData.months();
            case 'short':
                return this.localeData.monthsShort();
            case 'narrow':
                return this.localeData.monthsShort().map(month => month[0]);
        }
    }

    getDateNames(): string[] {
        return dateNames;
    }

    getDayOfWeekNames(style: 'long' | 'short' | 'narrow'): string[] {
        switch (style) {
            case 'long':
                return this.localeData.weekdays();
            case 'short':
                return this.localeData.weekdaysShort();
            case 'narrow':
                // Moment does not accept format even though @types/moment suggests it does
                return this.localeData.weekdaysShort();
        }
    }

    getYearName(date: Moment | Date): string {
        date = this.checkDate(date);
        return String(date.year());
    }

    getFirstDayOfWeek(): number {
        return this.localeData.firstDayOfWeek();
    }

    getNumDaysInMonth(date: Moment | Date): number {
        date = this.checkDate(date);
        return date.daysInMonth();
    }

    clone(date: Moment | Date): Moment {
        date = this.checkDate(date);
        return date.clone();
    }

    createDate(year: number, month: number, date: number): Moment {
        return moment([year, month, date], null, this.locale, true);
    }

    today(): Moment {
        return moment().locale(this.locale);
    }

    parse(value: any, parseFormat: any): Moment {
        let m = moment(value, parseFormat, this.locale, true)
        if (!m.isValid()) {
            if (value == "")
                m = moment().locale(this.locale);
            else
                m = moment(value).locale(this.locale);
        }
        if (m.isValid()) {
            if (m.year() === 2001 && value.indexOf('2001') === -1) {
                const currentYear = new Date().getFullYear();
                m.set('year', currentYear);
                if (m.isAfter(moment())) {
                    m.set('year', currentYear - 1);
                }
            }
            return m;
        }
        else {
            return null;
        }
    }

    format(date: Moment | Date, displayFormat: any): string {
        date = this.checkDate(date);

        if (date) {
            return date.format(displayFormat);
        }
        else {
            return '';
        }
    }

    addCalendarYears(date: Moment | Date, years: number): Moment {
        date = this.checkDate(date);
        return date.clone().add(years, 'y');
    }

    addCalendarMonths(date: Moment | Date, months: number): Moment {
        date = this.checkDate(date);
        return date.clone().add(months, 'M');
    }

    addCalendarDays(date: Moment | Date, days: number): Moment {
        date = this.checkDate(date);
        return date.clone().add(days, 'd');
    }

    compareDate(first: Moment | Date, second: Moment | Date): number {
        first = this.checkDate(first);
        second = this.checkDate(second);
        return first.diff(second, 'seconds', true);
    }

    sameDate(first: Moment | Date, second: Moment | Date): boolean {
        first = this.checkDate(first);
        second = this.checkDate(second);
        if (first == null) {
            // same if both null
            return second == null;
        }
        else if (isMoment(first)) {
            return first.isSame(second);
        }
        else {
            const isSame = super.sameDate(first, second);
            console.warn('first not a Moment. fallback to super.sameDate()', first, second, isSame);
            return isSame;
        }
    }

    clampDate(date: Moment | Date, min?: Moment | Date, max?: Moment | Date): Moment {
        date = this.checkDate(date);
        min = this.checkDate(min);
        max = this.checkDate(max);

        if (min && date.isBefore(min)) {
            return min;
        }
        else if (max && date.isAfter(max)) {
            return max;
        }
        else {
            return date;
        }
    }

    isDateInstance(obj: any): boolean {
        return isMoment(obj) || obj instanceof Date;
    }
    isValid(date: Moment | Date): boolean {
        date = this.checkDate(date);
        return date.isValid();
    }

    checkDate(date: Moment | Date | string) {
        if (typeof date === "string") date = new Date(date);
        if (date instanceof Date) date = moment(date, null, this.locale, false);
        return date;
    }

    toIso8601(date: Moment | Date): string {
        date = this.checkDate(date);
        return date.format();
    }
    fromIso8601(iso8601String: string): Moment {
        return this.parse(iso8601String, null);
    }

}
