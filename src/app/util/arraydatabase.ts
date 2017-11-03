import { EventEmitter } from "@angular/core";
import { BehaviorSubject } from "rxjs/Rx";
import { DataSource } from "@angular/cdk/collections";
import { Observable } from "rxjs/Observable";
import { Subscription } from "rxjs/Subscription";


type ArrayDBCallback<T> = (item: Array<T>) => Array<T>;
type SortArrayDBCallback<T> = (a: T, b: T) => number;
type FilterArrayDBCallback<T> = (item: T) => boolean;
export class PropertySort {
    property: string;
    direction: boolean;
}
type PageDBCallback = () => Page;
export class Page {
    page: number = 0;
    pageSize: number = 0;
}

export class ArrayDatabase<T> {

    private _data: Array<T>;
    private dataChange: BehaviorSubject<Array<T>>;
    public get data(): Array<T> { return this.dataChange.value; }
    public get length(): number { return this.dataChange.value.length; }

    constructor(array: Array<T> = []) {
        this._data = array;
        this.dataChange = new BehaviorSubject<Array<T>>(array);
    }

    public setDB(array: Array<T>) {
        this._data = array;
        this.refresh();
    }

    public add(item: T): void {
        this._data.push(item);
        this.refresh();
    }

    public addItems(items: Array<T>): void {
        items.forEach(item => this._data.push(item));
        this.refresh();
    }

    public remove(index: number): void {
        this._data = this._data.slice(index, 1);
        this.refresh();
    }

    public removeItem(item: T): void {
        this.remove(this._data.indexOf(item));
    }

    public get(index: number): T {
        return this._data[index];
    }

    public find(check: (item: T) => boolean): T {
        return this._data.find(check);
    }

    public findIndex(check: (item: T) => boolean): number {
        return this._data.findIndex(check);
    }

    public getObservable(): Observable<Array<T>> {
        return this.dataChange;
    }

    public refresh(): void {
        this.dataChange.next(this._data);
    }
}

export class ArrayDataSource<T> extends DataSource<T> {

    public database: ArrayDatabase<T>;
    private dataChange: BehaviorSubject<Array<T>>;
    private subscription: Subscription;
    private observer: Observable<Array<T>>;

    constructor(database: ArrayDatabase<T>) {
        super();
        this.database = database;
        this.dataChange = new BehaviorSubject<Array<T>>([]);
        this.observer = this.dataChange.asObservable();
    }

    public connect(): Observable<Array<T>> {
        this.subscription = this.database.getObservable().subscribe(items => {
            this.dataChange.next(items);
        });
        return this.observer;
    }

    public disconnect(): void {
        this.subscription.unsubscribe();
    }

    public setMap(callback: ArrayDBCallback<T>): void {
        this.observer = this.observer.map(items => {
            return callback(items);
        });
    }

    public setSort(callback: SortArrayDBCallback<T>): void {
        this.setMap(items => {
            return items.sort(callback)
        });
    }

    public setPropertySort(property: PropertySort) {
        this.setSort((a, b) => {
            let propertyA = a[property.property];
            let propertyB = b[property.property];
            let valueA = isNaN(+propertyA) ? propertyA : +propertyA;
            let valueB = isNaN(+propertyB) ? propertyB : +propertyB;
            return (valueA < valueB ? -1 : 1) * (property.direction ? 1 : -1);
        });
    }

    public setFilter(callback: FilterArrayDBCallback<T>): void {
        this.setMap(items => {
            return items.filter(callback);
        })
    }

    public setPageFilter(pageChange: EventEmitter<any>, page: Page) {
        this.observer = this.observer.merge(pageChange).map(() => {
            let data = this.dataChange.value.slice();
            const startIndex = page.page * page.pageSize;
            return data.splice(startIndex, page.pageSize);
        });
    }

    public refresh(): void {
        this.dataChange.next(this.database.data);
    }
}
