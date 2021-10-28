"use strict";

// taken from https://raw.githubusercontent.com/alexradulescu/FreezeUI, modified and converted to ES5
(function () {
  var freezeHtml = document.createElement("div");
  freezeHtml.classList.add("freeze-ui");
  var freezedItems = [];

  var getSelector = function getSelector(selector) {
    return selector ? selector : "body";
  };

  var normalizeFreezeDelay = function normalizeFreezeDelay(delay) {
    return delay ? delay : 250;
  };

  var shouldFreezeItem = function shouldFreezeItem(selector) {
    var itemSelector = getSelector(selector);
    return freezedItems.indexOf(itemSelector) >= 0;
  };

  var addFreezedItem = function addFreezedItem(selector) {
    var itemSelector = getSelector(selector);
    freezedItems.push(itemSelector);
  };

  var removeFreezedItem = function removeFreezedItem(selector) {
    var itemSelector = getSelector(selector);

    for (var i = 0; i < freezedItems.length; i++) {
      if (freezedItems[i] === itemSelector) {
        freezedItems.splice(i, 1);
      }
    }
  };

  window.FreezeUI = function () {
    var options = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : {};
    addFreezedItem(options.selector);
    var delay = normalizeFreezeDelay(options.delay);
    setTimeout(function () {
      if (!shouldFreezeItem(options.selector)) {
        return;
      }

      var parent;

      if (options.element) {
        parent = options.element;
      } else {
        parent = document.querySelector(options.selector) || document.body;
      }

      freezeHtml.setAttribute("data-text", options.text || "Loading");

      if (document.querySelector(options.selector) || options.element) {
        freezeHtml.style.position = "absolute";
      }

      parent.appendChild(freezeHtml);
    }, delay);
  };

  window.UnFreezeUI = function () {
    var options = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : {};
    removeFreezedItem(options.selector);
    var delay = normalizeFreezeDelay(options.delay) + 250;
    setTimeout(function () {
      var freezeHtml;

      if (options.element) {
        freezeHtml = options.element.querySelector(".freeze-ui");
      } else {
        freezeHtml = document.querySelector(".freeze-ui");
      }

      if (freezeHtml) {
        freezeHtml.classList.remove("is-unfreezing");

        if (freezeHtml.parentElement) {
          freezeHtml.parentElement.removeChild(freezeHtml);
        }
      }
    }, delay);
  };
})();