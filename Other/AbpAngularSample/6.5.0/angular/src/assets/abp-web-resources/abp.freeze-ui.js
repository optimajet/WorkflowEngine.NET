"use strict";

var abp = abp || {};

(function () {
  if (!FreezeUI || !UnFreezeUI) {
    return;
  }

  abp.ui.setBusy = function (elm, text, delay) {
    FreezeUI({
      element: elm,
      text: text ? text : " ",
      delay: delay
    });
  };

  abp.ui.clearBusy = function (elm, delay) {
    UnFreezeUI({
      element: elm,
      delay: delay
    });
  };
})();