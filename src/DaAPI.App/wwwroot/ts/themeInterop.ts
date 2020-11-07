
declare interface Window {
    getSelectedValues: any;
}

window.getSelectedValues = function (sel: HTMLSelectElement) {
    const results = [];
    for (let i = 0; i < sel.options.length; i++) {
        if (sel.options[i].selected) {
            results[results.length] = sel.options[i].value;
        }
    }

    return results;
};
