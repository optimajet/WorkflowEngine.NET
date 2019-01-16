export class MenuItem {
    name: string = '';
    permissionName: string = '';
    icon: string = '';
    route: string = '';
    items: MenuItem[];

    constructor(name: string, permissionName: string, icon: string, route: string, childItems: MenuItem[] = null) {
        this.name = name;
        this.permissionName = permissionName;
        this.icon = icon;
        this.route = route;

        if (childItems) {
            this.items = childItems;
        } else {
            this.items = [];
        }
    }
}
