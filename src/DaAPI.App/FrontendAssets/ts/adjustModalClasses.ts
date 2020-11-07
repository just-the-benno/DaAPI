/// <reference types="jquery" />

//thanks to https://stackoverflow.com/questions/2844565/is-there-a-javascript-jquery-dom-change-listener

MutationObserver = window.MutationObserver || (<any>window).WebKitMutationObserver;

var observer = new MutationObserver(  (mutations, observer) => {
    $('.blazored-modal-header').addClass('modal-header');
    $('.blazored-modal-title').addClass('modal-title');
});

observer.observe(document.body.children[0], {
    subtree: true,
    attributes: false,
    childList: true,
});