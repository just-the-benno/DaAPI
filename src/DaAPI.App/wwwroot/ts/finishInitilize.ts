
declare interface Window {
    finishInitilize: any
}

window.finishInitilize = function () {
    window.location.pathname = "/";
}
