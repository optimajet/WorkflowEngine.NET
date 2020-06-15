import { Directive, ElementRef, Injectable, AfterViewInit } from '@angular/core';

@Directive({
    selector: '[materialInput]'
})

@Injectable()
export class MaterialInput implements AfterViewInit {
    constructor(private _element: ElementRef) {
    }

    ngAfterViewInit(): void {
        $.AdminBSB.input.activate($(this._element.nativeElement).parent());
    }
}
