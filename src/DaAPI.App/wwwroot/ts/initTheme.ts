/// <reference types="jquery" />

declare const adminlte: any;

function InitAdminLayout() {
    $(document.body).removeClass("layout-top-nav");
    $(document.body).addClass("sidebar-mini layout-fixed");

    const pushmenu: any = $('[data-widget="pushmenu"]');
    pushmenu.PushMenu('expand');
    pushmenu.PushMenu('collapse');
    pushmenu.PushMenu('expand');

    const lteAdminLayour: any = $('body');

    lteAdminLayour.Layout('fixLayoutHeight');

    $('[data-widget="treeview"]').each( () => {
        adminlte.Treeview._jQueryInterface.call($(this), 'init')
    })
}